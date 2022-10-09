using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Http.Filters;
using iWMS.APILog.FileLogger;
using Newtonsoft.Json;
using FWCore;
using iWMS.APILog;
using NLog;

namespace iWMS.WebApplication
{
	public class UniversalExceptionAttribute : ExceptionFilterAttribute
	{
		private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			_logger.Error(actionExecutedContext.Exception, "LogException");

			HttpContext.Current.SetLogCode("500");
			var response = CreateErrorResponse(actionExecutedContext.Exception);
			HttpContext.Current.WriteMessage(response);

			actionExecutedContext.Response = new HttpResponseMessage()
			{
				Content = new StringContent(response, Encoding.UTF8, MediaType)
			};
		}

		protected virtual string CreateErrorResponse(Exception exception)
		{
			var response = JsonConvert.SerializeObject(new Outcome(500, exception.ConvertString()), GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			return response;
		}

		protected virtual string MediaType { get; } = "application/json";

	}
}
