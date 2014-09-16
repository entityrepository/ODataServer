// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using EntityRepository.ODataServer.Ioc;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Scrum.WebApi
{

	public class MvcApplication : HttpApplication
	{

		protected void Application_Start()
		{
			// ASP.NET MVC setup
			AreaRegistration.RegisterAllAreas();

			// DI config
			var container = new Container(new ContainerOptions() { AllowOverridingRegistrations = true });
			container.RegisterModules(new ODataServiceModule(), new AppModule());

			// Web API config
			GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			//MvcRouteConfig.RegisterRoutes(RouteTable.Routes);
		}

	}
}
