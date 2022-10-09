using FWCore.TaskManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCDigitService.Core.Task
{
    public class CommonBaseTask : BaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            throw new NotImplementedException();
        }

        private void WriteLog(TaskConfig config,string orderId)
        {

        }
    }
}
