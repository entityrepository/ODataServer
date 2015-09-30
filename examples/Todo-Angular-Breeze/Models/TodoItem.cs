// -----------------------------------------------------------------------
// <copyright file="TodoItem.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODataBreezejsSample.Models
{
	public class TodoItem
	{

		public virtual int Id { get; set; }
		public virtual int TodoListId { get; set; }
		public virtual TodoList TodoList { get; set; }

		[Required, MaxLength(60)]
		public virtual string Description { get; set; }

		[DefaultValue(false)]
		public virtual bool? IsDone { get; set; }

	}
}
