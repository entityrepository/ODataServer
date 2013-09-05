// -----------------------------------------------------------------------
// <copyright file="DbContextMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using EntityRepository.ODataServer.Model;
using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;
using System.Linq;

namespace EntityRepository.ODataServer.EF
{

	public class DbContextMetadata<TDbContext> : IContainerMetadata<TDbContext>
		where TDbContext : DbContext
	{

		private readonly IEdmModel _edmModel;
		private readonly IEntityTypeMetadata[] _entityTypes;
		private readonly IEntitySetMetadata[] _entitySets;

		public DbContextMetadata(TDbContext dbContext)
		{
			Contract.Requires<ArgumentNullException>(dbContext != null);

			ContainerType = dbContext.GetType();

			InitializeFrom(dbContext, out _entityTypes, out _entitySets, out _edmModel);
		}

		#region IContainerMetadata

		public string Name { get; set; }
		public string Namespace { get; set; }

		public Type ContainerType { get; private set; }

		public IEdmModel EdmModel { get { return _edmModel; } }
		public IEdmEntityContainer EdmContainer { get; private set; }

		public IEnumerable<IEntityTypeMetadata> EntityTypes { get { return _entityTypes; } }
		public IEnumerable<IEntitySetMetadata> EntitySets { get { return _entitySets; } }

		public IEntityTypeMetadata GetEntityType(Type clrType)
		{
			Contract.Requires<ArgumentNullException>(clrType != null);

			return _entityTypes.SingleOrDefault(et => et.ClrType == clrType);
		}

		public IEntityTypeMetadata GetEntityType(IEdmStructuredType edmEntityType)
		{
			Contract.Requires<ArgumentNullException>(edmEntityType != null);

			return _entityTypes.SingleOrDefault(et => et.EdmType.IsEquivalentTo(edmEntityType));
		}

		public IEntitySetMetadata GetEntitySet(string entitySetName)
		{
			Contract.Requires<ArgumentException>(! string.IsNullOrWhiteSpace(entitySetName));

			return _entitySets.SingleOrDefault(es => string.Equals(es.Name, entitySetName, StringComparison.Ordinal));
		}

		public IEntitySetMetadata GetEntitySetFor(Type clrType)
		{
			Contract.Requires<ArgumentNullException>(clrType != null);

			return _entitySets.SingleOrDefault(es => es.ElementTypeMetadata.ClrType == clrType);
		}

		public IEntitySetMetadata GetEntitySetFor(IEntityTypeMetadata entityTypeMetadata)
		{
			Contract.Requires<ArgumentNullException>(entityTypeMetadata != null);

			return _entitySets.SingleOrDefault(es => es.ElementTypeMetadata.ClrType == entityTypeMetadata.ClrType);
		}

		public IEntitySetMetadata GetEntitySetFor(IEdmStructuredType edmEntityType)
		{
			Contract.Requires<ArgumentNullException>(edmEntityType != null);

			return _entitySets.SingleOrDefault(es => es.EdmEntitySet.ElementType.IsEquivalentTo(edmEntityType));
		}

		#endregion

		/// <summary>
		/// Creates the datamodel hierarchy from <paramref name="dbContext"/>.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="entityTypesMetadata"></param>
		/// <param name="entitySetsMetadata"></param>
		/// <param name="edmModel"></param>
		/// <remarks>
		/// This method is a little confusing due to the two kinds of "EDM" types used.  The <see cref="Microsoft.Data.Edm.IEdmModel"/> universe is from the
		/// <c>Microsoft.Data.Edm</c> library, and it provides interfaces and classes for a storage-independent data model, which is why it's the backbone of
		/// the interfaces in <c>EntityRepository.ODataServer.Model</c>.  Separately, there's the Entity Framework EDM classes, which are used here to do things
		/// like extract the CLR types for entities and key properties.
		/// </remarks>
		private void InitializeFrom(TDbContext dbContext, out IEntityTypeMetadata[] entityTypesMetadata, out IEntitySetMetadata[] entitySetsMetadata, out IEdmModel edmModel)
		{
			edmModel = dbContext.GetEdmModel();

			var objectContextAdapter = dbContext as IObjectContextAdapter;
			ObjectContext objectContext = objectContextAdapter.ObjectContext;

			ItemCollection cSpaceItems = objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSpace);
//			var entityContainer = cSpaceItems.OfType<EntityContainer>().Single();
//			ItemCollection sSpaceItems = objectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);

			EdmContainer = edmModel.FindDeclaredEntityContainer(objectContext.DefaultContainerName);

			if (Name == null)
			{
				Name = EdmContainer.Name;
			}
			if (Namespace == null)
			{
				Namespace = EdmContainer.Namespace;
			}

			List<IEntityTypeMetadata> entityTypesList = new List<IEntityTypeMetadata>();
			foreach (IEdmEntityType edmEntityType in edmModel.SchemaElements.OfType<IEdmEntityType>())
			{
				EntityType entityType = cSpaceItems.OfType<EntityType>().Single(et => et.Name == edmEntityType.Name);
				Type clrEntityType = entityType.GetClrType();
				var clrKeyProperties = entityType.KeyProperties.Select(ep => ep.GetClrPropertyInfo()).ToArray();
				entityTypesList.Add(new EntityTypeMetadata(edmEntityType, clrEntityType, clrKeyProperties));
			}
			entityTypesMetadata = entityTypesList.ToArray();

			List<IEntitySetMetadata> entitySetsList = new List<IEntitySetMetadata>();
			foreach (IEdmEntitySet edmEntitySet in EdmContainer.EntitySets())
			{
				var entityTypeMetadata = entityTypesMetadata.Single(m => m.EdmType == edmEntitySet.ElementType);
				entitySetsList.Add(new EntitySetMetadata(this, edmEntitySet, entityTypeMetadata));
			}
			entitySetsMetadata = entitySetsList.ToArray();
		}

	}

}