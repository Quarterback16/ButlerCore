using LanguageExt;
using static LanguageExt.Prelude;
using static System.Console;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Configuration;
using InjectorMicroService;
using TipItService.Helpers;
using ButlerCore.Jobs;

namespace ButlerCore
{
    internal partial class Program
    {
        public ILogger? Logger { get; set; }

        static void Main()
        {
            _ = GetSettings()
                .Bind(x => SetupLogging(x))
                .Bind(x => DumpSettings(x))
                .Match(
                    Some: s => DoTheChores(s),
                    None: () => None);
        }

        private static Option<ButlerCoreContext> SetupLogging(
            ButlerCoreContext context)
        {
            if (string.IsNullOrEmpty(context.LogFile))
            {
                WriteLine("No LogFile path found");
                return None;
            }
            var logFile = $"{context.LogFile}-{DateTime.Now:yyyy-MM-dd}.log";
            context.StreamWriter = new(logFile, append: true);
            using ILoggerFactory factory = LoggerFactory.Create(
                builder =>
                {
                    // output to the console
                    builder.AddSimpleConsole(
                        options =>
                        {
                            options.IncludeScopes = false;
                            options.SingleLine = true;
                            options.TimestampFormat = "HH:mm:ss ";
                            options.ColorBehavior = LoggerColorBehavior.Enabled;
                        });
                    // and output to a text file
                    builder.AddProvider(
                        new CustomFileLoggerProvider(
                            context.StreamWriter));
                });
            context.Logger = factory.CreateLogger<Program>();
            return context;
        }

        private static Option<ButlerCoreContext> DoTheChores(
            ButlerCoreContext context)
        {
            if (context.Logger == null)
            {
                WriteLine("No logger - aborting mission");
                return None;
            }
            // connected now loop around until knock off time
            while (DateTime.Now.TimeOfDay < ToTimeOfDay(context.KnockOffTime))
            {
                var nErrors = TipitJob(context);
                nErrors += MovieJobs(context);
                nErrors += TvJobs(context);

                if (nErrors > 0)
                {
                    LogMessage(
                        context.Logger,
                        $"There are {nErrors} errors atm");
                }
                else
                    LogMessage(
                        context.Logger,
                        $@"No errors found at {DateTime.Now:u} {DaysSince(context)} days without incident");
                SleepForMinutes(context,60);
            }
            LogMessage(
                context.Logger,
                $"Knock off time {context.KnockOffTime} : Butler Core shutting down...");

            return None;
        }

        private static void SleepForMinutes(
            ButlerCoreContext context,
            int minutes)
        {
            var msToWait = context.Frequency == null
                    ? 60000 * minutes
                    : context.Frequency.Value * 1000 * minutes;
            LogMessage(context.Logger, $"Restart in {msToWait/60000} minutes");
            Thread.Sleep(msToWait);
        }

        private static string DaysSince(
            ButlerCoreContext context) =>

                context.StartDateTime.HasValue
                    ? (DateTime.Now - context.StartDateTime).Value.TotalDays.ToString("F1")
                    : "Missing StartDateTime";

        private static TimeSpan ToTimeOfDay(
            string? knockOffTime) =>

                (knockOffTime == null)
                    ? TimeSpan.Zero
                    : DateTime.Parse(knockOffTime).TimeOfDay;


        private static string SelectConnectionString(
            ButlerCoreContext settings) =>

                settings.ConnectTo == "Prod"
                    ? settings?.ConnectionStrings?.Prod ?? string.Empty
                    : settings?.ConnectionStrings?.Dev ?? string.Empty;


        private static Option<ButlerCoreContext> GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(
                    "appSettings.json",
                    optional: false,
                    reloadOnChange: true);

            var config = builder.Build();

