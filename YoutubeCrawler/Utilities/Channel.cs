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
        public static string channelTitleXPath = "//Atom:entry/Atom:title";
        public static string channelName = "";
        public static string channelId = "";
        public static int startIndex = 1;
        public static int recordCount = 0;
        public static string log = ConfigurationManager.AppSettings["LogFiles"].ToString();
        //public static List<string> tempFiles = new List<string>();
        public static bool ParseChannel(string pChannelName)
        {
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();
            File.AppendAllText(pChannelName + "/" + log, "Entered Inside Parse Channel at : " + DateTime.Now + Environment.NewLine + Environment.NewLine);

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
                if (node.InnerText.Equals(pChannelName))
                {
                    break;
                }
            }
            XmlNodeList entryNode = doc.SelectSingleNode(channelAtomEntry + "[" + count + "]", namespaceManager).ChildNodes;
            foreach (XmlNode n in entryNode)
            {
                if (n.Name.Equals("title") && n.InnerText.Equals(pChannelName))
                {
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Channel Name: " + n.InnerText + "\r\n");
                }
                else if (n.Name.Equals("yt:channelStatistics"))
                {
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Subscribers Count: " + n.Attributes["subscriberCount"].Value + "\r\n");
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Views Count: " + n.Attributes["viewCount"].Value + "\r\n");
                }
                else if (n.Name.Equals("summary"))
                {
                    File.AppendAllText(pChannelName + "/" + channelFileName, "Channel Description: " + n.InnerText + "\r\n");
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

            Constant.tempFiles.Add(channelFileNameXML);
            Dictionary<string, VideoWrapper> videoDictionary = new Dictionary<string, VideoWrapper>();

            File.AppendAllText(pChannelName + "/" + channelFileName, "Video Lists \r\n");

            startIndex = 1;
            
            File.AppendAllText(pChannelName + "/" + log, "\tEntering WriteVideoList at: " + DateTime.Now + Environment.NewLine + Environment.NewLine);
            
            WriteVideoLists(pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.All);
            
            File.AppendAllText(pChannelName + "/" + log, "\t\tTotal Dictionary Items : " + videoDictionary.Count + Environment.NewLine);
            File.AppendAllText(pChannelName + "/" + log, "\r\n\tLeft WriteVideoList at: " + DateTime.Now + Environment.NewLine + Environment.NewLine);
            File.AppendAllText(pChannelName + "/" + "Count.txt", "Count After complete Request Response (Expected 1000) : " + recordCount + "\r\n");
            
            File.AppendAllText(pChannelName + "/" + log, "Leaving Parse Channel at : " + DateTime.Now);

            ///Crawl Comments
            ///
            ChannelComment.CrawlComments(videoDictionary, pChannelName);            
            ///Done Crawling Comments
            ChannelVideo.parseVideo(videoDictionary);
            ///Done Crawling video description

            ///Remove All Temporary Files here
            ///
            Common.RemoveTempFiles(Constant.tempFiles, pChannelName);
            ///Done
            ///
            
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
                string channelUrl = String.Empty;
                if (requestType == Enumeration.VideoRequestType.All)
                {
                    //http://gdata.youtube.com/feeds/api/users/machinima/uploads?start-index=4000
                    channelUrl = "http://gdata.youtube.com/feeds/api/users/" + pChannelName + "/uploads?&start-index=" + startIndex;
                }


                HttpWebRequest nameRequest = (HttpWebRequest)WebRequest.Create(channelUrl);
                nameRequest.KeepAlive = false;
                nameRequest.ProtocolVersion = HttpVersion.Version10;
                HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();

                Stream nameStream = nameResponse.GetResponseStream();
                StreamReader nameReader = new StreamReader(nameStream);

                string xmlData = nameReader.ReadToEnd();
                File.WriteAllText(pChannelName + "/" + videFileNameXML, xmlData);

                XmlDocument doc = new XmlDocument();
                doc.Load(pChannelName + "/" + videFileNameXML);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");
                XmlNodeList listResult = doc.SelectNodes(channelAtomEntry, namespaceManager);
                
                ////Getting total Record
                XmlNamespaceManager namespaceManager1 = new XmlNamespaceManager(doc.NameTable);
                namespaceManager1.AddNamespace("openSearch", "http://a9.com/-/spec/opensearchrss/1.0/");

                XmlNode nodeTotal = doc.SelectSingleNode("//openSearch:totalResults", namespaceManager1);
                int total = Int32.Parse(nodeTotal.InnerText);
                
                //Base Case Started

                string flag = ConfigurationManager.AppSettings["testingFlag"].ToString();
                if (flag.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (startIndex > 26)
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

                File.AppendAllText(pChannelName + "/" + log, "\t\tTotal Record : " + total + "; Start Index : " + startIndex + Environment.NewLine);
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
                                videoDictionary.Add(videoId, new VideoWrapper(videoName, videoId, videoUrl));
                                File.AppendAllText(pChannelName + "/" + channelFileName, "\t" + videoName + "\r\n");
                                recordCount++;
                            }
                            break;
                        }
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
                File.AppendAllText(pChannelName + "/" + log, "\t\tException Found : " + ex.Message + Environment.NewLine);
                startIndex += 25;
                if (requestType == Enumeration.VideoRequestType.All)
                {
                    WriteVideoLists(pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.All); //Recursive Call
                }
            }
        }
    }
}
