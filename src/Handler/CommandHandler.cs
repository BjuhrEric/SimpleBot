namespace SimpleDiscordBot.Handler
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using SimpleDiscordBot.DataStorage.Common;

    public class CommandHandler<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
        : AbstractHandler<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>
        where TBotStorage : IBotStorage where TServerStorage : IServerStorage where TUserStorage : IUserStorage
        where TServerUserStorage : IServerUserStorage
    {
        public CommandService Service { protected get; init; }

        public IServiceProvider Provider { get; set; }

        private async Task InstallCommandsAsync()
        {
            this.Client.MessageReceived += this.HandleCommandAsync;
            await this.Service.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: this.Provider);
        }

        public override void RegisterHandler()
        {
            InstallCommandsAsync().GetAwaiter().GetResult();
        }

        private string GetPrefix(SocketUserMessage usermsg)
        {
            return usermsg.Channel is IGuildChannel guildChannel ? this.GetServerStorage(guildChannel.Guild.Id).Prefix : string.Empty;
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            if (msg is not SocketUserMessage usermsg) return;
            var prefix = this.GetPrefix(usermsg);

            var argpos = 0;

            if ((prefix.Length > 0 && !usermsg.HasStringPrefix(prefix, ref argpos)) || usermsg.Author.IsBot)
            {
                return;
            }

            ICommandContext context = new SocketCommandContext(this.Client, usermsg);
            await this.Service.ExecuteAsync(context: context, argPos: argpos, services: this.Provider);
        }
    }
}
