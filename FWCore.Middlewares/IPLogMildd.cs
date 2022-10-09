using FWCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FWCore.Middlewares
{
    /// <summary>
    /// 中间件
    /// 记录IP请求数据
    /// </summary>
    public class IPLogMildd
    {
        /// <summary>
        ///
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        ///
        /// </summary>
        /// <param name="next"></param>
        public IPLogMildd(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (GlobalContext.MiddlewareIpLogEnabled)
            {
                // 过滤，只有接口
                if (context.Request.Path.Value != null && 
                    (context.Request.Path.Value.Contains("api",StringComparison.OrdinalIgnoreCase)))
                {
                    context.Request.EnableBuffering();
                    try
                    {
                        // 存储请求数据
                        var dt = DateTime.Now;
                        var request = context.Request;
                        var requestInfo = JsonConvert.SerializeObject(new RequestInfo()
                        {
                            Ip = GetClientIp(context),
                            Url = request.Path.ConvertString().TrimEnd('/').ToLower(),
                            Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss"),
                            Date = dt.ToString("yyyy-MM-dd"),
                            Week = CommonHelper.GetWeek(),
                        });

                        if (!string.IsNullOrEmpty(requestInfo))
                        {
                            Parallel.For(0, 1, e =>
                            {
                                LogLockHelper.OutSql2Log("RequestIpInfoLog", "RequestIpInfoLog" + dt.ToString("yyyy-MM-dd-HH"), new string[] { requestInfo + "," }, false);
                            });
                            request.Body.Position = 0;
                        }
                        await _next(context);
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }

        public static string GetClientIp(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].ConvertString();
            if (string.IsNullOrEmpty(ip))
            {
                if (context.Connection.RemoteIpAddress != null)
                {
                    ip = context.Connection.RemoteIpAddress.MapToIPv4().ConvertString();
                }
            }
            return ip;
        }

    }
}
