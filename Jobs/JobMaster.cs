using InjectorMicroService;
using Microsoft.Extensions.Logging;

namespace ButlerCore.Jobs
{
	public class JobMaster
	{
#if !DEBUG
		private readonly ILogger _logger;
#endif
		public readonly IMarkdownInjector? MdInjector;

		public JobMaster(
			ILogger logger)
		{
#if !DEBUG
			_logger = logger;
#endif
		}

		public JobMaster(
			ILogger logger,
			string dropBoxFolder)
		{
#if !DEBUG
			_logger = logger;
#endif
			var obsidianFolder = $"{dropBoxFolder}Obsidian\\ChestOfNotes\\";

			MdInjector = new MarkdownInjector(
				obsidianFolder);
			LogIt($"md injector initialised to {obsidianFolder}");
		}

#if DEBUG
		public static void LogIt(string msg)
		{
			Console.WriteLine(msg);
		}
		public static void LogError(string msg)
		{
			Console.WriteLine(msg);
		}
#else
		public void LogIt(string msg)
		{
			_logger.LogInformation(msg);
		}

		public void LogError(string msg)
		{
			_logger?.LogError(msg);
		}
#endif

		public static string FormatDate(DateTime dt) =>

			dt.ToString("yyyy-MM-dd");

		public static string CurrentMonth() =>
		
			DateTime.Now.ToString("yyyy-MM");	

	}
}
