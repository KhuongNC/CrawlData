using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CrawlData.Model;
using CrawlData.Common;
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
        readonly string currentDate = DateTime.Now.ToString("yyyyMMdd");
        public Form1()
        {
            InitializeComponent();

            // Add items for Website combobox
            CbbWebsite.Items.Add(WebsiteLink.BHDSTAR);
            CbbWebsite.Items.Add(WebsiteLink.CGV);
            CbbWebsite.Items.Add(WebsiteLink.KENH14);
            //CbbWebsite.Items.Add(WebsiteLink._24H);

            // Add items for Formatter combobox
            CbbFormatter.Items.Add("txt");
            CbbFormatter.Items.Add("csv");
            CbbFormatter.Items.Add("pdf");
            //CbbFormatter.Items.Add(".xlsx");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (CbbFormatter.SelectedItem != null && CbbWebsite.SelectedItem != null)
                {
                    string website;

                    // Get name of website
                    if (CbbWebsite.SelectedItem.ToString().IndexOf("www") != -1)
                    {
                        website = CbbWebsite.SelectedItem.ToString().Split('.')[1].ToUpper();
                    }
                    else
                    {
                        website = CbbWebsite.SelectedItem.ToString().Split('.')[0].Substring(8).ToUpper();
                    }

                    string url = CbbWebsite.SelectedItem.ToString();
                    string extension = "." + CbbFormatter.SelectedItem.ToString();

                    dynamic dataList = null;

                    MessageBox.Show("Start getting data...");
                    switch (website)
                    {
                        case Constants.BHDSTAR:
                        case Constants.CGV:
                            dataList = CrawlDataForMovie(url, website);
                            break;
                        case Constants.KENH14:
                            dataList = CrawlDataForNews(url, website);
                            break;
                        default:
                            break;
                    }

                    if (dataList == null || dataList.Count == 0)
                    {
                        MessageBox.Show("There are something wrong when crawl data from " + website + " website", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    MessageBox.Show("Start exporting data...");
                    switch (extension)
                    {
                        case ExtensionOfFile.TXT:
                            ExportToTextFile(dataList, website, extension);
                            break;
                        case ExtensionOfFile.CSV:
                            ExportToCsvFile(dataList, website, extension);
                            break;
                        case ExtensionOfFile.PDF:
                            ExportToPdfFile(dataList, website, extension);
                            break;
                        case ExtensionOfFile.XLSX:
                            //ExportToTextFile(dataList, website, extension);
                            break;
                        default:
                            MessageBox.Show("Please select extension of file");
                            break;
                    }

                    Cursor = Cursors.Arrow;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show("Website and Formatter cannot be blank. Please enter!");
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                MessageBox.Show(ex.Message.ToString());
            }
        }

        #region Crawl data
        private List<Movie> CrawlDataForMovie(string url, string website)
        {
            return Utilities.CrawlData.CrawlDataFromWebsite<Movie>(url, website);
        }

        private List<News> CrawlDataForNews(string url, string website)
        {
            return Utilities.CrawlData.CrawlDataFromWebsite<News>(url, website);
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

                    switch (typeof(T).Name)
                    {
                        case FieldType.MOVIE:
                            writer.WriteRow(AddHeader(FieldType.MOVIE));

                            foreach (var item in list)
                            {
                                writer.WriteRow(AddRow(FieldType.MOVIE, item));
                            }

                            break;
                        case FieldType.NEWS:
                            writer.WriteRow(AddHeader(FieldType.NEWS));

                            foreach (var item in list)
                            {
                                writer.WriteRow(AddRow(FieldType.NEWS, item));
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

        private CsvRow AddHeader(string fieldType)
        {
            CsvRow header = new CsvRow();

            switch (fieldType)
            {
                case FieldType.MOVIE:
                    header.Add(string.Format("{0}", "Name"));
                    header.Add(string.Format("{0}", "Director"));
                    header.Add(string.Format("{0}", "Actors"));
                    header.Add(string.Format("{0}", "TypeOfMovie"));
                    header.Add(string.Format("{0}", "PremiereDate"));
                    header.Add(string.Format("{0}", "Duration"));
                    header.Add(string.Format("{0}", "Language"));
                    header.Add(string.Format("{0}", "Rated"));
                    header.Add(string.Format("{0}", "Content"));
                    header.Add(string.Format("{0}", "ImageLink"));
                    header.Add(string.Format("{0}", "TrailerLink"));

                    break;
                case FieldType.NEWS:
                    header.Add(string.Format("{0}", "Title"));
                    header.Add(string.Format("{0}", "Content"));
                    header.Add(string.Format("{0}", "ImageUrl"));
                    header.Add(string.Format("{0}", "TypeOfNews"));
                    header.Add(string.Format("{0}", "PostDate"));
                    break;
                default:
                    break;
            }

            return header;
        }

        private CsvRow AddRow<T>(string fieldType, T item)
        {
            CsvRow row = new CsvRow();

            switch (fieldType)
            {
                case FieldType.MOVIE:
                    var movie = (Movie)Convert.ChangeType(item, typeof(Movie));

                    row.Add(string.Format("{0}", movie.Name));
                    row.Add(string.Format("{0}", movie.Director));
                    row.Add(string.Format("{0}", movie.Actors));
                    row.Add(string.Format("{0}", movie.TypeOfMovie));
                    row.Add(string.Format("{0}", movie.PremiereDate));
                    row.Add(string.Format("{0}", movie.Duration));
                    row.Add(string.Format("{0}", movie.Language));
                    row.Add(string.Format("{0}", movie.Rated));
                    row.Add(string.Format("{0}", movie.Content));
                    row.Add(string.Format("{0}", movie.ImageLink));
                    row.Add(string.Format("{0}", movie.TrailerLink));

                    break;
                case FieldType.NEWS:
                    var news = (News)Convert.ChangeType(item, typeof(News));

                    row.Add(string.Format("{0}", news.Title));
                    row.Add(string.Format("{0}", news.Content));
                    row.Add(string.Format("{0}", news.ImageUrl));
                    row.Add(string.Format("{0}", news.TypeOfNews));
                    row.Add(string.Format("{0}", news.PostDate));
                    break;
                default:
                    break;
            }

            return row;
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
