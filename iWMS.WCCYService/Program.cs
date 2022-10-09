using System;
using System.ServiceProcess;
using FWCore;
using iWMS.APILog;
using iWMS.WCCYService.Core;

namespace iWMS.WCCYService
{
	static class Program
	{
		private const string ServiceName = "潍柴储运工厂接口回传服务";
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				NLogUtil.WriteError(e.ExceptionObject.ConvertString());
			};
			if (!Environment.UserInteractive)
			{
				RunAsService();
				return;
			}
			string exeArg = string.Empty;
			if (args == null || args.Length < 1)
			{
				Console.WriteLine(ServiceName);

				Console.WriteLine("请根据提示操作...");
				Console.WriteLine("-[i]: 按i安装Windows服务");
				Console.WriteLine("-[u]: 按u卸载Windows服务");
				Console.WriteLine("-[r]: 按r在控制台上运行（开发调试用）");

				while (true)
				{
					exeArg = Console.ReadKey().KeyChar.ToString();
					Console.WriteLine();

					if (Run(exeArg))
					{
						break;
					}
				}
			}
			else
			{
				exeArg = args[0];
				if (!string.IsNullOrWhiteSpace(exeArg))
				{
					exeArg = exeArg.TrimStart('-');
				}
				Run(exeArg);
			}
		}

		private static bool Run(string exeArg)
		{
			switch (exeArg.ToLower())
			{
				case "i":
					SystemInstaller.Install();
					break;
				case "u":
					SystemInstaller.UnInstall();
					break;
				case "r":
					RunAsConsole();
					break;
				default:
					Console.WriteLine("无效输入！");
					break;
			}
			return true;
		}

		private static void RunAsConsole()
		{
			Console.WriteLine(ServiceName + "正在运行...");
			TaskRunner.Start();

			Console.WriteLine("按回车关闭服务！");
			Console.ReadLine();
		}

		private static void RunAsService()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new TaskService()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
