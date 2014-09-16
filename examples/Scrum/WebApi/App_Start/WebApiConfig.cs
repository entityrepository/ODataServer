﻿// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Http.OData.Batch;
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

		private static void ConfigureODataService(HttpConfiguration config)
		{
			// Pull the container metadata from the DI service
			var scrumDbContainer = config.DependencyResolver.Resolve<IContainerMetadata<ScrumDb>>();
			if (scrumDbContainer == null)
			{
				throw new ArgumentException("IContainerMetadata<ScrumDb> could not be resolved from HttpConfiguration.DependencyResolver.");
			}

			// Configure OData controllers
			// NOTE: The use of MultiContainerMetadata is unnecessary - could just be scrumDbContainer without the wrapper.
			// The only reason to use MultiContainerMetadata here is to test it.
			var oDataServerConfigurer = new ODataServerConfigurer(config, new MultiContainerMetadata<ODataContainer>(scrumDbContainer));

			// Just to prove that regular controller classes can be added when customization is needed
			//oDataServerConfigurer.AddEntitySetController("Projects", typeof(Project), typeof(ProjectsController));
			//oDataServerConfigurer.AddEntitySetController("Users", typeof(User), typeof(UsersController));

			oDataServerConfigurer.AddStandardEntitySetControllers(DbSetControllerSelector);
			oDataServerConfigurer.ConfigureODataRoutes(config.Routes, "ODataRoute", ODataRoute, GlobalConfiguration.DefaultServer);
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
