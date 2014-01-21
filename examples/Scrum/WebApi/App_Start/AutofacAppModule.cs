﻿// -----------------------------------------------------------------------
// <copyright file="AutofacAppModule.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using Autofac;
using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.Model;
using Scrum.Dal;

namespace Scrum.WebApi
{
	/// <summary>
	/// Handles configuration of application specific types in AutoFac.
	/// </summary>
	public class AutofacAppModule : Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			// Required: How to instantiate the DbContext; and share it amongst objects participating in a single request.
			builder.Register(c => new ScrumDb()).InstancePerLifetimeScope();

			// Required: Register global datamodel metadata
			builder.RegisterType<DbContextMetadata<ScrumDb>>().As<IContainerMetadata<ScrumDb>>().SingleInstance();

			// Query validation settings could be specified here
			//builder.RegisterInstance(new ODataValidationSettings
			//{
			//	MaxExpansionDepth = 15,
			//	MaxTop = 200
			//}); //.Named<ODataValidationSettings>("Edit");  TODO: Figure out how to separate ODataValidationSettings for Edit controllers vs ReadOnly controllers
		}

	}
}
