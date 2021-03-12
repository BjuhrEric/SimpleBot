namespace SimpleDiscordBot.DataStorage.Base
{
    using System;
    using System.Runtime.Serialization;
    using SimpleDiscordBot.DataStorage.Common;

    [Serializable]
    public sealed class BaseServerUserStorage : BaseUserStorage, IServerUserStorage, IEquatable<BaseServerUserStorage>
    {
        private ulong serverIDInternal;

        public BaseServerUserStorage(ulong serverID, ulong id) : base(id)
        {
            this.ServerID = serverID;
        }

        private BaseServerUserStorage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ServerID = (ulong)info.GetValue(nameof(this.ServerID), typeof(ulong));
        }

        public ulong ServerID
        {
            get
            {
                return serverIDInternal;
            }
            set
            {
                serverIDInternal = value;
                NotifyObserversChangeCompleted();
            }
        }

        public bool Equals(BaseServerUserStorage other)
        {
            return base.Equals(other) &&
                   ServerID == other.ServerID;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BaseServerUserStorage);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), this.ServerID);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.ServerID), this.ServerID);
        }
    }
}
