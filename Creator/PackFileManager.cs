using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using FunkkModInstaller.JSON;

namespace FunkkModInstaller.Creator
{
    internal class PackDirectoryUtils
    {

        public const string DIALOG_ROOT = "./Packs";


        public static void NewPackDir()
        {


        }

        public static void SavePackDir()
        {



        }


    }


    internal class ModDirectoryUtils
    {

        public static void OpenModDir()
        {


        }

        public static void SaveModDir()
        {


        }

        public static void AddFile()
        {



        }

        public static void RemoveFile()
        {



        }


    }


    public class DirectoryTreeNode
    {
        public string? Directory;
        public readonly DirectoryTreeNode? Parent;

        public DirectoryTreeNode(string directory, DirectoryTreeNode? parent)
        {
            Directory = directory;
            Parent = parent;
        }

        public DirectoryTreeNode(string root)
        {
            Directory = root;
        }

        public String? GetFullPath()
        {
            if (Directory == null) { return null; }

            if (Parent == null)
            {
                return Directory;
            }
            else
            {
                var parent_path = Parent.GetFullPath();
                if (parent_path == null) { return null; }

                return vPath.Combine(parent_path,this.Directory);
            }
        }

        public String? GetRootPath()
        {
            if (Parent == null)
            {
                return Directory;
            }
            else
            {
                return Parent.GetFullPath();
            }
        }
    }

}
