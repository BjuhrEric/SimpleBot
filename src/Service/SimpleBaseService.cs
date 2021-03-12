using Discord.WebSocket;
using SimpleDiscordBot.DataStorage.Service;

namespace SimpleDiscordBot.Service
{
    public class SimpleBaseService : DataStorageServiceUser
    {
        public DiscordSocketClient Client { protected get; init; }
    }
}
