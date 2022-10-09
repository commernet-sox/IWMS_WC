using Common.DBCore;
using FWCore;
using FWCore.TaskManage;
using iWMS.TimerTaskService.Core.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    /// <summary>
    /// 库存检查
    /// </summary>
    public class StockCheckTask : BaseTask
    {

        protected override bool Execute(TaskConfig config)
        {
          
            return true;
        }

        
    }
}
