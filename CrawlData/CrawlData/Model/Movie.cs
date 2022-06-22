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

    public class CrawlDataForMovie
    {
        public List<Movie> CrawlData(string url, string website)
        {
            List<Movie> movieList = new List<Movie>();
            Movie movie = new Movie();

            try
            {
                HtmlWeb htmlWeb = new HtmlWeb()
                {
                    AutoDetectEncoding = false,
                    OverrideEncoding = Encoding.UTF8 // Set UTF8 to dsiplay vietnamese  
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
                            movie = CrawlDataFromDetailPage(movieLink, website);

                            if (movie != null)
                            {
                                movieList.Add(movie);
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
                            movie = CrawlDataFromDetailPage(movieLink, website);

                            if (movie != null)
                            {
                                movieList.Add(movie);
                            }
                        }
                        break;
                    default:
                        break;
                }

                return movieList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return movieList;
            }
        }

        private Movie CrawlDataFromDetailPage(string url, string website)
        {
            Movie movie = new Movie();

            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8 // Set UTF8 to dsiplay vietnamese  
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

                    movie = GetMovieDetailFromBhd(nodeList, detailMovieNodeArr);
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

                    movie = GetMovieDetailFromCgv(nodeList, detail_trailerNode);
                    break;
                default:
                    break;
            }

            return movie;
        }

        private Movie GetMovieDetailFromBhd(HtmlNode nodeList, HtmlNode[] detailMovieNodeArr)
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

        private Movie GetMovieDetailFromCgv(HtmlNode nodeList, HtmlNode detail_trailerNode)
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
