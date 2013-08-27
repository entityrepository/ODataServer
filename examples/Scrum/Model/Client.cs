// -----------------------------------------------------------------------
// <copyright file="Client.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using Scrum.Model.Base;

namespace Scrum.Model
{


	public class Client : BaseEntity<int, Client>
	{

		[Required]
		[StringLength(60, MinimumLength = 1)]
		public string Name { get; set; }

		[StringLength(512)]
		public string Description { get; set; }

	}
}
