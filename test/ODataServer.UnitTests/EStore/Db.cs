// // -----------------------------------------------------------------------
// <copyright file="Db.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Data.Entity;

namespace EntityRepository.ODataServer.UnitTests.EStore
{

	/// <summary>
	/// A <see cref="DbContext"/> class for unit testing.
	/// </summary>
	internal sealed class Db : DbContext
	{
		public DbSet<User> Users { get; set; }
		// Products purposefully not included
		public DbSet<Sku> Skus { get; set; }
		public DbSet<Order> Orders { get; set; }
		// OrderLineItems purposefully not included
	}

}