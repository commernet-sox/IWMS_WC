using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;

namespace iWMS.TimerTaskService.Core.Util
{
    public class ConfigUtil
    {
        public static int WebPort
        {
            get
            {
                if (!ConfigurationManager.AppSettings.AllKeys.Contains("WebPort"))
                {
                    throw new Exception("找不到应用程序配置节点WebPort！");
                }
                return ConfigurationManager.AppSettings["WebPort"].Trim().ConvertInt32();
            }
        }

        public static int StockBackupKeepDays
        {
            get
            {
                if (!ConfigurationManager.AppSettings.AllKeys.Contains("StockBackupKeepDays"))
                {
                    return 180;
                }
                return ConfigurationManager.AppSettings["StockBackupKeepDays"].Trim().ConvertInt32();
            }
        }
    }
}
