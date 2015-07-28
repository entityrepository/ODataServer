// // -----------------------------------------------------------------------
// <copyright file="OwinStartup.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Linq;
using System.Web.Http;
using EntityRepository.ODataServer.Ioc;
using LogJam.Config;
using LogJam.Trace;
using LogJam.Trace.Config;
using LogJam.Trace.Switches;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace Scrum.WebApi.IntegrationTests
{

	/// <summary>
	/// The Scrum example does not currently use OWIN (it assumes IIS or IIS Express).
	/// However, if Owin were being used (as is the case in Scrum.WebApi.IntegrationTests), this is the config that would be needed.
	/// </summary>
	public sealed class OwinStartup
	{

		/// <summary>
		/// Configure all the OWIN modules that participate in each request.
		/// </summary>
		/// <param name="app">The OWIN appBuilder</param>
		/// <remarks>
		/// The order of configuration is IMPORTANT - it sets the sequence of the request processing pipeline.
		/// </remarks>
		public void Configuration(IAppBuilder app)
		{
#if DEBUG
			// Debug builds get full call stack in error pages.
			app.Properties["host.AppMode"] = "development";
#else
			app.Properties["host.AppMode"] = "release";
#endif

			// DependencyInjection config
			var diContainer = new Container(new ContainerOptions() { AllowOverridingRegistrations = true });
			diContainer.RegisterModules(new ODataServiceModule(), new AppModule());

			// Configure logging
			ConfigureOwinLogging(app, diContainer);

			// Web API config
			var webApiConfig = new HttpConfiguration();
			webApiConfig.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(diContainer);

			// Map routes using class attributes
			webApiConfig.MapHttpAttributeRoutes();

			HttpServer webApiServer = new HttpServer(webApiConfig);
			WebApiConfig.ConfigureODataService(webApiConfig, webApiServer);

			app.UseWebApi(webApiServer);
		}

		private void ConfigureOwinLogging(IAppBuilder owinAppBuilder, Container diContainer)
		{
			owinAppBuilder.UseOwinTracerLogging();

			diContainer.RegisterSingle(owinAppBuilder.GetTracerFactory());

			ILogWriterConfig[] configuredLogWriters = owinAppBuilder.GetLogManagerConfig().Writers.ToArray();
			owinAppBuilder.TraceTo(configuredLogWriters);
			owinAppBuilder.LogHttpRequests(configuredLogWriters);
			owinAppBuilder.TraceExceptions(logFirstChance: false, logUnhandled: true);
		}

	}

}