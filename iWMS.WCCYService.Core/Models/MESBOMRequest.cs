using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCCYService.Core.Models
{
    internal class MESBOMRequest
    {
        /// <summary>
        /// 计划产线id，string，QZH产线ID：200000502
        /// </summary>
        public string LineId { get; set; }
        /// <summary>
        /// 发动机编号，string，必填
        /// </summary>
        public string ProductCode { get; set; }
    }

    public class MESBOMResponse
    {
        /// <summary>
        /// bom id
        /// </summary>
        public string BomHeaderId { get; set; }
        /// <summary>
        /// 物料id
        /// </summary>
        public string MaterialId { get; set; }
        /// <summary>
        /// 物料编码（件号）
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 物料数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 关重属性
        /// </summary>
        public string KeyFlag { get; set; }
        /// <summary>
        /// 工位id
        /// </summary>
        public string OperationId { get; set; }
        /// <summary>
        /// 工位编码
        /// </summary>
        public string OperationCode { get; set; }
        /// <summary>
        /// 分配供应商id
        /// </summary>
        public string AssignedSupplierId { get; set; }
        /// <summary>
        /// 分配供应商CODE
        /// </summary>
        public string AssignedSupplierCode { get; set; }
        /// <summary>
        /// 行ID
        /// </summary>
        public string RowId { get; set; }
    }
}
