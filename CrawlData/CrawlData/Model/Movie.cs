using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlData.Model
{
    public class Movie
    {
        public string Name { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public string TypeOfMovie { get; set; }
        public DateTime PremiereDate { get; set; }
        public int Duration { get; set; }
        public string Language { get; set; }
        public string Rated { get; set; }
        public string Content { get; set; }
    }
}
