namespace EpisodeTracker.Storage
{
    public interface IStorage
    {
        void Save(StoreModel model);
        StoreModel GetStoreModel();
    }
}
