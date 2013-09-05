// -----------------------------------------------------------------------
// <copyright file="EntitySetController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer
{
	/// <summary>
	/// Based on <see cref="System.Web.Http.OData.EntitySetController{TEntity,TKey}"/>, but pulled into this project to allow modification.
	/// This is the synchronous version of <see cref="AsyncEntitySetController{TEntity,TKey}"/>.
	/// </summary>
	/// <typeparam name="TEntity">The type associated with the exposed entity set's entity type.</typeparam>
	/// <typeparam name="TKey">The type associated with the entity key of the exposed entity set's entity type.</typeparam>
	/// <remarks>
	/// Like <see cref="System.Web.Http.OData.EntitySetController{TEntity,TKey}"/>, this base class is independent of entity framework 
	/// concerns, so could be used for other query sources or persistence options.
	/// </remarks>
	[ODataNullValue]
	public abstract class EntitySetController<TEntity, TKey> : ODataController where TEntity : class
	{

		private readonly ODataValidationSettings _queryValidationSettings;

		#region Constructor

		protected EntitySetController(ODataValidationSettings queryValidationSettings)
		{
			_queryValidationSettings = queryValidationSettings;
		}

		#endregion

		#region Properties

		protected virtual ODataValidationSettings QueryValidationSettings
		{
			get { return _queryValidationSettings; }
		}

		#endregion

		#region HTTP controller methods

		/// <summary>
		/// Handles GET requests including query validation.
		/// </summary>
		/// <param name="queryOptions"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
		public virtual HttpResponseMessage Get(ODataQueryOptions<TEntity> queryOptions)
		{
			queryOptions.Validate(QueryValidationSettings);
			IQueryable queryApplied = queryOptions.ApplyTo(GetBaseQueryable());

			// TODO: Make this an async continuation?

			return Request.CreateResponseFromRuntimeType(HttpStatusCode.OK, queryApplied);
		}


		/// <summary>
		/// Handles GET requests that attempt to retrieve an individual entity by key from the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to retrieve.</param>
		/// <param name="queryOptions"></param>
		/// <returns>The response message to send back to the client.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
		public virtual HttpResponseMessage Get([FromODataUri] TKey key, ODataQueryOptions<TEntity> queryOptions)
		{
			queryOptions.Validate(QueryValidationSettings);

			IQueryable<TEntity> query = GetEntityByKeyQuery(key);
			IQueryable queryOptionsApplied = queryOptions.ApplyTo(query);

			// TODO: Do this as an async continuation

			// Get a single element
			return Request.CreateSingleEntityResponse(queryOptionsApplied);
		}

		// REVIEW: This used to work, but now doesn't (web api odata rc1).  Which is fine, b/c the implementation above is better.
		//[Queryable(AllowedQueryOptions = AllowedQueryOptions.All, MaxExpansionDepth = 15)]
		//public virtual SingleResult<TEntity> Get([FromODataUri] TKey key)
		//{
		//	return SingleResult.Create(GetEntityByKeyQuery(key));
		//}

		/// <summary>
		/// Handles POST requests that create new entities in the entity set.
		/// </summary>
		/// <param name="entity">The entity to insert into the entity set.</param>
		/// <returns>The response message to send back to the client.</returns>
		public virtual HttpResponseMessage Post([FromBody] TEntity entity)
		{
			Contract.Requires<ArgumentNullException>(entity != null);

			TEntity createdEntity = CreateEntity(entity);
			TKey entityKey = GetKey(entity);
			return EntitySetControllerHelpers.PostResponse(this, createdEntity, entityKey);
		}

		/// <summary>
		/// Handles PUT requests that attempt to replace a single entity in the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to replace.</param>
		/// <param name="update">The updated entity.</param>
		/// <returns>The response message to send back to the client.</returns>
		public virtual HttpResponseMessage Put([FromODataUri] TKey key, [FromBody] TEntity update)
		{
			Contract.Requires<ArgumentNullException>(update != null);

			TEntity updatedEntity = UpdateEntity(key, update);
			return EntitySetControllerHelpers.PutResponse(Request, updatedEntity);
		}

		/// <summary>
		/// Handles PATCH and MERGE requests to partially update a single entity in the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to update.</param>
		/// <param name="patch">The patch representing the partial update.</param>
		/// <returns>The response message to send back to the client.</returns>
		[AcceptVerbs("PATCH", "MERGE")]
		[SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Patch is the action name by WebAPI convention.")]
		public virtual HttpResponseMessage Patch([FromODataUri] TKey key, Delta<TEntity> patch)
		{
			Contract.Requires<ArgumentNullException>(patch != null);

			TEntity patchedEntity = PatchEntity(key, patch);
			return EntitySetControllerHelpers.PatchResponse(Request, patchedEntity);
		}

		/// <summary>
		/// This method should be overriden to handle DELETE requests for deleting existing entities from the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to delete.</param>
		public virtual void Delete([FromODataUri] TKey key)
		{
			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "DELETE");
		}

		/// <summary>
		/// This method should be overridden to handle POST and PUT requests that attempt to create a link between two entities.
		/// </summary>
		/// <param name="key">The key of the entity with the navigation property.</param>
		/// <param name="navigationProperty">The name of the navigation property.</param>
		/// <param name="link">The URI of the entity to link.</param>
		[AcceptVerbs("POST", "PUT")]
		public virtual void CreateLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
		{
			Contract.Requires<ArgumentNullException>(navigationProperty != null);
			Contract.Requires<ArgumentNullException>(link != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "Create Link");
		}

		/// <summary>
		/// This method should be overridden to handle DELETE requests that attempt to break a relationship between two entities.
		/// </summary>
		/// <param name="key">The key of the entity with the navigation property.</param>
		/// <param name="navigationProperty">The name of the navigation property.</param>
		/// <param name="link">The URI of the entity to remove from the navigation property.</param>
		public virtual void DeleteLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
		{
			Contract.Requires<ArgumentNullException>(navigationProperty != null);
			Contract.Requires<ArgumentNullException>(link != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "DELETE Link");
		}

		/// <summary>
		/// This method should be overridden to handle DELETE requests that attempt to break a relationship between two entities.
		/// </summary>
		/// <param name="key">The key of the entity with the navigation property.</param>
		/// <param name="relatedKey">The key of the related entity.</param>
		/// <param name="navigationProperty">The name of the navigation property.</param>
		public virtual void DeleteLink([FromODataUri] TKey key, string relatedKey, string navigationProperty)
		{
			Contract.Requires<ArgumentNullException>(relatedKey != null);
			Contract.Requires<ArgumentNullException>(navigationProperty != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "DELETE Link");
		}

		/// <summary>
		/// This method should be overridden to handle all unmapped OData requests.
		/// </summary>
		/// <param name="odataPath">The OData path of the request.</param>
		/// <returns>The response message to send back to the client.</returns>
		[AcceptVerbs("GET", "POST", "PUT", "PATCH", "MERGE", "DELETE")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly.")]
		public virtual HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
		{
			throw EntitySetControllerHelpers.UnmappedRequestResponse(this, odataPath);
		}

		#endregion

		#region Overridable methods

		/// <summary>
		/// This method must be overridden to support GET requests.
		/// </summary>
		/// <returns></returns>
		protected virtual IQueryable<TEntity> GetBaseQueryable()
		{
			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "GET");
		}

		/// <summary>
		/// This method should be overridden to get the entity key of the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>The entity key value</returns>
		protected virtual TKey GetKey(TEntity entity)
		{
			Contract.Requires<ArgumentNullException>(entity != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "GetKey(TEntity entity)");
		}

		/// <summary>
		/// This method should be overridden to retrieve an entity by key from the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to retrieve.</param>
		/// <returns>The retrieved entity, or <c>null</c> if an entity with the specified entity key cannot be found in the entity set.</returns>
		protected virtual IQueryable<TEntity> GetEntityByKeyQuery(TKey key)
		{
			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "GET entity by key");
		}

		/// <summary>
		/// This method should be overridden to create a new entity in the entity set.
		/// </summary>
		/// <param name="entity">The entity to add to the entity set.</param>
		/// <returns>The created entity.</returns>
		protected internal virtual TEntity CreateEntity(TEntity entity)
		{
			Contract.Requires<ArgumentNullException>(entity != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "POST");
		}

		/// <summary>
		/// This method should be overridden to update an existing entity in the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to update.</param>
		/// <param name="update">The updated entity.</param>
		/// <returns>The updated entity.</returns>
		protected internal virtual TEntity UpdateEntity(TKey key, TEntity update)
		{
			Contract.Requires<ArgumentNullException>(update != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "PUT");
		}

		/// <summary>
		/// This method should be overridden to apply a partial update to an existing entity in the entity set.
		/// </summary>
		/// <param name="key">The entity key of the entity to update.</param>
		/// <param name="patch">The patch representing the partial update.</param>
		/// <returns>The updated entity.</returns>
		protected internal virtual TEntity PatchEntity(TKey key, Delta<TEntity> patch)
		{
			Contract.Requires<ArgumentNullException>(patch != null);

			throw EntitySetControllerHelpers.NotImplementedResponseException(this, "PATCH");
		}

		#endregion
	}
}
