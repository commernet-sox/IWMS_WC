using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using iWMS.WebApplication;

namespace iWMS.WCSAPI
{
	public class Global : System.Web.HttpApplication
	{
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