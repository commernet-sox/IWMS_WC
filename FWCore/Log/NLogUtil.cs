using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FWCore
{
    public static class NLogUtil
    {
        private static readonly Logger DbLogger = LogManager.GetLogger("logdb");
        private static readonly Logger FileLogger = LogManager.GetLogger("logfile");

        /// <summary>
        /// 同时写入到日志到数据库和文件
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="logType">日志类型</param>
        /// <param name="logTitle">标题（255字符）</param>
        /// <param name="message">信息</param>
        /// <param name="exception">异常</param>
        public static void WriteAll(LogLevel logLevel, LogType logType, string logTitle, string message, Exception exception = null)
        {
            //先存文件
            WriteFileLog(logLevel, logType, logTitle, message, exception);
            //后存数据库
            WriteDbLog(logLevel, logType, logTitle, message, exception);
        }

        /// <summary>
        /// 写日志到数据库
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="logType">日志类型</param>
        /// <param name="logTitle">标题（255字符）</param>
        /// <param name="message">信息</param>
        /// <param name="exception">异常</param>
        public static void WriteDbLog(LogLevel logLevel, LogType logType, string logTitle, string message, Exception exception = null)
        {
            LogEventInfo theEvent = new LogEventInfo(logLevel, DbLogger.Name, message);
            theEvent.Properties["LogType"] = logType.ToString();
            theEvent.Properties["LogTitle"] = logTitle;
            theEvent.Exception = exception;
            DbLogger.Log(theEvent);
        }

        /// <summary>
        /// 写日志到文件
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="logType">日志类型</param>
        /// <param name="logTitle">标题（255字符）</param>
        /// <param name="message">信息</param>
        /// <param name="exception">异常</param>
        public static void WriteFileLog(LogLevel logLevel, LogType logType, string logTitle, string message, Exception exception = null)
        {
            LogEventInfo theEvent = new LogEventInfo(logLevel, FileLogger.Name, message);
            theEvent.Properties["LogType"] = logType.ToString();
            theEvent.Properties["LogTitle"] = logTitle;
            theEvent.Exception = exception;

            FileLogger.Log(theEvent);
        }

        /// <summary>
        /// 确保NLog配置文件sql连接字符串正确
        /// </summary>
        /// <param name="nlogPath"></param>
        public static void EnsureNlogConfig(string nlogPath)
        {
           
        }
    }
}
