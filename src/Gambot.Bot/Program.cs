using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Gambot.Core;
using Gambot.Data;
using Gambot.Data.InMemory;
using Gambot.Data.SQLite;
using Gambot.IO;
using Gambot.IO.Discord;
using Gambot.Module.BandName;
using Gambot.Module.Config;
using Gambot.Module.Factoid;
using Gambot.Module.Say;
using Gambot.Module.Variables;
using NLog;
using NLog.Config;
using NLog.Targets;
using SimpleInjector;

namespace Gambot.Bot
{
    class Program
    {
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
            ConfigureLogging(configuration);

            var logger = new NLogLogger("Gambot.Bot");
            logger.Info("Starting the screaming robot.");

            logger.Trace("Configuring dependency injection container.");
            var container = new Container();

            // Configure logging proxy
            logger.Trace("Registering log proxy");
            container.RegisterConditional(typeof(Gambot.Core.ILogger), ctx => typeof(NLogLogger<>).MakeGenericType(ctx.Consumer.ImplementationType), Lifestyle.Singleton, ctx => true);

            logger.Trace("Registering the data store provider");
            var dataStore = configuration["Gambot:DataStoreProvider"] ?? "InMemory";

            if (String.Compare("inmemory", dataStore, true) == 0)
                container.Register<IDataStoreProvider, InMemoryDataStoreProvider>(Lifestyle.Singleton);
            if (String.Compare("sqlite", dataStore, true) == 0)
                container.Register<IDataStoreProvider>(() => new SQLiteDataStoreProvider(configuration["ConnectionStrings:Gambot"]), Lifestyle.Singleton);

            logger.Trace("Registering config provider");
            container.Register<IConfig, DataStoreConfig>(Lifestyle.Singleton);

            // Get all the included module assemblies
            logger.Debug("Loading module assemblies...");
            var assemblies = GetAssemblies();
            logger.Trace("Module assemblies loaded.");

            logger.Trace("Registering module components");
            logger.Trace("Registering commands...");
            container.Collection.Register<ICommand>(assemblies);

            logger.Trace("Registering variable handlers...");
            container.Collection.Register<IVariableHandler>(assemblies);

            logger.Trace("Registering listeners...");
            container.Collection.Register<IListener>(assemblies);

            logger.Trace("Registering responders...");
            container.Collection.Register<IResponder>(assemblies);

            logger.Trace("Registering transformers...");
            container.Collection.Register<ITransformer>(assemblies);

            logger.Info("Module components loaded.");

            logger.Trace("Registering IO");
            var io = configuration["Gambot:IO"] ?? "console";
            if (String.Compare(io, "discord", true) == 0)
            {
                container.RegisterSingleton(() => new DiscordConfiguration
                {
                    Token = configuration["Discord:Token"],
                        LogSeverity = 5 - LogManager.GlobalThreshold.Ordinal
                });
                container.Register<IMessenger, DiscordMessenger>(Lifestyle.Singleton);
                container.Register<IPersonProvider, DiscordMessenger>(Lifestyle.Singleton);
            }
            else
            {
                container.Register<IMessenger, ConsoleMessenger>(Lifestyle.Singleton);
                container.Register<IPersonProvider, ConsoleMessenger>(Lifestyle.Singleton);
            }

            logger.Trace("Registering Bot processor");
            container.Register<BotProcess>(Lifestyle.Singleton);

            logger.Debug("Validating container configuration...");
            container.Verify();
            logger.Info("Bot successfully configured.");

            logger.Info("Starting bot process.");
            var processor = container.GetInstance<BotProcess>();

            Console.CancelKeyPress += async(sender, eventArgs) =>
            {
                logger.Info("Shutting down...");
                await processor.Stop();
                logger.Info("Done.");
                Environment.Exit(0);
            };

            await processor.Initialize();

            await Task.Delay(-1);
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            // Still a temp implementation...
            return new []
            {
                typeof(IDataStore).Assembly,
                typeof(ConsoleMessenger).Assembly,
                typeof(ConfigModule).Assembly,
                typeof(VariableModule).Assembly,
                typeof(SayModule).Assembly,
                typeof(FactoidModule).Assembly,
                typeof(BandNameModule).Assembly,
            };
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

                var consoleRule = new LoggingRule("*", level, consoleTarget);
                logConfig.LoggingRules.Add(consoleRule);
            }

            LogManager.GlobalThreshold = level;
            LogManager.Configuration = logConfig;
        }
    }
}