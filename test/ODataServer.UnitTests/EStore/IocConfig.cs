// -----------------------------------------------------------------------
// <copyright file="IocConfig.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Http.OData.Query;
using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.Ioc;
using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.UnitTests.EStore.DataAccess;
using SimpleInjector;

namespace EntityRepository.ODataServer.UnitTests.EStore
{

	/// <summary>
	/// Configures SimpleInjector to use the EStore model + <see cref="EStoreDb"/> for odata tests.
	/// </summary>
	public sealed class IocConfig : IModule
	{

		public void RegisterServices(Container container)
		{
			container.RegisterWebApiRequestOrTransient<EStoreDb>();
			container.RegisterLazy<EStoreDb>();

			// Required: Register global datamodel metadata
			var mmRegistration = Lifestyle.Singleton.CreateRegistration<DbContextMetadata<EStoreDb>>(container);
			container.AddRegistration(typeof(IContainerMetadata), mmRegistration);
			container.AddRegistration(typeof(IContainerMetadata<EStoreDb>), mmRegistration);

			// Query validation settings could be specified here
			container.RegisterSingleton(new ODataValidationSettings()
			                            {
				                            MaxExpansionDepth = 5,
				                            MaxTop = 200
			                            });
		}

	}

}
