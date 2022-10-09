using System.IO;
using System;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using iWMS.APILog.FileLogger;
using Newtonsoft.Json;
using NLog;
using FWCore;

namespace iWMS.WCDigitAPI
{
	public class LogExceptionFilterAttribute : ExceptionFilterAttribute
	{
		private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			_logger.Error(actionExecutedContext.Exception, "LogException");
			var method = HttpContext.Current.Request.QueryString.Get("method");
			var response = JsonConvert.SerializeObject(new Outcome(500, actionExecutedContext.Exception?.ToString()).Convert(method), GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			var oc = JsonConvert.DeserializeObject<Outcome>(response);
			var path = string.Concat(method, "_", Guid.NewGuid().ToString("N"), "_", oc.Code, ".log");
			FileLogUtil.WriteMessage(path, HttpContext.Current.Request.Url.ToString(), HttpContext.Current.Items["reqBody"].ConvertString(), response, HttpContext.Current.Request.UserHostAddress);
			actionExecutedContext.Response = new HttpResponseMessage()
			{
				Content = new StringContent(response, Encoding.UTF8, "application/json")
			};
		}
	}
}