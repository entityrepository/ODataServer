// -----------------------------------------------------------------------
// <copyright file="EditDbSetController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Web.Http.OData.Query;

namespace EntityRepository.ODataServer
{
	/// <summary></summary>
	/// <typeparam name="TEntity">The type associated with the exposed entity set's entity type.</typeparam>
	/// <typeparam name="TKey">The type associated with the entity key of the exposed entity set's entity type.</typeparam>
	/// <typeparam name="TDbContext">The <see cref="DbContext"/> type containing the DbSet.</typeparam>
	public class EditDbSetController<TEntity, TKey, TDbContext>
		: ReadOnlyDbSetController<TEntity, TKey, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
	{

		public EditDbSetController(Lazy<TDbContext> lazyDbContext, EntityKeyHelper<TDbContext> entityKeyHelper, ODataValidationSettings queryValidationSettings)
			: base(lazyDbContext, entityKeyHelper, queryValidationSettings)
		{}

	}
}
