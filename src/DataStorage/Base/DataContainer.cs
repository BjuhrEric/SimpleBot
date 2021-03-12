namespace SimpleDiscordBot.DataStorage.Base
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Runtime.Serialization;
    using SimpleDiscordBot.DataStorage.Common;
    using SimpleDiscordBot.Logging;
    using static SimpleDiscordBot.DataStorage.Util.Parsing;

    [Serializable]
    public class DataContainer : IDataContainer, IEquatable<DataContainer>
    {
        protected readonly Dictionary<string, string> CommandLog;
        protected readonly Dictionary<string, ulong> NamedIDs;
        protected readonly Dictionary<string, string> NamedObjects;
        protected readonly Dictionary<string, string> NamedStrings;
        [NonSerialized]
        protected readonly LinkedList<IObserver<IDataContainer>> Observers;

        public DataContainer()
        {
            this.NamedIDs = new Dictionary<string, ulong>();
            this.NamedStrings = new Dictionary<string, string>();
            this.NamedObjects = new Dictionary<string, string>();
            this.CommandLog = new Dictionary<string, string>();
            this.Observers = new LinkedList<IObserver<IDataContainer>>();
        }

        protected DataContainer(SerializationInfo info, StreamingContext context)
        {
            this.NamedIDs = GetExistingOrNewObject<Dictionary<string, ulong>>(info, nameof(NamedIDs));
            this.NamedStrings = GetExistingOrNewObject<Dictionary<string, string>>(info, nameof(NamedStrings));
            this.NamedObjects = GetExistingOrNewObject<Dictionary<string, string>>(info, nameof(NamedObjects));
            this.CommandLog = GetExistingOrNewObject<Dictionary<string, string>>(info, nameof(CommandLog));
            this.Observers = new LinkedList<IObserver<IDataContainer>>();
        }

        public void ClearNamedID(string name) => this.ClearNamedID(name, true);

        public void ClearNamedID(string name, bool notifyObservers)
        {
            this.NamedIDs.Remove(name);
            if (notifyObservers)
            {
                this.NotifyObserversChangeCompleted();
            }
        }

        public void ClearNamedObject(string name) => this.ClearNamedObject(name, true);

        public void ClearNamedObject(string name, bool notifyObservers)
        {
            this.NamedObjects.Remove(name);
            if (notifyObservers)
            {
                this.NotifyObserversChangeCompleted();
            }
        }

        public void ClearNamedString(string name) => this.ClearNamedString(name, true);

        public void ClearNamedString(string name, bool notifyObservers)
        {
            this.NamedStrings.Remove(name);
            if (notifyObservers)
            {
                this.NotifyObserversChangeCompleted();
            }
        }

        public virtual bool Equals(DataContainer other)
        {
            return other != null && this.GetHashCode() == other.GetHashCode() &&
                   EqualityComparer<Dictionary<string, ulong>>.Default.Equals(this.NamedIDs, other.NamedIDs) &&
                   EqualityComparer<Dictionary<string, string>>.Default.Equals(this.NamedObjects, other.NamedObjects) &&
                   EqualityComparer<Dictionary<string, string>>.Default.Equals(this.NamedStrings, other.NamedStrings) &&
                   EqualityComparer<Dictionary<string, string>>.Default.Equals(this.CommandLog, other.CommandLog);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as DataContainer);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.NamedIDs, this.NamedObjects, this.NamedStrings);
        }

        public IReadOnlyList<T> GetLogEntries<T>(string className = default) where T : class, ILogEntry
        {
            var deserializedValue = DeserializeOrCreateFromDictionary<LinkedList<T>>(CommandLog, className ?? typeof(T).Name);
            return ImmutableList<T>.Empty.AddRange(deserializedValue);
        }

        public ulong GetNamedID(string name)
        {
            this.NamedIDs.TryGetValue(name, out var value);
            return value;
        }

        public T GetNamedObject<T>(string name) where T : class, ISerializable
            => DeserializeOrCreateFromDictionary<T>(NamedObjects, name);

        public string GetNamedString(string name)
        {
            this.NamedStrings.TryGetValue(name, out var value);
            return value;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.NamedIDs), this.NamedIDs);
            info.AddValue(nameof(this.NamedStrings), this.NamedStrings);
            info.AddValue(nameof(this.NamedObjects), this.NamedObjects);
            info.AddValue(nameof(this.CommandLog), this.CommandLog);
        }

        public void LogCommand<T>(T logEntry) where T : class, ILogEntry
        {
            var entries = DeserializeOrCreateFromDictionary<LinkedList<T>>(CommandLog, logEntry.GetType().Name, typeof(LinkedList<>).MakeGenericType(logEntry.GetType()));
            entries.AddFirst(logEntry);
            this.CommandLog.Remove(logEntry.GetType().Name);
            this.CommandLog.Add(logEntry.GetType().Name, Serialize(entries));
            this.NotifyObserversChangeCompleted();
        }

        public void SetNamedID(string name, ulong id)
        {
            this.ClearNamedID(name, false);
            this.NamedIDs.Add(name, id);
            this.NotifyObserversChangeCompleted();
        }

        public void SetNamedObject<T>(string name, T obj) where T : class, ISerializable
        {
            this.ClearNamedObject(name, false);
            var serializedContent = Serialize(obj);
            this.NamedObjects.Add(name, serializedContent);
            this.NotifyObserversChangeCompleted();
        }

        public void SetNamedString(string name, string strVal)
        {
            this.ClearNamedString(name, false);
            this.NamedStrings.Add(name, strVal);
            this.NotifyObserversChangeCompleted();
        }

        public virtual IDisposable Subscribe(IObserver<IDataContainer> observer)
        {
            this.Observers.AddLast(observer);
            return new Unsubscriber(this, observer);
        }

        protected void NotifyObserversChangeCompleted()
        {
            foreach (var observer in this.Observers)
            {
                observer.OnCompleted();
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            => this.GetObjectData(info, context);

        private sealed class Unsubscriber : IDisposable
        {
            private readonly DataContainer observable;
            private readonly IObserver<IDataContainer> observer;

            public Unsubscriber(DataContainer observable, IObserver<IDataContainer> observer)
            {
                this.observable = observable;
                this.observer = observer;
            }

            public void Dispose()
            {
                this.observable.Observers.Remove(this.observer);
            }
        }
    }
}
