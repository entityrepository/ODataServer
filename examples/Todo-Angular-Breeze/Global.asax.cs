using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EntityRepository.ODataServer.Ioc;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace ODataBreezejsSample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			// MVC
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

			// Web API is configured in OWIN (see Startup.cs)
        }
    }
}
