using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;

namespace iWMS.WCDigitAPI.Models
{
    public class MESPointRequest : IEvent<Outcome>
    {
        /// <summary>
        /// 投料点名称
        /// </summary>
        public string FeedingPointName { get; set; }
        /// <summary>
        /// 投料点编码
        /// </summary>
        public string FeedingPointCode { get; set; }
        /// <summary>
        /// 所属产线编码
        /// </summary>
        public long? ProductlineCode { get; set; }
        /// <summary>
        /// 所属装车组编码
        /// </summary>
        public long? CarGroupCode { get; set; }
        /// <summary>
        /// 投料区域
        /// </summary>
        public string FeedingZone { get; set; }
        /// <summary>
        /// 投料点属性
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// 是否父投料点，是：1，否：0
        /// </summary>
        public byte? IsParent { get; set; }
        /// <summary>
        /// 所属父投料点
        /// </summary>
        public long? ParentCode { get; set; }
    }
}