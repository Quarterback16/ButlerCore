using BattleNetApi.Service;
using EventStoreService;
using HearthstoneReportService;
using Microsoft.Extensions.Logging;

namespace ButlerCore.Jobs
{
	public class HearthstoneJobMaster : JobMaster
	{
		private readonly HsReportService? _hrs;

		public string? CurrentMeta { get; set; }
		public string? ObsidianHeartstoneMetasFolder { get; set; }

		public HearthstoneJobMaster(
			ILogger logger,
			string dropBoxFolder,
			string hsEventFolder)
			: base(logger, dropBoxFolder)  // <-- base constructor call here
		{
			try
			{
				LogIt($"Events loaded from : {hsEventFolder}");
				_hrs = new HsReportService(
					new HsEventStore(
						hsEventFolder));
				ObsidianHeartstoneMetasFolder = "//03 - Hearthstone//Metas//";
				LogIt("HS event service initialised.");
				LogIt(
					$"writing reports to {ObsidianHeartstoneMetasFolder} folder.");
				var hcs = new HearthstoneCardService();
				CurrentMeta = hcs.GetCurrentMeta();
				LogIt($"Current Meta is : {CurrentMeta}");
			}
			catch (Exception ex)
			{
				LogError( ex.Message );
				Console.WriteLine(ex.Message);
			}
		}

		public string DoChampDeckReport()
		{
			var targetFile = $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md";
			LogIt($"DoChampDeckReport...>> {targetFile}");
			var md = WrapWithPre(_hrs.ChampDeckReport());
			MdInjector?.InjectMarkdown(
				targetfile: targetFile,
				tagName: "champdeck",
				markdown: md);
			LogIt(md);
			return md;
		}

		public string DoMetaChampReport()
		{
			var targetFile = $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md";
			LogIt($"DoMetaChampReport...>> {targetFile}");
			var md = WrapWithPre(_hrs.MetaChampReport());
			MdInjector?.InjectMarkdown(
				targetfile: targetFile,
				tagName: "metachamp",
				markdown: md);
			LogIt(md);
			return md;
		}

		private static string WrapWithPre(string report) =>
		
			$"```cs{Environment.NewLine}{report}{Environment.NewLine}```{Environment.NewLine}";
		

		public string DoWinLossGraph()
		{
			var targetFile = $"{ObsidianHeartstoneMetasFolder}{CurrentMeta}.md";
			LogIt($"DoWinLossGraph...>> {targetFile}");
			var md = WrapWithPre(_hrs.WinLossGraph(DateTime.Now));
			MdInjector?.InjectMarkdown(
				targetfile: targetFile,
				tagName: "win-loss",
				markdown: md);
			LogIt(md);
			return md;
		}
	}
}
