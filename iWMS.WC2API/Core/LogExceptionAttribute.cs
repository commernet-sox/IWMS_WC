using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FWCore;
using System.Web.Http;
using iWMS.WebApplication;
using Newtonsoft.Json;

namespace iWMS.WC2API
{
	public class LogExceptionAttribute : UniversalExceptionAttribute
	{
		protected override string CreateErrorResponse(Exception exception)
		{
			var response = JsonConvert.SerializeObject(new Outcome(500, exception.ConvertString()).Convert(HttpContext.Current.GetLogTag()), GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			return response;
		}
	}

	public class ValidatorAttribute : UniversalValidatorAttribute
	{
		protected override string CreateResponse(Outcome oc)
		{
			var response = JsonConvert.SerializeObject(oc.Convert(HttpContext.Current.GetLogTag()), GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			return response;
		}
	}
}