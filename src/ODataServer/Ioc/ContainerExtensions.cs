// -----------------------------------------------------------------------
// <copyright file="ContainerExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using EntityRepository.ODataServer.Ioc;
using SimpleInjector.Integration.WebApi;

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
		public static void RegisterLazy<T>(this Container container) 
			where T : class
		{
			Contract.Requires<ArgumentNullException>(container != null);

			Func<T> factory = container.GetInstance<T>;
			container.Register(() => new Lazy<T>(factory));
		}

		/// <summary>
		/// Creates a <see cref="Lifestyle"/> that supports sharing an instance within a Web Api request, or if no Web Api request
		/// is in scope, creates a transient instance.
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		public static Lifestyle PerWebApiRequestOrTransientLifestyle(this Container container)
		{
			Contract.Requires<ArgumentNullException>(container != null);

			var webApiRequestLifestyle = new WebApiRequestLifestyle(true);
			var hybridLifestyle = Lifestyle.CreateHybrid(() => webApiRequestLifestyle.GetCurrentScope(container) == null, Lifestyle.Transient, webApiRequestLifestyle);
			return hybridLifestyle;
		}

		/// <summary>
		/// Registers type <typeparamref name="TConcrete"/> such that a single instance is shared within a Web Api request, or if no Web Api request
		/// is in scope, a transient instance is created. When the Web API request ends and <typeparamref name="TConcrete"/> implements 
		/// <see cref="T:System.IDisposable"/>, the cached instance will be disposed.
		/// </summary>
		/// <typeparam name="TConcrete"></typeparam>
		/// <param name="container"></param>
		public static void RegisterWebApiRequestOrTransient<TConcrete>(this Container container) 
			where TConcrete : class
		{
			Contract.Requires<ArgumentNullException>(container != null);

			container.Register<TConcrete>(container.PerWebApiRequestOrTransientLifestyle());
		}

		/// <summary>
		/// Registers type <typeparamref name="TImplementation"/> such that a single instance is shared within a Web Api request, or if no Web Api request
		/// is in scope, a transient instance is created. When the Web API request ends and <typeparamref name="TImplementation"/> implements 
		/// <see cref="T:System.IDisposable"/>, the cached instance will be disposed.
		/// </summary>
		/// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
		/// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
		/// <param name="container">The container to make the registrations in.</param>
		/// <exception cref="T:System.ArgumentNullException">Thrown when the <paramref name="container"/> is a null reference.</exception>
		/// <exception cref="T:System.InvalidOperationException">Thrown when this container instance is locked and can not be altered, or when an
		/// <typeparamref name="TService"/> has already been registered.</exception>
		/// <exception cref="T:System.ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> type is not 
		/// a type that can be created by the container.</exception>
		public static void RegisterWebApiRequestOrTransient<TService, TImplementation>(this Container container) 
			where TService : class
			where TImplementation : class, TService
		{
			Contract.Requires<ArgumentNullException>(container != null);

			container.Register<TService, TImplementation>(container.PerWebApiRequestOrTransientLifestyle());
		}

		/// <summary>
		/// Registers the specified delegate to return instances of type <typeparamref name="TService"/>.  The instance is shared within a Web Api request, or if no Web Api request is in scope, a transient instance is created. When the Web API request ends and the object implements 
		/// <see cref="T:System.IDisposable"/>, the cached instance will be disposed.
		/// </summary>
		/// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
		/// <param name="container">The container to make the registrations in.</param>
		/// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
		/// <exception cref="T:System.ArgumentNullException">Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are null references.</exception>
		/// <exception cref="T:System.InvalidOperationException">Thrown when this container instance is locked and can not be altered, or when the <typeparamref name="TService"/> has already been registered.</exception>
		public static void RegisterWebApiRequestOrTransient<TService>(this Container container, Func<TService> instanceCreator)
			where TService : class
		{
			Contract.Requires<ArgumentNullException>(container != null);
			Contract.Requires<ArgumentNullException>(instanceCreator != null);

			container.Register<TService>(instanceCreator, container.PerWebApiRequestOrTransientLifestyle());
		}

	}

}
