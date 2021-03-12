using System.Collections.Generic;

namespace SimpleDiscordBot.DataStorage.Common
{
    public interface IServerStorage : IDataContainer
    {
        public ulong ID { get; set; }

        public string Prefix { get; set; }

        public void AddAcceptedCommandChannel(string command, ulong id);

        public void ClearAcceptedCommandChannels(string command);

        public IReadOnlySet<ulong> GetAcceptedCommandChannels(string command);

        public void RemoveAcceptedCommandChannel(string command, ulong id);
    }
}
