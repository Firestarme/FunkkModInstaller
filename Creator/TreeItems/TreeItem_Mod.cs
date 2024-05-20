using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FunkkModInstaller.JSON;
using FunkkModInstaller.Utilities;

namespace FunkkModInstaller.Creator.TreeItems
{
    public class TreeItem_Mod : TreeViewItem, INotifyPropertyChanged
    {
        public string? Mod_ID
        {
            get => _JSONObj.GUID != null ? _JSONObj.GUID : "ERROR: NO GUID";
            set => _JSONObj.GUID = value;
        }
        public virtual string? Mod_Name
        {
            get => _JSONObj.Name != null ? _JSONObj.Name : "Not Set";
            set { _JSONObj.Name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mod_Name))); }
        }
        public virtual string? Description
        {
            get => _JSONObj.Description != null ? _JSONObj.Description : "Not Set";
            set => _JSONObj.Description = value;
        }
        public virtual string? Version
        {
            get => _JSONObj.Version != null ? _JSONObj.Version : "Not Set";
            set => _JSONObj.Version = value;
        }
        public virtual bool? IsRequired
        {
            get => _JSONObj.IsRequired != null ? _JSONObj.IsRequired : false;
            set => _JSONObj.IsRequired = value;
        }
        public virtual bool? InstallByDefault
        {
            get => _JSONObj.InstallByDefault != null ? _JSONObj.InstallByDefault : true;
            set => _JSONObj.InstallByDefault = value;
        }
        public String DirectoryPath
        {
            get
            {
                var path = directoryTreeNode.GetFullPath();
                if (path == null) return "??";
                return path;
            }
        }

        protected readonly DirectoryTreeNode directoryTreeNode;

        public event EventHandler? RequestDelete;

        public static readonly EditorControl editorControl = new EditorControl();
        static TreeItem_Mod()
        {
            var IDProp = new EditablePropertyString("GUID", nameof(Mod_ID));
            IDProp.Control.IsEnabled = false;
            editorControl.RegisterProperty(IDProp);

            editorControl.RegisterProperty(new EditablePropertyString("Mod Name", nameof(Mod_Name)));
            editorControl.RegisterProperty(new EditablePropertyString("Mod Description", nameof(Description)));
            editorControl.RegisterProperty(new EditablePropertyString("Mod Version", nameof(Version)));
            editorControl.RegisterProperty(new EditablePropertyBool("Is Required?", nameof(IsRequired)));
            editorControl.RegisterProperty(new EditablePropertyBool("Is Installed By Default?", nameof(InstallByDefault)));

        }

        private JSONMod _JSONObj;
        public JSONMod JSONObj => _JSONObj;

        private TreeViewItem TV_Targets;
        private TreeViewItem TV_Dependencies;

        protected MenuItem Context_Delete;
        protected MenuItem Context_OpenDir;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected TreeItem_Mod(JSONMod JSONObj, DirectoryTreeNode parent)
        {
            _JSONObj = JSONObj;

            if (this._JSONObj.GUID == null)
            {
                this._JSONObj.GUID = Guid.NewGuid().ToString();
            }

            this.directoryTreeNode = new DirectoryTreeNode(JSONObj.GUID, parent);

            TV_Targets = new TreeViewItem();
            TV_Dependencies = new TreeViewItem();

            this.SetupTargetTreeItem();
            this.SetupDependenciesTreeItem();
            this.SetJSONObj(JSONObj);

            //UI Stuff
            this.SetBinding(TreeViewItem.HeaderProperty, new Binding(nameof(Mod_Name)) { Source = this });
            this.SetupContextMenu();

        }

        public static TreeItem_Mod? TryLoadExisting(JSONMod JSONObj, DirectoryTreeNode parent)
        {
            if(JSONObj.GUID == null)
            {
                App.Console.PrintLn("ERR: [TI_MOD] Load mod failed, No ID");
                return null;
            }
            return new TreeItem_Mod(JSONObj, parent);
        }

        public static  TreeItem_Mod CreateNewMod(DirectoryTreeNode parent)
        {
            var mod = new JSONMod();
            mod.GUID = Guid.NewGuid().ToString();
            return new TreeItem_Mod(mod,parent);
        }

        public void SetupContextMenu()
        {
            ContextMenu TargetMenu = new ContextMenu();
            this.ContextMenu = TargetMenu;

            Context_Delete = new MenuItem() { Header = "Delete" };
            Context_Delete.Click += (object o, RoutedEventArgs a) => RequestDelete?.Invoke(this, new EventArgs());
            TargetMenu.Items.Add(Context_Delete);

            Context_OpenDir = new MenuItem() { Header = "Open Directory" };
            Context_OpenDir.Click += OnContextPress_OpenDir;
            TargetMenu.Items.Add(Context_OpenDir);
        }

        public void OnContextPress_OpenDir(object sender, RoutedEventArgs args)
        {
            var path = directoryTreeNode.GetFullPath();
            if (path == null) return;

            WinExplorer.Open(path);
        }

        public void SetupTargetTreeItem()
        {
            TV_Targets.Header = "Targets";
            this.Items.Add(TV_Targets);

            ContextMenu TargetMenu = new ContextMenu();
            TV_Targets.ContextMenu = TargetMenu;

            var Context_AddTarget = new MenuItem() { Header = "Add Target" };
            Context_AddTarget.Click += (_,_) => this.AddNewTarget("Not Set");
            TargetMenu.Items.Add(Context_AddTarget);

            var Context_AddTPlugins = new MenuItem() { Header = "Add Target: Plugins" };
            Context_AddTPlugins.Click += (_,_) => this.AddNewTarget("\\BepInEx\\plugins");
            TargetMenu.Items.Add(Context_AddTPlugins);

            var Context_AddTConfig = new MenuItem() { Header = "Add Target: Config" };
            Context_AddTConfig.Click += (_, _) => this.AddNewTarget("\\BepInEx\\config");
            TargetMenu.Items.Add(Context_AddTConfig);

            var Context_AddTPatchers = new MenuItem() { Header = "Add Target: Patchers" };
            Context_AddTPatchers.Click += (_, _) => this.AddNewTarget("\\BepInEx\\patchers");
            TargetMenu.Items.Add(Context_AddTPatchers);
        }

        public void SetupDependenciesTreeItem()
        {
            TV_Dependencies.Header = "Dependencies";
            this.Items.Add(TV_Dependencies);
        }

        public void Target_DeleteRequested(object? sender, EventArgs args)
        {
            var tgt = sender as TreeItem_Target; if (tgt == null) { return; }

            if (JSONObj.Targets == null) { return; }
            JSONObj.Targets.Remove(tgt.JSONObj);
            this.TV_Targets.Items.Remove(tgt);

        }

        private void SetJSONObj(JSONMod pack)
        {
            //Dependencies
            this.TV_Dependencies.Items.Clear();
            if (_JSONObj.Dependencies != null)
            {
                foreach (string dep_guid in _JSONObj.Dependencies)
                {
                    this.TV_Dependencies.Items.Add(new TreeItem_Dependency(dep_guid));
                }
            }

            //Targets
            this.TV_Targets.Items.Clear();
            if (_JSONObj.Targets != null)
            {
                foreach (JSONTarget tgt in _JSONObj.Targets)
                {
                    var tree_tgt = new TreeItem_Target(tgt, this.directoryTreeNode);
                    this.TV_Targets.Items.Add(tree_tgt);
                    tree_tgt.RequestDelete += Target_DeleteRequested;
                }
            }
        }

        public void PopulateFromDirectoryStructure(string srcDirectory, string tgtRoot)
        {
            PopulateFromDirectoryRecursive(new DirectoryInfo(srcDirectory),srcDirectory,tgtRoot);
        }

        private void PopulateFromDirectoryRecursive(DirectoryInfo dir, string SrcRoot, string tgtRoot)
        {
            //tgtroot is the root of the desired target path eg. /BepInEx

            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                PopulateFromDirectoryRecursive(subdir, SrcRoot, tgtRoot);
            }

            FileInfo[] files = dir.GetFiles();
            if (files.Length == 0) return;

            string tgtDestPath = tgtRoot;
            if (SrcRoot != dir.FullName)
            {
                var relpath = Path.GetRelativePath(SrcRoot, dir.FullName);
                tgtDestPath = Path.Combine(tgtRoot, relpath);
            }

            var tgt = TreeItem_Target.CreateNewTarget(tgtDestPath, this.directoryTreeNode);
            this.AddTarget(tgt);

            var PackPath = this.directoryTreeNode.GetRootPath();
            if(PackPath == null) return;

            foreach (FileInfo file in files)
            {
                tgt.AddNewModfile(PackPath, file.FullName);
            }
        }

        public void AddNewTarget(string tgtDestPath)
        {
            var tgt = TreeItem_Target.CreateNewTarget(tgtDestPath, this.directoryTreeNode);
            this.AddTarget(tgt);
        }

        private void AddTarget(TreeItem_Target target)
        {
            if (this.JSONObj.Targets == null) { this.JSONObj.Targets = new List<JSONTarget>(); }
            if (this.JSONObj.Targets.Select((tgt) => tgt.DestDirPath).Contains(target.DestDirPath)) return; //No duplicates

            if (!this.JSONObj.Targets.Contains(target.JSONObj))
            {
                this.JSONObj.Targets.Add(target.JSONObj); //Add to JSON Structure
            }
            if (!this.TV_Targets.Items.Contains(target))
            {
                this.TV_Targets.Items.Add(target); //Add to UI 
            }

            target.RequestDelete += Target_DeleteRequested; //Subscribe to delete event
        }

        public void VerifyDirectory()
        {
            try
            {
                Directory.CreateDirectory(this.DirectoryPath);
            }
            catch (Exception ex)
            {
                App.Console.PrintLn("ERR: [TI_MOD] Could not create mod directory");
                App.Console.PrintLn(2, ex.ToString());
                return;
            }
        }
    }

    public class TreeItem_Bepinex : TreeItem_Mod
    {
        public override string? Mod_Name
        {
            get => "Bepinex";
            set { base.Mod_Name = "Bepinex"; }
        }
        public override bool? IsRequired
        {
            get => true;
            set { base.IsRequired = true; }
        }
        public override bool? InstallByDefault
        {
            get => true;
            set { base.IsRequired = true; }
        }

        public static readonly new EditorControl editorControl = new EditorControl();
        static TreeItem_Bepinex()
        {
            editorControl.RegisterProperty(new EditablePropertyReadonlyString("GUID", nameof(Mod_ID)));
            editorControl.RegisterProperty(new EditablePropertyReadonlyString("Mod Name", nameof(Mod_Name)));
            editorControl.RegisterProperty(new EditablePropertyString("Bepinex Version", nameof(Version)));
            editorControl.RegisterProperty(new EditablePropertyString("Mod Description", nameof(Description)));
        }

        protected TreeItem_Bepinex(JSONMod JSONObj, DirectoryTreeNode parent) : base(JSONObj, parent) 
        {
            var MenuItem_ReadFolder = new MenuItem();
            MenuItem_ReadFolder.Header = "Read Bepinex from folder";
            MenuItem_ReadFolder.Click += OnContextPress_ReadFolder;
            this.ContextMenu.Items.Add(MenuItem_ReadFolder);

            var MenuItem_ReadZip = new MenuItem();
            MenuItem_ReadZip.Header = "Read Bepinex from zip";
            MenuItem_ReadZip.Click += OnContextPress_ReadZip;
            this.ContextMenu.Items.Add(MenuItem_ReadZip);

            //Dont let the user delete bepinex
            this.ContextMenu.Items.Remove(Context_Delete);
        }

        public static new TreeItem_Mod? TryLoadExisting(JSONMod JSONObj, DirectoryTreeNode parent)
        {
            if (JSONObj.GUID == null)
            {
                App.Console.PrintLn("ERR: [TI_MOD] Load bepinex failed, No ID");
                return null;
            }
            return new TreeItem_Bepinex(JSONObj, parent);
        }

        public static new TreeItem_Mod CreateNewMod(DirectoryTreeNode parent)
        {
            return CreateNewBepinex(parent);
        }

        public static TreeItem_Bepinex CreateNewBepinex(DirectoryTreeNode parent)
        {
            JSONMod JSONObj = new JSONMod();

            JSONObj.Name = "Bepinex";
            JSONObj.IsRequired = true;
            JSONObj.InstallByDefault = true;

            return new TreeItem_Bepinex(JSONObj, parent);
        }

        public void OnContextPress_ReadFolder(object sender, RoutedEventArgs args)
        {
            var modpath = directoryTreeNode.GetFullPath();
            if (modpath == null) return;

            this.PopulateFromDirectoryStructure(modpath,"\\"); //NB empty tgt root is game dir (bepinex must be installed to game dir)
        }

        public void OnContextPress_ReadZip(object sender, RoutedEventArgs args)
        {

            var modpath = directoryTreeNode.GetFullPath();
            if (modpath == null) return;
            if(!Directory.Exists(modpath)) return;

            var dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.Multiselect = false;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = App.ARCHIVE_EXTENSION;

            //TODO Add initial directory persistence

            if (!dialog.ShowDialog() == true) return;

            App.Console.PrintLn("[TI_Bepinex] Reading Bepinex from Zip ... ");
            try
            {
                ZipFile.ExtractToDirectory(dialog.FileName, modpath);
                App.Console.Print("OK");

                this.PopulateFromDirectoryStructure(modpath, "\\"); //NB empty tgt root is game dir (bepinex must be installed to game dir)

                return;
            }
            catch (Exception ex)
            {
                App.Console.Print("FAIL: Exception");
                App.Console.PrintLn(1, ex.Message);
                return;
            }

        }
    }
}
