using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    public class AllocateSQL
    {
        /// <summary>
        /// 标准库存分配SQL
        /// </summary>
        public const string OrderINVSql = @"
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
PackageQty DECIMAL(18,4),
UDF01 NVARCHAR(100),
UDF02 NVARCHAR(100),
UDF03 NVARCHAR(100),
UDF04 NVARCHAR(100),
UDF09 NVARCHAR(100)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,QtyPlan,Qty,SkuStatus,PackageCode,PackageQty,UDF01,UDF02,UDF03,UDF04,UDF09)
SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,C.UDF01,C.UDF02,C.UDF03,C.UDF04,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')

UPDATE T SET T.Qty=T.Qty-ISNULL(S.QtyScan,0)
FROM @TMP T INNER JOIN 
(SELECT SkuId,OrderLineNo,SUM(ISNULL(QtyScan,0)) QtyScan FROM TRM_OrderTaskDetail WHERE OrderId='{0}' GROUP BY SkuId,OrderLineNo) S
ON (T.SkuId=S.SkuId AND T.OrderLineNo=S.OrderLineNo)

SELECT S.StorerId,S.LocationId,S.SkuId,S.QtyValid,S.CID,S.BoxCode,S.BatchNo,S.SkuStatus,
L.WHAreaId,L.PickGoodsOrder,S.UDF01,S.UDF02,S.UDF03,S.UDF04,S.UDF05,S.DUDF01,S.DUDF02,S.DUDF03,S.DUDF04,S.DUDF05,S.DUDF06,
S.DUDF07,S.DUDF08,S.DUDF09,S.DUDF10,S.DUDF11,A.Property1,L.AisleId
INTO #INV
FROM (SELECT StorerId,SkuId,BatchNo,SkuStatus FROM @TMP GROUP BY StorerId,SkuId,BatchNo,SkuStatus) T  
INNER JOIN INV_BAL S ON (T.StorerId=S.StorerId AND T.SkuId=S.SkuId AND T.SkuStatus=S.SkuStatus)
INNER JOIN COM_Location L ON S.LocationId=L.LocationId
INNER JOIN COM_WHArea A ON L.WHAreaId=A.WHAreaId
INNER JOIN COM_AISLE E ON L.AisleId=E.AisleId
WHERE S.QtyValid>0 AND S.Qty>0 AND S.QtyOut>=0 
AND S.WarehouseId='{1}' AND A.IsTaskArea=1 AND L.Status='1' AND E.Status='1'
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND FromLoc=S.LocationId)

SELECT * FROM #INV ORDER BY PickGoodsOrder,CID
SELECT * FROM @TMP";

        /// <summary>
        /// picking库存分配SQL
        /// </summary>
        public const string Picking_OrderINVSql = @"
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
PackageQty DECIMAL(18,4),
UDF01 NVARCHAR(100),
UDF02 NVARCHAR(100),
UDF03 NVARCHAR(100),
UDF04 NVARCHAR(100),
UDF09 NVARCHAR(100)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,QtyPlan,Qty,SkuStatus,PackageCode,PackageQty,UDF01,UDF02,UDF03,UDF04,UDF09)
SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,C.UDF01,C.UDF02,C.UDF03,C.UDF04,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')

UPDATE T SET T.Qty=T.Qty-ISNULL(S.QtyScan,0)
FROM @TMP T INNER JOIN 
(SELECT SkuId,OrderLineNo,SUM(ISNULL(QtyScan,0)) QtyScan FROM TRM_OrderTaskDetail WHERE OrderId='{0}' GROUP BY SkuId,OrderLineNo) S
ON (T.SkuId=S.SkuId AND T.OrderLineNo=S.OrderLineNo)

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

UPDATE S SET S.AreaId=L.AreaId
FROM #INV S INNER JOIN COM_AreaLocation L ON S.LocationId=L.LocationId

SELECT * FROM #INV ORDER BY PickGoodsOrder,CID
SELECT * FROM @TMP";

        /// <summary>
        /// SAP库存分配SQL
        /// </summary>
        public const string SAP_OrderINVSql = @"
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
PackageQty DECIMAL(18,4),
UDF01 NVARCHAR(100),
UDF02 NVARCHAR(100),
UDF03 NVARCHAR(100),
UDF04 NVARCHAR(100),
UDF09 NVARCHAR(100)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,QtyPlan,Qty,SkuStatus,PackageCode,PackageQty,UDF01,UDF02,UDF03,UDF04,UDF09)
SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,C.UDF01,C.UDF02,C.UDF03,C.UDF04,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('10','12','25','35','40') AND C.Status IN('12')

