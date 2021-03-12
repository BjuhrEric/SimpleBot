namespace ExampleBot
{
    using Microsoft.Extensions.Logging;
    using SimpleDiscordBot;

    public class Program : SimpleBot
    {
        public static void Main()
        {
            // Creates a bot with log level set to Trace. Other options include settings for the interactive service or storage filenames.
            var bot = new Program
            {
                LogLevel = LogLevel.Trace
            };

            // Initializes the main components of the bot.
            bot.Init();

            /*
            This is where you would add additional services, e.g. a remote database service.
            To add a service use the bot.AddSingletonService() method.
            */

            // When all of the services are added we can finally start the bot!
            bot.Start();
        }
    }
}
