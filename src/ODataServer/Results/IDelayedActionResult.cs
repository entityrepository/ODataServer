// -----------------------------------------------------------------------
// <copyright file="IDelayedActionResult.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using EntityRepository.ODataServer.Batch;

namespace EntityRepository.ODataServer.Results
{
	/// <summary>
	/// Supports delaying creation of an <see cref="T:System.Net.Http.HttpResponseMessage"/> to later in the request handling.
	/// For example, <see cref="BatchChangeSetRequestItem"/> creates the responses for changeset requests after the 
	/// changeset success or failures actions are executed.
	/// </summary>
	public interface IDelayedActionResult : IHttpActionResult
	{

		/// <summary>
		/// Creates a final response <see cref="T:System.Net.Http.HttpResponseMessage"/> asynchronously.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		/// <returns>
		/// A task that, when completed, contains the <see cref="T:System.Net.Http.HttpResponseMessage"/>.
		/// </returns>
		Task<HttpResponseMessage> CreateFinalResponse(CancellationToken cancellationToken);

	}
}