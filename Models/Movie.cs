namespace ButlerCore.Models
{
    public class Movie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string FileName { get; set; }

        public Movie()
        {
            Title = string.Empty;
            Year = string.Empty;
            FileName = string.Empty;
        }

        public Movie(
            string title,
            string year,
            string fileName)
        {
            Title = title;
            Year = year;
            FileName = fileName;
        }

        public override string ToString() => Title;
    }

    
}
