﻿// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using EntityRepository.ODataServer;
using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.Util;
using Scrum.Dal;
using Scrum.Model.Base;
using System;
using System.Web.Http;
using Scrum.WebApi.Models;

namespace Scrum.WebApi
{
	public static class WebApiConfig
	{

		internal const string ODataRoute = "odata";

		public static void Register(HttpConfiguration config)
		{
#if DEBUG
			config.EnableSystemDiagnosticsTracing();
#endif

			// Ensures that this works with other attribute API routes
			config.MapHttpAttributeRoutes();

			ConfigureODataService(config);
		}

		internal static void ConfigureODataService(HttpConfiguration config)
		{
			// Configure OData controllers
			var oDataServerConfigurer = new ODataServerConfigurer(config);

			// Just to prove that regular controller classes can be added when customization is needed
			// However, this isn't needed, b/c the dependency injector normally picks up all controllers in the assembly.
			//oDataServerConfigurer.AddEntitySetController("Projects", typeof(Project), typeof(ProjectsController));
			//oDataServerConfigurer.AddEntitySetController("Users", typeof(User), typeof(UsersController));

			oDataServerConfigurer.AddStandardEntitySetControllers(DbSetControllerSelector);

			//// TODO: Remove this - using to compare ODataConventionModelBuilder's EDM to what EF creates.
			//var odataModelBuilder = new ODataConventionModelBuilder(config);
			//odataModelBuilder.ConfigureFromContainer(oDataServerConfigurer.ContainerMetadata);

			oDataServerConfigurer.ConfigureODataRoutes(config.Routes, "ODataRoute", ODataRoute, GlobalConfiguration.DefaultServer);
				// TODO: Remove this arg
				//odataModelBuilder.GetEdmModel());
		}

		/// <summary>
		/// For each entity, entity key, and DbContext type combination, determine the type of the controller
		/// to create for the entity set.
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="keyTypes"></param>
		/// <param name="dbContextType"></param>
		/// <returns></returns>
		private static Type DbSetControllerSelector(Type entityType, Type[] keyTypes, Type dbContextType)
		{
			if (keyTypes.Length != 1)
			{
				// Hide the entity type by returning null
				return null;
				// throw new ArgumentException("No default controller exists that supports multiple keys.");
			}

			if (entityType.IsDerivedFromGenericType(typeof(NamedDbEnum<,>)))
			{
				// DbEnum -> ReadOnlyDbSetController
				return typeof(ReadOnlyDbSetController<,,>).MakeGenericType(entityType, keyTypes[0], dbContextType);
			}
			else
			{
				// Everything else -> EditDbSetController
				return typeof(EditDbSetController<,,>).MakeGenericType(entityType, keyTypes[0], dbContextType);
			}
		}

	}

}
