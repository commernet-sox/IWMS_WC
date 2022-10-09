using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Util
{
    public class ExcelHelper
    {
        public static void ExportToExcel(string fileName, IDictionary<string, string> headerCols, DataTable dataSource)
        {
            ExcelFile ef = new ExcelFile();
            ExcelWorksheet ws = ef.Worksheets.Add(dataSource.TableName);
            ws.InsertDataTable(headerCols, dataSource);

            ExportToExcel(fileName, ef, dataSource.Rows.Count < 65535 ? "XLS" : "XLSX");
        }

        private static void ExportToExcel(string fileName, ExcelFile ef, string exportType)
        {
            switch (exportType.ToUpper())
            {
                case "XLS":
                    ef.SaveXls(fileName);
                    break;
                case "XLSX":
                    ef.SaveXlsx(fileName);
                    break;
            }
        }
    }

    public static class ExcelFileExtension
    {
        public static void InsertDataTable(this ExcelWorksheet sheet, IDictionary<string, string> headers, DataTable dataSource)
        {
            if (sheet == null)
            {
                throw new ArgumentNullException("sheet");
            }
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            if (headers.Count == 0)
            {
                throw new InvalidOperationException("headers.Count is ZERO");
            }

            bool genTitle = true;

            Int32 startRowIndex = 0;
            Int32 startColIndex = 0;

            Int32 rowIndex = startRowIndex;
            Int32 colIndex = startColIndex;

            Int32 j = 0;
            //Gen Title
            if (genTitle)
            {
                j = 0;
                foreach (var kv in headers)
                {
                    GenHeadSingleCell(sheet, rowIndex, colIndex + j, kv.Value);
                    j++;
                }
                startRowIndex++;
            }

            rowIndex = startRowIndex;
            colIndex = startColIndex;

            int columns = headers.Count;
            int rows = dataSource.Rows.Count;

            //先遍历列
            j = 0;
            foreach (var kv in headers)
            {
                if (!dataSource.Columns.Contains(kv.Key))
                {
                    continue;
                }

                AssitantEnum aenum = GetColFormat(dataSource.Columns[kv.Key].DataType);
                for (Int32 i = 0; i < rows; i++)
                {
                    Object value = dataSource.Rows[i][kv.Key];
                    if (value == null || value == DBNull.Value)
                    {
                        continue;
                    }

                    ExcelCell cell = sheet.Cells[rowIndex + i, colIndex + j];
                    switch (aenum)
                    {
                        case AssitantEnum.String:
                            cell.Style.NumberFormat = "@";
                            value = value.ToString().Trim();
                            break;
                        case AssitantEnum.Double:
                            Double oDouble;
                            if (Double.TryParse(value.ToString(), out oDouble))
                            {
                                value = oDouble;
                            }
                            break;
                        case AssitantEnum.Date:
                            String format = "yyyy-MM-dd";
                            DateTime oDT;
                            if (DateTime.TryParse(value.ToString(), out oDT))
                            {
                                if (oDT.Minute > 0)
                                {
                                    format = "yyyy-MM-dd HH:mm";
                                }
                            }
                            cell.Style.NumberFormat = format;
                            break;

                        case AssitantEnum.Blank:
                        default:
                            break;
                    }

                    cell.Value = value;
                }
                j++;
            }

            startRowIndex += rows;
        }

        public static void GenHeadSingleCell(ExcelWorksheet sheet, int rowIndex, int colIndex, Object value)
        {
            ExcelCell cell = sheet.Cells[rowIndex, colIndex];

            cell.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            cell.Style.VerticalAlignment = VerticalAlignmentStyle.Center;
            cell.Style.NumberFormat = "@";
            cell.Style.Font.Weight = ExcelFont.BoldWeight;
            cell.Value = value;
        }

        private static AssitantEnum GetColFormat(Type colDataType)
        {
            AssitantEnum ret = AssitantEnum.Blank;
            if (colDataType == null)
            {
                return ret;
            }

            if (colDataType == typeof(String) ||
                colDataType == typeof(Char))
            {
                ret = AssitantEnum.String;
            }
            else if (colDataType == typeof(Decimal) ||
                    colDataType == typeof(Double) ||
                    colDataType == typeof(Int16) ||
                    colDataType == typeof(Int32) ||
                    colDataType == typeof(Int64) ||
                    colDataType == typeof(Single) ||
                    colDataType == typeof(UInt16) ||
                    colDataType == typeof(UInt32) ||
                    colDataType == typeof(UInt64))
            {
                ret = AssitantEnum.Double;
            }
            else if (colDataType == typeof(DateTime))
            {
                ret = AssitantEnum.Date;
            }

            return ret;
        }

        private enum AssitantEnum
        {
            Blank,
            String,
            Double,
            Date
        }
    }
}
