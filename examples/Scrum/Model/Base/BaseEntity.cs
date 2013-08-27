// -----------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Scrum.Model.Base
{

	/// <summary>
	/// Shared entity functionality.
	/// </summary>
	/// <typeparam name="TId">The ID type - must be a valid ID type in entity framework and data services - signed numeric type, string, etc.</typeparam>
	/// <typeparam name="TEntity">The entity type, which is derived from <c>BaseEntity</c></typeparam>
	public abstract class BaseEntity<TId, TEntity> : IEquatable<TEntity>
		where TEntity : BaseEntity<TId, TEntity>
	{

		// HashCode is cached to ensure that it can't get "lost" in a Dictionary, which is heavily used in EF and data services client.
		private int _cachedHashCode;

		private TId _id;

		/// <summary>
		/// The database entity ID - uniquely identifies an entity.  May be <c>default(TId)</c> before the
		/// entity is added to the database.
		/// </summary>
		public TId ID
		{
			get { return _id; }
			set
			{
				if (Equals(value, _id))
				{
					return;
				}

				lock (this)
				{
					if (HasId)
					{
						throw new InvalidOperationException("Entity ID cannot be set more than once.");
					}

					_id = value;
				}
			}
		}

		/// <summary>
		/// Returns <c>true</c> if <see cref="ID"/> has been set; <see cref="ID"/> is normally only 
		/// set by a database.
		/// </summary>
		public bool HasId
		{
			get { return !Equals(ID, default(TId)); }
		}

		/// <summary>
		/// Implements <see cref="IEquatable{T}.Equals(T)"/> as identity equality.
		/// </summary>
		/// <param name="other">Another entity of the same type.</param>
		/// <returns><c>true</c> if <paramref name="other"/> represents the same object as <c>this</c>.</returns>
		public bool Equals(TEntity other)
		{
			if (ReferenceEquals(other, this))
			{
				return true;
			}
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			if (!HasId) // && ! other.HasId)
			{
				// Not the same instance, but neither entity has been stored in the DB yet.
				return false;
			}

			// Two entities are the same if they're the same type and have the same ID
			return Equals(ID, other.ID) && ReferenceEquals(GetType(), other.GetType());
		}

		/// <summary>
		/// Implements <see cref="GetHashCode"/> for identity equality.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyFieldInGetHashCode
			if (_cachedHashCode != 0)
			{
				return _cachedHashCode;
			}

			lock (this)
			{
				if (_cachedHashCode == 0)
				{
					if (HasId)
					{
						_cachedHashCode = _id.GetHashCode() * 127 ^ GetType().GetHashCode();
					}
					else
					{
						_cachedHashCode = RuntimeHelpers.GetHashCode(this);
					}
				}
			}

			return _cachedHashCode;
			// ReSharper restore NonReadonlyFieldInGetHashCode
		}

		/// <summary>
		/// Standard implementation of <see cref="object.Equals(object)"/>, which delegates to <see cref="IEquatable{T}"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals(obj as TEntity);
		}

		protected ICollection<T> EnsureCollectionProperty<T>(ref ICollection<T> collectionField)
		{
			Contract.Ensures(Contract.Result<ICollection<T>>() != null);

			if (collectionField == null)
			{
//#if SILVERLIGHT
				collectionField = new ObservableCollection<T>();
//#else
//				collectionField = new List<T>();
//#endif
			}

			return collectionField;
		}

		protected void SetCollectionProperty<T>(ref ICollection<T> collectionField, ICollection<T> value)
		{
			if (value == null)
			{
				collectionField = null;
			}
			else
			{
//#if SILVERLIGHT
				if (value is ObservableCollection<T>)
				{
					collectionField = value;
				}
				else
				{
					collectionField = new ObservableCollection<T>(value);
				}
//#else
//				collectionField = value;
//#endif
			}
		}

	}
}
