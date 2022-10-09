using Common.DBCore;
using FWCore;
using FWCore.TaskManage;
using iWMS.TimerTaskService.Core.Util;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 补货策略计算
    /// 优先推荐库位容量设置，其次考虑未设置情况，同时考虑上架规则
    /// </summary>
    public class ReplenishmentTask : BaseTask
    {
        private const string QuerySql = @"SELECT TOP 100 * FROM SYS_ReplenishmentPolicy A WHERE Status='1' 
AND BeginTime<=GETDATE() AND EndTime>=GETDATE() 
AND NOT EXISTS(SELECT 1 FROM WRM_Replenishment WITH (NOLOCK) WHERE PolicyId=A.PolicyId AND Status IN('10','20'))
Order BY Priority DESC";

        //存储区满足的SKU  按件
        private const string SourceWHAreaQuerySql = @"SELECT A.* FROM INV_BAL A WITH (NOLOCK) 
INNER JOIN COM_Location B WITH (NOLOCK) ON A.LocationId=B.LocationId
INNER JOIN COM_SKU C WITH (NOLOCK) ON (A.StorerId=C.StorerId AND A.SkuId=C.SkuId)
WHERE B.WHAreaId='{0}' AND A.StorerId='{1}' AND A.QtyValid>0 AND ISNULL(C.MinStockCount,0)>0 {2}";

        //存储区满足的SKU 按箱
        private const string SourceWHAreaByCartonQuerySql = @"SELECT A.*,C.MinStockCount FROM INV_BAL A WITH (NOLOCK) 
INNER JOIN COM_Location B WITH (NOLOCK) ON A.LocationId=B.LocationId
INNER JOIN COM_SKU C WITH (NOLOCK) ON (A.StorerId=C.StorerId AND A.SkuId=C.SkuId)
WHERE B.WHAreaId='{0}' AND A.StorerId='{1}' AND A.QtyValid>0 AND ISNULL(C.CartonPcs,0)>0 AND ISNULL(C.MinStockCount,0)>0 {2}";

        //拣选区库存(排查满足补货条件的库存)
        private const string TargetStockSql = @"SELECT 
ISNULL((SELECT SUM(A.QtyValid) FROM INV_BAL A WITH (NOLOCK) INNER JOIN COM_Location B WITH (NOLOCK) ON A.LocationId=B.LocationId
WHERE A.StorerId=C.StorerId AND A.SkuId=C.SkuId AND B.WHAreaId='{0}' AND A.QtyValid>0),0) QtyValid,
C.SkuId,C.MinStockCount FROM COM_SKU C WITH (NOLOCK)
WHERE  C.StorerId='{1}' AND ISNULL(C.CartonPcs,0)>0 AND ISNULL(C.MinStockCount,0)>0";

        //拣选区 数量容量限制补货
        private const string CountCapacitySql = @"SELECT LocationId,NumCapacity,PutawayRule,CAST(0 AS DECIMAL(18,4)) AS Qty,CAST('' AS NVARCHAR(50)) AS SkuId INTO #TMP 
FROM COM_Location WHERE WHAreaId='{0}' AND NumCapacity>0 
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND ToLoc=LocationId)

UPDATE T SET T.Qty=ISNULL(S.Qty,0),T.SkuId=S.SkuId
FROM #TMP T INNER JOIN 
(SELECT SUM(A.QTY) Qty,A.LocationId,A.SkuId FROM INV_BAL A WITH (NOLOCK) INNER JOIN #TMP B ON A.LocationId=B.LocationId 
WHERE A.Qty>0 AND A.StorerId='{1}' GROUP BY A.LocationId,A.SkuId) S
ON T.LocationId=S.LocationId

UPDATE T SET T.PutawayRule=(SELECT PutawayRule FROM COM_WHArea WHERE WHAreaId='{0}')
FROM #TMP T WHERE ISNULL(T.PutawayRule,'')=''

