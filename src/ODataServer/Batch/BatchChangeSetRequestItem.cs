// -----------------------------------------------------------------------
// <copyright file="BatchChangeSetRequestItem.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Batch;
using EntityRepository.ODataServer.Results;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer.Batch
{
	/// <summary>
	/// Customizes the behavior of changeset processing within a batch request to support intra-changeset entity references
	/// and make changesets atomic.
	/// </summary>
	/// <remarks>
	/// The main change is removing the <c>contentIdToLocationMapping</c> dictionary, and replacing it with
	/// <see cref="ChangeSetContext"/>.
	/// </remarks>
	public class BatchChangeSetRequestItem : ChangeSetRequestItem
	{

		private readonly HttpRequestMessage _parentRequest;
		private readonly ChangeSetContext _changeSetContext;

		public BatchChangeSetRequestItem(ChangeSetRequestItem copy, HttpRequestMessage parentRequest)
			: base(copy.Requests)
		{
			Contract.Requires<ArgumentNullException>(parentRequest != null);

			_parentRequest = parentRequest;
			_changeSetContext = new ChangeSetContext();
		}

		public override async Task<ODataBatchResponseItem> SendRequestAsync(HttpMessageInvoker invoker, CancellationToken cancellationToken)
		{
			if (invoker == null)
			{
				throw new ArgumentNullException("invoker");
			}

			List<HttpResponseMessage> responses = new List<HttpResponseMessage>();

			SetUpChangeSetContext(_parentRequest);

			ChangeSetResponseItem changeSetResponse;
			try
			{
				foreach (HttpRequestMessage request in Requests)
				{
					HttpResponseMessage response = await SendSubRequestAsync(invoker, request, cancellationToken);
					if (response.IsSuccessStatusCode)
					{
						responses.Add(response);
					}
					else
					{
						// In the case of an error response, just return the single error response
						responses.DisposeAll();
						responses.Clear();
						responses.Add(response);
						break;
					}
				}

				// Execute the changeset completion actions
				await ExecuteChangeSetCompletionActions(responses, cancellationToken);

				changeSetResponse = new ChangeSetResponseItem(responses);
			}
			catch
			{
				responses.DisposeAll();
				throw;
			}

			return changeSetResponse;
		}

		/// <summary>
		/// Sends a single OData changeset request.
		/// </summary>
		/// <param name="invoker">The invoker.</param>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> SendSubRequestAsync(HttpMessageInvoker invoker, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Modifies the request Uri if there's a Content-ID reference (eg http://localhost/odata/$46/Messages )
			ContentIdHelper.ResolveContentIdReferenceInRequestUrl(request);

			HttpResponseMessage response = await invoker.SendAsync(request, cancellationToken);
			ChangeSetExtensions.CopyContentIdHeaderToResponse(request, response);

			ChangeSetExtensions.StoreLocationHeaderForContentId(response, _changeSetContext);

			return response;
		}

		private void SetUpChangeSetContext(HttpRequestMessage parentHttpRequestMessage)
		{
			Contract.Requires<ArgumentNullException>(parentHttpRequestMessage != null);

			// Create a single dependency scope to be shared amongst all the requests in the Changeset
			IDependencyResolver dependencyResolver = parentHttpRequestMessage.GetConfiguration().DependencyResolver;
			IDependencyScope dependencyScope = dependencyResolver.BeginScope();
			parentHttpRequestMessage.RegisterForDispose(dependencyScope);

			// Create a single ChangeSetContext to be shared amongst all the requests in the Changeset
			parentHttpRequestMessage.RegisterForDispose(_changeSetContext);

			foreach (HttpRequestMessage subrequest in Requests)
			{
				// Store the shared ChangeSetContext and the shared DependencyScope in each subrequest
				subrequest.Properties[HttpPropertyKeys.DependencyScope] = dependencyScope;
				subrequest.Properties[ChangeSetContext.ChangeSetContextKey] = _changeSetContext;
			}
		}

		private async Task ExecuteChangeSetCompletionActions(List<HttpResponseMessage> responses, CancellationToken cancellationToken)
		{
			HttpResponseMessage firstResponseMessage = responses.FirstOrDefault();
			if (firstResponseMessage == null)
			{
				// No responses - so can't tell whether this was success or failure
				return;
			}

			// Per ChangeSetRequestItem.SendRequestAsync, if there are any errors the successful responses are removed.
			// Therefore the changeset fails if the first response is not successful
			if (firstResponseMessage.IsSuccessStatusCode)
			{
				await _changeSetContext.AsyncExecuteSuccessActions(cancellationToken);
			}
			else
			{
				await _changeSetContext.AsyncExecuteFailureActions(cancellationToken);
			}

			// Replace any PendingHttpResponseMessage objects with a "real" response
			for (int i = 0; i < responses.Count; ++i)
			{
				HttpResponseMessage httpResponse = responses[i];
				PendingHttpResponseMessage pendingHttpResponse = httpResponse as PendingHttpResponseMessage;
				if (pendingHttpResponse != null)
				{
					HttpResponseMessage finalResponse = await pendingHttpResponse.PendingResult.CreateFinalResponse(cancellationToken);
					responses[i] = finalResponse;
				}
			}
		}

	}
}