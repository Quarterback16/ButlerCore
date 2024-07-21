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
        public string CurrentMeta { get; set; }
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
            ObsidianHeartstoneMetasFolder = $"//03 - Hearthstone//Metas//";
            var hcs = new HearthstoneCardService();
            CurrentMeta = hcs.GetMeta();
            _mdInjector = new MarkdownInjector(
                $"{dropBoxFolder}Obsidian\\ChestOfNotes\\");
        }

        public string DoChampDeckReport()
        {
            var md = WrapWithPre(_hrs.ChampDeckReport());
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "metachamp",
                markdown: md);
            return md;
        }

        public string DoMetaChampReport()
        {
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
            var md = WrapWithPre(_hrs.WinLossGraph(DateTime.Now));
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "win-loss",
                markdown: md);
            return md;
        }
    }
}
