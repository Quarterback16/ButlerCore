using EventStoreService;
using HearthstoneReportService;
using InjectorMicroService;
using Knoware.HearthstoneApi.Service;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ButlerCore.Jobs
{
    public class HearthstoneJobMaster
    {
        private readonly ILogger _logger;

        private readonly HsReportService _hrs;
        private readonly IMarkdownInjector _mdInjector;
        public string? CurrentMeta { get; set; }
        public string ObsidianHeartstoneMetasFolder { get; set; }

        public HearthstoneJobMaster(
            ILogger logger,
            string dropBoxFolder,
            string hsEventFolder)
        {
            try
            {
                _logger = logger;
                LogIt($"Events loaded from : {hsEventFolder}");
                _hrs = new HsReportService(
                    new HsEventStore(
                        hsEventFolder));
                ObsidianHeartstoneMetasFolder = "//03 - Hearthstone//Metas//";
                LogIt("HS event service initialised.");
                LogIt(
                    $"writing reports to {ObsidianHeartstoneMetasFolder} folder.");
                var hcs = new HearthstoneCardService();
                var apiKey = hcs.ApiKey();
                LogIt($"HS API key : {apiKey}");
                var info = hcs.GetInfo();
                if (info != null && info.Patch != null)
                {
                    CurrentMeta = hcs.GetMeta();
                }
                LogIt($"Current Meta is : {CurrentMeta}");
                _mdInjector = new MarkdownInjector(
                    $"{dropBoxFolder}Obsidian\\ChestOfNotes\\");
                LogIt("md injector initialisd");
            }
            catch (Exception ex)
            {
                _logger.LogError( ex.Message );
                Console.WriteLine(ex.Message);
            }
        }

        public string DoChampDeckReport()
        {
            LogIt("DoChampDeckReport...");
            var md = WrapWithPre(_hrs.ChampDeckReport());
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "champdeck",
                markdown: md);
            LogIt(md);
            return md;
        }

        public string DoMetaChampReport()
        {
            LogIt("DoMetaChampReport...");
            var md = WrapWithPre(_hrs.MetaChampReport());
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "metachamp",
                markdown: md);
            LogIt(md);
            return md;
        }

        private void LogIt(string msg)
        {
#if DEBUG
            Debug.WriteLine(msg);
#else 
            _logger.LogInformation(msg);
#endif
        }

        private static string WrapWithPre(string report) =>
        
            $"```cs{Environment.NewLine}{report}{Environment.NewLine}```{Environment.NewLine}";
        

        public string DoWinLossGraph()
        {
            LogIt("DoWinLossGraph...");
            var md = WrapWithPre(_hrs.WinLossGraph(DateTime.Now));
            _mdInjector.InjectMarkdown(
                targetfile: $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md",
                tagName: "win-loss",
                markdown: md);
            LogIt(md);
            return md;
        }
    }
}
