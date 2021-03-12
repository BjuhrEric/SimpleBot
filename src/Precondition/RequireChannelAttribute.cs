namespace SimpleDiscordBot.Precondition
{
    using System;
    using System.Threading.Tasks;
    using Discord.Commands;
    using SimpleDiscordBot.DataStorage.Base;
    using SimpleDiscordBot.DataStorage.Service;

    public sealed class RequireChannelAttribute  : RequireContextAttribute
    {
        /// <summary>
        /// Checks that either there is no required channel or that the command was sent in one of the required channels.
        /// </summary>
        /// <remarks>
        /// This attribute should only be used with the base storage solution.
        /// </remarks
        public RequireChannelAttribute() : base(ContextType.Guild)
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissionsAsync(context, command, services);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            var type = typeof(DataStorageService<BaseBotStorage, BaseServerStorage, BaseUserStorage, BaseServerUserStorage>);
            var storageService = (DataStorageService<BaseBotStorage, BaseServerStorage, BaseUserStorage, BaseServerUserStorage>) services.GetService(type);
            var serverStorage = storageService.GetServerStorage(context.Guild.Id);
            var channels = serverStorage.GetAcceptedCommandChannels(command.Name.ToLower());

            return channels.Count == 0 || channels.Contains(context.Channel.Id) ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Command was not sent in an accepted channel!");
        }
    }
}