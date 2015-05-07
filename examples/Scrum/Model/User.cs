// -----------------------------------------------------------------------
// <copyright file="User.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Scrum.Model.Base;

namespace Scrum.Model
{


	public class User : BaseEntity<int, User>
	{

		private OptionalEntityRef<UserGroup, int> _group;

		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		[StringLength(40, MinimumLength = 3)]
		public string UserName { get; set; }

		public UserGroup Group
		{
			get { return _group.Entity; }
			set { _group.Entity = value; }
		}

		[IgnoreDataMember]
		public int? GroupId
		{
			get { return _group.ForeignKey; }
			set { _group.ForeignKey = value; }
		}

	}
}
