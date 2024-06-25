using ButlerCore.Helpers;
using ButlerCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ButlerCore.Jobs
{
    public class TvJobMaster
    {
        private readonly string _tvMarkdownFolder;
        private readonly string _tvRootFolder;
#if !DEBUG
        private readonly ILogger _logger;
#endif
        public TvJobMaster(
            ILogger logger,
            string dropBoxFolder,
            string tvRootFolder = "t:\\")
        {
            _tvMarkdownFolder = $"{dropBoxFolder}Obsidian\\ChestOfNotes\\tv\\";
            _tvRootFolder = tvRootFolder;
#if !DEBUG
            _logger = logger;
#endif
        }

        public int DoDetectorJob()
        {
            var list = GetTvList();
            LogIt($"There are {list.Count} TV shows");
            foreach (var Tv in list)
            {
                if (IsMarkdownFor(Tv.Title))
                    continue;

                WriteTvMarkdown(Tv);
                LogIt($"Markdown created for {Tv}");
                Console.WriteLine($"Markdown created for {Tv}");
            }
            return 0;
        }

        public int DoCullJob()
        {
            var cullCount = 0;
            var keeperCount = 0;
            var unwatchedCount = 0;
            var showCount = 0;
            var fileEntries = Directory.GetDirectories(
                _tvRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                var Tv = ParseTv(fileInfo.Name);
                showCount++;
                if (IsKeeper(Tv))
                {
                    keeperCount++;
                    continue;
                }
                if (!Watched(Tv))
                {
                    unwatchedCount++;
                    continue;
                }
                // remove it
#if !DEBUG
                FileSystemHelper.DeleteDirectory(
                    fileInfo.FullName);
                LogIt($"Deleted {Tv}");
#else
                LogIt($"{file} would be deleted");
#endif
                cullCount++;
            }
            LogIt($"{showCount} shows");
            LogIt($"{keeperCount} keepers");
            LogIt($"{unwatchedCount} unwatched");
            LogIt($"{cullCount} shows culled");
            return 0;
        }

        public List<Tv> GetTvList()
        {
            var list = new List<Tv>();
            var fileEntries = Directory.GetDirectories(
                _tvRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                list.Add(
                    ParseTv(fileInfo.Name));
            }
            return list;
        }

        public bool IsMarkdownFor(
            string moveTitle)
        {
            var mdFile = MarkdownFileName(moveTitle);
            return File.Exists(mdFile);
        }

        public static string? TvToMarkdown(Tv Tv)
        {
            var theWhen = DateTime.Now.ToString("yyyy-MM-dd");
            var sb = new StringBuilder()
                .AppendLine("---")
                .AppendLine("tags: [tv/planning]")
                .AppendLine("Priority: 5")
                .AppendLine($"when: {theWhen}")
                .AppendLine("genre:")
                .AppendLine("rating:")
                .AppendLine($"Year: {Tv.Year}")
                .AppendLine("Completion:")
                .AppendLine("Keeper:")
                .AppendLine("How: Plex")
                .AppendLine("With:")
                .AppendLine("---")
                .AppendLine()
                .AppendLine($"# {Tv.Title}");

            return sb.ToString();
        }

        public static Tv ParseTv(string foldername)
        {
            var Tv = new Tv();
            var match = Regex.Match(
                input: foldername,
                pattern: @"(.*)\s[\[\(](\d{4})[\]\)]");

            if (match.Success)
            {
                Tv.Title = match.Groups[1].Value.Trim();
                Tv.Year = match.Groups[2].Value;
            }
            else
                Tv.Title = foldername;
            return Tv;
        }

        public bool WriteTvMarkdown(Tv Tv)
        {
            var text = TvToMarkdown(Tv);
            var fileName = MarkdownFileName(Tv.Title);
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
            string TvTitle)
        {
            return $"{_tvMarkdownFolder}{TvTitle}.md";
        }

        public string? TvProperty(
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
            if (props.Exists(p => p.Name == "Keeper" && p.Value == "Y"))
                return true;
            return false;
        }

        public bool IsKeeper(Tv Tv) => IsKeeper(Tv.Title);

        public List<Tv> CullList()
        {
            var list = new List<Tv>();
            var fileEntries = Directory.GetDirectories(
                _tvRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                var Tv = ParseTv(fileInfo.Name);
                if (IsKeeper(Tv))
                    continue;
                if (!Watched(Tv))
                    continue;
                list.Add(Tv);
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

        public bool Watched(Tv Tv) =>

            ReadTags(Tv.Title)
                .Exists(t => t.Value == "tv/done");

        public List<Tv> UnprocessedFiles()
        {
            var unprocessedCount = 0;
            var list = new List<Tv>();
            var fileEntries = Directory.GetDirectories(
                _tvRootFolder,
                "*.*");
            foreach (var file in fileEntries)
            {
                var fileInfo = new FileInfo(file);
                var Tv = ParseTv(fileInfo.Name);
                var props = ReadProperties(Tv.Title);
                if (props.Count == 0)
                {
                    list.Add(Tv);
                    unprocessedCount++;
                }
            }
            LogIt($"{unprocessedCount} Tv Shows unprocessed");
            return list;
        }
    }
}
