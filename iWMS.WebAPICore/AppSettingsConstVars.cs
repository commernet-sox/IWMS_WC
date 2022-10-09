using FWCore;
using SqlSugar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WebAPICore
{
    /// <summary>
    /// 配置文件格式化
    /// </summary>
    public class AppSettingsConstVars
    {
        #region 数据库================================================================================
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static readonly string BusConnectString = AppSettingsHelper.GetContent("ConnectionStrings", "BusConnectString");
        #endregion

        #region 接口

        public static readonly string CustomerId = AppSettingsHelper.GetContent("InterfaceOptions", "CustomerId");
        public static readonly string AppKey = AppSettingsHelper.GetContent("InterfaceOptions", "AppKey");
        public static readonly string AppSecret = AppSettingsHelper.GetContent("InterfaceOptions", "AppSecret");
        public static readonly string PostUrl = AppSettingsHelper.GetContent("InterfaceOptions", "PostUrl");
        public static readonly string ReturnUrl = AppSettingsHelper.GetContent("InterfaceOptions", "ReturnUrl");

        #endregion

        #region 中间件
        /// <summary>
        /// Ip限流
        /// </summary>
        public static readonly bool MiddlewareIpLogEnabled = AppSettingsHelper.GetContent("Middleware", "IPLog", "Enabled").ObjToBool();
        /// <summary>
        /// 记录请求与返回数据
        /// </summary>
        public static readonly bool MiddlewareRequestResponseLogEnabled = AppSettingsHelper.GetContent("Middleware", "RequestResponseLog", "Enabled").ObjToBool();
        /// <summary>
        /// 用户访问记录-是否开启
        /// </summary>
        public static readonly bool MiddlewareRecordAccessLogsEnabled = AppSettingsHelper.GetContent("Middleware", "RecordAccessLogs", "Enabled").ObjToBool();
        /// <summary>
        /// 用户访问记录-过滤ip
        /// </summary>
        public static readonly string MiddlewareRecordAccessLogsIgnoreApis = AppSettingsHelper.GetContent("Middleware", "RecordAccessLogs", "IgnoreApis");
        #endregion
    }
}
