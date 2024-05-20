using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FunkkModInstaller
{
    class vPath
    {
        //Redefinition of Path to allow for more checks on the output of vPath.Combine


        public static string Combine(params string[] paths)
        {

            for(int i=0; i<paths.Length; i++)
            {
                paths[i] = paths[i].Trim('/','\\');
            }

            return System.IO.Path.Combine(paths);
        }

        public static string? GetExtension(string? path)
        {
            return System.IO.Path.GetExtension(path);
        }

        public static string? GetFileName(string? path)
        {
            return System.IO.Path.GetFileName(path); 
        }

        public static string? GetRelativePath(string relativeTo,string path)
        {
            return System.IO.Path.GetRelativePath(relativeTo, path);
        }


    }
}
