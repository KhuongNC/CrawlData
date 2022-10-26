using CrawlData.Common;
using CrawlData.Model;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlData.Utilities
{
    public static class CrawlData
    {
        public static List<T> CrawlDataFromWebsite<T>(string url, string website)
        {
            List<T> objList = new List<T>();
            object obj = new object();

            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8
            };

            // Load web, store data into document
            HtmlAgilityPack.HtmlDocument document = htmlWeb.Load(url);

            // Parent node
            List<HtmlNode> nodeList = new List<HtmlNode>();

            switch (website)
            {
                case Constants.BHDSTAR:
                    nodeList = document.DocumentNode.QuerySelectorAll("li#film-1 div ul.slides > li").ToList();

                    foreach (var item in nodeList)
                    {
                        // Get link to move detail page
                        var movieLinkNode = item.QuerySelector("div.film--item a");
                        string movieLink = movieLinkNode.Attributes["href"].Value.Trim();

                        // Get data from detail page
                        obj = CrawlDataFromDetailPage<Movie>(movieLink, website);

                        if (obj != null)
                        {
                            objList.Add((T)obj);
                        }
                    }
                    break;
                case Constants.CGV:
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                    // Because the error "please enable javascript to view the page content agility".
                    // So use ChromeDriver and Selenium to get html page of CGV website
                    using (var driver = new ChromeDriver())
                    {
                        driver.Navigate().GoToUrl(url);
                        doc.LoadHtml(driver.PageSource);
                        driver.SwitchTo().ParentFrame();
                    }

                    nodeList = doc.DocumentNode.QuerySelectorAll("div.slideshow-containe-movier ul div > li").ToList();

                    foreach (var item in nodeList)
                    {
                        // Get link to move detail page
                        var movieLinkNode = item.QuerySelector("div.feature_film_content a");
                        string movieLink = movieLinkNode.Attributes["href"].Value.Trim();

                        // Get data from detail page
                        obj = CrawlDataFromDetailPage<Movie>(movieLink, website);

                        if (obj != null)
                        {
                            objList.Add((T)obj);
                        }
                    }
                    break;
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
                                string newsLink = WebsiteLink.KENH14 + aNode.Attributes["href"].Value.Trim();

                                // Get data from detail page
                                obj = CrawlDataFromDetailPage<News>(newsLink, website, item);

                                if (obj != null)
                                {
                                    objList.Add((T)obj);
                                }
                            }
                        }
                    }
                    break;
            }

            return objList;
        }

        private static T CrawlDataFromDetailPage<T>(string url, string website, HtmlNode parentNodeFromMainPage = null)
        {
            object obj = new object();

            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8
            };

            // Load web, store data into document
            HtmlAgilityPack.HtmlDocument document = htmlWeb.Load(url);

            HtmlNode[] detailMovieNodeArr = new HtmlNode[25];

            // Parent node
            HtmlNode nodeList = document.DocumentNode.QuerySelector("div");

            switch (website)
            {
                case Constants.BHDSTAR:
                    // Node stores all infos which relate movie
                    nodeList = document.DocumentNode.QuerySelector("div.film--detail-content-top");

                    // Get node stores info about: Rated, Director, Actors, TypeOfMovie, PremiereDate, Duration, Language
                    detailMovieNodeArr = nodeList.QuerySelectorAll("ul.film--info > li").ToArray();

                    obj = GetMovieDetailFromBhd(nodeList, detailMovieNodeArr);
                    break;
                case Constants.CGV:
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                    // Because the error "please enable javascript to view the page content agility".
                    // So use ChromeDriver and Selenium to get html page of CGV website
                    using (var driver = new ChromeDriver())
                    {
                        driver.Navigate().GoToUrl(url);
                        doc.LoadHtml(driver.PageSource);
                        driver.SwitchTo().ParentFrame();
                    }

                    // Node stores all infos which relate movie
                    nodeList = doc.DocumentNode.QuerySelector("div.product-essential");

                    // Get node stores info about Content and Trailer link
                    HtmlNode detail_trailerNode = doc.DocumentNode.QuerySelector("div.product-collateral");

                    obj = GetMovieDetailFromCgv(nodeList, detail_trailerNode);
                    break;
                case Constants.KENH14:
                    // Get information about News
                    nodeList = document.DocumentNode.QuerySelector("div.klw-body-top");

                    if (nodeList != null)
                    {
                        obj = GetDetailFromKenh14(nodeList, parentNodeFromMainPage);
                    }
                    break;
            }

            return (T)obj;
        }

        private static Movie GetMovieDetailFromBhd(HtmlNode nodeList, HtmlNode[] detailMovieNodeArr)
        {
            Movie movie = new Movie()
            {
                ImageLink = nodeList != null ? nodeList.QuerySelector("img.movie-full").Attributes["src"].Value.Trim() : "",
                Name = nodeList != null ? nodeList.QuerySelector("div.product--name h3").InnerText.Trim() : "",
                TrailerLink = nodeList != null ? nodeList.QuerySelector("a.bhd-trailer").Attributes["href"].Value.Trim() : "",
                Content = nodeList != null ? nodeList.QuerySelector("div.film--detail").InnerText.Trim() : "",

                Rated = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[0].QuerySelector("span.col-right").InnerText.Trim() : "",
                Director = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[1].QuerySelector("span.col-right").InnerText.Trim() : "",
                Actors = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[2].QuerySelector("span.col-right").InnerText.Trim() : "",
                TypeOfMovie = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[3].QuerySelector("span.col-right").InnerText.Trim() : "",
                PremiereDate = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[4].QuerySelector("span.col-right").InnerText.Trim() : "",
                Duration = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[5].QuerySelector("span.col-right").InnerText.Trim() : "",
                Language = detailMovieNodeArr.Length != 0 ? detailMovieNodeArr[6].QuerySelector("span.col-right").InnerText.Trim() : ""
            };

            return movie;
        }

        private static News GetDetailFromKenh14(HtmlNode nodeList, HtmlNode parentNodeFromMainPage)
        {
            News news = new News()
            {
                TypeOfNews = parentNodeFromMainPage.QuerySelector(".knswli-meta a").InnerText,
                PostDate = parentNodeFromMainPage.QuerySelector(".knswli-meta .knswli-time").Attributes["title"].Value,
                Title = nodeList != null ? nodeList.QuerySelector(".knc-sapo").InnerText.Trim() : "",
                Author = nodeList.QuerySelector(".kbwc-header > .kbwc-meta > .kbwcm-author").InnerText.Trim().Replace(',', ' ')
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

        private static Movie GetMovieDetailFromCgv(HtmlNode nodeList, HtmlNode detail_trailerNode)
        {
            // Get actors, duration of movie
            var actor_durationArr = nodeList.QuerySelectorAll("div.movie-actress div.std").ToArray();

            Movie movie = new Movie()
            {
                ImageLink = nodeList != null ? nodeList.QuerySelector("img#image-main").Attributes["src"].Value.Trim() : "",
                Name = nodeList != null ? nodeList.QuerySelector("div.product-name span").InnerText.Trim() : "",
                Director = nodeList != null ? nodeList.QuerySelector("div.movie-director div.std").InnerText.Trim().Substring(6) : "",
                Actors = actor_durationArr.Length != 0 ? actor_durationArr[0].InnerText.Trim().Substring(6) : "",
                TypeOfMovie = nodeList != null ? nodeList.QuerySelector("div.movie-genre div.std").InnerText.Trim().Substring(6) : "",
                PremiereDate = nodeList != null ? nodeList.QuerySelector("div.movie-release div.std").InnerText.Trim().Substring(6) : "",
                Duration = actor_durationArr.Length != 0 ? actor_durationArr[1].InnerText.Trim().Substring(6) : "",
                Language = nodeList != null ? nodeList.QuerySelector("div.movie-language div.std").InnerText.Trim().Substring(6) : "",
                Rated = nodeList != null ? nodeList.QuerySelector("div.movie-rating div.std").InnerText.Trim().Substring(6) : "",
                Content = detail_trailerNode != null ? detail_trailerNode.QuerySelector("div.tab-content .std").InnerText.Trim() : "",
                TrailerLink = detail_trailerNode != null ? detail_trailerNode.QuerySelector("div.tab-content .std iframe").Attributes["src"].Value.Trim() : "",
            };

            return movie;
        }
    }
}
