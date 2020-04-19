using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Gambot.Core;
using Gambot.Data.InMemory;
using Gambot.IO;
using Gambot.Module.BandName;
using Gambot.Module.Config;
using Gambot.Module.Factoid;
using Gambot.Module.Say;
using Gambot.Module.Variables;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Gambot.Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
            ConfigureLogging(configuration);

            var logger = new NLogLogger("Gambot");
            logger.Info("Starting the screaming robot.");

            // Temp implementation
            var dataStoreProvider = new InMemoryDataStoreProvider();
            var config = new DataStoreConfig(dataStoreProvider, logger.GetChildLog("DataStoreLogger"));

            var variableHandlers = new List<IVariableHandler>
                {
                    new BasicVariableHandler(dataStoreProvider),
                };

            var commands = new List<ICommand>
                {
                    new SetConfigCommand(config),
                    new GetConfigCommand(config),
                    new AddFactoidCommand(dataStoreProvider),
                    new ForgetFactoidCommand(dataStoreProvider),
                    new LiteralFactoidCommand(dataStoreProvider),
                    new AddVariableCommand(dataStoreProvider),
                    new RemoveVariableCommand(dataStoreProvider),
                    new DeleteVariableCommand(dataStoreProvider),
                    new ListVariableCommand(dataStoreProvider),
                };

            var listeners = new List<IListener>
                {
                    new FactoidListener(dataStoreProvider),
                };
            var responders = new List<IResponder>
                {
                    new SayResponder(),
                    new FactoidResponder(dataStoreProvider),
                    new AddBandNameResponder(dataStoreProvider, config),
                    new ExpandBandNameResponder(dataStoreProvider),
                };
            var transformers = new List<ITransformer>
                {
                    new VariableTransformer(variableHandlers),
                };

            var messenger = new ConsoleMessenger(logger.GetChildLog("ConsoleMessenger"));

            var processor = new BotProcess(commands, listeners, responders, transformers, messenger, logger.GetChildLog("BotProcess"));

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                logger.Info("Shutting down...");
                if (messenger != null)
                    messenger.Dispose();
                logger.Info("Done.");
                Environment.Exit(0);
            };

            processor.Initialize();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void ConfigureLogging(IConfiguration configuration)
        {
            const string defaultMessageLayout = "${longdate} [${pad:padding=5:inner=${level:uppercase=true}}] ${processid}:${threadid} ${logger} - ${message} ${exception:format=tostring}";
            var layout = configuration["Logging:Layout"] ?? defaultMessageLayout;
            var level = LogLevel.FromString(configuration["Logging:DefaultLevel"] ?? "Info");

            var logConfig = new LoggingConfiguration();

            if (!Environment.UserInteractive)
            {
                var fileTarget = new FileTarget
                {
                    FileName = configuration["Logging:LogFile"] ?? "gambot.log",
                    Layout = layout
                };
                logConfig.AddTarget("file", fileTarget);

                var fileRule = new LoggingRule("*", level, fileTarget);
                logConfig.LoggingRules.Add(fileRule);
            }
            else
            {
                var consoleTarget = new ColoredConsoleTarget
                {
                    Layout = layout
                };
                logConfig.AddTarget("console", consoleTarget);

                var consoleRule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
                logConfig.LoggingRules.Add(consoleRule);
            }

            LogManager.Configuration = logConfig;
        }
    }
}