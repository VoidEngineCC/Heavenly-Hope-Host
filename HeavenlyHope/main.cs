using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IniParser;
using IniParser.Model;
using ProcessUtilities;

namespace HeavenlyHopeLauncher
{
    internal class App
    {
        static String applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static Process gameClient = new Process();
        static Process antiCheatClient = new Process();
        static Process gameLauncher = new Process();
        static Dictionary<string, (string Directory, string User, string Pass)> seasonData = new Dictionary<string, (string, string, string)>();
        static string selectedLanguage = "en";
        static String rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

        static Dictionary<string, Dictionary<string, string>> localizedMessages = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "en", new Dictionary<string, string>
                {
                    { "Welcome", "Welcome to HeavenlyHope BattleRoyale" },
                    { "AddVersion", "[0] Add a Fortnite version" },
                    { "LaunchVersion", "[1] Launch a Fortnite version" },
                    { "RemoveVersion", "[2] Remove a Fortnite version" },
                    { "SelectAction", "Please respond only with 0, 1, or 2" },
                    { "AddPath", "Please enter the path to your version:" },
                    { "AddName", "Please provide a name for this season:" },
                    { "AddEmail", "What email would you like to use?" },
                    { "AddPassword", "What password would you like to use?" },
                    { "SaveConfirmation", "Your path, username, and password have been saved successfully!" },
                    { "NoSeasons", "No seasons have been added." },
                    { "AvailableSeasons", "Available seasons:" },
                    { "SelectSeason", "Please select the season you want to launch by name:" },
                    { "Launching", "Launching season:" },
                    { "FileNotFound", "The file is not found:" },
                    { "GameFinished", "The game has ended. Restarting..." },
                    { "RemoveSeasonPrompt", "Please select the season you want to remove by name:" },
                    { "RemoveSuccess", "The season has been successfully removed." },
                    { "SeasonNotFound", "Season not found." },
                    { "InjectionFailed", "Injection failed:" },
                    { "DllMoveSuccess", "DLL successfully moved and renamed to the target path." },
                    { "DllMoveFailed", "Failed to move and rename the DLL to the target path." },
                    { "RedirectDllInjection", "The redirect DLL has been injected into the game." },
                    { "RedirectDllInjectionFailed", "The redirect DLL is missing." },
                    { "GameserverDllInjection", "The game server DLL has been injected into the game." },
                    { "GameserverDllInjectionFailed", "The game server DLL is missing." },
                    { "Restart", "The game is finished, restarting..." }
                }
            }
        };

        public static string GetLocalizedMessage(string key)
        {
            return localizedMessages[selectedLanguage].ContainsKey(key) ? localizedMessages[selectedLanguage][key] : key;
        }

        public static void TerminateProcess(string processName)
        {
            try
            {
                Process[] runningProcesses = Process.GetProcessesByName(processName);
                foreach (var process in runningProcesses)
                {
                    process.Kill();
                }
            }
            catch
            {
            }
        }

        // Helper function to format the current time
        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        static void Main(string[] args)
        {
            Console.Title = "HeavenlyHope Battle Royale";
            LoadSeasons();

            while (true)
            {
                DisplayMainMenu();
                string selection = Console.ReadLine();

                if (selection == "0")
                {
                    AddNewSeason();
                }
                else if (selection == "1")
                {
                    StartSeason();
                }
                else if (selection == "2")
                {
                    DeleteSeason();
                }
                else
                {
                    Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("SelectAction")}");
                    Thread.Sleep(2000);
                    Console.Clear();
                }
            }
        }

        static void DisplayMainMenu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("Welcome")}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AddVersion")}");
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("LaunchVersion")}");
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("RemoveVersion")}");
            Console.Write(">> ");
        }

        static void AddNewSeason()
        {
            Console.Clear();
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AddPath")}");
            Console.Write(">> ");
            string directory = Console.ReadLine();

            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AddName")}");
            Console.Write(">> ");
            string name = Console.ReadLine();

            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AddEmail")}");
            Console.Write(">> ");
            string userEmail = Console.ReadLine();

            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AddPassword")}");
            Console.Write(">> ");
            string userPassword = Console.ReadLine();

            seasonData[name] = (directory, userEmail, userPassword);
            SaveSeasonData();

            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("SaveConfirmation")}");
            Console.Clear();
        }

        static void StartSeason()
        {
            if (seasonData.Count == 0)
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("NoSeasons")}");
                return;
            }

            Console.Clear();
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AvailableSeasons")}");
            foreach (var season in seasonData.Keys)
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> - {season}");
            }

            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("SelectSeason")}");
            Console.Write(">> ");
            string selectedSeason = Console.ReadLine();

            if (seasonData.ContainsKey(selectedSeason))
            {
                Console.Clear();
                var season = seasonData[selectedSeason];
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("Launching")} {selectedSeason}");

                while (true)
                {
                    LaunchGameInstance(season);
                    Thread.Sleep(2000);
                }
            }
            else
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("SeasonNotFound")}");
            }

            Console.Clear();
            DisplayMainMenu();
        }

        static void LaunchGameInstance((string Directory, string User, string Pass) season)
        {
            try
            {
                string launcherPath = Path.Combine(season.Directory, "FortniteGame", "Binaries", "Win64", "FortniteLauncher.exe");
                if (!File.Exists(launcherPath))
                {
                    Console.WriteLine($"[{GetCurrentTime()}] >> The file {launcherPath} is not found.");
                    return;
                }

                gameLauncher.StartInfo.FileName = launcherPath;
                gameLauncher.Start();
                ThreadManager.PauseThread(gameLauncher);

                string eacPath = Path.Combine(season.Directory, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping_EAC.exe");
                if (!File.Exists(eacPath))
                {
                    Console.WriteLine($"[{GetCurrentTime()}] >> The file {eacPath} is not found.");
                    return;
                }

                antiCheatClient.StartInfo.FileName = eacPath;
                antiCheatClient.Start();
                ThreadManager.PauseThread(antiCheatClient);

                // Inject redirect DLL
                string dllDirectory = Path.Combine(season.Directory, "FortniteGame", "Binaries", "Win64");
                MemoryOperations.InjectLibrary(gameClient.Id, Path.Combine(dllDirectory, "redirect.dll")); // Injection of redirect.dll

                // Inject game server DLL
                MemoryOperations.InjectLibrary(gameClient.Id, Path.Combine(dllDirectory, "gameserver.dll")); // Injection of gameserver.dll

                Process gameClientProcess = new Process();
                gameClientProcess.StartInfo.FileName = Path.Combine(season.Directory, "FortniteGame/Binaries/Win64/FortniteClient-Win64-Shipping.exe");
                gameClientProcess.StartInfo.RedirectStandardOutput = true;
                gameClientProcess.StartInfo.UseShellExecute = false;
                gameClientProcess.StartInfo.Arguments = $@"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -user={season.User} -pass={season.Pass}";
                gameClientProcess.Start();

                while (!gameClientProcess.HasExited)
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("GameFinished")}");
                TerminateProcess("FortniteClient-Win64-Shipping");
                TerminateProcess("FortniteClient-Win64-Shipping_EAC");
                TerminateProcess("FortniteLauncher");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("InjectionFailed")}: {ex.Message}");
            }
        }

        static void DeleteSeason()
        {
            if (seasonData.Count == 0)
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("NoSeasons")}");
                return;
            }

            Console.Clear();
            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("AvailableSeasons")}");
            foreach (var season in seasonData.Keys)
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> - {season}");
            }

            Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("RemoveSeasonPrompt")}");
            Console.Write(">> ");
            string selectedSeasonToRemove = Console.ReadLine();

            if (seasonData.ContainsKey(selectedSeasonToRemove))
            {
                seasonData.Remove(selectedSeasonToRemove);
                SaveSeasonData();
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("RemoveSuccess")}");
            }
            else
            {
                Console.WriteLine($"[{GetCurrentTime()}] >> {GetLocalizedMessage("SeasonNotFound")}");
            }

            Console.Clear();
            DisplayMainMenu();
        }

        static void SaveSeasonData()
        {
            var parser = new FileIniDataParser();
            IniData data = new IniData();
            foreach (var season in seasonData)
            {
                data[season.Key]["Directory"] = season.Value.Directory;
                data[season.Key]["Email"] = season.Value.User;
                data[season.Key]["Password"] = season.Value.Pass;
            }

            parser.WriteFile("seasons.ini", data);
        }

        static void LoadSeasons()
        {
            if (!File.Exists("seasons.ini"))
            {
                return;
            }

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("seasons.ini");
            foreach (var section in data.Sections)
            {
                string seasonName = section.SectionName;
                string directory = section.Keys["Directory"];
                string user = section.Keys["Email"];
                string password = section.Keys["Password"];
                seasonData[seasonName] = (directory, user, password);
            }
        }
    }
}
