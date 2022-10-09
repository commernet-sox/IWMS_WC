using Common.DBCore;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// APP库存调整通知单，自动记账
    /// </summary>
    public class APPStockAdjustNotifyTask : BaseTask
    {
        private const string SelectSql = @"SELECT TOP 100 * FROM WRM_AdjustNotify WHERE UpLoadFlag=0 AND Status='10'";

        private const string UpdateSql = @"UPDATE WRM_AdjustNotify SET UpLoadFlag=1, Memo='{1}',ModifyBy='TimerTask',ModifyDate=GETDATE(),Version=Version+1 WHERE BillId='{0}'";

        private const string InvSelectSql = @"SELECT 1 AS CheckStatus,C.StorerId,C.WarehouseId,C.LocationId,C.CID,C.SkuId,C.SkuStatus,C.CID,C.BatchNo,C.BoxCode,C.QtyValid,
C.DUDF01,C.DUDF02,C.DUDF03,C.DUDF04,C.DUDF05,C.DUDF06,C.DUDF07,C.DUDF08,C.DUDF09,C.DUDF10,C.DUDF11,C.PackageCode,C.PackageQty,C.ProductDate,C.ExpiryDate,B.OrderLineNo,B.QtyPlan,B.UDF13,C.UDF01,C.UDF02
FROM WRM_AdjustNotify A INNER JOIN WRM_AdjustNotifyDetail B ON A.BillId=B.BillId
INNER JOIN INV_BAL C ON (A.WarehouseId=C.WarehouseId and B.StorerId=C.StorerId and B.SkuId=C.SkuId and 
B.BatchNo=C.BatchNo AND ISNULL(B.BoxCode,'')=ISNULL(C.BoxCode,'') AND ISNULL(B.DUDF01,'')=ISNULL(C.DUDF01,'')
AND ISNULL(B.DUDF02,'')=ISNULL(C.DUDF02,'') AND ISNULL(B.DUDF03,'')=ISNULL(C.DUDF03,'') AND ISNULL(B.DUDF04,'')=ISNULL(C.DUDF04,''))
INNER JOIN COM_Location D WITH (NOLOCK) ON C.LocationId=D.LocationId
INNER JOIN COM_WHArea E WITH (NOLOCK) ON D.WHAreaId=E.WHAreaId
WHERE A.BillId='{0}' AND C.QtyValid>0 AND B.QtyPlan<0";

//        private const string UpdateInvSql = @"UPDATE A SET A.DUDF02='',A.Version=A.Version+1,A.ModifyBy='{1}',A.ModifyDate=GETDATE()
//FROM INV_BAL A INNER JOIN WRM_AdjustNotifyDetail B ON (A.StorerId=B.StorerId and A.SkuId=B.SkuId AND A.BatchNo=B.BatchNo 
//AND ISNULL(A.DUDF01,'')=ISNULL(B.DUDF01,'')AND ISNULL(A.DUDF02,'')=ISNULL(B.DUDF02,'') AND ISNULL(A.DUDF03,'')=ISNULL(B.DUDF03,'') 
//AND ISNULL(A.DUDF04,'')=ISNULL(B.DUDF04,''))
//WHERE B.BillId='{0}'";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                DataTable table = BusinessDbUtil.GetDataTable(SelectSql);
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        lock (LockObject)
                        {
                            try
                            {

//                                var stockTable = BusinessDbUtil.GetDataTable(string.Format(InvSelectSql, row["BillId"]));
//                                if (stockTable == null || stockTable.Rows.Count < 1)
//                                {
//                                    BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateSql, row["BillId"], "库存中不存在符合条件的数据！"));
//                                    continue;
//                                }
//                                var groups = stockTable.AsEnumerable().GroupBy(item => item["OrderLineNo"].ToString());
//                                foreach (var group in groups)
//                                {
//                                    var groupRow = group.FirstOrDefault();
//                                    decimal totalCount = groupRow["QtyPlan"].ConvertDecimal();
//                                    decimal tmpCount = 0;
//                                    foreach (var tmpRow in group.ToList())
//                                    {
//                                        if (tmpCount + tmpRow["QtyValid"].ConvertDecimal() > Math.Abs(totalCount))
//                                        {
//                                            tmpRow["QtyValid"] = Math.Abs(totalCount) - tmpCount;
//                                            if (totalCount < 0)
//                                            {
//                                                tmpRow["QtyValid"] = -tmpRow["QtyValid"].ConvertDecimal();
//                                            }
//                                            break;
//                                        }
//                                        else
//                                        {
//                                            tmpCount += tmpRow["QtyValid"].ConvertDecimal();
//                                            if (totalCount < 0)
//                                            {
//                                                tmpRow["QtyValid"] = -tmpRow["QtyValid"].ConvertDecimal();
//                                            }
//                                        }
//                                    }
//                                }

//                                //处理数据
//                                string sql = string.Format(@"SELECT A.StorerId,A.WarehouseId,B.SkuId,B.SkuStatus,B.BatchNo,B.BoxCode,
//B.DUDF01,B.DUDF02,B.DUDF03,B.DUDF04,B.DUDF05,B.DUDF06,B.DUDF07,B.DUDF08,B.DUDF09,B.DUDF10,B.DUDF11,B.PackageCode,B.PackageQty,
//B.OrderLineNo,B.QtyPlan,B.UDF13
//FROM WRM_AdjustNotify A INNER JOIN WRM_AdjustNotifyDetail B ON A.BillId=B.BillId
//WHERE A.BillId='{0}' AND B.QtyPlan>0", row["BillId"]);
//                                var resultTable = BusinessDbUtil.GetDataTable(sql);
//                                if (resultTable == null || resultTable.Rows.Count < 1)
//                                {
//                                    BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateSql, row["BillId"], "数据异常，请联系系统管理员！"));
//                                    continue;
//                                }

//                                T_WRM_AdjustNotifyData data = new T_WRM_AdjustNotifyData();
//                                foreach (DataRow notifyRow in stockTable.Rows)
//                                {
//                                    //负数
//                                    var detailRow = data.WRM_AdjustNotifyRecord.NewWRM_AdjustNotifyRecordRow();
//                                    detailRow.BillId = row["BillId"].ToString();
//                                    detailRow.OrderLineNo = notifyRow["OrderLineNo"].ToString();
//                                    detailRow.StorerId = notifyRow["StorerId"].ToString();
//                                    detailRow.SkuId = notifyRow["SkuId"].ToString();
//                                    detailRow.SkuStatus = notifyRow["SkuStatus"].ToString();
//                                    detailRow.QtyPlan = notifyRow["QtyPlan"].ConvertDecimal();
//                                    detailRow.QtyScan = notifyRow["QtyValid"].ConvertDecimal();
//                                    detailRow.LocationId = notifyRow["LocationId"].ToString();
//                                    detailRow.CID = notifyRow["CID"].ToString();
//                                    detailRow.BatchNo = notifyRow["BatchNo"].ToString();
//                                    detailRow.BoxCode = notifyRow["BoxCode"].ToString();
//                                    detailRow.PackageCode = notifyRow["PackageCode"].ToString();
//                                    detailRow.PackageQty = notifyRow["PackageQty"].ConvertDecimal();
//                                    var date = notifyRow["ProductDate"].ConvertDateTime();
//                                    if (date != null)
//                                    {
//                                        detailRow.ProductDate = notifyRow["ProductDate"].ConvertDateTime().Value;
//                                    }
//                                    date = notifyRow["ExpiryDate"].ConvertDateTime();
//                                    if (date != null)
//                                    {
//                                        detailRow.ExpiryDate = notifyRow["ExpiryDate"].ConvertDateTime().Value;
//                                    }
//                                    detailRow.Version = 0;
//                                    detailRow.CreateBy = GlobalContext.CurUserInfo.UserId;
//                                    detailRow.CreateDate = GlobalContext.ServerTime;
//                                    detailRow.UDF01 = notifyRow["UDF01"].ToString();
//                                    detailRow.UDF02 = notifyRow["UDF02"].ToString();
//                                    detailRow.DUDF01 = notifyRow["DUDF01"].ToString();
//                                    detailRow.DUDF02 = notifyRow["DUDF02"].ToString();
//                                    detailRow.DUDF03 = notifyRow["DUDF03"].ToString();
//                                    detailRow.DUDF04 = notifyRow["DUDF04"].ToString();
//                                    detailRow.DUDF05 = notifyRow["DUDF05"].ToString();
//                                    detailRow.DUDF06 = notifyRow["DUDF06"].ToString();
//                                    detailRow.DUDF07 = notifyRow["DUDF07"].ToString();
//                                    detailRow.DUDF08 = notifyRow["DUDF08"].ToString();
//                                    detailRow.DUDF09 = notifyRow["DUDF09"].ToString();
//                                    detailRow.DUDF10 = notifyRow["DUDF10"].ToString();
//                                    detailRow.DUDF11 = notifyRow["DUDF11"].ToString();
//                                    detailRow.UDF13 = notifyRow["UDF13"].ToString();

//                                    data.WRM_AdjustNotifyRecord.AddWRM_AdjustNotifyRecordRow(detailRow);


//                                    //正数
//                                    var tmpRow = resultTable.AsEnumerable().FirstOrDefault(
//                                        item => item["SkuId"].ToString() == notifyRow["SkuId"].ToString());
//                                    if (tmpRow == null)
//                                    {
//                                        BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateSql, row["BillId"], "数据异常[无法找到调整SKU]，请联系系统管理员！"));
//                                        continue;
//                                    }
//                                    var detailAddRow = data.WRM_AdjustNotifyRecord.NewWRM_AdjustNotifyRecordRow();
//                                    detailAddRow.BillId = row["BillId"].ToString();
//                                    detailAddRow.OrderLineNo = tmpRow["OrderLineNo"].ToString();
//                                    detailAddRow.StorerId = tmpRow["StorerId"].ToString();
//                                    detailAddRow.SkuId = notifyRow["SkuId"].ToString();
//                                    detailAddRow.SkuStatus = notifyRow["SkuStatus"].ToString();
//                                    detailAddRow.QtyPlan = tmpRow["QtyPlan"].ConvertDecimal();
//                                    detailAddRow.QtyScan = Math.Abs(notifyRow["QtyValid"].ConvertDecimal());
//                                    detailAddRow.LocationId = notifyRow["LocationId"].ToString();
//                                    detailAddRow.CID = notifyRow["CID"].ToString();
//                                    detailAddRow.BatchNo = tmpRow["BatchNo"].ToString();
//                                    detailAddRow.BoxCode = tmpRow["BoxCode"].ToString();
//                                    detailAddRow.PackageCode = tmpRow["PackageCode"].ToString();
//                                    detailAddRow.PackageQty = tmpRow["PackageQty"].ConvertDecimal();
//                                    date = notifyRow["ProductDate"].ConvertDateTime();
//                                    if (date != null)
//                                    {
//                                        detailAddRow.ProductDate = notifyRow["ProductDate"].ConvertDateTime().Value;
//                                    }
//                                    date = notifyRow["ExpiryDate"].ConvertDateTime();
//                                    if (date != null)
//                                    {
//                                        detailAddRow.ExpiryDate = notifyRow["ExpiryDate"].ConvertDateTime().Value;
//                                    }
//                                    detailAddRow.Version = 0;
//                                    detailAddRow.CreateBy = GlobalContext.CurUserInfo.UserId;
//                                    detailAddRow.CreateDate = GlobalContext.ServerTime;
//                                    detailAddRow.UDF01 = notifyRow["UDF01"].ToString();
//                                    detailAddRow.UDF02 = notifyRow["UDF02"].ToString();
//                                    detailAddRow.DUDF01 = notifyRow["DUDF01"].ToString();
//                                    detailAddRow.DUDF02 = tmpRow["DUDF02"].ToString();
//                                    detailAddRow.DUDF03 = notifyRow["DUDF03"].ToString();
//                                    detailAddRow.DUDF04 = notifyRow["DUDF04"].ToString();
//                                    detailAddRow.DUDF05 = notifyRow["DUDF05"].ToString();
//                                    detailAddRow.DUDF06 = notifyRow["DUDF06"].ToString();
//                                    detailAddRow.DUDF07 = notifyRow["DUDF07"].ToString();
//                                    detailAddRow.DUDF08 = notifyRow["DUDF08"].ToString();
//                                    detailAddRow.DUDF09 = notifyRow["DUDF09"].ToString();
//                                    detailAddRow.DUDF10 = notifyRow["DUDF10"].ToString();
//                                    detailAddRow.DUDF11 = notifyRow["DUDF11"].ToString();
//                                    detailAddRow.UDF13 = notifyRow["UDF13"].ToString();

//                                    data.WRM_AdjustNotifyRecord.AddWRM_AdjustNotifyRecordRow(detailAddRow);
//                                }

//                                BusinessDbUtil.ExecuteNonQuery(string.Format(
//                                    @"DELETE FROM WRM_AdjustNotifyRecord WHERE BillId='{0}'", row["BillId"]));
//                                BusinessDbUtil.Save(data,
//                                    string.Format(@"UPDATE WRM_AdjustNotify SET Status='20',ModifyDate=GETDATE(),ModifyBy='{1}' WHERE BillId='{0}' AND Status='10'", row["BillId"], "TimerTask"));

                                string procSql = string.Format(@"EXEC SP_WRM_AdjustNotify_Charge '{0}','{1}','{2}','{3}'",
                                 row["BillId"], "TimerTask", "", "");
                                var result = BusinessDbUtil.InvokeProc(procSql);
                                if (!result.IsSuccess)
                                {
                                    BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateSql, row["BillId"], result.ErrMsg));
                                    continue;
                                }
                                BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateSql, row["BillId"], ""));
                            }
                            catch (Exception ex)
                            {
                                var errorMsg = ex.Message;
                                errorMsg = errorMsg.Length > 200 ? errorMsg.Substring(0, 200) : errorMsg;
                                BusinessDbUtil.ExecuteNonQuery(string.Format(UpdateSql, row["BillId"], errorMsg));
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }
    }
}
