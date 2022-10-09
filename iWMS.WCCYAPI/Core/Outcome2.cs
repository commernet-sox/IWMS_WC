using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.Services.Description;
using FWCore;
using iWMS.WebApplication;
using Newtonsoft.Json;

namespace iWMS.WCCYAPI
{
	public class Outcome2
	{
		public Outcome2(int code = 200, string message = "")
		{
			Code = code;
			Message = message;
		}

		public int Code { get; set; }

		[JsonProperty("msg")]
		public string Message { get; set; }
	}

	internal static class OutComeExtensions
	{
		public static Outcome2 ToWc2(this Outcome oc)
		{
			return new Outcome2(oc.Code.ConvertInt32(), oc.Message);
		}

		public static object Convert(this Outcome oc, string method)
		{
			var tags = new[] { "gz.wms.pointtask.create", "gz.wms.emptypalletback.create", "gz.wms.pullpoint.create" };
			if (tags.Contains(method, StringComparer.OrdinalIgnoreCase))
			{
				return oc;
			}

			return oc.ToWc2();
		}
	}
}