using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace GrScore
{
    class XlsExporter
    {
        /// <summary>
        /// CSV格式导出Excel
        /// </summary>
        public DataGrid dataGrid { get; set; }
        public XlsExporter(DataGrid _dataGrid)
        {
            dataGrid = _dataGrid;
        }
        /// <summary>
        /// 导出DataGrid控件的内容到CSV格式文件（可以用excel打开）
        /// </summary>
        /// <param name="withHeaders"></param>
        /// <returns></returns>
        public string ExportDataGrid(bool withHeaders)
        {
            string colPath;
            System.Reflection.PropertyInfo propInfo;
            System.Windows.Data.Binding binding;
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            System.Collections.IEnumerable source = (dataGrid.ItemsSource as System.Collections.IEnumerable);

            if (source == null) return "";

            List<string> headers = new List<string>();

            dataGrid.Columns.ToList().ForEach(col =>
            {
                if (col is DataGridBoundColumn)
                {
                    headers.Add(FormatCSVField(col.Header.ToString()));
                }
            });

            //strBuilder.Append(String.Join(",", headers.ToArray())).Append("\r\n");
            strBuilder.Append(String.Join(" ", headers.ToArray())).Append("\r\n");//不用逗号分隔，而是用空格
            foreach (Object data in source)
            {
                List<string> csvRow = new List<string>();
                foreach (DataGridColumn col in dataGrid.Columns)
                {
                    if (col is DataGridBoundColumn)
                    {
                        binding = (Binding)(col as DataGridBoundColumn).Binding;
                        colPath = binding.Path.Path;
                        propInfo = data.GetType().GetProperty(colPath);

                        if (propInfo != null)
                        {
                            if (propInfo.GetValue(data, null) != null)
                            {
                                csvRow.Add(FormatCSVField(propInfo.GetValue(data, null).ToString()));
                            }
                            else
                            {
                                csvRow.Add(string.Empty);
                            }
                        }
                    }
                }

                //strBuilder.Append(String.Join(",", csvRow.ToArray())).Append("\r\n");
                strBuilder.Append(String.Join(" ", csvRow.ToArray())).Append("\r\n");
            }

            return strBuilder.ToString();
        }
        private static string FormatCSVField(string data)
        {
            //return String.Format("\"{0}\"", data.Replace("\"", "\"\"\"").Replace("\n", "").Replace("\r", ""));
            return String.Format("\t{0}", data.Replace("\"", "\t\n"));
        }
    }
}
