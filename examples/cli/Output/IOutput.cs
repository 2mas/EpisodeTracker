namespace EpisodeTracker.CLI.Output
{
    public interface IOutput
    {
        void Write(string value);
        void WriteLine();
        void WriteLine(string value);
    }
}