// -----------------------------------------------------------------------
// <copyright file="RoutingExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Diagnostics.Contracts;
using System.Threading;
using System.Web.Http.Controllers;
using EntityRepository.ODataServer.Model;

namespace EntityRepository.ODataServer.Routing
{
	/// <summary>
	/// Extension methods to assist with the routing implementation.
	/// </summary>
	internal static class RoutingExtensions
	{

		internal const string ContainerMetadataKey = "EntityRepository.ContainerMetadata";

		/// <summary>
		/// During initialization, holds the current IContainerMetadata in a threadlocal, so it can be used by other classes in the same thread
		/// during initialization.  Specifically, <see cref="UseEntityRepositoryActionSelectorAttribute"/> requires it because the attribute is 
		/// initialized before properties can be set in a <see cref="HttpControllerDescriptor"/>.
		/// </summary>
		internal readonly static ThreadLocal<IContainerMetadata> InitializingContainerMetadata = new ThreadLocal<IContainerMetadata>();

		/// <summary>
		/// Store the <see cref="IContainerMetadata"/> in the controller descriptor for later use.
		/// </summary>
		/// <param name="httpControllerDescriptor"></param>
		/// <param name="containerMetadata"></param>
		internal static void CacheContainerMetadata(this HttpControllerDescriptor httpControllerDescriptor, IContainerMetadata containerMetadata)
		{
			Contract.Assert(httpControllerDescriptor != null);
			Contract.Assert(containerMetadata != null);

			httpControllerDescriptor.Properties[ContainerMetadataKey] = containerMetadata;
		}

		/// <summary>
		/// Get the <see cref="IContainerMetadata"/> stored with this controller descriptor, if any.  If no instance was
		/// stored, <see cref="null"/> is returned.
		/// </summary>
		/// <param name="httpControllerDescriptor"></param>
		/// <returns></returns>
		internal static IContainerMetadata GetContainerMetadata(this HttpControllerDescriptor httpControllerDescriptor)
		{
			Contract.Assert(httpControllerDescriptor != null);

			object containerMetadata;
			if (! httpControllerDescriptor.Properties.TryGetValue(ContainerMetadataKey, out containerMetadata))
			{
				// Try reading from the threadlocal.
				containerMetadata = InitializingContainerMetadata.Value;
			}
			return containerMetadata as IContainerMetadata;			
		}

	}
}