using iWMS.BlueSkyAPI.Handlers;
using iWMS.BlueSkyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace iWMS.BlueSkyAPI.Controllers
{
    public class TestController : PostController
    {

		/// <summary>
		/// 物料同步任务下发
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome SKU([FromBody] IEnumerable<COMSKURequest> request)
		{
			var handler = new COMSKUEventHandler();
			return handler.Handle(request);
		}
		/// <summary>
		/// 蓝天新能源 入库任务下发
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Asn([FromBody] IEnumerable<SRMOrderRequest> request)
		{
			var handler = new SRMOrderEventHandler();
			return handler.Handle(request);
		}
		/// <summary>
		/// 蓝天新能源 出库任务下发
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Delivery([FromBody] IEnumerable<SOMOrderRequest> request)
		{
			var handler = new SOMOrderEventHandler();
			return handler.Handle(request);
		}
		/// <summary>
		/// 蓝天新能源 盘点任务下发
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Inventory([FromBody] IEnumerable<WRMInventoryRequest> request)
		{
			var handler = new WRMInventoryEventHandler();
			return handler.Handle(request);
		}
	}
}