using ButlerCore.Models;

namespace ButlerCore.Helpers
{
    public static class MovieHelper
    {
        public static List<MovieProperty> KnownProperties()
        {
            return new List<MovieProperty>()
            {
                new MovieProperty("How"),
                new MovieProperty("With"),
                new MovieProperty("when"),
                new MovieProperty("Year"),
                new MovieProperty("genre"),
                new MovieProperty("author"),
                new MovieProperty("rating"),
                new MovieProperty("Keeper"),
                new MovieProperty("Priority"),
                new MovieProperty("Completion"),
            };
        }
    }
}
