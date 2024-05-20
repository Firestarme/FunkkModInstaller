using Microsoft.WindowsAPICodePack.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using FunkkModInstaller.JSON;

namespace FunkkModInstaller
{
    class Installer
    {

        public const String PersistentFilePath = "FML_PERSISTENT.json";

        private String _StagingDirectory = "??";
        private String _GameDirectory = "??";
        public String StagingDirectory => _StagingDirectory;
        public String GameDirectory => _GameDirectory;

        private PackInfo? _ActivePack;
        public PackInfo? ActivePack => _ActivePack;

        public bool IsUpdateRequired => _ActivePack != null ? _ActivePack.IsUpdateRequired : false;

        public Installer() { }

        private void SetActivePack(PackInfo? pack)
        {
            if(ActivePack != null) ActivePack.IsPackActive = false;
            if(pack != null) pack.IsPackActive = true;

            _ActivePack = pack;
        }


        //##### Pack Methods ######

        //This method allows the replacement of the persistant pack with the equivelant PackInfo loaded
        //in to memory when populating modpacks.
        public void SwapPackObject(PackInfo pack)
        {
            if (ActivePack == null) { return; }
            if (pack.PackHash == ActivePack.PackHash)
            {
                var new_moddict = pack.GetModDict();

                foreach (var mod in ActivePack.Mods) 
                {
                    new_moddict[mod.ModID].IsInstallDesired = mod.IsInstalled;
                    new_moddict[mod.ModID].IsInstalled = mod.IsInstalled;
                }
                SetActivePack(pack);
            }
        }

        public void SetGameDir(string game_path)
        {
            App.Console.PrintLn($"Setting game directory to: {game_path}");
            this._GameDirectory = game_path;
            SetActivePack(null);

            string full_persistent_path = vPath.Combine(GameDirectory, PersistentFilePath);
            if (!File.Exists(full_persistent_path)) { return; }

            App.Console.PrintLn($"Persistent file exists, Loading ... ");
            var json_io = new JSONReader();
            var doc = json_io.TryReadFile(full_persistent_path);

            if (doc == null) { App.Console.Print("FAIL"); return; }
            SetActivePack(PackInfo.CreateWithNoSource(doc));
            ActivePack.IsInstalled = true;
            App.Console.Print("OK");

        }

        public void SetStagingDirectory(string staging_path)
        {
            this._StagingDirectory = staging_path;
        }

        public void UninstallPack()
        {
            App.Console.PrintLn("\n##### Uninstall Request ##### ");
            _UninstallPack();
        }

        private void _UninstallPack()
        {
            if (ActivePack == null)
            {
                App.Console.Print("No Pack Installed!");
                return;
            }

            var total_mods = ActivePack.Mods.Count;
            var removed_mods = 0;

            App.Console.PrintLn($"Uninstalling Pack: {ActivePack.Name}");
            foreach (var mod in ActivePack.Mods)
            {
                UninstallMod(mod);
                if (!mod.IsInstalled) { removed_mods++; }
            }

            //Uninstall Bepinex
            if (ActivePack.Bepinex != null)
            {
                UninstallMod(ActivePack.Bepinex);

                if (ActivePack.Bepinex.IsInstalled)
                {
                    App.Console.PrintLn($"ERROR: Could not uninstall BepInEx");
                }
            }

            //Remove Persistent File
            if (Directory.Exists(GameDirectory))
            {
                var persist_full_path = vPath.Combine(GameDirectory, PersistentFilePath);
                try { File.Delete(persist_full_path); }
                catch { App.Console.PrintLn("Persistent File Removal FAILED"); }
            }

            App.Console.CR();
            if (removed_mods == total_mods)
            {
                ActivePack.IsInstalled = false;
                SetActivePack(null);
                App.Console.PrintLn($"--- Uninstall Completed Succesfully ---");
            }
            else
            {
                App.Console.PrintLn($"--- Uninstall FAILED ---");
            }
        }

