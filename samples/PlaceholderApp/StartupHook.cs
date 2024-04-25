using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Azure.Functions.Worker;

using SysEnv = System.Environment;

internal class StartupHook
{
    // FNH will signal this handle when it receives env reload req.
    static readonly EventWaitHandle s_waitHandle = new EventWaitHandle(
        initialState: false,
        mode: EventResetMode.ManualReset,
        name: "AzureFunctionsNetHostSpecializationWaitHandle"
    );

    const string PrejitFileEnvVar        = "FUNCTIONS_PREJIT_FILE_PATH";
    const string SpecEntryAssemblyEnvVar = "AZURE_FUNCTIONS_SPECIALIZED_ENTRY_ASSEMBLY";
    const string WorkerLogFileEnvVar     = "AZURE_FUNCTIONS_WORKER_LOGFILE_PATH";

    public static void Initialize()
    {
        LogMessage($"{SysEnv.NewLine}Startup Hook Called!{SysEnv.NewLine}");

        string jitTraceFile = SysEnv.GetEnvironmentVariable(PrejitFileEnvVar);

        if (!string.IsNullOrWhiteSpace(jitTraceFile))
        {
            LogMessage($"{PrejitFileEnvVar} env was set. Will attempt to carry out"
                       + $" the prejitting process using '{jitTraceFile}'.");
            PreJitPrepare(jitTraceFile);
        }
        else
        {
            LogMessage($"{PrejitFileEnvVar} env was not set. Will proceed with the"
                       + " non-prejitting version of the scenario.");
        }

        LogMessage("{SysEnv.NewLine}Now waiting for Specialization Mode signal...");
        s_waitHandle.WaitOne();
        LogMessage("Specialization Mode signal received!");

        string specEntryAsmName = SysEnv.GetEnvironmentVariable(SpecEntryAssemblyEnvVar);

        if (string.IsNullOrWhiteSpace(specEntryAsmName))
        {
            LogMessage("Received a signal but the new specialized entry assembly"
                       + $" was empty. Exiting...{SysEnv.NewLine}");
            return ;
        }

        LogMessage($"Setting entry assembly to '{specEntryAsmName}'.");

        Assembly specializedEntryAsm = Assembly.LoadFrom(specEntryAsmName);
        Assembly.SetEntryAssembly(specializedEntryAsm);

        Assembly verifyTest = Assembly.GetEntryAssembly();
        LogMessage($"The new entry assembly is '{verifyTest.FullName}'.");
    }

    private static void LogMessage(string message)
    {
        string logFile = SysEnv.GetEnvironmentVariable(WorkerLogFileEnvVar);

        if (string.IsNullOrWhiteSpace(logFile))
            Console.WriteLine($"STARTUP CONSOLE: {message}");
        else
            File.AppendAllTextAsync(logFile, $"{message}{SysEnv.NewLine}");
    }

    private static void PreJitPrepare(string jitTraceFile)
    {
        LogMessage($"StartupHook.PreJitPrepare -> JitTraceFile: {jitTraceFile}");

        if (!File.Exists(jitTraceFile))
        {
            LogMessage($"StartupHook.PreJitPrepare -> Provided jittracefile was"
                       + " not found. Will continue with the non-prejit version"
                       + " of the scenario.");
            return ;
        }

        JitTraceRuntime.Prepare(new FileInfo(jitTraceFile),
                                out int successes,
                                out int failures);

        LogMessage($"StartupHook.PreJitPrepare -> Successful Prepares: {successes}");
        LogMessage($"StartupHook.PreJitPrepare -> Failed Prepares: {failures}");
    }
}
