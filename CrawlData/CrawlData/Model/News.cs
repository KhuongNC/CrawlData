using CrawlData.Common;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CrawlData.Model
{
    public class News
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string TypeOfNews { get; set; }
        public string PostDate { get; set; }
        public string Author { get; set; }
    }
}
