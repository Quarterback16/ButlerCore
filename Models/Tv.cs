namespace ButlerCore.Models
{
    public class Tv
    {
        public string Title { get; set; }
        public string Year { get; set; }

        public Tv()
        {
        }

        public Tv(
            string title,
            string year)
        {
            Title = title;
            Year = year;
        }

        public override string ToString() => Title;
    }
}
