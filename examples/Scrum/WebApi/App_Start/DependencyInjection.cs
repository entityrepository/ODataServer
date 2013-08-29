// -----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using Autofac;
using EntityRepository.ODataServer;
using EntityRepository.ODataServer.Autofac;
using Scrum.Dal;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;

namespace Scrum.WebApi
{
	/// <summary>
	/// Handles configuration of dependency injection - using AutoFac.
	/// </summary>
	public static class DependencyInjection
	{

		public static IContainer Container { get; private set; }

		public static void Configure(HttpConfiguration webApiConfig)
		{
			var builder = new ContainerBuilder();
			ConfigureContainer(builder);
			Container = builder.Build();
			webApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
		}

		private static void ConfigureContainer(ContainerBuilder builder)
		{
			builder.Register(c => new ScrumDb());
			builder.RegisterGeneric(typeof(EntityKeyHelper<>)).SingleInstance();
			builder.RegisterGeneric(typeof(ReadOnlyDbSetController<,,>));
			builder.RegisterGeneric(typeof(EditDbSetController<,,>));

			builder.RegisterType<ODataMetadataController>();

			// Query validation settings
			builder.RegisterInstance(new ODataValidationSettings
			                         {
				                         MaxExpansionDepth = 15,
				                         MaxTop = 200
			                         }); //.Named<ODataValidationSettings>("Edit");  TODO: Figure out how to separate ODataValidationSettings for Edit controllers vs ReadOnly controllers
		}

	}
}
