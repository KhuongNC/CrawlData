using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlData.Model;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using CrawlData.Common;

namespace CrawlData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Add items for Website combobox
            CbbWebsite.Items.Add("https://www.bhdstar.vn/");
            CbbWebsite.Items.Add("https://www.cgv.vn/");
            CbbWebsite.Items.Add("https://www.24h.com.vn/");
            CbbWebsite.Items.Add("https://kenh14.vn/");

            // Add items for Formatter combobox
            CbbFormatter.Items.Add(".txt");
            CbbFormatter.Items.Add(".csv");
            CbbFormatter.Items.Add(".xlsx");
            CbbFormatter.Items.Add(".pdf");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            string website = CbbWebsite.SelectedItem.ToString().Split('.')[1].ToUpper();
            string url = CbbWebsite.SelectedItem.ToString();
            List<Movie> movieList = CrawlData(url, website);
            MessageBox.Show("Export successfully");
        }

        private List<Movie> CrawlData(string url, string website)
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
                List<HtmlNode> nodeList = new List<HtmlNode>();

                switch (website)
                {
                    case Constants.BHDSTAR:
                        nodeList = document.DocumentNode.QuerySelectorAll("li#film-1 div ul.slides > li").ToList();

                        // Get link to move detail page
                        foreach (var item in nodeList)
                        {
                            var movieLinkNode = item.QuerySelector("div.film--item a");
                            string movieLink = movieLinkNode.Attributes["href"].Value.Trim();
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
            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8 // Set UTF8 to dsiplay vietnamese  
            };

            // Load web, store data into document
            HtmlAgilityPack.HtmlDocument document = htmlWeb.Load(url);
            HtmlNode nodeList = document.DocumentNode.QuerySelector("div");
            HtmlNode[] detailMovieNodeArr = new HtmlNode[25];
            Movie movie = new Movie();

            switch (website)
            {
                case Constants.BHDSTAR:
                    // Get information about movie
                    nodeList = document.DocumentNode.QuerySelector("div.film--detail-content-top");
                    detailMovieNodeArr = nodeList.QuerySelectorAll("ul.film--info > li").ToArray();
                    movie = GetMovieDetailFromBhd(nodeList, detailMovieNodeArr);
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
    }
}
