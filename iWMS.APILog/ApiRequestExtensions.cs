using System;
using System.IO;
using FWCore;
using iWMS.APILog.FileLogger;

namespace iWMS.Core.Http
{
	public static class ApiRequestExtensions
	{
		private const string LogId = "logId";
		private const string LogTag = "logTag";
		private const string LogCode = "logCode";

		private static string CreateLogId()
		{
			return Guid.NewGuid().ToString("N");
		}

		public static string GetLogId(this ApiRequest api)
		{
			var logId = string.Empty;

			if (api.Items.Contains(LogId))
			{
				logId = api.Items[LogId].ConvertString();
			}

			if (string.IsNullOrWhiteSpace(logId))
			{
				logId = CreateLogId();
			}

			return logId;
		}

		public static ApiRequest SetLogId(this ApiRequest api, string logId)
		{
			if (string.IsNullOrWhiteSpace(logId))
			{
				logId = CreateLogId();
			}

			api.Items[LogId] = logId;
			return api;
		}

		public static ApiRequest SetLogCode(this ApiRequest api, string code)
		{
			api.Items[LogCode] = code;
			return api;
		}

		public static string GetLogCode(this ApiRequest api)
		{
			if (api.Items.Contains(LogCode))
			{
				return api.Items[LogCode].ConvertString();
			}

			return string.Empty;
		}

		public static ApiRequest SetLogTag(this ApiRequest api, string tag)
		{
			tag = Path.GetFileName(tag);
			api.Items[LogTag] = tag;
			return api;
		}

		public static string GetLogTag(this ApiRequest api)
		{
			if (api.Items.Contains(LogTag))
			{
				return api.Items[LogTag].ConvertString().ToLowerInvariant();
			}

			return string.Empty;
		}

		public static void WriteMessage(this ApiRequest api, string response)
		{
			var path = string.Concat(api.GetLogTag(), "_", api.GetLogId(), "_", api.GetLogCode(), ".log");
			var url = api.Request.RequestUri.ToString();
			FileLogUtil.WriteMessage(path, url, api.Body, response);
		}

	}
}
