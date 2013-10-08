// -----------------------------------------------------------------------
// <copyright file="Project.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Scrum.Model.Base;

namespace Scrum.Model
{


	public class Project : BaseEntity<int, Project>
	{
		#region Fields

		private string _key;

		private ICollection<User> _owners;
		private ICollection<ProjectArea> _areas;
		private ICollection<ProjectVersion> _versions;
		private ICollection<WorkItem> _workItems;

		#endregion

		[RegularExpression("^[A-Z]{2,20}$", ErrorMessage = "Project key must be all CAPs, 2 to 20 letters.")]
		[StringLength(20, MinimumLength = 1)]
		public string Key
		{
			get { return _key; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException("Value cannot be empty or whitespace.");
				}

				_key = value.Trim().ToUpperInvariant();
			}
		}

		[Required]
		[StringLength(100, MinimumLength = 2)]
		public string Name { get; set; }

		[StringLength(512)]
		public string Description { get; set; }

		public ICollection<User> Owners
		{
			get { return EnsureCollectionProperty(ref _owners); }
		}

		public ICollection<ProjectArea> Areas
		{
			get { return EnsureCollectionProperty(ref _areas); }
		}

		public ICollection<ProjectVersion> Versions
		{
			get { return EnsureCollectionProperty(ref _versions); }
		}

		public ICollection<WorkItem> WorkItems
		{
			get { return EnsureCollectionProperty(ref _workItems); }
		}

	}
}
