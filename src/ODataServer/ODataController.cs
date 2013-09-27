// -----------------------------------------------------------------------
// <copyright file="ODataController.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.OData;
using System.Web.Http.OData.Results;
using EntityRepository.ODataServer.Batch;
using EntityRepository.ODataServer.Routing;

namespace EntityRepository.ODataServer
{
	/// <summary>
	/// Defines a base class for OData controllers that support writing and reading data using the OData formats.
	/// </summary>
	/// <remarks>
	/// Based on <see cref="System.Web.Http.OData.ODataController"/>, but modified.
	/// </remarks>
	[ODataFormatting]
	[UseEntityRepositoryActionSelector]
	[ApiExplorerSettings(IgnoreApi = true)]
	public abstract class ODataController : ApiController
	{
		/// <summary>
		/// Creates an action result with the specified values that is a response to a POST operation with an entity 
		/// to an entity set.
		/// </summary>
		/// <typeparam name="TEntity">The created entity type.</typeparam>
		/// <param name="entity">The created entity.</param>
		/// <returns>A <see cref="CreatedODataResult{T}"/> with the specified values.</returns>
		protected virtual CreatedODataResult<TEntity> Created<TEntity>(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Request.TrySetChangeSetContentIdEntity(entity);

			return new CreatedODataResult<TEntity>(entity, this);
		}

		/// <summary>
		/// Creates an action result with the specified values that is a response to a PUT, PATCH, or a MERGE operation 
		/// on an OData entity.
		/// </summary>
		/// <typeparam name="TEntity">The updated entity type.</typeparam>
		/// <param name="entity">The updated entity.</param>
		/// <returns>An <see cref="UpdatedODataResult{TEntity}"/> with the specified values.</returns>
		protected virtual UpdatedODataResult<TEntity> Updated<TEntity>(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			return new UpdatedODataResult<TEntity>(entity, this);
		}
	}

}