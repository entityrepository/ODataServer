// -----------------------------------------------------------------------
// <copyright file="CollectionExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace EntityRepository.ODataServer.Util
{
	/// <summary>
	/// Extension methods for collection types.
	/// </summary>
	public static class CollectionExtensions
	{

		/// <summary>
		/// Dispose all elements in the collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		public static void DisposeAll<T>(this IEnumerable<T> collection)
			where T : IDisposable
		{
			if (collection == null)
			{
				return;
			}

			foreach (var t in collection)
			{
				if (t != null)
				{
					t.Dispose();
				}
			}
		}

		public static void DisposeAll<T>(this IEnumerable<T> collection, Func<T, object> selector)
		{
			Contract.Requires<ArgumentNullException>(selector != null);
			if (collection == null)
			{
				return;
			}

			foreach (var t in collection)
			{
				if (t != null)
				{
					IDisposable disposable = selector(t) as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
		}

	}
}