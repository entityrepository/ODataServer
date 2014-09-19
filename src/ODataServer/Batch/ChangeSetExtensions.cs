// -----------------------------------------------------------------------
// <copyright file="ChangeSetExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
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
using System.Threading.Tasks;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer.Batch
{
	/// <summary>
	/// Extension methods for <see cref="ODataController"/> subclasses to handle OData changesets.
	/// </summary>
	public static class ChangeSetExtensions
	{
		/// <summary>
		/// The name of the Content-ID header.
		/// </summary>
		public const string ContentIdHeaderName = "Content-ID";

		private const string c_initialRequestUriKey = "EntityRepository.InitialChangeSetRequestUri";

		#region Public ODataController extension methods

		/// <summary>
		/// Set the synchronous function to call when the changeset is complete.
		/// </summary>
		/// <param name="oDataController"></param>
		/// <param name="completionFunction"></param>
		/// <returns></returns>
		/// <remarks>
		/// This method can result in the completionFunction being called, so it can block for a while.
		/// </remarks>
		//public static HttpResponseMessage OnChangeSetSuccess(this ODataController oDataController, Func<HttpResponseMessage> completionFunction)
		//{
		//	Contract.Requires<ArgumentNullException>(oDataController != null);
		//	Contract.Requires<ArgumentNullException>(completionFunction != null);

		//	return OnChangeSetSuccess(oDataController, new Task<HttpResponseMessage>(completionFunction)).Result;
		//}
		public static void OnChangeSetSuccess(this ODataController oDataController, Action completionFunction)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);
			Contract.Requires<ArgumentNullException>(completionFunction != null);

			OnChangeSetSuccess(oDataController, new Task(completionFunction)).Wait();
		}

		/// <summary>
		/// Set the async task to call when the changeset is complete, if the current request is within a changeset.
		/// If the current request is not within a changeset, <paramref name="onSuccessTask"/> is run immediately.
		/// </summary>
		/// <param name="oDataController"></param>
		/// <param name="completionTask"></param>
		//public static Task<HttpResponseMessage> OnChangeSetSuccess(this ODataController oDataController, Task<HttpResponseMessage> completionTask)
		//{
		//	Contract.Requires<ArgumentNullException>(oDataController != null);
		//	Contract.Requires<ArgumentNullException>(completionTask != null);

		//	ChangeSetContext changeSetContext = oDataController.Request.GetChangeSetContext();
		//	if (changeSetContext == null)
		//	{	// Not in a ChangeSet, so execute completionTask immediately
		//		return completionTask;
		//	}
		//	else
		//	{
		//		changeSetContext.AddOnChangeSetSuccessTask(completionTask);
		//		return new Task<HttpResponseMessage>(() => new HttpResponseMessage(HttpStatusCode.Accepted) 
		//														{ Content = new StringContent("Changeset action method called; pending completion task.") });
		//	}
		//}
		public static async Task OnChangeSetSuccess(this ODataController oDataController, Task completionTask)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);
			Contract.Requires<ArgumentNullException>(completionTask != null);

			ChangeSetContext changeSetContext = oDataController.Request.GetChangeSetContext();
			if (changeSetContext == null)
			{	// Not in a ChangeSet, so execute completionTask immediately
				await completionTask.EnsureStarted();
			}
			else
			{
				changeSetContext.AddOnChangeSetSuccessTask(completionTask);
			}
		}

		public static void OnChangeSetFailure(this ODataController oDataController, Action onFailureAction)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);
			Contract.Requires<ArgumentNullException>(onFailureAction != null);

			OnChangeSetFailure(oDataController, new Task(onFailureAction));
		}

		public static void OnChangeSetFailure(this ODataController oDataController, Task onFailureTask)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);
			Contract.Requires<ArgumentNullException>(onFailureTask != null);

			ChangeSetContext changeSetContext = oDataController.Request.GetChangeSetContext();
			if (changeSetContext != null)
			{
				changeSetContext.AddOnChangeSetFailureTask(onFailureTask);
			}
		}

		public static bool InChangeSet(this ODataController oDataController)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);

			return oDataController.Request.InChangeSet();
		}

		public static bool InChangeSet(this HttpRequestMessage request)
		{
			Contract.Requires<ArgumentNullException>(request != null);

			object changeSetObject;
			return request.Properties.TryGetValue(ChangeSetContext.ChangeSetContextKey, out changeSetObject)
				   && changeSetObject is ChangeSetContext;
		}

		public static int? ContentId(this HttpRequestMessage request)
		{
			IEnumerable<string> headerValues;
			int requestContentId;
			if (!request.Headers.TryGetValues(ContentIdHeaderName, out headerValues)
				|| !int.TryParse(headerValues.First(), out requestContentId))
			{
				return null;
			}

			return requestContentId;
		}

		public static void TrySetChangeSetContentIdEntity<TEntity>(this HttpRequestMessage request, TEntity entity)
		{
			int? requestContentId = request.ContentId();
			ChangeSetContext changeSetContext = request.GetChangeSetContext();
			if ((changeSetContext == null)
				|| (!requestContentId.HasValue))
			{
				return;
			}

			changeSetContext.AddContentIdRecord(requestContentId.Value, request, entity, typeof(TEntity));
		}

		internal static void StoreLocationHeaderForContentId(HttpResponseMessage response, ChangeSetContext changeSetContext)
		{
			int? requestContentId = response.RequestMessage.ContentId();
			if (requestContentId.HasValue)
			{
				if (response.Headers.Location != null)
				{
					changeSetContext.GetContentIdRecord(requestContentId.Value).Location = response.Headers.Location.AbsoluteUri;
				}
			}
		}

		public static bool ContentIdReferenceToEntity(this HttpRequestMessage request, string reference, out object referencedEntity)
		{
			referencedEntity = null;
			int contentId;
			if (! ContentIdHelper.TryParseContentIdReference(reference, out contentId))
			{
				return false;
			}

			ChangeSetContext changeSetContext = request.GetChangeSetContext();
			if (changeSetContext != null)
			{
				ContentIdRecord contentIdRecord = changeSetContext.GetContentIdRecord(contentId);
				if (contentIdRecord != null)
				{
					referencedEntity = contentIdRecord.Entity;
				}
			}

			return referencedEntity != null;
		}

		#endregion

		#region Internal and private helper methods

		internal static ChangeSetContext GetChangeSetContext(this HttpRequestMessage requestMessage)
		{
			object objChangeSetContext;
			if (requestMessage.Properties.TryGetValue(ChangeSetContext.ChangeSetContextKey, out objChangeSetContext))
			{
				return objChangeSetContext as ChangeSetContext;
			}
			else
			{
				return null;
			}
		}

		internal static void SaveInitialChangeSetRequestUri(this HttpRequestMessage requestMessage)
		{
			requestMessage.Properties.Add(c_initialRequestUriKey, requestMessage.RequestUri.OriginalString);
		}

		internal static string GetInitialChangeSetRequestUri(this HttpRequestMessage requestMessage)
		{
			object value;
			if (requestMessage.Properties.TryGetValue(c_initialRequestUriKey, out value))
			{
				return value as string;
			}
			else
			{
				return requestMessage.RequestUri.OriginalString;
			}
		}

		internal static void CopyContentIdHeaderToResponse(HttpRequestMessage request, HttpResponseMessage response)
		{
			Contract.Assert(request != null);
			Contract.Assert(response != null);

			IEnumerable<string> values;
			if (request.Headers.TryGetValues(ContentIdHeaderName, out values)
				&& !response.Headers.Contains(ContentIdHeaderName))
			{
				response.Headers.TryAddWithoutValidation(ContentIdHeaderName, values);
			}
		}

		#endregion

	}
}