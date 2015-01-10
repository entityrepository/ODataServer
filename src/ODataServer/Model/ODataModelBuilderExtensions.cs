// // -----------------------------------------------------------------------
// <copyright file="ODataModelBuilderExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{

	/// <summary>
	/// Provides extension methods for <see cref="ODataModelBuilder"/> and <see cref="ODataConventionModelBuilder"/>,
	/// to make it easy to convert an <see cref="IContainerMetadata"/> object to an <see cref="IEdmModel"/>.
	/// </summary>
	/// <remarks>
	/// See <a href="https://entityrepository.codeplex.com/workitem/12">EntityRepository $metadata is not Breeze-compatible</a>.
	/// Use of <see cref="ODataConventionModelBuilder"/> currently breaks Breeze compatibility.
	/// </remarks>
	public static class ODataModelBuilderExtensions
	{

		public static void ConfigureFromContainer(this ODataModelBuilder modelBuilder, IContainerMetadata containerMetadata)
		{
			modelBuilder.ContainerName = containerMetadata.Name;
			modelBuilder.Namespace = containerMetadata.Namespace;

			// Add all entity types
			foreach (IEntityTypeMetadata entityTypeMetadata in containerMetadata.EntityTypes)
			{
				EntityTypeConfiguration entityTypeConfig = modelBuilder.AddEntity(entityTypeMetadata.ClrType);
			}

			// Add all entity sets
			foreach (IEntitySetMetadata entitySetMetadata in containerMetadata.EntitySets)
			{
				string entitySetName = entitySetMetadata.Name;
				EntityTypeConfiguration entityTypeConfig = (EntityTypeConfiguration) modelBuilder.GetTypeConfigurationOrNull(entitySetMetadata.ElementTypeMetadata.ClrType);
				EntitySetConfiguration entitySetConfig = modelBuilder.AddEntitySet(entitySetName, entityTypeConfig);
			}
		}

	}

}