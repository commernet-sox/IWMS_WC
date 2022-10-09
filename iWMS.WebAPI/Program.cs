using FWCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iWMS.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            try
            {
                //确保NLog.config中连接字符串与appsettings.json中同步
                NLogUtil.EnsureNlogConfig("NLog.config");
                //其他项目启动时需要做的事情
                NLogUtil.WriteAll(NLog.LogLevel.Trace, LogType.Web, "接口启动", "接口启动成功");

                host.Run();
            }
            catch (Exception ex)
            {
                //使用nlog写到本地日志文件（万一数据库没创建/连接成功）
                NLogUtil.WriteFileLog(NLog.LogLevel.Error, LogType.ApiRequest, "接口启动", "初始化数据异常", ex);
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); //移除已经注册的其他日志处理程序
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace); //设置最小的日志级别
                })
                .UseNLog() //NLog: Setup NLog for Dependency injection
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.AllowSynchronousIO = true; //启用同步 IO
                        })
                        .UseStartup<Startup>();
                });
        }
    }
}
