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
using System.Threading;

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
        public static int QueueLength = 0;
        private static bool WaitForComplete;
        private static ManualResetEvent Event;
        public static int totalComments = 0;
        public static int pageNo = 1;
        public static string fileComment = ConfigurationManager.AppSettings["channelVideoComments"].ToString();
        public static int commentCount = 0;
        public static string parseAllComments = ConfigurationManager.AppSettings["ParseAllComments"].ToString();
        public static int totalCommentsParse = Int32.Parse(ConfigurationManager.AppSettings["parseComment"].ToString());
        public static string commentLogFile = ConfigurationManager.AppSettings["LogFilesComment"].ToString();
        //public static Dictionary<string, VideoCommentWrapper> commentDictionary = new Dictionary<string, VideoCommentWrapper>();
        public static string channelName = string.Empty;

        public static void Produce(VideoWrapper videoDictionary)
        {
            ThreadPool.QueueUserWorkItem(
            new WaitCallback(Consume), videoDictionary);
            QueueLength++;
            Console.WriteLine("producing" + QueueLength);
        }

        public void Wait()
        {
            if (QueueLength == 0)
            {
                return;
            }
            Event = new ManualResetEvent(false);
            WaitForComplete = true;
            Event.WaitOne();
        }
        public static void Consume(Object obj)
        {
            try
            {
                string videoFile = String.Empty;
                int pageNo = 1;
                Dictionary<int, string> htmlFiles = null;
                htmlFiles = new Dictionary<int, string>();

                //VideoWrapper video = pair.Value;

                VideoWrapper video = obj as VideoWrapper;
                

                htmlFiles = new Dictionary<int, string>();

                DownloadHtmls(channelName, video, htmlFiles, pageNo);

         //       GetAllComments(video, pChannelName, htmlFiles);
                commentCount = 0;
                QueueLength--;
                Console.WriteLine("consuming" + QueueLength);
                if (WaitForComplete)
                {
                    if (QueueLength == 0)
                    {
                        Event.Set();
                    }
                }
                //break;
            }
            catch (Exception ex)
            {
                //   continue;
            }
        }
        
        public static bool CrawlComments(Dictionary<string, VideoWrapper> videoDictionary, string pChannelName)
        {
            channelName = pChannelName;
            //Dictionary<int, string> htmlFiles = null;
            //if(File.Exists("ThreadsLog.txt"))
            //{
            //    File.Delete("ThreadsLog.txt");
            //}
            //File.AppendAllText("CommentsTime.txt", "Time Start : " + DateTime.Now);
            int totalThreads = Int32.Parse(ConfigurationManager.AppSettings["totalThreadsAtOneTime"].ToString());
            foreach (KeyValuePair<string, VideoWrapper> pair in videoDictionary)
            {
                try
                {

                    //string videoFile = String.Empty;
                    //int pageNo = 1;

                    VideoWrapper video = pair.Value;
                    Produce(video);
                    while (QueueLength >= totalThreads)
                        Thread.Sleep(2000);
                    //htmlFiles = new Dictionary<int, string>();
                    //DownloadHtmls(pChannelName, video, htmlFiles, pageNo);

                    ////GetAllComments(video, pChannelName, htmlFiles);
                    //commentCount = 0;
                    ////break;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            while (QueueLength > 0)
            {
                Thread.Sleep(1000);
            }
            //File.AppendAllText("CommentsTime.txt", "Time End : " + DateTime.Now);
            
            return true;
        }

        public static void DownloadHtmls(string pChannelName, VideoWrapper pVideo, Dictionary<int, string> pHtmlFiles, int pPageNo)
        {
            string url = string.Empty;
            try
            {
                url = ConfigurationManager.AppSettings["VideoAllCommentsUrl"].ToString() + pVideo.getVideoKey() + "&page=" + pPageNo;
                //string url = "http://www.youtube.com/all_comments?v=LMiNEC1M-zY" + "&page=" + pPageNo;
                ///Base Case
                ///
                HtmlWeb hwObject = new HtmlWeb();
                //hwObject.UseCookies = false; // Experimental
                //File.AppendAllText("ThreadsLog.txt", "Thread " + Thread.CurrentThread.GetHashCode() + " going to hit URL at page # " + pPageNo + ".. " + DateTime.Now + Environment.NewLine);
                HtmlDocument doc = hwObject.Load(url);
                //File.AppendAllText("ThreadsLog.txt", "Thread " + Thread.CurrentThread.GetHashCode() + " got response of page # " + pPageNo + ".." + DateTime.Now + Environment.NewLine);

                HtmlNodeCollection totalCollection = doc.DocumentNode.SelectNodes("//ul[@id='all-comments']//li[@class='comment']");
                if (totalCollection == null)
                    return;
                int totalCollectionCount = totalCollection.Count;
                if (totalCollectionCount <= 0)
                    return;
                ///Base Case Ended
                ///

                //Code Added by Me Right Now ....
                ///

                totalCollection = doc.DocumentNode.SelectNodes("//ul[@id='all-comments']//li[@class='comment']//div[@class='content']");
                string videoUrl = "https://www.youtube.com/watch?v=" + pVideo.getVideoKey();
                bool videoUrlFlag = false;
                bool breakLoop = false;
                //File.AppendAllText("ThreadsLog.txt", "Thread " + Thread.CurrentThread.GetHashCode() + " starting to extract data.." + Environment.NewLine);
                foreach (HtmlNode node in totalCollection)
                {
                    //string[] userArr = node.InnerText.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    string user = string.Empty;
                    string displayName = string.Empty;
                    string date = string.Empty;
                    string comment = string.Empty;
                    HtmlNode nodeData = node.ParentNode;
                    string dataId = nodeData.Attributes[2].Value.Trim();
                    string authorId = nodeData.Attributes[1].Value.Trim();
                    HtmlNodeCollection childNodes = node.ChildNodes;
                    int divCount = 0;
                    foreach (HtmlNode child in childNodes)
                    {
                        if (child.Name.Equals("p"))
                        {
                            bool userFlag = false;
                            //bool dateFlag = false;
                            HtmlNodeCollection col = child.ChildNodes;
                            foreach (HtmlNode n in col)
                            {
                                if (n.Name.Equals("span") && !userFlag)
                                {
                                    foreach (HtmlNode nNode in n.ChildNodes)
                                    {
                                        if (nNode.Name.Equals("a"))
                                        {
                                            user = nNode.Attributes["href"].Value.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
                                            break;
                                        }
                                    }
                                    displayName = n.InnerText.Trim();
                                    userFlag = true;
                                }
                                else if (n.Name.Equals("span"))
                                {
                                    date = n.InnerText.Trim();
                                    //dateFlag = true;
                                    break;
                                }
                            }
                        }
                        else if (child.Name.Equals("div"))
                        {
                            if (divCount == 0)
                            {
                                //That means Its Comment Text
                                comment = child.InnerText.Trim();
                                divCount++;
                            }
                        }
                    }
                    
                    //File.AppendAllText("ThreadsLog.txt", "Thread " + Thread.CurrentThread.GetHashCode() + " starting to write data in file.." + Environment.NewLine);
                    if (!displayName.Equals("") && !comment.Equals("") && !dataId.Equals("") && !authorId.Equals("") && !user.Equals("") && !GlobalConstants.commentDictionary.ContainsKey(dataId))
                    {
                        VideoCommentWrapper commentWrapper = new VideoCommentWrapper();

                        commentWrapper.authorId = authorId;
                        commentWrapper.commentId = dataId;
                        commentWrapper.commentText = comment;
                        commentWrapper.time = date;
                        commentWrapper.displayName = displayName;
                        commentWrapper.userName = user;

                        GlobalConstants.commentDictionary.Add(dataId, commentWrapper);

                        string videoFileName = pVideo.getVideoName();
                        //videoFile = videoName;
                        string videoName = Common.CleanFileName(videoFileName + "-" + fileComment) + ".txt";
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
                        File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment Date : " + date + Environment.NewLine);
                        File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment : " + comment + Environment.NewLine);
                    }
                    //File.AppendAllText("ThreadsLog.txt", "Thread " + Thread.CurrentThread.GetHashCode() + " ended writing data in file.." + Environment.NewLine);
                    if (parseAllComments.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (totalCommentsParse <= commentCount)
                        {
                            breakLoop = true;
                            break;
                        }
                    }

                }
                //File.AppendAllText("ThreadsLog.txt", "Thread " + Thread.CurrentThread.GetHashCode() + " extracted all data.." + Environment.NewLine);
                ////Ended Added

                ////Commented by Me


                //File.AppendAllText(pChannelName + "/CommentsTimeLog.txt", "Start Download Time for file : " + pVideo.getVideoName() + "-" + pPageNo + ": " + DateTime.Now + Environment.NewLine);
                //WebRequest nameRequest = WebRequest.Create(url);
                //HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
                //Stream nameStream = nameResponse.GetResponseStream();
                //StreamReader nameReader = new StreamReader(nameStream);
                //string htmlData = nameReader.ReadToEnd();
                //if (htmlData != null && !htmlData.Equals(""))
                //{
                //    string videoName = pChannelName + "/Comments/" + Common.CleanFileName(pVideo.getVideoName()) + "-" + pPageNo + ".html";
                //    string dictionaryValue = Common.CleanFileName(pVideo.getVideoName()) + "-" + pPageNo + ".html";
                //    if (!Directory.Exists(pChannelName + "/Comments/"))
                //    {
                //        Directory.CreateDirectory(pChannelName + "/Comments/");
                //    }

                //    File.WriteAllText(videoName, htmlData);
                //    File.AppendAllText(pChannelName + "/CommentsTimeLog.txt", "End Download Time for file : " + pVideo.getVideoName() + "-" + pPageNo + ": " + DateTime.Now + Environment.NewLine + Environment.NewLine);
                //    //tempFiles.Add("/Comments/" + dictionaryValue);
                //    pHtmlFiles.Add(pPageNo, dictionaryValue);
                //}

                ////Comment Ended

                pPageNo++;
                if(parseAllComments.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    DownloadHtmls(pChannelName, pVideo, pHtmlFiles, pPageNo);   //Recursive Call
            }
            catch (Exception ex)
            {
                //Delete Cookies
                //pPageNo++;
                //File.AppendAllText(pChannelName + "/Comments/" + "ExceptionLogs.txt", "Exception : at URL : " + url + " -> Exception Message : " + ex.Message);
                DownloadHtmls(pChannelName, pVideo, pHtmlFiles, pPageNo);
            }
        }

        public static void GetAllComments(VideoWrapper pVideoWrapper, string pChannelName, Dictionary<int, string> pHtmlFiles)
        {
            string videoUrl = "https://www.youtube.com/watch?v=" + pVideoWrapper.getVideoKey();
            bool videoUrlFlag = false;
            List<string> tempFiles = new List<string>();
            foreach (KeyValuePair<int, string> pair in pHtmlFiles)
            {
                try
                {
                    
                    string videoName = pair.Value;
                    //string videoName = "Machinima PlayStation Viewer's Choice LiveStream!-1";
                    //Stream stream = File.OpenRead("New folder/Machinima PlayStation Viewer's Choice LiveStream!-1.html");
                    Stream stream = File.OpenRead(pChannelName + "/Comments/" + videoName);
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    StreamReader reader = new StreamReader(stream);
                    doc.LoadHtml(reader.ReadToEnd().ToString());
                    bool breakLoop = false;

                    HtmlNodeCollection totalCollection = doc.DocumentNode.SelectNodes("//ul[@id='all-comments']//li[@class='comment']//div[@class='content']");
                    
            
                    foreach (HtmlNode node in totalCollection)
                    {
                        //string[] userArr = node.InnerText.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        string user = string.Empty;
                        string displayName = string.Empty;
                        string date = string.Empty;
                        string comment = string.Empty;
                        HtmlNode nodeData = node.ParentNode;
                        string dataId = nodeData.Attributes[2].Value.Trim();
                        string authorId = nodeData.Attributes[1].Value.Trim();
                        HtmlNodeCollection childNodes = node.ChildNodes;
                        int divCount = 0;
                        foreach (HtmlNode child in childNodes)
                        {
                            if (child.Name.Equals("p"))
                            {
                                bool userFlag = false;
                                //bool dateFlag = false;
                                HtmlNodeCollection col = child.ChildNodes;
                                foreach (HtmlNode n in col)
                                {
                                    if (n.Name.Equals("span") && !userFlag)
                                    {
                                        foreach (HtmlNode nNode in n.ChildNodes)
                                        {
                                            if (nNode.Name.Equals("a"))
                                            {
                                                user = nNode.Attributes["href"].Value.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
                                                break;
                                            }
                                        }
                                        displayName = n.InnerText.Trim();
                                        userFlag = true;
                                    }
                                    else if (n.Name.Equals("span"))
                                    {
                                        date = n.InnerText.Trim();
                                        //dateFlag = true;
                                        break;
                                    }
                                }
                            }
                            else if (child.Name.Equals("div"))
                            {
                                if (divCount == 0)
                                {
                                    //That means Its Comment Text
                                    comment = child.InnerText.Trim();
                                    divCount++;
                                }
                            }   
                        }
                        if (!displayName.Equals("") && !comment.Equals("") && !dataId.Equals("") && !authorId.Equals("") && !user.Equals("") && !GlobalConstants.commentDictionary.ContainsKey(dataId))
                        {
                            VideoCommentWrapper commentWrapper = new VideoCommentWrapper();

                            commentWrapper.authorId = authorId;
                            commentWrapper.commentId = dataId;
                            commentWrapper.commentText = comment;
                            commentWrapper.time = date;
                            commentWrapper.displayName = displayName;
                            commentWrapper.userName = user;

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
                            File.AppendAllText(pChannelName + "/" + "Comments" + "/" + videoName, "Comment Date : " + date + Environment.NewLine);
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

                    }
                    reader.Close();
                    
                    
                    if (breakLoop)
                        break;
                }
                catch (Exception ex)
                {
                    //File.AppendAllText("Logs Exception Comments.txt", ex.Message + Environment.NewLine + Environment.NewLine);
                    continue;
                }
            }
            foreach (KeyValuePair<int, string> file in pHtmlFiles)
            {
                tempFiles.Add("/Comments/" + file.Value);
            }

            Common.RemoveTempFiles(tempFiles, pChannelName);
        }

        //public static string GetUser(int index, HtmlDocument doc)
        //{
        //    try
        //    {
        //        string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author ']");
        //        HtmlNode userName = doc.DocumentNode.SelectSingleNode(xPath);
        //        if (userName == null)
        //        {
        //            xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author']");
        //            userName = doc.DocumentNode.SelectSingleNode(xPath);
        //        }
        //        if(userName == null)
        //            return String.Empty;
        //        return userName.InnerText.Trim();
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(commentLogFile, "Exception in GetUser: " + ex.Message + Environment.NewLine);
        //        return String.Empty;
        //    }
        //}
        //public static string GetTime(int index, HtmlDocument doc)
        //{
        //    try
        //    {
        //        string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='time']");
        //        HtmlNode time = doc.DocumentNode.SelectSingleNode(xPath);
        //        if (time == null)
        //            return String.Empty;
        //        return time.InnerText.Trim();
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(commentLogFile, "Exception in GetTime: " + ex.Message + Environment.NewLine);
        //        return String.Empty;
        //    }
        //}
        //public static string GetComment(int index, HtmlDocument doc)
        //{
        //    try
        //    {
        //        string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "]//div[@class='content']/div[@class='comment-text']//p");
        //        HtmlNodeCollection col = doc.DocumentNode.SelectNodes(xPath);
        //        StringBuilder str = new StringBuilder();
        //        if (col == null)
        //            return String.Empty;
        //        foreach (HtmlNode comment in col)
        //        {
        //            str.Append(comment.InnerText.Trim());
        //        }
        //        if (str.ToString() == null)
        //            return String.Empty;
        //        return str.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(commentLogFile, "Exception in GetComment : " + ex.Message + Environment.NewLine);
        //        return String.Empty;
        //    }
        //}
        //public static string GetDataId(int index, HtmlDocument doc)
        //{
        //    try
        //    {
        //        string xPath = Common.CleanXpath(".//*[@id='all-comments']/li[" + index + "]");
        //        HtmlNode dataIdNode = doc.DocumentNode.SelectSingleNode(xPath);
        //        string dataId = dataIdNode.Attributes["data-id"].Value;
        //        if (dataId == null || dataId == "")
        //            return String.Empty;
        //        return dataId;
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(commentLogFile, "Exception in GetDataId: " + ex.Message + Environment.NewLine);
        //        return String.Empty;
        //    }
        //}
        //public static string GetAuthorId(int index, HtmlDocument doc)
        //{
        //    try
        //    {
        //        string xPath = Common.CleanXpath(".//*[@id='all-comments']/li[" + index + "]");
        //        HtmlNode dataIdNode = doc.DocumentNode.SelectSingleNode(xPath);
        //        string dataAuthorId = dataIdNode.Attributes["data-author-id"].Value;
        //        if (dataAuthorId == null || dataAuthorId == "")
        //            return String.Empty;
        //        return dataAuthorId;
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(commentLogFile, "Exception in GetAuthorId: " + ex.Message + Environment.NewLine);
        //        return String.Empty;
        //    }
        //}
        //public static string GetUserName(int index, HtmlDocument doc)
        //{
        //    try
        //    {
        //        string userNameValue = String.Empty;
        //        string xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author ']//a");
        //        HtmlNode userName = doc.DocumentNode.SelectSingleNode(xPath);
        //        if (userName == null)
        //        {
        //            xPath = Common.CleanXpath("//ul[@id='all-comments']//li[" + index + "][@class='comment']//div[@class='content']//p[@class='metadata']//span[@class='author']//a");
        //            userName = doc.DocumentNode.SelectSingleNode(xPath);
        //        }
        //        if (userName == null)
        //            return String.Empty;
        //        else
        //        {
        //            userNameValue = userName.Attributes["href"].Value;
        //        }
        //        if (userNameValue == null || userNameValue == "")
        //        {
        //            return String.Empty;
        //        }
        //        string[] user = userNameValue.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        //        return user[user.Length - 1];
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText(commentLogFile, "Exception in GetUserName : " + ex.Message + Environment.NewLine);
        //        return String.Empty;
        //    }
        //}
    }
}