UPDATE T SET T.Qty=T.Qty-ISNULL(S.QtyScan,0)
FROM @TMP T INNER JOIN 
(SELECT SkuId,OrderLineNo,SUM(ISNULL(QtyScan,0)) QtyScan FROM TRM_OrderTaskDetail WHERE OrderId='{0}' GROUP BY SkuId,OrderLineNo) S
ON (T.SkuId=S.SkuId AND T.OrderLineNo=S.OrderLineNo)

SELECT S.StorerId,S.LocationId,S.SkuId,S.QtyValid,S.CID,S.BoxCode,S.BatchNo,S.SkuStatus,
L.WHAreaId,L.PickGoodsOrder,S.UDF01,S.UDF02,S.UDF03,S.UDF04,S.UDF05,S.DUDF01,S.DUDF02,S.DUDF03,S.DUDF04,S.DUDF05,S.DUDF06,
S.DUDF07,S.DUDF08,S.DUDF09,S.DUDF10,S.DUDF11,A.Property1,L.AisleId
INTO #INV
FROM (SELECT StorerId,SkuId,BatchNo,SkuStatus FROM @TMP GROUP BY StorerId,SkuId,BatchNo,SkuStatus) T  
INNER JOIN INV_BAL S ON (T.StorerId=S.StorerId AND T.SkuId=S.SkuId AND T.BatchNo=S.BatchNo AND T.SkuStatus=S.SkuStatus)
INNER JOIN COM_Location L ON S.LocationId=L.LocationId
INNER JOIN COM_WHArea A ON L.WHAreaId=A.WHAreaId
INNER JOIN COM_AISLE E ON L.AisleId=E.AisleId
WHERE S.QtyValid>0 AND S.Qty>0 AND S.QtyOut>=0 
AND S.WarehouseId='{1}' AND A.IsTaskArea=1 AND L.Status='1' AND E.Status='1'
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND FromLoc=S.LocationId)
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND CID=S.CID)

SELECT * FROM #INV ORDER BY PickGoodsOrder,CID
SELECT * FROM @TMP";

        /// <summary>
        /// MES叫料库存分配SQL
        /// </summary>
        public const string MES_OrderINVSql = @"
DECLARE @TMP TABLE(
FlowNo BIGINT,
OrderId NVARCHAR(30),
StorerId NVARCHAR(50),
OrderLineNo NVARCHAR(50),
SkuId NVARCHAR(30),
BatchNo NVARCHAR(30),
Qty DECIMAL(18,4),
SkuStatus NVARCHAR(10),
PackageCode NVARCHAR(10),
PackageQty DECIMAL(18,4),
UDF01 NVARCHAR(100),
UDF02 NVARCHAR(100),
UDF03 NVARCHAR(100),
UDF04 NVARCHAR(100),
UDF09 NVARCHAR(100)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,Qty,SkuStatus,PackageCode,PackageQty,UDF01,UDF02,UDF03,UDF04,UDF09)
SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,C.UDF01,C.UDF02,C.UDF03,C.UDF04,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')

SELECT InWHAreaId INTO #WHArea FROM COM_WHAreaRelation WHERE OutWHAreaId IN(
SELECT L.WHAreaId FROM @TMP T INNER JOIN COM_Location L ON T.UDF09=L.LocationId GROUP BY L.WHAreaId)

SELECT S.StorerId,S.LocationId,S.SkuId,S.QtyValid,S.CID,S.BoxCode,S.BatchNo,S.SkuStatus,
L.WHAreaId,L.PickGoodsOrder,S.UDF01,S.UDF02,S.UDF03,S.UDF04
INTO #INV
FROM (SELECT StorerId,SkuId,BatchNo,SkuStatus FROM @TMP GROUP BY StorerId,SkuId,BatchNo,SkuStatus) T  
INNER JOIN INV_BAL S ON (T.StorerId=S.StorerId AND T.SkuId=S.SkuId AND T.SkuStatus=S.SkuStatus)
INNER JOIN COM_Location L ON S.LocationId=L.LocationId
INNER JOIN COM_WHArea A ON L.WHAreaId=A.WHAreaId
INNER JOIN #WHArea WH ON A.WHAreaId=WH.InWHAreaId
INNER JOIN COM_AISLE E ON L.AisleId=E.AisleId
WHERE S.QtyValid>0 AND S.Qty>0 AND S.QtyOut=0 
AND S.WarehouseId='{1}' AND L.Status='1' AND E.Status='1' 
AND A.WHAreaType IN('STORAGE') AND A.IsTaskArea=1 AND ISNULL(S.DUDF02,'')='' AND ISNULL(S.DUDF01,'')='1101'

