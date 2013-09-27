// -----------------------------------------------------------------------
// <copyright file="IEntitySetMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Storage-independent model of entityset metadata.
	/// </summary>
	public interface IEntitySetMetadata
	{
		IContainerMetadata ContainerMetadata { get; }
		string Name { get; }
		IEdmEntitySet EdmEntitySet { get; }
		IEntityTypeMetadata ElementTypeMetadata { get; }
		IEnumerable<IEntityTypeMetadata> ElementTypeHierarchyMetadata { get; }
		IEnumerable<INavigationMetadata> NavigationProperties { get; } 
	}
}