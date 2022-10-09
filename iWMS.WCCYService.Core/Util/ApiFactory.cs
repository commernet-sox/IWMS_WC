using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using iWMS.Core.Http;
using FWCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using iWMS.APILog.FileLogger;
using iWMS.Core;
using iWMS.WCCYService.Core.Models;

namespace iWMS.WCCYService.Core
{
	internal static class ApiFactory
	{
		public static ApiRequest Create(string urlTag)
		{
			var baseUrl = ConfigurationUtil.GetAppSettingString("BaseUrl") + urlTag;
			var request = new ApiRequest(baseUrl);
			var reqId = Guid.NewGuid().ToString("N");
			request.WithMethod(HttpMethod.Post).WithHeader("reqId", reqId).WithHeader("reqFrom", "GMS").WithHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(ConfigurationUtil.GetAppSettingString("Basic")))).WithFormatter(GetHttpFormatter()).SetLogTag(urlTag).SetLogId(reqId);
			return request;
		}
		public static ApiRequest CreateWCMES(string urlTag)
		{
			var baseUrl = ConfigurationUtil.GetAppSettingString("WCMESUrl") + urlTag;
			var request = new ApiRequest(baseUrl);
			var reqId = Guid.NewGuid().ToString("N");
			request.WithMethod(HttpMethod.Post).WithHeader("Token", "factory2").WithFormatter(GetHttpFormatter()).SetLogTag(urlTag).SetLogId(reqId);
			return request;
		}
		public static ApiRequest CreateMES(string urlTag)
		{
			var baseUrl = ConfigurationUtil.GetAppSettingString("MESUrl") + urlTag;
			var request = new ApiRequest(baseUrl);
			var reqId = Guid.NewGuid().ToString("N");
			request.WithMethod(HttpMethod.Post).WithHeader("Token", "factory2").WithFormatter(GetHttpFormatter()).SetLogTag(urlTag).SetLogId(reqId);
			return request;
		}
		public static Outcome LogResult(this ApiRequest request)
		{
			var url = request.Request.RequestUri.ToString();
			var body = request.Body;
			try
			{
				var response = request.GetString();
				var result = request.Formatter.Deserialize<Outcome>(response);
				request.SetLogCode(result.Code.ToString());
				request.WriteMessage(response);
				return result;
			}
			catch (Exception ex)
			{
				request.SetLogCode("500");
				request.WriteMessage(ex.ToString());
				return new Outcome(500, ex.ToString());
			}
		}

		public static Outcome<T> LogResult<T>(this ApiRequest request)
		{
			var url = request.Request.RequestUri.ToString();
			var body = request.Body;
			try
			{
				var response = request.GetString();
				var result = request.Formatter.Deserialize<Outcome<T>>(response);
				request.SetLogCode(result.Code.ToString());
				request.WriteMessage(response);
				return result;
			}
			catch (Exception ex)
			{
				request.SetLogCode("500");
				request.WriteMessage(ex.ToString());
				return new Outcome<T>(500, ex.ToString());
			}
		}

		private static IHttpFormatter GetHttpFormatter()
		{
			var settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Include,
				ContractResolver = new CamelCasePropertyNamesContractResolver() { IgnoreSerializableInterface = true },
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				DateTimeZoneHandling = DateTimeZoneHandling.Local,
				DateFormatString = "yyyy-MM-dd HH:mm:ss"
			};
			return new JsonHttpFormatter(settings);
		}
	}
}
