using FWCore;
using FWCore.TaskManage;
using iWMS.TimerTaskService.Core.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core
{
    public static class TaskRunner
    {
        public static TaskManager Manager = null;

        public static async void Start()
        {
            if (Manager != null && Manager.Tasks != null && Manager.Tasks.Count > 0)
            {
                throw new Exception("TaskRunner is running,Restart failure!");
            }
            GlobalContext.ServiceMode = ServiceMode.Server;
            //var taskList = TaskProvider.GetTaskConfigList();
            //Manager = new TaskManager(taskList);
            //Manager.Start();

            WC2AuditOrderTask task1 = new WC2AuditOrderTask();
            task1.TryExecute(new TaskConfig());
            

            await WebServerManage.StartAsync();
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
