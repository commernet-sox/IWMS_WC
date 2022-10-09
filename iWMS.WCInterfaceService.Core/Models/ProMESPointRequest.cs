using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Models
{
    public class ProMESPointRequest
    {
        /// <summary>
        /// 任务号
        /// </summary>
        public string TaskId { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductlineCode { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string OperationCode { get; set; }
        /// <summary>
        /// 投料点
        /// </summary>
        public string LocationId { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        public string PalletId { get; set; }
        /// <summary>
        /// 物料明细
        /// </summary>
        public List<ProMESPointRequestDetail> Items { get; set; }
        /// <summary>
        /// 备用字段1
        /// </summary>
        public string BackupStr1 { get; set; }
        /// <summary>
        /// 备用字段2
        /// </summary>
        public string BackupStr2 { get; set; }

        /// <summary>
        /// 备用字段3
        /// </summary>
        public string BackupStr3 { get; set; }
    }
    public class ProMESPointRequestDetail
    {
        /// <summary>
        /// 发动机序列号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 物料位置
        /// </summary>
        public string LocationId { get; set; }
        /// <summary>
        /// 物料批次
        /// </summary>
        public string MaterialBatch { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string vendorCode { get; set; }
        /// <summary>
        /// 是否关键件 1表示是，0表示否
        /// </summary>
        public int IsKeyParts { get; set; }
        /// <summary>
        /// 是批次件还是序列件   0既不是批次也不是序列件，1表示批次件，2表示序列件
        /// </summary>
        public int IsBatch { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TrimMark { get; set; }

    }
}
