using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.WC2API.Models
{
    public class MESBOMRequest : IEvent<Outcome>
    {
        /// <summary>
        /// bom id
        /// </summary>
        public string BomHeaderId { get; set; }
        /// <summary>
        /// 物料id
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 物料编码（件号）
        /// </summary>
        public string ItemCode { get; set; }
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