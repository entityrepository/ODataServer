// -----------------------------------------------------------------------
// <copyright file="ReadOnlyDbSetController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.Util;
using Microsoft.OData.Edm;
using System;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.OData.Query;
using System.Web.OData.Routing;

namespace EntityRepository.ODataServer.EF
{
	/// <summary>An OData API controller that exposes query operations, but no edit operations, on an Entity Framework <see cref="DbSet"/>.</summary>
	/// <typeparam name="TEntity">The entity type for the <c>DbSet</c>.</typeparam>
	/// <typeparam name="TKey">The type of the entity key.</typeparam>
	/// <typeparam name="TDbContext">The <see cref="DbContext"/> type containing the DbSet.</typeparam>
	public class ReadOnlyDbSetController<TEntity, TKey, TDbContext>
		: EntitySetController<TEntity, TKey>
		where TEntity : class
		//where TKey : IEquatable<TKey>
		where TDbContext : DbContext
	{

		private readonly Lazy<TDbContext> _lazyDb;

		public ReadOnlyDbSetController(Lazy<TDbContext> lazyDbContext, IContainerMetadata<TDbContext> containerMetadata, ODataValidationSettings queryValidationSettings, ODataQuerySettings querySettings)
			: base(containerMetadata, queryValidationSettings, querySettings)
		{
			Contract.Requires<ArgumentNullException>(lazyDbContext != null);

			_lazyDb = lazyDbContext;
		}

		protected bool IsDbCreated
		{
			get { return _lazyDb.IsValueCreated; }	
		}

		protected TDbContext Db
		{
			get { return _lazyDb.Value; }
		}

		protected DbSet<TEntity> DbSet
		{
			get { return Db.Set<TEntity>(); }
		}

		protected override TKey GetKey(TEntity entity)
		{
			Func<TEntity, TKey> entityKeyFunc = EntityKeyFunction<TEntity, TKey>.GetEntityKeyFunction(EntitySetMetadata.ElementTypeMetadata);
			return entityKeyFunc(entity);
		}

		protected override IQueryable<TEntity> GetBaseQueryable()
		{
			// No point in tracking changes for Query operations, since changes will be tracked on the client
			return DbSet.AsNoTracking();
		}

		protected override TEntity GetEntityByKey(TKey key)
		{
			return DbSet.Find(key);
		}

		protected override IQueryable<TEntity> GetEntityByKeyQuery(TKey key)
		{
			return EntityKeyFunction<TEntity, TKey>.QueryWhereKeyMatches(GetBaseQueryable(), key, EntitySetMetadata.ElementTypeMetadata);
		}

		protected override IQueryable<TEntity> GetEntityWithNavigationPropertyQuery<TProperty>(TKey key, IEdmNavigationProperty edmNavigationProperty)
		{
			return GetEntityByKeyQuery(key).Include(edmNavigationProperty.Name);
		}

		/// <summary>
		/// Returns the entity specified by <paramref name="link"/>.  This can be an entity from 
		/// any entityset in the <see cref="Db"/>.
		/// </summary>
		/// <param name="link"></param>
		/// <returns></returns>
		protected virtual object GetEntityForLink(Uri link)
		{
			// Parse the link to fetch the linkedObject
			IEdmEntitySet edmEntitySet;
			object key;
			if (! this.ParseSingleEntityLink(link, out edmEntitySet, out key))
			{
				return null;
			}

			Type entitySetType = ContainerMetadata.GetEntitySet(edmEntitySet.Name).ElementTypeMetadata.ClrType;
			return Db.Set(entitySetType).Find(key);
		}
	}
}
