using System;

namespace SimpleDiscordBot.Logging
{
    public interface ILogFactory<TLogEntry>
        where TLogEntry : class, ILogEntry
    {
        public ulong ActorID { get; set; }

        public ulong TargetID { get; set; }

        public ulong ServerID { get; set; }

        public TLogEntry CreateLogEntry();
    }
}
