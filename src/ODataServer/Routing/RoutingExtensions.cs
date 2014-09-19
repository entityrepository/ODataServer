// -----------------------------------------------------------------------
// <copyright file="RoutingExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.Util;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;

namespace EntityRepository.ODataServer.Routing
{
	/// <summary>
	/// Extension methods to assist with the routing implementation.
	/// </summary>
	internal static class RoutingExtensions
	{

		/// <summary>
		/// Get the <see cref="IContainerMetadata"/> for this application, if any.  If no instance was
		/// stored, <see cref="null"/> is returned.
		/// </summary>
		/// <param name="httpControllerDescriptor"></param>
		/// <returns></returns>
		internal static IContainerMetadata GetContainerMetadata(this HttpControllerDescriptor httpControllerDescriptor)
		{
			Contract.Assert(httpControllerDescriptor != null);

			return httpControllerDescriptor.Configuration.DependencyResolver.Resolve<IContainerMetadata>();
		}

	}
}