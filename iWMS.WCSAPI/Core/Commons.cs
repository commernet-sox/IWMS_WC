using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCSAPI
{
    public class Commons
    {
        public static string StoreId { get; } = System.Configuration.ConfigurationManager.AppSettings["StorerId"];

        public static string WarehouseId { get; } = System.Configuration.ConfigurationManager.AppSettings["WarehouseId"];
    }
}