SELECT * FROM #INV ORDER BY BatchNo
SELECT * FROM @TMP";

        /// <summary>
        /// 库卡美的叫料库存分配SQL
        /// </summary>
        public const string MD_CALL_OrderINVSql = @"
DECLARE @TMP TABLE(
FlowNo BIGINT,
OrderId NVARCHAR(30),
StorerId NVARCHAR(50),
OrderLineNo NVARCHAR(50),
SkuId NVARCHAR(30),
BatchNo NVARCHAR(30),
Qty DECIMAL(18,4),
SkuStatus NVARCHAR(10),
PackageCode NVARCHAR(10),
PackageQty DECIMAL(18,4),
UDF01 NVARCHAR(100),
UDF02 NVARCHAR(100),
UDF03 NVARCHAR(100),
UDF04 NVARCHAR(100),
UDF09 NVARCHAR(100)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,Qty,SkuStatus,PackageCode,PackageQty,UDF01,UDF02,UDF03,UDF04,UDF09)
SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,C.UDF01,C.UDF02,C.UDF03,C.UDF04,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')

SELECT S.StorerId,S.LocationId,S.SkuId,S.QtyValid,S.CID,S.BoxCode,S.BatchNo,S.SkuStatus,
L.WHAreaId,L.PickGoodsOrder,S.UDF01,S.UDF02,S.UDF03,S.UDF04,A.AreaOrder
INTO #INV
FROM (SELECT StorerId,SkuId,BatchNo,SkuStatus FROM @TMP GROUP BY StorerId,SkuId,BatchNo,SkuStatus) T  
INNER JOIN INV_BAL S ON (T.StorerId=S.StorerId AND T.SkuId=S.SkuId AND T.SkuStatus=S.SkuStatus)
INNER JOIN COM_Location L ON S.LocationId=L.LocationId
INNER JOIN COM_WHArea A ON L.WHAreaId=A.WHAreaId
INNER JOIN COM_AISLE E ON L.AisleId=E.AisleId
WHERE S.QtyValid>0 AND S.Qty>0 AND S.QtyOut=0 
AND S.WarehouseId='{1}' AND L.Status='1' AND E.Status='1' 
AND A.IsTaskArea=1 AND ISNULL(S.DUDF02,'')=''

SELECT * FROM #INV ORDER BY AreaOrder,PickGoodsOrder
SELECT * FROM @TMP";

        /// <summary>
        /// 山东小森叫料库存分配SQL
        /// </summary>
        public const string SDXS_CALL_OrderINVSql = @"
DECLARE @TMP TABLE(
FlowNo BIGINT,
OrderId NVARCHAR(30),
StorerId NVARCHAR(50),
OrderLineNo NVARCHAR(50),
SkuId NVARCHAR(30),
BatchNo NVARCHAR(30),
Qty DECIMAL(18,4),
SkuStatus NVARCHAR(10),
PackageCode NVARCHAR(10),
PackageQty DECIMAL(18,4),
UDF01 NVARCHAR(100),
DUDF09 NVARCHAR(100)
)

INSERT INTO @TMP(FlowNo,OrderId,StorerId,OrderLineNo,SkuId,BatchNo,Qty,SkuStatus,PackageCode,PackageQty,UDF01,DUDF09)
SELECT C.FlowNo,'{0}',A.StorerId,C.OrderLineNo,C.SkuId,C.BatchNo,C.Qty,C.SkuStatus,C.PackageCode,C.PackageQty,B.UDF01,C.UDF09
FROM SOM_Order A
INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
INNER JOIN SOM_OrderDetail C ON A.BillId=C.BillId
WHERE A.BillId='{0}' AND A.Status IN('12')