DECLARE @TaskCount INT
SELECT @TaskCount=(CASE WHEN ISNUMERIC(ParmValue)=1 THEN CAST(ParmValue AS INT) ELSE 0 END) FROM SYS_Parameter WHERE ParmId='PSTOCK041' 
IF ISNULL(@TaskCount,'')<=0
BEGIN
    SET @TaskCount=5;
END
EXEC('SELECT TOP '+@TaskCount+' * FROM #TMP WHERE NumCapacity>0 AND Qty=0')";

        private const string CartonPcsSql = @"SELECT CartonPcs FROM COM_SKU WITH (NOLOCK) WHERE StorerId='{0}' AND SkuId='{1}'";

        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    DataTable table = BusinessDbUtil.GetDataTable(QuerySql);
                    if (table != null && table.Rows.Count > 0)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            if (string.IsNullOrWhiteSpace(row["StorerId"].ToString()) || string.IsNullOrWhiteSpace(row["ReplenishmentType"].ToString())
                                || string.IsNullOrWhiteSpace(row["Cardinality"].ToString()) || string.IsNullOrWhiteSpace(row["CountType"].ToString())
                                || string.IsNullOrWhiteSpace(row["FromWHAreaId"].ToString()) || string.IsNullOrWhiteSpace(row["ToWHAreaId"].ToString())
                                || string.IsNullOrWhiteSpace(row["WorkMode"].ToString()))
                            {
                                continue;
                            }

                            //拣选区库存
                            var pickStockTable = BusinessDbUtil.GetDataTable(string.Format(TargetStockSql, row["ToWHAreaId"], row["StorerId"]));
                            Dictionary<string, decimal> skuList = new Dictionary<string, decimal>();
                            if (pickStockTable != null && pickStockTable.Rows.Count > 0)
                            {
                                foreach (DataRow pickRow in pickStockTable.Rows)
                                {
                                    if (pickRow["QtyValid"].ConvertDecimal() < pickRow["MinStockCount"].ConvertDecimal())
                                    {
                                        skuList.Add(pickRow["SkuId"].ToString(), pickRow["MinStockCount"].ConvertDecimal() - pickRow["QtyValid"].ConvertDecimal());
                                    }
                                }
                            }

                            if (skuList.Count < 1)
                            {
                                continue;
                            }

                            DataTable invBalTable = null;
                            if (row["CountType"].ToString() == "ByPCS")
                            {
                                string whereFilter = string.Empty;
                                if (skuList.Count > 0)
                                {
                                    whereFilter = string.Format(" AND A.SkuId IN('{0}')", string.Join("','", skuList.Keys.ToList()));
                                }

                                //判断来源库区库存是否满足
                                invBalTable = BusinessDbUtil.GetDataTable(string.Format(SourceWHAreaQuerySql, row["FromWHAreaId"], row["StorerId"], whereFilter));

                            }
                            else if (row["CountType"].ToString() == "ByCarton")
                            {
                                string whereFilter = string.Empty;
                                if (skuList.Count > 0)
                                {
                                    whereFilter = string.Format(" AND A.SkuId IN('{0}')", string.Join("','", skuList.Keys.ToList()));
                                }

                                //判断来源库区库存是否满足
                                invBalTable = BusinessDbUtil.GetDataTable(string.Format(SourceWHAreaByCartonQuerySql, row["FromWHAreaId"], row["StorerId"], whereFilter));
                            }
                            else
                            {
                                continue;
                            }

                            if (invBalTable == null || invBalTable.Rows.Count < 1)
                            {
                                continue;
                            }


                            //判断是否设置目标库区库位容量限制
                            var capacityTable = BusinessDbUtil.GetDataTable(string.Format(CountCapacitySql, row["ToWHAreaId"], row["StorerId"]));
                            if (capacityTable == null || capacityTable.Rows.Count < 1)
                            {
                                continue;
                            }
                            //生成补货单
                            CreateReplenishmentOrder(row, invBalTable, capacityTable, skuList);
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

        #region 创建补货单
        private void CreateReplenishmentOrder(DataRow policyRow, DataTable invBalTable, DataTable capacityTable, Dictionary<string, decimal> skuList)
        {
            List<InvBalItem> list = new List<InvBalItem>();
            foreach (DataRow capacityRow in capacityTable.Rows)
            {
                string putawayRule = capacityRow["PutawayRule"].ToString();

                switch (putawayRule)
                {
                    case "SKU":
                        {
                            List<InvBalItem> items = Location_SKU_One_To_One(invBalTable, capacityRow,
                                   policyRow, skuList);
                            list.AddRange(items.ToArray());
                        }
                        break;
                    case "BATCH":
                        {
                            List<InvBalItem> items = Location_SKUBatch_One_To_One(invBalTable, capacityRow,
                                   policyRow, skuList);
                            list.AddRange(items.ToArray());
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
            if (list.Count < 1)
            {
                return;
            }
            var newList = new List<InvBalItem>();
            var groups = list.GroupBy(item => item.FromLoc + item.ToLoc + item.SkuId + item.SkuStatus);
            foreach (var group in groups)
            {
                var groupItem = group.FirstOrDefault();
                newList.Add(new InvBalItem
                {
                    WarehouseId = groupItem.WarehouseId,
                    StorerId = groupItem.StorerId,
                    FromLoc = groupItem.FromLoc,
                    ToLoc = groupItem.ToLoc,
                    SkuId = groupItem.SkuId,
                    SkuStatus = groupItem.SkuStatus,
                    BatchNo = groupItem.BatchNo,
                    CID = groupItem.CID,
                    BoxCode = groupItem.BoxCode,
                    Qty = group.Sum(g => g.Qty),
                    UDF01 = groupItem.UDF01,
                    UDF02 = groupItem.UDF02,
                    UDF03 = groupItem.UDF03,
                    UDF04 = groupItem.UDF04,
                    UDF05 = groupItem.UDF05,
                    UDF06 = groupItem.UDF06,
                    UDF07 = groupItem.UDF07,
                    UDF08 = groupItem.UDF08,
                    UDF09 = groupItem.UDF09,
                    UDF10 = groupItem.UDF10,
                    UDF11 = groupItem.UDF11,
                    UDF12 = groupItem.UDF12,
                    UDF13 = groupItem.UDF13,
                    UDF14 = groupItem.UDF14,
                    UDF15 = groupItem.UDF15,
                    UDF16 = groupItem.UDF16,
                    UDF17 = groupItem.UDF17,
                    UDF18 = groupItem.UDF18,
                    UDF19 = groupItem.UDF19,
                    UDF20 = groupItem.UDF20,
                    DUDF01 = groupItem.DUDF01,
                    DUDF02 = groupItem.DUDF02,
                    DUDF03 = groupItem.DUDF03,
                    DUDF04 = groupItem.DUDF04,
                    DUDF05 = groupItem.DUDF05,
                    DUDF06 = groupItem.DUDF06,
                    DUDF07 = groupItem.DUDF07,
                    DUDF08 = groupItem.DUDF08,
                    DUDF09 = groupItem.DUDF09,
                    DUDF10 = groupItem.DUDF10,
                    DUDF11 = groupItem.DUDF11,
                    DUDF12 = groupItem.DUDF12,
                    DUDF13 = groupItem.DUDF13,
                    DUDF14 = groupItem.DUDF14,
                    DUDF15 = groupItem.DUDF15,
                    DUDF16 = groupItem.DUDF16,
                    DUDF17 = groupItem.DUDF17,
                    DUDF18 = groupItem.DUDF18,
                    DUDF19 = groupItem.DUDF19,
                    DUDF20 = groupItem.DUDF20
                });
            }
            if (newList != null && newList.Count > 0)
            {
                T_WRM_ReplenishmentData data = new T_WRM_ReplenishmentData();
                var repRow = data.WRM_Replenishment.NewWRM_ReplenishmentRow();
                var billId = BillIdGenUtil.GenBillId("RP", newList[0].WarehouseId, newList[0].StorerId, "");
                repRow.BillId = billId;
                repRow.WarehouseId = newList[0].WarehouseId;
                repRow.StorerId = newList[0].StorerId;
                repRow.OrderDate = DateTime.Now;
                repRow.OrderType = policyRow["ReplenishmentType"].ToString() == "NORMAL" ? "SKU" : "ORDER";
                repRow.PolicyId = policyRow["PolicyId"].ConvertLong();
                repRow.Status = "10";
                repRow.WorkMode = policyRow["WorkMode"].ToString();
                repRow.Version = 0;
                repRow.CreateBy = "TimerTask";
                repRow.CreateDate = DateTime.Now;
                repRow.SkuCount = newList.Sum(item => item.Qty);
                repRow.GoodsCount = newList.GroupBy(item => item.SkuId).Count();
                repRow.Memo = "系统补货策略生成";
                data.WRM_Replenishment.AddWRM_ReplenishmentRow(repRow);

                foreach (var item in newList)
                {
                    var repDetailRow = data.WRM_ReplenishmentDetail.NewWRM_ReplenishmentDetailRow();
                    repDetailRow.Status = "10";
                    repDetailRow.BillId = billId;
                    repDetailRow.StorerId = item.StorerId;
                    repDetailRow.SkuId = item.SkuId;
                    repDetailRow.SkuStatus = item.SkuStatus;
                    repDetailRow.FromLoc = item.FromLoc;
                    repDetailRow.ToLoc = item.ToLoc;
                    repDetailRow.FromQty = item.Qty;
                    repDetailRow.ToQty = item.Qty;
                    repDetailRow.BatchNo = item.BatchNo;
                    repDetailRow.CID = item.CID;
                    repDetailRow.BoxCode = item.BoxCode;
                    repDetailRow.UDF01 = item.UDF01;
                    repDetailRow.UDF02 = item.UDF02;
                    repDetailRow.UDF03 = item.UDF03;
                    repDetailRow.UDF04 = item.UDF04;
                    repDetailRow.UDF05 = item.UDF05;
                    repDetailRow.UDF06 = item.UDF06;
                    repDetailRow.UDF07 = item.UDF07;
                    repDetailRow.UDF08 = item.UDF08;
                    repDetailRow.UDF09 = item.UDF09;
                    repDetailRow.UDF10 = item.UDF10;
                    repDetailRow.UDF11 = item.UDF11;
                    repDetailRow.UDF12 = item.UDF12;
                    repDetailRow.UDF13 = item.UDF13;
                    repDetailRow.UDF14 = item.UDF14;
                    repDetailRow.UDF15 = item.UDF15;
                    repDetailRow.UDF16 = item.UDF16;
                    repDetailRow.UDF17 = item.UDF17;
                    repDetailRow.UDF18 = item.UDF18;
                    repDetailRow.UDF19 = item.UDF19;
                    repDetailRow.UDF20 = item.UDF20;
                    repDetailRow.DUDF01 = item.DUDF01;
                    repDetailRow.DUDF02 = item.DUDF02;
                    repDetailRow.DUDF03 = item.DUDF03;
                    repDetailRow.DUDF04 = item.DUDF04;
                    repDetailRow.DUDF05 = item.DUDF05;
                    repDetailRow.DUDF06 = item.DUDF06;
                    repDetailRow.DUDF07 = item.DUDF07;
                    repDetailRow.DUDF08 = item.DUDF08;
                    repDetailRow.DUDF09 = item.DUDF09;
                    repDetailRow.DUDF10 = item.DUDF10;
                    repDetailRow.DUDF11 = item.DUDF11;
                    repDetailRow.DUDF12 = item.DUDF12;
                    repDetailRow.DUDF13 = item.DUDF13;
                    repDetailRow.DUDF14 = item.DUDF14;
                    repDetailRow.DUDF15 = item.DUDF15;
                    repDetailRow.DUDF16 = item.DUDF16;
                    repDetailRow.DUDF17 = item.DUDF17;
                    repDetailRow.DUDF18 = item.DUDF18;
                    repDetailRow.DUDF19 = item.DUDF19;
                    repDetailRow.DUDF20 = item.DUDF20;
                    repDetailRow.Version = 0;
                    repDetailRow.CreateBy = "TimerTask";
                    repDetailRow.CreateDate = DateTime.Now;
                    data.WRM_ReplenishmentDetail.AddWRM_ReplenishmentDetailRow(repDetailRow);
                }
                BusinessDbUtil.Save(data);
                //var result = BusinessDbUtil.SaveAndProc(data,
                //    string.Format(@"EXEC SP_WRM_Replenishment_Check '{0}','TimerTask','',''", billId));
                //if (!result.IsSuccess)
                //{
                //    NLogUtil.WriteError(result.ErrMsg);
                //}
            }
        }
        #endregion

        #region 一位一品
        private List<InvBalItem> Location_SKU_One_To_One(DataTable invBalTable, DataRow capacityRow, DataRow policyRow, Dictionary<string, decimal> skuList)
        {
            List<InvBalItem> list = new List<InvBalItem>();
            string defaultSkuId = string.Empty;
            if (capacityRow["Qty"].ConvertDecimal() == 0)//库存为0，默认选择一个SKU存放
            {
                var invBalRow = invBalTable.AsEnumerable().Where(item => item["QtyValid"].ConvertDecimal() > 0).FirstOrDefault();
                if (invBalRow != null)
                {
                    defaultSkuId = invBalRow["SkuId"].ToString();
                }
                else
                {
                    return list;
                }
            }
            else//库存大于0，根据当前库存SKU存放
            {
                defaultSkuId = capacityRow["SkuId"].ToString();
            }
            var cartonPcs = BusinessDbUtil.ExecuteScalar(string.Format(CartonPcsSql, policyRow["StorerId"], defaultSkuId)).ConvertInt32();
            decimal leftCapacity = 0;
            if (skuList.ContainsKey(defaultSkuId))
            {
                leftCapacity = skuList[defaultSkuId];
            }
            else
            {
                return list;
            }
            //if (policyRow["CountType"].ToString() == "ByPCS")
            //{
            //    leftCapacity = capacityRow["NumCapacity"].ConvertDecimal();
            //}
            //else if (policyRow["CountType"].ToString() == "ByCarton")
            //{
            //    leftCapacity = capacityRow["NumCapacity"].ConvertDecimal() * cartonPcs;
            //}

            if (leftCapacity <= 0)
            {
                return list;
            }
            foreach (DataRow invBalRow in invBalTable.Rows)
            {
                if (invBalRow["QtyValid"].ConvertDecimal() <= 0) continue;
                if (invBalRow["SkuId"].ToString() == defaultSkuId)
                {
                    var item = new InvBalItem
                    {
                        WarehouseId = invBalRow["WarehouseId"].ToString(),
                        StorerId = invBalRow["StorerId"].ToString(),
                        FromLoc = invBalRow["LocationId"].ToString(),
                        ToLoc = capacityRow["LocationId"].ToString(),
                        SkuId = invBalRow["SkuId"].ToString(),
                        SkuStatus = invBalRow["SkuStatus"].ToString(),
                        BatchNo = invBalRow["BatchNo"].ToString(),
                        CID = invBalRow["CID"].ToString(),
                        BoxCode = invBalRow["BoxCode"].ToString(),
                        UDF01 = invBalRow["UDF01"].ToString(),
                        UDF02 = invBalRow["UDF02"].ToString(),
                        UDF03 = invBalRow["UDF03"].ToString(),
                        UDF04 = invBalRow["UDF04"].ToString(),
                        UDF05 = invBalRow["UDF05"].ToString(),
                        UDF06 = invBalRow["UDF06"].ToString(),
                        UDF07 = invBalRow["UDF07"].ToString(),
                        UDF08 = invBalRow["UDF08"].ToString(),
                        UDF09 = invBalRow["UDF09"].ToString(),
                        UDF10 = invBalRow["UDF10"].ToString(),
                        UDF11 = invBalRow["UDF11"].ToString(),
                        UDF12 = invBalRow["UDF12"].ToString(),
                        UDF13 = invBalRow["UDF13"].ToString(),
                        UDF14 = invBalRow["UDF14"].ToString(),
                        UDF15 = invBalRow["UDF15"].ToString(),
                        UDF16 = invBalRow["UDF16"].ToString(),
                        UDF17 = invBalRow["UDF17"].ToString(),
                        UDF18 = invBalRow["UDF18"].ToString(),
                        UDF19 = invBalRow["UDF19"].ToString(),
                        UDF20 = invBalRow["UDF20"].ToString(),
                        DUDF01 = invBalRow["DUDF01"].ToString(),
                        DUDF02 = invBalRow["DUDF02"].ToString(),
                        DUDF03 = invBalRow["DUDF03"].ToString(),
                        DUDF04 = invBalRow["DUDF04"].ToString(),
                        DUDF05 = invBalRow["DUDF05"].ToString(),
                        DUDF06 = invBalRow["DUDF06"].ToString(),
                        DUDF07 = invBalRow["DUDF07"].ToString(),
                        DUDF08 = invBalRow["DUDF08"].ToString(),
                        DUDF09 = invBalRow["DUDF09"].ToString(),
                        DUDF10 = invBalRow["DUDF10"].ToString(),
                        DUDF11 = invBalRow["DUDF11"].ToString(),
                        DUDF12 = invBalRow["DUDF12"].ToString(),
                        DUDF13 = invBalRow["DUDF13"].ToString(),
                        DUDF14 = invBalRow["DUDF14"].ToString(),
                        DUDF15 = invBalRow["DUDF15"].ToString(),
                        DUDF16 = invBalRow["DUDF16"].ToString(),
                        DUDF17 = invBalRow["DUDF17"].ToString(),
                        DUDF18 = invBalRow["DUDF18"].ToString(),
                        DUDF19 = invBalRow["DUDF19"].ToString(),
                        DUDF20 = invBalRow["DUDF20"].ToString()
                    };
                    list.Add(item);
                    if (policyRow["CountType"].ToString() == "ByCarton")
                    {
                        var repQty = capacityRow["NumCapacity"].ConvertDecimal() * cartonPcs;
                        if (invBalRow["QtyValid"].ConvertDecimal() >= repQty)
                        {
                            item.Qty = repQty;
                            invBalRow.BeginEdit();
                            invBalRow["QtyValid"] = invBalRow["QtyValid"].ConvertDecimal() - repQty;
                            invBalRow.EndEdit();
                            break;
                        }
                        else
                        {
                            item.Qty = invBalRow["QtyValid"].ConvertDecimal();
                            invBalRow.BeginEdit();
                            invBalRow["QtyValid"] = 0;
                            invBalRow.EndEdit();

                            leftCapacity -= invBalRow["QtyValid"].ConvertDecimal();
                        }
                    }
                    else
                    {
                        if (invBalRow["QtyValid"].ConvertDecimal() >= leftCapacity)
                        {
                            item.Qty = leftCapacity;
                            invBalRow.BeginEdit();
                            invBalRow["QtyValid"] = invBalRow["QtyValid"].ConvertDecimal() - leftCapacity;
                            invBalRow.EndEdit();
                            break;
                        }
                        else
                        {
                            item.Qty = invBalRow["QtyValid"].ConvertDecimal();
                            invBalRow.BeginEdit();
                            invBalRow["QtyValid"] = 0;
                            invBalRow.EndEdit();

                            leftCapacity -= invBalRow["QtyValid"].ConvertDecimal();
                        }
                    }
                }
            }
            return list;
        }
        #endregion

        #region 一位一批次
        private List<InvBalItem> Location_SKUBatch_One_To_One(DataTable invBalTable, DataRow capacityRow, DataRow policyRow, Dictionary<string, decimal> skuList)
        {
            List<InvBalItem> list = new List<InvBalItem>();
            string defaultSkuId = string.Empty, defaultBatchNo = string.Empty;
            if (capacityRow["Qty"].ConvertDecimal() == 0)//库存为0，默认选择一个SKU存放
            {
                defaultSkuId = invBalTable.Rows[0]["SkuId"].ToString();
                defaultBatchNo = invBalTable.Rows[0]["BatchNo"].ToString();
            }
            else//库存大于0，根据当前库存SKU存放  ,当前不考虑
            {
                //defaultSkuId = capacityRow["SkuId"].ToString();

            }
            var cartonPcs = BusinessDbUtil.ExecuteScalar(string.Format(CartonPcsSql, policyRow["StorerId"], defaultSkuId)).ConvertInt32();
            decimal leftCapacity = 0;
            if (policyRow["CountType"].ToString() == "ByPCS")
            {
                leftCapacity = capacityRow["NumCapacity"].ConvertDecimal();
            }
            else if (policyRow["CountType"].ToString() == "ByCarton")
            {
                leftCapacity = capacityRow["NumCapacity"].ConvertDecimal() * cartonPcs;
            }
            if (leftCapacity <= 0)
            {
                return list;
            }
            foreach (DataRow invBalRow in invBalTable.Rows)
            {
                if (invBalRow["QtyValid"].ConvertDecimal() <= 0) continue;
                if (invBalRow["SkuId"].ToString() == defaultSkuId &&
                    invBalRow["BatchNo"].ToString() == defaultBatchNo)
                {
                    var item = new InvBalItem
                    {
                        WarehouseId = invBalRow["WarehouseId"].ToString(),
                        StorerId = invBalRow["StorerId"].ToString(),
                        FromLoc = invBalRow["LocationId"].ToString(),
                        ToLoc = capacityRow["LocationId"].ToString(),
                        SkuId = invBalRow["SkuId"].ToString(),
                        SkuStatus = invBalRow["SkuStatus"].ToString(),
                        BatchNo = invBalRow["BatchNo"].ToString(),
                        CID = invBalRow["CID"].ToString(),
                        BoxCode = invBalRow["BoxCode"].ToString(),
                        UDF01 = invBalRow["UDF01"].ToString(),
                        UDF02 = invBalRow["UDF02"].ToString(),
                        UDF03 = invBalRow["UDF03"].ToString(),
                        UDF04 = invBalRow["UDF04"].ToString(),
                        UDF05 = invBalRow["UDF05"].ToString(),
                        UDF06 = invBalRow["UDF06"].ToString(),
                        UDF07 = invBalRow["UDF07"].ToString(),
                        UDF08 = invBalRow["UDF08"].ToString(),
                        UDF09 = invBalRow["UDF09"].ToString(),
                        UDF10 = invBalRow["UDF10"].ToString(),
                        UDF11 = invBalRow["UDF11"].ToString(),
                        UDF12 = invBalRow["UDF12"].ToString(),
                        UDF13 = invBalRow["UDF13"].ToString(),
                        UDF14 = invBalRow["UDF14"].ToString(),
                        UDF15 = invBalRow["UDF15"].ToString(),
                        UDF16 = invBalRow["UDF16"].ToString(),
                        UDF17 = invBalRow["UDF17"].ToString(),
                        UDF18 = invBalRow["UDF18"].ToString(),
                        UDF19 = invBalRow["UDF19"].ToString(),
                        UDF20 = invBalRow["UDF20"].ToString(),
                        DUDF01 = invBalRow["DUDF01"].ToString(),
                        DUDF02 = invBalRow["DUDF02"].ToString(),
                        DUDF03 = invBalRow["DUDF03"].ToString(),
                        DUDF04 = invBalRow["DUDF04"].ToString(),
                        DUDF05 = invBalRow["DUDF05"].ToString(),
                        DUDF06 = invBalRow["DUDF06"].ToString(),
                        DUDF07 = invBalRow["DUDF07"].ToString(),
                        DUDF08 = invBalRow["DUDF08"].ToString(),
                        DUDF09 = invBalRow["DUDF09"].ToString(),
                        DUDF10 = invBalRow["DUDF10"].ToString(),
                        DUDF11 = invBalRow["DUDF11"].ToString(),
                        DUDF12 = invBalRow["DUDF12"].ToString(),
                        DUDF13 = invBalRow["DUDF13"].ToString(),
                        DUDF14 = invBalRow["DUDF14"].ToString(),
                        DUDF15 = invBalRow["DUDF15"].ToString(),
                        DUDF16 = invBalRow["DUDF16"].ToString(),
                        DUDF17 = invBalRow["DUDF17"].ToString(),
                        DUDF18 = invBalRow["DUDF18"].ToString(),
                        DUDF19 = invBalRow["DUDF19"].ToString(),
                        DUDF20 = invBalRow["DUDF20"].ToString()
                    };
                    list.Add(item);
                    if (invBalRow["QtyValid"].ConvertDecimal() >= leftCapacity)
                    {
                        item.Qty = leftCapacity;
                        invBalRow.BeginEdit();
                        invBalRow["QtyValid"] = invBalRow["QtyValid"].ConvertDecimal() - leftCapacity;
                        invBalRow.EndEdit();
                        break;
                    }
                    else
                    {
                        item.Qty = invBalRow["QtyValid"].ConvertDecimal();
                        invBalRow.BeginEdit();
                        invBalRow["QtyValid"] = 0;
                        invBalRow.EndEdit();

                        leftCapacity -= invBalRow["QtyValid"].ConvertDecimal();
                    }
                }
            }
            return list;
        }
        #endregion

    }

    public class InvBalItem
    {
        public string WarehouseId { get; set; }
        public string StorerId { get; set; }
        public string FromLoc { get; set; }
        public string ToLoc { get; set; }
        public string SkuId { get; set; }
        public string SkuStatus { get; set; }
        public decimal Qty { get; set; }
        public string BatchNo { get; set; }
        public string CID { get; set; }
        public string BoxCode { get; set; }

        public string UDF01 { get; set; }
        public string UDF02 { get; set; }
        public string UDF03 { get; set; }
        public string UDF04 { get; set; }
        public string UDF05 { get; set; }
        public string UDF06 { get; set; }
        public string UDF07 { get; set; }
        public string UDF08 { get; set; }
        public string UDF09 { get; set; }
        public string UDF10 { get; set; }
        public string UDF11 { get; set; }
        public string UDF12 { get; set; }
        public string UDF13 { get; set; }
        public string UDF14 { get; set; }
        public string UDF15 { get; set; }
        public string UDF16 { get; set; }
        public string UDF17 { get; set; }
        public string UDF18 { get; set; }
        public string UDF19 { get; set; }
        public string UDF20 { get; set; }
        public string DUDF01 { get; set; }
        public string DUDF02 { get; set; }
        public string DUDF03 { get; set; }
        public string DUDF04 { get; set; }
        public string DUDF05 { get; set; }
        public string DUDF06 { get; set; }
        public string DUDF07 { get; set; }
        public string DUDF08 { get; set; }
        public string DUDF09 { get; set; }
        public string DUDF10 { get; set; }
        public string DUDF11 { get; set; }
        public string DUDF12 { get; set; }
        public string DUDF13 { get; set; }
        public string DUDF14 { get; set; }
        public string DUDF15 { get; set; }
        public string DUDF16 { get; set; }
        public string DUDF17 { get; set; }
        public string DUDF18 { get; set; }
        public string DUDF19 { get; set; }
        public string DUDF20 { get; set; }
    }
}
