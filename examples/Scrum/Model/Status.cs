// -----------------------------------------------------------------------
// <copyright file="Status.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Scrum.Model.Base;

namespace Scrum.Model
{

	/// <summary>
	/// The status of a work item.
	/// </summary>
	public class Status : NamedDbEnum<short, Status>
	{

		/// <summary>
		/// Function to obtain the key for a <see cref="Status"/>.
		/// </summary>
		public static readonly Func<Status, short> KeyFunc = status => status.ID;

		// Needed for WCF Data Services; also requires public setters. :(

		public static readonly Status Unknown = new Status(0, "Unknown");
		public static readonly Status Open = new Status(1, "Open");
		public static readonly Status WorkingOn = new Status(2, "WorkingOn");
		public static readonly Status DevComplete = new Status(3, "DevComplete");
		public static readonly Status QaComplete = new Status(4, "QaComplete");
		public static readonly Status Deployed = new Status(5, "Deployed");
		public static readonly Status Closed = new Status(6, "Closed");

		// Needed for deserialization
		public Status()
		{}

		private Status(short id, string name)
			: base(id, name)
		{}

		public static IEnumerable<Status> All
		{
			get { return DbEnumManager.GetValues<short, Status>(); }
		}

	}
}
