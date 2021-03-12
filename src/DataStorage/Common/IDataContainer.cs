namespace SimpleDiscordBot.DataStorage.Common
{
    using SimpleDiscordBot.Logging;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public interface IDataContainer : ISerializable, IObservable<IDataContainer>
    {
        public void ClearNamedID(string name);

        public void ClearNamedString(string name);

        public void ClearNamedObject(string name);

        public IReadOnlyList<T> GetLogEntries<T>(string className = default) where T : class, ILogEntry;

        public ulong GetNamedID(string name);

        public string GetNamedString(string name);

        public T GetNamedObject<T>(string name) where T : class, ISerializable;

        public void LogCommand<T>(T logEntry) where T : class, ILogEntry;

        public void SetNamedID(string name, ulong id);

        public void SetNamedString(string name, string strVal);

        public void SetNamedObject<T>(string name, T obj) where T : class, ISerializable;
    }
}
