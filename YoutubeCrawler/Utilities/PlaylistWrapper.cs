using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    class PlaylistWrapper
    {
        public string playListKey { get; set; }
        public string playListName { get; set; }
        public string playListURL { get; set; }


        public PlaylistWrapper()
        {
            this.playListKey = string.Empty;
            this.playListName = string.Empty;
            this.playListURL = string.Empty;
        }
    }
}
