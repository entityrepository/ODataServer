// -----------------------------------------------------------------------
// <copyright file="ChangeSetItemResult.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Linq;
using EntityRepository.ODataServer.Batch;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace EntityRepository.ODataServer.Results
{
	/// <summary>
	/// An action result for an entity that is created or updated possibly within a changeset.
	/// </summary>
	/// <typeparam name="T">The entity type.</typeparam>
	/// <remarks>This action result handles content negotiation and the HTTP prefer header and generates a location header
	/// that is the same as the edit link of the created entity.</remarks>
	public abstract class ChangeSetItemResult<T> : IDelayedActionResult
	{

		/// <summary>
		/// Set to true if this result is returned within a response to an OData changeset request
		/// </summary>
		private readonly bool _inChangeSet;
		/// <summary>
		/// Performs the real conversion to an HttpResponseMessage.
		/// </summary>
		private readonly NegotiatedContentResult<T> _innerResult;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChangeSetItemResult{T}"/> class.
		/// </summary>
		/// <param name="entity">The created, updated, or deleted entity.</param>
		/// <param name="statusCode">The status code for the response.</param>
		/// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
		protected ChangeSetItemResult(T entity, HttpStatusCode statusCode, ApiController controller)
			: this(new NegotiatedContentResult<T>(statusCode, CheckNull(entity), controller))
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChangeSetItemResult{T}"/> class.
		/// </summary>
		/// <param name="entity">The created entity.</param>
		/// <param name="statusCode">The status code for the response.</param>
		/// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
		/// <param name="request">The request message which led to this result.</param>
		/// <param name="formatters">The formatters to use to negotiate and format the content.</param>
		public ChangeSetItemResult(T entity, HttpStatusCode statusCode, IContentNegotiator contentNegotiator, HttpRequestMessage request,
			IEnumerable<MediaTypeFormatter> formatters)
			: this(new NegotiatedContentResult<T>(HttpStatusCode.Created, CheckNull(entity), contentNegotiator, request, formatters))
		{}

		private ChangeSetItemResult(NegotiatedContentResult<T> innerResult)
		{
			Contract.Assert(innerResult != null);
			_innerResult = innerResult;
			_inChangeSet = innerResult.Request.InChangeSet();
		}

		/// <summary>
		/// Gets the entity that was created.
		/// </summary>
		public T Entity
		{
			get
			{
				return _innerResult.Content;
			}
		}

		/// <summary>
		/// Gets the content negotiator to handle content negotiation.
		/// </summary>
		public IContentNegotiator ContentNegotiator
		{
			get
			{
				return _innerResult.ContentNegotiator;
			}
		}

		/// <summary>
		/// Gets the request message which led to this result.
		/// </summary>
		public HttpRequestMessage Request
		{
			get
			{
				return _innerResult.Request;
			}
		}

		/// <summary>
		/// Gets the formatters to use to negotiate and format the created entity.
		/// </summary>
		public IEnumerable<MediaTypeFormatter> Formatters
		{
			get
			{
				return _innerResult.Formatters;
			}
		}

		/// <inheritdoc/>
		public virtual async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			// This method is called by the Web API framework to transfrom the IHttpActionResult into an HttpResponseMessage.
			// If in a changeset, return a pending result; BatchChangeSetRequestItem will call CreateFinalResponse().
			// If not in a changeset, return the final response.
			if (_inChangeSet)
			{
				var response = new PendingHttpResponseMessage(Request, this);
				SetPendingResponseHeaders(response);
				return response;
			}
			else
			{
				return await CreateFinalResponse(cancellationToken);
			}
		}

		public virtual async Task<HttpResponseMessage> CreateFinalResponse(CancellationToken cancellationToken)
		{
			IHttpActionResult result = GetInnerActionResult();
			HttpResponseMessage response = await result.ExecuteAsync(cancellationToken);
			SetFinalResponseHeaders(response);
			return response;
		}

		internal IHttpActionResult GetInnerActionResult()
		{
			if (EntitySetControllerHelpers.RequestPrefersReturnNoContent(Request))
			{
				return new StatusCodeResult(HttpStatusCode.NoContent, Request);
			}
			else
			{
				return _innerResult;
			}
		}

		protected virtual void SetPendingResponseHeaders(HttpResponseMessage response)
		{}

		protected virtual void SetFinalResponseHeaders(HttpResponseMessage response)
		{
			ChangeSetExtensions.CopyContentIdHeaderToResponse(response.RequestMessage, response);

			IEnumerable<string> preferences = null;
			if (Request.Headers.TryGetValues(EntitySetControllerHelpers.PreferHeaderName, out preferences))
			{
				if (preferences.Contains(EntitySetControllerHelpers.ReturnNoContentHeaderValue))
				{
					response.Headers.Add(EntitySetControllerHelpers.PreferenceAppliedHeaderName, EntitySetControllerHelpers.ReturnNoContentHeaderValue);
				}
				else if (preferences.Contains(EntitySetControllerHelpers.ReturnContentHeaderValue))
				{
					response.Headers.Add(EntitySetControllerHelpers.PreferenceAppliedHeaderName, EntitySetControllerHelpers.ReturnContentHeaderValue);
				}
			}
		}

		private static T CheckNull(T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			return entity;
		}
	}
}