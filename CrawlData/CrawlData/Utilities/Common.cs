using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrawlData.Utilities
{
    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream) : base(stream)
        {
        }

        public CsvFileWriter(string fileName) : base(fileName)
        {
        }

        public void WriteRow(CsvRow row)
        {
            StringBuilder builder = new StringBuilder();
            bool isFirstColumn = true;

            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!isFirstColumn)
                    builder.Append(',');
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                isFirstColumn = false;
            }

            row.LineText = builder.ToString();
            WriteLine(row.LineText);
        }
    }
}
