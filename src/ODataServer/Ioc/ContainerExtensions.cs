// -----------------------------------------------------------------------
// <copyright file="ContainerExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using EntityRepository.ODataServer.Ioc;

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{

	/// <summary>
	/// Extension methods for SimpleInjector <see cref="Container"/>.
	/// </summary>
	public static class ContainerExtensions
	{

		/// <summary>
		/// Runs the registration code in the specified <paramref name="modules"/>.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="modules"></param>
		public static void RegisterModules(this Container container, params IModule[] modules)
		{
			Contract.Requires<ArgumentNullException>(container != null);
			Contract.Requires<ArgumentNullException>(modules != null);

			foreach (var module in modules)
			{
				module.RegisterServices(container);
			}
		}

		/// <summary>
		/// Registers a <see cref="Lazy{T}"/> wrapper around the registration of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		public static void RegisterLazy<T>(this Container container) where T : class
		{
			Contract.Requires<ArgumentNullException>(container != null);

			Func<T> factory = container.GetInstance<T>;
			container.Register(() => new Lazy<T>(factory));
		}

	}

}
