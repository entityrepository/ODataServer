// -----------------------------------------------------------------------
// <copyright file="EditDbSetController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer.EF
{
	/// <summary>An OData API controller that implements query and edit operations on an Entity Framework <see cref="DbSet"/>.</summary>
	/// <typeparam name="TEntity">The entity type for the <c>DbSet</c>.</typeparam>
	/// <typeparam name="TKey">The type of the entity key.</typeparam>
	/// <typeparam name="TDbContext">The <see cref="DbContext"/> type containing the DbSet.</typeparam>
	/// <remarks>
	/// Query operation support is inherited from <see cref="ReadOnlyDbSetController{TEntity,TKey,TDbContext}"/>
	/// </remarks>
	public class EditDbSetController<TEntity, TKey, TDbContext>
		: ReadOnlyDbSetController<TEntity, TKey, TDbContext>
		where TEntity : class
		where TKey : IEquatable<TKey>
		where TDbContext : DbContext
	{

		public EditDbSetController(Lazy<TDbContext> lazyDbContext, IContainerMetadata<TDbContext> containerMetadata, ODataValidationSettings queryValidationSettings)
			: base(lazyDbContext, containerMetadata, queryValidationSettings)
		{}

		private void SaveChanges()
		{
			// TODO: Figure out how to delay SaveChanges during batch operations, so a single SaveChanges() can be used.
			Db.SaveChanges();
		}

		#region Additional request formats

		/// <summary>
		/// Handles POST requests that create new entities in the entity set.
		/// </summary>
		/// <param name="entity">The entity to insert into the entity set.</param>
		/// <returns>The response message to send back to the client.</returns>
		public virtual HttpResponseMessage PostNavigation<TChildEntity>([FromODataUri] TKey key, [FromODataUri] string navigation, [FromBody] TChildEntity childEntity)
			where TChildEntity : class
		{
			Contract.Requires<ArgumentException>(! string.IsNullOrWhiteSpace(navigation));
			Contract.Requires<ArgumentNullException>(childEntity != null);

			TEntity original = DbSet.Find(key);
			if (original == null)
			{
				string error = string.Format("Entity lookup failed for key {0} in {1}", key, DbSet);
				throw new ArgumentException(error, "key");
			}

			original.SetPropertyValue(navigation, childEntity);

			return EntitySetControllerHelpers.PostResponse<TChildEntity, object>(this, childEntity, null);
		}

		#endregion
		#region Support for Create-Update-Delete operations on entities

		protected internal override TEntity CreateEntity(TEntity entity)
		{
			DbSet.Add(entity);

			SaveChanges();
			return entity;
		}

		protected internal override TEntity UpdateEntity(TKey key, TEntity update)
		{
			TEntity original = DbSet.Find(key);
			if (original == null)
			{
				string error = string.Format("Entity lookup failed for key {0} in {1}", key, DbSet);
				throw new ArgumentException(error, "key");
			}

			// Apply changes
			ReflectionExtensions.CopyPublicPrimitivePropertyValues(update, original);

			SaveChanges();
			return original;
		}

		protected internal override TEntity PatchEntity(TKey key, Delta<TEntity> patch)
		{
			TEntity entity = DbSet.Find(key);
			if (entity == null)
			{
				string error = string.Format("Entity lookup failed for key {0} in {1}", key, DbSet);
				throw new ArgumentException(error, "key");
			}

			// Apply changes
			patch.CopyChangedValues(entity);

			SaveChanges();
			return entity;
		}

		public override void Delete(TKey key)
		{
			// TODO: Add support for executing a simple query, like DELETE FROM (Table) WHERE (KeyColumn)=key

			TEntity entity = DbSet.Find(key);
			if (entity == null)
			{
				// Doesn't exist, which makes the delete successful
				return;
			}

			DbSet.Remove(entity);
			SaveChanges();
		}

		#endregion
		#region Support for Create-Update-Delete operations on links

		public override void CreateLink(TKey key, string navigationProperty, Uri link)
		{
			base.CreateLink(key, navigationProperty, link);
		}

		public override void DeleteLink(TKey key, string navigationProperty, Uri link)
		{
			base.DeleteLink(key, navigationProperty, link);
		}

		public override void DeleteLink(TKey key, string relatedKey, string navigationProperty)
		{
			base.DeleteLink(key, relatedKey, navigationProperty);
		}

		#endregion

		public override HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
		{
			return base.HandleUnmappedRequest(odataPath);
		}
	}
}
