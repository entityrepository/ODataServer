// -----------------------------------------------------------------------
// <copyright file="EntityTypeMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	public class EntitySetMetadata : IEntitySetMetadata
	{

		private readonly IEdmEntitySet _edmEntitySet;

		internal EntitySetMetadata(IContainerMetadata container, IEdmEntitySet edmEntitySet, IEntityTypeMetadata entityTypeMetadata)
		{
			ContainerMetadata = container;
			_edmEntitySet = edmEntitySet;
			ElementTypeMetadata = entityTypeMetadata;
		}


		public IContainerMetadata ContainerMetadata { get; private set; }

		public string Name { get { return _edmEntitySet.Name; } }

		public IEdmEntitySet EdmEntitySet
		{
			get { return _edmEntitySet; }
		}

		public IEntityTypeMetadata ElementTypeMetadata { get; private set; }

	}
}