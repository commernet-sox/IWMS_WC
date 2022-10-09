using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using iWMS.WCInterfaceService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
    /// <summary>
    /// 潍柴MES BOM
    /// </summary>
    public class MESBOMTask : CommonBaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            var typeData = new T_MES_BOMData();
            var MESProducePlanData = new T_MES_ProducePlanData();
            var sql = $"select * from MES_ProducePlan a WHERE NOT EXISTS (SELECT 1 FROM dbo.MES_BOM b WHERE a.BOMID=b.BOMID)";
            //var sql = $"select * from MES_ProducePlan WHERE CONVERT(DATE,CreateDate) ='{DateTime.Now.ToString("yyyy-MM-dd")}' AND NOT EXISTS (SELECT 1 FROM dbo.MES_BOM b WHERE a.BOMID=b.BOMID)";
            var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, MESProducePlanData.MES_ProducePlan.TableName);
            MESProducePlanData.Merge(MESProducePlanData.MES_ProducePlan.TableName, ds);

            if (MESProducePlanData.MES_ProducePlan.Count > 0)
            {
                foreach (var items in MESProducePlanData.MES_ProducePlan)
                {
                    MESBOMRequest request = new MESBOMRequest();
                    request.LineId = "200000502";
                    request.ProductCode = items.ProductCode;

                    var oc = ApiFactory.CreateWCMES("open/listBoms").WithBody(request).LogResult<List<MESBOMResponse>>();

                    //回传成功后更新主表信息
                    if (oc.IsSuccess)
                    {
                        if (oc.Data.Count > 0)
                        {
                            var sqlBom = $"select * from MES_BOM where BOMID in ('{string.Join("','", oc.Data.Select(t => t.BomHeaderId))}') and Ex1 in ('{string.Join("','", oc.Data.Select(t => t.MaterialId))}')";
                            var dsBom = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlBom, typeData.MES_BOM.TableName);
                            typeData.Merge(typeData.MES_BOM.TableName, dsBom);

                            foreach (var item in oc.Data)
                            {
                                var row = typeData.MES_BOM.Where(t => t.BOMID == item.BomHeaderId && t.Ex1==item.MaterialId ).FirstOrDefault();
                                if (row != null)
                                {
                                    row.Version = row["Version"].ConvertInt32() + 1;
                                    row.ModifyBy = "WC2Task";
                                    row.ModifyDate = GlobalContext.ServerTime;

                                    
                                    row.SkuId = item.MaterialCode;
                                    row.Qty = item.Quantity.ConvertDecimal();
                                    row.KeyFlag = item.KeyFlag;
                                    row.Ex2 = item.OperationId;
                                    row.StationCode = item.OperationCode;
                                    row.Ex3 = item.AssignedSupplierId;
                                    row.Ex4 = item.MaterialName;
                                    row.SupplierCode = item.AssignedSupplierCode;
                                    row.RowId = item.RowId;
                                    
                                }
                                else
                                {
                                    row = typeData.MES_BOM.NewMES_BOMRow();
                                    row.Version = 1;
                                    row.CreateBy = "WC2Task";
                                    row.CreateDate = GlobalContext.ServerTime;
                                    row.BOMID = item.BomHeaderId;
                                    row.Ex1 = item.MaterialId;

                                    row.SkuId = item.MaterialCode;
                                    row.Qty = item.Quantity.ConvertDecimal();
                                    row.KeyFlag = item.KeyFlag;
                                    row.Ex2 = item.OperationId;
                                    row.StationCode = item.OperationCode;
                                    row.Ex3 = item.AssignedSupplierId;
                                    row.Ex4 = item.MaterialName;
                                    row.SupplierCode = item.AssignedSupplierCode;
                                    row.RowId = item.RowId;
                                    typeData.MES_BOM.AddMES_BOMRow(row);
                                }
                                
                            }
                            GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
                            typeData.AcceptChanges();
                        }
                        
                    }
                }
                
            }

            return true;

        }
    }
}
