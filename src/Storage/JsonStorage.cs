using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace EpisodeTracker.Storage
{
    public class JsonStorage : IStorage
    {
        private readonly string JsonFile;

        public JsonStorage(string jsonFile)
        {
            JsonFile = jsonFile;
            EnsureExistingFile();
        }

        public StoreModel Load()
        {
            StoreModel storeModel = new StoreModel();

            using (StreamReader r = new StreamReader(JsonFile, Encoding.UTF8))
            {
                string json = r.ReadToEnd();
                if (!String.IsNullOrEmpty(json))
                {
                    storeModel = JsonConvert.DeserializeObject<StoreModel>(json);
                }
            }

            return storeModel;
        }

        public void Save(StoreModel storeModel)
        {
            string json = JsonConvert.SerializeObject(storeModel, Formatting.Indented);
            File.WriteAllText(JsonFile, json, Encoding.UTF8);
        }

        private void EnsureExistingFile()
        {
            FileInfo fileInfo = new FileInfo(JsonFile);

            if (!fileInfo.Exists)
            {
                CreateFile();
            }
        }

        private void CreateFile()
        {
            FileInfo fileInfo = new FileInfo(JsonFile);
            Directory.CreateDirectory(fileInfo.Directory.FullName);
            File.Create(JsonFile).Close();
        }
    }
}
