using Common.ISource;
using FWCore;
using FWCore.TaskManage;
using iWMS.TypedData;
using iWMS.WCInterfaceService.Core.Models;
using iWMS.WCInterfaceService.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
	/// <summary>
	/// 物料档案
	/// </summary>
    public class COMSKUTask : CommonBaseTask
    {
		protected override bool Execute(TaskConfig config)
		{
			var typeData = new T_COM_SKUData();
			var sql = $"select top 1 * from COM_SKU Order by CreateDate desc";
			var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.COM_SKU.TableName);
			typeData.Merge(typeData.COM_SKU.TableName, ds);

			var ComSkuReq = new COMSKURequest();
			ComSkuReq.StartTime = typeData.COM_SKU.FirstOrDefault()==null ? DateTime.Now.AddDays(-31): typeData.COM_SKU.FirstOrDefault().CreateDate;
			ComSkuReq.EndTime = DateTime.Now;
			ComSkuReq.LineCode = "QZH";
			
			var oc = ApiFactory.Create("SC/LES/GmsMaterialDateSync").WithBody(ComSkuReq).LogResult<List<COMSKUResponse>>();
			if (oc.IsSuccess)
			{
				if (oc.Data.Count > 0)
				{
					var sqlDtl = $"select * from COM_SKU where SkuId in ('{string.Join("','", oc.Data.Select(t => t.Code))}')";
					var dsDtl = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sqlDtl, typeData.COM_SKU.TableName);
					typeData.Merge(typeData.COM_SKU.TableName, dsDtl);
				}

				foreach (var item in oc.Data)
				{
					var row = typeData.COM_SKU.Where(t => t.SkuId == item.Code).FirstOrDefault();
					if (row != null)
					{
						row.Version = row["Version"].ConvertInt32() + 1;
						row.ModifyBy = "WC2Task";
						row.ModifyDate = GlobalContext.ServerTime;

						row.SkuName = item.Name;
						row.CategoryId = item.CategoryCode;
						row.Spec = item.SpecificationName;
						row.UDF01 = item.DeliveryTypeCode;
						row.UDF02 = item.UtensilTypeCode;
						row.UDF03 = item.ProductLineCode;
						row.UDF04 = item.DeliveryPack.ConvertString();
						row.UDF05 = item.Subassembly.ConvertString();
						row.UDF06 = item.CategoryName;
						
					}
					else
					{
						row = typeData.COM_SKU.NewCOM_SKURow();
						row.StorerId = Commons.StoreId;
						row.SkuId = item.Code;
						row.SkuName = item.Name;
						row.CategoryId = item.CategoryCode;
						row.Spec = item.SpecificationName;
						row.UDF01 = item.DeliveryTypeCode;
						row.UDF02 = item.UtensilTypeCode;
						row.UDF03 = item.ProductLineCode;
						row.UDF04 = item.DeliveryPack.ConvertString();
						row.UDF05 = item.Subassembly.ConvertString();
						row.UDF06 = item.CategoryName;

						row.CreateDate = ComSkuReq.EndTime;
						row.CreateBy = "WC2Task";
						row.Version = 1;

						typeData.COM_SKU.AddCOM_SKURow(row);
					}
					
				}
				GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
				typeData.AcceptChanges();
			}
			Console.WriteLine(DateTime.Now);
			return true;
		}
	}
}
