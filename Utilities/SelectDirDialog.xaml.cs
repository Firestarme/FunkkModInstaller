using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FunkkModInstaller.Utilities
{
    /// <summary>
    /// Interaction logic for SelectFileDialog.xaml
    /// </summary>
    public partial class SelectDirDialog : Window
    {

        public readonly String Root_Path;


        public SelectDirDialog(string root_path)
        {
            InitializeComponent();

            Root_Path = root_path;
            var Root_Item = new DirTreeItem(new DirectoryInfo(root_path)); //Check Path?
            Root_Item.PopulateChildren();
            this.DirViewer.Items.Add(Root_Item);

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
                    this.Items.Add(item);
                }
            }

        }
    }
}
