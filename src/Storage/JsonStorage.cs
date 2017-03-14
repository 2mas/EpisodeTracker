using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace EpisodeTracker.Storage
{
    public class JsonStorage : IStorage
    {
        private readonly string JsonFile;

        private StoreModel StoreModel;

        public JsonStorage(string jsonFile)
        {
            JsonFile = jsonFile;
            EnsureExistingFile();
            Load();
        }

        private void Load()
        {
            this.StoreModel = new StoreModel();

            using (StreamReader r = new StreamReader(JsonFile, Encoding.UTF8))
            {
                string json = r.ReadToEnd();
                if (!String.IsNullOrEmpty(json))
                {
                    this.StoreModel = JsonConvert.DeserializeObject<StoreModel>(json, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });
                }
            }
        }

        public StoreModel GetStoreModel()
        {
            return this.StoreModel;
        }

        public void Save(StoreModel storeModel)
        {
            string json = JsonConvert.SerializeObject(
                storeModel, 
                Formatting.Indented, 
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                }
            );

            File.WriteAllText(JsonFile, json, Encoding.UTF8);
            this.StoreModel = storeModel;
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

            // Save a skeleton model
            Save(new StoreModel());
        }
    }
}
