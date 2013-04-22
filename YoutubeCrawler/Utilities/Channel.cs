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
using System.IO;
using System.Xml;
using System.Net;

namespace YoutubeCrawler.Utilities
{
    class Channel
    {
        public static string channelAtomEntry = "//Atom:entry";
        public static string channelTitleXPath = "//Atom:entry/Atom:author";
        public static string channelName = "";
        public static string channelId = "";
        public static int startIndex = 1;
        public static int recordCount = 0;
        public static string log = ConfigurationManager.AppSettings["LogFiles"].ToString();
        public static string channelUrlMain = string.Empty;
        public static int lastLevel = 0;
        public static Dictionary<string, VideoCommentWrapper> dictionary;
        public static bool updatedFlag = false;
        //public static List<string> tempFiles = new List<string>();
        public static int exceptionCounter = 0;
        public static bool ParseChannel(string pChannelName, string pAppName, string pDevKey, int pLevel)
        {
            ///ReInitialize All Data
            ///
            channelName = "";
            channelId = "";
            startIndex = 1;
            recordCount = 0;
            updatedFlag = false;
            ///ReInitializing Done
            ///

            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();
            //File.AppendAllText(pChannelName + "/" + log, "Entered Inside Parse Channel at : " + DateTime.Now + Environment.NewLine + Environment.NewLine);

            //This Request will give us 10 channels from index 1, which is searched by adding its name.

            //e.g.https://gdata.youtube.com/feeds/api/channels?q=" + pChannelName + "&start-index=1&max-results=10&v=2

            //q=<Channel Name>
            //start-index = <start Index of Search result> (by default 1st Index is '1')
            //max-result = <page size (containing number of channels)>
            //v = Not known yet. :S

            //e.g.https://gdata.youtube.com/feeds/api/channels?q=" + pChannelName + "&start-index=1&max-results=10&v=2
            string channelUrl = ConfigurationManager.AppSettings["ChannelSearchUrl"].ToString() + pChannelName + "&start-index=1&max-results=10&v=2";
            WebRequest nameRequest = WebRequest.Create(channelUrl);
            HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();

            Stream nameStream = nameResponse.GetResponseStream();
            StreamReader nameReader = new StreamReader(nameStream);

            string xmlData = nameReader.ReadToEnd();

            File.WriteAllText(pChannelName + "/" + channelFileNameXML, xmlData);

            XmlDocument doc = new XmlDocument();
            doc.Load(pChannelName + "/" + channelFileNameXML);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");

            XmlNodeList listResult = doc.SelectNodes(channelTitleXPath, namespaceManager);
            int count = 0;
            foreach (XmlNode node in listResult)
            {
                count++;
                if (node.FirstChild.InnerText.Equals(pChannelName, StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
            XmlNodeList entryNode = doc.SelectSingleNode(channelAtomEntry + "[" + count + "]", namespaceManager).ChildNodes;
            foreach (XmlNode n in entryNode)
            {
                if (n.Name.Equals("title"))
                {
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Channel Name: " + n.InnerText + "\r\n");
                }
                else if (n.Name.Equals("yt:channelStatistics"))
                {
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Subscribers Count: " + n.Attributes["subscriberCount"].Value + "\r\n");
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Views Count: " + n.Attributes["viewCount"].Value + "\r\n");
                }
                else if (n.Name.Equals("link"))
                {
                    if (n.Attributes["rel"].Value.Equals("alternate", StringComparison.CurrentCultureIgnoreCase))
                    {
                        File.AppendAllText(pChannelName + "/" + channelFileName, "Channel URL: " + n.Attributes["href"].Value + "\r\n");
                        channelUrlMain = n.Attributes["href"].Value;
                    }
                }
                else if (n.Name.Equals("id"))
                {
                    string id = n.InnerText;
                    string[] arrId = n.InnerText.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    bool indexFound = false;
                    for (int i = 0; i < arrId.Length; i++)
                    {
                        if (arrId[i].Equals("Channel", StringComparison.CurrentCultureIgnoreCase))
                        {
                            indexFound = true;
                            continue;
                        }
                        if (indexFound)
                        {
                            channelId = arrId[i];
                            break;
                        }
                    }
                }
            }
            //"AI39si7SUChDwy6-ms_bz7rY-mzkqWc9vouhT_XZfh_xery5HjOujHc4USzQJ-M6XeWPCmGtaMzBgs3QP5S4O3vFBHoxmCfIjA"
            YouTubeRequestSettings settings = new YouTubeRequestSettings(pAppName, pDevKey);
            YouTubeRequest request = new YouTubeRequest(settings);

            Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/users/" + pChannelName);
            Feed<Video> videoFeed = request.Get<Video>(videoEntryUrl);
            foreach (Entry e in videoFeed.Entries)
            {
                File.AppendAllText(pChannelName + "/" + channelFileName, "Channel Description: " + e.Summary + "\r\n");
                break;
            }

            Constant.tempFiles.Add(channelFileNameXML);
            //Constant.tempFiles.Add(pChannelName + "/" + log);
            Dictionary<string, VideoWrapper> videoDictionary = new Dictionary<string, VideoWrapper>();

            File.AppendAllText(pChannelName + "/" + channelFileName, "Video Lists \r\n");

            startIndex = 1;

            //File.AppendAllText(pChannelName + "/" + log, "\tEntering WriteVideoList at: " + DateTime.Now + Environment.NewLine + Environment.NewLine);

            WriteVideoLists(pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.All);

            //File.AppendAllText(pChannelName + "/" + log, "\t\tTotal Dictionary Items : " + videoDictionary.Count + Environment.NewLine);
            //File.AppendAllText(pChannelName + "/" + log, "\r\n\tLeft WriteVideoList at: " + DateTime.Now + Environment.NewLine + Environment.NewLine);
            //File.AppendAllText(pChannelName + "/" + "Count.txt", "Count After complete Request Response (Expected 1000) : " + recordCount + "\r\n");

            //File.AppendAllText(pChannelName + "/" + log, "Leaving Parse Channel at : " + DateTime.Now);

            
            ChannelVideo.parseVideo(videoDictionary, pChannelName);
            ///Done Crawling video description

            ///Crawl Comments
            ///
            ChannelComment.CrawlComments(videoDictionary, pChannelName);
            ///Done Crawling Comments
            ///
            ///Remove All Temporary Files here
            ///
            Common.RemoveTempFiles(Constant.tempFiles, pChannelName);
            ///Done
            ///

            //Commenting Temporarily on Paolo Request.. <04/19/2013>

            /////ReInitialize All Data
            /////
            //channelName = "";
            //channelId = "";
            //startIndex = 1;
            //recordCount = 0;
            //updatedFlag = false;
            /////ReInitializing Done

            //dictionary = new Dictionary<string, VideoCommentWrapper>();
            //dictionary = GlobalConstants.commentDictionary;
            //int testCount = 0;
            //foreach (KeyValuePair<string, VideoCommentWrapper> pair in dictionary)
            //{
            //    GlobalConstants.commentDictionary = new Dictionary<string, VideoCommentWrapper>();
            //    VideoCommentWrapper videoComment = pair.Value;
            //    ParseChannelLevel2(videoComment, pAppName, pDevKey);
            //    testCount++;
            //    //if (testCount > 3)
            //    //    break;
            //}

            return true;
        }

        public static void WriteVideoLists(string pChannelName, string pChannelId, int startIndex, Dictionary<string, VideoWrapper> videoDictionary, Enumeration.VideoRequestType requestType)
        {
            try
            {
                //Base Case of Recursion
                //if (startIndex >= 1000)
                //    return;
                //Base Case Ended of Recursion
                string videoName = String.Empty;
                string videoUrl = String.Empty;
                //string url = String.Empty;
                string videoId = String.Empty;

                string videoFileName = ConfigurationManager.AppSettings["channelsVideoFile"].ToString();
                string videFileNameXML = ConfigurationManager.AppSettings["channelsVideoFileXML"].ToString();
                string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
                string channelCleanedName = Common.CleanFileName(pChannelName);
                string channelUrl = String.Empty;
                if (requestType == Enumeration.VideoRequestType.All)
                {
                    //http://gdata.youtube.com/feeds/api/users/machinima/uploads?start-index=4000
                    channelUrl = "https://gdata.youtube.com/feeds/api/users/" + pChannelName + "/uploads?&start-index=" + startIndex;
                }


                HttpWebRequest nameRequest = (HttpWebRequest)WebRequest.Create(channelUrl);
                nameRequest.KeepAlive = false;
                nameRequest.ProtocolVersion = HttpVersion.Version10;
                HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();

                Stream nameStream = nameResponse.GetResponseStream();
                StreamReader nameReader = new StreamReader(nameStream);

                string xmlData = nameReader.ReadToEnd();
                File.WriteAllText(channelCleanedName + "/" + videFileNameXML, xmlData);

                XmlDocument doc = new XmlDocument();
                doc.Load(channelCleanedName + "/" + videFileNameXML);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");
                XmlNodeList listResult = doc.SelectNodes(channelAtomEntry, namespaceManager);
                
                ////Getting total Record
                XmlNamespaceManager namespaceManager1 = new XmlNamespaceManager(doc.NameTable);
                namespaceManager1.AddNamespace("openSearch", "http://a9.com/-/spec/opensearchrss/1.0/");

                XmlNode nodeTotal = doc.SelectSingleNode("//openSearch:totalResults", namespaceManager1);
                int total = Int32.Parse(nodeTotal.InnerText);
                
                //Base Case Started

                if (total == 0)
                    return;

                if (listResult == null)
                    return;

                string flag = ConfigurationManager.AppSettings["testingFlag"].ToString();
                if (flag.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (startIndex >= 26)
                    {
                        Constant.tempFiles.Add(videFileNameXML);
                        return;
                    }
                }
                else
                {
                    if (ConfigurationManager.AppSettings["ExtractAllVideosFlag"].ToString().Equals("False", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int totalVideo = Int32.Parse(ConfigurationManager.AppSettings["totalVideos"].ToString());
                        if (totalVideo <= recordCount)
                        {
                            Constant.tempFiles.Add(videFileNameXML);
                            return;
                        }
                    }
                    else
                    {
                        if (total <= startIndex)
                        {
                            Constant.tempFiles.Add(videFileNameXML);
                            return;
                        }
                    }
                }

                //Base Case Ended

                //File.AppendAllText(channelCleanedName + "/" + log, "\t\tTotal Record : " + total + "; Start Index : " + startIndex + Environment.NewLine);
                foreach (XmlNode entry in listResult)
                {
                    bool idFound = false;
                    bool titleFound = false;
                    foreach (XmlNode node in entry.ChildNodes)
                    {
                        if (node.Name.Equals("id"))
                        {
                            videoUrl = node.InnerText;
                            string id = videoUrl;
                            string[] arrId = id.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            videoId = arrId[arrId.Length - 1];
                            idFound = true;
                        }
                        else if (node.Name.Equals("title"))
                        {
                            videoName = node.InnerText;
                            titleFound = true;
                        }
                        if (idFound && titleFound)
                        {
                            if (videoDictionary != null && !videoDictionary.ContainsKey(videoId))
                            {
                                videoDictionary.Add(videoId, new VideoWrapper(videoName, videoId, videoUrl, channelUrlMain));
                                File.AppendAllText(channelCleanedName + "/" + channelFileName, "\t" + videoName + "\r\n");
                                recordCount++;
                            }
                            break;
                        }
                    }
                    int totalVideo = Int32.Parse(ConfigurationManager.AppSettings["totalVideos"].ToString());
                    if (totalVideo <= recordCount)
                    {
                        Constant.tempFiles.Add(videFileNameXML);
                        return;
                    }
                }
                startIndex += 25;
                if (requestType == Enumeration.VideoRequestType.All)
                {
                    WriteVideoLists(pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.All); //Recursive Call
                }
            }
            catch (Exception ex)
            {
                exceptionCounter++;
                if (exceptionCounter == 2)
                {
                    exceptionCounter = 0;
                    return;
                }
                //File.AppendAllText(Common.CleanFileName(pChannelName) + "/" + log, "\t\tException Found : " + ex.Message + Environment.NewLine + "startIndex = " + startIndex + Environment.NewLine + Environment.NewLine);
                startIndex += 25;
                if (requestType == Enumeration.VideoRequestType.All)
                {
                    WriteVideoLists(pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.All); //Recursive Call
                }
            }
        }

        public static void ParseChannelLevel2(VideoCommentWrapper commentWrapper, string pAppName, string pDevKey)
        {
            string channelName = commentWrapper.displayName;
            string userId = commentWrapper.userName;
            string channelCleanedName = Common.CleanFileName(channelName);
            if (!Directory.Exists(channelCleanedName))
            {
                Directory.CreateDirectory(channelCleanedName);
            }
            else
            {
                Directory.Delete(channelCleanedName, true);
                Directory.CreateDirectory(channelCleanedName);
            }

            //Search for Channel... 
            // If Channel doesn't exit Search for following things
            //1. Favourties, 2. Uploaded , 3. Playlist
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();
            //File.AppendAllText(channelCleanedName + "/" + log, "Entered Inside Parse Channel at : " + DateTime.Now + Environment.NewLine + Environment.NewLine);

            string videoUrl = "https://gdata.youtube.com/feeds/api/users/" + channelName + "/uploads?&start-index=" + startIndex;
            HttpWebRequest nameRequest1 = (HttpWebRequest)WebRequest.Create(videoUrl);
            nameRequest1.KeepAlive = false;
            nameRequest1.ProtocolVersion = HttpVersion.Version10;
            HttpWebResponse nameResponse1 = (HttpWebResponse)nameRequest1.GetResponse();

            Stream nameStream1 = nameResponse1.GetResponseStream();
            StreamReader nameReader1 = new StreamReader(nameStream1);

            string xmlData1 = nameReader1.ReadToEnd();
            File.WriteAllText(channelCleanedName + "/" + channelFileNameXML, xmlData1);
            //Constant.tempFiles.Add(channelFileNameXML);
            XmlDocument doc1 = new XmlDocument();
            doc1.Load(channelCleanedName + "/" + channelFileNameXML);
            XmlNamespaceManager namespaceManager2 = new XmlNamespaceManager(doc1.NameTable);
            namespaceManager2.AddNamespace("Atom", "http://www.w3.org/2005/Atom");
            //XmlNodeList listResult1 = doc1.SelectNodes(channelAtomEntry, namespaceManager2);

            ////Getting total Record
            XmlNamespaceManager namespaceManager3 = new XmlNamespaceManager(doc1.NameTable);
            namespaceManager3.AddNamespace("openSearch", "http://a9.com/-/spec/opensearchrss/1.0/");

            XmlNode nodeTotal = doc1.SelectSingleNode("//openSearch:totalResults", namespaceManager3);
            int total = Int32.Parse(nodeTotal.InnerText);
            if (total != 0)
            {
                string channelUrl = ConfigurationManager.AppSettings["ChannelSearchUrl"].ToString() + channelName + "&start-index=1&max-results=10&v=2";
                WebRequest nameRequest = WebRequest.Create(channelUrl);
                HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
                Stream nameStream = nameResponse.GetResponseStream();
                StreamReader nameReader = new StreamReader(nameStream);

                string xmlData = nameReader.ReadToEnd();

                File.WriteAllText(channelCleanedName + "/" + channelFileNameXML, xmlData);

                XmlDocument doc = new XmlDocument();
                doc.Load(channelCleanedName + "/" + channelFileNameXML);

                XmlNamespaceManager namespaceManager1 = new XmlNamespaceManager(doc.NameTable);
                namespaceManager1.AddNamespace("openSearch", "http://a9.com/-/spec/opensearch/1.1/");
                XmlNode node1 = doc.SelectSingleNode("//openSearch:totalResults", namespaceManager1);


                //This means Channel Exists
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");

                XmlNodeList listResult = doc.SelectNodes(channelTitleXPath, namespaceManager);
                int count = 0;
                foreach (XmlNode node in listResult)
                {
                    count++;
                    if (node.FirstChild.InnerText.Equals(channelName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                }
                XmlNodeList entryNode = doc.SelectSingleNode(channelAtomEntry + "[" + count + "]", namespaceManager).ChildNodes;
                foreach (XmlNode n in entryNode)
                {
                    if (n.Name.Equals("title"))
                    {
                        File.AppendAllText(channelCleanedName + "/" + channelFileName, "Channel Name: " + n.InnerText + "\r\n");
                    }
                    else if (n.Name.Equals("yt:channelStatistics"))
                    {
                        File.AppendAllText(channelCleanedName + "/" + channelFileName, "Subscribers Count: " + n.Attributes["subscriberCount"].Value + "\r\n");
                        File.AppendAllText(channelCleanedName + "/" + channelFileName, "Views Count: " + n.Attributes["viewCount"].Value + "\r\n");
                    }
                    //else if (n.Name.Equals("summary"))
                    //{
                    //    File.AppendAllText(pChannelName + "/" + channelFileName, "Channel Description: " + n.InnerText + "\r\n");
                    //}
                    else if (n.Name.Equals("link"))
                    {
                        if (n.Attributes["rel"].Value.Equals("alternate", StringComparison.CurrentCultureIgnoreCase))
                        {
                            File.AppendAllText(channelCleanedName + "/" + channelFileName, "Channel URL: " + n.Attributes["href"].Value + "\r\n");
                            channelUrlMain = n.Attributes["href"].Value;
                        }
                    }
                    else if (n.Name.Equals("id"))
                    {
                        string id = n.InnerText;
                        string[] arrId = n.InnerText.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        bool indexFound = false;
                        for (int i = 0; i < arrId.Length; i++)
                        {
                            if (arrId[i].Equals("Channel", StringComparison.CurrentCultureIgnoreCase))
                            {
                                indexFound = true;
                                continue;
                            }
                            if (indexFound)
                            {
                                channelId = arrId[i];
                                break;
                            }
                        }
                    }
                }
                //"AI39si7SUChDwy6-ms_bz7rY-mzkqWc9vouhT_XZfh_xery5HjOujHc4USzQJ-M6XeWPCmGtaMzBgs3QP5S4O3vFBHoxmCfIjA"
                YouTubeRequestSettings settings = new YouTubeRequestSettings(pAppName, pDevKey);
                YouTubeRequest request = new YouTubeRequest(settings);

                Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/users/" + channelName);
                Feed<Video> videoFeed = request.Get<Video>(videoEntryUrl);
                foreach (Entry e in videoFeed.Entries)
                {
                    File.AppendAllText(channelCleanedName + "/" + channelFileName, "Channel Description: " + e.Summary + "\r\n");
                    break;
                }

                Constant.tempFiles.Add(channelFileNameXML);
                Dictionary<string, VideoWrapper> videoDictionary = new Dictionary<string, VideoWrapper>();

                File.AppendAllText(channelCleanedName + "/" + channelFileName, "Video Lists \r\n");

                startIndex = 1;

                //File.AppendAllText(channelCleanedName + "/" + log, "\tEntering WriteVideoList at: " + DateTime.Now + Environment.NewLine + Environment.NewLine);

                WriteVideoLists(channelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.All);

                //File.AppendAllText(channelCleanedName + "/" + log, "\t\tTotal Dictionary Items : " + videoDictionary.Count + Environment.NewLine);
                //File.AppendAllText(channelCleanedName + "/" + log, "\r\n\tLeft WriteVideoList at: " + DateTime.Now + Environment.NewLine + Environment.NewLine);
                //File.AppendAllText(channelCleanedName + "/" + "Count.txt", "Count After complete Request Response (Expected 1000) : " + recordCount + "\r\n");

                //File.AppendAllText(channelCleanedName + "/" + log, "Leaving Parse Channel at : " + DateTime.Now);

                ///Crawl Comments
                ///
                ChannelComment.CrawlComments(videoDictionary, channelName);
                ///Done Crawling Comments
                ChannelVideo.parseVideo(videoDictionary, channelName);
                ///Done Crawling video description

                ///Remove All Temporary Files here
                ///
                Common.RemoveTempFiles(Constant.tempFiles, channelName);
                ///Done
                ///

                ///ReInitialize All Data
                ///
                //channelName = "";
                channelId = "";
                startIndex = 1;
                recordCount = 0;
                updatedFlag = false;
                ///ReInitializing Done
            }
            //Extract Playlists
            ExtractFromPlaylist(channelName, userId, 1);
            
            ///ReInitialize All Data
            ///
            //channelName = "";
            channelId = "";
            startIndex = 1;
            recordCount = 0;
            updatedFlag = false;
            ///ReInitializing Done
            
            //Extract Favourites
            
            ExtractFromUserFavourite(channelName, userId, 1);
            
        }

        public static void GetPlaylistVideos(string pChannelName, string pPlaylistURL, Dictionary<string, VideoWrapper> pDictionaryVideoWrapper, StringBuilder strBuilder, int pStartIndex)
        {
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = "Playlist-" + ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();
            string channelCleanedName = Common.CleanFileName(pChannelName);
            //For Debugging
            if (ConfigurationManager.AppSettings["ExtractAllVideosFlag"].ToString().Equals("False", StringComparison.InvariantCultureIgnoreCase))
            {
                int totalVideo = Int32.Parse(ConfigurationManager.AppSettings["totalVideos"].ToString());
                if (totalVideo <= recordCount)
                {
                    //Constant.tempFiles.Add(videFileNameXML);
                    return;
                }
            }

            WebRequest nameRequest = WebRequest.Create(pPlaylistURL + "?start-index=" + pStartIndex);
            HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
            Stream nameStream = nameResponse.GetResponseStream();
            StreamReader nameReader = new StreamReader(nameStream);

            string xmlData = nameReader.ReadToEnd();

            File.WriteAllText(channelCleanedName + "/" + channelFileNameXML, xmlData);
            Constant.tempFiles.Add(channelFileNameXML);

            XmlDocument doc = new XmlDocument();
            doc.Load(channelCleanedName + "/" + channelFileNameXML);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");
            namespaceManager.AddNamespace("openSearch", "http://a9.com/-/spec/opensearchrss/1.0/");

            //XmlNamespaceManager openSearchNameSpace = new XmlNamespaceManager(doc.NameTable);
            //openSearchNameSpace.AddNamespace("openSearch", "http://a9.com/-/spec/opensearch/1.1/");
            XmlNode totalRecordNode = doc.SelectNodes("//openSearch:totalResults", namespaceManager)[0];//SelectSingleNode("//openSearch:totalResults", namespaceManager);
            if (totalRecordNode != null && !totalRecordNode.InnerText.Equals("0"))
            {
                XmlNodeList listNodes = doc.SelectNodes("//Atom:entry", namespaceManager);
                if (listNodes.Count == 0)
                    return;
                string title = String.Empty;
                string url = String.Empty;
                string key = string.Empty;
                foreach (XmlNode n in listNodes)
                {
                    foreach (XmlNode node in n.ChildNodes)
                    {
                        if (node.Name.Equals("title"))
                        {
                            title = node.InnerText;
                        }
                        else if (node.Name.Equals("link"))
                        {
                            if (node.Attributes["rel"].Value.Equals("alternate", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string[] linkArr = node.Attributes["href"].Value.Split(new Char[] {'=', '&' }, StringSplitOptions.RemoveEmptyEntries);
                                key = linkArr[1];
                                url = "http://www.youtube.com/watch?v=" + key;
                            }
                        }
                    }
                    if (!pDictionaryVideoWrapper.ContainsKey(key))
                    {
                        recordCount++;
                        VideoWrapper vWrapper = new VideoWrapper();
                        vWrapper.setVideoKey(key);
                        vWrapper.setVideoName(title);
                        vWrapper.setVideoUrl(url);
                        pDictionaryVideoWrapper.Add(key, vWrapper);
                        strBuilder.Append("\t\t" + title + "\r\n");
                        updatedFlag = true;
                    }
                }
            }
            GetPlaylistVideos(pChannelName, pPlaylistURL, pDictionaryVideoWrapper, strBuilder, pStartIndex);
            Common.RemoveTempFiles(Constant.tempFiles, channelCleanedName);
        }

        public static void ExtractFromPlaylist(string pChannelName, string pUserId, int pStartIndex)
        {
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();
            string channelCleanedName = Common.CleanFileName(pChannelName);
            //File.AppendAllText(channelCleanedName + "/" + log, "Entered Inside Parse Channel at : " + DateTime.Now + Environment.NewLine + Environment.NewLine);

            //string channelUrl = ConfigurationManager.AppSettings["ChannelSearchUrl"].ToString() + pChannelName + "&start-index=1&max-results=10&v=2";
            WebRequest nameRequest;
            HttpWebResponse nameResponse;
            Stream nameStream;
            StreamReader nameReader;

            //File.WriteAllText(pChannelName + "/" + channelFileNameXML, xmlData);

            //Other type of extraction here
            //Extract Playlists
            string playlistUrl = "https://gdata.youtube.com/feeds/api/users/" + pUserId + "/playlists?v=2&start-index=" + pStartIndex;    //This will return all Playlists of this user
            nameRequest = WebRequest.Create(playlistUrl);
            nameResponse = (HttpWebResponse)nameRequest.GetResponse();

            nameStream = nameResponse.GetResponseStream();
            nameReader = new StreamReader(nameStream);

            string xmlData = nameReader.ReadToEnd();

            XmlDocument doc = new XmlDocument();
            //doc.Load(pChannelName + "/" + channelFileNameXML);


            File.WriteAllText(channelCleanedName + "/" + channelFileNameXML, xmlData);
            Constant.tempFiles.Add(channelFileNameXML);
            doc = new XmlDocument();
            doc.Load(channelCleanedName + "/" + channelFileNameXML);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");

            XmlNamespaceManager openSearchNameSpace = new XmlNamespaceManager(doc.NameTable);
            openSearchNameSpace.AddNamespace("openSearch", "http://a9.com/-/spec/opensearch/1.1/");
            XmlNode totalRecordNode = doc.SelectSingleNode("//openSearch:totalResults", openSearchNameSpace);

            if (totalRecordNode != null && !totalRecordNode.InnerText.Equals("0"))
            {
                XmlNode titleNode = doc.SelectSingleNode("//Atom:title", namespaceManager);
                File.AppendAllText(channelCleanedName + "/" + channelFileName, titleNode.InnerText + "\r\n");
                Dictionary<string, PlaylistWrapper> dictionaryPlayList = new Dictionary<string, PlaylistWrapper>();

                XmlNodeList listNodes = doc.SelectNodes("//Atom:entry", namespaceManager);
                if (listNodes.Count == 0)
                    return;
                string title = String.Empty;
                string key = String.Empty;
                string url = String.Empty;
                string apiURL = String.Empty;
                Dictionary<string, VideoWrapper> dictionaryVideoWrapper = new Dictionary<string, VideoWrapper>();
                foreach (XmlNode n in listNodes)
                {
                    foreach (XmlNode node in n.ChildNodes)
                    {
                        if (node.Name.Equals("title"))
                        {
                            title = node.InnerText;
                        }
                        else if (node.Name.Equals("link"))
                        {
                            if (node.Attributes["rel"].Value.Equals("alternate", StringComparison.CurrentCultureIgnoreCase))
                            {
                                url = node.Attributes["href"].Value;
                                //key = linkArr[1];
                            }
                        }
                        else if (node.Name.Equals("id"))
                        {
                            string[] idArr = node.InnerText.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            key = idArr[idArr.Length - 1];
                            apiURL = "https://gdata.youtube.com/feeds/api/playlists/" + key;
                        }
                    }
                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.Append("\tPlaylist Name: " + title + "\r\n");
                    strBuilder.Append("\tPlaylist URL: " + url + "\r\n");
                    strBuilder.Append("\tPlaylist Videos:\r\n");
                    GetPlaylistVideos(pChannelName, apiURL, dictionaryVideoWrapper, strBuilder, 1);
                    if(updatedFlag)
                        File.AppendAllText(channelCleanedName + "/" + channelFileName, strBuilder.ToString());
                    updatedFlag = false;
                    strBuilder.Remove(0, strBuilder.Length);
                }
                ChannelVideo.parseVideo(dictionaryVideoWrapper, pChannelName);
                ChannelComment.CrawlComments(dictionaryVideoWrapper, pChannelName);
                pStartIndex += 25;
                ExtractFromPlaylist(pChannelName, pUserId, pStartIndex);
            }
            Common.RemoveTempFiles(Constant.tempFiles, channelCleanedName);
        }
        public static void ExtractFromUserFavourite(string pChannelName, string pUserId, int pStartIndex)
        {
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();
            string channelCleanedName = Common.CleanFileName(pChannelName);
            //File.AppendAllText(channelCleanedName + "/" + log, "Entered Inside Parse Channel at : " + DateTime.Now + Environment.NewLine + Environment.NewLine);
            //For Debugging
            if (ConfigurationManager.AppSettings["ExtractAllVideosFlag"].ToString().Equals("False", StringComparison.InvariantCultureIgnoreCase))
            {
                int totalVideo = Int32.Parse(ConfigurationManager.AppSettings["totalVideos"].ToString());
                if (totalVideo <= recordCount)
                {
                    //Constant.tempFiles.Add(videFileNameXML);
                    return;
                }
            }
            //string channelUrl = ConfigurationManager.AppSettings["ChannelSearchUrl"].ToString() + pChannelName + "&start-index=1&max-results=10&v=2";
            WebRequest nameRequest;
            HttpWebResponse nameResponse;
            Stream nameStream;
            StreamReader nameReader;

            //File.WriteAllText(pChannelName + "/" + channelFileNameXML, xmlData);

            //Other type of extraction here
            //Extract Playlists
            string favouriteUrl = "https://gdata.youtube.com/feeds/api/users/" + pUserId + "/favorites?start-index="+ pStartIndex +"&v=2";    //This will return all Playlists of this user
            nameRequest = WebRequest.Create(favouriteUrl);
            nameResponse = (HttpWebResponse)nameRequest.GetResponse();

            nameStream = nameResponse.GetResponseStream();
            nameReader = new StreamReader(nameStream);

            string xmlData = nameReader.ReadToEnd();

            XmlDocument doc = new XmlDocument();
            //doc.Load(pChannelName + "/" + channelFileNameXML);


            File.WriteAllText(channelCleanedName + "/" + channelFileNameXML, xmlData);
            Constant.tempFiles.Add(channelFileNameXML);

            doc = new XmlDocument();
            doc.Load(channelCleanedName + "/" + channelFileNameXML);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");

            XmlNamespaceManager openSearchNameSpace = new XmlNamespaceManager(doc.NameTable);
            openSearchNameSpace.AddNamespace("openSearch", "http://a9.com/-/spec/opensearch/1.1/");
            XmlNode totalRecordNode = doc.SelectSingleNode("//openSearch:totalResults", openSearchNameSpace);

            if (totalRecordNode != null && !totalRecordNode.InnerText.Equals("0"))
            {
                XmlNode titleNode = doc.SelectSingleNode("//Atom:title", namespaceManager);
                File.AppendAllText(channelCleanedName + "/" + channelFileName, titleNode.InnerText + "\r\n");
                Dictionary<string, PlaylistWrapper> dictionaryPlayList = new Dictionary<string, PlaylistWrapper>();

                XmlNodeList listNodes = doc.SelectNodes("//Atom:entry", namespaceManager);
                if (listNodes.Count == 0)
                    return;
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append("\tFavourite Videos:\r\n");
                string title = String.Empty;
                string key = String.Empty;
                string url = String.Empty;
                string apiURL = String.Empty;
                Dictionary<string, VideoWrapper> dictionaryVideoWrapper = new Dictionary<string, VideoWrapper>();
                foreach (XmlNode n in listNodes)
                {
                    foreach (XmlNode node in n.ChildNodes)
                    {
                        if (node.Name.Equals("title"))
                        {
                            title = node.InnerText;
                        }
                        else if (node.Name.Equals("link"))
                        {
                            if (node.Attributes["rel"].Value.Equals("alternate", StringComparison.CurrentCultureIgnoreCase))
                            {
                                url = node.Attributes["href"].Value.Split(new Char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)[0];
                                key = url.Split(new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1];
                            }
                        }
                    }
                    
                    
                    strBuilder.Append("\t\tVideo Name: " + title + "\r\n");
                    if (!dictionaryVideoWrapper.ContainsKey(key))
                    {
                        VideoWrapper vWrapper = new VideoWrapper();
                        vWrapper.setVideoKey(key);
                        vWrapper.setVideoName(title);
                        vWrapper.setVideoUrl(url);
                        dictionaryVideoWrapper.Add(key, vWrapper);
                        updatedFlag = true;
                        recordCount++;
                    }
                    if (updatedFlag)
                        File.AppendAllText(channelCleanedName + "/" + channelFileName, strBuilder.ToString());
                    updatedFlag = false;
                    strBuilder.Remove(0, strBuilder.Length);
                }
                ChannelVideo.parseVideo(dictionaryVideoWrapper, pChannelName);
                ChannelComment.CrawlComments(dictionaryVideoWrapper, pChannelName);
                pStartIndex += 25;
                ExtractFromUserFavourite(pChannelName, pUserId, pStartIndex);
            }
            Common.RemoveTempFiles(Constant.tempFiles, channelCleanedName);
        }
    }
}
