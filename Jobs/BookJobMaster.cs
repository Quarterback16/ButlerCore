using ButlerCore.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ButlerCore.Jobs
{
    public class BookJobMaster : JobMaster
    {
        public string[] BookFolders { get; set; }
        public BookJobMaster(
            ILogger logger,
            string[] bookFolders)
        {
            BookFolders = bookFolders ?? throw new ArgumentNullException(
                nameof(bookFolders));
        }

        public int DoDetectorJob(
            int nMonthsBack = 1)
        {
            var newBookFiles = new List<Book>();
            (DateTime startDate, DateTime endDate) = GetDateRange(nMonthsBack);

            foreach (var bookFolder in BookFolders)
            {
                var bookFiles = IdentifyNewBooks(
                    bookFolder,
                    startDate,
                    endDate);
                newBookFiles.AddRange(bookFiles);
            }

            LogIt($"{newBookFiles.Count} new books detected tra {FormatDate(startDate)} and {FormatDate(endDate)}");
            int maxTopicLength = newBookFiles.Max(b => b.Topic.Length);
            newBookFiles.ForEach(b =>
                LogIt(
                    $"{b.AccessDate}: {TopicName(b,maxTopicLength)} : {b.Title}"));
            return 0;
        }

        private static string TopicName(
            Book b, 
            int maxTopicLength)
        {
            var sb = new StringBuilder();
            sb.Append(b.Topic.PadRight(maxTopicLength));
            return sb.ToString();
        }

        public static List<Book> IdentifyNewBooks(
            string bookRootFolder,
            DateTime startDate,
            DateTime endDate)
        {
            var list = new List<FileInfo>();
            var fileEntries = Directory.GetFiles(
                bookRootFolder,
                "*.*",
                SearchOption.AllDirectories);
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                if (ExtensionSaysBook(fileInfo.Extension))
                    list.Add(fileInfo);
            }
            var recentBooks = list
                .Where(
                    b => b.LastWriteTime <= endDate
                    && b.LastWriteTime >= startDate)
                .Select(
                    b => new Book
                    {
                        Topic = TopicFrom(
                            b, 
                            bookRootFolder),
                        Title = TitleFrom(
                            b.Name,
                            b.Extension),
                        Format = b.Extension.Replace(".", string.Empty),
                        AccessDate = $"{b.LastWriteTime:yyyy-MM-dd}"
                    })
                .OrderByDescending(o => o.AccessDate)
                .ToList();
            return recentBooks;
        }

        private static (DateTime startDate, DateTime endDate) GetDateRange(
            int nMonthsBack)
        {
            var endDate = DateTime.Today.AddDays(1);
            var startDate = endDate.AddMonths(-nMonthsBack);
            var monthStartDate = new DateTime(
                startDate.Year,
                startDate.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Unspecified);
            return (monthStartDate.Date, endDate.Date);  // Normalize to midnight for clean ranges [cite:33]
        }

        private static string TitleFrom(
            string name,
            string extension) =>
        
            name.Replace(
                extension,
                string.Empty);
        

        private static string TopicFrom(
            FileInfo b,
            string rootFolder)
        {
            if (b.DirectoryName == null)
                return string.Empty;

            var path = b.DirectoryName
                .Replace(
                    rootFolder,
                    string.Empty);
            if (path.Length < 2)
                return rootFolder.Substring(3);
            return path.Substring(0, path.Length);
        }

        private static bool ExtensionSaysBook(
            string extension)
        {
            if (extension.ToLower() == ".pdf")
                return true;
            if (extension.ToLower() == ".epub")
                return true;
            return false;
        }
    }
}
