using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Web;
using System.Web.Http.Filters;
using FWCore;
using iWMS.APILog.FileLogger;
using Newtonsoft.Json;

namespace iWMS.WebApplication
{
	public class UniversalValidatorAttribute : AuthorizationFilterAttribute
	{
		public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			var body = await actionContext.Request.Content.ReadAsStringAsync();
			HttpContext.Current.SetBody(body);

			var oc = HttpContext.Current.Validator();
			if (!oc.IsSuccess)
			{
				CreateResponseMessage(actionContext, oc);
				return;
			}

			await base.OnAuthorizationAsync(actionContext, cancellationToken);
		}

		protected virtual string CreateResponse(Outcome oc)
		{
			var response = JsonConvert.SerializeObject(oc, GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			return response;
		}

		protected virtual string MediaType { get; } = "application/json";

		private void CreateResponseMessage(HttpActionContext actionContext, Outcome oc)
		{
			HttpContext.Current.SetLogCode(oc.Code);
			var response = CreateResponse(oc);
			actionContext.Response = new HttpResponseMessage()
			{
				Content = new StringContent(response, Encoding.UTF8, MediaType)
			};
			HttpContext.Current.WriteMessage(response);
		}
	}
}
