using System;

namespace EpisodeTracker.CLI.Output
{
    class NoOutput : IOutput
    {
        public void Write(string value)
        {
            return;
        }

        public void WriteLine()
        {
            return;
        }

        public void WriteLine(string value)
        {
            return;
        }
    }
}
