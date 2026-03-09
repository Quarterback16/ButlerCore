using ButlerCore.Models;
using WikiPages;

namespace ButlerCore.Helpers
{
    public static class MarkdownHelper
    {
        public static string GenerateBookTable(
            List<Book> books)
        {
            var page = new WikiPageWithTable();
            page.AddHeading("New Books this month", 3);
            page.Table.AddColumn("Topic");
            page.Table.AddColumn("Book");
            page.Table.AddColumn("when");
            page.Table.AddColumnCentred("format");
            page.Table.AddRows(books.Count);
            var nRow = 0;
            foreach (var book in books)
            {
                nRow++;
                page.Table.AddCell(nRow, "Topic", book.Topic ?? string.Empty);
                page.Table.AddCell(nRow, "Book", book.Title ?? string.Empty);
                page.Table.AddCell(nRow, "when", book.AccessDate ?? string.Empty);
                page.Table.AddCell(nRow, "format", book.Format ?? string.Empty);
            }
            page.AddBlankLine();
            return page.PageTableContents();
        }
    }
}
