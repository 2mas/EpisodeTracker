namespace EpisodeTracker.Storage
{
    public interface IStorage
    {
        void Save();
        StoreModel GetStoreModel();
        void SetStoreModel(StoreModel storeModel);
    }
}
