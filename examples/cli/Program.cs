using EpisodeTracker.CLI.Output;
using System;
using System.Linq;

namespace EpisodeTracker.CLI
{
    class Program
    {
        /// <summary>
        /// The main tracker library
        /// </summary>
        internal static Tracker episodeTracker;

        /// <summary>
        /// Possiblity to redirect output
        /// </summary>
        internal static IOutput Output;

        /// <summary>
        /// If run as cli, controls if application should quit
        /// </summary>
        private static bool ShouldExit = false;

        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--silent")
                {
                    Program.Output = new NoOutput();
                }
                else
                {
                    Program.Output = new ConsoleOutput();
                }
            }
            else
            {
                Program.Output = new ConsoleOutput();
            }

            try
            {
                Bootstrap.Initialize();
                
                // Run as service or schedule etc, no messages printed
                if (args.Length > 0)
                {   
                    Commands.Update();
                } else
                {
                    Run();
                }
                Environment.ExitCode = 0;
            }
            catch (Exception e)
            {
                Program.Output.WriteLine(e.Message);
                Environment.ExitCode = 1;
            }

            return Environment.ExitCode;
        }

        private static void Run()
        {
            Program.Output.WriteLine("Welcome to  EpisodeTracker, type -h/--help for help or q/quit to quit");
            Program.Output.WriteLine();

            ShowConfigSuggestions();
            
            string input;
            while (!ShouldExit)
            {
                Program.Output.Write("EpisodeTracker> ");

                input = Console.ReadLine();

                if (input.ToLower() == "q" || input.ToLower() == "quit")
                    ShouldExit = true;

                Options options = new Options();
                ParseCommand(input.Split(new char[] { ' ' }, 2), options);
            }
        }

        private static void ShowConfigSuggestions()
        {
            if (!Program.episodeTracker.Storage.GetStoreModel().ApiCredentials.IsValid)
            {
                Program.Output.WriteLine("Your ApiCredentials are not yet set, please provide these in configuration before use");
                Program.Output.WriteLine();
            }

            if (!Program.episodeTracker.Storage.GetStoreModel().NotificationSettings.Configurations.Any())
            {
                Program.Output.WriteLine("You have no notifications set up, provide these in configuration to recieve notification when new releases are available");
                Program.Output.WriteLine();
            }
        }

        private static void ParseCommand(string[] args, Options options)
        {
            Program.Output.WriteLine();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (!String.IsNullOrEmpty(options.Search))
                    Commands.Search(options.Search);

                if (options.Track > 0)
                    Commands.Track(options.Track);

                if (options.UnTrack > 0)
                    Commands.UnTrack(options.UnTrack);

                if (options.View > 0)
                    Commands.View(options.View);

                if (options.Episodes > 0)
                    Commands.ViewEpisodes(options.Episodes);

                if (!String.IsNullOrEmpty(options.List))
                    Commands.List(options.List);

                if (!String.IsNullOrEmpty(options.SeenSeries))
                    Commands.SeenSeries(options.SeenSeries);

                if (options.Seen > 0)
                    Commands.Seen(options.Seen);

                if (options.Update > 0)
                    Commands.Update();

                if (!String.IsNullOrEmpty(options.Configuration))
                    Commands.Configuration(options.Configuration);
            }
        }
    }
}
