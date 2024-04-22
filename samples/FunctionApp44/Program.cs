using System;
using System.IO;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        string? path = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_LOGFILE_PATH");

        if (!string.IsNullOrEmpty(path))
        {
            File.AppendAllText(path, "Function44App LOADED!" + Environment.NewLine);    
        }
        else
        {
            File.AppendAllText("C:\\dev\\functions\\ArtifactsForProfileCollection\\func.txt", "LOGFILE PATH NOT FOUND!" + Environment.NewLine);
        }

        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        host.Run();
    }
}


