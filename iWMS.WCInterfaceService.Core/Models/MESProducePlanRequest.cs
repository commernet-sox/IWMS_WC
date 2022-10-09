using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Models
{
    internal class MESProducePlanRequest
    {
        /// <summary>
        /// 计划产线id QZH产线ID：200000502
        /// </summary>
        public string LineId { get; set; }
        /// <summary>
        /// 计划日期 格式yyyy-mm-dd
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// 计划锁定标识，未锁定标识为1，加工车间锁定标识为2，总装车间锁定标识为3；非必填 固定传3
        /// </summary>
        public string IsLocked { get; set; }
    }

    public class MESProducePlanResponse
    {
        /// <summary>
        /// BOM头id
        /// </summary>
        public string BomHeaderId { get; set; }
        /// <summary>
        /// 工单号ID
        /// </summary>
        public string WipEntityId { get; set; }
        /// <summary>
        /// 工单编码
        /// </summary>
        public string WipEntityName { get; set; }
        /// <summary>
        /// 计划顺序
        /// </summary>
        public string MesPlanOrder { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 机型
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// ERP计划开始时间
        /// </summary>
        public DateTime? ErpStartDate { get; set; }
        /// <summary>
        /// 3为锁定，1为未锁定
        /// </summary>
        public string IsLocked { get; set; }
    }
}
