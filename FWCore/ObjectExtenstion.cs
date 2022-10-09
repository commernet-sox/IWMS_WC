using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FWCore
{
    public static class ObjectExtenstion
    {
        #region DataSetExtenstion
        public static bool Merge(this DataSet dsDest, string tableName, DataSet dsSrc, MissingSchemaAction msAction)
        {
            if ((null == dsSrc) || (null == dsDest)) return false;
            if ((null == dsSrc.Tables[tableName]) || (null == dsDest.Tables[tableName])) return false;

            dsDest.Tables[tableName].Clear();
            dsDest.Tables[tableName].AcceptChanges();

            dsDest.Tables[tableName].Merge(dsSrc.Tables[tableName], true, msAction);

            return true;
        }
        public static bool Merge(this DataSet dsDest, string tableName, DataSet dsSrc)
        {
            return Merge(dsDest, tableName, dsSrc, MissingSchemaAction.Ignore);
        }

        /// <summary>
        /// 结构完全一样时，建议用这个方法替换Merge方法；效率更好
        /// </summary>
        /// <param name="dsDest"></param>
        /// <param name="tableName"></param>
        /// <param name="dsSrc"></param>
        public static void Import(this DataSet dsDest, string tableName, DataSet dsSrc)
        {
            var destTable = dsDest.Tables[tableName];
            destTable.Clear();
            destTable.AcceptChanges();

            destTable.BeginLoadData();
            foreach (DataRow rawRow in dsSrc.Tables[tableName].Rows)
            {
                destTable.ImportRow(rawRow);
            }
            destTable.EndLoadData();
            destTable.AcceptChanges();
        }

        public static void CopyRow(this DataRow destRow, DataRow origRow)
        {
            Type rowtp = destRow.GetType();
            PropertyInfo[] piarray = rowtp.GetProperties();
            for (int i = 0; i < piarray.Length; i++)
            {
                PropertyInfo pi = piarray[i];

                try
                {
                    object ovalue = pi.GetValue(origRow, null);

                    pi.SetValue(destRow, ovalue, null);
                }
                catch { continue; }
            }
        }
        #endregion

        #region StreamExtenstion
        public static byte[] ReadFully(this Stream source)
        {
            if (source == null)
            {
                throw new NullReferenceException("source");
            }

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        #endregion

        #region ObjectExtenstion
        public static byte ConvertByte(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value) || string.IsNullOrEmpty(obj.ToString()))
            {
                return 0;
            }
            byte result = 0;
            byte.TryParse(obj.ToString(), out result);
            return result;
        }

        public static int ConvertInt32(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value) || string.IsNullOrEmpty(obj.ToString()))
            {
                return 0;
            }

            var str = obj.ToString();
            if (str.Contains("."))
            {
                return ConvertDecimal(obj).ConvertInt32();
            }

            int result = 0;
            int.TryParse(str, out result);
            return result;
        }

        public static int ConvertInt32(this decimal obj)
        {
            if (obj > int.MaxValue || obj < int.MinValue)
            {
                return 0;
            }

            return Convert.ToInt32(obj);
        }

        public static long ConvertLong(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value) || string.IsNullOrEmpty(obj.ToString()))
            {
                return 0;
            }
            long result = 0;
            long.TryParse(obj.ToString(), out result);
            return result;
        }

        public static decimal ConvertDecimal(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value) || string.IsNullOrEmpty(obj.ToString()))
            {
                return 0;
            }
            decimal result;
            decimal.TryParse(obj.ToString(), out result);
            return result;
        }

        public static double ConvertDouble(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value) || string.IsNullOrEmpty(obj.ToString()))
            {
                return 0;
            }
            double result;
            double.TryParse(obj.ToString(), out result);
            return result;
        }

        public static string ConvertString(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return string.Empty;
            }
            return obj.ToString();
        }

        public static DateTime? ConvertDateTime(this object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }
            DateTime result;
            if (DateTime.TryParse(obj.ToString(), out result))
            {
                return result;
            }
            return null;
        }
        #endregion
    }
}
