using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using Newtonsoft.Json;

namespace iWMS.WCDigitService.Core.Models
{
	public class Outcome
	{
		[JsonConstructor]
		public Outcome(int code = 200, string message = "")
		{
			Code = code;
			Message = message;
		}

		public int Code { get; set; }

		public string Message { get; set; }

		[JsonIgnore]
		public bool IsSuccess => Code == 200;
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
			Code = 200;
			Message = string.Empty;
			Data = data;
		}

		public T Data { get; set; }
	}
}