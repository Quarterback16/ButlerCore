using EventStoreService;
using HearthstoneReportService;
using InjectorMicroService;
using Knoware.HearthstoneApi.Service;
using Microsoft.Extensions.Logging;

namespace ButlerCore.Jobs
{
    public class HearthstoneJobMaster
    {
#if !DEBUG
        private readonly ILogger _logger;
#endif
        private readonly HsReportService _hrs;
        private readonly IMarkdownInjector _mdInjector;
        public string? CurrentMeta { get; set; }
        public string ObsidianHeartstoneMetasFolder { get; set; }

        public HearthstoneJobMaster(
            ILogger logger,
            string dropBoxFolder,
            string hsEventFolder)
        {
#if !DEBUG
            _logger = logger;
            _logger.LogInformation($"Events loaded from : {hsEventFolder}");
#endif
            _hrs = new HsReportService(
                new HsEventStore(
                    hsEventFolder));
            ObsidianHeartstoneMetasFolder = "//03 - Hearthstone//Metas//";
#if !DEBUG
            _logger.LogInformation("HS event service initialised.");
            _logger.LogInformation($"writing reports to {ObsidianHeartstoneMetasFolder} folder.");
#endif
            var hcs = new HearthstoneCardService();
            CurrentMeta = hcs.GetMeta();
#if !DEBUG
            _logger.LogInformation($"Current Meta is : {CurrentMeta}");
#endif
            _mdInjector = new MarkdownInjector(
                $"{dropBoxFolder}Obsidian\\ChestOfNotes\\");
#if !DEBUG
            _logger.LogInformation("md injector initialisd");
#endif
        }

        public string DoChampDeckReport()
        {
#if !DEBUG
            _logger.LogInformation("DoChampDeckReport...");
#endif
            var md = WrapWithPre(_hrs.ChampDeckReport());
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "metachamp",
                markdown: md);
            return md;
        }

        public string DoMetaChampReport()
        {
#if !DEBUG
            _logger.LogInformation("DoMetaChampReport...");
#endif
            var md = WrapWithPre(_hrs.MetaChampReport());
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "metachamp",
                markdown: md);
            return md;
        }

        private static string WrapWithPre(string report) =>
        
            $"```cs{Environment.NewLine}{report}{Environment.NewLine}```{Environment.NewLine}";
        

        public string DoWinLossGraph()
        {
#if !DEBUG
            _logger.LogInformation("DoWinLossGraph...");
#endif
            var md = WrapWithPre(_hrs.WinLossGraph(DateTime.Now));
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "win-loss",
                markdown: md);
            return md;
        }
    }
}
