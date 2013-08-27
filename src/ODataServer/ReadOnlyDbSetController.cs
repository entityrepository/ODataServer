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

namespace EntityRepository.ODataServer
{
	/// <summary>A controller that exposes query operations on an Entity Framework <see cref="DbSet"/>.</summary>
	/// <typeparam name="TEntity">The type associated with the exposed entity set's entity type.</typeparam>
	/// <typeparam name="TKey">The type associated with the entity key of the exposed entity set's entity type.</typeparam>
	/// <typeparam name="TDbContext">The <see cref="DbContext"/> type containing the DbSet.</typeparam>
	public class ReadOnlyDbSetController<TEntity, TKey, TDbContext>
		: EntitySetController<TEntity, TKey>
		where TEntity : class
		where TDbContext : DbContext
	{

		private readonly Lazy<TDbContext> _lazyDb;
		private readonly EntityKeyHelper<TDbContext> _entityKeyHelper;

		public ReadOnlyDbSetController(Lazy<TDbContext> lazyDbContext, EntityKeyHelper<TDbContext> entityKeyHelper, ODataValidationSettings queryValidationSettings)
			: base(queryValidationSettings)
		{
			Contract.Requires<ArgumentNullException>(lazyDbContext != null);
			_lazyDb = lazyDbContext;
			_entityKeyHelper = entityKeyHelper;
		}

		protected TDbContext Db
		{
			get { return _lazyDb.Value; }
		}

		protected DbSet<TEntity> DbSet
		{
			get { return Db.Set<TEntity>(); }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_lazyDb.IsValueCreated)
				{
					_lazyDb.Value.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override TKey GetKey(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Func<TEntity, TKey> entityKeyFunc = _entityKeyHelper.GetEntityKeyFunction<TEntity, TKey>();
			return entityKeyFunc(entity);
		}

		protected override IQueryable<TEntity> GetBaseQueryable()
		{
			return DbSet;
		}

		protected override IQueryable<TEntity> GetEntityByKeyQuery(TKey key)
		{
			return _entityKeyHelper.QueryWhereKeyMatches(DbSet, key);
		}

		public override System.Net.Http.HttpResponseMessage HandleUnmappedRequest(System.Web.Http.OData.Routing.ODataPath odataPath)
		{
			return base.HandleUnmappedRequest(odataPath);
		}

	}
}
