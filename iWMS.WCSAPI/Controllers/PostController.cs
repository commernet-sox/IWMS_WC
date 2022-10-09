using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using FWCore;
using iWMS.Core;
using iWMS.WebApplication;
using Microsoft.Practices.Unity;

namespace iWMS.WCSAPI.Controllers
{
	public class PostController : ApiController
	{
		[HttpPost, Route("api/post")]
		public object Post()
		{
			var body = HttpContext.Current.GetBody();
			var method = HttpContext.Current.GetLogTag();
			var eventBus = GlobalContext.AppContainer.Resolve<IEventBus>();

			switch (method)
			{
				default:
				return new Outcome(400, "无效的method参数");

			}
		}
	}
}