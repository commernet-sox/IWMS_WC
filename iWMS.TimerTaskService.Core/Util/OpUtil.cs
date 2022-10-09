using Common.DBCore;
using iWMS.Core;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iWMS
{
    public class OpUtil
    {
        public static void Write(string mid, string menuName, string commandId, string commanadName, string orderId, string tableName, string userId)
        {
            var data = new T_SYS_OpLogData();
            var row = data.SYS_OpLog.NewSYS_OpLogRow();
            row.MID = mid;
            row.MName = menuName;
            row.SysCode = "1";
            row.CommandId = commandId;
            row.CommandName = commanadName;
            row.OrderId = orderId;
            row.TableName = tableName;
            row.CreateBy = userId;
            row.CreateDate = DateTime.Now;
            row.ClientName = Dns.GetHostName();
            row.OperationIP = IPUtil.GetIP();
            data.SYS_OpLog.AddSYS_OpLogRow(row);
            Task.Factory.StartNew(() => BusinessDbUtil.Save(data));
        }
    }
}
