using FWCore;
using FWCore.TaskManage;
using iWMS.WCInterfaceService.Core.Task;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core
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

			if (!(ConfigurationManager.GetSection(funity) is ConfigurationSection cs))
			{
				return;
			}

			var section = cs as UnityConfigurationSection;

            section.Configure(mainContainer, "Core.Contianer");
            GlobalContext.AppContainer = mainContainer;

            GlobalContext.ServiceMode = ServiceMode.Server;
            BllUnityWrapper.InitType = UnityInitType.WebApp;
            LogQueue.StartQueueTask();
            //本地跑
            var taskList = TaskProvider.GetTaskConfigList();
            Manager = new TaskManager(taskList);
            Manager.Start();

            //获取指定日期的生产计划
            //var task1 = new MESProducePlanTask();
            //task1.TryExecute(new TaskConfig());

            //获取BOM
            //var task2 = new MESBOMTask();
            //task2.TryExecute(new TaskConfig());

            //WCP2PTask T1 = new WCP2PTask();
            //T1.TryExecute(new TaskConfig());
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
