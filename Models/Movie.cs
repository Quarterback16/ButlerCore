namespace ButlerCore.Models
{
    public class Movie
    {
        public string Title { get; set; }
        public string Year { get; set; }

        public Movie()
        {
        }

        public Movie(
            string title,
            string year)
        {
            Title = title;
            Year = year;
        }

        public override string ToString() => Title;
    }

    
}
