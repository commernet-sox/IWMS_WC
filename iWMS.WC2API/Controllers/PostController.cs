using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Web.Http;
using iWMS.WC2API.Models;
using FWCore;
using iWMS.Core;
using Microsoft.Practices.Unity;
using System.Text;
using System.Security.Cryptography;
using iWMS.WebApplication;

namespace iWMS.WC2API.Controllers
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
				//产线管理
				case "gz.wms.post_mes_productline.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<MESProductLineRequest>>();
						return eventBus.Send<MESProductLineRequest, Outcome>(data).ToWc2();
					}
				//工位管理
				case "gz.wms.post_mes_station.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<MESStationRequest>>();
						return eventBus.Send<MESStationRequest, Outcome>(data).ToWc2();
					}
				//投料点
				case "gz.wms.post_mes_point.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<MESPointRequest>>();
						return eventBus.Send<MESPointRequest, Outcome>(data).ToWc2();
					}
				//入库任务
				case "gz.wms.post_srm_order.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<SRMOrderRequest>>();
						return eventBus.Send<SRMOrderRequest, Outcome>(data).ToWc2();
					}
				//投料任务
				//case "gz.wms.post_mes_pointtask.sync":
				//	{
				//		var data = HttpContext.Current.ReadBody<IEnumerable<MESPointTaskRequest>>();
				//		return eventBus.Send<MESPointTaskRequest, Outcome>(data).ToWc2();
				//	}
				//巡线任务
				case "gz.wms.post_mes_pointtaskline.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<MESPointTaskLineRequest>>();
						return eventBus.Send<MESPointTaskLineRequest, Outcome>(data).ToWc2();
					}
				//装车组
				case "gz.wms.post_mes_cargroup.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<MESCarGroupRequest>>();
						return eventBus.Send<MESCarGroupRequest, Outcome>(data).ToWc2();
					}
				//物料分类
				case "gz.wms.post_com_category.sync":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<COMCategoryRequest>>();
						return eventBus.Send<COMCategoryRequest, Outcome>(data).ToWc2();
					}
				//产线MES 过点信息
				case "gz.wms.pullpoint.create":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<PullPointRequest>>();
						return eventBus.Send<PullPointRequest, Outcome>(data);
					}
				//产线MES 退空任务下发
				case "gz.wms.emptypalletback.create":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<EmptyPalletBackRequest>>();
						return eventBus.Send<EmptyPalletBackRequest, Outcome>(data);
					}
				//产线MES 投料任务下发
				case "gz.wms.pointtask.create":
					{
						var data = HttpContext.Current.ReadBody<IEnumerable<ProMESPointTaskRequest>>();
						return eventBus.Send<ProMESPointTaskRequest, Outcome>(data);
					}
				//MES 生产计划
				//case "gz.wms.produceplan.create":
				//	{
				//		var data = HttpContext.Current.ReadBody<IEnumerable<MESProducePlanRequest>>();
				//		return eventBus.Send<MESProducePlanRequest, Outcome>(data);
				//	}
				////MES BOM
				//case "gz.wms.bom.create":
				//	{
				//		var data = HttpContext.Current.ReadBody<IEnumerable<MESBOMRequest>>();
				//		return eventBus.Send<MESBOMRequest, Outcome>(data);
				//	}
				default:
					return new Outcome(400, "无效的method参数");

			}
		}
	}
}