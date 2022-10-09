using System;
using System.Net.Http;
using System.Text;
using FWCore;
using iWMS.APILog.FileLogger;
using iWMS.Core.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iWMS.BlueSkyService.Core
{
	internal static class ApiFactory
	{
		public static ApiRequest Create(string urlTag)
		{
			var baseUrl = ConfigurationUtil.GetAppSettingString("BaseUrl") + urlTag;
			var request = new ApiRequest(baseUrl);
			request.WithMethod(HttpMethod.Post).WithFormatter(GetHttpFormatter()).SetLogTag(urlTag);
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
