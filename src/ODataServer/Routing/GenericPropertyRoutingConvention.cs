// -----------------------------------------------------------------------
// <copyright file="GenericPropertyRoutingConvention.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Routing
{
	/// <summary>
	/// Similar to <see cref="PropertyRoutingConvention"/>, but routes to generic action methods instead of action methods containing the entity type name and the property name.
	/// </summary>
	internal class GenericPropertyRoutingConvention : EntitySetRoutingConvention
	{

		internal const string GetNavigationPropertyMethodName = "GetNavigationProperty";
		internal const string PostNavigationPropertyMethodName = "PostNavigationProperty";

		public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
		{
			if (odataPath == null)
			{
				throw new ArgumentNullException("odataPath");
			}
			if (controllerContext == null)
			{
				throw new ArgumentNullException("controllerContext");
			}
			if (actionMap == null)
			{
				throw new ArgumentNullException("actionMap");
			}

			IEdmNavigationProperty navigationProperty = GetNavigationProperty(odataPath);
			IEdmEntityType declaringType = navigationProperty == null ? null : navigationProperty.DeclaringType as IEdmEntityType;

			if (declaringType != null)
			{
				HttpMethod httpMethod = controllerContext.Request.Method;
				string actionName = null;

				if (httpMethod == HttpMethod.Get)
				{
					actionName = actionMap.FindMatchingAction("Get" + navigationProperty.Name, GetNavigationPropertyMethodName);
				}
				else if (httpMethod == HttpMethod.Post)
				{
					actionName = actionMap.FindMatchingAction("Post" + navigationProperty.Name, PostNavigationPropertyMethodName);
				}

				if (actionName != null)
				{
					KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
					if (keyValueSegment != null)
					{
						controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
					}

					controllerContext.RouteData.Values[ODataRouteConstants.NavigationProperty] = navigationProperty.Name;

					return actionName;
				}
			}
			return null;
		}

		internal static IEdmNavigationProperty GetNavigationProperty(ODataPath odataPath)
		{
			NavigationPathSegment segment = null;
			if (odataPath.PathTemplate == "~/entityset/key/navigation")
			{
				segment = odataPath.Segments[odataPath.Segments.Count - 1] as NavigationPathSegment;
			}
			return segment == null ? null : segment.NavigationProperty;
		}		 

	}
}