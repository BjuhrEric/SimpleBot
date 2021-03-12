namespace SimpleDiscordBot.DataStorage.Common
{
    public interface IServerUserStorage : IUserStorage
    {
        public ulong ServerID { get; set; }
    }
}
