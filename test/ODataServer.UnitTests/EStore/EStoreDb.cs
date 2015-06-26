// // -----------------------------------------------------------------------
// <copyright file="EStoreDb.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Data.Entity;
using EntityRepository.ODataServer.UnitTests.EStore.Model;

// ReSharper disable once CheckNamespace
namespace EntityRepository.ODataServer.UnitTests.EStore.DataAccess
{

	/// <summary>
	/// A <see cref="DbContext"/> class for unit testing.
	/// </summary>
	internal sealed class EStoreDb : DbContext
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Product> Products { get; set; }
		// Skus purposefully not included
		public DbSet<Order> Orders { get; set; }
		// OrderLineItems purposefully not included
	}

}