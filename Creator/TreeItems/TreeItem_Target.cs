using FunkkModInstaller.JSON;
using FunkkModInstaller.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FunkkModInstaller.Creator.TreeItems
{
    public class TreeItem_Target : TreeViewItem, INotifyPropertyChanged
    {

        public string? DestDirPath
        {
            get => _JSONObj.DestDirPath != null ? _JSONObj.DestDirPath : "Not Set";
            set { _JSONObj.DestDirPath = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestDirPath))); }
        }

        private DirectoryTreeNode mod_dtnode;

        public static readonly EditorControl editorControl = new EditorControl();
        static TreeItem_Target()
        {
            editorControl.RegisterProperty(new EditablePropertyString("Target Directory", nameof(DestDirPath)));
        }

        public event EventHandler RequestDelete;

        private JSONTarget _JSONObj;
        public JSONTarget JSONObj => _JSONObj;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TreeItem_Target(JSONTarget JsonObj,DirectoryTreeNode dtNode) 
        {
            this.mod_dtnode = dtNode;

            //Add existing JSON Objects to tree
            _JSONObj = JsonObj;
            if (_JSONObj.ModFiles != null)
            {
                foreach (JSONModFile mod_file in _JSONObj.ModFiles)
                {
                    this.Items.Add(TreeItem_ModFile.LoadExisting(mod_file));
                }
            }

            //UI Stuff
            this.SetBinding(TreeViewItem.HeaderProperty, new Binding(nameof(DestDirPath)) { Source = this });
            this.SetupContextMenu();

        }

        public static TreeItem_Target CreateNewTarget(string DestDirPath, DirectoryTreeNode dtree)
        {
            var JSONObj = new JSONTarget();
            JSONObj.DestDirPath = DestDirPath;

            return new TreeItem_Target(JSONObj,dtree);
        }

        private void SetupContextMenu()
        {

            this.ContextMenu = new ContextMenu();

            //Add Files
            MenuItem MenuItem_AddFiles = new MenuItem() { Header = "Add Files" };
            MenuItem_AddFiles.Click += OnContextPress_AddFiles;
            this.ContextMenu.Items.Add(MenuItem_AddFiles);

            //Delete Target
            MenuItem MenuItem_DeleteTarget = new MenuItem() { Header = "Delete Target" };
            MenuItem_DeleteTarget.Click += (object o, RoutedEventArgs a) => RequestDelete?.Invoke(this, new EventArgs()) ;
            this.ContextMenu.Items.Add(MenuItem_DeleteTarget);

        }

        public void OnContextPress_AddFiles(object sender, RoutedEventArgs args)
        {
            var mod_dir = this.mod_dtnode.GetFullPath();
            if (mod_dir == null ) { return; }

            if (!Directory.Exists(mod_dir)) 
            {
                try
                {
                    Directory.CreateDirectory(mod_dir);
                }
                catch ( Exception e )
                {
                    App.Console.PrintLn($"ERR: [Add Target Files] Could not create mod directory at {mod_dir}");
                    App.Console.PrintLn(2,e.Message);
                    return;
                }
            
            }

            var file_dialog = new SelectFileDialog(mod_dir);
            file_dialog.ShowDialog();
                
            var root_dir = this.mod_dtnode.GetRootPath();
            foreach(string file_path in file_dialog.Selected_Paths)
            {
                AddNewModfile(root_dir != null ? root_dir : "??", file_path);
            }  
        }

        public void AddNewModfile(string path_packsrc, string path_filecurrent)
        {
            var modfile = TreeItem_ModFile.CreateNewModFile(path_packsrc, path_filecurrent);
            this.AddModfile(modfile);
        }

        private void AddModfile(TreeItem_ModFile modfile) 
        {
            if (this._JSONObj.ModFiles == null) { this._JSONObj.ModFiles = new List<JSONModFile>(); }

            this._JSONObj.ModFiles.Add(modfile.JSONObj); //Add to JSON Structure
            this.Items.Add(modfile); //Add to UI Structure
        }

    }
}
