using iWMS.TimerTaskService.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.TimerTaskService.Core
{
   public class WebServerManage
    {
        public static async System.Threading.Tasks.Task StartAsync()
        {
            try
            {
                if (ConfigUtil.WebPort <= 0)
                    return;

                var httpListener = new HttpListener();
                httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                httpListener.Prefixes.Add($"http://*:{ConfigUtil.WebPort}/");
                httpListener.Start();
                await System.Threading.Tasks.Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var httpListenerContext = httpListener.GetContext();
                            httpListenerContext.Response.StatusCode = 200;
                            var query = httpListenerContext.Request.QueryString;
                            var type = query["type"];
                            httpListenerContext.Response.ContentType = "text/plain;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
                            httpListenerContext.Response.AddHeader("Content-type", "text/plain");//添加响应头信息
                            httpListenerContext.Response.AddHeader("Access-Control-Allow-Origin", "*");

                            using (var writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                            {
                                var text = "failure";

                                if (TaskRunner.Manager != null && TaskRunner.Manager.Tasks != null)
                                {
                                    if (TaskRunner.Manager.TaskDict != null)
                                    {
                                        foreach (var pair in TaskRunner.Manager.TaskDict)
                                        {
                                            if (pair.Key.TaskId.Equals(type, StringComparison.OrdinalIgnoreCase))
                                            {
                                                pair.Value.TryExecute(pair.Key);
                                                text = "success";
                                                break;
                                            }
                                        }
                                    }
                                }
                                writer.WriteLine(text);
                            }
                        }
                        catch (Exception ex)
                        {
                            NLogUtil.WriteError(ex.ToString());
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
            }
        }
    }
}
