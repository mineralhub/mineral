using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    using VMConfig = Runtime.Config.VMConfig;

    public static class VMUtil
    {
        private static void WriteStringToFile(FileInfo file, string data)
        {
            try
            {
                using (FileStream stream = new FileStream(file.FullName, FileMode.OpenOrCreate))
                {
                    if (data != null)
                    {
                        byte[] output = Encoding.UTF8.GetBytes(data);
                        stream.Write(output, 0, output.Length);
                    }

                }
            }
            catch
            {
                Logger.Error(string.Format("Can't wwrite to file {0}", file.FullName));
            }
        }

        private static FileInfo CreateProgramTraceFile(VMConfig config, string tx_hash)
        {
            FileInfo result = null;
            if (config.IsVmTrace)
            {
                DirectoryInfo directory = new DirectoryInfo(@"./vm_trace/");
                result = new FileInfo(directory.FullName + tx_hash + @".json");
                {
                    if (result.Exists)
                    {
                        if (!result.IsReadOnly)
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        try
                        {
                            Directory.CreateDirectory(directory.FullName);
                            File.Create(result.FullName);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }

        public static void Dispose(IDisposable disposable)
        {
            try
            {
                if (disposable != null)
                    disposable.Dispose();
            }
            catch
            {
            }
        }

        public static void SaveProgramTraceFile(VMConfig config, string tx_hash, string content)
        {
            FileInfo file = CreateProgramTraceFile(config, tx_hash);
            if (file != null)
            {
                WriteStringToFile(file, content);
            }
        }

        public static byte[] Compress(byte[] data)
        {
            byte[] result = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream compress = new DeflateStream(ms, CompressionMode.Compress))
                {
                    compress.Write(data, 0, data.Length);
                }
                result = ms.ToArray();
            }

            return result;
        }

        public static byte[] Compress(string content)
        {
            return Compress(Encoding.UTF8.GetBytes(content));
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] result = null;
            MemoryStream stream = new MemoryStream();
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (DeflateStream decompress = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    decompress.CopyTo(stream);
                }
            }

            result = stream.ToArray();
            stream.Close();

            return result;
        }

        public static string ZipAndEncode(string content)
        {
            try
            {
                return Convert.ToBase64String(Compress(content));
            }
            catch (System.Exception e)
            {
                Logger.Error("Cannot zip or encode : " + e.Message);
                return content;
            }
        }

        public static string UnzipAndDecode(string content)
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(content);
                return Encoding.UTF8.GetString(Decompress(decoded));
            }
            catch (System.Exception e)
            {
                Logger.Error("Cannot unzip or decode : " + e.Message);
                return content;
            }
        }
    }
}
