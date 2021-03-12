using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace SimpleDiscordBot.DataStorage.Util
{
    public static class Parsing
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create();

        public static T ParseObject<T>(string jsonSerializedObject, Type type = null) where T : class, ISerializable
        {
            type ??= typeof(T);
            try
            {
                var reader = new JsonTextReader(new StringReader(jsonSerializedObject));
                return Serializer.Deserialize(reader, type) as T;
            }
            catch (Exception e)
            {
                Log.Debug(e, string.Empty);
            }
            return default;
        }

        public static T CreateNewObject<T>() where T : class, ISerializable
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            catch (Exception e)
            {
                Log.Debug(e, string.Empty);
            }

            return default;
        }

        public static string Serialize<T>(T obj) where T : class, ISerializable
        {
            var strWriter = new StringWriter();
            var writer = new JsonTextWriter(strWriter);

            Serializer.Serialize(writer, obj, obj.GetType());

            var serializedContent = strWriter.ToString();
            writer.Close();
            strWriter.Close();
            return serializedContent;
        }

        public static T DeserializeOrCreateInstance<T>(string jsonSerializedObject, Type type = null) where T : class, ISerializable
        {
            var parsedObject = jsonSerializedObject != null ? ParseObject<T>(jsonSerializedObject, type ?? typeof(T)) : null;
            return parsedObject ?? CreateNewObject<T>();
        }

        public static T DeserializeOrCreateFromDictionary<T>(Dictionary<string, string> dictionary, string name, Type type = null) where T : class, ISerializable
        {
            type ??= typeof(T);
            if (dictionary != null)
            {
                dictionary.TryGetValue(name, out var jsonSerializedObject);
                var rVal = DeserializeOrCreateInstance<T>(jsonSerializedObject, type);
                if (rVal == null)
                {
                    Log.Debug($"Unable to parse or create object {name}. It might not be serialized properly, " +
                        $"or there might not exist a default constructor for type {nameof(T)}.");
                }

                return rVal ?? default;
            }
            return default;
        }

        public static T GetExistingOrNewObject<T>(SerializationInfo info, string name, Type type = null) where T : class, ISerializable
        {
            type ??= typeof(T);
            T obj = null;
            try
            {
                obj = info.GetValue(name, type) as T;
            }
            catch (Exception e)
            {
                Log.Debug(e, e.Message);
            }

            if (obj == null)
            {
                try
                {
                    obj = Activator.CreateInstance(type) as T;
                }
                catch (Exception e)
                {
                    Log.Debug(e, e.Message);
                }
            }

            return obj;
        }
    }
}
