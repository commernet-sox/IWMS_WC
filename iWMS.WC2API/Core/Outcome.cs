using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using FWCore;
using Newtonsoft.Json;

namespace iWMS.WC2API
{
	public class Outcome
	{
		[JsonConstructor]
		public Outcome(string code, string message)
		{
			Code = code;
			Message = message;
		}

		public Outcome(int code = 200, string message = "")
		{
			Code = code.ToString();
			Message = message;
		}

		public string Code { get; set; }

		public string Message { get; set; }

		public string Flag
		{
			get
			{
				if (Code == "200")
				{
					return "success";
				}
				else
				{
					return "failure";
				}
			}
		}

		public Outcome2 ToWc2()
		{
			return new Outcome2(Code.ConvertInt32(), Message);
		}

		public object Convert(string method)
		{
			var tags = new[] { "gz.wms.pointtask.create", "gz.wms.emptypalletback.create", "gz.wms.pullpoint.create" };
			if (tags.Contains(method, StringComparer.OrdinalIgnoreCase))
			{
				return this;
			}

			return ToWc2();
		}
	}

	public class Outcome<T> : Outcome
	{
		[JsonConstructor]
		public Outcome(int code = 200, string message = "", T data = default) : base(code, message)
		{
			Data = data;
		}
		public Outcome(T data)
		{
			Code = "200";
			Message = string.Empty;
			Data = data;
		}

		public T Data { get; set; }
	}


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
}