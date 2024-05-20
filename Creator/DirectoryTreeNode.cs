using FunkkModInstaller.Utilities;
using System;

namespace FunkkModInstaller.Creator
{

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
