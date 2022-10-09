using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Web.Http;
using FWCore;
using iWMS.Core;
using Microsoft.Practices.Unity;
using System.Text;
using System.Security.Cryptography;
using iWMS.WCDigitAPI.Models;

namespace iWMS.WCDigitAPI.Controllers
{

	public class PostController : ApiController
	{
		[Validator]
		[HttpPost, Route("api/post"), LogMessage]
		public async Task<object> Post()
		{
			var body = await Request.Content.ReadAsStringAsync();

			var method = HttpContext.Current.Request.QueryString.Get("method").ToLowerInvariant();
			var eventBus = GlobalContext.AppContainer.Resolve<IEventBus>();
			switch (method)
			{
				//器具类型
				case "gz.wms.container.type.sync":
					{
						var data = ReadBody<IEnumerable<ContainerRequest>>(body);
						return eventBus.Send<ContainerRequest, Outcome>(data);
					}
				//产线管理
				case "gz.wms.post_mes_productline.sync":
					{
						var data = ReadBody<IEnumerable<MESProductLineRequest>>(body);
						return eventBus.Send<MESProductLineRequest, Outcome>(data);
					}
				//工位管理
				case "gz.wms.post_mes_station.sync":
					{
						var data = ReadBody<IEnumerable<MESStationRequest>>(body);
						return eventBus.Send<MESStationRequest, Outcome>(data);
					}
				//投料点
				case "gz.wms.post_mes_point.sync":
					{
						var data = ReadBody<IEnumerable<MESPointRequest>>(body);
						return eventBus.Send<MESPointRequest, Outcome>(data);
					}
				
				//装车组
				case "gz.wms.post_mes_cargroup.sync":
					{
						var data = ReadBody<IEnumerable<MESCarGroupRequest>>(body);
						return eventBus.Send<MESCarGroupRequest, Outcome>(data);
					}
				//物料分类
				case "gz.wms.post_com_category.sync":
					{
						var data = ReadBody<IEnumerable<COMCategoryRequest>>(body);
						return eventBus.Send<COMCategoryRequest, Outcome>(data);
					}
				//ASN入库任务
				case "gz.wms.post_srm_order.sync":
					{
						var data = ReadBody<IEnumerable<ERPSRMOrderRequest>>(body);
						return eventBus.Send<ERPSRMOrderRequest, Outcome>(data);
					}
				default:
					return new Outcome(400, "无效的method参数");

			}
		}

		public T ReadBody<T>(string json)
		{
			var body = JsonConvert.DeserializeObject<T>(json, GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			return body;
		}
	}
}