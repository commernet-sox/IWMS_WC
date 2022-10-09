using System;
using System.Collections.Generic;
using System.Web.Http;
using Common.DBCore;
using iWMS.WCCYAPI.Handlers;
using iWMS.WCCYAPI.Models;
using FWCore;
using Common.ISource;
using iWMS.Core;
using Microsoft.Practices.Unity;
using iWMS.WebApplication;

namespace iWMS.WCCYAPI.Controllers
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