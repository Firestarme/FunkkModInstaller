using FunkkModInstaller.JSON;
using FunkkModInstaller.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

namespace FunkkModInstaller.Creator.TreeItems
{
    public class TreeItem_ModFile : TreeViewItem
    {
        public string? Filename
        {
            get => _JSONObj.Filename != null ? _JSONObj.Filename : "Not Set";
            set => _JSONObj.Filename = value;
        }
        public string? SrcPath
        {
            get => _JSONObj.SrcPath != null ? _JSONObj.SrcPath : "Not Set";
            set => _JSONObj.SrcPath = value;
        }
        public string? Version
        {
            get => _JSONObj.Version != null ? _JSONObj.Version : "Not Set";
            set => _JSONObj.Version = value;
        }
        public string? ModPath
        {
            get
                {
                if (SrcPath == null) return "??";
                return TruncatePath(SrcPath);
                }
            }

        public static readonly EditorControl editorControl = new EditorControl();
        static TreeItem_ModFile()
        {
            editorControl.RegisterProperty(new EditablePropertyString("File Name", nameof(Filename)));
            editorControl.RegisterProperty(new EditablePropertyString("File Path", nameof(SrcPath)));
            editorControl.RegisterProperty(new EditablePropertyString("Version", nameof(Version)));
        }

        private JSONModFile _JSONObj;
        public JSONModFile JSONObj => _JSONObj;

        private  TreeItem_ModFile(JSONModFile jsonObj) 
        {
            this._JSONObj = jsonObj;
            this.SetBinding(TreeViewItem.HeaderProperty, new Binding(nameof(ModPath)) { Source = this , Mode = BindingMode.OneWay});
        }

        public static TreeItem_ModFile LoadExisting(JSONModFile jsonObj)
        {
            return new TreeItem_ModFile(jsonObj);
        }

        public static TreeItem_ModFile CreateNewModFile(string path_packsrc, string path_filecurrent)
        {
            var file_obj = new JSONModFile();
            file_obj.Filename = Path.GetFileName(path_filecurrent);
            file_obj.SrcPath = Path.GetRelativePath(path_packsrc, path_filecurrent);
            file_obj.Version = FileVersionInfo.GetVersionInfo(path_filecurrent).FileVersion;

            return new TreeItem_ModFile(file_obj);
        }

        //Removes the highest level directory from the path (in the case of a modfile, this should be the GUID folder
        //This stops the display getting cluttered with guid folders
        private static string TruncatePath(string path)
        {
            int i;
            for (i = 0; i < path.Length; i++ ) 
            {
                if (path[i] == '\\') { break; }
            }
            return path.Substring(i);
        }

    }
}
