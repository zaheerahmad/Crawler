using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using System.Configuration;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;

namespace YoutubeCrawler.Utilities
{
    class ChannelComment
    {
        //videoCommentUserName = "//ul[@id='all-comments']//li//div[@class='content']//span[@class='author']/a"
        //time = "//ul[@id='all-comments']//li//div[@class='content']//span[@class='time']/a"
        //Comment Text = "//ul[@id='all-comments']//li//div[@class='content']/div[@class='comment-text']//p"
        //totalcomments = "//div[@id='comments-view']//h4"
        //http://www.youtube.com/all_comments?v=
        public static int totalComments = 0;
        public static int pageNo = 1;
        public static string fileComment = ConfigurationManager.AppSettings["channelVideoComments"].ToString();
        public static bool CrawlComments(Dictionary<string, VideoWrapper> videoDictionary, string pChannelName)
        {
            foreach (KeyValuePair<string, VideoWrapper> pair in videoDictionary)
            {
                VideoWrapper video = pair.Value;
                string url = ConfigurationManager.AppSettings["VideoAllCommentsUrl"].ToString() + video.getVideoKey() + "&page=" + pageNo;
                HtmlWeb hwObject = new HtmlWeb();
                HtmlDocument doc = hwObject.Load(url);

                HtmlNode totalCommentsCount = doc.DocumentNode.SelectSingleNode("//div[@id='comments-view']//h4");
                
                string totalCountStr = totalCommentsCount.InnerText.Trim().Replace(",", "").Replace(" ", "");
                string[] totalCountArr = Regex.Split(totalCountStr, @"[^\d]+");
                StringBuilder str = new StringBuilder();
                for (int index = 0; index < totalCountArr.Length; index++)
                {
                    if (!totalCountArr[index].Equals(""))
                    {
                        str.Append(totalCountArr[index]);
                    }
                }
                
               totalComments = Int32.Parse(str.ToString());
                
                for (int i = 0; i < totalComments; i++)
                {
                    string user = GetUser(i + 1, doc);
                    string time = GetTime(i + 1, doc);
                    string comment = GetComment(i + 1, doc);

                    if (!user.Equals("") && !time.Equals("") && !comment.Equals(""))
                    {
                        string videoName = video.getVideoName();
                        videoName = Common.CleanFileName(videoName);
                        File.AppendAllText(pChannelName + "/" + videoName + "-" + fileComment, "User name : " + user + Environment.NewLine);
                        File.AppendAllText(pChannelName + "/" + videoName + "-" + fileComment, "Comment Date : " + time + Environment.NewLine);
                        File.AppendAllText(pChannelName + "/" + videoName + "-" + fileComment, "Comment : " + comment + Environment.NewLine);
                        //File.AppendAllText(pChannelName + "/" + video.getVideoName() + fileComment, "Value of i : " + i + Environment.NewLine);
                    }
                }
               

            }
            return true;
        }

        public static string GetUser(int index, HtmlDocument doc)
        {
            HtmlNode userName = doc.DocumentNode.SelectSingleNode("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author']");
            if (userName == null)
            {
                userName = doc.DocumentNode.SelectSingleNode("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author ']");
            }
            if (userName == null)
                return String.Empty;
            return userName.InnerText.Trim();
        }
        public static string GetTime(int index, HtmlDocument doc)
        {
            HtmlNode time = doc.DocumentNode.SelectSingleNode("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='time']");
            if (time == null)
                return String.Empty;
            return time.InnerText.Trim();
        }
        public static string GetComment(int index, HtmlDocument doc)
        {
            HtmlNode comment = doc.DocumentNode.SelectSingleNode("//ul[@id='all-comments']//li[" + index + "]//div[@class='content']/div[@class='comment-text']//p");
            if (comment == null)
                return String.Empty;
            return comment.InnerText.Trim();
            //return String.Empty;
        }
    }
}
