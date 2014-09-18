// -----------------------------------------------------------------------
// <copyright file="AppModule.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.Ioc;
using EntityRepository.ODataServer.Model;
using Scrum.Dal;
using SimpleInjector;
using System.Web.OData.Query;
using SimpleInjector.Integration.WebApi;

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
			// Support sharing the DbContext amongst objects participating in a single request;
			// but if there is no request, just make it transient.
			var webApiRequestLifestyle = new WebApiRequestLifestyle(true);
			var hybridLifestyle = Lifestyle.CreateHybrid(() => webApiRequestLifestyle.GetCurrentScope(container) == null, Lifestyle.Transient, webApiRequestLifestyle);
			container.Register<ScrumDb>(hybridLifestyle);
			container.RegisterLazy<ScrumDb>();

			// Required: Register global datamodel metadata
			container.RegisterSingle<IContainerMetadata<ScrumDb>, DbContextMetadata<ScrumDb>>();

			// Query validation settings could be specified here
			container.RegisterSingle(new ODataValidationSettings
			                         {
				                         MaxExpansionDepth = 5,
				                         MaxTop = 200
			                         }); //.Named<ODataValidationSettings>("Edit");  TODO: Figure out how to separate ODataValidationSettings for Edit controllers vs ReadOnly controllers
		}

	}
}
