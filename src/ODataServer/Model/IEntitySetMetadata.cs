// -----------------------------------------------------------------------
// <copyright file="IEntitySetMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Storage-independent model of entityset metadata.
	/// </summary>
	public interface IEntitySetMetadata
	{
		/// <summary>
		/// The context type for the entityset; or <c>null</c> if no context type exists.
		/// </summary>
		Type ContextType { get; }
		IContainerMetadata ContainerMetadata { get; }
		string Name { get; }
		IEdmEntitySet EdmEntitySet { get; }
		IEntityTypeMetadata ElementTypeMetadata { get; }
		IEnumerable<IEntityTypeMetadata> ElementTypeHierarchyMetadata { get; }
		IEnumerable<INavigationMetadata> NavigationProperties { get; } 
	}
}