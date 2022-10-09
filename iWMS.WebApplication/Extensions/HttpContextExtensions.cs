using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FWCore;
using iWMS.APILog.FileLogger;
using Newtonsoft.Json;

namespace iWMS.WebApplication
{
	public static class HttpContextExtensions
	{
		private const string BodyStr = "reqBody";
		private const string LogId = "logId";
		private const string LogTag = "logTag";
		private const string LogCode = "logCode";

		/// <summary>
		/// 设置原始body
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="body"></param>
		public static void SetBody(this HttpContext ctx, string body)
		{
			ctx.Items[BodyStr] = body;
		}

		/// <summary>
		/// 获取原始Body
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static string GetBody(this HttpContext ctx)
		{
			if (ctx.Items.Contains(BodyStr))
			{
				return ctx.Items[BodyStr].ConvertString();
			}

			return string.Empty;
		}

		public static void SetLogTag(this HttpContext ctx, string tag)
		{
			ctx.Items[LogTag] = tag;
		}

		public static string GetLogTag(this HttpContext ctx)
		{
			if (ctx.Items.Contains(LogTag))
			{
				return ctx.Items[LogTag].ConvertString().ToLowerInvariant();
			}

			return string.Empty;
		}

		/// <summary>
		/// 设置日志文件订单Id
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="logId"></param>
		public static void SetLogId(this HttpContext ctx, string logId)
		{
			if (string.IsNullOrWhiteSpace(logId))
			{
				logId = CreateLogId();
			}

			ctx.Items[LogId] = logId;
		}

		private static string CreateLogId()
		{
			return Guid.NewGuid().ToString("N");
		}

		/// <summary>
		/// 获取日志文件订单Id
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static string GetLogId(this HttpContext ctx)
		{
			var logId = string.Empty;

			if (ctx.Items.Contains(LogId))
			{
				logId = ctx.Items[LogId].ConvertString();
			}

			if (string.IsNullOrWhiteSpace(logId))
			{
				logId = CreateLogId();
			}

			return logId;
		}

		public static void SetLogCode(this HttpContext ctx, string code)
		{
			ctx.Items[LogCode] = code;
		}

		public static string GetLogCode(this HttpContext ctx)
		{
			if (ctx.Items.Contains(LogCode))
			{
				return ctx.Items[LogCode].ConvertString();
			}

			return string.Empty;
		}

		public static void WriteMessage(this HttpContext ctx, string response)
		{
			var tag = ctx.GetLogTag();
			var num = ctx.GetLogId();
			var code = ctx.GetLogCode();
			var url = ctx.Request.Url.ToString();
			var body = ctx.GetBody();
			var ip = ctx.Request.UserHostAddress;

			var path = string.Concat(tag, "_", num, "_", code + ".log");
			FileLogUtil.WriteMessage(path, url, body, response, ip);
		}

		public static T ReadBody<T>(this HttpContext ctx)
		{
			var json = ctx.GetBody();
			var body = JsonConvert.DeserializeObject<T>(json, GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			return body;
		}

		/// <summary>
		/// 通用的验证
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static Outcome Validator(this HttpContext ctx)
		{
			var request = ctx.Request;
			var mustParams = new[] { "sign", "app_key", "customerId", "format", "method", "sign_method", "timestamp", "v" };
			foreach (var pKey in mustParams)
			{
				var value = request.QueryString.Get(pKey);
				if (string.IsNullOrWhiteSpace(value))
				{
					return new Outcome(400, $"参数{pKey} 不能为空");
				}

				if (pKey == "method")
				{
					ctx.SetLogTag(value);
				}
			}

			var appKey = ConfigurationUtil.GetAppSettingString("AppKey");
			if (request.QueryString.Get("app_key") != appKey)
			{
				return new Outcome(400, "app_key 无效");
			}

			var customerId = ConfigurationUtil.GetAppSettingString("CustomerId");
			if (request.QueryString.Get("customerId") != customerId)
			{
				return new Outcome(400, "customerId 无效");
			}

			var secret = ConfigurationUtil.GetAppSettingString("AppSecret");
			var sb = new StringBuilder();
			sb.Append(secret);

			foreach (var pKey in request.QueryString.AllKeys.OrderBy(t => t))
			{
				if (pKey.Equals("sign", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!mustParams.Contains(pKey, StringComparer.OrdinalIgnoreCase))
				{
					continue;
				}
				 
				var value = request.QueryString.Get(pKey);

				sb.Append(pKey);
				sb.Append(value);
			}

			sb.Append(ctx.GetBody());
			sb.Append(secret);
			var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
			var hex = new StringBuilder();
			foreach (var b in bytes)
			{
				hex.AppendFormat("{0:X2}", b);
			}

			var sign = request.QueryString.Get("sign");
            //if (sign != hex.ToString())
            //{
            //    return new Outcome(401, "签名错误");
            //}

            return new Outcome();
		}
	}
}
