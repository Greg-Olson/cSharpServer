using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

class StaticFileServer
{
    private HttpListener listener;
    private string hostUrl;
    private string hostDir;

    private string AbsolutePath(string route){
        return (route.StartsWith("/") || hostDir.EndsWith("/"))
        ? $"{hostDir}{route}"
        : $"{hostDir}/{route}";

    }

    private string notFoundPath = "notFound.html";
    private string errorPath = "error.html";

    private bool running;

    public StaticFileServer(string hostUrl, string hostDir){
        this.hostDir = hostDir;
        this.hostUrl = hostUrl;
    }

    private async Task ProcessRequest(HttpListenerContext ctx)
    {
        string route = ctx.Request.Url.AbsolutePath;
        if(route == "/shutdown")
        {
            running = false;
            ctx.Response.StatusCode = (int) HttpStatusCode.NoContent;
            return;
        }
        string absolutePath = AbsolutePath(route);
        if(File.Exists( absolutePath))
        {
            try
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                SendFile(ctx.Response, absolutePath);
            }
            catch(HttpListenerException e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"HttpException: {e.Message}");

            }
            catch(Exception e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"{e.GetType()}:{e.Message}");
                SendFile(ctx.Response, errorPath);
            }
        }
        ctx.Response.StatusCode = (int) HttpStatusCode.OK;
        
    }

    private void SendFile(HttpListenerResponse response, string filePath)
    {

    }

    public async Task<int> RunAsync(string[] args)
    {
        listener = new HttpListener();
        listener.Prefixes.Add(hostUrl);
        listener.Start();
        Console.WriteLine($"Listening on {hostUrl}");
        running = true;

        while(running){
            HttpListenerContext ctx = await listener.GetContextAsync();

            Console.WriteLine($"New request: {ctx.Request.HttpMethod}{ctx.Request.Url}");
            await ProcessRequest(ctx);
            Console.WriteLine($"Responding: {ctx.Response.StatusCode}, {ctx.Response.ContentType}, {ctx.Response.ContentLength64}");
            ctx.Response.Close();
        }

        listener.Close();

        return 0;
    }

}