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
using System.Net;

namespace YoutubeCrawler.Utilities
{
    class ChannelComment
    {
        //videoCommentUserName = "//ul[@id='all-comments']//li//div[@class='content']//span[@class='author']/a"
        //time = "//ul[@id='all-comments']//li//div[@class='content']//span[@class='time']/a"
        //Comment Text = "//ul[@id='all-comments']//li//div[@class='content']/div[@class='comment-text']//p"
        //totalcomments = "//div[@id='comments-view']//h4"
        //dataId = ".//*[@id='all-comments']/li[1][@data-id]"
        //http://www.youtube.com/all_comments?v=
        public static int totalComments = 0;
        public static int pageNo = 1;
        public static string fileComment = ConfigurationManager.AppSettings["channelVideoComments"].ToString();
        public static int commentCount = 0;
        public static string parseAllComments = ConfigurationManager.AppSettings["ParseAllComments"].ToString();
        public static int totalCommentsParse = Int32.Parse(ConfigurationManager.AppSettings["parseComment"].ToString());
        public static string commentLogFile = ConfigurationManager.AppSettings["LogFilesComment"].ToString();
        public static Dictionary<string, VideoCommentWrapper> commentDictionary = new Dictionary<string, VideoCommentWrapper>();

        public static bool CrawlComments(Dictionary<string, VideoWrapper> videoDictionary, string pChannelName)
        {   
            foreach (KeyValuePair<string, VideoWrapper> pair in videoDictionary)
            {
                string videoFile = String.Empty;
                int pageNo = 1;
                
                VideoWrapper video = pair.Value;
                GetAllComments(video, pChannelName, pageNo);
                
                commentCount = 0;
            }
            
            return true;
        }

        public static void GetAllComments(VideoWrapper pVideoWrapper, string pChannelName, int pPageNo)
        {
            //Stream stream = File.OpenRead(textBox1.Text + "/N7KnEehYqxw.html");
            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //StreamReader reader = new StreamReader(stream);
            //doc.LoadHtml(reader.ReadToEnd().ToString());
            try
            {
                //string url = "http://www.youtube.com/all_comments?v=LMiNEC1M-zY" + "&page=" + pPageNo;
                string url = ConfigurationManager.AppSettings["VideoAllCommentsUrl"].ToString() + pVideoWrapper.getVideoKey() + "&page=" + pPageNo;
                HtmlWeb hwObject = new HtmlWeb();

                HtmlDocument doc = hwObject.Load(url);
                //WebRequest nameRequest = WebRequest.Create(url);
                //HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
                //Stream nameStream = nameResponse.GetResponseStream();
                //StreamReader nameReader = new StreamReader(nameStream);

                //string xmlData = nameReader.ReadToEnd();

                //File.WriteAllText(pChannelName + "/" + Common.CleanFileName(pVideoWrapper.getVideoKey()) + ".html", xmlData);
                
                
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

                HtmlNodeCollection totalCollection = doc.DocumentNode.SelectNodes("//ul[@id='all-comments']//li[@class='comment']");
                int totalCollectionCount = totalCollection.Count;
                if (totalCollectionCount <= 0)
                    return;
                for (int i = 0; i < totalCollectionCount; i++)
                {
                    commentCount++;
                    string dataId = GetDataId(i + 1, doc);
                    string authorId = GetAuthorId(i + 1, doc);
                    string displayName = GetUser(i + 1, doc);
                    string time = GetTime(i + 1, doc);
                    string comment = GetComment(i + 1, doc);
                    string userName = GetUserName(i + 1, doc);
                    comment = Common.FilterCommentText(comment);

                    if (!displayName.Equals("") && !time.Equals("") && !comment.Equals("") && !dataId.Equals("") && !authorId.Equals("") && !userName.Equals("") && !commentDictionary.ContainsKey(dataId))
                    {
                        VideoCommentWrapper commentWrapper = new VideoCommentWrapper();

                        commentWrapper.authorId = authorId;
                        commentWrapper.commentId = dataId;
                        commentWrapper.commentText = comment;
                        commentWrapper.time = time;
                        commentWrapper.displayName = displayName;
                        commentWrapper.userName = userName;

                        commentDictionary.Add(dataId, commentWrapper);

                        string videoName = pVideoWrapper.getVideoName();
                        //videoFile = videoName;
                        videoName = Common.CleanFileName(videoName + "-" + fileComment);
                        if (!Directory.Exists(pChannelName + "/" + "Comments"))
                        {
                            Directory.CreateDirectory(pChannelName + "/" + "Comments");
                        }
                        File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "User name : " + displayName + Environment.NewLine);
                        File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment Date : " + time + Environment.NewLine);
                        File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment : " + comment + Environment.NewLine);
                    }
                    if (parseAllComments.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (totalCommentsParse <= commentCount)
                            return;
                    }
                }
                pPageNo++;
                GetAllComments(pVideoWrapper, pChannelName, pageNo);
            }
            catch (Exception ex)
            {
                File.AppendAllText(pChannelName + "/" + Common.CleanFileName(commentLogFile), "Exception : " + ex.Message + Environment.NewLine);
                return;
            }
        }

