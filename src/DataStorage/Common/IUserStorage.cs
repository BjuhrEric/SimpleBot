namespace SimpleDiscordBot.DataStorage.Common
{
    public interface IUserStorage : IDataContainer
    {
        public ulong ID { get; set; }
    }
}
