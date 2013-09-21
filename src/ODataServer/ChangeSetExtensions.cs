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
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Batch;

namespace EntityRepository.ODataServer
{
	/// <summary>
	/// Extension methods for <see cref="ODataController"/> subclasses to handle OData changesets.
	/// </summary>
	public static class ChangeSetExtensions
	{

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
				await completionTask;
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

		public static bool InChangeset(this ODataController oDataController)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);

			object changeSetObject;
			return oDataController.Request.Properties.TryGetValue(ChangeSetContextKey, out changeSetObject)
				   && changeSetObject is ChangeSetContext;
		}

		#endregion

		#region Internal and private static methods

		internal const string ChangeSetContextKey = "EntityRepository.ChangeSet";

		private static ChangeSetContext GetChangeSetContext(this HttpRequestMessage requestMessage)
		{
			object objChangeSetContext;
			if (requestMessage.Properties.TryGetValue(ChangeSetContextKey, out objChangeSetContext))
			{
				return objChangeSetContext as ChangeSetContext;
			}
			else
			{
				return null;
			}
		}

		internal static void SetUpChangeSetContext(this ChangeSetRequestItem changeSetRequest, HttpRequestMessage parentHttpRequestMessage)
		{
			Contract.Requires<ArgumentNullException>(changeSetRequest != null);
			Contract.Requires<ArgumentNullException>(parentHttpRequestMessage != null);
			
			// Create a single dependency scope to be shared amongst all the requests in the Changeset
			IDependencyResolver dependencyResolver = parentHttpRequestMessage.GetConfiguration().DependencyResolver;
			IDependencyScope dependencyScope = dependencyResolver.BeginScope();

			// Create a single ChangeSetContext to be shared amongst all the requests in the Changeset
			ChangeSetContext changeSetContext = new ChangeSetContext();

			foreach (HttpRequestMessage subrequest in changeSetRequest.Requests)
			{
				// Store the shared ChangeSetContext and the shared DependencyScope in each subrequest
				subrequest.Properties[HttpPropertyKeys.DependencyScope] = dependencyScope;
				subrequest.Properties[ChangeSetContextKey] = changeSetContext;
			}
		}

		internal static async Task ExecuteChangeSetCompletionActions(this ChangeSetResponseItem changeSetResponse)
		{
			Contract.Requires<ArgumentNullException>(changeSetResponse != null);

			HttpResponseMessage firstResponseMessage = changeSetResponse.Responses.FirstOrDefault();
			if (firstResponseMessage == null)
			{
				// No responses
				return;
			}

			// Get the ChangeSetContext
			ChangeSetContext changeSetContext = firstResponseMessage.RequestMessage.GetChangeSetContext();
			if (changeSetContext == null)
			{
				// Not in a ChangeSet; just return
				return;
			}

			// Per ChangeSetRequestItem.SendRequestAsync, if there are any errors the successful responses are removed.
			// Therefore the changeset fails if the first response is not successful
			if (firstResponseMessage.IsSuccessStatusCode)
			{
				await changeSetContext.AsyncExecuteSuccessActions();
			}
			else
			{
				await changeSetContext.AsyncExecuteFailureActions();
			}
		}

		#endregion


		internal class ChangeSetContext
		{
			private List<Task> _onSuccessTasks = new List<Task>();
			private List<Task> _onFailureTasks = new List<Task>();

			public void AddOnChangeSetSuccessTask(Task onSuccessTask)
			{
				lock (this)
				{
					_onSuccessTasks.Add(onSuccessTask);
				}
			}

			public void AddOnChangeSetFailureTask(Task onFailureTask)
			{
				lock (this)
				{
					_onFailureTasks.Add(onFailureTask);
				}
			}

			public async Task AsyncExecuteSuccessActions()
			{
				foreach (Task task in _onSuccessTasks)
				{
					await task;
				}
			}

			public async Task AsyncExecuteFailureActions()
			{
				foreach (Task task in _onFailureTasks)
				{
					await task;
				}
			}

		}

	}
}