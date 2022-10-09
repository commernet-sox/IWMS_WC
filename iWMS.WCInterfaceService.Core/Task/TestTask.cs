using Common.DBCore;
using FWCore;
using FWCore.TaskManage;
using iWMS.WCInterfaceService.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCInterfaceService.Core.Task
{
	/// <summary>
	/// 测试任务
	/// </summary>
	public class TestTask : CommonBaseTask
	{

		protected override bool Execute(TaskConfig config)
		{

			var oc = ApiFactory.Create("xx").WithBody(new TestRequest() { Id = "idx" }).LogResult();
			Console.WriteLine(DateTime.Now);
			return true;
		}


	}
}
