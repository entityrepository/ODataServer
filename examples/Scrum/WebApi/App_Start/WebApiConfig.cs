// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using EntityRepository.ODataServer;
using EntityRepository.ODataServer.Util;
using Scrum.Dal;
using Scrum.Model;
using Scrum.Model.Base;
using Scrum.WebApi.Controllers;
using System;
using System.Web.Http;
//using System.Web.Http.OData.Batch;

namespace Scrum.WebApi
{
	public static class WebApiConfig
	{

		internal const string ODataRoute = "odata";

		public static void Register(HttpConfiguration config)
		{
			// REVIEW: Batch is only supported in newer web api odata
			// Configure OData $batch
			//config.Routes.MapHttpBatchRoute("ODataBatchRoute", ODataRoute + "/$batch", new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer));

			// Configure OData controllers
			var oDataServiceConfig = new ODataServiceConfig(config);

			// TODO: Remove these after testing
			//oDataServiceConfig.AddEntitySetController("Projects", typeof(Project), typeof(ProjectsController));
			oDataServiceConfig.AddEntitySetController("Users", typeof(User), typeof(UsersController));

			oDataServiceConfig.AddDbContextControllers<ScrumDb>(DbSetControllerSelector);

			config.Routes.MapODataRoute("ODataRoute", ODataRoute, oDataServiceConfig.BuildEdmModel(typeof(WebApiConfig).Namespace));
		}

		/// <summary>
		/// For each entity, entity key, and DbContext type combination, determine the type of the controller
		/// to create for the entity set.
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="keyType"></param>
		/// <param name="dbContextType"></param>
		/// <returns></returns>
		private static Type DbSetControllerSelector(Type entityType, Type keyType, Type dbContextType)
		{
			if (entityType.IsDerivedFromGenericType(typeof(NamedDbEnum<,>)))
			{
				// DbEnum -> ReadOnlyDbSetController
				return typeof(ReadOnlyDbSetController<,,>).MakeGenericType(entityType, keyType, dbContextType);
			}
			else
			{
				// Everything else -> EditDbSetController
				return typeof(EditDbSetController<,,>).MakeGenericType(entityType, keyType, dbContextType);
			}
		}

	}
}
