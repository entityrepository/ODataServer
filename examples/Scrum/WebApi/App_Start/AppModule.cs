// -----------------------------------------------------------------------
// <copyright file="AppModule.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Http.OData.Query;
using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.Ioc;
using EntityRepository.ODataServer.Model;
using Scrum.Dal;
using SimpleInjector;

namespace Scrum.WebApi
{
	/// <summary>
	/// Handles configuration of application specific types in SimpleInjector.
	/// </summary>
	internal class AppModule : IModule
	{

		/// <summary>
		/// Registers application-level IoC settings.
		/// </summary>
		/// <param name="container"></param>
		public void RegisterServices(Container container)
		{
			container.RegisterWebApiRequestOrTransient<ScrumDb>();
			container.RegisterLazy<ScrumDb>();

			// Required: Register global datamodel metadata (IContainerMetadata and IContainerMetadata<DbContext>)
			var mmRegistration = Lifestyle.Singleton.CreateRegistration(() => new DbContextMetadata<ScrumDb>(new ScrumDb()), container);
			container.AddRegistration(typeof(IContainerMetadata), mmRegistration);
			container.AddRegistration(typeof(IContainerMetadata<ScrumDb>), mmRegistration);

			// Query validation settings could be specified here
			container.RegisterSingleton(new ODataValidationSettings()
			                            {
				                            MaxExpansionDepth = 5,
				                            MaxTop = 200
			                            }); //.Named<ODataValidationSettings>("Edit");  TODO: Figure out how to separate ODataValidationSettings for Edit controllers vs ReadOnly controllers
		}

	}
}
