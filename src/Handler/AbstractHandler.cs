namespace SimpleDiscordBot.Handler
{
    using Discord;
    using Discord.WebSocket;
    using SimpleDiscordBot.DataStorage.Base;
    using SimpleDiscordBot.DataStorage.Common;
    using SimpleDiscordBot.DataStorage.Service;

    public abstract class AbstractHandler : AbstractHandler<BaseBotStorage, BaseServerStorage, BaseUserStorage, BaseServerUserStorage>
    {
    }

    public abstract class AbstractHandler<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
        : DataStorageServiceUser<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
        where TBotStorage : IBotStorage where TServerStorage : IServerStorage where TUserStorage : IUserStorage 
        where TServerUserStorage : IServerUserStorage
    {
        public DiscordSocketClient Client { protected get; init; }

        public abstract void RegisterHandler();
    }
}
