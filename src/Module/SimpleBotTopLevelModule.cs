namespace SimpleDiscordBot.Module
{
    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;
    using SimpleDiscordBot.DataStorage.Base;
    using SimpleDiscordBot.DataStorage.Common;
    using SimpleDiscordBot.DataStorage.Service;

    public class SimpleBotBaseTopLevelModule : SimpleBotBaseTopLevelModule<SocketCommandContext>
    {
    }

    public class SimpleBotBaseTopLevelModule<TContext> : SimpleBotTopLevelModule<BaseBotStorage, BaseServerStorage, BaseUserStorage, BaseServerUserStorage, TContext>
        where TContext : SocketCommandContext
    {
    }

    public class SimpleBotTopLevelModule<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
        : SimpleBotTopLevelModule<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage, SocketCommandContext>
        where TBotStorage : IBotStorage where TServerStorage : IServerStorage where TUserStorage : IUserStorage where TServerUserStorage : IServerUserStorage
    {
    }

    public class SimpleBotTopLevelModule<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage, TContext> : InteractiveBase<TContext>
        where TBotStorage : IBotStorage where TServerStorage : IServerStorage where TUserStorage : IUserStorage where TServerUserStorage : IServerUserStorage
        where TContext : SocketCommandContext
    {
        public DataStorageService<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage> DataStorage { get; set; }

        protected TBotStorage GetGlobalStorage() => this.DataStorage.BotStorage;

        protected TServerStorage GetServerStorage(IGuild guild) => this.GetServerStorage(guild.Id);

        protected TServerStorage GetServerStorage(ulong id) => this.DataStorage.GetServerStorage(id);

        protected TServerUserStorage GetServerUserStorage(IGuildUser guildUser) => this.GetServerUserStorage(guildUser.GuildId, guildUser.Id);

        protected TServerUserStorage GetServerUserStorage(ulong guildID, ulong id) => this.DataStorage.GetServerUserStorage(guildID, id);

        protected TUserStorage GetUserStorage(IUser user) => this.GetUserStorage(user.Id);

        protected TUserStorage GetUserStorage(ulong id) => this.DataStorage.GetUserStorage(id);
    }
}
