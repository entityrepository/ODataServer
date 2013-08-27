// -----------------------------------------------------------------------
// <copyright file="Sprint.cs" company="EntityRepository Contributors" year="2013">
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


	public class Sprint : BaseEntity<int, Sprint>
	{

		public DateTime? EndDate { get; set; }

		[StringLength(64)]
		public string Name { get; set; }

		public DateTime? StartDate { get; set; }

	}
}
