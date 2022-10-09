using System;
using System.Collections.Generic;
using System.Web.Http;
using Common.DBCore;
using FWCore;
using Common.ISource;
using iWMS.Core;
using Microsoft.Practices.Unity;
using iWMS.WCDigitAPI.Models;
using iWMS.WCDigitAPI.Handlers;

namespace iWMS.WCDigitAPI.Controllers
{
	public class TestController : PostController
	{
		[HttpGet]
		public FWCore.UserInfo Info()
		{
			var eventBus = GlobalContext.AppContainer.Resolve<IEventBus>();
			var sql = GlobalContext.Resolve<ISource_SQLHelper>();
			var list = new List<ContainerRequest>();
			var oc = eventBus.Send<ContainerRequest, Outcome>(list);
			ISyncListEventHandler<ContainerRequest, Outcome> a = new ContainerEventHandler();
			return new FWCore.UserInfo { CompanyId = "123", UserId = "admin" };
		}
		/// <summary>
		/// 器具类型
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Post([FromBody] IEnumerable<ContainerRequest> request)
		{
			var hanlder = new ContainerEventHandler();
			return hanlder.Handle(request);
		}
		/// <summary>
		/// 产线管理
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Post_MES_ProductLine([FromBody] IEnumerable<MESProductLineRequest> request)
		{
			var handler = new MESProductLineEventHandler();
			return handler.Handle(request);
		}
		/// <summary>
		/// 工位管理
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Post_MES_Station([FromBody] IEnumerable<MESStationRequest> request)
		{
			var handler = new MESStationEventHandler();
			return handler.Handle(request);
		}
		/// <summary>
		/// 投料点
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Post_MES_Point([FromBody] IEnumerable<MESPointRequest> request)
		{
			var handler = new MESPointEventHandler();
			return handler.Handle(request);
		}
		
		/// <summary>
		/// 装车组
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Post_MES_CarGroup([FromBody] IEnumerable<MESCarGroupRequest> request)
		{
			var handler = new MESCarGroupEventHandler();
			return handler.Handle(request);
		}

		/// <summary>
		/// 物料分类
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public Outcome Post_COM_Category([FromBody] IEnumerable<COMCategoryRequest> request)
		{
			var handler = new COMCategoryEventHandler();
			return handler.Handle(request);
		}
	}
}