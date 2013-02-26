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
        //public static bool Crawl(string pChannelName, YouTubeRequest pYouTubeRequest)
        //{
        //    string channelFileName = ConfigurationManager.AppSettings["channelsFileName"].ToString();

        //    //This Request will give us 10 channels from index 1, which is searched by adding its name.
            
        //    //e.g.https://gdata.youtube.com/feeds/api/channels?q=" + pChannelName + "&start-index=1&max-results=10&v=2
            
        //    //q=<Channel Name>
        //    //start-index = <start Index of Search result> (by default 1st Index is '1')
        //    //max-result = <page size (containing number of channels)>
        //    //v = Not known yet. :S

            

        //    //string channelUrl = "https://gdata.youtube.com/feeds/api/channels?q=" + pChannelName + "&start-index=1&max-results=10&v=2";
        //    //WebRequest nameRequest = WebRequest.Create(channelUrl);
        //    //HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();
        //    //Stream nameStream = nameResponse.GetResponseStream();
        //    //StreamReader nameReader = new StreamReader(nameStream);
        //    //string XML = nameReader.ReadToEnd();
        //    //File.WriteAllText("myFile.xml", XML);

        //    //XmlDocument doc = new XmlDocument();
        //    //doc.Load("myFile.xml");
        //    //XmlElement element = doc.DocumentElement;
        //    ////For Subscription and Views Count
        //    //XmlNodeList nodeList = element.ChildNodes;
        //    //bool doneTitle = false;
        //    //bool doneStats = false;
        //    //bool doneSummary = false;
        //    //foreach (XmlNode node in nodeList)
        //    //{
        //    //    if (node.Name.Equals("entry"))
        //    //    {
        //    //        XmlNodeList list = node.ChildNodes;
        //    //        //bool entryDone = false;
        //    //        foreach (XmlNode n in list)
        //    //        {
                        
        //    //            if (n.Name.Equals("title") && n.InnerText.Equals(pChannelName))
        //    //            {
        //    //                File.AppendAllText(channelFileName, "Channel Name: " + n.InnerText + "\r\n");
        //    //                doneTitle = true;
        //    //            }
        //    //            else if (n.Name.Equals("yt:channelStatistics") && doneTitle)
        //    //            {
        //    //                File.AppendAllText(channelFileName, "Subscribers Count: " + n.Attributes["subscriberCount"].Value + "\r\n");
        //    //                File.AppendAllText(channelFileName, "Views Count: " + n.Attributes["viewCount"].Value + "\r\n");
        //    //                doneStats = true;
        //    //            }
        //    //            else if (n.Name.Equals("summary") && doneTitle)
        //    //            {
        //    //                File.AppendAllText(channelFileName, "Channel Description: " + Environment.NewLine + "\r" + n.InnerText + "\r\n");
        //    //                doneSummary = true;
        //    //            }
        //    //            if (doneTitle && doneSummary && doneStats)
        //    //            {
        //    //                //File.AppendAllText(channelFileName, Environment.NewLine + "\r\n");
        //    //                break;
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    //int startIndex = 1;
        //    //bool continueLoop = true;
        //    //int totalRecord = -1;
        //    ////int doneCount = 0;
        //    //int i = 0;
        //    //try
        //    //{
        //    //    while (continueLoop)
        //    //    {
        //    //        File.AppendAllText("text.txt", startIndex + Environment.NewLine + "\r\n");
        //    //        string nameURL = "https://gdata.youtube.com/feeds/api/videos?author=" + pChannelName + "&start-index=" + startIndex + "&pagesize=25&orderby=published";
        //    //        nameRequest = WebRequest.Create(nameURL);
        //    //        nameResponse = (HttpWebResponse)nameRequest.GetResponse();
        //    //        nameStream = nameResponse.GetResponseStream();
        //    //        nameReader = new StreamReader(nameStream);
        //    //        XML = nameReader.ReadToEnd();
        //    //        File.WriteAllText("myFile.xml", XML);

        //    //        doc = new XmlDocument();
        //    //        doc.Load("myFile.xml");
        //    //        element = doc.DocumentElement;
        //    //        nodeList = element.ChildNodes;
        //    //        doneTitle = false;
        //    //        doneStats = false;

        //    //        foreach (XmlNode node in nodeList)
        //    //        {
        //    //            if (node.Name.Equals("openSearch:totalResults") && totalRecord < 0)
        //    //            {
        //    //                totalRecord = Int32.Parse(node.InnerText);
        //    //            }
        //    //            if (node.Name.Equals("entry"))
        //    //            {
        //    //                XmlNodeList list = node.ChildNodes;
        //    //                foreach (XmlNode n in list)
        //    //                {
        //    //                    if (n.Name.Equals("title"))
        //    //                    {
        //    //                        File.AppendAllText(channelFileName, "Video Name: " + n.InnerText + "\r\n");
        //    //                        doneTitle = true;
        //    //                    }
        //    //                    if (n.Name.Equals("yt:statistics"))
        //    //                    {
        //    //                        File.AppendAllText(channelFileName, "Video Views: " + n.Attributes["viewCount"].Value + "\r\n");
        //    //                        doneStats = true;
        //    //                    }
        //    //                    if (doneTitle && doneStats)
        //    //                    {
        //    //                        i++;
        //    //                        doneStats = false;
        //    //                        doneTitle = false;

        //    //                    }
        //    //                }
        //    //            }
        //    //        }
        //    //        startIndex += 25;
        //    //        if (startIndex == totalRecord)
        //    //        {
        //    //            continueLoop = false;
                        
        //    //            break;
        //    //        }
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    File.AppendAllText("Count.txt", "" + i + "\r\n");
        //    //    return true;
        //    //}
        //    //File.AppendAllText(channelFileName, Environment.NewLine + "\r\n");
        //    //return true;
        //    //XmlNode xmlNode = doc.SelectSingleNode("/feed");
        //    //XmlNodeList StudentNodeList = xmlNode.SelectNodes("entry");
        //    //foreach (XmlNode node in StudentNodeList)
        //    //{
        //    //    File.AppendAllText(channelFileName, "Views: " + node["yt:statistics"].Attributes["viewCount"].Value + "\r\n");
        //    //}
        //    //Uri channelEntryUrl = new Uri("https://gdata.youtube.com/feeds/api/channels?q=" + pChannelName + "&start-index=1&max-results=10&v=2");
            
        //    //Feed<Video> videoFeel = pYouTubeRequest.Get<Video>(channelEntryUrl);
        //    //XmlDocument xDoc = new XmlDocument();
        //    //xDoc.Load("test.xml");

        //    //XmlNamespaceManager manager = new XmlNamespaceManager(xDoc.NameTable);
        //    //manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

        //    //XmlNodeList xnList = xDoc.SelectNodes("atom:feed/atom:entry", manager);
        //    //foreach (XmlNode xn in xnList)
        //    //{
        //    //    File.AppendAllText(channelFileName, xn.LocalName.ToString() + "\r\n");
        //    //    //Debug.WriteLine(xn.LocalName.ToString());
        //    //}
        //    //AtomEntryCollection col = videoFeel.AtomFeed.Entries;
        //    //foreach (AtomEntry entry in col)
        //    //{
        //    //    string n = entry.Title.Text;
        //    //    AtomFeed f = entry.Feed;
                
        //    //    break;
        //    //}
        //    //XmlDocument xml = new XmlDocument();
        //    //xml.Load("test.xml");
        //    //XmlElement root = xml.DocumentElement;
            
        //    ////String mediaUri = "http://search.yahoo.com/mrss/";
        //    //foreach (XmlElement entry in root.GetElementsByTagName("entry", ))
        //    //{
        //    //    foreach (XmlElement group in
        //    //             entry.GetElementsByTagName("group", mediaUri))
        //    //    {
        //    //        foreach (XmlElement content in
        //    //                 entry.GetElementsByTagName("content", mediaUri))
        //    //        {
        //    //            Console.WriteLine(content.Attributes["url"].Value);
        //    //        }
        //    //    }
        //    //}
        //    //XmlDocument xmlDoc = pYouTubeRequest.GetSearchResults(search.Term, "published", 1, 50);
        //    //XmlNodeList listNodes = xmlDoc.GetElementsByTagName("entry");
        //    //foreach (XmlNode node in listNodes)
        //    //{
        //    //    // get child nodes
        //    //    foreach (XmlNode childNode in node.ChildNodes)
        //    //    {
        //    //    }

        //    //    // get specific child nodes
        //    //    XPathNavigator navigator = node.CreateNavigator();
        //    //    XPathNodeIterator iterator = navigator.Select(/* xpath selector according to the elements/attributes you need */);

        //    //    while (iterator.MoveNext())
        //    //    {
        //    //        // f.e. iterator.Current.GetAttribute(), iterator.Current.Name and iterator.Current.Value available here
        //    //    }
        //    //}
        //    //if(videoFeel.TotalResults > 0)
        //    //{
        //    //    StringBuilder strChannelInfo = new StringBuilder();
        //    //    foreach (Video video in videoFeel.Entries)
        //    //    {
        //    //        String id = video.Id;
        //    //        string id2 = video.VideoId;
        //    //        int count = video.ViewCount;
        //    //        DateTime a = video.Updated;
        //    //        string b = video.Uploader;
        //    //        string c = video.ETag;
        //    //        int d = video.CommmentCount;
        //    //        AtomCategoryCollection col = video.Categories;
        //    //        object o = video.AppControl;
                    

        //    //        File.AppendAllText(channelFileName, "Channel Name: " + video.Title + "\r\n");
                    
        //    //        if (video.AtomEntry.ExtensionElements["Statistics"] != null)
        //    //        {
        //    //            File.AppendAllText(channelFileName, "Subscribers: " + video.YouTubeEntry.Statistics.SubscriberCount + "\r\n");
        //    //            File.AppendAllText(channelFileName, "View Count: " + video.YouTubeEntry.Statistics.ViewCount + "\r\n");
        //    //        }
        //    //        File.AppendAllText(channelFileName, "Channel Description: " + video.Summary + "\r\r\n\n" + Environment.NewLine);
                    
        //    //        Uri channelEntryUrl1 = new Uri("http://gdata.youtube.com/feeds/api/videos?author=" + pChannelName + "&orderby=publish");
        //    //        Feed<Video> feeds = pYouTubeRequest.Get<Video>(channelEntryUrl1);
        //    //        File.AppendAllText(channelFileName, "Video Lists" + "\r\n");
        //    //        foreach (Video v in feeds.Entries)
        //    //        {
        //    //            File.AppendAllText(channelFileName, "Viedo Tile: " + v.Title + "\r\n");
        //    //        }
        //    //        return true;
        //    //    }
        //    //}
        //}
        //string pVideoId, YouTubeRequest pYoutubeRequest, string pFilePath

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
            
            XmlElement element = doc.DocumentElement;
            //For Subscription and Views Count
            XmlNodeList nodeList = element.ChildNodes;
            bool doneTitle = false;
            bool doneStats = false;
            bool doneSummary = false;
            foreach (XmlNode node in nodeList)
            {
                if (node.Name.Equals("entry"))
                {
                    XmlNodeList list = node.ChildNodes;
                    //bool entryDone = false;
                    foreach (XmlNode n in list)
                    {

                        if (n.Name.Equals("title") && n.InnerText.Equals(pChannelName))
                        {
                            File.AppendAllText(channelFileName, "Channel Name: " + n.InnerText + "\r\n");
                            doneTitle = true;
                        }
                        else if (n.Name.Equals("yt:channelStatistics") && doneTitle)
                        {
                            File.AppendAllText(channelFileName, "Subscribers Count: " + n.Attributes["subscriberCount"].Value + "\r\n");
                            File.AppendAllText(channelFileName, "Views Count: " + n.Attributes["viewCount"].Value + "\r\n");
                            doneStats = true;
                        }
                        else if (n.Name.Equals("summary") && doneTitle)
                        {
                            File.AppendAllText(channelFileName, "Channel Description: " + Environment.NewLine + "\r" + n.InnerText + "\r\n");
                            doneSummary = true;
                        }
                        if (doneTitle && doneSummary && doneStats)
                        {
                            //File.AppendAllText(channelFileName, Environment.NewLine + "\r\n");
                            break;
                        }
                    }
                }
            }
            return true;
        }
        //public static void WriteVideoLists(YouTubeRequest pYoutubeRequest)
        //{
        //    int startIndex = 1;
        //    int count = 0;
        //    try
        //    {
        //        while (true)
        //        {
        //            Uri channelEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/BfJVgXBfSH8?start-index=" + startIndex);
        //            Video videoFeel = pYoutubeRequest.Retrieve<Video>(channelEntryUrl);
        //            Feed<Comment> comments = pYoutubeRequest.GetComments(videoFeel);
        //            foreach (Comment c in comments.Entries)
        //            {

        //                count++;
        //            }
        //            startIndex += 25;
        //            if (startIndex == comments.TotalResults)
        //            {
        //                File.AppendAllText("count.txt", count + "\r\n");
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        File.AppendAllText("count.txt", count + "\r\n");
        //    }
        //}
    }
}
