using FunkkModInstaller.Creator.TreeItems;
using FunkkModInstaller.JSON;
using FunkkModInstaller.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using Valheim_ModInstaller.Creator;

namespace FunkkModInstaller.Creator
{
    /// <summary>
    /// Interaction logic for PackCreator.xaml
    /// </summary>
    public partial class PackCreator : UserControl
    {

        private readonly Dictionary<Type, EditorControl> EditorControls;

        public PackCreator()
        {

            this.EditorControls = new Dictionary<Type, EditorControl>();
            this.EditorControls.Add(typeof(TreeItem_Pack), TreeItem_Pack.editorControl);
            this.EditorControls.Add(typeof(TreeItem_Mod), TreeItem_Mod.editorControl);
            this.EditorControls.Add(typeof(TreeItem_ModFile), TreeItem_ModFile.editorControl);
            this.EditorControls.Add(typeof(TreeItem_Target), TreeItem_Target.editorControl);
            this.EditorControls.Add(typeof(TreeItem_Bepinex), TreeItem_Bepinex.editorControl);

            InitializeComponent();
            UpdatePacks(doSave: false);
        }

        private void AddNewPack()
        {
            AddPackDialog dialog = new AddPackDialog();
            dialog.ShowDialog();

            if (!dialog.Result_ok) { return; }

            if (dialog.ModpackPath == null) { MessageBox.Show("Path Not Set"); return; }
            if (dialog.ModpackName == null) { MessageBox.Show("Name Not Set"); return; }

            string packPath = vPath.Combine(App.EditorPackPath, dialog.ModpackPath);
            if (Directory.Exists(packPath)) { MessageBox.Show("Directory Exists"); return; }

            var treeItem = TreeItem_Pack.CreateNewPack(dialog.ModpackName, packPath);
            this.TV_PackViewer.Items.Add(treeItem);
        }

        private void UpdatePacks(bool doSave = true)
        {
            List<string> SavedLog = new List<string>();
            var json_io = new JSONReader();

            if(!Directory.Exists(App.EditorPackPath)) { Directory.CreateDirectory(App.EditorPackPath); }

            //Save All
            if (doSave)
            {
                foreach (TreeItem_Pack pack in TV_PackViewer.Items)
                {
                    try
                    {
                        if (!Directory.Exists(pack.PackDirectoryPath)) { Directory.CreateDirectory(pack.PackDirectoryPath); }
                        var success = json_io.TryWritePackDirectory(pack.PackDirectoryPath, pack.JSONObj);

                        if (success) { SavedLog.Add(pack.PackDirectoryPath); }
                    }
                    catch (Exception e)
                    {


                    }
                }
            }

            //Load All
            foreach (string subdir_path in Directory.EnumerateDirectories(App.EditorPackPath))
            {
                if (SavedLog.Contains(subdir_path)) { continue; }

                var packJson = json_io.TryReadPackDirectory(subdir_path);
                if (packJson == null) { continue; }

                var packTreeItem = TreeItem_Pack.LoadExisting(packJson.JSONModPack, subdir_path);
                this.TV_PackViewer.Items.Add(packTreeItem);
            }

        }

        private void TV_PackViewer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            EditorControl? e_control = TryGetRelevantEditorControl(TV_PackViewer.SelectedItem);
            Editor_Area.Content = e_control;
            if (e_control != null)
            {
                e_control.SetItem(TV_PackViewer.SelectedItem);
            }
        }

        private EditorControl? TryGetRelevantEditorControl(object item)
        {
            if (item == null) return null;
            if (!(EditorControls.ContainsKey(item.GetType()))) return null;

            return EditorControls[item.GetType()];
        }

        private void Button_OpenDir(object sender, RoutedEventArgs e)
        {
            WinExplorer.Open(App.EditorPackPath);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddNewPack();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdatePacks();
        }

        private void ButtonZip_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeItem_Pack pack in TV_PackViewer.Items)
            {
                try
                {
                    if (!Directory.Exists(App.PackPath)) { Directory.CreateDirectory(App.PackPath); }
                    if (!Directory.Exists(pack.PackDirectoryPath)) { continue; }

                    var dest_path = vPath.Combine(App.PackPath, pack.ModPackName + App.ARCHIVE_EXTENSION);
                    if (File.Exists(dest_path)) { File.Delete(dest_path); }

                    ZipFile.CreateFromDirectory(pack.PackDirectoryPath, dest_path);
                }
                catch (Exception ex)
                {
                    //SUPER lazy
                    MessageBox.Show("Error\n"+ex.Message);
                }
            }

        }
    }
}
