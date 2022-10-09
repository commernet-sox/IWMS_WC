using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iWMS.BlueSkyAPI
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Include,
				ContractResolver = new CamelCasePropertyNamesContractResolver() { IgnoreSerializableInterface = true },
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				DateTimeZoneHandling = DateTimeZoneHandling.Local,
				DateFormatString = "yyyy-MM-dd HH:mm:ss"
			};

			config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
			config.Filters.Add(new LogExceptionFilterAttribute());
			// Web API 配置和服务

			// Web API 路由
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{action}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
