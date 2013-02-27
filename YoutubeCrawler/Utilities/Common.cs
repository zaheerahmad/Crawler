using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using System.IO;

namespace YoutubeCrawler.Utilities
{
    class Common
    {
        public static void RemoveTempFiles(List<string> pFileName, string pChannelName)
        {
            foreach (string fileName in pFileName)
            {
                if (File.Exists(pChannelName + "/" + fileName))
                {
                    File.Delete(pChannelName + "/" + fileName);
                }
            }
        }
        public static string CleanFileName(string pFileName)
        {
            return pFileName.Trim().Replace(":", "").Replace("/", "").Replace("\\", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
        }
    }
}
