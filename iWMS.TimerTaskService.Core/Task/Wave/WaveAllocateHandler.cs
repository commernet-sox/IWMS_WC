using Common.DBCore;
using FWCore;
using iWMS.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
   public class WaveAllocateHandler
    {
        #region SQL定义
        private const string Picking_WaveINVSql = @"
DECLARE @TMP TABLE(
FlowNo BIGINT,
OrderId NVARCHAR(30),
StorerId NVARCHAR(50),
OrderLineNo NVARCHAR(50),
SkuId NVARCHAR(30),
BatchNo NVARCHAR(30),
QtyPlan DECIMAL(18,4),
Qty DECIMAL(18,4),
SkuStatus NVARCHAR(10),
PackageCode NVARCHAR(10),
PackageQty DECIMAL(18,4)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,QtyPlan,Qty,SkuStatus,PackageCode,PackageQty)
SELECT E.FlowNo,C.BillId,C.StorerId,E.OrderLineNo,E.SkuId,E.BatchNo,E.Qty,E.Qty,E.SkuStatus,E.PackageCode,E.PackageQty
FROM SOM_Wav A INNER JOIN SOM_WavDetail B ON A.BillId=B.BillId
INNER JOIN SOM_Order C ON C.BillId=B.OrderId
INNER JOIN SOM_SyncOrder D ON D.BillId=C.BillId
INNER JOIN SOM_OrderDetail E ON E.BillId=D.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')

SELECT S.StorerId,S.LocationId,S.SkuId,S.QtyOut,S.QtyValid,S.CID,S.BoxCode,S.BatchNo,S.SkuStatus,
L.WHAreaId,L.PickGoodsOrder,S.UDF01,S.UDF02,S.UDF03,S.UDF04,S.UDF05,S.DUDF01,S.DUDF02,S.DUDF03,S.DUDF04,S.DUDF05,S.DUDF06,
S.DUDF07,S.DUDF08,S.DUDF09,S.DUDF10,S.DUDF11,A.Property1,L.AisleId,A.WHAreaType,CAST('' AS VARCHAR(30)) AreaId
INTO #INV
FROM (SELECT StorerId,SkuId,BatchNo,SkuStatus FROM @TMP GROUP BY StorerId,SkuId,BatchNo,SkuStatus) T  
INNER JOIN INV_BAL S ON (T.StorerId=S.StorerId AND T.SkuId=S.SkuId AND T.SkuStatus=S.SkuStatus)
INNER JOIN COM_Location L ON S.LocationId=L.LocationId
INNER JOIN COM_WHArea A ON L.WHAreaId=A.WHAreaId
INNER JOIN COM_AISLE E ON L.AisleId=E.AisleId
WHERE S.QtyValid>0 AND S.Qty>0 AND S.QtyMove=0 AND S.QtyOut>=0
AND S.WarehouseId='{1}' AND A.IsTaskArea=1 AND L.Status='1' AND E.Status='1'
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND FromLoc=S.LocationId)
{2}

UPDATE S SET S.AreaId=L.AreaId
FROM #INV S INNER JOIN COM_AreaLocation L ON S.LocationId=L.LocationId

SELECT * FROM #INV ORDER BY PickGoodsOrder,CID
SELECT * FROM @TMP";
        #endregion


        //public bool Allocate(DataRow row)
        //{
             
        //}

        //private Tuple<bool,StrJoin> Calc(List<TRM_TaskXJDetail> tasks,DataRow row)
        //{
        //    StrJoin sj = string.Empty;
        //    DataSet dataSet = BusinessDbUtil.GetDataSet(string.Format(Picking_WaveINVSql, row["BillId"],row["WarehouseId"],""));
        //    var stockTable = dataSet.Tables[0];
        //    var detailTable = dataSet.Tables[1];

        //}
    }
}
