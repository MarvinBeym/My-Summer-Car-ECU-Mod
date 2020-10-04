using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DonnerTech_ECU_Mod
{
    class Helper
    {
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }
        public static string CreatePathIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
               Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
