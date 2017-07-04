using System;
using System.IO;

namespace PagarMe.Generic
{
    public class FileExtension
    {
        public static void DeleteIfExists(String path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
