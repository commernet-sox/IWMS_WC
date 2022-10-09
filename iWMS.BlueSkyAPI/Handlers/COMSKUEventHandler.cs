using Common.ISource;
using FWCore;
using iWMS.BlueSkyAPI.Models;
using iWMS.Core;
using iWMS.TypedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Handlers
{
    /// <summary>
    /// 物料信息
    /// </summary>
    public class COMSKUEventHandler : ISyncListEventHandler<COMSKURequest, Outcome>
    {
        public Outcome Handle(IEnumerable<COMSKURequest> data)
        {
            if (data == null || !data.Any())
            {
                return new Outcome(400, "数据列表为空");
            }
            if (data.Any(t => t.SkuId == null || t.SkuId == ""))
            {
                return new Outcome(400, "物料代码不能为空");
            }
            if (data.Any(t => t.StorerId == null || t.StorerId == ""))
            {
                return new Outcome(400, "货主代码不能为空");
            }
            if (data.Any(t => t.StorerId!="BSE"))
            {
                return new Outcome(400, "货主代码错误");
            }
            if (data.Any(t => t.SkuDec == null || t.SkuDec == ""))
            {
                return new Outcome(400, "物料描述不能为空");
            }
            if (data.Any(t => t.Upclist == null &&!t.Upclist.Any()))
            {
                return new Outcome(400, "条码不能为空");
            }
            var noDistinct = data.GroupBy(x => x.SkuId).All(x => x.Count() == 1);
            if (!noDistinct) 
            {
                return new Outcome(400, "物料代码重复");
            }

            //条码不能重复
            var upcCodeList = data.SelectMany(t => t.Upclist.Select(x => x.UpcCode)).ToList();
            HashSet<string> hashSet = new HashSet<string>(upcCodeList);
            if (upcCodeList.Count() != hashSet.Count()) 
            {
                return new Outcome(400, "物料条码重复");
            }

            //物料分类不存在则插入
            var categoryData = new T_COM_CategoryData();
            var categorySql = "select * from COM_Category";
            var categoryds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(categorySql, categoryData.COM_Category.TableName);
            categoryData.Merge(categoryData.COM_Category.TableName, categoryds);
            var categories = new HashSet<string>(categoryData.COM_Category.Select(t => t.CategoryId), StringComparer.OrdinalIgnoreCase);

            //100 判断 循环or查询
            var typeData = new T_COM_SKUData();
            var skuupcsql = $"select * from COM_SKUUPC where StorerId = 'BSE'";
            var skuupcds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(skuupcsql, typeData.COM_SKUUPC.TableName);
            typeData.Merge(typeData.COM_SKUUPC.TableName, skuupcds);

            var count = Math.Floor(data.Count() / 100d)+1;
            for (int i = 0; i < count; i++)
            {
                var index = i * 100;
                var list = data.Skip(index).Take(100);
                string sql = "select * from COM_SKU where ";
                int j = 0;
                foreach (var item in list)
                {
                    j++;
                    sql = sql + " (SkuId = " + "'" + item.SkuId + "'" + " and StorerId =" + "'" + item.StorerId + "'" + ") ";
                    if (j < list.Count())
                    {
                        sql += "or";
                    }
                }
                var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.COM_SKU.TableName);
                typeData.Merge(typeData.COM_SKU.TableName, ds);

                foreach (var item in list)
                {
                    var row = typeData.COM_SKU.Where(t => t.SkuId == item.SkuId && t.StorerId == item.StorerId).FirstOrDefault();
                    if (row != null)
                    {
                        row.Version = row["Version"].ConvertInt32() + 1;
                        row.ModifyBy = "System";
                        row.ModifyDate = GlobalContext.ServerTime;

                        row.SkuId = item.SkuId;
                        row.StorerId = item.StorerId;
                        row.SkuName = item.SkuDec;
                        row.SkuENName = item.EnglishName;
                        row.SkuType = item.SkuType ?? "1";
                        row.CategoryId = item.CategoryId;
                        row.BrandId = item.BrandId;
                        row.Price = item.Price;
                        row.Unit = item.GoodsUnit;
                        row.Color = item.Color;
                        row.Size = item.Size;
                        row.Spec = item.Spec;
                        row.IsSnMgt = item.IsSnMgt.ConvertString();
                        row.IsLifeMgt = item.IsLifeMgt.ConvertString();
                        row.LifeCycle = item.LifeCycle;
                        row.RejectCycle = item.RejectCycle;
                        row.UDF04 = item.CategoryName;
                        row.UDF05 = item.BrandName;
                        row.ImgUrl = item.ImgUrl;
                        row.Memo = item.Memo;
                        row.ShortName = item.ShortName;
                        row.Status = "1";
                        row.ABCType = item.ABCType;
                        row.Length = item.Length;
                        row.Width = item.Width;
                        row.Height = item.Height;
                        row.Volume = item.Length * item.Width * item.Height;
                        row.Weight = item.NetWeight;
                        row.GrossWeight = item.GrossWeight;
                        row.CartonWeight = item.CartonWeight;
                        row.CartonLength = item.CartonLength;
                        row.CartonWidth = item.CartonWidth;
                        row.CartonHeight = item.CartonHeight;
                        row.CartonVolume = item.CartonVolume;
                        row.CartonPcs = item.Pcs;
                        if (!string.IsNullOrWhiteSpace(item.ProductDate))
                        {
                            row.ProductDate = Convert.ToDateTime(item.ProductDate);
                        }
                        if (!string.IsNullOrWhiteSpace(item.ExpireDate))
                        {
                            row.ExpireDate = Convert.ToDateTime(item.ExpireDate);
                        }
                        row.ShelfLife = item.ShelfLife;
                        row.MinStockCount = item.SafetyStock;
                    }
                    else
                    {
                        row = typeData.COM_SKU.NewCOM_SKURow();
                        row.Version = 0;
                        row.CreateBy = "System";
                        row.CreateDate = GlobalContext.ServerTime;

                        row.SkuId = item.SkuId;
                        row.StorerId = item.StorerId;
                        row.SkuName = item.SkuDec;
                        row.SkuENName = item.EnglishName;
                        row.SkuType = item.SkuType ?? "1";
                        row.CategoryId = item.CategoryId;
                        row.BrandId = item.BrandId;
                        row.Price = item.Price;
                        row.Unit = item.GoodsUnit;
                        row.Color = item.Color;
                        row.Size = item.Size;
                        row.Spec = item.Spec;
                        row.IsBom = "0";
                        row.IsSnMgt = item.IsSnMgt.ConvertString();
                        row.IsLifeMgt = item.IsLifeMgt.ConvertString();
                        row.LifeCycle = item.LifeCycle;
                        row.RejectCycle = item.RejectCycle;
                        row.ImgUrl = item.ImgUrl;
                        row.UDF04 = item.CategoryName;
                        row.UDF05 = item.BrandName;
                        row.Memo = item.Memo;
                        row.ShortName = item.ShortName;
                        row.Status = "1";
                        row.ABCType = item.ABCType;
                        row.Length = item.Length;
                        row.Width = item.Width;
                        row.Height = item.Height;
                        row.Volume = item.Length * item.Width * item.Height;
                        row.Weight = item.NetWeight;
                        row.GrossWeight = item.GrossWeight;
                        row.CartonWeight = item.CartonWeight;
                        row.CartonLength = item.CartonLength;
                        row.CartonWidth = item.CartonWidth;
                        row.CartonHeight = item.CartonHeight;
                        row.CartonVolume = item.CartonVolume;
                        row.CartonPcs = item.Pcs;
                        if (!string.IsNullOrWhiteSpace(item.ProductDate))
                        {
                            row.ProductDate = Convert.ToDateTime(item.ProductDate);
                        }
                        if (!string.IsNullOrWhiteSpace(item.ExpireDate))
                        {
                            row.ExpireDate = Convert.ToDateTime(item.ExpireDate);
                        }
                        row.ShelfLife = item.ShelfLife;
                        row.MinStockCount = item.SafetyStock;
                        typeData.COM_SKU.AddCOM_SKURow(row);
                    }

                    //新增物料条码,已存在的条码不新增
                    foreach (var itemUpc in item.Upclist)
                    {
                        var rowDtl = typeData.COM_SKUUPC.Where(t => t.UpcCode == itemUpc.UpcCode).FirstOrDefault();
                        if (rowDtl==null) 
                        {
                            rowDtl = typeData.COM_SKUUPC.NewCOM_SKUUPCRow();
                            rowDtl.SkuId = row.SkuId;
                            rowDtl.StorerId = row.StorerId;
                            rowDtl.UpcCode = itemUpc.UpcCode;
                            rowDtl.IsDefault = "1";
                            rowDtl.Version = 0;
                            rowDtl.CreateBy = "System";
                            rowDtl.CreateDate = GlobalContext.ServerTime;
                            typeData.COM_SKUUPC.AddCOM_SKUUPCRow(rowDtl);
                        }
                    }

                    //不存在的分类同时插入COM_Category
                    if (!string.IsNullOrWhiteSpace(item.CategoryId) && categories.Add(item.CategoryId))
                    {
                        var rowDt2 = categoryData.COM_Category.NewCOM_CategoryRow();
                        rowDt2.Version = 1;
                        rowDt2.CreateBy = "System";
                        rowDt2.CreateDate = GlobalContext.ServerTime;
                        rowDt2.CategoryName = item.CategoryName;
                        rowDt2.CategoryId = item.CategoryId;
                        rowDt2.ParentId = "";
                        rowDt2.Layer = 10;
                        rowDt2.IsRoot = 10;
                        rowDt2.IsFoot = 10;
                        categoryData.COM_Category.AddCOM_CategoryRow(rowDt2);
                    }
                }
                GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
                typeData.AcceptChanges();
                GlobalContext.Resolve<ISource_SQLHelper>().Save(categoryData);
                categoryData.AcceptChanges();
            }
            return new Outcome();
        }
    }
}