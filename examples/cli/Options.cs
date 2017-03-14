using CommandLine;
using CommandLine.Text;

namespace EpisodeTracker.CLI
{
    internal class Options
    {
        [Option('s', "search", Required = false,
          HelpText = "-s {search string} to search for series")]
        public string Search { get; set; }

        [Option('t', "track", Required = false,
          HelpText = "-t {number} to track series by listing-number")]
        public int Track { get; set; }

        [Option("untrack", Required = false,
          HelpText = "--untrack {number} to untrack series by listing-number")]
        public int UnTrack { get; set; }

        [Option('l', "list", Required = false,
          HelpText = "-l {tracked/unseen} to list all tracked series or unseen episodes")]
        public string List { get; set; }

        [Option("seenseries", Required = false,
          HelpText = "--seenseries {all/series id} to mark unseen episodes as seen")]
        public string SeenSeries { get; set; }

        [Option("seen", Required = false,
          HelpText = "--seen {episode id} to mark an unseen episode as seen")]
        public int Seen { get; set; }

        [Option('v', "view", Required = false,
          HelpText = "-v {series id} to view series information by Series Id")]
        public int View { get; set; }

        [Option('e', "view-episodes", Required = false,
          HelpText = "-e {series id} to view episode information by Series Id")]
        public int Episodes { get; set; }

        [Option("update", Required = false,
          HelpText = "--update {1} to run updatecheck for new episodes.")]
        public int Update { get; set; }

        [Option("config", Required = false,
          HelpText = "--config list | add to view current config or add a new config-element such as a notifier.")]
        public string Configuration { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

            help.AddPostOptionsLine("To quit type q/quit\n\n");

            return help;
        }
    }
}
