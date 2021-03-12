namespace SimpleDiscordBot.DataStorage.Base
{
    using System;
    using System.Runtime.Serialization;
    using SimpleDiscordBot.DataStorage.Common;
    [Serializable]
    public sealed class BaseBotStorage : DataContainer, IBotStorage, IEquatable<BaseBotStorage>
    {
        private string tokenInternal = string.Empty;

        public BaseBotStorage() : this(string.Empty)
        {
        }

        public BaseBotStorage(string token)
        {
            this.Token = token;
        }

        private BaseBotStorage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Token = info.GetValue(nameof(this.Token), typeof(string)) as string;
        }

        public string Token
        {
            get => this.tokenInternal;
            set
            {
                this.tokenInternal = value;
                this.NotifyObserversChangeCompleted();
            }
        }

        public bool Equals(BaseBotStorage other)
        {
            return other != null && base.Equals(other) && this.Token == other.Token;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BaseBotStorage);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), this.Token);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.Token), this.Token);
        }
    }
}
