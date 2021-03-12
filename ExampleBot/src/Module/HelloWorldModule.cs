namespace ExampleBot.Module
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using SimpleDiscordBot.Module;

    public class HelloWorldModule : SimpleBotBaseTopLevelModule
    {
        // To call on this method type !Hello in a text chat that the bot can see.
        [Command("Hello")]
        public async Task HelloWorldAsync() => await this.ReplyAsync("Hello World").ConfigureAwait(true);
    }
}
