using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FWCore;
using iWMS.BlueSkyAPI.Models;
using iWMS.Core;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Controllers
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
				//物料同步任务下发
				case "gz.wms.sku.create":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<COMSKURequest>>();
						return eventBus.Send<COMSKURequest, Outcome>(data);
					}
				//入库任务下发
				case "gz.wms.asn.create":
					{
						var data = HttpContext.Current.ReadBody<SRMOrderRequest>();
						return eventBus.Send(data);
					}
				//出库任务下发
				case "gz.wms.delivery.create":
					{
						var data = HttpContext.Current.ReadBody<SOMOrderRequest>();
						return eventBus.Send(data);
					}
				//盘点任务下发
				case "gz.wms.inventory.create":
					{
						var data = HttpContext.Current.ReadBody<WRMInventoryRequest>();
						return eventBus.Send(data);
					}
				//订单取消任务下发
				case "gz.wms.order.cancel":
					{
						var data = HttpContext.Current.ReadBody<ORDERCancelRequest>();
						return eventBus.Send(data);
					}
				default:
				return new Outcome(400, "无效的method参数");

			}
		}
	}
}