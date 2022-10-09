using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using FWCore;
using iWMS.APILog.FileLogger;
using iWMS.WebApplication;
using Newtonsoft.Json.Linq;

namespace iWMS.WebApplication
{
	/// <summary>
	/// 通用的日志报文记录
	/// </summary>
	public class UniversalLogAttribute : ActionFilterAttribute
	{
		public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
		{
			if (actionExecutedContext.Response != null)
			{
				var response = await actionExecutedContext.Response.Content.ReadAsStringAsync();

				var code = AnalysisCode(response);
				HttpContext.Current.SetLogCode(code);

				HttpContext.Current.WriteMessage(response);
			}

			await base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}

		protected virtual string AnalysisCode(string response)
		{
			var jobj = JObject.Parse(response);
			var code = jobj["code"].ConvertString();
			return code;
		}

	}
}
