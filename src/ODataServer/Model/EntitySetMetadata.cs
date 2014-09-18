// -----------------------------------------------------------------------
// <copyright file="EntityTypeMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace EntityRepository.ODataServer.Model
{
	public class EntitySetMetadata : IEntitySetMetadata
	{

		private readonly IEdmEntitySet _edmEntitySet;

		internal EntitySetMetadata(Type contextType, IContainerMetadata container, IEdmEntitySet edmEntitySet, IEntityTypeMetadata entityTypeMetadata, IEntityTypeMetadata[] entityTypeHierarchyMetadata)
		{
			Contract.Assert(container != null);
			Contract.Assert(edmEntitySet != null);
			Contract.Assert(entityTypeMetadata != null);
			Contract.Assert(entityTypeHierarchyMetadata != null);
			Contract.Assert(entityTypeHierarchyMetadata.Length >= 1);

			ContextType = contextType;
			ContainerMetadata = container;
			_edmEntitySet = edmEntitySet;
			ElementTypeMetadata = entityTypeMetadata;
			ElementTypeHierarchyMetadata = entityTypeHierarchyMetadata;
		}

		public Type ContextType { get; private set; }

		public IContainerMetadata ContainerMetadata { get; private set; }

		public string Name { get { return _edmEntitySet.Name; } }

		public IEdmEntitySet EdmEntitySet
		{
			get { return _edmEntitySet; }
		}

		public IEntityTypeMetadata ElementTypeMetadata { get; private set; }

		public IEnumerable<IEntityTypeMetadata> ElementTypeHierarchyMetadata { get; private set; }

		public IEnumerable<INavigationMetadata> NavigationProperties { get; internal set; }

	}
}