SELECT S.StorerId,S.LocationId,S.SkuId,S.QtyValid,S.CID,S.BoxCode,S.BatchNo,S.SkuStatus,
L.WHAreaId,L.PickGoodsOrder,S.UDF01,S.UDF02,S.UDF03,S.UDF04,A.AreaOrder
INTO #INV
FROM (SELECT StorerId,SkuId,BatchNo,SkuStatus FROM @TMP GROUP BY StorerId,SkuId,BatchNo,SkuStatus) T  
INNER JOIN INV_BAL S ON (T.StorerId=S.StorerId AND T.SkuId=S.SkuId AND T.SkuStatus=S.SkuStatus)
INNER JOIN COM_Location L ON S.LocationId=L.LocationId
INNER JOIN COM_WHArea A ON L.WHAreaId=A.WHAreaId
INNER JOIN COM_AISLE E ON L.AisleId=E.AisleId
WHERE S.QtyValid>0 AND S.Qty>0 AND S.QtyOut=0 AND S.QtyFreeze=0 AND S.QtyMove=0
AND S.WarehouseId='{1}' AND L.Status='1' AND E.Status='1' 
AND A.IsTaskArea=1 
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND FromLoc=S.LocationId)
AND NOT EXISTS(SELECT 1 FROM TRM_TaskSJ WHERE Status='10' AND CID=S.CID)


SELECT * FROM #INV ORDER BY AreaOrder,PickGoodsOrder
SELECT * FROM @TMP";

        public const string OrderTaskDetailSql = @"SELECT A.FromLoc,A.SkuId,A.QtyScan,B.WarehouseId,B.StorerId,B.SkuId,B.SkuStatus,B.LocationId,B.CID,B.BatchNo,B.BoxCode,B.QtyValid,
B.UDF01,B.UDF02,B.UDF03,B.UDF04,B.UDF05,B.DUDF01,B.DUDF02,B.DUDF03,B.DUDF04,B.DUDF05,B.DUDF06,B.DUDF07,B.DUDF08,B.DUDF09,B.DUDF10,B.DUDF11 FROM 
(SELECT WarehouseId,StorerId,FromLoc,SkuId,CID,BoxCode,BatchNo,SkuStatus,ISNULL(SUM(QtyScan),0) QtyScan FROM TRM_OrderTaskDetail WHERE OrderId='{0}'
GROUP BY WarehouseId,StorerId,FromLoc,SkuId,CID,BoxCode,BatchNo,SkuStatus) A
LEFT JOIN (SELECT WarehouseId,StorerId,SkuId,SkuStatus,LocationId,CID,BatchNo,BoxCode,QtyValid,
UDF01,UDF02,UDF03,UDF04,UDF05,DUDF01,DUDF02,DUDF03,DUDF04,DUDF05,DUDF06,DUDF07,DUDF08,DUDF09,DUDF10,DUDF11 FROM INV_BAL
WHERE QtyValid>0
GROUP BY WarehouseId,StorerId,SkuId,SkuStatus,LocationId,CID,BatchNo,BoxCode,QtyValid,
UDF01,UDF02,UDF03,UDF04,UDF05,DUDF01,DUDF02,DUDF03,DUDF04,DUDF05,DUDF06,DUDF07,DUDF08,DUDF09,DUDF10,DUDF11) B ON 
(A.WarehouseId=B.WarehouseId AND A.StorerId=B.StorerId AND A.FromLoc=B.LocationId AND A.SkuId=B.SkuId
AND ISNULL(A.CID,'')=ISNULL(B.CID,'') AND ISNULL(A.BoxCode,'')=ISNULL(B.BoxCode,'') AND ISNULL(A.BatchNo,'')=ISNULL(B.BatchNo,'')) AND ISNULL(A.SkuStatus,'')=ISNULL(B.SkuStatus,'')
";
        public const string OrderTaskDetailOrderLineSql = @"SELECT A.FromLoc,A.SkuId,A.QtyScan,B.WarehouseId,B.StorerId,B.SkuId,B.SkuStatus,B.LocationId,B.CID,B.BatchNo,B.BoxCode,B.QtyValid,
