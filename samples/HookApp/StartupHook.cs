using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Azure.Functions.Worker;

internal class StartupHook
{
    // FNH will signal this handle when it receives env reload req. 
    static readonly EventWaitHandle WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "AzureFunctionsNetHostSpecializationWaitHandle");

    const string SpecializedEntryAssemblyEnvVar = "AZURE_FUNCTIONS_SPECIALIZED_ENTRY_ASSEMBLY";

    public static void Initialize()
    {
        LogToStandardOutput("Startup Hook Called!");

        var filePath = Environment.GetEnvironmentVariable("FUNCTIONS_PREJIT_FILE_PATH");

        if (string.IsNullOrEmpty(filePath))
        {
            LogToStandardOutput($"Exiting because FUNCTIONS_PREJIT_FILE_PATH env variable is empty.");
            return;
        }

        LogToStandardOutput($"FUNCTIONS_PREJIT_FILE_PATH env variable is not empty. Prejitting...");

        PreJitPrepare(filePath);

        LogToStandardOutput($"Waiting for specialization signal.");

        WaitHandle.WaitOne();

        LogToStandardOutput($"Specialization signal received.");

        string? entryAssemblyName = Environment.GetEnvironmentVariable(SpecializedEntryAssemblyEnvVar);

        if (string.IsNullOrEmpty(entryAssemblyName))
        {
            LogToStandardOutput($"Exiting because AZURE_FUNCTIONS_SPECIALIZED_ENTRY_ASSEMBLY env variable is empty.");
            return;
        }

        LogToStandardOutput($"Setting new entry assembly to {entryAssemblyName}.");

        try
        {
            Assembly specializedEntryAssembly = Assembly.LoadFrom(entryAssemblyName);
            Assembly.SetEntryAssembly(specializedEntryAssembly);
        }
        catch(Exception e)
        {
            LogToStandardOutput($"Can't set new entry assembly. {e.ToString()}");
        }
    }

    static string logPrefix = "LanguageWorkerConsoleLog";
    private static void LogToStandardOutput(string message)
    {
        string? path = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_LOGFILE_PATH");

        if (!string.IsNullOrEmpty(path))
        {
            File.AppendAllText(path, message + Environment.NewLine);
        }
        Console.WriteLine($"{logPrefix} {message}");

    }

    private static void PreJitPrepare(string filePath)
    {
        var file = new FileInfo(filePath);
        var fileExist = file.Exists;

        if (!file.Exists)
        {
            LogToStandardOutput($"StartupHook.PreJitPrepare - JIT file path: {filePath}. fileExist:{fileExist}");
            return;
        }

        JitTraceRuntime.Prepare(file, out int successfulPrepares, out int failedPrepares);
        LogToStandardOutput($"StartupHook.PreJitPrepare > successfulPrepares: {successfulPrepares}, failedPrepares:{failedPrepares}");
    }
}
