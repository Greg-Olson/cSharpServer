using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        StaticFileServer server = new StaticFileServer("http://localhost:8080/", "view");
        int res = await server.RunAsync(args);

        Console.WriteLine($"Ended with code {res}");
        Console.ReadKey();
    }
}
