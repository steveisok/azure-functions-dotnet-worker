// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using FunctionsNetHost.Grpc;

namespace FunctionsNetHost
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("HELLLO HOST!");
                Logger.Log("Starting FunctionsNetHost0213");

                var workerStartupOptions = await GetStartupOptionsFromCmdLineArgs(args);

                var executableDir = Path.GetDirectoryName(args[0])!;

                if (string.IsNullOrEmpty(executableDir))
                {
                    executableDir = "C:\\dev\\functions\\ArtifactsForProfileCollection\\FunctionsHookPreJit";
                }

                Logger.Log("Executable DIR: " + executableDir);

                AssemblyPreloader.Preload(executableDir);

                var preJitFilePath = Path.Combine(executableDir, "PreJit", "coldstart.jittrace");
                var exist = File.Exists(preJitFilePath);
                Logger.Log($"{preJitFilePath} exist: {exist}");

                var dummyAppEntryPoint = Path.Combine(executableDir, "HookApp", "HookApp.dll");

                EnvironmentUtils.SetValue(EnvironmentVariables.PreJitFilePath, preJitFilePath);
                EnvironmentUtils.SetValue(EnvironmentVariables.DotnetStartupHooks, dummyAppEntryPoint);

                if (!File.Exists(dummyAppEntryPoint))
                {
                    Logger.Log($"Dummy app entry point not found: {dummyAppEntryPoint}");
                    throw new FileNotFoundException($"Dummy app entry point not found: {dummyAppEntryPoint}");
                }

                EnvironmentUtils.SetValue(EnvironmentVariables.AppEntryPoint, dummyAppEntryPoint);

                using var appLoader = new AppLoader();

                _ = Task.Run(() => appLoader.RunApplication(dummyAppEntryPoint));

                var grpcClient = new GrpcClient(workerStartupOptions, appLoader);

                await grpcClient.InitAsync();
            }
            catch (Exception exception)
            {
                Logger.Log($"An error occurred while running FunctionsNetHost \n\n {exception.ToString()}");
            }
        }

        private static async Task<GrpcWorkerStartupOptions> GetStartupOptionsFromCmdLineArgs(string[] args)
        {
            var uriOption = new Option<string>("--functions-uri") { IsRequired = true };
            var requestIdOption = new Option<string>("--functions-request-id") { IsRequired = true };
            var workerIdOption = new Option<string>("--functions-worker-id") { IsRequired = true };
            var grpcMaxMessageLengthOption = new Option<int>("--functions-grpc-max-message-length") { IsRequired = true };

            var rootCommand = new RootCommand
            {
                TreatUnmatchedTokensAsErrors = false
            };

            rootCommand.AddOption(uriOption);
            rootCommand.AddOption(requestIdOption);
            rootCommand.AddOption(workerIdOption);
            rootCommand.AddOption(grpcMaxMessageLengthOption);

            var workerStartupOptions = new GrpcWorkerStartupOptions();

            rootCommand.SetHandler((context) =>
            {
                var uriString = context.ParseResult.GetValueForOption(uriOption);
                if (!Uri.TryCreate(uriString, UriKind.Absolute, out var endpointUri))
                {
                    throw new UriFormatException($"'{uriString}' is not a valid value for argument '{uriOption.Name}'. Value should be a valid URL.");
                }

                workerStartupOptions.ServerUri = endpointUri;
                workerStartupOptions.GrpcMaxMessageLength = context.ParseResult.GetValueForOption(grpcMaxMessageLengthOption);
                workerStartupOptions.RequestId = context.ParseResult.GetValueForOption(requestIdOption);
                workerStartupOptions.WorkerId = context.ParseResult.GetValueForOption(workerIdOption);
            });

            Logger.LogTrace($"raw args:{string.Join(" ", args)}");

            var argsWithoutExecutableName = args.Skip(1).ToArray();
            await rootCommand.InvokeAsync(argsWithoutExecutableName);

            return workerStartupOptions;
        }
    }
}
