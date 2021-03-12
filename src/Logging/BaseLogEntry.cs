namespace SimpleDiscordBot.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    public class BaseLogEntry : ILogEntry, IEquatable<BaseLogEntry>
    {
        protected BaseLogEntry(ulong actorID = default, ulong targetID = default, ulong serverID = default, DateTime timestamp = default)
        {
            this.TargetID = targetID;
            this.ActorID = actorID;
            this.ServerID = serverID;
            this.TimeStamp = timestamp;
        }

        protected BaseLogEntry(SerializationInfo info, StreamingContext context)
        {
            this.TargetID = info.GetUInt64(nameof(TargetID));
            this.ActorID = info.GetUInt64(nameof(ActorID));
            this.ServerID = info.GetUInt64(nameof(ServerID));
            this.TimeStamp = info.GetDateTime(nameof(TimeStamp));
        }

        public ulong ActorID { get; set; }

        public ulong TargetID { get; set; }

        public ulong ServerID { get; set; }

        public DateTime TimeStamp { get; set; }

        public static bool operator !=(BaseLogEntry left, BaseLogEntry right)
        {
            return !(left == right);
        }

        public static bool operator ==(BaseLogEntry left, BaseLogEntry right)
        {
            return EqualityComparer<BaseLogEntry>.Default.Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            return obj is BaseLogEntry entry && this.Equals(entry);
        }

        public virtual bool Equals(BaseLogEntry other)
        {
            return this.TargetID == other.TargetID &&
                   this.ActorID == other.ActorID &&
                   this.ServerID == other.ServerID &&
                   this.TimeStamp == other.TimeStamp;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.TargetID, this.ActorID, this.TimeStamp);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.TargetID), this.TargetID);
            info.AddValue(nameof(this.ActorID), this.ActorID);
            info.AddValue(nameof(this.ServerID), this.ServerID);
            info.AddValue(nameof(this.TimeStamp), this.TimeStamp);
        }
    }
}
