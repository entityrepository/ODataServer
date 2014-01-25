// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using EntityRepository.ODataServer.Autofac;

namespace Scrum.WebApi
{

	public class MvcApplication : HttpApplication
	{

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			AutofacConfiguration.Configure(GlobalConfiguration.Configuration, new AutofacAppModule());

			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			//MvcRouteConfig.RegisterRoutes(RouteTable.Routes);
		}

	}
}
