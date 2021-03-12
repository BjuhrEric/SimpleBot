namespace SimpleDiscordBot.Logging
{
    using System;

    public abstract class AbstractLogFactory<TLogEntry> : ILogFactory<TLogEntry>
        where TLogEntry : class, ILogEntry
    {
        public ulong ActorID { get; set; }

        public ulong TargetID { get; set; }

        public ulong ServerID { get; set; }

        public static DateTime TimeStamp => DateTime.Now;

        public abstract TLogEntry CreateLogEntry();
    }
}
