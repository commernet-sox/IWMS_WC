using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWCore
{
    /// <summary>
    /// 获取Appsettings配置信息
    /// </summary>
    public class AppSettingsHelper
    {
        static IConfiguration Configuration { get; set; }

        //public AppSettingsHelper(string contentPath)
        //{
        //    string Path = "appsettings.json";
        //    Configuration = new ConfigurationBuilder().SetBasePath(contentPath).Add(new JsonConfigurationSource { Path = Path, Optional = false, ReloadOnChange = true }).Build();
        //}

        public AppSettingsHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 封装要操作的字符
        /// AppSettingsHelper.GetContent(new string[] { "JwtConfig", "SecretKey" });
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string GetContent(params string[] sections)
        {
            try
            {

                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";
        }

    }
}
