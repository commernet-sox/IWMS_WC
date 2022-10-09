using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WebAPICore.Model
{
    [Serializable]
   public class DeliveryTaskRequest
    {
        public string taskId { get; set; }
        public string taskType { get; set; }
        public int taskPriority { get; set; }
        public string ifEmptyPalletExist { get; set; }
        public string emptyPalletPosition { get; set; }
        public string palletId { get; set; }
        public string sendTime { get; set; }
        public string backupStr1 { get; set; }
        public string backupStr2 { get; set; }
        public string backupStr3 { get; set; }
        public string backupStr4 { get; set; }
        public string backupStr5 { get; set; }
        public string backupStr6 { get; set; }
        public List<DeliveryTaskRequestItem> items { get; set; }
    }

    [Serializable]
    public class DeliveryTaskRequestItem
    {
        public string bizPlantId { get; set; }
        public string workshopId { get; set; }
        public string machineLineId { get; set; }
        public string orderNo { get; set; }
        public string orderItemId { get; set; }
        public string batchNo { get; set; }
        public string skuId { get; set; }
        public decimal qty { get; set; }
        public string unit { get; set; }
        public string locationId { get; set; }
    }
}
