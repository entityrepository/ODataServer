// -----------------------------------------------------------------------
// <copyright file="IContainerMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Storage-independent model describing the structure of a container or context of entity sets and entity types.
	/// </summary>
	public interface IContainerMetadata
	{
		string Name { get; set; }
		string Namespace { get; set; }

		Type ContainerType { get; }

		IEdmModel EdmModel { get; }
		IEdmEntityContainer EdmContainer { get; }

		IEnumerable<IEntityTypeMetadata> EntityTypes { get; }
		IEnumerable<IEntitySetMetadata> EntitySets { get; }

		IEntityTypeMetadata GetEntityType(Type clrType);
		IEntityTypeMetadata GetEntityType(IEdmStructuredType edmEntityType);

		IEntitySetMetadata GetEntitySet(string entitySetName);
		IEntitySetMetadata GetEntitySetFor(Type clrType);
		IEntitySetMetadata GetEntitySetFor(IEntityTypeMetadata entityTypeMetadata);
		IEntitySetMetadata GetEntitySetFor(IEdmStructuredType edmEntityType);

	}
}