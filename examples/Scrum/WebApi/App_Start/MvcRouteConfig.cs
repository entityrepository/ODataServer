// -----------------------------------------------------------------------
// <copyright file="MvcRouteConfig.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Mvc;
using System.Web.Routing;

namespace Scrum.WebApi
{
	public class MvcRouteConfig
	{

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
			                name: "Default",
			                url: "{controller}/{action}/{id}",
			                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
				);
		}

	}
}
