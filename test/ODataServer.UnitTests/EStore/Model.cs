﻿// // -----------------------------------------------------------------------
// <copyright file="User.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// <summary>
// Example model types used in unit tests.  Models on online store.
// </summary>
// ReSharper disable once CheckNamespace
namespace EntityRepository.ODataServer.UnitTests.EStore.Model
{

	public abstract class BaseEntity
	{
		public int Id { get; set; }
	}

	public sealed class User : BaseEntity
	{
		[Required, StringLength(50, MinimumLength = 2)]
		public string Name { get; set; }
	}


	public class Product : BaseEntity
	{
		private readonly ICollection<Sku> _skus = new HashSet<Sku>();

		[StringLength(128, MinimumLength = 2)]
		public string Brand { get; set; }

		[Required, StringLength(128, MinimumLength = 2)]
		public string Name { get; set; }

		public ICollection<Sku> Skus
		{ get { return _skus; } }
	}

	public class MusicTrackProduct : Product
	{
		[Required, StringLength(128, MinimumLength = 2)]
		public string Artist { get; set; }

		[StringLength(128, MinimumLength = 2)]
		public string Album { get; set; }
	}

	public sealed class Sku : BaseEntity
	{
		[Required, StringLength(128, MinimumLength = 2)]
		public string Name { get; set; }
		public decimal RetailPrice { get; set; }

		// Optional product that can be used to group skus
		// For unit testing, we just need an 0..1 relationship
		public Product Product { get; set; }
	}

	public sealed class Order : BaseEntity
	{
		private readonly ICollection<OrderLineItem> _lineItems = new HashSet<OrderLineItem>();

		public User OrderedBy { get; set; }
		public int OrderedByUserId { get; set; }

		public ICollection<OrderLineItem> OrderLineItems
		{ get { return _lineItems; } }
	}

	public sealed class OrderLineItem : BaseEntity
	{
		public Order Order { get; set; }
		public int OrderId { get; set; }

		public Sku Sku { get; set; }
		public int SkuId { get; set; }

		public int Quantity { get; set; }
		public decimal ItemPrice { get; set; }
	}

}