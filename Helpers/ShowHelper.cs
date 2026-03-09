namespace ButlerCore.Helpers
{
    public static class ShowHelper
    {
        public static string Plot(
            MovieService.Models.Show? apiData)
        {
            if (apiData == null)
                return string.Empty;

            return string.IsNullOrEmpty(apiData.Plot)
                ? string.Empty
                : apiData.Plot;
        }

        public static string? EmbedPoster(string? poster) =>

            string.IsNullOrEmpty(poster)
                ? string.Empty
                : $"![poster]({poster})";
    }
}
