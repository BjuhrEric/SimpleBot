namespace SimpleDiscordBot.Logging
{
    using System;
    using System.Runtime.Serialization;

    public interface ILogEntry : ISerializable
    {
        public ulong ActorID { get; set; }

        public ulong TargetID { get; set; }

        public ulong ServerID { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}