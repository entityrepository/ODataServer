// -----------------------------------------------------------------------
// <copyright file="EntityRef.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

namespace Scrum.Model.Base
{


	public struct EntityRef<TEntity, TKey> where TEntity : class
	{

		// Whether the entity reference is set.
		// The entity value.
		private TEntity _entity;
		private TKey _foreignKey;
		// Function to obtain the key of an entity
		private Func<TEntity, TKey> _funcEntityToKey;
		private bool _isSet;

		public EntityRef(Func<TEntity, TKey> funcEntityToKey)
		{
			Contract.Requires<ArgumentNullException>(funcEntityToKey != null);

			_funcEntityToKey = funcEntityToKey;
			_isSet = false;
			_foreignKey = default(TKey);
			_entity = null;
		}

		public TKey ForeignKey
		{
			get { return _foreignKey; }
			set
			{
				_foreignKey = value;
				_isSet = ! Equals(_foreignKey, default(TKey));
			}
		}

		public bool HasReference
		{
			get { return _isSet; }
		}

		public TEntity Entity
		{
			get
			{
				return _entity;
			}
			set
			{
				if (Object.ReferenceEquals(value, null))
				{
					Clear();
				}
				else
				{
					_entity = value;
					_foreignKey = GetKeyOfEntity(value);
					_isSet = true;
				}
			}
		}

		public void Clear()
		{
			_isSet = false;
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
					string message = string.Format("Error extracting the key for an entity of type {0} ; to fix this, pass a valid funcEntityToKey to the EntityRef constructor.",
					                               entity.GetType().FullName);
					throw new InvalidOperationException(message, ex);
				}
			}
		}

	}
}
