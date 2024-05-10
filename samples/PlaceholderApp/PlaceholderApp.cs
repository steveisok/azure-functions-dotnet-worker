using System;
using System.IO;
using System.Reflection;

using static System.Environment;

public class PlaceholderApp
{
    const string WorkerLogFileEnvVar = "AZURE_FUNCTIONS_WORKER_LOGFILE_PATH";

    static int Main(string[] args)
    {
        // Console.WriteLine("Hello from Placeholder App! If you got here, then you"
        //                   + " made a wrong turn along the way. Retrace your steps"
        //                   + " and try again. Good luck!");

        Assembly entry = Assembly.GetEntryAssembly();
        //LogMessage($"Going to execute the entry assembly {entry.FullName}...");

        AppDomain thisDomain = AppDomain.CurrentDomain;
        int exitCode = thisDomain.ExecuteAssemblyByName(entry.GetName());
        return exitCode;
    }

    /*
    private static void LogMessage(string message)
    {
        string logFile = GetEnvironmentVariable(WorkerLogFileEnvVar);

        if (string.IsNullOrWhiteSpace(logFile))
            Console.WriteLine($"PLACEHOLDER CONSOLE: {message}");
        else
            File.AppendAllTextAsync(logFile, $"{message}{NewLine}");
    }
    */
}
