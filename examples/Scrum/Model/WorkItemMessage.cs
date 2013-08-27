// -----------------------------------------------------------------------
// <copyright file="WorkItemMessage.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using Scrum.Model.Base;

namespace Scrum.Model
{


	public class WorkItemMessage : BaseEntity<long, WorkItemMessage>
	{

		private EntityRef<WorkItem, int> _workItemEntityRef = new EntityRef<WorkItem, int>(workItem => workItem.ID);
		private EntityRef<User, int> _authorEntityRef = new EntityRef<User, int>(user => user.ID);

		public WorkItemMessage(WorkItem workItem, User author)
		{
			WorkItem = workItem;
			Author = author;
			Created = DateTime.Now;
		}

		/// <summary>
		/// Useful for deserialization.
		/// </summary>
		public WorkItemMessage()
		{}

		[Required]
		public WorkItem WorkItem
		{
			get { return _workItemEntityRef.Entity; }
			set { _workItemEntityRef.Entity = value; }
		}

		public int WorkItemId
		{
			get { return _workItemEntityRef.ForeignKey; }
			set { _workItemEntityRef.ForeignKey = value; }
		}

		[Required]
		public User Author
		{
			get { return _authorEntityRef.Entity; }
			set { _authorEntityRef.Entity = value; }
		}

		//public int AuthorId
		//{
		//	get { return _authorEntityRef.ForeignKey; }
		//	set { _authorEntityRef.ForeignKey = value; }
		//}

		[Required]
		public string Message { get; set; }

		[Required]
		public DateTime Created { get; set; }

		public DateTime? LastUpdated { get; set; }

	}
}
