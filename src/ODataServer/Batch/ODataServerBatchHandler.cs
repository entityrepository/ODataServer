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
using System.Web.OData.Batch;

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
			for (int i = 0; i < requestItems.Count; ++i)
			{
				ChangeSetRequestItem changeSetRequest = requestItems[i] as ChangeSetRequestItem;
				if (changeSetRequest != null)
				{
					// Replace the ChangeSetRequestItem with a BatchChangeSetRequestItem
					requestItems[i] = new BatchChangeSetRequestItem(changeSetRequest, parentRequest);
				}
			}

			return requestItems;
		}

	}
}