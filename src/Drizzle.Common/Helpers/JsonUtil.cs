using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drizzle.Common.Helpers
{
    public static class JsonUtil
    {
        public static T Load<T>(string filePath)
        {
            using StreamReader file = File.OpenText(filePath);
            var serializer = new JsonSerializer
            {
                //TypeNameHandling = TypeNameHandling.Auto
            };
            var tmp = (T)serializer.Deserialize(file, typeof(T));

            //if file is corrupted, json can return null.
            return tmp != null ? tmp : throw new InvalidOperationException("json null/corrupt");
        }

        public static void Save<T>(string filePath, T data)
        {
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                //TypeNameHandling = TypeNameHandling.Auto,
            };

            using StreamWriter sw = new StreamWriter(filePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, data, typeof(T));
        }
    }
}
