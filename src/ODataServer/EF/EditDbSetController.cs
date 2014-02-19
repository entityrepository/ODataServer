// -----------------------------------------------------------------------
// <copyright file="EditDbSetController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using EntityRepository.ODataServer.Batch;
using EntityRepository.ODataServer.Model;
using EntityRepository.ODataServer.Results;
using EntityRepository.ODataServer.Routing;
using EntityRepository.ODataServer.Util;
using Microsoft.Data.Edm;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;

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
		//where TKey : IEquatable<TKey>
		where TDbContext : DbContext
	{

		public EditDbSetController(Lazy<TDbContext> lazyDbContext, IContainerMetadata<TDbContext> containerMetadata, ODataValidationSettings queryValidationSettings, ODataQuerySettings querySettings)
			: base(lazyDbContext, containerMetadata, queryValidationSettings, querySettings)
		{}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		/// <summary>
		/// SaveChanges() is called after a changeset has been applied, using <see cref="ChangeSetExtensions.OnChangeSetSuccess(System.Web.Http.OData.ODataController,System.Action)"/>.
		/// </summary>
		private void SaveChanges()
		{
			if (! IsDbCreated)
			{
				return;
			}


			TDbContext dbContext = Db;
			dbContext.ChangeTracker.DetectChanges();
			if (dbContext.ChangeTracker.HasChanges())
			{
				dbContext.SaveChanges();
			}
			else
			{
				Trace.WriteLine("  No changes detected in " + dbContext.GetType().FullName);
			}
		}

		#region Support for Create-Update-Delete operations on entities

		protected internal override TEntity CreateEntity(TEntity entity)
		{
			DbSet.Add(entity);

			this.OnChangeSetSuccess(SaveChanges);
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

			this.OnChangeSetSuccess(SaveChanges);
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

			this.OnChangeSetSuccess(SaveChanges);
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

			this.OnChangeSetSuccess(SaveChanges);
		}

		#endregion
		#region Navigation properties

		public override CreatedItemResult<TProperty> PostNavigationProperty<TProperty>(TKey key, string navigationProperty, TProperty propertyEntity)
		{
			TEntity entity = DbSet.Find(key);
			if (entity == null)
			{
				string error = string.Format("Entity lookup failed for key {0} in {1}", key, DbSet);
				throw new ArgumentException(error, "key");
			}

			return PostNavigationProperty(entity, navigationProperty, propertyEntity);
		}

		public override CreatedItemResult<TProperty> PostNavigationProperty<TProperty>([ModelBinder(typeof(ChangeSetEntityModelBinder))] TEntity entity, string navigationProperty, TProperty propertyEntity)
		{
			IEdmNavigationProperty edmNavigationProperty = GenericNavigationPropertyRoutingConvention.GetNavigationProperty(Request.GetODataPath());
			Contract.Assert(navigationProperty == edmNavigationProperty.Name);

			// Add the new propertyEntity to the appropriate DbSet; Find its EntitySet first
			//IEdmEntityType edmEntityType = edmNavigationProperty.ToEntityType();
			//IEntitySetMetadata entitySetMetadata = ContainerMetadata.GetEntitySetFor(edmEntityType);
			//if (entitySetMetadata == null)
			//{
			//	throw new InvalidOperationException("Unable to find the entityset for entity type " + edmEntityType.ToTraceString());
			//}
			//Db.AddEntity(entitySetMetadata.Name, propertyEntity);

			if (edmNavigationProperty.Type.IsCollection())
			{
				object propertyCollection = entity.GetPropertyValue(navigationProperty);
				propertyCollection.InvokeMethod("Add", propertyEntity);
			}
			else
			{
				entity.SetPropertyValue(navigationProperty, propertyEntity);
			}

			this.OnChangeSetSuccess(SaveChanges);

			return Created(propertyEntity);
		}


		#endregion
		#region Support for Create-Update-Delete operations on links

		public override void CreateLink(TKey key, string navigationProperty, [FromBody] Uri link)
		{
			TEntity entity = DbSet.Find(key);
			if (entity == null)
			{
				string error = string.Format("Entity lookup failed for key {0} in {1}", key, DbSet);
				throw new ArgumentException(error, "key");
			}

			CreateLink(entity, navigationProperty, link);
		}

		public override void CreateLink([ModelBinder(typeof(ChangeSetEntityModelBinder))] TEntity entity, string navigationProperty, [FromBody] Uri link)
		{
			IEdmNavigationProperty edmNavigationProperty = GenericNavigationPropertyRoutingConvention.GetNavigationProperty(Request.GetODataPath());
			Contract.Assert(navigationProperty == edmNavigationProperty.Name);

			// Fetch the linked object either via a ChangeSet/Content-ID reference, or by fetching it from the database.
			object linkedObject = null;
			if (! Request.ContentIdReferenceToEntity(link.OriginalString, out linkedObject))
			{
				linkedObject = GetEntityForLink(link);
			}
			if (linkedObject == null)
			{
				throw new ArgumentException(string.Format("Link: {0} could not be resolved to an entity", link), "link");
			}

			if (edmNavigationProperty.Type.IsCollection())
			{
				object propertyCollection = entity.GetPropertyValue(navigationProperty);
				propertyCollection.InvokeMethod("Add", linkedObject);
			}
			else
			{
				entity.SetPropertyValue(navigationProperty, linkedObject);
			}

			this.OnChangeSetSuccess(SaveChanges);
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
