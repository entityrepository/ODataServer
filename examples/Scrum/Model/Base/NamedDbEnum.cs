// -----------------------------------------------------------------------
// <copyright file="NamedDbEnum.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Scrum.Model.Base
{
	/// <summary>
	/// A base class for any enumeration-like class that is connected to the database,  
	/// which has a friendly <c>string</c> name for each enumeration value.
	/// </summary>
	/// <typeparam name="TId">Type of the ID field in the derived class.</typeparam>
	/// <typeparam name="T">The <see cref="NamedDbEnum{TId,T}"/>-derived class.  Needed so that equality can be properly implemented in the base class.</typeparam>
	/// <example>
	/// The standard use-case is a derived (preferably <c>sealed</c>) class with a number of <c>static readonly</c> values.  These values can
	/// be validated and enhanced by checking with the database.
	/// <code>
	/// [DataContract]
	/// public sealed class FoodGroup : NamedDbEnum&lt;char, FoodGroup&gt;
	/// {
	///		public static readonly FoodGroup Meat = new FoodGroup('m', "Meat");
	/// 	public static readonly FoodGroup Fruit = new FoodGroup('f', "Fruit");
	/// 	public static readonly FoodGroup Vegetable = new FoodGroup('v', "Vegetable");
	/// 	public static readonly FoodGroup Grain = new FoodGroup('g', "Grain");
	/// 	public static readonly FoodGroup Dairy = new FoodGroup('d', "Dairy");
	/// 	public static readonly FoodGroup Butterfinger = new FoodGroup('b', "Butterfinger");
	/// 
	/// 	private FoodGroup(char id, string name)
	/// 		: base(id, name)
	/// 	{}
	/// }
	/// </code>
	/// <para>
	/// Note that if values of your <c>NamedDbEnum</c>-derived class are passed through WCF remoting boundaries, the 
	/// <c><see cref="DataContractAttribute">[DataContract]</see></c> must be added to the derived class.
	/// </para>
	/// </example>
	[DataContract]
	public abstract class NamedDbEnum<TId, T>
		where T : NamedDbEnum<TId, T>
	{

		/// <summary>
		/// The max string length for <see cref="Name"/>.
		/// </summary>
		public const int MaxNameLength = 128;

		private TId _id;
		private string _name;

		/// <summary>
		/// Default ctor for remoting.
		/// TODO: Make DbEnum and related classes remotable and immutable.
		/// </summary>
		protected NamedDbEnum()
		{}

		protected NamedDbEnum(TId id, string name)
		{
			_id = id;
			_name = name;
			DbEnumManager.RegisterDbEnumValue(this);
		}

		/// <summary>
		/// ID property.  Note that the name <c>ID</c> is required (<c>Id</c> won't work) based on limitations in
		/// the WCF data services client code.
		/// </summary>
		[DataMember(Name = "id", IsRequired = true, Order = 0)]
		public TId ID
		{
			get { return _id; }
			set
			{
				if (!Equals(value, _id))
				{
					if (!Equals(_id, default(TId)))
					{
						throw new InvalidOperationException("NamedDbEnum ID cannot be set more than once.");
					}

					_id = value;
				}
			}
		}

		/// <summary>
		/// The friendly name for this <see cref="NamedDbEnum{TId,T}"/> value.
		/// </summary>
		[DataMember(Name = "name", Order = 1)]
		[StringLength(MaxNameLength)]
		public virtual string Name
		{
			get { return _name; }
			set
			{
				Contract.Requires<ArgumentOutOfRangeException>(value == null || value.Length < MaxNameLength);

				if (!string.Equals(value, _name, StringComparison.Ordinal))
				{
					_name = value;
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}

	}

}
