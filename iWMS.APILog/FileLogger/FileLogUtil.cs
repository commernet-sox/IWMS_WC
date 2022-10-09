using System.IO;
using System.Text;
using FWCore;

namespace iWMS.APILog.FileLogger
{
	public class FileLogUtil
	{
		static FileLogUtil()
		{
			FileLogUtil.SetBaseDirectory(ConfigurationUtil.GetAppSettingString("LogDir"));
		}

		private static string _baseDir;

		/// <summary>
		/// 设置写日志的默认目录
		/// </summary>
		/// <param name="baseDir"></param>
		/// <returns></returns>
		public static void SetBaseDirectory(string baseDir)
		{
			_baseDir = baseDir;
		}

		public static void Write(string relativePath, string message)
		{
			var path = Path.Combine(_baseDir, GlobalContext.ServerTime.ToString("yyMMdd"), relativePath);
			WriteFile(path, message);
		}

		public static void WriteFile(string filePath, string message)
		{
			LogQueue.Write(filePath, message);
		}

		public static void WriteMessage(string path, string url, string request, string response, string ip = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine("time:" + GlobalContext.ServerTime.ToString("yyyy-MM-dd HH:mm:ss fffff"));
			sb.AppendLine("url:" + url);
			if (!string.IsNullOrWhiteSpace(ip))
			{
				sb.AppendLine("ip:" + ip);
			}

			sb.AppendLine();
			sb.AppendLine("request:" + request);
			sb.AppendLine();
			sb.AppendLine("response:" + response);
			Write(path, sb.ToString());
		}
	}
}