        public static string GetUser(int index, HtmlDocument doc)
        {

            try
            {
                string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author ']");
                HtmlNode userName = doc.DocumentNode.SelectSingleNode(xPath);
                if (userName == null)
                {
                    xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author']");
                    userName = doc.DocumentNode.SelectSingleNode(xPath);
                }
                if(userName == null)
                    return String.Empty;
                return userName.InnerText.Trim();
            }
            catch (Exception ex)
            {
                File.AppendAllText(commentLogFile, "Exception in GetUser: " + ex.Message + Environment.NewLine);
                return String.Empty;
            }
        }
        public static string GetTime(int index, HtmlDocument doc)
        {
            try
            {
                string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='time']");
                HtmlNode time = doc.DocumentNode.SelectSingleNode(xPath);
                if (time == null)
                    return String.Empty;
                return time.InnerText.Trim();
            }
            catch (Exception ex)
            {
                File.AppendAllText(commentLogFile, "Exception in GetTime: " + ex.Message + Environment.NewLine);
                return String.Empty;
            }
        }
        public static string GetComment(int index, HtmlDocument doc)
        {
            try
            {
                string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "]//div[@class='content']/div[@class='comment-text']//p");
                HtmlNodeCollection col = doc.DocumentNode.SelectNodes(xPath);
                StringBuilder str = new StringBuilder();
                foreach (HtmlNode comment in col)
                {
                    str.Append(comment.InnerText.Trim());
                }
                if (str.ToString() == null)
                    return String.Empty;
                return str.ToString();
            }
            catch (Exception ex)
            {
                File.AppendAllText(commentLogFile, "Exception in GetComment : " + ex.Message + Environment.NewLine);
                return String.Empty;
            }
        }
        public static string GetDataId(int index, HtmlDocument doc)
        {
            try
            {
                string xPath = Common.CleanXpath(".//*[@id='all-comments']/li[" + index + "]");
                HtmlNode dataIdNode = doc.DocumentNode.SelectSingleNode(xPath);
                string dataId = dataIdNode.Attributes["data-id"].Value;
                if (dataId == null || dataId == "")
                    return String.Empty;
                return dataId;
            }
            catch (Exception ex)
            {
                File.AppendAllText(commentLogFile, "Exception in GetDataId: " + ex.Message + Environment.NewLine);
                return String.Empty;
            }
        }
        public static string GetAuthorId(int index, HtmlDocument doc)
        {
            try
            {
                string xPath = Common.CleanXpath(".//*[@id='all-comments']/li[" + index + "]");
                HtmlNode dataIdNode = doc.DocumentNode.SelectSingleNode(xPath);
                string dataAuthorId = dataIdNode.Attributes["data-author-id"].Value;
                if (dataAuthorId == null || dataAuthorId == "")
                    return String.Empty;
                return dataAuthorId;
            }
            catch (Exception ex)
            {
                File.AppendAllText(commentLogFile, "Exception in GetAuthorId: " + ex.Message + Environment.NewLine);
                return String.Empty;
            }
        }
        public static string GetUserName(int index, HtmlDocument doc)
        {
            try
            {
                string userNameValue = String.Empty;
                string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author ']//a");
                HtmlNode userName = doc.DocumentNode.SelectSingleNode(xPath);
                if (userName == null)
                {
                    xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author']//a");
                    userName = doc.DocumentNode.SelectSingleNode(xPath);
                }
                if (userName == null)
                    return String.Empty;
                else
                {
                    userNameValue = userName.Attributes["href"].Value;
                }
                if (userNameValue == null || userNameValue == "")
                {
                    return String.Empty;
                }
                string[] user = userNameValue.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                return user[user.Length - 1];
            }
            catch (Exception ex)
            {
                File.AppendAllText(commentLogFile, "Exception in GetUserName : " + ex.Message + Environment.NewLine);
                return String.Empty;
            }
        }
    }
}
