using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    class VideoWrapper
    {
        private string videoName;
        private string videoKey;
        private string videoUrl;

        public VideoWrapper()
        {
            videoName = String.Empty;
            videoUrl = String.Empty;
            videoKey = String.Empty;
        }
        public VideoWrapper(string pVideoName, string pVideoKey, string pVideoUrl)
        {
            this.videoName = pVideoName;
            this.videoKey = pVideoKey;
            this.videoUrl = pVideoUrl;
        }

        public void setVideoKey(string pKey)
        {
            this.videoKey = pKey;
        }

        public void setVideoName(string pName)
        {
            this.videoName = pName;
        }

        public void setVideoUrl(string pUrl)
        {
            this.videoUrl = pUrl;
        }
        public string getVideoKey()
        {
            return this.videoKey;
        }

        public string getVideoName()
        {
            return this.videoName;
        }

        public string getVideoUrl()
        {
            return this.videoUrl;
        }
    }
}