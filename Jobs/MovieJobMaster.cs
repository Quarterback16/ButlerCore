using ButlerCore.Helpers;
using ButlerCore.Models;
using Microsoft.Extensions.Logging;
using MovieService;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ButlerCore.Jobs
{
    public class MovieJobMaster
    {
        private readonly string _movieMarkdownFolder;
        private readonly string _movieRootFolder;
#if !DEBUG
        private readonly ILogger _logger;
#endif
        public MovieJobMaster(
            ILogger logger,
            string dropBoxFolder,
            string movieRootFolder = "m:\\")
        {
            _movieMarkdownFolder = $"{dropBoxFolder}Obsidian\\ChestOfNotes\\movies\\";
            _movieRootFolder = movieRootFolder;
#if !DEBUG
            _logger = logger;
#endif
        }

        public int DoDetectorJob(
            IMovieService movieService)
        {
            var newMovies = 0;
            var list = GetMovieList();
            foreach (var movie in list) 
            {
                if (IsMarkdownFor(movie.Title))
                    continue;

                WriteMovieMarkdown(movie,movieService);
                LogIt($"Markdown created for {movie}");
                newMovies++;
            }
            LogIt($"{newMovies} new movies detected");
            return 0;
        }

        public int DoCullJob()
        {
            var fileEntries = Directory.GetDirectories(
                _movieRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                var movie = ParseMovie(fileInfo.Name);
                if (IsKeeper(movie))
                    continue;
                if (!Watched(movie))
                    continue;
                // remove it
                FileSystemHelper.DeleteDirectory(
                    fileInfo.FullName);
                LogIt($"Deleted {movie}");
            }
            return 0;
        }

        public List<Movie> GetMovieList()
        {
            var list = new List<Movie>();
            var fileEntries = Directory.GetDirectories(
                _movieRootFolder,
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
            var mdFile = MarkdownFileName(moveTitle);
            return File.Exists(mdFile);
        }

        public static string? MovieToMarkdown(
            Movie movie,
            IMovieService movieService)
        {
            MovieService.Models.Movie apiData = null;

            try
            {
                if (string.IsNullOrEmpty(movie.Year))
                {
                    apiData = movieService.GetMovie(movie.Title);
                }
                else
                {
                    apiData = movieService.GetMovie(movie.Title,movie.Year);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting {movie} {ex.Message}");
            }
            var theWhen = DateTime.Now.ToString("yyyy-MM-dd");
            var sb = new StringBuilder()
                .AppendLine("---")
                .AppendLine("tags: [movie/planning]")
                .AppendLine("Priority: 5")
                .AppendLine($"when: {theWhen}")
                .AppendLine($"genre: {Genre(apiData)}")
                .AppendLine($"actors: {Actors(apiData)}")
                .AppendLine("rating:")
                .AppendLine($"Year: {movie.Year}")
                .AppendLine("Completion:")
                .AppendLine("Keeper:")
                .AppendLine("How: Plex")
                .AppendLine("With:")
                .AppendLine($"Poster: {apiData.Poster}")
                .AppendLine("q-type: movie")
                .AppendLine("---")
                .AppendLine()
                .AppendLine($"# {movie.Title}")
                .AppendLine()
                .AppendLine(Plot(apiData))
                .AppendLine()
                .AppendLine(EmbedPoster(apiData.Poster));

            return sb.ToString();
        }

        private static string Actors(
            MovieService.Models.Movie apiData)
        {
            if (apiData == null)
                return string.Empty;

            return string.IsNullOrEmpty(apiData.Actors)
                ? string.Empty
                : apiData.Actors;
        }

        private static string Genre(
            MovieService.Models.Movie apiData)
        {
            if (apiData == null)
                return string.Empty;

            return string.IsNullOrEmpty(apiData.Genre)
                ? string.Empty
                : apiData.Genre;
        }

        private static string Plot(
            MovieService.Models.Movie apiData)
        {
            if (apiData == null)
                return string.Empty;

            return string.IsNullOrEmpty(apiData.Plot)
                ? string.Empty
                : apiData.Plot;
        }

        private static string? EmbedPoster(string poster) =>
        
            string.IsNullOrEmpty(poster)
                ? string.Empty
                : $"![poster]({poster})";
       

        public static Movie ParseMovie(string foldername)
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

        public bool WriteMovieMarkdown(
            Movie movie,
            IMovieService movieService)
        {
            var text = MovieToMarkdown(movie,movieService);
            var fileName = MarkdownFileName(movie.Title);
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

        public string MarkdownFileName(
            string movieTitle)
        {
            return $"{_movieMarkdownFolder}{movieTitle}.md";
        }

        public string? MovieProperty(
            string title,
            string propertyName)
        {
            if (IsMarkdownFor(title))
            {
                var props = ReadProperties(title);
                var prop = props.Find(p => p.Name == propertyName);

                return prop == null ? "?" : prop.Value;
            }
            else
            {
                return "?";
            }
        }

        public List<MediaProperty> ReadProperties(string title)
        {
            var props = new List<MediaProperty>();
            var fileName = MarkdownFileName(title);
            if (!File.Exists(fileName))
                return props;  // hasnt been processed yet
            string[] lines = File.ReadAllLines(fileName);
            var startProps = false;
            foreach (var line in lines)
            {
                if (startProps)
                {
                    var prop = LineToProp(line);
                    if (prop.Name != null)
                    {
                        props.Add(prop);
                    }
                }
                if (line.StartsWith("---") && !startProps)
                {
                    startProps = true;
                    continue;
                }
                if (line.StartsWith("---") && startProps)
                {
                    break;
                }
            }
            // add defaults 
            if (!props.Exists(p => p.Name == "Keeper"))
                props.Add(new MediaProperty("Keeper", "Keeper: N"));
            return props;
        }

        public List<MediaTag> ReadTags(string title)
        {
            var tags = new List<MediaTag>();
            var fileName = MarkdownFileName(title);
            string[] lines = File.ReadAllLines(fileName);
            var startTags = false;
            foreach (var line in lines)
            {
                if (startTags)
                {
                    var tag = LineToTag(line);
                    if (tag.Value != null)
                    {
                        tags.Add(tag);
                    }
                }
                if (line.StartsWith("tags:") && !startTags)
                {
                    if (line.Contains("[") && line.Contains("]"))
                    {
                        var tagString = line.Substring(
                            line.IndexOf("[") + 1, 
                            line.IndexOf("]") - line.IndexOf("[") - 1);
                        var tagArray = tagString.Split(',');
                        foreach (var item in tagArray)
                        {
                            tags.Add(new MediaTag(item));
                        }
                        break;
                    }
                    startTags = true;
                    continue;
                }
                if (line.StartsWith("---") && startTags)
                {
                    break;
                }
            }
            return tags;
        }
        private MediaTag LineToTag(string line)
        {
            if (line.StartsWith("  - "))
                return new MediaTag(line.Substring(4));
            return new MediaTag();
        }

        private static MediaProperty LineToProp(string line)
        {
            var kp = MediaHelper.KnownProperties()
                .Find(kp => !string.IsNullOrEmpty(kp.Name) && line.StartsWith(kp.Name));
            if (kp != null && !string.IsNullOrEmpty(kp.Name))
                return new MediaProperty(kp.Name, line);
            return new MediaProperty();
        }

        public bool IsKeeper(string title)
        {
            var props = ReadProperties(title);
            if (props.Exists(p=>p.Name == "Keeper" && p.Value == "Y"))
                return true;
            return false;
        }

        public bool IsKeeper(Movie movie) => IsKeeper(movie.Title);

        public List<Movie> CullList()
        {
            var list = new List<Movie>();
            var fileEntries = Directory.GetDirectories(
                _movieRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                var movie = ParseMovie(fileInfo.Name);
                if (IsKeeper(movie))
                    continue;
                if (!Watched(movie))
                    continue;
                list.Add(movie);
            }
            return list;
        }

        private void LogIt(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#else
            _logger.LogInformation(msg);
#endif
        }

        public bool Watched(Movie movie) =>

            ReadTags(movie.Title)
                .Exists( t => t.Value == "movie/done");

        public List<Movie> UnprocessedFiles()
        {
            var list = new List<Movie>();
            var fileEntries = Directory.GetDirectories(
                _movieRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                var movie = ParseMovie(fileInfo.Name);
                var props = ReadProperties(movie.Title);
                if (props.Count == 0)
                    list.Add(movie);
            }
            return list;
        }
    }
}
