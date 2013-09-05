// -----------------------------------------------------------------------
// <copyright file="ReadOnlyDbSetController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Query;
using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer.EF
{
	/// <summary>An OData API controller that exposes query operations, but no edit operations, on an Entity Framework <see cref="DbSet"/>.</summary>
	/// <typeparam name="TEntity">The entity type for the <c>DbSet</c>.</typeparam>
	/// <typeparam name="TKey">The type of the entity key.</typeparam>
	/// <typeparam name="TDbContext">The <see cref="DbContext"/> type containing the DbSet.</typeparam>
	public class ReadOnlyDbSetController<TEntity, TKey, TDbContext>
		: EntitySetController<TEntity, TKey>
		where TEntity : class
		where TKey : IEquatable<TKey>
		where TDbContext : DbContext
	{

		private readonly Lazy<TDbContext> _lazyDb;
		//private readonly EntityKeyHelper<TDbContext> _entityKeyHelper;
		private readonly IEntitySetMetadata _entitySetMetadata;

		public ReadOnlyDbSetController(Lazy<TDbContext> lazyDbContext, IContainerMetadata<TDbContext> containerMetadata, ODataValidationSettings queryValidationSettings)
			: base(queryValidationSettings)
		{
			Contract.Requires<ArgumentNullException>(lazyDbContext != null);
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(queryValidationSettings != null);

			_lazyDb = lazyDbContext;

			_entitySetMetadata = containerMetadata.GetEntitySetFor(typeof(TEntity));
			//_entityKeyHelper = entityKeyHelper;
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
			Func<TEntity, TKey> entityKeyFunc = EntityKeyFunctions<TEntity, TKey>.GetEntityKeyFunction(_entitySetMetadata.ElementTypeMetadata);
			return entityKeyFunc(entity);
		}

		protected override IQueryable<TEntity> GetBaseQueryable()
		{
			// No point in tracking changes for Query operations, since changes will be tracked on the client
			return DbSet.AsNoTracking();
		}

		protected override IQueryable<TEntity> GetEntityByKeyQuery(TKey key)
		{
			return EntityKeyFunctions<TEntity, TKey>.QueryWhereKeyMatches(GetBaseQueryable(), key, _entitySetMetadata.ElementTypeMetadata);
		}

		public override System.Net.Http.HttpResponseMessage HandleUnmappedRequest(System.Web.Http.OData.Routing.ODataPath odataPath)
		{
			return base.HandleUnmappedRequest(odataPath);
		}

	}
}
