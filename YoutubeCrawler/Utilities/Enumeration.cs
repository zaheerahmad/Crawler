using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeCrawler.Utilities
{
    public class Enumeration
    {
        public enum VideoRequestType
        {
            PublishedSort,
            TopRated,
            MostViewed,
            MostShared,
            MostPopular,
            TopFavourites,
            MostRecent,
            MostDiscussed,
            MostResponded,
            SortDescending,
            SortAscending
        }
    }
}
