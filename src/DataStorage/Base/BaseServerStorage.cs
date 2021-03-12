namespace SimpleDiscordBot.DataStorage.Base
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Runtime.Serialization;
    using SimpleDiscordBot.DataStorage.Common;
    using static SimpleDiscordBot.DataStorage.Util.Parsing;

    [Serializable]
    public sealed class BaseServerStorage : DataContainer, IServerStorage, IEquatable<BaseServerStorage>
    {
        private ulong idInternal;
        private string prefixInternal;

        public BaseServerStorage(ulong id) : this(id, "!")
        {
        }

        public BaseServerStorage(ulong id, string prefix)
        {
            this.ID = id;
            this.Prefix = prefix;
            this.AcceptedCommandChannels = new Dictionary<string, HashSet<ulong>>();
        }

        private BaseServerStorage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ID = (ulong)info.GetValue(nameof(this.ID), typeof(ulong));
            this.Prefix = info.GetValue(nameof(this.Prefix), typeof(string)) as string;
            this.AcceptedCommandChannels = GetExistingOrNewObject<Dictionary<string, HashSet<ulong>>>(info, nameof(AcceptedCommandChannels));
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

        public string Prefix
        {
            get
            {
                return this.prefixInternal;
            }

            set
            {
                this.prefixInternal = value;
                this.NotifyObserversChangeCompleted();
            }
        }
        private Dictionary<string, HashSet<ulong>> AcceptedCommandChannels { get; init; }

        public void AddAcceptedCommandChannel(string command, ulong id)
        {
            var channels = GetAcceptedCommandChannelsInternal(command);
            channels.Add(id);
            NotifyObserversChangeCompleted();
        }

        public void ClearAcceptedCommandChannels(string command)
        {
            var channels = GetAcceptedCommandChannelsInternal(command);
            channels.Clear();
            NotifyObserversChangeCompleted();
        }

        public bool Equals(BaseServerStorage other)
        {
            return base.Equals(other) &&
                   this.ID == other.ID &&
                   this.Prefix == other.Prefix &&
                   EqualityComparer<Dictionary<string, HashSet<ulong>>>.Default.Equals(this.AcceptedCommandChannels, other.AcceptedCommandChannels);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BaseServerStorage);
        }

        public IReadOnlySet<ulong> GetAcceptedCommandChannels(string command) => ImmutableHashSet<ulong>.Empty.Union(GetAcceptedCommandChannelsInternal(command));

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), this.ID, this.Prefix, this.AcceptedCommandChannels);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.ID), this.idInternal);
            info.AddValue(nameof(this.Prefix), this.prefixInternal);
            info.AddValue(nameof(this.AcceptedCommandChannels), AcceptedCommandChannels);
        }

        public void RemoveAcceptedCommandChannel(string command, ulong id)
        {
            var channels = GetAcceptedCommandChannelsInternal(command);
            channels.Remove(id);
            NotifyObserversChangeCompleted();
        }

        private HashSet<ulong> GetAcceptedCommandChannelsInternal(string command)
        {
            AcceptedCommandChannels.TryGetValue(command, out var channels);
            if (channels == null)
            {
                channels = new HashSet<ulong>();
                AcceptedCommandChannels.Add(command, channels);
            }

            return channels;
        }
    }
}
