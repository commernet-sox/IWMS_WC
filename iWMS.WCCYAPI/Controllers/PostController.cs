using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Web.Http;
using iWMS.WCCYAPI.Models;
using FWCore;
using iWMS.Core;
using Microsoft.Practices.Unity;
using System.Text;
using System.Security.Cryptography;
using iWMS.WebApplication;

namespace iWMS.WCCYAPI.Controllers
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
				//器具类型
				case "gz.wms.container.type.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<ContainerRequest>>();
						return eventBus.Send<ContainerRequest, Outcome>(data).ToWc2();
					}
				
				//物料分类
				case "gz.wms.post_com_category.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<COMCategoryRequest>>();
						return eventBus.Send<COMCategoryRequest, Outcome>(data).ToWc2();
					}
				
				default:
					return new Outcome(400, "无效的method参数");

			}
		}
	}
}