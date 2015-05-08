// // -----------------------------------------------------------------------
// <copyright file="Class1.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;

namespace EntityRepository.ODataServer.Routing
{

	/// <summary>
	/// Replaces <see cref="MetadataRoutingConvention"/> to use <see cref="EntityRepositoryMetadataController"/> instead.
	/// </summary>
	public sealed class EntityRepositoryMetadataRoutingConvention : IODataRoutingConvention
	{

		/// <summary>
		/// Selects the controller for OData requests.
		/// </summary>
		/// <param name="odataPath">The OData path.</param>
		/// <param name="request">The request.</param>
		/// <returns>
		///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
		/// </returns>
		public string SelectController(ODataPath odataPath, HttpRequestMessage request)
		{
			Contract.Requires<ArgumentNullException>(odataPath != null);
			Contract.Requires<ArgumentNullException>(request != null);

			if (odataPath.PathTemplate == "~" ||
			    odataPath.PathTemplate == "~/$metadata")
			{
				return "EntityRepositoryMetadata";
			}

			return null;
		}

		/// <summary>
		/// Selects the action for OData requests.
		/// </summary>
		/// <param name="odataPath">The OData path.</param>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="actionMap">The action map.</param>
		/// <returns>
		///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
		/// </returns>
		public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
		{
			Contract.Requires<ArgumentNullException>(odataPath != null);
			Contract.Requires<ArgumentNullException>(controllerContext != null);
			Contract.Requires<ArgumentNullException>(actionMap != null);

			if (odataPath.PathTemplate == "~")
			{
				return "GetServiceDocument";
			}

			if (odataPath.PathTemplate == "~/$metadata")
			{
				return "GetMetadata";
			}

			return null;
		}

	}

}