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
        //public static Dictionary<string, VideoCommentWrapper> commentDictionary = new Dictionary<string, VideoCommentWrapper>();
        

        public static bool CrawlComments(Dictionary<string, VideoWrapper> videoDictionary, string pChannelName)
        {   Dictionary<int, string> htmlFiles = null;
            foreach (KeyValuePair<string, VideoWrapper> pair in videoDictionary)
            {
                string videoFile = String.Empty;
                int pageNo = 1;
                
                VideoWrapper video = pair.Value;
                htmlFiles = new Dictionary<int, string>();
                DownloadHtmls(pChannelName, video, htmlFiles, pageNo);

                GetAllComments(video, pChannelName, htmlFiles);
                commentCount = 0;
            }
            
            return true;
        }

        public static void DownloadHtmls(string pChannelName, VideoWrapper pVideo, Dictionary<int, string> pHtmlFiles, int pPageNo)
        {
            string url = ConfigurationManager.AppSettings["VideoAllCommentsUrl"].ToString() + pVideo.getVideoKey() + "&page=" + pPageNo;
            //string url = "http://www.youtube.com/all_comments?v=LMiNEC1M-zY" + "&page=" + pPageNo;
            ///Base Case
            ///
            HtmlWeb hwObject = new HtmlWeb();
            HtmlDocument doc = hwObject.Load(url);
            HtmlNodeCollection totalCollection = doc.DocumentNode.SelectNodes("//ul[@id='all-comments']//li[@class='comment']");
            if (totalCollection == null)
                return;
            int totalCollectionCount = totalCollection.Count;
            if (totalCollectionCount <= 0)
                return;
            ///Base Case Ended
            ///

            WebRequest nameRequest = WebRequest.Create(url);
            HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
            Stream nameStream = nameResponse.GetResponseStream();
            StreamReader nameReader = new StreamReader(nameStream);
            string htmlData = nameReader.ReadToEnd();
            if (htmlData != null && !htmlData.Equals(""))
            {
                string videoName = pChannelName + "/Comments/" + Common.CleanFileName(pVideo.getVideoName()) + "-" + pPageNo + ".html";
                string dictionaryValue = Common.CleanFileName(pVideo.getVideoName()) + "-" + pPageNo + ".html";
                if (!Directory.Exists(pChannelName + "/Comments/"))
                {
                    Directory.CreateDirectory(pChannelName + "/Comments/");
                }
                File.WriteAllText(videoName, htmlData);
                //tempFiles.Add("/Comments/" + dictionaryValue);
                pHtmlFiles.Add(pPageNo, dictionaryValue);
            }
            pPageNo++;

            //DownloadHtmls(pChannelName, pVideo, pHtmlFiles, pPageNo);   //Recursive Call
        }

        public static void GetAllComments(VideoWrapper pVideoWrapper, string pChannelName, Dictionary<int, string> pHtmlFiles)
        {
            string videoUrl = "https://www.youtube.com/watch?v=" + pVideoWrapper.getVideoKey();
            bool videoUrlFlag = false;
            foreach (KeyValuePair<int, string> pair in pHtmlFiles)
            {
                try
                {
                    List<string> tempFiles = new List<string>();
                    string videoName = pair.Value;
                    //string videoName = "Machinima PlayStation Viewer's Choice LiveStream!-1";
                    //Stream stream = File.OpenRead("New folder/Machinima PlayStation Viewer's Choice LiveStream!-1.html");
                    Stream stream = File.OpenRead(pChannelName + "/Comments/" + videoName);
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    StreamReader reader = new StreamReader(stream);
                    doc.LoadHtml(reader.ReadToEnd().ToString());
                    bool breakLoop = false;

                    HtmlNodeCollection totalCollection = doc.DocumentNode.SelectNodes("//ul[@id='all-comments']//li[@class='comment']");
                    int i = 1;
                    foreach (HtmlNode node in totalCollection)
                    {
                        try
                        {
                            string dataId = GetDataId(i, doc);
                            string authorId = GetAuthorId(i, doc);
                            string displayName = GetUser(i, doc);
                            string time = GetTime(i, doc);
                            string comment = GetComment(i, doc);
                            string userName = GetUserName(i, doc);
                            comment = Common.FilterCommentText(comment);

                            //Deleted time check as sometime "time is not shown on videos comments.."

                            if (!displayName.Equals("") && !comment.Equals("") && !dataId.Equals("") && !authorId.Equals("") && !userName.Equals("") && !GlobalConstants.commentDictionary.ContainsKey(dataId))
                            {
                                VideoCommentWrapper commentWrapper = new VideoCommentWrapper();

                                commentWrapper.authorId = authorId;
                                commentWrapper.commentId = dataId;
                                commentWrapper.commentText = comment;
                                commentWrapper.time = time;
                                commentWrapper.displayName = displayName;
                                commentWrapper.userName = userName;

                                GlobalConstants.commentDictionary.Add(dataId, commentWrapper);

                                string videoFileName = pVideoWrapper.getVideoName();
                                //videoFile = videoName;
                                videoName = Common.CleanFileName(videoFileName + "-" + fileComment) + ".txt";
                                if (!Directory.Exists(pChannelName + "/" + "Comments"))
                                {
                                    Directory.CreateDirectory(pChannelName + "/" + "Comments");
                                }
                                commentCount++;
                                if (!videoUrlFlag)
                                {
                                    File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Video Url : " + videoUrl + Environment.NewLine + "\r\n");
                                    videoUrlFlag = true;
                                }
                                File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "User name : " + displayName + Environment.NewLine);
                                File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment Date : " + time + Environment.NewLine);
                                File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment : " + comment + Environment.NewLine);
                            }
                            if (parseAllComments.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (totalCommentsParse <= commentCount)
                                {
                                    breakLoop = true;
                                    break;
                                }
                            }
                            i++;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText("Logs Exception Comments.txt", ex.Message + Environment.NewLine + Environment.NewLine);
                            continue;
                        }
                    }
                    reader.Close();
                    //foreach (KeyValuePair<int, string> file in pHtmlFiles)
                    //{
                    //    tempFiles.Add("/Comments/" + file.Value);
                    //}
                    //Common.RemoveTempFiles(tempFiles, pChannelName);
                    if (breakLoop)
                        break;
                }
                catch (Exception ex)
                {
                    File.AppendAllText("Logs Exception Comments.txt", ex.Message + Environment.NewLine + Environment.NewLine);
                    continue;
                }
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
                if (col == null)
                    return String.Empty;
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
