using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FunkkModInstaller.Utilities
{
    /// <summary>
    /// Interaction logic for SelectFileDialog.xaml
    /// </summary>
    public partial class SelectFileDialog : Window
    {

        public readonly string Root_Path;
        private readonly DirTreeItem root_item;
        public List<string> Selected_Paths = new List<string>();

        private DirectoryInfo? SelectedDirectory
        {
            get 
            {
                var sel = this.DirViewer.SelectedItem as DirTreeItem;
                if (sel == null) { return null; }
                return sel.directory;
            }
        }

        public SelectFileDialog(string root_path)
        {
            InitializeComponent();

            Root_Path = root_path;
            root_item = new DirTreeItem(new DirectoryInfo(root_path)); //Check Path?
            root_item.PopulateChildren();

            this.DirViewer.Items.Add(root_item);
            this.DirViewer.SelectedItemChanged += DirViewer_SelectionChanged;
            root_item.IsSelected = true;
        }

        public void DirViewer_SelectionChanged(object sender, RoutedEventArgs args)
        {
            RefreshFileView();

            var selected_item = this.SelectedDirectory;
            if (selected_item == null) { return; }
            this.DragTarget.Dest_Path = selected_item.FullName;
        }

        private void DragTarget_FilesCopied(object sender, RoutedEventArgs e)
        {
            RefreshDirView();
            RefreshFileView();
        }

        private void RefreshFileView()
        {
            this.FileViewer.Items.Clear();

            var selected_item = this.SelectedDirectory;
            if (selected_item == null) { return; }
            foreach (FileInfo file in selected_item.GetFiles())
            {
                this.FileViewer.Items.Add(new FileItem(file));
            }
        }

        private void RefreshDirView()
        {
            root_item.Items.Clear();
            root_item.PopulateChildren();
            root_item.IsSelected = true;  
        }

        public class DirTreeItem : TreeViewItem
        {
            public readonly DirectoryInfo directory;

            public DirTreeItem(DirectoryInfo dir)
            {
                directory = dir;
                Header = dir.Name;
            }

            public void PopulateChildren()
            {
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    var item = new DirTreeItem(dir);
                    item.PopulateChildren();
                    Items.Add(item);
                }
            }

        }

        public class FileItem
        {
            public readonly BitmapSource bitmap;
            public readonly FileInfo File;

            public ImageSource IconSrc => bitmap;
            public string Name => File.Name;

            public FileItem(FileInfo file)
            {

                File = file;

                try
                {
                    using (var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName))
                    {
                        bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            sysicon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch { }

            }
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileItem file in FileViewer.SelectedItems)
            {
                Selected_Paths.Add(file.File.FullName);
            }

            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            WinExplorer.Open(this.SelectedDirectory.FullName);
        }

    }

    internal class Root_Item
    {
    }
}
