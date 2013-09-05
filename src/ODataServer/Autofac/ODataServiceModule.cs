// -----------------------------------------------------------------------
// <copyright file="ODataServiceModule.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Autofac;
using EntityRepository.ODataServer.EF;

namespace EntityRepository.ODataServer.Autofac
{
	/// <summary>
	/// Autofac configuration module for <c>EntityRepository.ODataServer</c>.  Useful if the application is creating its own Autofac container.
	/// </summary>
	public class ODataServiceModule : Module
	{

		/// <summary>
		/// Configures almost all of the defaults needed to run <c>EntityRepository.ODataServer</c>.
		/// </summary>
		/// <param name="builder"></param>
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterGeneric(typeof(ReadOnlyDbSetController<,,>));
			builder.RegisterGeneric(typeof(EditDbSetController<,,>));

			builder.RegisterType<ODataMetadataController>();

			// Default query validation settings (can be overridden)
			builder.RegisterInstance(new ODataValidationSettings
			{
				MaxExpansionDepth = 15,
				MaxTop = 200
			}); //.Named<ODataValidationSettings>("Edit");  TODO: Figure out how to separate ODataValidationSettings for Edit controllers vs ReadOnly controllers
		}
		 
	}
}