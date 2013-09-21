// -----------------------------------------------------------------------
// <copyright file="ProjectArea.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Scrum.Model.Base;

namespace Scrum.Model
{


	public class ProjectArea : BaseEntity<int, ProjectArea>
	{
		#region Fields

		private ICollection<User> _owners;

		#endregion

		[StringLength(512)]
		public string Description { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 2)]
		public string Name { get; set; }

		public virtual ICollection<User> Owners
		{
			get { return EnsureCollectionProperty(ref _owners); }
			set { SetCollectionProperty(ref _owners, value); }
		}

		[Required]
		public virtual Project Project { get; set; }

	}
}
