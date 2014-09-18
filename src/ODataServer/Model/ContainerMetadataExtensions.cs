// -----------------------------------------------------------------------
// <copyright file="ContainerMetadataExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Extension methods for <see cref="IContainerMetadata"/>.
	/// </summary>
	public static class ContainerMetadataExtensions
	{
		public static IEntityTypeMetadata GetEntityType(this IContainerMetadata containerMetadata, Type clrType)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(clrType != null);
			Contract.Ensures(Contract.Result<IEntityTypeMetadata>() != null);

			return containerMetadata.EntityTypes.SingleOrDefault(et => et.ClrType == clrType);
		}

		public static IEntityTypeMetadata GetEntityType(this IContainerMetadata containerMetadata, IEdmSchemaType edmSchemaType)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(edmSchemaType != null);
			
			return containerMetadata.EntityTypes.SingleOrDefault(et =>
			{
				IEdmEntityType edmType = et.EdmType;
				return edmType.TypeKind == edmSchemaType.TypeKind
					   && edmType.Name == edmSchemaType.Name
					   && ((edmType.Namespace == edmSchemaType.Namespace) || (containerMetadata.Namespace == edmSchemaType.Namespace) || (et.ClrType.Namespace == edmSchemaType.Namespace));
			});
		}

		public static IEntitySetMetadata GetEntitySet(this IContainerMetadata containerMetadata, string entitySetName)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(entitySetName));

			return containerMetadata.EntitySets.SingleOrDefault(es => string.Equals(es.Name, entitySetName, StringComparison.Ordinal));
		}

		public static IEntitySetMetadata GetEntitySetFor(this IContainerMetadata containerMetadata, Type clrType)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(clrType != null);

			return containerMetadata.EntitySets.SingleOrDefault(es => es.ElementTypeMetadata.ClrType == clrType);
		}

		public static IEntitySetMetadata GetEntitySetFor(this IContainerMetadata containerMetadata, IEntityTypeMetadata entityTypeMetadata)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(entityTypeMetadata != null);

			return containerMetadata.EntitySets.SingleOrDefault(es => es.ElementTypeMetadata.ClrType == entityTypeMetadata.ClrType);
		}

		public static IEntitySetMetadata GetEntitySetFor(this IContainerMetadata containerMetadata, IEdmSchemaType edmSchemaType)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(edmSchemaType != null);

			IEntityTypeMetadata entityType = GetEntityType(containerMetadata, edmSchemaType);
			if (entityType == null)
			{
				return null;
			}
			return containerMetadata.EntitySets.SingleOrDefault(es => es.ElementTypeHierarchyMetadata.Contains(entityType));
		}

	}

}
