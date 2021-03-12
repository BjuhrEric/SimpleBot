namespace SimpleDiscordBot.DataStorage.Base
{
    using System;
    using System.Runtime.Serialization;
    using SimpleDiscordBot.DataStorage.Common;

    [Serializable]
    public class BaseUserStorage : DataContainer, IUserStorage, IEquatable<BaseUserStorage>
    {
        private ulong idInternal;

        public BaseUserStorage(ulong id)
        {
            this.ID = id;
        }

        protected BaseUserStorage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ID = (ulong)info.GetValue(nameof(this.ID), typeof(ulong));
        }

        public ulong ID
        {
            get
            {
                return this.idInternal;
            }

            set
            {
                this.idInternal = value;
                this.NotifyObserversChangeCompleted();
            }
        }

        public virtual bool Equals(BaseUserStorage other)
        {
            return base.Equals(other) && this.ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BaseUserStorage);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), this.ID);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.ID), this.ID);
        }
    }
}
