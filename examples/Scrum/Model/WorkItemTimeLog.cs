// -----------------------------------------------------------------------
// <copyright file="WorkItemTimeLog.cs" company="EntityRepository Contributors" year="2013">
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


	public class WorkItemTimeLog : BaseEntity<long, WorkItemTimeLog>
	{

		private RequiredEntityRef<WorkItem, int> _workItem = new RequiredEntityRef<WorkItem, int>(workItem => workItem.ID);
		private RequiredEntityRef<User, int> _worker = new RequiredEntityRef<User, int>(user => user.ID);

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

		public User Worker
		{
			get { return _worker.Entity; }
			set { _worker.Entity = value; }
		}
		public int WorkerId
		{
			get { return _worker.ForeignKey; }
			set { _worker.ForeignKey = value; }
		}

		public TimeSpan TimeWorked { get; set; }

		public DateTime? StartDateTime { get; set; }
		public DateTime? EndDateTime { get; set; }

		public string Comments { get; set; }

	}
}
