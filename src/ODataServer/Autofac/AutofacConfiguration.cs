// -----------------------------------------------------------------------
// <copyright file="AutofacConfiguration.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using Autofac;
using Autofac.Core;
using System.Collections.Generic;
using System.Web.Http;

namespace EntityRepository.ODataServer.Autofac
{
	/// <summary>
	/// Handles configuration of dependency injection for Web API - using AutoFac.
	/// </summary>
	public class AutofacConfiguration 
	{

		//private static IContainer _container;

		///// <summary>
		///// The Autofac <see cref="IContainer"/>.
		///// </summary>
		//public static IContainer Container
		//{
		//	get { return _container; }
		//	set
		//	{
		//		lock (typeof(AutofacConfiguration))
		//		{
		//			if (_container != null)
		//			{
		//				throw new InvalidOperationException("Autofac IContainer can only be set once.");
		//			}
		//			_container = value;
		//		}
		//	}
		//}

		/// <summary>
		/// Configuration method to create and configure the DI container.
		/// </summary>
		/// <param name="webApiConfig"></param>
		/// <param name="autofacModules"></param>
		public static void Configure(HttpConfiguration webApiConfig, params IModule[] autofacModules)
		{
			var builder = new ContainerBuilder();

			// Register standard ODataService types
			builder.RegisterModule(new ODataServiceModule());
			
			// Register any passed in modules
			if (autofacModules != null)
			{
				foreach (var autofacModule in autofacModules)
				{
					builder.RegisterModule(autofacModule);
				}
			}

			//Container = builder.Build();
			IContainer container = builder.Build();
			webApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}


	}
}