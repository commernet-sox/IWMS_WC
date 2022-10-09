using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.Core;

namespace iWMS.WCDigitAPI.Models
{
	public class ContainerRequest : IEvent<Outcome>
	{
		/// <summary>
		/// 器具类型编码
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// 器具类型名称
		/// </summary>
		public string ApplianceTypeName { get; set; }

		/// <summary>
		/// 投料包装数
		/// </summary>
		public string FeedPackaging { get; set; }

		/// <summary>
		/// 逻辑删除标识
		/// </summary>
		public byte Removed { get; set; }
	}
}