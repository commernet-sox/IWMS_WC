using FWCore;
using FWCore.TaskManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCDigitService.Core
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
            GlobalContext.ServiceMode = ServiceMode.Server;
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
