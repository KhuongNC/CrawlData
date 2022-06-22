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
        string currentDate = DateTime.Now.ToString("yyyyMMdd");
        public Form1()
        {
            InitializeComponent();

            // Add items for Website combobox
            CbbWebsite.Items.Add("https://www.bhdstar.vn/");
            CbbWebsite.Items.Add("https://www.cgv.vn/");
            CbbWebsite.Items.Add("https://kenh14.vn/");
            //CbbWebsite.Items.Add("https://www.24h.com.vn/");

            // Add items for Formatter combobox
            CbbFormatter.Items.Add(".txt");
            CbbFormatter.Items.Add(".csv");
            CbbFormatter.Items.Add(".pdf");
            //CbbFormatter.Items.Add(".xlsx");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            if (CbbFormatter.SelectedItem != null && CbbWebsite.SelectedItem != null)
            {
                string website = string.Empty;

                if (CbbWebsite.SelectedItem.ToString().IndexOf("www") != -1)
                {
                    website = CbbWebsite.SelectedItem.ToString().Split('.')[1].ToUpper();
                }
                else
                {
                    website = CbbWebsite.SelectedItem.ToString().Split('.')[0].Substring(8).ToUpper();
                }

                string url = CbbWebsite.SelectedItem.ToString();
                string extension = CbbFormatter.SelectedItem.ToString();


                dynamic dataList = null;

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

            }
            else
            {
                MessageBox.Show("Website and Formatter cannot be blank. Please enter!");
            }

            Cursor = Cursors.Arrow;
        }

        #region Crawl data
        private List<Movie> CrawlDataForMovie(string url, string website)
        {
            CrawlDataForMovie data = new CrawlDataForMovie();
            return data.CrawlData(url, website);
        }

        private List<News> CrawlDataForNews(string url, string website)
        {
            CrawlDataForNews data = new CrawlDataForNews();
            return data.CrawlData(url, website);
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
