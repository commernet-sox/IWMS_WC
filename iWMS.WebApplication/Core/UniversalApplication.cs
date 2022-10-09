using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Http;
using FWCore;
using iWMS.APILog;
using iWMS.Core;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace iWMS.WebApplication
{
	public static class UniversalApplication
	{
		public static void Start(HttpApplication application, Assembly assembly, bool registerFilters = true)
		{
			FWCore.AppContext.AppDirectory = application.Server.MapPath("~/");
			IUnityContainer mainContainer = new UnityContainer();
			mainContainer.AddEventBus(assembly);
			//从配置文件读取Container配置
			var funity = ConfigurationUtil.GetAppSettingString("file.b");

			if (!(ConfigurationManager.GetSection(funity) is ConfigurationSection cs))
			{
				return;
			}
			var section = cs as UnityConfigurationSection;

			section.Configure(mainContainer, "Core.Contianer");

			GlobalContext.AppContainer = mainContainer;
			GlobalContext.ServiceMode = ServiceMode.Server;
			BllUnityWrapper.InitType = UnityInitType.WebApp;

			NLogUtil.WriteInfo("Application_Start");
			GlobalConfiguration.Configure(UniversalApiConfig.Register);
			if (registerFilters)
			{
				GlobalConfiguration.Configure(UniversalApiConfig.RegisterFilters);
			}

			LogQueue.StartQueueTask();
		}

		public static void Error(HttpApplication application)
		{
			var ex = application.Server.GetLastError().GetBaseException();
			NLogUtil.WriteError(ex.ConvertString());
		}

		public static void End()
		{
			NLogUtil.WriteInfo("Application_End");
		}
	}
}
