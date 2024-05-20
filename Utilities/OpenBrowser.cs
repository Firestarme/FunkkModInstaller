using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FunkkModInstaller.Utilities
{
    internal class WinExplorer
    {
        private WinExplorer() { }

        public static void Open(string path)
        {
            if (Directory.Exists(path))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = path,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show($"Directory does not exist!\n{path}");
            }
        }
    }
}
