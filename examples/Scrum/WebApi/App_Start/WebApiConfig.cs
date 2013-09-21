// -----------------------------------------------------------------------
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

namespace Scrum.WebApi
{
	public static class WebApiConfig
	{

		internal const string ODataRoute = "odata";

		public static void Register(HttpConfiguration config)
		{
			// Pull the container metadata from the DI service
			var containerMetadata = config.DependencyResolver.Resolve<IContainerMetadata<ScrumDb>>();

			// Configure OData controllers
			var oDataServiceManager = new ODataServerConfigurer(config, containerMetadata);

			// Just to prove that regular controller classes can be added when customization is needed
			//oDataServiceConfig.AddEntitySetController("Projects", typeof(Project), typeof(ProjectsController));
			//oDataServiceManager.AddEntitySetController("Users", typeof(User), typeof(UsersController));

			oDataServiceManager.AddStandardEntitySetControllers(DbSetControllerSelector);
			oDataServiceManager.ConfigureODataRoutes(config.Routes, "ODataRoute", ODataRoute, GlobalConfiguration.DefaultServer);
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
				throw new ArgumentException("No default controller exists that supports multiple keys.");
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
