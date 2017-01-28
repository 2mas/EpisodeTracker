namespace EpisodeTracker.Storage
{
    public interface IStorage
    {
        StoreModel Load();
        void Save(StoreModel model);
    }
}
