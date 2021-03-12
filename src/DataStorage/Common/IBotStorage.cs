namespace SimpleDiscordBot.DataStorage.Common
{
    public interface IBotStorage : IDataContainer
    {
        public string Token { get; set; }
    }
}
