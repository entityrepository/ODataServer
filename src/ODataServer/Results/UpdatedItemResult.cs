// -----------------------------------------------------------------------
// <copyright file="UpdatedItemResult.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace EntityRepository.ODataServer.Results
{
	/// <summary>
	/// An action result for an entity that is updated within a changeset, or updated outside of a changeset.
	/// </summary>
	/// <typeparam name="T">The entity type.</typeparam>
	/// <remarks>This action result handles content negotiation and the HTTP prefer header.</remarks>
	public class UpdatedItemResult<T> : ChangeSetItemResult<T>
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdatedItemResult{T}"/> class.
		/// </summary>
		/// <param name="entity">The created entity.</param>
		/// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
		public UpdatedItemResult(T entity, ApiController controller)
			: base(entity, HttpStatusCode.OK, controller)
		{}
		 
		/// <summary>
		/// Initializes a new instance of the <see cref="UpdatedItemResult{T}"/> class.
		/// </summary>
		/// <param name="entity">The created entity.</param>
		/// <param name="contentNegotiator">The content negotiator to handle content negotiation.</param>
		/// <param name="request">The request message which led to this result.</param>
		/// <param name="formatters">The formatters to use to negotiate and format the content.</param>

		public UpdatedItemResult(T entity, IContentNegotiator contentNegotiator, HttpRequestMessage request,
			IEnumerable<MediaTypeFormatter> formatters)
			: base(entity, HttpStatusCode.OK, contentNegotiator, request, formatters)
		{}
		 

	}
}