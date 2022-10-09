using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWCore
{
    public enum LogType
    {
        [Description("网站")]
        Web,
        [Description("数据库")]
        DataBase,
        [Description("Api接口")]
        ApiRequest,
        [Description("中间件")]
        Middleware,
        [Description("其他")]
        Other,
        [Description("Swagger")]
        Swagger,
        [Description("定时任务")]
        Task,
        [Description("订单")]
        Order,
        [Description("订单退款")]
        Refund,
        [Description("退款结果通知")]
        RefundResultNotification,
        [Description("Redis消息队列")]
        RedisMessageQueue,
        [Description("微信推送消息")]
        WxPost,
    }
}
