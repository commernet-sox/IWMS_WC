using System;
using System.Reflection;
using System.Web.Http;
using iWMS.WebApplication;

namespace iWMS.WC2API
{
	public class Global : System.Web.HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			UniversalApplication.Start(this, Assembly.GetExecutingAssembly(), false);
			GlobalConfiguration.Configure(c =>
			{
				c.Filters.Add(new LogExceptionAttribute());
				c.Filters.Add(new ValidatorAttribute());
				c.Filters.Add(new UniversalLogAttribute());
			});
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