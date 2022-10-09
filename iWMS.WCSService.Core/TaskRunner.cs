using FWCore;
using FWCore.TaskManage;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCSService.Core
{
	public static class TaskRunner
	{
		public static TaskManager Manager = null;

		public static void Start()
		{
			if (Manager != null && Manager.Tasks != null && Manager.Tasks.Count > 0)
			{
				throw new Exception("TaskRunner is running,Restart failure!");
			}

			var mainContainer = new UnityContainer();

			//从配置文件读取Container配置
			var funity = ConfigurationUtil.GetAppSettingString("file.b");

			var cs = ConfigurationManager.GetSection(funity) as ConfigurationSection;
			if (null == cs)
			{
				return;
			}
			var section = cs as UnityConfigurationSection;

			section.Configure(mainContainer, "Core.Contianer");
			GlobalContext.AppContainer = mainContainer;
			GlobalContext.ServiceMode = ServiceMode.Server;
			BllUnityWrapper.InitType = UnityInitType.WebApp;
			LogQueue.StartQueueTask();
			var taskList = TaskProvider.GetTaskConfigList();
			Manager = new TaskManager(taskList);
			Manager.Start();
		}

		public static void Stop()
		{
			if (Manager != null)
			{
				Manager.Stop();
			}
		}
	}
}
