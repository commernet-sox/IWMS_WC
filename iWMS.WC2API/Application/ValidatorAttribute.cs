using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FWCore;
using iWMS.APILog.FileLogger;
using Newtonsoft.Json;

namespace iWMS.WC2API
{
	public class ValidatorAttribute : AuthorizationFilterAttribute
	{
		public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			var body = await actionContext.Request.Content.ReadAsStringAsync();
			HttpContext.Current.Items.Add("reqBody", body);
			//await base.OnAuthorizationAsync(actionContext, cancellationToken);
			//return;

			var request = HttpContext.Current.Request;
			var mustParams = new[] { "sign", "app_key", "customerId", "format", "method", "sign_method", "timestamp", "v" };
			foreach (var pKey in mustParams)
			{
				var value = request.QueryString.Get(pKey);
				if (string.IsNullOrWhiteSpace(value))
				{
					CreateResponseMessage(actionContext, new Outcome(400, $"参数{pKey} 不能为空"));
					return;
				}
			}

			var appKey = ConfigurationUtil.GetAppSettingString("AppKey");
			if (request.QueryString.Get("app_key") != appKey)
			{
				CreateResponseMessage(actionContext, new Outcome(400, "app_key 无效"));
				return;
			}

			var customerId = ConfigurationUtil.GetAppSettingString("CustomerId");
			if (request.QueryString.Get("customerId") != customerId)
			{
				CreateResponseMessage(actionContext, new Outcome(400, "customerId 无效"));
				return;
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
				if (string.IsNullOrWhiteSpace(value))
				{
					continue;
				}

				sb.Append(pKey);
				sb.Append(value);
			}

			sb.Append(body);
			sb.Append(secret);
			var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
			var hex = new StringBuilder();
			foreach (var b in bytes)
			{
				hex.AppendFormat("{0:X2}", b);
			}

			var sign = request.QueryString.Get("sign");
            if (sign != hex.ToString())
            {
                CreateResponseMessage(actionContext, new Outcome(401, "签名错误"));
                return;
            }

            await base.OnAuthorizationAsync(actionContext, cancellationToken);
		}

		private void CreateResponseMessage(HttpActionContext actionContext, Outcome oc)
		{
			var method = HttpContext.Current.Request.QueryString.Get("method");
			var response = JsonConvert.SerializeObject(oc.Convert(method), GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);

			actionContext.Response = new HttpResponseMessage()
			{
				Content = new StringContent(response, Encoding.UTF8, "application/json")
			};

			var path = string.Concat(method, "_", Guid.NewGuid().ToString("N"), "_", oc.Code, ".log");
			FileLogUtil.WriteMessage(path, HttpContext.Current.Request.Url.ToString(), HttpContext.Current.Items["reqBody"].ConvertString(), response);
		}
	}
}