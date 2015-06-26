using System.Web.Http;
using EntityRepository.ODataServer.Ioc;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

[assembly: OwinStartupAttribute(typeof(ODataBreezejsSample.Startup))]
namespace ODataBreezejsSample
{
    public partial class Startup
    {

		private Container _container = new Container(new ContainerOptions() { AllowOverridingRegistrations = true });

		public void Configuration(IAppBuilder owinAppBuilder)
        {
			// DependencyInjection config
			_container.RegisterModules(new ODataServiceModule(), new IocConfig());

			// Add Web API to the Owin pipeline
			HttpConfiguration webApiConfig = new HttpConfiguration();
#if DEBUG
			webApiConfig.EnableSystemDiagnosticsTracing();
#endif
			webApiConfig.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(_container);
			HttpServer webApiServer = new HttpServer(webApiConfig);
			EntityRepositoryConfig.ConfigureODataService(webApiServer);

			// Map routes using class attributes
			webApiConfig.MapHttpAttributeRoutes();

			owinAppBuilder.UseWebApi(webApiServer);

			// Verify that DI config is valid; and initialize everything
			_container.Verify();

        }
    }
}
