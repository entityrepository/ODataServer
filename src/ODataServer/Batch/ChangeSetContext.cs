// -----------------------------------------------------------------------
// <copyright file="ChangeSetContext.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer.Batch
{
	/// <summary>
	/// One <c>ChangeSetContext</c> exists per changeset.  It holds the objects that
	/// can be shared between actions in the same changeset.
	/// </summary>
	/// <remarks>
	/// See <a href="http://www.odata.org/documentation/odata-v3-documentation/batch-processing/">OData Batch Processing spec</a> for more info
	/// on batch processing and changesets.
	/// <para></para>
	/// </remarks>
	internal sealed class ChangeSetContext : IDisposable
	{

		internal const string ChangeSetContextKey = "EntityRepository.ChangeSet";

		private readonly List<ContentIdRecord> _contentIdRecords = new List<ContentIdRecord>();
		private readonly List<Task> _onSuccessTasks = new List<Task>();
		private readonly List<Task> _onFailureTasks = new List<Task>();
		private bool _disposed = false;

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

		public Task AsyncExecuteSuccessActions(CancellationToken cancellationToken)
		{
			return AsyncExecuteCompletionTasks(_onSuccessTasks, "success actions", cancellationToken);
		}

		public Task AsyncExecuteFailureActions(CancellationToken cancellationToken)
		{
			return AsyncExecuteCompletionTasks(_onFailureTasks, "failure actions", cancellationToken);
		}

		private async Task AsyncExecuteCompletionTasks(IEnumerable<Task> completionTasks, string description, CancellationToken cancellationToken)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(this.ToString());
			}

			List<Exception> exceptions = new List<Exception>();
			foreach (Task task in completionTasks)
			{
				cancellationToken.ThrowIfCancellationRequested();
				try
				{
					await task.EnsureStarted();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			if (exceptions.Count > 0)
			{
				throw new AggregateException("Exceptions occurred while executing ChangeSetContext " + description, exceptions);
			}
		}

		internal void AddContentIdRecord(int requestContentId, HttpRequestMessage requestMessage, object entity, Type entityType)
		{
			lock (this)
			{
				_contentIdRecords.Add(new ContentIdRecord(requestContentId, entity, entityType, requestMessage));
			}
		}

		internal ContentIdRecord GetContentIdRecord(int contentId)
		{
			lock (this)
			{
				return _contentIdRecords.SingleOrDefault(rec => rec.ContentId == contentId);
			}
		}

		public void Dispose()
		{
			lock (this)
			{
				if (!_disposed)
				{
					_disposed = true;
					_onSuccessTasks.DisposeAll();
					_onFailureTasks.DisposeAll();
					_contentIdRecords.DisposeAll(contentIdRecord => contentIdRecord.Entity);
				}
			}
		}

	}

	/// <summary>
	/// Tracks the entity associated with each 'Content-Id' value.
	/// </summary>
	internal sealed class ContentIdRecord
	{
		internal ContentIdRecord(int contentId, object entity, Type entityType, HttpRequestMessage request)
		{
			ContentId = contentId;
			Entity = entity;
			EntityType = entityType;
			Request = request;
		}

		public int ContentId { get; private set; }
		public object Entity { get; private set; }
		public Type EntityType { get; private set; }
		public HttpRequestMessage Request { get; private set; }
		/// <summary>
		/// The value portion of the Location header.
		/// </summary>
		public string Location { get; set; }

	}

}