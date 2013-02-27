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
        public static void parseVideo(){

          //  string videoUrl = ConfigurationManager.AppSettings["ChannelVideoSearchById"].ToString();
            string videoUrl = "https://gdata.youtube.com/feeds/api/videos/5l3jrR5XMSU/comments?v=2";
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
        }
    }
}
