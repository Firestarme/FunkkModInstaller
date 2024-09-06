using FunkkModInstaller.Properties;
using FunkkModInstaller.Utilities;
using FunkkMI_Common.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FunkkModInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        //Constants
        public const string PACK_CFG_FILENAME = "PACK_MANIFEST.json";
        public const string ARCHIVE_EXTENSION = ".zip";
        public const int MAX_RECURSION = 20;

        //Root Paths
        public static readonly string ProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        public static readonly string ApplicationPath = Environment.CurrentDirectory;

        //Steam Info
        public static string TgtGameSteamName = "Valheim";
        public static string SteamAppId = "892970";
        public static string SteamLibPath = vPath.Combine(ProgramFilesPath, "Steam\\steamapps\\common");
        public static string DefaultSteamExecPath =  vPath.Combine(ProgramFilesPath, "Steam\\steam.exe");

        //Game Info
        public static string TgtGameExecutable = "valheim.exe";
        public static string DefaultTgtGamePath = vPath.Combine(SteamLibPath, TgtGameSteamName);

        //Installer Paths
        public static string InstallerDirectoryPath = ApplicationPath;
        public static string InstallerConfigFileName = "FModLoader_Persistant.json";
        public static string InstallerStagingDir => vPath.Combine(InstallerDirectoryPath, "Staging");
        public static string PackPath => vPath.Combine(InstallerDirectoryPath,"ModPacks");
        public static string EditorPackPath => vPath.Combine(InstallerDirectoryPath, "EditorModPacks");

        //Variable Paths
        public static string PersistantConfigFilePath => vPath.Combine(TgtGamePath,InstallerConfigFileName);
        public static string TgtGamePath => AppSettings.GameInstallPath != null ? AppSettings.GameInstallPath : DefaultTgtGamePath;
        public static string GameExecPath => Path.Combine(AppSettings.GameInstallPath, TgtGameExecutable);

        //App info
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static SimpleConsole Console = new SimpleConsole();
        static Settings AppSettings => Settings.Default;

        public App()
        {
            Console.PrintLn($"#### Starting Funkk Installer Version {Version} ####");

            InitializeComponent();
            SetDefaultPaths();
        }

        private void SetDefaultPaths()
        {
            if (!Directory.Exists(AppSettings.GameInstallPath))
            {
                AppSettings.GameInstallPath = DefaultTgtGamePath;
            }

            if (!File.Exists(AppSettings.SteamExecPath))
            {
                AppSettings.SteamExecPath = DefaultSteamExecPath;
            }
        }

        public static void LaunchGame()
        {
            Console.CR();

            if (AppSettings.GameInstallPath == null)
            {
                App.Console.PrintLn("Launch Request Failed: Game Directory Invalid");
                return;
            }

            var game_exec_path = App.GameExecPath;
            if (!File.Exists(game_exec_path))
            {
                App.Console.PrintLn("Launch Request Failed: Game .exe not found");
                return;
            }

            var steam_exec_path = App.DefaultSteamExecPath;
            if (!File.Exists(steam_exec_path) && AppSettings.LaunchWithSteam)
            {
                App.Console.PrintLn("Launch Request: Steam .exe not found, disabling steam-launch");
            }

            var launch_title = AppSettings.LaunchWithSteam ? "Steam" : @"Exec";
            Console.PrintLn($"#### Launching Game [{launch_title}] ####");


            if (AppSettings.LaunchWithSteam)
            {
                try { SteamLaunch(steam_exec_path); } catch (Exception e) { Console.PrintLn("Launch Failed [Steam] "); Console.PrintLn(1,e.Message); }
            }
            else
            {
                try { ExecLaunch(game_exec_path); } catch (Exception e) { Console.PrintLn("Launch Failed [Exec]"); Console.PrintLn(1, e.Message); }
            }

        }

        private static void ExecLaunch(string exec_path)
        {

            if (File.Exists(exec_path))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exec_path
                };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show($"Executable does not exist!\n{exec_path}");
            }

        }

        private static void SteamLaunch(string steam_path)
        {
            if (File.Exists(steam_path))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = $"-applaunch {App.SteamAppId}",
                    FileName = steam_path
                };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show($"Executable does not exist!\n{steam_path}");
            }
        }

    }
}
