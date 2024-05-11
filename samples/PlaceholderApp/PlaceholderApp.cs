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

        //Assembly entry = Assembly.GetEntryAssembly();
        //LogMessage($"Going to execute the entry assembly {entry.FullName}...");

        string specEntryAsmName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_SPECIALIZED_ENTRY_ASSEMBLY")!;

        LogMessage($"Trying to load in placeholder main: {specEntryAsmName}");

        if (string.IsNullOrWhiteSpace(specEntryAsmName))
        {
            return -1;
        }


        Assembly entry = Assembly.LoadFrom(specEntryAsmName);

        try
        {
            LogMessage($"Before ExecuteAssembly");
            AppDomain thisDomain = AppDomain.CurrentDomain;
            int exitCode = thisDomain.ExecuteAssemblyByName(entry.GetName());
            LogMessage("After ExecuteAssembly");
            return exitCode;
        }
        catch(Exception ex)
        {
            LogMessage($"Error executing entry assembly: {ex.ToString()}");
            return -1;
        }
    }

    private static void LogMessage(string message)
    {
        string logFile = GetEnvironmentVariable(WorkerLogFileEnvVar);

        if (string.IsNullOrWhiteSpace(logFile))
            Console.WriteLine($"PLACEHOLDER CONSOLE: {message}");
        else
            File.AppendAllTextAsync(logFile, $"{message}{NewLine}");
    }
}
