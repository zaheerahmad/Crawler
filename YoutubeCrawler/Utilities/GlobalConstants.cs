using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    static class GlobalConstants
    {
        public static int _level = 1;
        public static Dictionary<string, VideoCommentWrapper> commentDictionary = new Dictionary<string, VideoCommentWrapper>();
    }
}
