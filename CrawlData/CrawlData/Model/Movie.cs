using CrawlData.Common;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrawlData.Model
{
    public class Movie
    {
        public string Name { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public string TypeOfMovie { get; set; }
        public string PremiereDate { get; set; }
        public string Duration { get; set; }
        public string Language { get; set; }
        public string Rated { get; set; }
        public string Content { get; set; }
        public string ImageLink { get; set; }
        public string TrailerLink { get; set; }
    }
}
