using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WCCYService.Core.Util
{
    public class Commons
    {
        public static string StoreId { get; } = System.Configuration.ConfigurationManager.AppSettings["WCStorerId"];

        public static string WarehouseId { get; } = System.Configuration.ConfigurationManager.AppSettings["WCWarehouseId"];
    }
}
