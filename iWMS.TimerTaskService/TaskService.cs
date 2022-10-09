using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using FWCore;
using iWMS.TimerTaskService.Core;

namespace iWMS.TimerTaskService
{
    public partial class TaskService : ServiceBase
    {
        public TaskService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                TaskRunner.Start();

                NLogUtil.WriteError("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: 定时应用服务已启动");
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
        }

        protected override void OnStop()
        {
            try
            {
                TaskRunner.Stop();

                NLogUtil.WriteError("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: 定时应用服务已停止");
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
        }
    }
}
