using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCCYAPI.Core
{
    public class Commons
    {
        public static string StoreId { get; } = System.Configuration.ConfigurationManager.AppSettings["WCStorerId"];

        public static string WarehouseId { get; } = System.Configuration.ConfigurationManager.AppSettings["WCWarehouseId"];
        public static string HSMES { get; } = System.Configuration.ConfigurationManager.AppSettings["HSMES"];
        public static string KMMES { get; } = System.Configuration.ConfigurationManager.AppSettings["KMMES"];

        public static string GetOrigSystem()
        {
            var reqUrl = HttpContext.Current.Request.UserHostAddress;
            if (reqUrl.Contains(HSMES))
            {
                return "HSMES";
            }
            else if (reqUrl.Contains(KMMES))
            {
                return "KMMES";
            }
            else
            {
                return "Unknow";
            }
        }
    }
}