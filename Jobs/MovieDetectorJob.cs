﻿using ButlerCore.Helpers;
using ButlerCore.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

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
                Console.WriteLine($"Markdown created for {movie}");
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

        public List<MovieProperty> ReadProperties(string title)
        {
            var props = new List<MovieProperty>();
            var fileName = MarkdownFile(title);
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
                props.Add(new MovieProperty("Keeper", "Keeper: N"));
            return props;
        }

        public List<MovieTag> ReadTags(string title)
        {
            var tags = new List<MovieTag>();
            var fileName = MarkdownFile(title);
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
                            tags.Add(new MovieTag(item));
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
        private MovieTag LineToTag(string line)
        {
            if (line.StartsWith("  - "))
                return new MovieTag(line.Substring(4));
            return new MovieTag();
        }

        private static MovieProperty LineToProp(string line)
        {
            var kp = MovieHelper.KnownProperties()
                .Find(kp => !string.IsNullOrEmpty(kp.Name) && line.StartsWith(kp.Name));
            if (kp != null && !string.IsNullOrEmpty(kp.Name))
                return new MovieProperty(kp.Name, line);
            return new MovieProperty();
        }

        public bool IsKeeper(string title)
        {
            var props = ReadProperties(title);
            if (props.Exists(p=>p.Name == "Keeper" && p.Value == "Y"))
                return true;
            return false;
        }

        public bool IsKeeper(Movie movie) => IsKeeper(movie.Title);

        public List<Movie> CullList(
            string movieRootFolder = "m:\\")
        {
            var list = new List<Movie>();
            var fileEntries = Directory.GetDirectories(
                movieRootFolder,
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

        public bool Watched(Movie movie) =>

            ReadTags(movie.Title)
                .Exists( t => t.Value == "movie/done");

        public List<Movie> UnprocessedFiles(
            string movieRootFolder = "m:\\")
        {
            var list = new List<Movie>();
            var fileEntries = Directory.GetDirectories(
                movieRootFolder,
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
