using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        public static void EnsureFilePathExists(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        public static string ReadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                return null;
            }
        }

        public static string GetMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream))
                        .Replace("-", "‌​")
                        .ToLower();
                }
            }
        }
 
    }
}
