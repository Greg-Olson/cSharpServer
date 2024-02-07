using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
            await WriteFileAsync(ctx.Response, route);
        }
        else
        {
            await WriteFileAsync(ctx.Response, route);
        }
        ctx.Response.StatusCode = (int) HttpStatusCode.OK;
        
    }

    public async Task WriteFileAsync(HttpListenerResponse response, string filePath)
    {
            try
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                await SendFileAsync(response, AbsolutePath(filePath));
            }
            catch(HttpListenerException e)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"HttpException: {e.Message}");

            }
            catch(Exception e)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"{e.GetType()}:{e.Message}");
                await SendFileAsync(response, AbsolutePath(errorPath));
            }
    }

    private async Task SendFileAsync(HttpListenerResponse response, string filePath)
    {
        Stream input = new FileStream(filePath, FileMode.Open);

        try
        {
            response.ContentLength64 = input.Length;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = GetMimeType(filePath);

            byte[] buffer = new byte[65536];
            int noBytes;
            while((noBytes = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                await response.OutputStream.WriteAsync(buffer, 0 , noBytes);
                response.OutputStream.Flush();
            }
            input.Close();
        }
        catch
        {
            input.Close();
            throw;
        }
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

         private static readonly IDictionary<string, string> mimeTypes = new Dictionary<string, string>
        {
            { ".asf", "video/x-ms-asf" },
            { ".asx", "video/x-ms-asf" },
            { ".avi", "video/x-msvideo" },
            { ".bin", "application/octet-stream" },
            { ".cco", "application/x-cocoa" },
            { ".crt", "application/x-x509-ca-cert" },
            { ".css", "text/css" },
            { ".deb", "application/octet-stream" },
            { ".der", "application/x-x509-ca-cert" },
            { ".dll", "application/octet-stream" },
            { ".dmg", "application/octet-stream" },
            { ".ear", "application/java-archive" },
            { ".eot", "application/octet-stream" },
            { ".exe", "application/octet-stream" },
            { ".flv", "video/x-flv" },
            { ".gif", "image/gif" },
            { ".hqx", "application/mac-binhex40" },
            { ".htc", "text/x-component" },
            { ".htm", "text/html" },
            { ".html", "text/html" },
            { ".ico", "image/x-icon" },
            { ".img", "application/octet-stream" },
            { ".iso", "application/octet-stream" },
            { ".jar", "application/java-archive" },
            { ".jardiff", "application/x-java-archive-diff" },
            { ".jng", "image/x-jng" },
            { ".jnlp", "application/x-java-jnlp-file" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".js", "application/x-javascript" },
            { ".mml", "text/mathml" },
            { ".mng", "video/x-mng" },
            { ".mov", "video/quicktime" },
            { ".mp3", "audio/mpeg" },
            { ".mpeg", "video/mpeg" },
            { ".mpg", "video/mpeg" },
            { ".msi", "application/octet-stream" },
            { ".msm", "application/octet-stream" },
            { ".msp", "application/octet-stream" },
            { ".pdb", "application/x-pilot" },
            { ".pdf", "application/pdf" },
            { ".pem", "application/x-x509-ca-cert" },
            { ".pl", "application/x-perl" },
            { ".pm", "application/x-perl" },
            { ".png", "image/png" },
            { ".prc", "application/x-pilot" },
            { ".ra", "audio/x-realaudio" },
            { ".rar", "application/x-rar-compressed" },
            { ".rpm", "application/x-redhat-package-manager" },
            { ".rss", "text/xml" },
            { ".run", "application/x-makeself" },
            { ".sea", "application/x-sea" },
            { ".shtml", "text/html" },
            { ".sit", "application/x-stuffit" },
            { ".swf", "application/x-shockwave-flash" },
            { ".tcl", "application/x-tcl" },
            { ".tk", "application/x-tcl" },
            { ".txt", "text/plain" },
            { ".war", "application/java-archive" },
            { ".wasm", "application/wasm" },
            { ".wbmp", "image/vnd.wap.wbmp" },
            { ".wmv", "video/x-ms-wmv" },
            { ".xml", "text/xml" },
            { ".xpi", "application/x-xpinstall" },
            { ".zip", "application/zip" }
        };

         public static string GetMimeType(string route)
        {
            // find extension
            string ext = Path.GetExtension(route);

            if (mimeTypes.ContainsKey(ext))
            {
                return mimeTypes[ext];
            }
            return "application/octet-stream";
        }

}