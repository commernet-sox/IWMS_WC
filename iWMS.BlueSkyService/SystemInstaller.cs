using System;
using System.Configuration.Install;
using System.Reflection;
using iWMS.APILog;
using iWMS.BlueSkyService.Core;

namespace iWMS.BlueSkyService
{
	public class SystemInstaller
    {
        private static readonly string exePath = Assembly.GetExecutingAssembly().Location;

        public static bool Install()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { exePath });
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
                return false;
            }
            return true;
        }

        public static bool UnInstall()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", exePath });
            }
            catch (Exception ex)
            {
                NLogUtil.WriteError(ex.ToString());
                return false;
            }
            return true;
        }
    }
}
