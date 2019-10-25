using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mineral.Utils
{
    public static class FileUtils
    {
        public static void RecursiveDelete(string file_path)
        {
            DirectoryInfo dir_info = new DirectoryInfo(file_path);
            if (dir_info.Attributes == FileAttributes.Directory)
            {
                foreach (DirectoryInfo info in dir_info.GetDirectories())
                {
                    RecursiveDelete(info.FullName);
                }

                foreach (FileInfo info in dir_info.GetFiles())
                {
                    info.Attributes = FileAttributes.Normal;
                    info.Delete();
                }
                Directory.Delete(file_path);
            }
            else
            {
                FileInfo info = new FileInfo(file_path);
                info.Attributes = FileAttributes.Normal;
                info.Delete();
            }
        }
    }
}