        public void InstallPack(PackInfo pack)
        {

            App.Console.PrintLn("\n##### Install Request ##### ");

            if (ActivePack != null)
            {
                if (ActivePack.PackHash != pack.PackHash)
                {
                    App.Console.PrintLn("Existing Installation Detected ... Uninstalling this first");
                    _UninstallPack();

                    if (ActivePack != null)
                    {
                        App.Console.PrintLn("Existing Installation Detected ... Cannot Install");
                        return;
                    }
                }
            }

            SetActivePack(pack);
            var total_mods = ActivePack.Mods.Count;
            var sucessful_mods = 0;

            App.Console.PrintLn($"Installing Pack: {pack.Name}");

            //Install Bepinex
            if (pack.Bepinex != null) {

                InstallMod(pack.Bepinex);

                if (!pack.Bepinex.IsInstalled)
                {
                    App.Console.PrintLn($"FAIL: Could not install BepInEx");
                    App.Console.PrintLn($"--- Install Failed  ---");
                    return;
                }
            }

            foreach (ModInfo mod in ActivePack.Mods)
            {
                if (!mod.IsUpdateRequired) {
                    App.Console.CR();
                    App.Console.PrintLn(1, $"> Skipping Mod: {mod.ModName}"); 
                    sucessful_mods++;
                    continue;
                }

                if (mod.IsInstallDesired)
                {
                    InstallMod(mod);
                }
                else
                {
                    UninstallMod(mod);
                }

                if (!mod.IsUpdateRequired) { sucessful_mods++; }
            }

            //Create Persistent File
            if (Directory.Exists(GameDirectory))
            {
                var json_io = new JSONReader();
                var persist_full_path = vPath.Combine(GameDirectory, PersistentFilePath);
                json_io.TryWriteFile(persist_full_path, ActivePack.JSONObj);
            }

            App.Console.CR();
            if (sucessful_mods == total_mods)
            {
                App.Console.PrintLn($"--- Install Completed Succesfully ---");
            }
            else
            {
                App.Console.PrintLn($"--- Install Completed with Errors ---");
            }
        }

        public void VerifyPack()
        {

            App.Console.PrintLn("\n##### Verify Request ##### ");
            if (ActivePack == null)
            {
                App.Console.Print("No Pack Installed!");
                return;
            }

            App.Console.PrintLn(ActivePack.Name);
            if (ActivePack.Bepinex !=  null)
            {
                VerifyModInstallation(ActivePack.Bepinex);
            }

            foreach (ModInfo mod in ActivePack.Mods)
            {
                VerifyModInstallation(mod);
                //if (mod.IsUpdateRequired) { _IsUpdateRequired = true; }
            }

            App.Console.CR();
            App.Console.PrintLn($"--- Verify Completed ---");
        }

        public void SetDesiredToInstalled()
        {
            if (ActivePack == null)
            {
                return;
            }

            foreach (ModInfo mod in ActivePack.Mods)
            {
                mod.IsInstallDesired = mod.IsInstalled;
            }
        }

        //public bool CheckIsUpdateRequired()
        //{
        //    //dont need to update if no pack is selected
        //    if (ActivePack == null)
        //    {
        //        _IsUpdateRequired = false;
        //        return false;
        //    }

        //    //update is needed if bepinex requires update
        //    if(ActivePack.Bepinex != null)
        //    {
        //        if(ActivePack.Bepinex.IsUpdateRequired)
        //        {
        //            _IsUpdateRequired = true;
        //            return true;
        //        }
        //    }

        //    //check mods for update status
        //    foreach (ModInfo mod in ActivePack.Mods)
        //    {
        //        if (mod.IsUpdateRequired) { 
        //            _IsUpdateRequired = true; 
        //            return true;
        //        }
        //    }

        //    _IsUpdateRequired = false;
        //    return false;
        //}

        //##### Directory Structure Methods ######

        private void CreateDirectoryStructure(JSONTarget tgt)
        {
            App.Console.PrintLn(2, $"-Creating Directory Structure for Target: {tgt.DestDirPath} ... ");

            if (tgt.DestDirPath == null)
            {
                App.Console.Print("FAIL: No Path");
            }

            var success = true;
            foreach (string path in IterateTargetDirs(tgt.DestDirPath).Reverse())
            {
                var full_path = vPath.Combine(GameDirectory, tgt.DestDirPath);
                if (Directory.Exists(full_path)) { continue; }

                try { Directory.CreateDirectory(full_path); }
                catch { success = false; }
            }

            if (success) { App.Console.Print("OK"); }
            else { App.Console.Print("FAIL"); }
        }

        private void RemoveDirectoryStructure(JSONTarget tgt)
        {
            App.Console.PrintLn(2, $"-Removing Directory Structure for Target: {tgt.DestDirPath} ... ");

            if (tgt.DestDirPath == null)
            {
                App.Console.Print("FAIL: No Path");
            }

            var success = true;
            foreach (string path in IterateTargetDirs(tgt.DestDirPath).Reverse())
            {
                var full_path = vPath.Combine(GameDirectory, tgt.DestDirPath);
                if (!Directory.Exists(full_path)) { continue; }
                if (Directory.GetFiles(full_path).Length > 0) { success = false; continue; }

                try { Directory.Delete(full_path); }
                catch { success = false; }
            }

            if (success) { App.Console.Print("OK"); }
            else { App.Console.Print("FAIL"); }
        }

        private IEnumerable<string> IterateTargetDirs(string path)
        {
            path = path.Trim('/', '\\');

            yield return path;

            for (var i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/' || path[i] == '\\')
                {
                    yield return path.Substring(0, i - 1);
                }
            }
        }

