using System;
using EntityRepository.ODataServer;
using EntityRepository.ODataServer.EF;
using Microsoft.Data.Edm;
using ODataBreezejsSample.Models;
using System.Web.Http;
using System.Web.Http.OData.Batch;

namespace ODataBreezejsSample
{
    public static class EntityRepositoryConfig
    {

		internal static void ConfigureODataService(HttpServer server)
		{
			// Configure OData controllers
			var oDataServerConfigurer = new ODataServerConfigurer(server.Configuration);

			oDataServerConfigurer.AddStandardEntitySetControllers();
			oDataServerConfigurer.ConfigureODataRoutes(server.Configuration.Routes, "ODataRoute", "odata", server);
		}

    }
}
