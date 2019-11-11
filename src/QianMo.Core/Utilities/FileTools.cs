using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QianMo.Core.Utilities
{
    public class FileTools
    {
        public static IEnumerable<string> GetAllFile(string dir, string searchPattern = "*")
        {
            if (Directory.Exists(dir) == false) return null;
            var files = Directory.GetFiles(dir, searchPattern).ToList();
            var paths = Directory.GetDirectories(dir);
            paths.ToList().ForEach(t =>
            {
                var subFiles = GetAllFile(t, searchPattern);
                if (subFiles?.Any() == true)
                {
                    files.AddRange(subFiles);
                }
            });
            return files;
        }
    }
}