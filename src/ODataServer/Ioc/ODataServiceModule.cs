// -----------------------------------------------------------------------
// <copyright file="ODataServiceModule.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using EntityRepository.ODataServer.EF;
using SimpleInjector;
using SimpleInjector.Extensions;

namespace EntityRepository.ODataServer.Ioc
{

	/// <summary>
	/// SimpleInjector configuration module for <c>EntityRepository.ODataServer</c>.
	/// </summary>
	public class ODataServiceModule : IModule
	{

		/// <summary>
		/// Configures almost all of the defaults needed to run <c>EntityRepository.ODataServer</c>.
		/// </summary>
		/// <param name="container">A <see cref="Container"/>.</param>
		public void RegisterServices(Container container)
		{
			container.RegisterOpenGeneric(typeof(ReadOnlyDbSetController<,,>), typeof(ReadOnlyDbSetController<,,>));
			container.RegisterOpenGeneric(typeof(EditDbSetController<,,>), typeof(EditDbSetController<,,>));

			container.Register<ODataMetadataController>();

			// Default query validation settings (can be overridden)
			container.RegisterSingle(new ODataValidationSettings
			                         {
				                         MaxExpansionDepth = 15,
				                         MaxTop = 200
			                         }); //.Named<ODataValidationSettings>("Edit");  TODO: Figure out how to separate ODataValidationSettings for Edit controllers vs ReadOnly controllers

			// Default query settings (can be overridden)
			container.RegisterSingle(new ODataQuerySettings()
			                         {
				                         PageSize = 200
			                         });
		}
		 
	}
}