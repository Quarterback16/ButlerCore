using ButlerCore.Models;
using LanguageExt.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace ButlerCore.Jobs
{
    public class MovieDetectorJob
    {
        private readonly string _movieMarkdownFolder;

        public MovieDetectorJob()
        {
            _movieMarkdownFolder = "d:\\Dropbox\\Obsidian\\ChestOfNotes\\movies\\";
        }

        public int DoJob()
        {
            var list = GetMovieList();
            foreach (var movie in list) 
            {
                if (IsMarkdownFor(movie.Title))
                    continue;

                WriteMovieMarkdown(movie);
            }
            return 0;
        }

        public List<Movie> GetMovieList(
            string movieRootFolder = "m:\\")
        {
            var list = new List<Movie>();
            var fileEntries = Directory.GetDirectories(
                movieRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                list.Add(
                    ParseMovie(fileInfo.Name));
            }
            return list;
        }

        public bool IsMarkdownFor(
            string moveTitle) 
        {
            var mdFile = MarkdownFile(moveTitle);
            return File.Exists(mdFile);
        }

        public string? MovieToMarkdown(Movie movie)
        {
            var theWhen = DateTime.Now.ToString("yyyy-MM-dd");
            var sb = new StringBuilder()
                .AppendLine("---")
                .AppendLine("tags: [movie/planning]")
                .AppendLine("Priority: 5")
                .AppendLine($"when: {theWhen}")
                .AppendLine("genre:")
                .AppendLine("rating:")
                .AppendLine($"Year: {movie.Year}")
                .AppendLine("Completion:")
                .AppendLine("Keeper:")
                .AppendLine("How:")
                .AppendLine("With:")
                .AppendLine("---")
                .AppendLine()
                .AppendLine($"# {movie.Title}");

            return sb.ToString();
        }

        public Movie ParseMovie(string foldername)
        {
            var movie = new Movie();
            var match = Regex.Match(
                input: foldername,
                pattern: @"(.*)\s[\[\(](\d{4})[\]\)]");

            if (match.Success)
            {
                movie.Title = match.Groups[1].Value.Trim();
                movie.Year = match.Groups[2].Value;
            }
            else
                movie.Title = foldername;
            return movie;
        }

        public bool WriteMovieMarkdown(Movie movie)
        {
            var text = MovieToMarkdown(movie);
            var fileName = MarkdownFile(movie.Title);
            bool result;
            try
            {
                using (StreamWriter outputFile = new StreamWriter(
                    fileName))
                {
                    outputFile.WriteLine(text);
                }
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public string MarkdownFile(
            string movieTitle)
        {
            return $"{_movieMarkdownFolder}{movieTitle}.md";
        }
    }
}
