using FunkkMI_Common.JSON;
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
    public class TreeItem_Pack : TreeViewItem, INotifyPropertyChanged
    {

        public static readonly EditorControl editorControl = new EditorControl();
        static TreeItem_Pack()
        {
            editorControl.RegisterProperty(new EditablePropertyString("ModPack Name", nameof(ModPackName)));
            editorControl.RegisterProperty(new EditablePropertyReadonlyString("Bepinex Version", nameof(BepinexVersion)));
            editorControl.RegisterProperty(new EditablePropertyReadonlyString("Pack Directory", nameof(PackDirectoryPath)));
        }

        public string ModPackName
        {
            get => _JSONObj.ModPackName != null ? _JSONObj.ModPackName : "Not Set";
            set
            {
                _JSONObj.ModPackName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModPackName)));
            }
        }

        public string BepinexVersion
        {
            get
            {
                if (_JSONObj.Bepinex == null) return "Bepinex Not Set";
                if (_JSONObj.Bepinex.Version == null) return "Bepinex Not Set";
                return _JSONObj.Bepinex.Version;
            }
        }

        private readonly DirectoryTreeNode directoryTreeNode;
        public string? PackDirectoryPath => directoryTreeNode.GetFullPath();

        private JSONModPack _JSONObj;
        public JSONModPack JSONObj => _JSONObj;

        public event PropertyChangedEventHandler? PropertyChanged;

        private TreeItem_Pack(JSONModPack JSONObj, string packDirectoryPath)
        {
            this._JSONObj = JSONObj;
            this.directoryTreeNode = new DirectoryTreeNode(packDirectoryPath);
            this.PopulateChildren(JSONObj);

            MenuItem Context_AddMod = new MenuItem();
            Context_AddMod.Header = "Add New Mod";
            Context_AddMod.Click += this.OnContextPress_AddNewMod;

            this.ContextMenu = new ContextMenu();
            this.ContextMenu.Items.Add(Context_AddMod);

            //UI Stuff
            this.SetBinding(TreeViewItem.HeaderProperty, new Binding(nameof(ModPackName)) { Source = this });

        }

        public static TreeItem_Pack LoadExisting(JSONModPack JSONObj, string packDirectoryPath)
        {
            return new TreeItem_Pack(JSONObj, packDirectoryPath);
        }

        public static TreeItem_Pack CreateNewPack(string name, string packDirectoryPath)
        {
            var obj = new JSONModPack();
            obj.ModPackName = name;
            
            var pack = new TreeItem_Pack(obj, packDirectoryPath);
            pack.AddNewBepinex();

            return pack;
        }

        public void AddNewEmptyMod()
        {
            var mod = TreeItem_Mod.CreateNewMod(this.directoryTreeNode);
            AddMod(mod);
        }

        private void AddMod(TreeItem_Mod mod)
        {
            if (this.JSONObj.Mods == null) { this.JSONObj.Mods = new List<JSONMod>(); }

            mod.VerifyDirectory(); // ensure directory exists

            this.JSONObj.Mods.Add(mod.JSONObj); //Add to JSON Structure
            this.Items.Add(mod); //Add to UI 

            mod.RequestDelete += Mod_RequestDelete; //Subscribe to delete event
        }

        public void AddNewBepinex()
        {
            var mod = TreeItem_Bepinex.CreateNewBepinex(this.directoryTreeNode);

            if (this.JSONObj.Mods == null) { this.JSONObj.Mods = new List<JSONMod>(); }

            mod.VerifyDirectory(); // ensure directory exists

            this.JSONObj.Bepinex = mod.JSONObj; //Add to JSON Structure
            this.Items.Add(mod); //Add to UI 
        }

        private void Mod_RequestDelete(object? sender, EventArgs args)
        {
            var mod = sender as TreeItem_Mod; if (mod == null) { return; }

            if (JSONObj.Mods == null) { return; }
            JSONObj.Mods.Remove(mod.JSONObj);
            this.Items.Remove(mod);
        }

        private void OnContextPress_AddNewMod(object sender, RoutedEventArgs args)
        {
            AddNewEmptyMod();
        }

        private void PopulateChildren(JSONModPack pack)
        {
            if(_JSONObj.Bepinex != null) 
            {

                var modti = TreeItem_Bepinex.TryLoadExisting(_JSONObj.Bepinex, this.directoryTreeNode);
                this.Items.Add(modti);
            }

            if (_JSONObj.Mods != null)
            {
                foreach (JSONMod mod in _JSONObj.Mods)
                {
                    var modti = TreeItem_Mod.TryLoadExisting(mod, this.directoryTreeNode);
                    if (modti == null) return;

                    this.Items.Add(modti);
                    modti.RequestDelete += Mod_RequestDelete;
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModPackName)));
        }


        public void VerifyDirectory()
        {
            if( this.PackDirectoryPath == null)
            {
                App.Console.PrintLn("ERR: [TI_PACK] No Directory Path");
                return;
            }

            try
            {
                Directory.CreateDirectory(this.PackDirectoryPath);
            }
            catch (Exception ex)
            {
                App.Console.PrintLn("ERR: [TI_PACK] Could not create mod directory");
                App.Console.PrintLn(2, ex.ToString());
                return;
            }
        }
    }
}
