// -----------------------------------------------------------------------
// <copyright file="WorkItemVersion.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using Scrum.Model.Base;

namespace Scrum.Model
{

	/// <summary>
	/// An example entity with multiple keys - just to test the "more than one PK column" case.
	/// </summary>
	public sealed class WorkItemVersion
	{

		private RequiredEntityRef<WorkItem, int> _workItem = new RequiredEntityRef<WorkItem, int>(workItem => workItem.ID);

		public WorkItemVersion(WorkItem workItem, byte version)
		{
			WorkItem = workItem;
			Version = version;
		}

		/// <summary>
		/// Useful for deserialization.
		/// </summary>
		public WorkItemVersion()
		{}

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

		public byte Version { get; set; }
	}

}