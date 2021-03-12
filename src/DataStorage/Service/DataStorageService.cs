namespace SimpleDiscordBot.DataStorage.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Serilog;
    using SimpleDiscordBot.DataStorage.Common;

    public class StorageFileLocker
    {
        protected static readonly object BotFileLock = new object();
        protected static readonly object ServerFileLock = new object();
        protected static readonly object ServerUserFileLock = new object();
        protected static readonly object UserFileLock = new object();

        protected StorageFileLocker()
        {
        }
    }

    public class DataStorageService<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage> : StorageFileLocker, IObserver<IDataContainer>
        where TBotStorage : IBotStorage where TServerStorage : IServerStorage where TUserStorage : IUserStorage where TServerUserStorage : IServerUserStorage
    {
        private readonly JsonSerializer serializer;
        private TBotStorage botStorageInternal;
        private Dictionary<ulong, TServerStorage> storedServers;
        private Dictionary<ulong, Dictionary<ulong,TServerUserStorage>> storedServerUsers;
        private Dictionary<ulong, TUserStorage> storedUsers;

        public DataStorageService()
        {
            this.serializer = JsonSerializer.Create();
        }

        public string BotFileName { get; set; } = "BotData.json";

        public TBotStorage BotStorage { get => this.botStorageInternal; }

        public string ServerFileName { get; set; } = "ServerData.json";

        public string ServerUserFileName { get; set; } = "ServerUserData.json";

        public string UserFileName { get; set; } = "UserData.json";

        public void ClearServerStorage(ulong id) => this.ClearServerStorage(id, true);

        public void ClearServerUserStorage(ulong serverID, ulong id) => this.ClearServerUserStorage(serverID, id, true);

        public void ClearUserStorage(ulong id) => this.ClearUserStorage(id, true);

        public TServerStorage GetServerStorage(ulong id)
        {
            this.storedServers.TryGetValue(id, out var storage);
            if (storage == null)
            {
                storage = (TServerStorage)Activator.CreateInstance(typeof(TServerStorage), id);
                this.SetServerStorage(id, storage);
            }

            return storage;
        }

        public TServerUserStorage GetServerUserStorage(ulong serverID, ulong id)
        {
            var dict = this.GetServerUserStorageDictionary(serverID);
            dict.TryGetValue(id, out var storage);
            if (storage == null)
            {
                storage = (TServerUserStorage) Activator.CreateInstance(typeof(TServerUserStorage), serverID, id);
                this.SetServerUserStorage(serverID, id, storage);
            }

            return storage;
        }

        private Dictionary<ulong, TServerUserStorage> GetServerUserStorageDictionary(ulong serverID)
        {
            this.storedServerUsers.TryGetValue(serverID, out var dict);
            if (dict == null)
            {
                dict = (Dictionary<ulong, TServerUserStorage>)Activator.CreateInstance(typeof(Dictionary<ulong, TServerUserStorage>));
                this.storedServerUsers.Add(serverID, dict);
            }
            return dict;
        }

        public TUserStorage GetUserStorage(ulong id)
        {
            this.storedUsers.TryGetValue(id, out var storage);
            if (storage == null)
            {
                storage = (TUserStorage)Activator.CreateInstance(typeof(TUserStorage), id);
                this.SetUserStorage(id, storage);
            }

            return storage;
        }

        public void LoadFromFile()
        {
            lock (StorageFileLocker.BotFileLock)
            {
                this.LoadStorageFromFile(ref this.botStorageInternal, this.BotFileName);
            }

            lock (StorageFileLocker.ServerFileLock)
            {
                this.LoadStorageFromFile(ref this.storedServers, this.ServerFileName);
            }

            lock (StorageFileLocker.UserFileLock)
            {
                this.LoadStorageFromFile(ref this.storedUsers, this.UserFileName);
            }

            lock (StorageFileLocker.ServerUserFileLock)
            {
                this.LoadStorageFromFile(ref this.storedServerUsers, this.ServerUserFileName);
            }

            this.SubscribeToObservable(this.BotStorage);
            this.SubscribeToObservables(this.storedServers.Values);
            this.SubscribeToObservables(this.storedUsers.Values);

            foreach (var serverUserList in this.storedServerUsers.Values)
            {
                this.SubscribeToObservables(serverUserList.Values);
            }
        }

        public void OnCompleted() => this.SaveStoragesToFile();

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnNext(IDataContainer value)
        {
        }

        public void SetServerStorage(ulong id, TServerStorage storage)
        {
            this.ClearServerStorage(id, false);
            this.storedServers.Add(id, storage);
            this.SubscribeToObservable(storage);
            this.SaveStoragesToFile();
        }

        public void SetServerUserStorage(ulong serverID, ulong id, TServerUserStorage storage)
        {
            var dict = this.GetServerUserStorageDictionary(serverID);
            this.ClearServerUserStorage(serverID, id, false, false);
            dict.Add(id, storage);
            this.SubscribeToObservable(storage);
            this.SaveStoragesToFile();
        }

        public void SetUserStorage(ulong id, TUserStorage storage)
        {
            this.ClearUserStorage(id, false);
            this.storedUsers.Add(id, storage);
            this.SubscribeToObservable(storage);
            this.SaveStoragesToFile();
        }

        private static void CreateNewInstanceIfNecessary<T>(ref T storage)
        {
            var type = typeof(T);
            try
            {
                if (storage == null)
                {
                    Log.Debug($"Attempting to create a new {type.Name} instance.");
                    storage = (T)Activator.CreateInstance(type);
                }
            }
            catch (Exception e)
            {
                Log.Warning($"Exception when creating new instance of type {type?.Name}.");
                Log.Debug(e, string.Empty);
            }
        }

        private void ClearServerStorage(ulong id, bool notifyObservers)
        {
            this.storedServers.Remove(id);
            if (notifyObservers)
            {
                this.SaveStoragesToFile();
            }
        }

        private void ClearServerUserStorage(ulong serverID, ulong id, bool notifyObservers, bool removeDictionaryIfEmpty = true)
        {
            var dict = this.GetServerUserStorageDictionary(serverID);
            dict.Remove(id);
            if (removeDictionaryIfEmpty && dict.Count < 1)
            {
                storedServerUsers.Remove(serverID);
            }

            if (notifyObservers)
            {
                this.SaveStoragesToFile();
            }
        }

        private void ClearUserStorage(ulong id, bool notifyObservers)
        {
            this.storedUsers.Remove(id);
            if (notifyObservers)
            {
                this.SaveStoragesToFile();
            }
        }

        private void LoadStorageFromFile<T>(ref T storage, string filename)
        {
            this.TryToLoadStorageFromFile(ref storage, filename);
            DataStorageService<TBotStorage, TServerStorage, TUserStorage, TServerUserStorage>.CreateNewInstanceIfNecessary(ref storage);
        }

        private T ParseJsonStorage<T>(Type type, JsonTextReader reader)
        {
            return type != null ? (T)this.serializer.Deserialize(reader, type) : this.serializer.Deserialize<T>(reader);
        }

        private void SaveBotStorageToFile()
        {
            lock (StorageFileLocker.BotFileLock)
            {
                this.SaveStorageToFile(this.BotStorage, this.BotFileName);
            }
        }

        private void SaveServerStoragesToFile()
        {
            lock (StorageFileLocker.ServerFileLock)
            {
                this.SaveStorageToFile(this.storedServers, this.ServerFileName);
            }
        }

        private void SaveServerUserStoragesToFile()
        {
            lock (StorageFileLocker.ServerUserFileLock)
            {
                this.SaveStorageToFile(this.storedServerUsers, this.ServerUserFileName);
            }
        }

        private void SaveStoragesToFile()
        {
            this.SaveBotStorageToFile();
            this.SaveServerStoragesToFile();
            this.SaveUserStoragesToFile();
            this.SaveServerUserStoragesToFile();
        }

        private void SaveStorageToFile(object storage, string filename)
        {
            var strWriter = new StringWriter();
            var writer = new JsonTextWriter(strWriter)
            {
                Formatting = Formatting.Indented
            };
            this.serializer.Serialize(writer, storage, storage.GetType());

            var serializedContent = strWriter.ToString();
            writer.Close();
            strWriter.Close();
            File.WriteAllText(filename, serializedContent);
        }

        private void SaveUserStoragesToFile()
        {
            lock (StorageFileLocker.UserFileLock)
            {
                this.SaveStorageToFile(this.storedUsers, this.UserFileName);
            }
        }

        private void SubscribeToObservable(IObservable<IDataContainer> observable)
        {
            observable.Subscribe(this);
        }

        private void SubscribeToObservables<T>(Dictionary<T, TServerStorage>.ValueCollection storages)
        {
            foreach (var storage in storages)
            {
                this.SubscribeToObservable(storage);
            }
        }

        private void SubscribeToObservables<T>(Dictionary<T, TServerUserStorage>.ValueCollection storages)
        {
            foreach (var storage in storages)
            {
                this.SubscribeToObservable(storage);
            }
        }

        private void SubscribeToObservables<T>(Dictionary<T, TUserStorage>.ValueCollection storages)
        {
            foreach (var storage in storages)
            {
                this.SubscribeToObservable(storage);
            }
        }

        private void TryToLoadStorageFromFile<T>(ref T storage, string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Log.Debug($"Storage file {filename} found. Attempting to parse Json data.");
                    var reader = new JsonTextReader(new StringReader(File.ReadAllText(filename)));
                    storage = this.ParseJsonStorage<T>(typeof(T), reader);
                }
            }
            catch (Exception e)
            {
                Log.Warning("Exception when parsing stored Data. File might be corrupt.");
                Log.Debug(e, string.Empty);
            }

            if (storage == null)
            {
                Log.Debug($"Was unable to parse file {filename}; it might not exist. ");
            }
        }
    }
}
