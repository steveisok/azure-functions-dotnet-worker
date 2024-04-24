using System;
using System.IO;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {

        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        host.Run();
    }
}
