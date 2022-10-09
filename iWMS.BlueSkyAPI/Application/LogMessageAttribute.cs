using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using FWCore;
using iWMS.APILog.FileLogger;
using Newtonsoft.Json;

namespace iWMS.BlueSkyAPI
{
	public class LogMessageAttribute : ActionFilterAttribute
	{
		public LogMessageAttribute()
		{
		}

		public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
		{

			if (actionExecutedContext.Response != null)
			{
				var response = await actionExecutedContext.Response.Content.ReadAsStringAsync();
				var method = HttpContext.Current.Request.QueryString.Get("method");
				var oc = JsonConvert.DeserializeObject<Outcome>(response);
				var path = string.Concat(method, "_", Guid.NewGuid().ToString("N"), "_", oc.Code + ".log");
				FileLogUtil.WriteMessage(path, HttpContext.Current.Request.Url.ToString(), HttpContext.Current.Items["reqBody"].ConvertString(), response);
			}
			await base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}
	}
}