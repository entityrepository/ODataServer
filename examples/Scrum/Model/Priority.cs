// -----------------------------------------------------------------------
// <copyright file="Priority.cs" company="EntityRepository Contributors" year="2013">
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


	public sealed class Priority : NamedDbEnum<short, Priority>
	{

		/// <summary>
		/// Function to obtain the key for a <see cref="Status"/>.
		/// </summary>
		public static readonly Func<Priority, short> KeyFunc = priority => priority.ID;

		// Needed for WCF Data Services; also requires public setters. :(

		public static readonly Priority Unknown = new Priority(0, "Unknown");
		public static readonly Priority Optional = new Priority(1, "Optional");
		public static readonly Priority Low = new Priority(2, "Low");
		public static readonly Priority Normal = new Priority(3, "Normal");
		public static readonly Priority High = new Priority(4, "High");
		public static readonly Priority Critical = new Priority(5, "Critical");
		public static readonly Priority Blocking = new Priority(6, "Blocking");

		// Needed for deserialization
		public Priority()
		{}

		private Priority(short id, string name)
			: base(id, name)
		{}

		public static IEnumerable<Priority> All
		{
			get { return DbEnumManager.GetValues<short, Priority>(); }
		}
	}

}
