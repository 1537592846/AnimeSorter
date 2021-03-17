using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnimeSorter
{
    class Program
    {
        public static string AnimeFolder = Directory.GetCurrentDirectory();
        public static string DownloadedFolder = AnimeFolder.Remove(AnimeFolder.LastIndexOf('\\'));
        public static List<string> RegexConfigs = new List<string>();

        static void Main()
        {
            if (Debugger.IsAttached)
                AnimeFolder = @"C:\Users\andre\Downloads\Downloaded\Animes";

            try
            {
                Console.WriteLine("Getting regex configs...");
                GetRegexConfigs();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting regex configs");
                Console.WriteLine(e.Message);
                Console.Read();
            }

            try
            {
                Console.WriteLine("Moving Episodes...");
                MoveEpisodes();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error moving episodes");
                Console.WriteLine(e.Message);
                Console.Read();
            }

            try
            {
                Console.WriteLine("Moving Folders...");
                MoveFolders();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error moving folders");
                Console.WriteLine(e.Message);
                Console.Read();
            }

            try
            {
                Console.WriteLine("Moving Episodes to Folders...");
                MoveEpisodesToFolders();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error moving episodes to folders");
                Console.WriteLine(e.Message);
                Console.Read();
            }

            try
            {
                Console.WriteLine("Renaming Episodes...");
                RenameEpisodes();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error renaming episodes");
                Console.WriteLine(e.Message);
                Console.Read();
            }
            Console.WriteLine("Ending...");
        }

        static void GetRegexConfigs()
        {
            var regexConfigPath= Path.Combine(AnimeFolder, "regexConfig.animeSorter");
            if (!File.Exists(regexConfigPath))
            {
                var regexFile=File.Create(regexConfigPath);
                var regexStreamWriter = new StreamWriter(regexFile);
                regexStreamWriter.WriteLine(@"([\s\S]*) (\d*)\.([\s\S]*)");
                regexStreamWriter.Close();
                regexFile.Close();
            }

            var regexStreamReader = new StreamReader(regexConfigPath);
            var regexConfig = regexStreamReader.ReadLine();
            while (!string.IsNullOrEmpty(regexConfig))
            {
                RegexConfigs.Add(regexConfig);
                regexConfig = regexStreamReader.ReadLine();
            }
        }

        static void MoveEpisodes()
        {
            var listEpisodes = Directory.EnumerateFiles(DownloadedFolder).Where(x=>!x.Contains(".animeSorter")&&!x.Contains(".exe")).ToList();

            foreach (var episode in listEpisodes)
            {
                var file = episode.Substring(episode.LastIndexOf("\\") + 1);
                var reg = getRegex(file);
                if (reg == null)
                    continue;
                var originalName = reg.Match(file).Groups[0].ToString();
                var animeName = reg.Match(file).Groups[1].ToString();
                var episodeNumber = reg.Match(file).Groups[2].ToString();
                var format = reg.Match(file).Groups[3].ToString();

                if (originalName.Contains("[") && (format == "mkv" && format == "avi" && format == "mp4"))
                {
                    Console.WriteLine("Moving " + originalName + " to the Anime folder");
                    MoveFile(DownloadedFolder + "\\" + originalName, AnimeFolder + "\\" + animeName + "\\" + animeName + " " + episodeNumber + "." + format);
                }
            }
        }

        static void MoveFolders()
        {
            var listFolders = Directory.EnumerateDirectories(DownloadedFolder).Where(x=>!x.Contains(".animeSorter")&&!x.Contains(".exe")).ToList();

            foreach (var folder in listFolders)
            {
                var folderName = folder.Substring(folder.LastIndexOf('\\') + 1);

                if (folderName.Contains("["))
                {
                    string entry;
                    Console.WriteLine("Is " + folderName + " an anime folder? [Y,N]: ");

                    do
                    {
                        entry = Console.ReadKey().Key.ToString();
                        Console.WriteLine();
                    } while (entry != "Y" && entry != "N");

                    if (entry == "Y")
                    {
                        Console.WriteLine("What is the name of this anime?: ");
                        var folderNewName = Console.ReadLine();
                        Console.WriteLine("Moving " + folderName + " to the Anime folder");
                        MoveFolder(DownloadedFolder + "\\" + folderName, AnimeFolder + "\\" + folderNewName);
                    }
                }
            }
        }

        static void MoveEpisodesToFolders()
        {
            var listEpisodes = Directory.EnumerateFiles(AnimeFolder).Where(x=>!x.Contains(".animeSorter")&&!x.Contains(".exe")).ToList();

            foreach (var episode in listEpisodes)
            {
                try
                {
                    var file = episode.Substring(episode.LastIndexOf("\\") + 1);
                    var reg = getRegex(file);
                    if (reg == null)
                        continue;
                    var originalName = reg.Match(file).Groups[0].ToString();
                    var animeName = reg.Match(file).Groups[1].ToString();
                    var episodeNumber = reg.Match(file).Groups[2].ToString();
                    var format = reg.Match(file).Groups[3].ToString();

                    if (format != "mkv" && format != "avi" && format != "mp4")
                    {
                        continue;
                    }

                    Console.WriteLine("Moving " + originalName + " to " + animeName + "...");
                    MoveFile(AnimeFolder + "\\" + originalName, AnimeFolder + "\\" + animeName + "\\" + animeName + " " + episodeNumber + "." + format);
                }
                catch { }
            }
        }

        static void RenameEpisodes()
        {
            var listFolders = Directory.EnumerateDirectories(AnimeFolder).Where(x=>!x.Contains(".animeSorter")&&!x.Contains(".exe")).ToList();

            foreach (var folder in listFolders)
            {
                var listEpisodes = Directory.EnumerateFiles(folder).Where(x=>!x.Contains(".animeSorter")&&!x.Contains(".exe")).ToList();

                foreach (var episode in listEpisodes)
                {
                    var file = episode.Substring(episode.LastIndexOf("\\") + 1);
                    var reg = getRegex(file);
                    if (reg == null)
                        continue;
                    var originalName = reg.Match(file).Groups[0].ToString();
                    var animeName = reg.Match(file).Groups[1].ToString();
                    var episodeNumber = reg.Match(file).Groups[2].ToString();
                    var format = reg.Match(file).Groups[3].ToString();
                    var episodeNewName = animeName + " " + episodeNumber + "." + format;

                    if (format != "mkv" && format != "avi" && format != "mp4")
                    {
                        continue;
                    }

                    Console.WriteLine("Renaming " + originalName + " to " + episodeNewName + "...");
                    MoveFile(folder + "\\" + originalName, folder + "\\" + episodeNewName);
                }
            }
        }

        static Regex getRegex(string episodeName)
        {
            foreach (var regexInfo in RegexConfigs)
            {
                var reg = new Regex(regexInfo.Replace("\\\\","\\"));
                if (reg.IsMatch(episodeName))
                {
                    return reg;
                }
            }

            Console.WriteLine("Episode from different source, update regex info.");
            Console.WriteLine(episodeName);
            Console.Read();
            return null;
        }

        static void MoveFile(string file, string newFile)
        {
            try
            {
                if (Directory.Exists(newFile.Remove(file.LastIndexOf("\\"))))
                {
                    Directory.CreateDirectory(newFile.Remove(newFile.LastIndexOf("\\")));
                }
                Directory.Move(file, newFile);
            }
            catch (Exception ioe)
            {
                if (!ioe.Message.Contains("existe"))
                {
                    return;
                }

                var fileToMove = new DirectoryInfo(file);
                var fileOnFolder = new DirectoryInfo(newFile);
                if (!DirectoryInfo.Equals(fileToMove, fileOnFolder))
                {
                    if (fileToMove.CreationTime > fileOnFolder.CreationTime)
                    {
                        File.Delete(newFile);
                        MoveFile(file, newFile);
                    }
                    else
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        static void MoveFolder(string folder, string newFolder)
        {
            try
            {
                Directory.Move(folder, newFolder);
            }
            catch
            { }
        }
    }
}
