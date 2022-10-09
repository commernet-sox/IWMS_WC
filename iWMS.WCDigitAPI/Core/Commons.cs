using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iWMS.WCDigitAPI.Core
{
    public class Commons
    {
        public static string StoreId { get; } = System.Configuration.ConfigurationManager.AppSettings["WCStorerId"];

        public static string WarehouseId { get; } = System.Configuration.ConfigurationManager.AppSettings["WCWarehouseId"];
    }
}