using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using iWMS.WCCYService.Core;
using iWMS.WCCYService.Core.Models;
using iWMS.WCCYService.Core.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
    /// <summary>
    /// 潍柴MES 生产计划
    /// </summary>
    public class MESProducePlanTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_MES_ProducePlanData();
            

            MESProducePlanRequest request = new MESProducePlanRequest();
            request.LineId = "200000502";
            request.StartDate = "2022-09-29";
            //request.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            request.IsLocked = "3";
            
            var oc = ApiFactory.CreateWCMES("open/listPlans").WithBody(request).LogResult<List<MESProducePlanResponse>>();
            
            //回传成功后更新主表信息
            if (oc.IsSuccess)
            {
                if (oc.Data.Count > 0)
                {
                    var sql = $"select * from MES_ProducePlan where SkuId in ('{string.Join("','", oc.Data.Select(t => t.ItemCode))}') and ProductCode in ('{string.Join("','", oc.Data.Select(t => t.ProductCode))}')";
                    var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.MES_ProducePlan.TableName);
                    typeData.Merge(typeData.MES_ProducePlan.TableName, ds);

                    foreach (var item in oc.Data)
                    {
                        var row = typeData.MES_ProducePlan.Where(t => t.SkuId == item.ItemCode && t.ProductCode == item.ProductCode && t.Status=="10").FirstOrDefault();
                        if (row != null)
                        {
                            row.Version = row["Version"].ConvertInt32() + 1;
                            row.ModifyBy = "WC2Task";
                            row.ModifyDate = GlobalContext.ServerTime;
                            //row.SkuId = item.ItemCode;
                            //row.ProductCode = item.ProductCode;

                            row.Status = "10";
                            row.OrderNo = item.WipEntityName;
                            row.BOMID = item.BomHeaderId;
                            row.OrderId = item.WipEntityId;
                            row.ProductOrder = item.MesPlanOrder.ConvertLong();
                            row.Model = item.ProductType;
                            row.StartDate = item.ErpStartDate ?? DateTime.Now;
                            row.IsLocked = item.IsLocked;

                        }
                        else
                        {
                            row = typeData.MES_ProducePlan.NewMES_ProducePlanRow();
                            row.Version = 1;
                            row.CreateBy = "WC2Task";
                            row.CreateDate = GlobalContext.ServerTime;
                            row.SkuId = item.ItemCode;
                            row.ProductCode = item.ProductCode;

                            row.OrderNo = item.WipEntityName;
                            row.Status = "10";
                            row.BOMID = item.BomHeaderId;
                            row.OrderId = item.WipEntityId;
                            row.ProductOrder = item.MesPlanOrder.ConvertLong();
                            row.Model = item.ProductType;
                            row.StartDate = item.ErpStartDate ?? DateTime.Now;
                            row.IsLocked = item.IsLocked;
                            typeData.MES_ProducePlan.AddMES_ProducePlanRow(row);
                        }
                    }
                    GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
                    typeData.AcceptChanges();

                }
            }

            return true;

        }
    }
}
