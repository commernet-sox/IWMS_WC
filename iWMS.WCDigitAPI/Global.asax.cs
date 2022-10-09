using System;
using System.IO;
using System.Web.Http;
using FWCore;
using iWMS.APILog.FileLogger;
using Microsoft.Practices.Unity;
using NLog;
using iWMS.Core;

namespace iWMS.WCDigitAPI
{
	public class Global : System.Web.HttpApplication
	{
		private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		protected void Application_Start(object sender, EventArgs e)
		{
			FWCore.AppContext.AppDirectory = Server.MapPath("~/");
			IUnityContainer mainContainer = new UnityContainer();
			mainContainer.AddEventBus(Path.Combine(Server.MapPath("~/bin"), "iWMS.WCDigitAPI.dll"));
			GlobalContext.AppContainer = mainContainer;
			GlobalContext.ServiceMode = ServiceMode.Server;
			BllUnityWrapper.InitType = UnityInitType.WebApp;

			_logger.Info("Application_Start");
			GlobalConfiguration.Configure(WebApiConfig.Register);
			LogQueue.StartQueueTask();
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{
			var ex = Server.GetLastError().GetBaseException();
			_logger.Error(ex, "Application_Error");
		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{
			_logger.Info("Application_End");
		}
	}
}