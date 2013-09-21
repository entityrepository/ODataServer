// -----------------------------------------------------------------------
// <copyright file="ODataServerBatchHandler.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Batch;

namespace EntityRepository.ODataServer.Batch
{
	/// <summary>
	/// Adds some additional functionality to <see cref="DefaultODataBatchHandler"/>.
	/// </summary>
	public class ODataServerBatchHandler : DefaultODataBatchHandler
	{

		public ODataServerBatchHandler(HttpServer httpServer)
			: base(httpServer)
		{}

		public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage parentRequest, CancellationToken cancellationToken)
		{
			IList<ODataBatchRequestItem> requestItems = await base.ParseBatchRequestsAsync(parentRequest, cancellationToken);

			// For each ChangeSet in a batch request, set up a ChangeSetContext to support committing the entire ChangeSet at once.
			foreach (ODataBatchRequestItem oDataBatchRequestItem in requestItems)
			{
				ChangeSetRequestItem changeSetRequest = oDataBatchRequestItem as ChangeSetRequestItem;
				if (changeSetRequest != null)
				{
					changeSetRequest.SetUpChangeSetContext(parentRequest);
				}
			}

			return requestItems;
		}

		public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, CancellationToken cancellationToken)
		{
			IList<ODataBatchResponseItem> responseItems = await base.ExecuteRequestMessagesAsync(requests, cancellationToken);

			// For each ChangeSet in a batch request, invoke the success/failure handlers
			foreach (ODataBatchResponseItem oDataBatchResponseItem in responseItems)
			{
				ChangeSetResponseItem changeSetResponse = oDataBatchResponseItem as ChangeSetResponseItem;
				if (changeSetResponse != null)
				{
					await changeSetResponse.ExecuteChangeSetCompletionActions();
				}
			}

			return responseItems;
		}
	}
}