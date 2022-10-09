using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;

namespace iWMS.WCDigitAPI.Models
{
    public class COMCategoryRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 物料分类编码（唯一）
        /// </summary>
        public string CategoryCode { get; set; }
        /// <summary>
        /// 物料分类名称
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 物料规格
        /// </summary>
        public string SpecificationName { get; set; }
        /// <summary>
        /// 产线编码
        /// </summary>
        public string ProductLineCode { get; set; }
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductLineName { get; set; }
    }
}