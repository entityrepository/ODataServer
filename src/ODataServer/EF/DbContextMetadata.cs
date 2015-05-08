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

			InitializeFrom(dbContext, out _entityTypes, out _entitySets, out _edmModel);
		}

		#region IContainerMetadata

		public string Name { get; set; }
		public string Namespace { get; set; }

		public IEdmModel EdmModel { get { return _edmModel; } }
		public IEdmEntityContainer EdmContainer { get; private set; }

		public IEnumerable<IEntityTypeMetadata> EntityTypes { get { return _entityTypes; } }
		public IEnumerable<IEntitySetMetadata> EntitySets { get { return _entitySets; } }

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
				Contract.Assert(clrEntityType != null, "Unable to find ClrType of EDM conceptual type " + entityType.ToString());

				var clrKeyProperties = entityType.KeyProperties.Select(ep => ep.GetClrPropertyInfo()).ToArray();
				entityTypesList.Add(new EntityTypeMetadata(edmEntityType, clrEntityType, clrKeyProperties));
			}
			entityTypesMetadata = entityTypesList.ToArray();

			List<EntitySetMetadata> entitySetsList = new List<EntitySetMetadata>();
			foreach (IEdmEntitySet edmEntitySet in EdmContainer.EntitySets())
			{
				var elementTypeMetadata = entityTypesMetadata.Single(m => m.EdmType == edmEntitySet.ElementType);
				var elementTypeHierarchy = FindTypeHierarchyFrom(elementTypeMetadata, entityTypesMetadata);
				entitySetsList.Add(new EntitySetMetadata(dbContext.GetType(), this, edmEntitySet, elementTypeMetadata, elementTypeHierarchy));
			}
			entitySetsMetadata = entitySetsList.Cast<IEntitySetMetadata>().ToArray();

			// Iterate over the EntitySets a second time to populate the navigation properties
			foreach (IEdmEntitySet edmEntitySet in EdmContainer.EntitySets())
			{
				EntitySetMetadata sourceEntitySet = entitySetsList.First(esm => ReferenceEquals(esm.EdmEntitySet, edmEntitySet));
				List<INavigationMetadata> navigationMetadata = new List<INavigationMetadata>();
				foreach (var edmNavigationTargetMapping in edmEntitySet.NavigationTargets)
				{
					var targetEntitySet = entitySetsMetadata.First(esm => ReferenceEquals(esm.EdmEntitySet, edmNavigationTargetMapping.TargetEntitySet));
					navigationMetadata.Add(new NavigationMetadata(edmNavigationTargetMapping.NavigationProperty, targetEntitySet));
				}
				sourceEntitySet.NavigationProperties = navigationMetadata;
			}
		}

		private IEntityTypeMetadata[] FindTypeHierarchyFrom(IEntityTypeMetadata root, IEntityTypeMetadata[] allEntityTypes)
		{
			ISet<Type> typeHierarchy = new HashSet<Type> { root.ClrType };
			bool newTypesAdded = true;

			// Keep looping until no more elements are found with parents in typeHierarchy
			while (newTypesAdded)
			{
				newTypesAdded = false;
				// Find types in allEntityTypes that are directly derived from types in typeHierarchy, but which are not in the typeHierarchy set; then add them
				var newDerivedTypes = allEntityTypes.Where(entityTypeMetadata => !typeHierarchy.Contains(entityTypeMetadata.ClrType) && typeHierarchy.Contains(entityTypeMetadata.ClrType.BaseType))
					.Select(entityTypeMetadata => entityTypeMetadata.ClrType);
				foreach (var derivedType in newDerivedTypes)
				{
					newTypesAdded = true; // Loop again
					typeHierarchy.Add(derivedType);
				}
			}

			return allEntityTypes.Where(entityTypeMetadata => typeHierarchy.Contains(entityTypeMetadata.ClrType)).ToArray();
		}

	}

}