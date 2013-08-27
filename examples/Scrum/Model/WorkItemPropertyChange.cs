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

		[Required]
		public WorkItem WorkItem { get; set; }

		public int WorkItemID { get; set; }

		[Required]
		public User Author { get; set; }

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
