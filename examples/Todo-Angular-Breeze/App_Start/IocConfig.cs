// // -----------------------------------------------------------------------
// <copyright file="IocConfig.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System.Web.Http.OData.Query;
using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.Ioc;
using EntityRepository.ODataServer.Model;
using ODataBreezejsSample.Models;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace ODataBreezejsSample
{

	/// <summary>
	/// Handles configuration of application specific types in SimpleInjector.
	/// </summary>
	internal sealed class IocConfig : IModule
	{

		/// <summary>
		/// Registers application-level IoC settings.
		/// </summary>
		/// <param name="container"></param>
		public void RegisterServices(Container container)
		{
			container.RegisterWebApiRequestOrTransient<TodoListContext>();
			container.RegisterLazy<TodoListContext>();

			// Required: Register global datamodel metadata (IContainerMetadata and IContainerMetadata<DbContext>)
			var mmRegistration = Lifestyle.Singleton.CreateRegistration<DbContextMetadata<TodoListContext>>(container);
			container.AddRegistration(typeof(IContainerMetadata), mmRegistration);
			container.AddRegistration(typeof(IContainerMetadata<TodoListContext>), mmRegistration);

			// Query validation settings could be specified here
			container.RegisterSingle(new ODataValidationSettings()
			{
				MaxExpansionDepth = 5
			});
		}

	}

}