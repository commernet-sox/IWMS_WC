using System;
using System.IO;
using System.Web.Http;
using FWCore;
using iWMS.APILog.FileLogger;
using Microsoft.Practices.Unity;
using NLog;
using iWMS.Core;
using iWMS.WebApplication;
using System.Reflection;

namespace iWMS.BlueSkyAPI
{
	public class Global : System.Web.HttpApplication
	{
		private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		protected void Application_Start(object sender, EventArgs e)
		{
			UniversalApplication.Start(this, Assembly.GetExecutingAssembly());
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{
			UniversalApplication.Error(this);
		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{
			UniversalApplication.End();
		}
	}
}