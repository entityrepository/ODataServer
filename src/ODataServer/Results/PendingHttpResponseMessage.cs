// -----------------------------------------------------------------------
// <copyright file="PendingHttpResponseMessage.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EntityRepository.ODataServer.Results
{
	/// <summary>
	/// Used to identify a pending HTTP response from an action being completed within a changeset.
	/// The pending response will be replaced by a real response after the changeset has completed.
	/// </summary>
	internal class PendingHttpResponseMessage : HttpResponseMessage
	{

		private const string c_pendingResponseReason = "Action method completed; pending changeset completion.";

		private readonly IDelayedActionResult _pendingResult;

		internal PendingHttpResponseMessage(HttpRequestMessage requestMessage, IDelayedActionResult pendingResult)
			: base(HttpStatusCode.Accepted)
		{
			Contract.Assert(requestMessage != null);
			Contract.Assert(pendingResult != null);

			RequestMessage = requestMessage;
			_pendingResult = pendingResult;
			ReasonPhrase = c_pendingResponseReason;
		}

		internal IDelayedActionResult PendingResult
		{ get { return _pendingResult; } }
	}
}