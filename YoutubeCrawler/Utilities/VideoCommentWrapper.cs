using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    class VideoCommentWrapper
    {
        public string commentId { get; set; }
        public string displayName { get; set; }
        public string userName { get; set; }
        public string time { get; set; }
        public string commentText { get; set; }
        public string authorId { get; set; }

        public VideoCommentWrapper()
        {
            this.commentId = string.Empty;
            this.displayName = string.Empty;
            this.userName = string.Empty;
            this.time = string.Empty;
            this.commentText = string.Empty;
            this.authorId = string.Empty;
        }
    }
}
