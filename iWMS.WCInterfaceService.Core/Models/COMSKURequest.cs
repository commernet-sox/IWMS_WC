using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Models
{
    public class COMSKURequest
    {
        /// <summary>
        /// 拉取开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 拉取结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 产线code  数字产业园公用
        /// </summary>
        public string LineCode { get; set; }
    }

    public class COMSKUResponse
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 物料分类名称
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 物料分类编码
        /// </summary>
        public string CategoryCode { get; set; }
        /// <summary>
        /// 物料规格
        /// </summary>
        public string SpecificationName { get; set; }
        /// <summary>
        /// 投料类型
        /// </summary>
        public string DeliveryTypeCode { get; set; }
        /// <summary>
        /// 器具类型编码
        /// </summary>
        public string UtensilTypeCode { get; set; }
        /// <summary>
        /// 所属产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 投料包装
        /// </summary>
        public int? DeliveryPack { get; set; }
        /// <summary>
        /// 是否部装件（0：否/1：是）
        /// </summary>
        public byte? Subassembly { get; set; }
    }
}
