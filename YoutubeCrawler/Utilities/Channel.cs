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

        public static bool ParseChannel(YouTubeRequest pYoutubeRequest, string pChannelName)
        {
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();

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
            
            File.WriteAllText(channelFileNameXML, xmlData);

            XmlDocument doc = new XmlDocument();
            doc.Load(channelFileNameXML);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom"); 
            XmlNodeList listResult = doc.SelectNodes(channelTitleXPath,namespaceManager);
            int count = 0;
            foreach (XmlNode node in listResult)
            {
                count++;
                if (node.InnerText.Equals(pChannelName))
                {
                    break;
                }
            }
            XmlNodeList entryNode = doc.SelectSingleNode(channelAtomEntry + "[" + count + "]",namespaceManager).ChildNodes;
            foreach (XmlNode n in entryNode)
            {
                if (n.Name.Equals("title") && n.InnerText.Equals(pChannelName))
                {
                    File.AppendAllText(channelFileName, "Channel Name: " + n.InnerText + "\r\n");
                }
                else if (n.Name.Equals("yt:channelStatistics"))
                {
                    File.AppendAllText(channelFileName, "Subscribers Count: " + n.Attributes["subscriberCount"].Value + "\r\n");
                    File.AppendAllText(channelFileName, "Views Count: " + n.Attributes["viewCount"].Value + "\r\n");
                }
                else if (n.Name.Equals("summary"))
                {
                    File.AppendAllText(channelFileName, "Channel Description: " + n.InnerText + "\r\n");
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

            //Working for Channel's Video

            Dictionary<string, VideoWrapper> videoDictionary = new Dictionary<string, VideoWrapper>();
            //File.AppendAllText(channelFileName, "Video Lists \r\n");
            //WriteVideoLists(pYoutubeRequest, pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.PublishedSort);

            //File.AppendAllText("Count.txt", "Count After complete Request Response (Expected 1000) : " + recordCount + "\r\n");
            //startIndex = 1;
            //WriteVideoLists(pYoutubeRequest, pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.TopRated);
            //File.AppendAllText("Count.txt", "Count After complete Request Response (Expected 1000) : " + recordCount + "\r\n");

            startIndex = 1;
            WriteVideoLists(pYoutubeRequest, pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.MostViewed);
            File.AppendAllText("Count.txt", "Count After complete Request Response (Expected 1000) : " + recordCount + "\r\n");
            return true;
        }

        public static void WriteVideoLists(YouTubeRequest pYoutubeRequest, string pChannelName, string pChannelId, int startIndex, Dictionary<string,VideoWrapper> videoDictionary, Enumeration.VideoRequestType requestType)
        {
            //Base Case of Recursion
            if (startIndex >= 1000)
                return;
            //Base Case Ended of Recursion
            string videoName = String.Empty;
            string videoUrl = String.Empty;
            //string url = String.Empty;
            string videoId = String.Empty;

            string videoFileName = ConfigurationManager.AppSettings["channelsVideoFile"].ToString();
            string videFileNameXML = ConfigurationManager.AppSettings["channelsVideoFileXML"].ToString();
            string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();
            string channelUrl = String.Empty;

            if (requestType == Enumeration.VideoRequestType.PublishedSort)
            {
                 channelUrl = ConfigurationManager.AppSettings["ChannelVideoSearch"].ToString() + pChannelName + "&start-index=" + startIndex + "&pagesize=25&orderby=published";
            }
            else if(requestType == Enumeration.VideoRequestType.TopRated)
            {
                channelUrl = ConfigurationManager.AppSettings["ChannelVideoSearch"].ToString() + pChannelName + "&start-index=" + startIndex + "&pagesize=25&top_rated";
            }
            else if (requestType == Enumeration.VideoRequestType.MostViewed)
            {
                channelUrl = ConfigurationManager.AppSettings["ChannelVideoSearch"].ToString() + pChannelName + "&start-index=" + startIndex + "&pagesize=25&most_viewed";
            }
            HttpWebRequest nameRequest = (HttpWebRequest)WebRequest.Create(channelUrl);
            nameRequest.KeepAlive = false;
            nameRequest.ProtocolVersion = HttpVersion.Version10; 
            HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
            
            Stream nameStream = nameResponse.GetResponseStream();
            StreamReader nameReader = new StreamReader(nameStream);

            string xmlData = nameReader.ReadToEnd();

            File.WriteAllText(videFileNameXML, xmlData);

            XmlDocument doc = new XmlDocument();
            doc.Load(videFileNameXML);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");
            XmlNodeList listResult = doc.SelectNodes(channelAtomEntry, namespaceManager);
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
                        if(videoDictionary != null && !videoDictionary.ContainsKey(videoId))
                        {
                            videoDictionary.Add(videoId, new VideoWrapper(videoName, videoId, videoUrl));
                            File.AppendAllText(channelFileName, "\t" + videoName + "\r\n");
                            recordCount++;
                        }
                        break;
                    }
                }
            }
            startIndex+=25;
            if (requestType == Enumeration.VideoRequestType.PublishedSort)
            {
                WriteVideoLists(pYoutubeRequest, pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.PublishedSort); //Recursive Call
            }
            else if (requestType == Enumeration.VideoRequestType.TopRated)
            {
                WriteVideoLists(pYoutubeRequest, pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.TopRated); //Recursive Call
            }
            else if (requestType == Enumeration.VideoRequestType.MostViewed)
            {
                WriteVideoLists(pYoutubeRequest, pChannelName, channelId, startIndex, videoDictionary, Enumeration.VideoRequestType.MostViewed); //Recursive Call
            }
        }
    }
}
