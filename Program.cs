using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnimeSorter
{
    class Program
    {
        //public static string AnimeFolder = Directory.GetCurrentDirectory();
        public static string AnimeFolder = @"C:\Users\770688\Specials\Test Space\Anime";
        public static string DownloadedFolder = AnimeFolder.Remove(AnimeFolder.LastIndexOf('\\'));

        static void Main(string[] args)
        {
            Console.WriteLine("Moving Episodes...");
            MoveEpisodes();
            Console.WriteLine("Moving Folders...");
            MoveFolders();
            Console.WriteLine("Creating new folders...");
            CreateNewAnimeFolders();
            Console.WriteLine("Moving Episodes to Folders...");
            MoveEpisodesToFolders();
            Console.WriteLine("Renaming Episodes...");
            RenameEpisodes();
            Console.WriteLine("Ending...");
        }

        static void MoveEpisodes()
        {
            var listEpisodes = Directory.EnumerateFiles(DownloadedFolder).ToList();
            var regex = new Regex("[0-9]");

            foreach (var episode in listEpisodes)
            {
                var episodeName = episode.Substring(episode.LastIndexOf('\\') + 1);
                if (regex.IsMatch(episodeName) && episodeName.Contains("[") && (episodeName.Contains(".mkv") || episodeName.Contains(".avi") || episodeName.Contains(".mp4")))
                {
                    Console.WriteLine("Moving " + episodeName.Remove(episodeName.LastIndexOf('.')) + " to the Anime folder");
                    Directory.Move(DownloadedFolder + "\\" + episodeName, AnimeFolder + "\\" + episodeName);
                }
            }
        }

        static void MoveFolders()
        {
            var listFolders = Directory.EnumerateDirectories(DownloadedFolder).ToList();

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
                        Directory.Move(DownloadedFolder + "\\" + folderName, AnimeFolder + "\\" + folderNewName);
                    }
                }
            }
        }

        static void CreateNewAnimeFolders()
        {
            var listEpisodes = Directory.EnumerateFiles(AnimeFolder).ToList();

            foreach (var episode in listEpisodes)
            {
                var episodeName = episode.Substring(episode.LastIndexOf('\\') + 1);

                if (!episode.Contains(".mkv") && !episode.Contains("mp4") && !episode.Contains("avi"))
                {
                    continue;
                }

                string animeName = getAnimeName(episodeName);
                if (animeName == "")
                {
                    continue;
                }

                string folderName = hasFolder(animeName);

                if (folderName == "")
                {
                    folderName = animeName;
                    Console.WriteLine("Creating " + folderName + " folder...");
                    Directory.CreateDirectory(AnimeFolder + "\\" + folderName);
                }
            }
        }

        static void MoveEpisodesToFolders()
        {
            var listEpisodes = Directory.EnumerateFiles(AnimeFolder).ToList();

            foreach (var episode in listEpisodes)
            {
                var episodeName = episode.Substring(episode.LastIndexOf('\\') + 1);

                if (!episode.Contains(".mkv") && !episode.Contains("mp4") && !episode.Contains("avi"))
                {
                    continue;
                }

                Console.WriteLine("Moving " + episodeName + " to " + getAnimeName(episodeName) + "...");
                Directory.Move(AnimeFolder + "\\" + episodeName, AnimeFolder + "\\" + getAnimeName(episodeName) + "\\" + episodeName);
            }
        }

        static void RenameEpisodes()
        {
            var listFolders = Directory.EnumerateDirectories(AnimeFolder).ToList();

            foreach (var folder in listFolders)
            {
                var listEpisodes = Directory.EnumerateFiles(folder).ToList();

                foreach (var episode in listEpisodes)
                {
                    var episodeName = episode.Substring(episode.LastIndexOf('\\') + 1);

                    if (!episode.Contains(".mkv") && !episode.Contains("mp4") && !episode.Contains("avi"))
                    {
                        continue;
                    }

                    string episodeNewName = Directory.GetParent(episode).FullName;
                    episodeNewName = episodeNewName.Substring(episodeNewName.LastIndexOf("\\") + 1);
                    episodeNewName += " " + getEpisodeNumber(episodeName);
                    episodeNewName += episodeName.Substring(episodeName.LastIndexOf("."));

                    if (episodeName == episodeNewName)
                    {
                        continue;
                    }

                    Console.WriteLine("Renaming " + episodeName + " to " + episodeNewName + "...");

                    if (File.Exists(folder + "\\" + episodeNewName))
                    {
                        episodeNewName = DateTime.Now.Ticks + "-" + episodeNewName;
                    }

                    Directory.Move(folder + "\\" + episodeName, folder + "\\" + episodeNewName);
                }
            }
        }

        static string hasFolder(string animeName)
        {
            var listFolders = Directory.EnumerateDirectories(AnimeFolder).ToList();

            foreach (var folder in listFolders)
            {
                var folderName = folder.Substring(folder.LastIndexOf('\\') + 1);
                if (animeName == folderName)
                {
                    return folderName;
                }
            }

            return "";
        }

        static string getEpisodeNumber(string episodeName)
        {
            var numbers = new List<string>();
            var start = -1;
            for (int i = 0; i < episodeName.Length; i++)
            {
                if (start < 0 && Char.IsDigit(episodeName[i]))
                {
                    start = i;
                }
                else if (start >= 0 && !Char.IsDigit(episodeName[i]))
                {
                    numbers.Add(episodeName.Substring(start, i - start));
                    start = -1;
                }
            }
            if (start >= 0)
                numbers.Add(episodeName.Substring(start, episodeName.Length - start));
            foreach (var number in numbers)
            {
                if (number.StartsWith("0"))
                {
                    return number;
                }
            }
            return "";
        }

        static string getAnimeName(string episodeName)
        {
            Regex regex = null;

            if (episodeName.Contains("[HorribleSubs]"))
            {
                regex = new Regex(@"(\[HorribleSubs\] )(.*)( -.*)");
                return regex.Match(episodeName).Groups[2].ToString();
            }

            if (episodeName.Contains("[DeadFish]"))
            {
                regex = new Regex(@"(\[DeadFish\] )(.*)( -.*)");
                return regex.Match(episodeName).Groups[2].ToString();
            }

            Console.WriteLine("Episode from different source, update regex info.");
            Console.WriteLine(episodeName);
            return "";
        }
    }
}
