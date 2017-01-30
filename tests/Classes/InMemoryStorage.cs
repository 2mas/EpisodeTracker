using EpisodeTracker.Storage;

namespace EpisodeTracker.Tests.Classes
{
    class InMemoryStorage : IStorage
    {
        private StoreModel StoreModel;

        public InMemoryStorage()
        {
            StoreModel = new StoreModel();
        }

        public StoreModel Load()
        {
            return this.StoreModel;
        }

        public void Save(StoreModel model)
        {
        }
    }
}
