using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MineralCLI.Util
{
    public static class PathUtil
    {
        public static bool IsExistExtension(string name, string extention)
        {
            return name.Contains(extention);
        }

        public static bool MakeDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                return true;
            }

            return Directory.CreateDirectory(directory).Exists;
        }

        public static string MergeFileExtention(string name, string extention)
        {
            return name + extention;
        }

        public static string RemoveFirstBackSlash(string directory)
        {
            string result = directory;

            if (result == null || result.Length == 0)
                return "";

            int index = 0;
            if (result[index] == '\\')
            {
                result = result.Substring(0, index);
            }

            return result;
        }

        public static string RemoveLastBackSlash(string directory)
        {
            string result = directory;

            if (directory == null)
                return null;

            int length = result.Length - 1;
            if (result[length] == '\\')
            {
                result = result.Substring(0, length);
            }

            return result;
        }

        public static string MergePath(string path, string file_name, string extention)
        {
            string result = RemoveLastBackSlash(path);
            result += @"\";
            result += RemoveFirstBackSlash(file_name);
            result += extention;

            return result;
        }
    }
}
