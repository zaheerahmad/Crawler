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
using System.Net;
using System.IO;
using System.Xml;
namespace YoutubeCrawler.Utilities
{
    class ChannelVideo
    {
        public static void parseVideo(Dictionary<string,VideoWrapper> paramsVideoDict){
           
            
            Dictionary<string, VideoInfoWrapper> videoInfo = new Dictionary<string, VideoInfoWrapper>();
            foreach (var pair in paramsVideoDict)
            {

                string videoUrl = string.Format("https://gdata.youtube.com/feeds/api/videos/{1}?v=2", pair.Key);
               // string videoUrl = "https://gdata.youtube.com/feeds/api/videos/5l3jrR5XMSU?v=2";
                string channelFileNameXML = ConfigurationManager.AppSettings["channelsFileNameXML"].ToString();


                WebRequest nameRequest = WebRequest.Create(videoUrl);
                HttpWebResponse nameResponse = (HttpWebResponse)nameRequest.GetResponse();

                Stream nameStream = nameResponse.GetResponseStream();
                StreamReader nameReader = new StreamReader(nameStream);


                string xmlData = nameReader.ReadToEnd();

                File.WriteAllText(channelFileNameXML, xmlData);

                XmlDocument doc = new XmlDocument();
                doc.Load(channelFileNameXML);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);

                namespaceManager.AddNamespace("Atom", "http://www.w3.org/2005/Atom");
                namespaceManager.AddNamespace("yt", "http://gdata.youtube.com/schemas/2007");
                namespaceManager.AddNamespace("media", "http://search.yahoo.com/mrss/");

            

                VideoInfoWrapper obj = new VideoInfoWrapper
                {
                    videoChannelName = doc.SelectSingleNode("//Atom:entry/Atom:author/Atom:name", namespaceManager) != null ? doc.SelectSingleNode("//Atom:entry/Atom:author/Atom:name", namespaceManager).InnerText.ToString() : string.Empty,
                    videoName = doc.SelectSingleNode("//Atom:entry/Atom:title", namespaceManager) != null ? doc.SelectSingleNode("//Atom:entry/Atom:title", namespaceManager).InnerText.ToString() : string.Empty,
                    date = doc.SelectSingleNode("//Atom:entry/Atom:published", namespaceManager) != null ? doc.SelectSingleNode("//Atom:entry/Atom:published", namespaceManager).InnerText.ToString() : string.Empty,
                    iDislike = doc.SelectSingleNode("//Atom:entry/yt:rating", namespaceManager) != null ? doc.SelectSingleNode("//Atom:entry/yt:rating", namespaceManager).Attributes["numDislikes"].Value.ToString() : string.Empty,
                    iLike = doc.SelectSingleNode("//Atom:entry/yt:rating", namespaceManager) != null ? doc.SelectSingleNode("//Atom:entry/yt:rating", namespaceManager).Attributes["numLikes"].Value.ToString() : string.Empty,
                    description = doc.SelectSingleNode(" //Atom:entry/media:group/media:description", namespaceManager) != null ? doc.SelectSingleNode(" //Atom:entry/media:group/media:description", namespaceManager).InnerText.ToString() : string.Empty,
                    videoTags = preapreParamsTags(doc.SelectNodes("//Atom:entry/Atom:category", namespaceManager)) != null ? preapreParamsTags(doc.SelectNodes("//Atom:entry/Atom:category", namespaceManager)) : null,
                };

                // for time being i have added temoporary key.. need to test this function against results provided by you
                videoInfo.Add("key", obj);

            }
        }
        // this function would be preparing tags for the selected video
        public static List<string> preapreParamsTags(XmlNodeList xmlNodeList)
        {
            List<string> returnList = new List<string>();
            foreach (XmlNode node in xmlNodeList)
            {
                if (node.Attributes["label"] != null)
                {
                    returnList.Add(node.Attributes["label"].Value.ToString());
                }
            }
            return returnList.Count > 0 ? returnList : null;
        }
    }

  
}
