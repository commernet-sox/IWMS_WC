using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using Newtonsoft.Json;

namespace iWMS.BlueSkyAPI
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
}