        //#####  Mod Methods #####
        private void UninstallMod(ModInfo mod)
        {
            const int loglv = 1;

            int manifest_files = 0;
            int removed_files = 0;

            App.Console.CR();
            App.Console.PrintLn(loglv, $"> Uninstalling Mod: {mod.ModName}");

            //Check Root Paths
            if (!Directory.Exists(this.GameDirectory))
            {
                App.Console.Print($" ... FAIL: Cant Find Game Directory at: {this.GameDirectory} ");
                return;
            }

            //Iterate Modfiles
            foreach (var fileinfo in IterateModfiles(mod))
            {
                manifest_files++;

                App.Console.PrintLn(loglv + 1, $"-Removing File: {fileinfo.File.Filename} ... ");

                //Null checks
                if (fileinfo.Target.DestDirPath == null) { App.Console.Print("FAIL: No Tgt Path"); continue; }
                if (fileinfo.File.Filename == null) { App.Console.Print("FAIL: No Filename"); continue; }

                //Check that this path is real
                var tgt_dir_path = fileinfo.GetDestinationDirPath(GameDirectory);
                if (!Directory.Exists(tgt_dir_path)) { App.Console.Print($"FAIL: Bad Path {tgt_dir_path}"); continue; }

                //Ok if the target file does not exists
                var tgt_file_path = fileinfo.GetDestinationFilePath(GameDirectory);
                if (!File.Exists(tgt_file_path))
                {
                    App.Console.Print("OK: File Already Removed");
                    removed_files++;
                    continue;
                }

                try
                {
                    File.Delete(tgt_file_path);
                    App.Console.Print("OK");
                    removed_files++;
                }
                catch (Exception e)
                {
                    App.Console.Print("FAIL: Unspecified");
                    App.Console.PrintLn(loglv + 2, e.Message);
                }
            }

            //Remove Directory Structure
            if (mod.JSONObj.Targets != null)
            {
                foreach (var tgt in mod.JSONObj.Targets)
                {
                    RemoveDirectoryStructure(tgt);
                }
            }

            //Finish up
            mod.IsInstalled = !(removed_files == manifest_files);
            mod.IsErrored = removed_files < manifest_files;
            App.Console.PrintLn(loglv, $"{mod.ModName} Installed: {mod.IsInstalled} Errored: {mod.IsErrored}");
        }

        private void InstallMod(ModInfo mod)
        {
            const int loglv = 1;
            int manifest_files = 0;
            int installed_files = 0;

            App.Console.CR();
            App.Console.PrintLn(loglv, $"> Installing Mod: {mod.ModName}");


            //Check Root Paths
            if (!Directory.Exists(this.GameDirectory))
            {
                App.Console.Print($" ... FAIL: Cant Find Game Directory at: {this.GameDirectory} ");
                return;
            }

            if (!Directory.Exists(this.StagingDirectory))
            {
                App.Console.Print($" ... FAIL: Cant Find Staging Directory at: {this.StagingDirectory} ");
                return;
            }

            //Create Directory Structure
            if (mod.JSONObj.Targets != null)
            { 
                foreach (var tgt in mod.JSONObj.Targets)
                {
                    CreateDirectoryStructure(tgt);
                }
            }

            //Iterate Modfiles
            foreach (var fileinfo in IterateModfiles(mod))
            {
                manifest_files++;

                App.Console.PrintLn(loglv + 1, $"-Installing File: {fileinfo.File.Filename} ... ");

                //Null checks
                if (fileinfo.Target.DestDirPath == null) { App.Console.Print("FAIL: No Tgt Path"); continue; }
                if (fileinfo.File.Filename == null) { App.Console.Print("FAIL: No Filename"); continue; }
                if (fileinfo.File.SrcPath == null) { App.Console.Print("FAIL: No Src Path"); continue; }

                //Check that destination directory exists
                var tgt_dir_path = fileinfo.GetDestinationDirPath(GameDirectory);
                if (!Directory.Exists(tgt_dir_path)) { App.Console.Print($"FAIL: Bad Tgt Path: {tgt_dir_path}"); continue; }

                //Check the file exists
                var src_file_path = fileinfo.GetSourceFilePath(StagingDirectory);
                if (!File.Exists(src_file_path)) { App.Console.Print($"FAIL: Bad File Path: {src_file_path}"); continue; }

                //Create destination path
                var dest_file_path = fileinfo.GetDestinationFilePath(GameDirectory);

                //Check if the file is alreay installed (TODO add Force Overwrite Option)
                if (File.Exists(dest_file_path))
                {
                    App.Console.Print("OK: File Already Exists");
                    installed_files++;
                    continue;
                }

                try
                {
                    File.Copy(src_file_path, dest_file_path);
                    App.Console.Print("OK");
                    installed_files++;
                }
                catch (Exception e)
                {
                    App.Console.Print("FAIL: Unspecified");
                    App.Console.PrintLn(loglv + 2, e.Message);
                }
            }

            //Finish up
            mod.IsInstalled = installed_files > 0;
            mod.IsErrored = installed_files < manifest_files;
            App.Console.PrintLn(loglv, $"{mod.ModName} Installed: {mod.IsInstalled} Errored: {mod.IsErrored}");

        }