            if (config != null)
            {
                var settings = config.GetSection("Settings")
                    .Get<ButlerCoreContext>(); // needs Configuration.Binder NuGet

                if (settings != null)
                {
                    //  Use default values if none found
                    if (settings.StartDateTime == null)
                        settings.StartDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Local);

                    if (string.IsNullOrEmpty(settings.ConnectTo))
                        settings.ConnectTo = "Dev";

                    settings.Beep ??= false;

                    settings.Stats ??= false;

                    if (!settings.Frequency.HasValue)
                        settings.Frequency = 60;

                    return Some(settings);
                }
                else
                    return None;
            }
            else
                return None;
        }

        private static Option<ButlerCoreContext> DumpSettings(
            ButlerCoreContext context)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                DateTime buildDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                    .AddDays(version.Build)
                    .AddSeconds(version.Revision * 2);
                LogSettingMessage(context.Logger, "Build        ", buildDate.ToString("yyyy-MM-dd"));
            }

            var startDate = context.StartDateTime?.ToString("yyyy'-'MM'-'dd' 'hh':'mm");

            LogSettingMessage(context.Logger, "Starting", startDate);
            WriteSettings(context.Logger, "AppNames  ", context.AppNames);
            LogSettingMessage(context.Logger, "ConnectTo      ", context.ConnectTo);
            LogSettingMessage(context.Logger, "Conn str       ", SelectConnectionString(context));
            LogSettingMessage(context.Logger, "Frequency      ", context.Frequency?.ToString());
            LogSettingMessage(context.Logger, "Log File       ", context.LogFile?.ToString());
            LogSettingMessage(context.Logger, "Result File    ", context.ResultsFile?.ToString());
            LogSettingMessage(context.Logger, "Dropbox Folder ", context.DropBoxFolder?.ToString());
            return Some(context);
        }

        private static void WriteSettings(
            ILogger? logger,
            string category,
            string[]? settings)
        {
            if (settings == null)
                return;
            LogSettingMessage(
                logger,
                category,
                string.Join(", ", settings));
        }

        private static int TipitJob(
            ButlerCoreContext settings)
        {
            try
            {
                LogMessage(settings.Logger, "TipItJob ...");
                if (settings.StartDateTime == null)
                    return 1;
                if (settings.ResultsFile == null)
                {
                    LogMessage(settings.Logger,"No Results file found");
                    return 1;
                }
                var msg = IsWeekend() 
                    ? "Not processing on the Weekend"
                    : $"Doing the TipitIn job writing new results to {settings.ResultsFile}";
                LogMessage(
                    settings.Logger,
                    msg);

                var ts = new TipItService.TipItService(
                    settings.DropBoxFolder);
                //  1. Update Results   /////////////////////////////////////////////////////////
                var newState = ts.GetNewTippingState(
                    DateTime.Now.AddDays(1));
                LogMessage(settings.Logger, $"{ts.NewResults.Count} new results added");
                ts.NewResults.ForEach(
                    nr =>
                    {
                        LogMessage(settings.Logger, nr.ToString());
                    });
                ts.WriteMatchEventJson(newState, settings.ResultsFile);

                //  2. Inject the Tips   ////////////////////////////////////////////////////////
                var mi = new MarkdownInjector(
                    $"{settings.DropBoxFolder}obsidian//ChestOfNotes//");
                if (!IsWeekend())
                {
                    LogMessage(settings.Logger, $"Injecting into NRL");
                    ts.Inject("NRL", "nrl-tips", mi);
                    LogMessage(settings.Logger, $"Injecting into AFL");
                    ts.Inject("AFL", "afl-tips", mi);
                }
                //  3.  Inject Easy Tips  //////////////////////////////////////////////////////
                LogMessage(settings.Logger, $"Injecting easiest into {DashboardUtils.DashboardFile(2024)}");
                var md = ts.Easiest();
                mi.InjectMarkdown(
                    DashboardUtils.DashboardFile(2024),
                    "easiest",
                    md);

                //  4.  Inject Rankings  //////////////////////////////////////////////////////
                if (!IsWeekend())
                {
                    var nrlRanks = ts.InjectRankings("NRL", "nrl-ranks", mi);
                    LogMessage(settings.Logger, nrlRanks);
                    var aflRanks = ts.InjectRankings("AFL", "afl-ranks", mi);
                    LogMessage(settings.Logger, aflRanks);
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogMessage(
                    settings.Logger,
                    $"Exception {ex.Message}");
                throw;
            }

        }

        private static int TvJobs(
            ButlerCoreContext settings)
        {
            try
            {
                LogMessage(settings.Logger, "TvJobs ...");
                if (settings.TvRootFolder == null)
                {
                    LogMessage(settings.Logger, "No TV Root Folder set");
                    return 1;
                }
                if (settings.DropBoxFolder == null)
                {
                    LogMessage(settings.Logger, "No Dropbox Folder set");
                    return 1;
                }
                if (settings.Logger == null)
                {
                    Console.WriteLine("No Logger set");
                    return 1;
                }

                var tjm = new TvJobMaster(
                    settings.Logger,
                    settings.DropBoxFolder,
                    settings.TvRootFolder);

                //  1. Detector always Detects   /////////////////////////////////////////////////////////
                tjm.DoDetectorJob();

                //  2. Optionally Cull   ////////////////////////////////////////////////////////
                var msg = DateTime.Now.Day <= 27
                    ? "You have til the 28th before we start culling"
                    : $"Doing the TV Cull job from {settings.TvRootFolder}";
                LogMessage(
                    settings.Logger,
                    msg);

                if (DateTime.Now.Day > 27)
                    tjm.DoCullJob();

                return 0;
            }
            catch (Exception ex)
            {
                LogMessage(
                    settings.Logger,
                    $"Exception {ex.Message}");
                throw;
            }

        }

        private static int MovieJobs(
            ButlerCoreContext settings)
        {
            try
            {
                LogMessage(settings.Logger, "MovieJobs ...");
                if (settings.MovieRootFolder == null)
                {
                    LogMessage(settings.Logger, "No Movie Root Folder set");
                    return 1;
                }
                if (settings.DropBoxFolder == null)
                {
                    LogMessage(settings.Logger, "No Dropbox Folder set");
                    return 1;
                }
                if (settings.Logger == null)
                {
                    Console.WriteLine( "No Logger set");
                    return 1;
                }

                var mjm = new MovieJobMaster(
                    settings.Logger,
                    settings.DropBoxFolder,
                    settings.MovieRootFolder);

                //  1. Detector always Detects   /////////////////////////////////////////////////////////
                mjm.DoDetectorJob(
                    new MovieService.MovieService());

                //  2. Optionally Cull   ////////////////////////////////////////////////////////
                var msg = DateTime.Now.Day <= 27
                    ? "You have til the 28th before we start culling"
                    : $"Doing the Movie Cull job from {settings.MovieRootFolder}";
                LogMessage(
                    settings.Logger,
                    msg);

                if (DateTime.Now.Day > 27)
                    mjm.DoCullJob();

                return 0;
            }
            catch (Exception ex)
            {
                LogMessage(
                    settings.Logger,
                    $"Exception {ex.Message}");
                throw;
            }

        }

        private static bool IsWeekend() =>
        
            DateTime.Now.DayOfWeek == DayOfWeek.Saturday 
            || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;
        

        [LoggerMessage(Level = LogLevel.Information, Message = "{anyMessage}")]
        static partial void LogMessage(
            ILogger? logger,
            string? anyMessage);

        [LoggerMessage(Level = LogLevel.Information, Message = "Butler Core {startDate}.")]
        static partial void LogStartupMessage(
            ILogger? logger,
            string? startDate);

        [LoggerMessage(Level = LogLevel.Information, Message = "{settingName}: {settingValue}.")]
        static partial void LogSettingMessage(
            ILogger? logger,
            string settingName,
            string? settingValue);

        [LoggerMessage(Level = LogLevel.Information, Message = "{line}")]
        static partial void LogErrorLine(
            ILogger? logger,
            string line);
    }


}
