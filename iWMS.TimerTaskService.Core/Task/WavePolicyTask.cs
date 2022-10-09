using FWCore.TaskManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core.Task
{
    public class WavePolicyTask : BaseTask
    {
        protected override bool Execute(TaskConfig config)
        {
            try
            {
                lock (LockObject)
                {
                    WaveAlgorithm algorithm = new WaveAlgorithm();
                    algorithm.Calc();
                }
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
            return true;
        }
    }
}
