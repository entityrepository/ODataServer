// -----------------------------------------------------------------------
// <copyright file="WorkItemPropertyChange.cs" company="EntityRepository Contributors" year="2013">
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

	public class WorkItemPropertyChange : BaseEntity<long, WorkItemPropertyChange>
	{

		private RequiredEntityRef<WorkItem, int> _workItem = new RequiredEntityRef<WorkItem, int>(workItem => workItem.ID);
		private RequiredEntityRef<User, int> _author = new RequiredEntityRef<User, int>(user => user.ID);

		public WorkItem WorkItem
		{
			get { return _workItem.Entity; }
			set { _workItem.Entity = value; }
		}
		public int WorkItemId
		{
			get { return _workItem.ForeignKey; }
			set { _workItem.ForeignKey = value; }
		}

		public User Author
		{
			get { return _author.Entity; }
			set { _author.Entity = value; }
		}
		public int AuthorId
		{
			get { return _author.ForeignKey; }
			set { _author.ForeignKey = value; }
		}

		[Required]
		public DateTime ChangeDateTime { get; set; }

		[Required, StringLength(64, MinimumLength = 2)]
		public string PropertyName { get; set; }

		[Required, StringLength(255, MinimumLength = 1)]
		public string PriorValue { get; set; }

		[Required, StringLength(255, MinimumLength = 1)]
		public string ChangeToValue { get; set; }

	}
}
