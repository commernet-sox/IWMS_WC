using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Web.Http;

namespace iWMS.WebApplication
{
	public static class UniversalApiConfig
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

			// Web API 路由
			config.MapHttpAttributeRoutes();

			// Web API 配置和服务
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{action}",
				defaults: new { id = RouteParameter.Optional }
			);
		}

		public static void RegisterFilters(HttpConfiguration config)
		{
			config.Filters.Add(new UniversalExceptionAttribute());
			config.Filters.Add(new UniversalLogAttribute());
			config.Filters.Add(new UniversalValidatorAttribute());
		}
	}
}
