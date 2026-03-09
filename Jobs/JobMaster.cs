namespace ButlerCore.Jobs
{
    public class JobMaster
    {
        public static void LogIt(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#else
            _logger.LogInformation(msg);
#endif
        }

        public static string FormatDate(DateTime dt) =>
        
            dt.ToString("yyyy-MM-dd");
           
    }
}