        private void VerifyModInstallation(ModInfo mod)
        {
            const int loglv = 1;

            int manifest_files = 0;
            int installed_files = 0;

            App.Console.CR();
            App.Console.PrintLn(loglv, $"> Verifying Mod Install: {mod.ModName}");

            //Check Root Paths
            if (!Directory.Exists(this.GameDirectory))
            {
                App.Console.Print($" ... FAIL: Cant Find Game Directory at: {this.GameDirectory} ");
                return;
            }

            //Iterate Modfiles
            foreach (var fileinfo in IterateModfiles(mod))
            {
                manifest_files++;

                App.Console.PrintLn(loglv + 1, $"-Verifying File: {fileinfo.File.Filename} ... ");

                //Null Checks
                if (fileinfo.File.Filename == null) { App.Console.Print("FAIL: No Filename"); continue; }
                if (fileinfo.Target.DestDirPath == null) { App.Console.Print("FAIL: No Tgt Path"); continue; }

                //Check if file exists
                var dest_path = fileinfo.GetDestinationFilePath(GameDirectory);
                if (!File.Exists(dest_path)) { App.Console.Print("FAIL: File does not Exist"); continue; }

                installed_files++;
                App.Console.Print("OK");

            }

            //Finish up
            mod.IsInstalled = (manifest_files == installed_files);
            App.Console.PrintLn(loglv, $"{mod.ModName} Installed: {mod.IsInstalled}");
        }

        private void VerifyModSourceFiles(ModInfo mod)
        {
            const int loglv = 1;

            int manifest_files = 0;
            int src_files = 0;

            App.Console.CR();
            App.Console.PrintLn(loglv, $"> Verifying Mod Source: {mod.ModName}");

            if (!Directory.Exists(this.StagingDirectory))
            {
                App.Console.Print($" ... FAIL: Cant Find Staging Directory at: {this.StagingDirectory} ");
                return;
            }

            //Iterate Modfiles
            foreach (var fileinfo in IterateModfiles(mod))
            {
                manifest_files++;

                App.Console.PrintLn(loglv + 1, $"-Verifying File: {fileinfo.File.Filename} ... ");
                if (fileinfo.File.SrcPath == null) { App.Console.Print("FAIL: No Src Path"); continue; }
                if (fileinfo.Target.DestDirPath == null) { App.Console.Print("FAIL: No Tgt Path"); continue; }

                //Check if file exists
                var dest_path = fileinfo.GetSourceFilePath(StagingDirectory);
                if (!File.Exists(dest_path)) { App.Console.Print("FAIL: File does not Exist"); continue; }

                src_files++;
                App.Console.Print("OK");
            }

            //Finish up
            mod.IsInstalled = (manifest_files == src_files);
        }

        private IEnumerable<TargetFilePair> IterateModfiles(ModInfo mod)
        {
            //Iterate Modfiles
            if (mod.JSONObj.Targets == null) { yield break; }
            foreach (var tgt in mod.JSONObj.Targets)
            {
                if (tgt.ModFiles == null) { continue; }
                foreach (var modfile in tgt.ModFiles)
                {
                    yield return new TargetFilePair { Target = tgt, File = modfile };
                }
            }
        }

        //Event handlers
        //private void OnModPropertyChanged(object? sender, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName == nameof(ModInfo.IsInstallDesired))
        //    {
        //        this.CheckIsUpdateRequired();
        //    }

        //}

        private struct TargetFilePair
        {
            public JSONTarget Target;
            public JSONModFile File;


            //Source Paths
            public String GetSourceFilePath(string StagingDirectory)
            {
                if (File.SrcPath == null) return "??";
                return vPath.Combine(StagingDirectory, File.SrcPath);
            }

            //Destination Paths
            public String GetDestinationDirPath(string GameDirectory)
            {
                if (Target.DestDirPath == null) return "??";
                return vPath.Combine(GameDirectory, Target.DestDirPath);
            }
            public String GetDestinationFilePath(string GameDirectory)
            {
                if (File.Filename == null) return "??";
                return vPath.Combine(this.GetDestinationDirPath(GameDirectory), File.Filename);
            }


        }








    }
}
