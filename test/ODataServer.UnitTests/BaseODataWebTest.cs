// // -----------------------------------------------------------------------
// <copyright file="BaseODataWebTest.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using EntityRepository.ODataServer.Ioc;
using LogJam;
using LogJam.Owin.Http;
using LogJam.Trace;
using LogJam.Trace.Config;
using LogJam.Trace.Format;
using LogJam.XUnit2;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Owin;
using Simple.OData.Client;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using Xunit;
using Xunit.Abstractions;

namespace EntityRepository.ODataServer.UnitTests
{

	/// <summary>
	/// Provides common functionality for unit/integration tests that hit an EntityRepository.ODataServer instance..
	/// </summary>
	public class BaseODataWebTest : IDisposable
	{
		private const int c_testTimeout = 20000;

		private readonly ITestOutputHelper _testOutputHelper;
		private readonly CancellationTokenSource _cancellationSource;
		protected readonly CancellationToken CancelTest;

		private LogManager _logManager;
		private ITracerFactory _tracerFactory;

		protected BaseODataWebTest(ITestOutputHelper testOutputHelper)
		{
			Contract.Requires<ArgumentNullException>(testOutputHelper != null);

			_testOutputHelper = testOutputHelper;

			_cancellationSource = new CancellationTokenSource(c_testTimeout);
			CancelTest = _cancellationSource.Token;
		}

		public virtual void Dispose()
		{
			_cancellationSource.Dispose();
		}

		/// <summary>
		/// Returns a new <see cref="TestServer"/>, configured with WebApp.Web startup code.
		/// Requests against the test server are processed directly in memory without going over the network.
		/// </summary>
		/// <param name="iocModule">Configures the odata model for the test in SimpleInjector</param>
		/// <param name="odataConfigAction">Optional configuration for <see cref="ODataServerConfigurer"/>.</param>
		/// <returns></returns>
		/// <remarks>
		/// Note that the returned <see cref="TestServer"/> should be disposed.
		/// </para>
		/// </remarks>
		protected TestServer CreateTestServer(IModule iocModule, Action<ODataServerConfigurer> odataConfigAction = null)
		{

			// Setup the DI container
			Container container = new Container(new ContainerOptions() { AllowOverridingRegistrations = true });
			container.RegisterModules(new ODataServiceModule(), iocModule);

			var testServer = TestServer.Create(owinAppBuilder => ConfigureOwinPipeline(owinAppBuilder, container, odataConfigAction));

			// Ensure logging setup is healthy
			_tracerFactory.TracerFor(this).Info("Test OData server started...");
			Assert.True(_logManager.IsHealthy);

			return testServer;
		}

		/// <summary>
		/// Configures the OWIN pipeline for a test.
		/// </summary>
		/// <param name="owinAppBuilder"></param>
		/// <param name="iocContainer">A SimpleInjector IOC container.</param>
		/// <param name="odataConfigAction">Optional configuration for <see cref="ODataServerConfigurer"/>.</param>
		protected virtual void ConfigureOwinPipeline(IAppBuilder owinAppBuilder, Container iocContainer, Action<ODataServerConfigurer> odataConfigAction = null)
		{
			// Test class name is the app name
			owinAppBuilder.Properties["host.AppName"] = GetType().FullName;
			owinAppBuilder.Properties["EntityRepository.InUnitTest"] = true; // In case any test-conditional logic is needed

			// HTTP and trace logging
			ConfigureLogging(owinAppBuilder, out _logManager, out _tracerFactory);

			// Request filter method, useful breakpoint for debugging
			owinAppBuilder.Use(TestRequestFilter);

			// Add Web API to the Owin pipeline
			HttpConfiguration webApiConfig = new HttpConfiguration();
			//webApiConfig.EnableSystemDiagnosticsTracing();
			// Use SimpleInjector as the web API DependencyResolver
			webApiConfig.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(iocContainer);
			HttpServer webApiServer = new HttpServer(webApiConfig);

			// Configure EntityRepository.ODataServer controllers
			var oDataServerConfigurer = new ODataServerConfigurer(webApiConfig);
			if (odataConfigAction != null)
			{
				odataConfigAction(oDataServerConfigurer);
			}
			oDataServerConfigurer.AddStandardEntitySetControllers();
			oDataServerConfigurer.ConfigureODataRoutes(webApiServer.Configuration.Routes, "ODataRoute", "odata", webApiServer);

			owinAppBuilder.UseWebApi(webApiServer);

			// Verify that DI config is valid; and initialize everything
			iocContainer.Verify();
		}

		/// <summary>
		/// Initialize request and trace logging.
		/// </summary>
		/// <param name="logManager"></param>
		/// <param name="tracerFactory"></param>
		protected virtual void ConfigureLogging(IAppBuilder owinAppBuilder, out LogManager logManager, out ITracerFactory tracerFactory)
		{
			var logWriterConfig = new TestOutputLogWriterConfig(_testOutputHelper);
			logWriterConfig.Format(new TestOutputTraceFormatter()).Format(new HttpRequestFormatter()).Format(new HttpResponseFormatter());

			owinAppBuilder.GetTraceManagerConfig().TraceTo(logWriterConfig);
			owinAppBuilder.GetLogManagerConfig().Writers.Add(logWriterConfig);
			owinAppBuilder.TraceExceptions();
			owinAppBuilder.LogHttpRequests(logWriterConfig);

			tracerFactory = owinAppBuilder.GetTracerFactory();
			logManager = owinAppBuilder.GetLogManager();
		}

		/// <summary>
		/// Can be overridden to inspect or tweak requests; or just a handy place for a breakpoint.
		/// </summary>
		/// <param name="owinContext"></param>
		/// <param name="next"></param>
		/// <returns></returns>
		protected async virtual Task TestRequestFilter(IOwinContext owinContext, Func<Task> next)
		{
			await next();
		}

		/// <summary>
		/// Creates and returns a new <see cref="ODataClient"/> which is configured to talk to <paramref name="testServer"/>.
		/// </summary>
		/// <param name="testServer"></param>
		/// <param name="odataBaseUrl"></param>
		/// <returns></returns>
		protected virtual ODataClient CreateODataClient(TestServer testServer, string odataBaseUrl = "/odata/")
		{
			var baseUrl = new Uri(new Uri("http://localhost/"), odataBaseUrl);
			ODataClientSettings oDataClientSettings = new ODataClientSettings(baseUrl)
			                                          {
				                                          OnCreateMessageHandler = () => testServer.Handler
			                                          };
			return new ODataClient(oDataClientSettings);
		}

	}

}