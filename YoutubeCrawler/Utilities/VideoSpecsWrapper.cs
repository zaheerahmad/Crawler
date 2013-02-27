using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    class VideoSpecsWrapper
    {
        public string videoName { get; set; }
        public string date { get; set; }
        public string iLike { get; set; }
        public string iDislike { get; set; }
        public string description { get; set; }

        public VideoSpecsWrapper()
        {
            this.videoName = string.Empty;
            this.date = string.Empty;
            this.iLike = string.Empty;
            this.iDislike = string.Empty;
            this.description = string.Empty;
        }
    }
}
