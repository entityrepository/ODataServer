// -----------------------------------------------------------------------
// <copyright file="EntityKeyHelper.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EntityRepository.ODataServer.Util;

namespace EntityRepository.ODataServer
{
	/// <summary>
	/// Implements extracting the entity key from an entity, using the schema defined in Entity Framework.
	/// </summary>
	public class EntityKeyHelper<TDbContext>
		where TDbContext : DbContext
	{

		/// <summary>Creates <typeparam name="TDbContext" /> instances..</summary>
		private readonly Func<TDbContext> _dbContextFactory;

		/// <summary>Holds entity type -> key properties for each entity type.</summary>
		private readonly Dictionary<Type, PropertyInfo[]> _entityTypeKeys;

		/// <summary>Holds Entity -> Key conversion functions for each entity type.</summary>
		private readonly Dictionary<Type, object> _entityKeyFunctions;

		public EntityKeyHelper(Func<TDbContext> dbContextFactory)
		{
			Contract.Requires<ArgumentNullException>(dbContextFactory != null);

			_dbContextFactory = dbContextFactory;
			const int initialCollectionCapacity = ODataServiceConfig.InitialEntitySetCapacity;
			_entityTypeKeys = new Dictionary<Type, PropertyInfo[]>(initialCollectionCapacity);
			_entityKeyFunctions = new Dictionary<Type, object>(initialCollectionCapacity);
		}

		/// <summary>
		/// Helper method to determine the EntityFramework key type for an entity type.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public PropertyInfo SingleKeyPropertyForEntity<TEntity>(TDbContext dbContext)
			where TEntity : class
		{
			PropertyInfo[] keyProperties = KeyPropertiesForEntity<TEntity>(dbContext);
			if (keyProperties.Length == 1)
			{
				return keyProperties[0];
			}
			else if (keyProperties.Length > 1)
			{
				throw new NotImplementedException("Not supported: More than 1 key member in entity type " + typeof(TEntity).FullName);
			}
			else
			{
				throw new InvalidOperationException("Entity type '" + typeof(TEntity).FullName + "' does not appear to have any keys.");
			}
		}

		public PropertyInfo SingleKeyPropertyForEntity(TDbContext dbContext, Type entityType)
		{
			object returnValue = GetType().InvokeMethod(this, "SingleKeyPropertyForEntity", new[] { entityType }, dbContext);
			return (PropertyInfo) returnValue;
		}

		/// <summary>
		/// Returns a function that returns the <typeparamref name="TKey"/> from a <typeparamref name="TEntity"/>.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <returns></returns>
		public Func<TEntity, TKey> GetEntityKeyFunction<TEntity, TKey>()
			where TEntity : class
		{
			Type entityType = typeof(TEntity);
			lock (this)
			{
				object keyFunction;
				if (_entityKeyFunctions.TryGetValue(entityType, out keyFunction))
				{
					Func<TEntity, TKey> keyFunc = keyFunction as Func<TEntity, TKey>;
					if (keyFunc != null)
					{
						return keyFunc;
					}
					else
					{
						throw new InvalidOperationException(string.Format("Key function {0} could not be cast to type Func<{1},{2}>.", keyFunction, entityType, typeof(TKey)));
					}
				}

				PropertyInfo keyProperty = SingleKeyPropertyForEntity<TEntity>(null);
				if (keyProperty.PropertyType != typeof(TKey))
				{
					throw new InvalidOperationException(string.Format("The key of {0} is type {1}; does not match passed in key type {2}.",
					                                                  entityType.FullName,
					                                                  keyProperty.PropertyType.FullName,
					                                                  typeof(TKey).FullName));
				}

				// Create a lambda expression that returns the property, and compile it
				ParameterExpression param = Expression.Parameter(entityType, "e");
				var lambda = Expression.Lambda<Func<TEntity, TKey>>(Expression.Property(param, keyProperty), param);
				Func<TEntity, TKey> func = lambda.Compile();
				_entityKeyFunctions.Add(entityType, func);
				return func;
			}
		}

		public PropertyInfo[] KeyPropertiesForEntity<TEntity>(TDbContext dbContext)
			where TEntity : class
		{
			PropertyInfo[] keyProperties;
			if (_entityTypeKeys.TryGetValue(typeof(TEntity), out keyProperties))
			{
				return keyProperties;
			}

			if (dbContext == null)
			{
				// Use _dbContextFactory to create one
				return KeyPropertiesForEntity<TEntity>();
			}

			ObjectContext objectContext = ((IObjectContextAdapter) dbContext).ObjectContext;
			ObjectSet<TEntity> objectSet = objectContext.CreateObjectSet<TEntity>();
			var keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

			Type entityType = typeof(TEntity);
			keyProperties = new PropertyInfo[keyMembers.Count];
			for (int i = 0; i < keyMembers.Count; ++i)
			{
				keyProperties[i] = entityType.GetProperty(keyMembers[i].Name);
			}
			_entityTypeKeys.Add(entityType, keyProperties);
			return keyProperties;
		}

		public PropertyInfo[] KeyPropertiesForEntity<TEntity>()
			where TEntity : class
		{
			PropertyInfo[] keyProperties;
			if (_entityTypeKeys.TryGetValue(typeof(TEntity), out keyProperties))
			{
				return keyProperties;
			}

			using (TDbContext dbContext = _dbContextFactory())
			{
				return KeyPropertiesForEntity<TEntity>(dbContext);
			}
		}

		public IQueryable<TEntity> QueryWhereKeyMatches<TEntity, TKey>(IQueryable<TEntity> queryable, TKey key)
			where TEntity : class
		{
			Type keyType = typeof(TKey);
			Type entityType = typeof(TEntity);

			PropertyInfo keyProperty = SingleKeyPropertyForEntity<TEntity>(null);
			if (keyProperty.PropertyType != keyType)
			{
				throw new InvalidOperationException(string.Format("The key of {0} is type {1}; does not match passed in key type {2}.",
				                                                  entityType.FullName,
				                                                  keyProperty.PropertyType.FullName,
				                                                  keyType.FullName));
			}

			// Create a lambda expression for (entity => entity{.KeyProperty} == key)
			ParameterExpression param = Expression.Parameter(entityType, "e");
			var lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(Expression.Property(param, keyProperty), Expression.Constant(key, keyType)), param);

			return queryable.Where(lambda);
		}

	}
}
