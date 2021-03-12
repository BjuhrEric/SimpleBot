using SimpleDiscordBot.Handler;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;
using SimpleDiscordBot.DataStorage.Service;
using SimpleDiscordBot.DataStorage.Base;
using SimpleDiscordBot.DataStorage.Common;
using Discord.Addons.Interactive;

#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
namespace SimpleDiscordBot
{
    public class SimpleBot : SimpleBot<BaseBotStorage, BaseServerStorage, BaseUserStorage, BaseServerUserStorage>
    {
    }

    public class SimpleBot<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage> where TBotStorage : IBotStorage
        where TServerStorage : IServerStorage where TUserStorage : IUserStorage where TServerUserStorage : IServerUserStorage
    {
        private IServiceCollection Collection = new ServiceCollection();
        private bool HasInitiated;
        private bool IsRunning;

        private LogLevel LogLevelInternal;

        public string BotStorageFilename { get; set; } = "BotData.json";

        public DiscordSocketClient Client { get; private set; }

        public bool CreateServerStoragesOnStartup { get; set; } = false;

        public bool CreateUserStoragesOnStartup { get; set; } = false;

        public DataStorageService<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage> DataStorage { get; private set; }

        public CommandHandler<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage> Handler { get; private set; }

        public InteractiveService InteractiveService { get; set; }

        public InteractiveServiceConfig InteractiveServiceConfig { get; set; } = new InteractiveServiceConfig
        {
            DefaultTimeout = TimeSpan.FromHours(1)
        };

        public LogLevel LogLevel
        {
            get => LogLevelInternal;
            set { if (!IsRunning) LogLevelInternal = value; }
        }
        public IServiceProvider Provider { get; private set; }

        public string ServerStorageFilename { get; set; } = "ServerData.json";

        public CommandService Service { get; private set; }

        public string UserStorageFilename { get; set; } = "UserData.json";

        public void AddSingletonService<T>(T Service) where T : class => Collection = Collection.AddSingleton(Service);
        public void AddSingletonService(Type ServiceType) => Collection = Collection.AddSingleton(ServiceType);
        public IServiceProvider BuildServiceProvider() =>
            Collection
            .AddSingleton(Client)
            .AddSingleton(Service)
            .AddSingleton(Handler)
            .AddSingleton(DataStorage)
            .AddSingleton(InteractiveService)
            .BuildServiceProvider();

        public void Init()
        {
            SetupLogger();
            SetupDataStorage();
            SetupClient();
            SetupInteractiveService();
            SetupCommandService();

            Handler = new CommandHandler<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
            {
                Client = Client,
                DataStorage = DataStorage,
                Service = Service
            };
            HasInitiated = true;
        }

        public void Start()
        {
            if (HasInitiated)
            {
                Provider = BuildServiceProvider();
                Handler.Provider = Provider;
                InstallCommands();
                LoginClientAsync().GetAwaiter().GetResult();
                StartClientAsync().GetAwaiter().GetResult();
                AllocateStorages();
                RunPermanentlyAsync().GetAwaiter().GetResult();
            }
        }

        private static DiscordSocketConfig CreateClientConfig()
        {
            return new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
            };
        }

        private static CommandServiceConfig CreateCommandConfig()
        {
            return new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Verbose
            };
        }

        private static Task LogMsgAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical: Log.Fatal(msg.Message); break;
                case LogSeverity.Error: Log.Error(msg.Message); break;
                case LogSeverity.Warning: Log.Warning(msg.Message); break;
                case LogSeverity.Info: Log.Information(msg.Message); break;
                case LogSeverity.Verbose: Log.Verbose(msg.Message); break;
                default: Log.Debug(msg.Message); break;
            }
            return Task.CompletedTask;
        }

        private static async Task RunPermanentlyAsync()
        {
            Log.Debug("Keeping program running indefinitely");
            await Task.Delay(-1);
        }

        private void AllocateServerStorage(SocketGuild Guild)
        {
            if (CreateServerStoragesOnStartup)
            {
                DataStorage.GetServerStorage(Guild.Id);
            }
        }

        private void AllocateStorages()
        {
            WaitUntilGuildsAreLoaded();
            if (ShouldAllocateStoragesOnStartup())
            {
                foreach (var Guild in Client.Guilds)
                {
                    AllocateServerStorage(Guild);
                    AllocateUserStorages(Guild);
                }
            }
        }

        private void AllocateUserStorages(SocketGuild Guild)
        {
            if (CreateUserStoragesOnStartup)
            {
                foreach (var User in Guild.Users)
                {
                    DataStorage.GetUserStorage(User.Id);
                }
            }
        }

        private void InstallCommands()
        {
            Log.Debug("Installing commands");
            Handler.RegisterHandler();
        }

        private async Task LoginClientAsync()
        {
            Log.Debug("Logging in");
            var BotStorage = DataStorage.BotStorage;
            try
            {
                await Client.LoginAsync(TokenType.Bot, BotStorage.Token, true);
            }
            catch (Exception)
            {
                Log.Warning("The supplied token is either missing or invalid. Please reenter.");
                PromptForNewToken(BotStorage);
                await LoginClientAsync();
            }
        }

        private static void PromptForNewToken(TBotStorage BotStorage)
        {
            Console.Out.WriteLine("Please provide a token for the bot.");
            BotStorage.Token = Console.In.ReadLine();
        }

        private void SetupClient()
        {
            Client = new DiscordSocketClient(CreateClientConfig());
            Client.Log += LogMsgAsync;
        }

        private void SetupCommandService()
        {
            Log.Debug("Creating command service");
            Service = new CommandService(CreateCommandConfig());
        }

        private void SetupDataStorage()
        {
            DataStorage = new DataStorageService<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
            {
                BotFileName = BotStorageFilename,
                ServerFileName = ServerStorageFilename,
                UserFileName = UserStorageFilename
            };
            DataStorage.LoadFromFile();
        }

        private void SetupInteractiveService()
        {
            InteractiveService = new InteractiveService(Client, InteractiveServiceConfig);
        }

        private void SetupLogger()
        {
            var tmpConfiguration = LogLevel switch
            {
                LogLevel.None => new LoggerConfiguration(),
                LogLevel.Critical => new LoggerConfiguration().MinimumLevel.Fatal(),
                LogLevel.Error => new LoggerConfiguration().MinimumLevel.Error(),
                LogLevel.Warning => new LoggerConfiguration().MinimumLevel.Warning(),
                LogLevel.Information => new LoggerConfiguration().MinimumLevel.Information(),
                LogLevel.Trace => new LoggerConfiguration().MinimumLevel.Verbose(),
                _ => new LoggerConfiguration().MinimumLevel.Debug(),
            };

            Log.Logger = tmpConfiguration.WriteTo.Console().CreateLogger();
        }

        private bool ShouldAllocateStoragesOnStartup()
        {
            return CreateServerStoragesOnStartup || CreateUserStoragesOnStartup;
        }

        private async Task StartClientAsync()
        {
            Log.Debug("Starting session");
            await Client.StartAsync();
            IsRunning = true;
        }

        private void WaitUntilGuildsAreLoaded()
        {
            while (ShouldAllocateStoragesOnStartup() && Client.Guilds.Count < 1)
            {
                Task.Delay(250).GetAwaiter().GetResult();
            }
        }
    }
}

#pragma warning restore RCS1090