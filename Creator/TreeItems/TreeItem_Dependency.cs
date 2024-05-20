using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FunkkModInstaller.Creator.TreeItems
{
    public class TreeItem_Dependency : TreeViewItem
    {

        public string GUID;

        public TreeItem_Dependency() { GUID = Guid.NewGuid().ToString(); }
        public TreeItem_Dependency(string gUID) { GUID = gUID; }
    }
}
