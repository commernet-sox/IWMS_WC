using FWCore.TaskManage;
using iWMS.APILog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
	public abstract class CommonBaseTask : BaseTask
	{
		public override void TryExecute(TaskConfig config)
		{
			base.TryExecute(config);
			if (!string.IsNullOrWhiteSpace(config.LastRunMessage))
			{
				NLogUtil.WriteError(config.LastRunMessage);
			}
		}
	}
}