B.UDF01,B.UDF02,B.UDF03,B.UDF04,B.UDF05,B.DUDF01,B.DUDF02,B.DUDF03,B.DUDF04,B.DUDF05,B.DUDF06,B.DUDF07,B.DUDF08,B.DUDF09,B.DUDF10,B.DUDF11 FROM 
(SELECT OrderId,OrderLineNo,WarehouseId,StorerId,FromLoc,SkuId,CID,BoxCode,BatchNo,SkuStatus,ISNULL(SUM(QtyScan),0) QtyScan FROM TRM_OrderTaskDetail WHERE OrderId='{0}'
GROUP BY OrderId,OrderLineNo,WarehouseId,StorerId,FromLoc,SkuId,CID,BoxCode,BatchNo,SkuStatus) A
LEFT JOIN (SELECT WarehouseId,StorerId,SkuId,SkuStatus,LocationId,CID,BatchNo,BoxCode,QtyValid,
UDF01,UDF02,UDF03,UDF04,UDF05,DUDF01,DUDF02,DUDF03,DUDF04,DUDF05,DUDF06,DUDF07,DUDF08,DUDF09,DUDF10,DUDF11 FROM INV_BAL
WHERE QtyValid>0
GROUP BY WarehouseId,StorerId,SkuId,SkuStatus,LocationId,CID,BatchNo,BoxCode,QtyValid,
UDF01,UDF02,UDF03,UDF04,UDF05,DUDF01,DUDF02,DUDF03,DUDF04,DUDF05,DUDF06,DUDF07,DUDF08,DUDF09,DUDF10,DUDF11) B ON 
(A.WarehouseId=B.WarehouseId AND A.StorerId=B.StorerId AND A.FromLoc=B.LocationId AND A.SkuId=B.SkuId
AND ISNULL(A.CID,'')=ISNULL(B.CID,'') AND ISNULL(A.BoxCode,'')=ISNULL(B.BoxCode,'') AND ISNULL(A.BatchNo,'')=ISNULL(B.BatchNo,'')) AND ISNULL(A.SkuStatus,'')=ISNULL(B.SkuStatus,'')
INNER JOIN SOM_OrderDetail C ON(A.OrderId=C.BillId AND A.OrderLineNo=C.OrderLineNo AND A.SkuId=C.SkuId)
WHERE C.Status IN('12')";

        public const string UpdateOrderSql = @"UPDATE SOM_Order SET AutoAllocateComplete=1,AllocateTaskReason='{1}',Version=Version+1,ModifyBy='TimerTask',ModifyDate=GETDATE() WHERE BillId='{0}' AND NOT EXISTS(SELECT 1 FROM SOM_OrderDetail WHERE BillId='{0}' AND Status IN('10','12'))";

        public const string UpdateOrderStatusSql = @"UPDATE SOM_Order SET Status='{2}',AutoAllocateComplete=1,AllocateTaskReason='{1}',Version=Version+1,ModifyBy='TimerTask',ModifyDate=GETDATE() WHERE BillId='{0}' AND Status='12'";

        public const string UpdateOrderDetailStatusSql = @"UPDATE SOM_OrderDetail SET Status='{2}',Version=Version+1,ModifyBy='TimerTask',ModifyDate=GETDATE() WHERE BillId='{0}' AND FlowNo={1} AND Status='12'";

        public const string UpdateOrderLineStatusSql = @"UPDATE SOM_OrderDetail SET Status='{1}',Version=Version+1,ModifyBy='TimerTask',ModifyDate=GETDATE() WHERE BillId='{0}' AND Status='12';
UPDATE A SET A.Status='{1}',A.Version=Version+1,A.ModifyBy='TimerTask',A.ModifyDate=GETDATE()
FROM SOM_Order A INNER JOIN SOM_SyncOrder B ON A.BillId=B.BillId
WHERE A.BillId='{0}' AND 
EXISTS(SELECT 1 FROM (SELECT SUM(Qty) Qty FROM SOM_OrderDetail WHERE BillId=A.BillId AND Status='{1}') T WHERE T.Qty=B.SkuCount);";

        public const string QueryOrderTaskDetailSql = "SELECT T.* FROM TRM_OrderTaskDetail T INNER JOIN SOM_OrderDetail S ON (T.OrderId=S.BillId AND T.OrderLineNo=S.OrderLineNo AND T.SkuId=S.SkuId) WHERE T.OrderId='{0}' AND S.Status IN('12')";
    }
}
