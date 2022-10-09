using Common.DBCore;
using Common.ISource;
using FWCore;
using iWMS.APILog;
using iWMS.BlueSkyAPI.Models;
using iWMS.Core;
using iWMS.TypedData;
using iWMS.WebApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.BlueSkyAPI.Handlers
{
    /// <summary>
    /// 盘点单创建任务
    /// </summary>
    public class ORDERCancelEventHandler : ISyncEventHandler<ORDERCancelRequest, Outcome>
    {
        public Outcome Handle(ORDERCancelRequest data)
        {
            if (data == null)
            {
                return new Outcome(400, "数据列表为空");
            }
            if (string.IsNullOrWhiteSpace(data.OrderCode))
            {
                return new Outcome(400, "任务编号不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.OrderType))
            {
                return new Outcome(400, "订单类型不能为空");
            }
            if (string.IsNullOrWhiteSpace(data.StorerId))
            {
                return new Outcome(400, "货主代码不能为空");
            }
            if (data.StorerId!="BSE")
            {
                return new Outcome(400, "货主代码数值错误");
            }
            string[] orderTypes = new string[3] { "asn", "so", "inventory" };
            if (!orderTypes.Contains(data.OrderType))
            {
                return new Outcome(400, "盘点类型数据错误");
            }

            switch (data.OrderType)
            {
                //取消入库订单
                case "asn":
                    {
                        var typeData = new T_SRM_OrderData();
                        var sql = $"select * from SRM_Order where SyncBillId = '{data.OrderCode}'";
                        var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SRM_Order.TableName);
                        typeData.Merge(typeData.SRM_Order.TableName, ds);

                        if (typeData.SRM_Order==null||typeData.SRM_Order.Count < 0)
                        {
                            return new Outcome(400, "入库单不存在，请重新输入！");
                        }
                        if (typeData.SRM_Order.FirstOrDefault().Status!="10"&& typeData.SRM_Order.FirstOrDefault().Status != "12") 
                        {
                            return new Outcome(400, "该入库单状态为"+ typeData.SRM_Order.FirstOrDefault().Status + "不允许取消");
                        }
                        var dt = typeData.SRM_Order[0];
                        var updateSql = @"UPDATE SRM_Order SET Status='00',Version=Version+1,ModifyBy='System',ModifyDate=GETDATE() WHERE SyncBillId='{0}' and Status in('10','12')";
                        try
                        {
                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(updateSql, data.OrderCode));

                        }
                        catch (Exception ex)
                        {
                            return new Outcome(400, ex.ToString());
                        }

                        return new Outcome();
                    }
                //取消出库订单
                case "so":
                    {
                        var typeData = new T_SOM_OrderData();
                        var sql = $"select * from SOM_Order where SyncBillId = '{data.OrderCode}'";
                        var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.SOM_Order.TableName);
                        typeData.Merge(typeData.SOM_Order.TableName, ds);

                        if (typeData.SOM_Order.Count < 0)
                        {
                            return new Outcome(400, "出库单不存在，请重新输入！");
                        }
                        if (typeData.SOM_Order.FirstOrDefault().Status != "10" && typeData.SOM_Order.FirstOrDefault().Status != "106")
                        {
                            return new Outcome(400, "该出库单状态为" + typeData.SOM_Order.FirstOrDefault().Status + "不允许取消");
                        }

                        //var row = typeData.SOM_OrderHold.NewSOM_OrderHoldRow();
                        ////获取BillId  
                        //row.BillId = typeData.SOM_Order.FirstOrDefault().BillId;
                        //row.SyncBillId = data.OrderCode;
                        //row.StorerId = data.StorerId;
                        //row.HoldType = "10";
                        //row.HoldDate = GlobalContext.ServerTime;
                        //row.Status = "10";
                        //row.Version = 0;
                        //row.CreateBy = "System";
                        //row.CreateDate = GlobalContext.ServerTime;
                        //row.ModifyBy = "System";
                        //row.ModifyDate = GlobalContext.ServerTime;
                        //row.Memo = "测试回传";
                        //typeData.SOM_OrderHold.AddSOM_OrderHoldRow(row);
                        //GlobalContext.Resolve<ISource_SQLHelper>().Save(typeData);
                        //typeData.AcceptChanges();
                        //string procSql = string.Format(@"EXEC SP_SOM_Order_CancelHandler '{0}','{1}'", typeData.SOM_Order.FirstOrDefault().BillId, "System");
                        //var result = BusinessDbUtil.InvokeProc(procSql);
                        //if (!result.IsSuccess)
                        //{
                        //    return new Outcome(400, result.ErrMsg);
                        //}

                        string sql1 = string.Format(@"INSERT INTO SOM_OrderHold (BillId, SyncBillId,StorerId,HoldType,HoldDate,Status,Version,CreateBy,CreateDate,ModifyBy,ModifyDate,Memo) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')" , typeData.SOM_Order.FirstOrDefault().BillId, data.OrderCode, data.StorerId, "10", GlobalContext.ServerTime, "10",0, "System", GlobalContext.ServerTime,"System", GlobalContext.ServerTime,"");

                        string sql2 = string.Format(@"EXEC SP_SOM_Order_CancelHandler '{0}','{1}'", typeData.SOM_Order.FirstOrDefault().BillId, "System");

                        BusinessDbUtil.DoActionWithTrascation(dbUtil =>
                        {
                            try
                            {
                                dbUtil.ExecuteNonQuery(sql1);
                                dbUtil.ExecuteNonQuery(sql2);
                            }
                            catch (Exception ex)
                            {
                                NLogUtil.WriteError(ex.ToString());
                                throw ex;
                            }
                        });

                        return new Outcome();
                    }
                //取消盘点单
                case "inventory":
                    {
                        var typeData = new T_WRM_InventoryData();
                        var sql = $"select * from WRM_Inventory where OrigBillId = '{data.OrderCode}'";
                        var ds = GlobalContext.Resolve<ISource_SQLHelper>().GetDataSet(sql, typeData.WRM_Inventory.TableName);
                        typeData.Merge(typeData.WRM_Inventory.TableName, ds);
                        if (typeData.WRM_Inventory.Count < 0)
                        {
                            return new Outcome(400, "盘点单不存在，请重新输入！");
                        }
                        if (typeData.WRM_Inventory.FirstOrDefault().Status != "10")
                        {
                            return new Outcome(400, "该盘点单状态为" + typeData.WRM_Inventory.FirstOrDefault().Status + "不允许取消");
                        }
                        var dt = typeData.WRM_Inventory[0];
                        var updateSql = @"UPDATE WRM_Inventory SET Status='00',Version=Version+1,ModifyBy='System',ModifyDate=GETDATE() WHERE OrigBillId='{0}' and Status in('10')";
                        try
                        {
                            GlobalContext.Resolve<ISource_SQLHelper>().ExecuteNonQuery(string.Format(updateSql, data.OrderCode));
                        }
                        catch (Exception ex)
                        {
                            return new Outcome(400, ex.ToString());
                        }
                        return new Outcome();
                    }
                default:
                    return new Outcome();
            }
        }
    }
}