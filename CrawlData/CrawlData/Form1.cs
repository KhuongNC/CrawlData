using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrawlData.Model;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using CrawlData.Common;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.IO;
using CrawlData.Utilities;
using iTextSharp.text;
using Newtonsoft.Json.Linq;

namespace CrawlData
{
    public partial class Form1 : Form
    {
        readonly string rootPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
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
            if (CbbFormatter.SelectedItem != null && CbbWebsite.SelectedItem != null)
            {
                string website = CbbWebsite.SelectedItem.ToString().Split('.')[1].ToUpper();
                string url = CbbWebsite.SelectedItem.ToString();
                string extension = CbbFormatter.SelectedItem.ToString();

                Cursor = Cursors.WaitCursor;
                List<Movie> movieList = CrawlData(url, website);

                switch (extension)
                {
                    case ExtensionOfFile.TXT:
                        ExportToTextFile(movieList, website, extension);
                        break;
                    case ExtensionOfFile.CSV:
                        ExportToCsvFile(movieList, website, extension);
                        break;
                    case ExtensionOfFile.PDF:
                        ExportToPdfFile(movieList, website, extension);
                        break;
                    case ExtensionOfFile.XLSX:
                        ExportToTextFile(movieList, website, extension);
                        break;
                    default:
                        MessageBox.Show("Please select extension of file");
                        break;
                }

                Cursor = Cursors.Arrow;
            }
            else
            {
                MessageBox.Show("Website and Formatter cannot be blank. Please enter!");
            }
        }

        #region Crawl data
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

                        // Get link to move detail page
                        foreach (var item in nodeList)
                        {
                            var movieLinkNode = item.QuerySelector("div.feature_film_content a");
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

                    // Get information about movie
                    nodeList = doc.DocumentNode.QuerySelector("div.product-essential");
                    HtmlNode detail_trailerNode = doc.DocumentNode.QuerySelector("div.product-collateral");
                    detailMovieNodeArr = nodeList.QuerySelectorAll("div.product-shop > div").ToArray();
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

        #endregion

        #region Export data
        private void ExportToTextFile<T>(List<T> list, string website, string extension)
        {
            try
            {
                // Pretty Json string
                var json = JsonConvert.SerializeObject(list);
                string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented);

                string pathToSave = rootPath + "\\" + currentDate;

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                File.WriteAllText(pathToSave + "\\" + website + extension, jsonFormatted);

                MessageBox.Show("Export successfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsvFile<T>(List<T> list, string website, string extension)
        {
            try
            {
                string pathToSave = rootPath + "\\" + currentDate;

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                using (CsvFileWriter writer = new CsvFileWriter(pathToSave + "\\" + website + extension))
                {
                    CsvRow header = new CsvRow
                    {
                        string.Format("{0}","Name"),
                        string.Format("{0}","Director"),
                        string.Format("{0}","Actors"),
                        string.Format("{0}","TypeOfMovie"),
                        string.Format("{0}","PremiereDate"),
                        string.Format("{0}","Duration"),
                        string.Format("{0}","Language"),
                        string.Format("{0}","Rated"),
                        string.Format("{0}","Content"),
                        string.Format("{0}","ImageLink"),
                        string.Format("{0}","TrailerLink")
                    };

                    writer.WriteRow(header);

                    switch (typeof(T).Name)
                    {
                        case FieldType.MOVIE:
                            foreach (var item in list)
                            {
                                var x = (Movie)Convert.ChangeType(item, typeof(Movie));

                                CsvRow row = new CsvRow
                                {
                                    string.Format("{0}",x.Name),
                                    string.Format("{0}",x.Director),
                                    string.Format("{0}",x.Actors),
                                    string.Format("{0}",x.TypeOfMovie),
                                    string.Format("{0}",x.PremiereDate),
                                    string.Format("{0}",x.Duration),
                                    string.Format("{0}",x.Language),
                                    string.Format("{0}",x.Rated),
                                    string.Format("{0}",x.Content),
                                    string.Format("{0}",x.ImageLink),
                                    string.Format("{0}",x.TrailerLink)
                                };

                                writer.WriteRow(row);
                            }
                            break;
                        default:
                            break;
                    }

                }

                MessageBox.Show("Export successfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToPdfFile<T>(List<T> list, string website, string extension)
        {
            try
            {
                string pathToSave = rootPath + "\\" + currentDate;

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                // Pretty Json string
                var json = JsonConvert.SerializeObject(list);
                string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented);

                string fileName = Path.Combine(pathToSave, website + extension);
                var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                var document = new Document(PageSize.A4);
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, fs);
                document.Open();
                document.NewPage();
                Paragraph content = new Paragraph(jsonFormatted);
                document.Add(content);
                document.Close();
                fs.Close();

                MessageBox.Show("Export successfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
