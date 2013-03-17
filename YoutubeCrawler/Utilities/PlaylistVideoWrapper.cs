using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    class PlaylistVideoWrapper
    {
        public string videoTitle { get; set; }
        public string videoKey { get; set; }
        public string videoUrl { get; set; }


        public PlaylistVideoWrapper()
        {
            this.videoTitle = string.Empty;
            this.videoKey = string.Empty;
            this.videoUrl = string.Empty;
        }
    }
}
