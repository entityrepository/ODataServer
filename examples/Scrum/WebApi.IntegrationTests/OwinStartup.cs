// // -----------------------------------------------------------------------
// <copyright file="OwinStartup.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Web.Http;
using EntityRepository.ODataServer.Ioc;
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
			WebApiConfig.Register(webApiConfig);
			//WebApiConfig.ConfigureODataService(webApiConfig);
			app.UseWebApi(webApiConfig);
		}

		private void ConfigureOwinLogging(IAppBuilder owinAppBuilder, Container diContainer)
		{
			owinAppBuilder.UseTracerLogging();
			diContainer.RegisterSingle(owinAppBuilder.GetTracerFactory());

			owinAppBuilder.LogHttpRequests(logRequestBodies: true, logResponseBodies: false /* Log response bodies HANGS WITH Microsoft.Owin.Testing.ResponseStream */);

			owinAppBuilder.TraceExceptions(logFirstChance: false, logUnhandled: true);
		}

	}

}