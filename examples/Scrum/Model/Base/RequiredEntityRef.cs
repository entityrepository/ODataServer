// -----------------------------------------------------------------------
// <copyright file="RequiredEntityRef.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

namespace Scrum.Model.Base
{

	/// <summary>
	/// A reference to an entity which is required.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public struct RequiredEntityRef<TEntity, TKey>
		where TEntity : class
	{

		// Whether the entity reference is set.
		// The entity value.
		private TEntity _entity;
		private TKey _foreignKey;
		// Function to obtain the key of an entity
		private readonly Func<TEntity, TKey> _funcEntityToKey;

		public RequiredEntityRef(Func<TEntity, TKey> funcEntityToKey)
		{
			Contract.Requires<ArgumentNullException>(funcEntityToKey != null);

			_funcEntityToKey = funcEntityToKey;
			_foreignKey = default(TKey);
			_entity = null;
		}

		public TKey ForeignKey
		{
			get
			{
				TEntity entity = _entity;
				if (entity != null)
				{
					return GetKeyOfEntity(entity);
				}
				return _foreignKey;
			}
			set
			{
				if (HasReference && !Equals(value, ForeignKey))
				{
					throw new InvalidOperationException("ForeignKey value cannot be directly modified after it is set.");	
				}

				_foreignKey = value;
			}
		}

		public bool HasReference
		{
			get { return (_entity != null) || !Equals(_foreignKey, default(TKey)); }
		}

		public TEntity Entity
		{
			get
			{
				return _entity;
			}
			set
			{
				if (ReferenceEquals(value, null))
				{
					Clear();
				}
				else
				{
					_entity = value;
					_foreignKey = GetKeyOfEntity(value);
				}
			}
		}

		public void Clear()
		{
			_foreignKey = default(TKey);
			_entity = null;
		}

		public TKey GetKeyOfEntity(TEntity entity)
		{
			Contract.Requires<ArgumentNullException>(entity != null);

			if (_funcEntityToKey != null)
			{
				return _funcEntityToKey(entity);
			}
			else
			{
				try
				{
					return (TKey) ((dynamic) entity).ID;
				}
				catch (Exception ex)
				{
					string message = string.Format("Error extracting the key for an entity of type {0} ; to fix this, pass a valid funcEntityToKey to the RequiredEntityRef constructor.",
					                               entity.GetType().FullName);
					throw new InvalidOperationException(message, ex);
				}
			}
		}

	}
}
