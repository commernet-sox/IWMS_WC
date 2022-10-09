using Common.DBCore;
using FWCore;
using iWMS.Entity;
using iWMS.TimerTaskService.Core.Util;
using iWMS.TypedData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    public class WaveAlgorithm
    {
        #region SQL定义
        private const string WaveTemplateSql = @"SELECT * FROM SYS_PolicyTemplate WHERE TemplateType='WAVE' AND Status=1 ORDER BY Priority DESC";
        private const string QueryOrderSql = @"SELECT A.BillId,A.StorerId,A.WarehouseId,B.Priority,A.AnalysisSku,A.AnalysisLocation,A.WbId,
B.SkuCount,B.GoodsCount,CAST('' AS NVARCHAR(30)) AS WaveId INTO #TMP
FROM SOM_Order A INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
{1}
WHERE ISNULL(A.WaveId,'')=''{0}

SELECT * FROM #TMP
";
        private const string WavePolicyDetailSql = @"SELECT * FROM SYS_WavPolicyDetail WHERE RuleId='{0}' AND Status=1 ORDER BY Priority DESC";
        #endregion


        public void Calc()
        {
            var table = BusinessDbUtil.GetDataTable(WaveTemplateSql);
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        var template = JsonConvert.DeserializeObject<WaveTemplate>(row["TemplateContent"].ToString());
                        CreateWave(template);
                        Allocate(template);
                    }
                    catch (Exception ex)
                    {
                        NLogUtil.WriteError(ex.ToString());
                    }
                }
            }
        }

        private void Allocate(WaveTemplate template)
        {
            if (template.OrderStatus == "10")
            {

            }
        }

        private void CreateWave(WaveTemplate template)
        {
            var templateSql = GetTemplateSql(template);
            var condition = "";
            if (!string.IsNullOrWhiteSpace(template.OrderStatus))
            {
                if (template.OrderStatus=="30")
                {
                    condition = string.Format(" AND A.Status IN('{0}','35')", template.OrderStatus);
                }
                else
                {
                    condition = string.Format(" AND A.Status='{0}'", template.OrderStatus);
                }
            }
            var orderList = BusinessDbUtil.GetDataTable(string.Format(QueryOrderSql, templateSql, condition));
            if (orderList != null && orderList.Rows.Count > 0)
            {
                var orderGroups = orderList.AsEnumerable().GroupBy(item => item["WbId"].ToString());
                foreach (var orderGroup in orderGroups)
                {
                    var orderIds = orderGroup.ToList().Select(item => item["BillId"].ToString());
                    if (ExsistWaveOrder(orderIds))
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(template.PolicyId))//根据波次策略计算波次订单结构
                    {
                        T_SYS_WavPolicyData policyData = new T_SYS_WavPolicyData();
                        var policyDetailSet = BusinessDbUtil.GetDataSet(string.Format(WavePolicyDetailSql, template.PolicyId), policyData.SYS_WavPolicyDetail.TableName);
                        policyData.Merge(policyData.SYS_WavPolicyDetail.TableName, policyDetailSet, MissingSchemaAction.Add);
                        if (policyData != null && policyData.SYS_WavPolicyDetail.Count > 0)
                        {
                            foreach (var policyRow in policyData.SYS_WavPolicyDetail)
                            {
                                var orderRows = orderGroup.ToList()
                                    .Where(item => string.IsNullOrWhiteSpace(item["WaveId"].ToString()));
                                switch (policyRow.PicklistType)
                                {
                                    case "Single"://单品
                                        {
                                            List<DataRow> tmpOrderRows = null;
                                            string orderStructure = "";
                                            if (policyRow.OrderStructure.Contains("11"))
                                            {
                                                tmpOrderRows = orderRows
                                                .Where(item => item["SkuCount"].ConvertDecimal() == 1 && item["GoodsCount"].ConvertInt32() == 1)
                                                .ToList();
                                                orderStructure = "11";
                                            }
                                            if (tmpOrderRows.Count==0 && policyRow.OrderStructure.Contains("1N"))
                                            {
                                                tmpOrderRows = orderRows
                                              .Where(item => item["SkuCount"].ConvertDecimal() > 1 && item["GoodsCount"].ConvertInt32() == 1)
                                               .ToList();
                                                orderStructure = "1N";
                                            }
                                            if (tmpOrderRows.Count > 0)
                                            {
                                                if (policyRow.OrderStdQty > 0)
                                                {
                                                    if (tmpOrderRows.Count <= policyRow.OrderStdQty)
                                                    {
                                                        if ((tmpOrderRows.Count == policyRow.OrderStdQty) ||
                                                            (tmpOrderRows.Count < policyRow.OrderStdQty && tmpOrderRows.Count >= policyRow.OrderMinQty))
                                                        {
                                                            CreateWaveData(tmpOrderRows.ToArray(), policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                policyRow.RuleId, policyRow.RuleDId.ToString(), template.IsLastWave, template.OrderStatus);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < tmpOrderRows.Count; i++)
                                                        {
                                                            if (tmpOrderRows.Count - i > policyRow.OrderStdQty)
                                                            {
                                                                var stdRows = tmpOrderRows.Skip(i).Take(policyRow.OrderStdQty).ToArray();
                                                                CreateWaveData(stdRows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                    policyRow.RuleId, policyRow.RuleDId.ToString(), 0, template.OrderStatus);
                                                            }
                                                            else
                                                            {
                                                                //尾波
                                                                var lastRows = tmpOrderRows.Skip(i).Take(tmpOrderRows.Count - i).ToArray();
                                                                CreateWaveData(lastRows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                    policyRow.RuleId, policyRow.RuleDId.ToString(), template.IsLastWave, template.OrderStatus);
                                                            }
                                                            i += policyRow.OrderStdQty;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case "Multi"://多品
                                        {
                                            List<DataRow> tmpOrderRows = null;
                                            string orderStructure = "";
                                            if (policyRow.OrderStructure.Contains("N1"))
                                            {
                                                tmpOrderRows = orderRows
                                               .Where(item => item["SkuCount"].ConvertDecimal() == 1 && item["GoodsCount"].ConvertInt32() > 1)
                                                .ToList();
                                                orderStructure = "N1";
                                            }
                                            if (tmpOrderRows.Count==0 && policyRow.OrderStructure.Contains("NN"))
                                            {
                                                tmpOrderRows = orderRows
                                              .Where(item => item["SkuCount"].ConvertDecimal() > 1 && item["GoodsCount"].ConvertInt32() > 1)
                                               .ToList();
                                                orderStructure = "NN";
                                            }
                                            if (tmpOrderRows.Count == 0 && policyRow.OrderStructure.Contains("1N"))
                                            {
                                                tmpOrderRows = orderRows
                                              .Where(item => item["SkuCount"].ConvertDecimal() > 1 && item["GoodsCount"].ConvertInt32() == 1)
                                               .ToList();
                                                orderStructure = "1N";
                                            }
                                            else
                                            {
                                                tmpOrderRows = orderRows.AsEnumerable().ToList();
                                                orderStructure = "NN";
                                            }
                                            if (tmpOrderRows.Count > 0)
                                            {
                                                if (policyRow.OrderStdQty > 0)
                                                {
                                                    if (tmpOrderRows.Count <= policyRow.OrderStdQty)
                                                    {
                                                        if ((tmpOrderRows.Count == policyRow.OrderStdQty) ||
                                                            (tmpOrderRows.Count < policyRow.OrderStdQty && tmpOrderRows.Count >= policyRow.OrderMinQty))
                                                        {
                                                            CreateWaveData(tmpOrderRows.ToArray(), policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                policyRow.RuleId, policyRow.RuleDId.ToString(), template.IsLastWave, template.OrderStatus);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < tmpOrderRows.Count; i++)
                                                        {
                                                            if (tmpOrderRows.Count - i > policyRow.OrderStdQty)
                                                            {
                                                                var stdRows = tmpOrderRows.Skip(i).Take(policyRow.OrderStdQty).ToArray();
                                                                CreateWaveData(stdRows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                    policyRow.RuleId, policyRow.RuleDId.ToString(), 0, template.OrderStatus);
                                                            }
                                                            else
                                                            {
                                                                //尾波
                                                                var lastRows = tmpOrderRows.Skip(i).Take(tmpOrderRows.Count - i).ToArray();
                                                                CreateWaveData(lastRows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                    policyRow.RuleId, policyRow.RuleDId.ToString(), template.IsLastWave, template.OrderStatus);
                                                            }
                                                            i += policyRow.OrderStdQty;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case "Spike"://秒杀
                                        {
                                            List<DataRow> tmpOrderRows = null;
                                            string orderStructure = "";
                                            if (policyRow.OrderStructure.Contains("11"))
                                            {
                                                tmpOrderRows = orderRows
                                                .Where(item => !string.IsNullOrWhiteSpace(item["AnalysisSku"].ToString()))
                                                .Where(item => item["SkuCount"].ConvertDecimal() == 1 && item["GoodsCount"].ConvertInt32() == 1)
                                                .ToList();
                                                orderStructure = "11";
                                            }
                                            if (tmpOrderRows.Count == 0 &&  policyRow.OrderStructure.Contains("1N"))
                                            {
                                                tmpOrderRows = orderRows
                                                .Where(item => !string.IsNullOrWhiteSpace(item["AnalysisSku"].ToString()))
                                               .Where(item => item["SkuCount"].ConvertDecimal() > 1 && item["GoodsCount"].ConvertInt32() == 1)
                                                .ToList();
                                                orderStructure = "1N";
                                            }
                                            if (tmpOrderRows.Count == 0 &&  policyRow.OrderStructure.Contains("N1"))
                                            {
                                                tmpOrderRows = orderRows
                                               .Where(item => !string.IsNullOrWhiteSpace(item["AnalysisSku"].ToString()))
                                               .Where(item => item["SkuCount"].ConvertDecimal() == 1 && item["GoodsCount"].ConvertInt32() > 1)
                                               .ToList();
                                                orderStructure = "N1";
                                            }
                                            if (tmpOrderRows.Count == 0 &&  policyRow.OrderStructure.Contains("NN"))
                                            {
                                                tmpOrderRows = orderRows
                                                 .Where(item => !string.IsNullOrWhiteSpace(item["AnalysisSku"].ToString()))
                                              .Where(item => item["SkuCount"].ConvertDecimal() > 1 && item["GoodsCount"].ConvertInt32() > 1)
                                               .ToList();
                                                orderStructure = "NN";
                                            }
                                            var groups = tmpOrderRows.GroupBy(g => g["AnalysisSku"].ToString());
                                            foreach (var group in groups)
                                            {
                                                var grows = group.ToArray();
                                                if (policyRow.OrderStdQty > 0)
                                                {
                                                    if (grows.Length <= policyRow.OrderStdQty)
                                                    {
                                                        if ((grows.Length == policyRow.OrderStdQty) ||
                                                            (grows.Length < policyRow.OrderStdQty && grows.Length >= policyRow.OrderMinQty))
                                                        {
                                                            CreateWaveData(grows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                policyRow.RuleId, policyRow.RuleDId.ToString(), template.IsLastWave, template.OrderStatus);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < grows.Length; i++)
                                                        {
                                                            if (grows.Length - i > policyRow.OrderStdQty)
                                                            {
                                                                var stdRows = grows.Skip(i).Take(policyRow.OrderStdQty).ToArray();
                                                                CreateWaveData(stdRows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                    policyRow.RuleId, policyRow.RuleDId.ToString(), 0, template.OrderStatus);
                                                            }
                                                            else
                                                            {
                                                                //尾波
                                                                var lastRows = grows.Skip(i).Take(grows.Length - i).ToArray();
                                                                CreateWaveData(lastRows, policyRow.PicklistType, orderStructure, template.TemplateName,
                                                                    policyRow.RuleId, policyRow.RuleDId.ToString(), template.IsLastWave, template.OrderStatus);
                                                            }
                                                            i += policyRow.OrderStdQty;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        string picklistType = "", orderStructure = "", templateName = template.TemplateName, ruleId = "", orderStatus = template.OrderStatus;
                        int isLast = 0;
                        string ruleDId = "";
                        //计算订单结构
                        var groups = orderList.AsEnumerable().GroupBy(item => item["AnalysisSku"].ToString());
                        if (groups.Count() == 1)
                        {
                            if (orderList.AsEnumerable()
                                  .Where(item => item["GoodsCount"].ConvertInt32() == 1 && item["SkuCount"].ConvertDecimal() == 1)
                                  .Count() == orderList.Rows.Count)
                            {
                                picklistType = "Spike";
                                orderStructure = "11";
                            }
                            else if (orderList.AsEnumerable()
                                 .Where(item => item["GoodsCount"].ConvertInt32() == 1 && item["SkuCount"].ConvertDecimal() > 1)
                                 .Count() == orderList.Rows.Count)
                            {
                                picklistType = "Spike";
                                orderStructure = "1N";
                            }
                            else if (orderList.AsEnumerable()
                               .Where(item => item["GoodsCount"].ConvertInt32() > 1 && item["SkuCount"].ConvertDecimal() == 1)
                               .Count() == orderList.Rows.Count)
                            {
                                picklistType = "Spike";
                                orderStructure = "N1";
                            }
                            else if (orderList.AsEnumerable()
                               .Where(item => item["GoodsCount"].ConvertInt32() > 1 && item["SkuCount"].ConvertDecimal() > 1)
                               .Count() == orderList.Rows.Count)
                            {
                                picklistType = "Spike";
                                orderStructure = "NN";
                            }

                        }
                        else
                        {
                            var orderCount = orderList.AsEnumerable()
                                  .Where(item => item["GoodsCount"].ConvertInt32() == 1 && item["SkuCount"].ConvertDecimal() == 1)
                                  .Count();
                            if (orderCount == orderList.Rows.Count)
                            {
                                picklistType = "Single";
                                orderStructure = "11";
                            }
                            else
                            {
                                picklistType = "Multi";
                                orderStructure = "NN";
                            }
                        }
                        CreateWaveData(orderList.AsEnumerable().ToArray(), picklistType, orderStructure, templateName
                            , ruleId, ruleDId, isLast, orderStatus);
                    }
                }
               
            }
        }

        private void CreateWaveData(DataRow[] orderRows, string picklistType, string orderStructure,
            string templateName, string ruleId, string ruleDId, int isLast, string orderStatus)
        {
            //创建波次
            T_SOM_WavData data = new T_SOM_WavData();
            var waveId = BillIdGenUtil.GenBillId("WAV", orderRows[0]["WarehouseId"].ToString(),
                orderRows[0]["StorerId"].ToString(), "WAVE");
            var row = data.SOM_Wav.NewSOM_WavRow();
            row.BillId = waveId;
            row.WarehouseId = orderRows[0]["WarehouseId"].ToString();
            row.StorerId = orderRows[0]["StorerId"].ToString();
            row.WaveType = "NORMAL";
            row.OrderStructure = orderStructure;
            row.OrigBillType = 0;
            row.PicklistType = picklistType;
            row.Status = orderStatus;
            row.IsLast = isLast;
            row.TemplateName = templateName;
            row.RuleId = ruleId;
            row.RuleDId = ruleDId;
            row.Priority = orderRows.Max(item => item["Priority"].ConvertInt32());
            row.Version = 0;
            row.CreateBy = "TimerTask";
            row.CreateDate = DateTime.Now;
            data.SOM_Wav.AddSOM_WavRow(row);

            for (int i = 0; i < orderRows.Length; i++)
            {
                DataRow orderRow = orderRows[i];
                var detailRow = data.SOM_WavDetail.NewSOM_WavDetailRow();
                detailRow.BillId = waveId;
                detailRow.OrderId = orderRow["BillId"].ToString();
                detailRow.CellNo = i + 1;
                detailRow.Status = orderStatus;
                detailRow.Version = 0;
                detailRow.CreateBy = "TimerTask";
                detailRow.CreateDate = DateTime.Now;
                data.SOM_WavDetail.AddSOM_WavDetailRow(detailRow);

                orderRow.BeginEdit();
                orderRow["WaveId"] = waveId;
                orderRow.EndEdit();
            }
            string procSql = string.Format("EXEC SP_WAVE_CREATE '{0}','{1}','TimerTask'", waveId, orderStatus);
            var result = BusinessDbUtil.SaveProc(data, procSql);
            if (!result.IsSuccess)
            {
                NLogUtil.WriteError(result.ErrMsg);
            }
        }

        private bool ExsistWaveOrder(IEnumerable<string> orderIds)
        {
            var sql = string.Format(@"DECLARE @TMP TABLE(BillId NVARCHAR(30))
                INSERT INTO @TMP(BillId) SELECT Text FROM F_GetSplitTable('{0}')
SELECT 1 FROM SOM_Order A INNER JOIN @TMP B ON A.BillId=B.BillId
WHERE ISNULL(A.WaveId,'')<>'' AND A.Status<>'00'
", string.Join(",", orderIds));
            return BusinessDbUtil.ExecuteScalar(sql).ConvertInt32() == 1;
        }

        private string GetTemplateSql(WaveTemplate template)
        {
            StrJoin sj = string.Empty;
            //发运信息
            if (template.StorerIds != null && template.StorerIds.Count > 0)
            {
                sj += string.Format(" AND A.StorerId IN('{0}')", string.Join("','", template.StorerIds));
            }
            if (!string.IsNullOrWhiteSpace(template.WarehouseId))
            {
                sj += string.Format(" AND A.WarehouseId='{0}'", template.WarehouseId);
            }
            if (template.OrderTypes != null && template.OrderTypes.Count > 0)
            {
                sj += string.Format(" AND A.OrderType IN('{0}')", string.Join("','", template.OrderTypes));
            }
            if (template.LogisticsCodes != null && template.LogisticsCodes.Count > 0)
            {
                sj += string.Format(" AND A.ShippingCode IN('{0}')", string.Join("','", template.LogisticsCodes));
            }
            if (template.Provinces != null && template.Provinces.Count > 0)
            {
                sj += string.Format(" AND B.ReceiverProvince IN('{0}')", string.Join("','", template.Provinces));
            }
            if (template.Cities != null && template.Cities.Count > 0)
            {
                sj += string.Format(" AND B.ReceiverCity IN('{0}')", string.Join("','", template.Cities));
            }
            if (template.Areas != null && template.Areas.Count > 0)
            {
                sj += string.Format(" AND B.ReceiverArea IN('{0}')", string.Join("','", template.Areas));
            }
            //交易/时间信息
            if (!string.IsNullOrWhiteSpace(template.OrderSource))
            {
                sj += string.Format(" AND A.OrderSource='{0}'", template.OrderSource);
            }
            if (!string.IsNullOrWhiteSpace(template.ShopCode))
            {
                sj += string.Format(" AND B.ShopCode='{0}'", template.ShopCode);
            }
            if (!string.IsNullOrWhiteSpace(template.BeginCreateTime))
            {
                sj += string.Format(" AND A.CreateDate>='{0}'", template.BeginCreateTime);
            }
            if (!string.IsNullOrWhiteSpace(template.EndCreateTime))
            {
                sj += string.Format(" AND A.CreateDate<='{0}'", template.EndCreateTime);
            }
            if (!string.IsNullOrWhiteSpace(template.BeginAddTime))
            {
                sj += string.Format(" AND B.AddTime>='{0}'", template.BeginAddTime);
            }
            if (!string.IsNullOrWhiteSpace(template.EndAddTime))
            {
                sj += string.Format(" AND B.AddTime<='{0}'", template.EndAddTime);
            }
            if (!string.IsNullOrWhiteSpace(template.BeginPayTime))
            {
                sj += string.Format(" AND B.PayTime>='{0}'", template.BeginPayTime);
            }
            if (!string.IsNullOrWhiteSpace(template.EndPayTime))
            {
                sj += string.Format(" AND B.PayTime<='{0}'", template.EndPayTime);
            }
            if (!string.IsNullOrWhiteSpace(template.BeginPlanOutTime))
            {
                sj += string.Format(" AND A.PreOutDate>='{0}'", template.BeginPlanOutTime);
            }
            if (!string.IsNullOrWhiteSpace(template.EndPlanOutTime))
            {
                sj += string.Format(" AND A.PreOutDate<='{0}'", template.EndPlanOutTime);
            }
            //商品/库区信息
            if (!string.IsNullOrWhiteSpace(template.SkuUpcs))
            {
                sj += string.Format(@" AND EXISTS(SELECT 1 FROM SOM_OrderDetail D INNER JOIN COM_SKUUPC S ON (D.StorerId=S.StorerId AND D.SkuId=S.SkuId)) 
WHERE S.UpcCode IN('{0}')", template.SkuUpcs);
            }
            if (template.MinSkuCount > 0)
            {
                sj += string.Format(@" AND B.SkuCount>={0}", template.MinSkuCount);
            }
            if (template.MaxSkuCount > 0)
            {
                sj += string.Format(@" AND B.SkuCount<={0}", template.MaxSkuCount);
            }
            if (template.MinGoodsCount > 0)
            {
                sj += string.Format(@" AND B.GoodsCousnt>={0}", template.MinGoodsCount);
            }
            if (template.MaxGoodsCount > 0)
            {
                sj += string.Format(@" AND B.GoodsCount<={0}", template.MaxGoodsCount);
            }
            return sj;
        }
    }
}
