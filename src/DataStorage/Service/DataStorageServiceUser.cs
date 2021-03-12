using Discord;
using SimpleDiscordBot.DataStorage.Base;
using SimpleDiscordBot.DataStorage.Common;

namespace SimpleDiscordBot.DataStorage.Service
{
    public class DataStorageServiceUser : DataStorageServiceUser<BaseBotStorage, BaseServerStorage, BaseUserStorage, BaseServerUserStorage>
    {
    }

    public class DataStorageServiceUser<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
        where TBotStorage : IBotStorage where TServerStorage : IServerStorage where TUserStorage : IUserStorage
        where TServerUserStorage : IServerUserStorage
    {
        public DataStorageService<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage> DataStorage { protected get; init; }

        protected TBotStorage GetGlobalStorage() => this.DataStorage.BotStorage;

        protected TServerStorage GetServerStorage(IGuild guild) => this.GetServerStorage(guild.Id);

        protected TServerStorage GetServerStorage(ulong id) => this.DataStorage.GetServerStorage(id);

        protected TServerUserStorage GetServerUserStorage(IGuildUser guildUser) => this.GetServerUserStorage(guildUser.GuildId, guildUser.Id);

        protected TServerUserStorage GetServerUserStorage(ulong guildID, ulong id) => this.DataStorage.GetServerUserStorage(guildID, id);

        protected TUserStorage GetUserStorage(IUser user) => this.GetUserStorage(user.Id);

        protected TUserStorage GetUserStorage(ulong id) => this.DataStorage.GetUserStorage(id);
    }
}
