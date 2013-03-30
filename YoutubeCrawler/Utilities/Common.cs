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
using System.Text.RegularExpressions;

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
                    try
                    {
                        File.Delete(pChannelName + "/" + fileName);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
        }
        public static string CleanFileName(string pFileName)
        {
            return pFileName.Trim().Replace(":", "").Replace("/", "").Replace("\\", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
        }
        public static String FilterCommentText(String comment)
        {
            //For lawfirm having title like http://www.dwt.com/LearningCenter/BooksPublications?viewPage=all
            comment = comment.Replace("&#039;", "'");
            comment = comment.Replace("&#39;", "'");
            comment = comment.Replace("&rsquo;", "'");
            comment = comment.Replace("â€™", "'");
            comment = comment.Replace("â€”", "-");
            comment = comment.Replace("&rdquo;", "\"");
            comment = comment.Replace("&shy;", "-");
            comment = comment.Replace("&ldquo;", "\"");
            comment = comment.Replace("â€œ", "\"");
            comment = comment.Replace("â€", "\"");
            comment = comment.Replace("â€“", "-");
            comment = comment.Replace("â€˜", "'");
            comment = comment.Replace("&#8212;", string.Empty);
            comment = comment.Replace("&#8216;", string.Empty);
            comment = comment.Replace("&#8217;", string.Empty);
            comment = comment.Replace("&#8220;", string.Empty);
            comment = comment.Replace("&#8221;", string.Empty);
            comment = comment.Replace("&#39;", string.Empty);
            comment = comment.Replace("&nbsp;", " ").Trim();
            comment = comment.Replace("&quot;", string.Empty);
            comment = comment.Replace("&amp;", "&");
            comment = comment.Replace("\r\n", " ");//
            comment = comment.Replace("&nbsp;", string.Empty);
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            comment = regex.Replace(comment, @" ");
            comment = Regex.Replace(comment, @"<[^>]*>", String.Empty);

            return comment;


        }

        public static string CleanXpath(string xPath)
        {
            return xPath.Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
        }

    }
}
