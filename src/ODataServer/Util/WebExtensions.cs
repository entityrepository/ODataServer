// -----------------------------------------------------------------------
// <copyright file="WebExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Web.Http.Dependencies;

namespace EntityRepository.ODataServer.Util
{
	/// <summary>
	/// Static helper methods and extension methods.
	/// </summary>
	public static class WebExtensions
	{
		/// <summary>
		/// Obtain a registered object of type <typeparamref name="T"/> from <paramref name="dependencyResolver"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dependencyResolver"></param>
		/// <returns></returns>
		 public static T Resolve<T>(this IDependencyResolver dependencyResolver)
			 where T : class
		 {
			 Type serviceType = typeof(T);
			 object oService = dependencyResolver.GetService(serviceType);
			 T tService = oService as T;

			 if ((tService == null)
			     && (oService != null))
			 {
				 throw new InvalidOperationException(string.Format("Object {0} was registered in the DependencyResolver using type {1}, which it is not compatible with.", oService, serviceType));
			 }

			 return tService;
		 }

	}
}