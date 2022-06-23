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
    }

    public class CrawlDataForNews
    {
        public List<News> CrawlData(string url, string website)
        {
            List<News> newsList = new List<News>();
            News news = new News();

            try
            {
                HtmlWeb htmlWeb = new HtmlWeb()
                {
                    AutoDetectEncoding = false,
                    OverrideEncoding = Encoding.UTF8 // Set UTF8 to dsiplay vietnamese  
                };

                // Load web, store data into document
                HtmlAgilityPack.HtmlDocument document = htmlWeb.Load(url);

                // List stores all nodes to get information
                List<HtmlNode> nodeList = new List<HtmlNode>();

                switch (website)
                {
                    case Constants.KENH14:
                        nodeList = document.DocumentNode.QuerySelectorAll("ul.knsw-list > div > li").ToList();

                        if (nodeList != null)
                        {

                            foreach (var item in nodeList)
                            {
                                // Get link to move detail page
                                var aNode = item.QuerySelector("div.knswli-left a");

                                if (aNode != null)
                                {
                                    string newsLink = "https://kenh14.vn" + aNode.Attributes["href"].Value.Trim();

                                    // Get type of news and post date
                                    string typeOfNews = item.QuerySelector(".knswli-meta a").InnerText;
                                    string postDate = item.QuerySelector(".knswli-meta .knswli-time").Attributes["title"].Value;

                                    // Get data from detail page
                                    news = CrawlDataFromDetailPage(newsLink, website);
                                    news.TypeOfNews = typeOfNews;
                                    news.PostDate = postDate;

                                    if (news != null)
                                    {
                                        newsList.Add(news);
                                    }
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }

                return newsList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return newsList;
            }
        }

        private News CrawlDataFromDetailPage(string url, string website)
        {
            News news = new News();
            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8 // Set UTF8 to dsiplay vietnamese  
            };

            // Load web, store data into document
            HtmlAgilityPack.HtmlDocument document = htmlWeb.Load(url);

            HtmlNode[] detailNewsNodeArr = new HtmlNode[25];

            // Parent node
            HtmlNode nodeList = document.DocumentNode.QuerySelector("div");

            switch (website)
            {
                case Constants.KENH14:
                    // Get information about News
                    nodeList = document.DocumentNode.QuerySelector("div.klw-new-content");

                    if (nodeList != null)
                    {
                        news = GetDetailFromKenh14(nodeList);
                    }
                    break;
                default:
                    break;
            }

            return news;
        }

        private News GetDetailFromKenh14(HtmlNode nodeList)
        {
            News news = new News()
            {
                Title = nodeList != null ? nodeList.QuerySelector(".knc-sapo").InnerText.Trim() : ""
            };

            // Get all nodes which store all contents of article
            var contentList = nodeList.QuerySelectorAll(".knc-content > p").ToList();

            foreach (var item in contentList)
            {
                news.Content += System.Net.WebUtility.HtmlDecode(item.InnerText);
            }

            // Get all images of article
            var divNodeList = nodeList.QuerySelectorAll(".knc-content > div.VCSortableInPreviewMode").ToList();

            foreach (var item in divNodeList)
            {
                var imgNode = item.QuerySelector("img");

                if (imgNode != null)
                {
                    news.ImageUrl += imgNode.Attributes["src"].Value + "; ";
                }
            }

            return news;
        }
    }
}
