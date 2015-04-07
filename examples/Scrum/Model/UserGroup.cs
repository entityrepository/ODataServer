// // -----------------------------------------------------------------------
// <copyright file="UserGroup.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Collections.Generic;
using Scrum.Model.Base;

namespace Scrum.Model
{

	/// <summary>
	/// Groups users.
	/// </summary>
	public sealed class UserGroup : BaseEntity<int, UserGroup>
	{

		private ICollection<User> _users;

		public ICollection<User> Users
		{
			get { return EnsureCollectionProperty(ref _users); }
		}

	}

}