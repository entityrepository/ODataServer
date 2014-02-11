// -----------------------------------------------------------------------
// <copyright file="EntityKeyFunction.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using EntityRepository.ODataServer.Model;

namespace EntityRepository.ODataServer.Util
{
	/// <summary>
	/// Provides a function for determining the key from an entity.  Currently this only works for single property keys.
	/// </summary>
	/// <typeparam name="TEntity">The entity type</typeparam>
	/// <typeparam name="TKey">The type of key for <typeparamref name="TEntity"/>.  For single keys, this should be the type of the key.  For multiple keys,
	/// this should be an object array (<c>object[]</c>).</typeparam>
	public static class EntityKeyFunction<TEntity, TKey>
		where TEntity : class
	{
		/// <summary>
		/// Cached EntityKeyFunction.
		/// </summary>
		private static Func<TEntity, TKey> s_entityKeyFunc;

		public static Func<TEntity, TKey> GetEntityKeyFunction(IEntityTypeMetadata entityTypeMetadata)
		{
			if (s_entityKeyFunc != null)
			{
				return s_entityKeyFunc;
			}

			Initialize(entityTypeMetadata);

			return s_entityKeyFunc;
		}

		public static Func<object, object> GetUntypedEntityKeyFunction(IEntityTypeMetadata entityTypeMetadata)
		{
			Func<TEntity, TKey> entityKeyFunction = GetEntityKeyFunction(entityTypeMetadata);
			return new Func<object, object>((object untypedEntity) =>
			{
				if (untypedEntity == null)
				{
					throw new ArgumentNullException("entity");
				}
				TEntity entity = untypedEntity as TEntity;
				if (entity == null)
				{
					throw new ArgumentException(string.Format("Entity must be type {0}; is type {1}.", typeof(TEntity), untypedEntity.GetType()), "entity");
				}
				return entityKeyFunction(entity);
			});
		}

		public static IQueryable<TEntity> QueryWhereKeyMatches(IQueryable<TEntity> queryable, TKey key, IEntityTypeMetadata entityTypeMetadata)
		{
			Contract.Requires<ArgumentNullException>(entityTypeMetadata != null);
			Contract.Requires<ArgumentException>(entityTypeMetadata.ClrKeyProperties != null);
			Contract.Requires<ArgumentException>(entityTypeMetadata.CountKeyProperties == 1);
			Contract.Requires<ArgumentException>(typeof(TEntity) == entityTypeMetadata.ClrType, "The TEntity type parameter doesn't match the entity type in the datamodel entitytype.");
			Contract.Requires<ArgumentException>(typeof(TKey) == entityTypeMetadata.SingleClrKeyProperty.PropertyType, "The TKey type parameter doesn't match the key type in the datamodel entitytype.");

			PropertyInfo keyProperty = entityTypeMetadata.SingleClrKeyProperty;

			// Create a lambda expression for (entity => entity{.KeyProperty} == key)
			ParameterExpression param = Expression.Parameter(entityTypeMetadata.ClrType, "e");
			var lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(Expression.Property(param, keyProperty), Expression.Constant(key, typeof(TKey))), param);

			return queryable.Where(lambda);
		}

		private static void Initialize(IEntityTypeMetadata entityTypeMetadata)
		{
			Contract.Requires<ArgumentNullException>(entityTypeMetadata != null);
			Contract.Requires<ArgumentException>(entityTypeMetadata.ClrKeyProperties != null);
			Contract.Requires<ArgumentException>(typeof(TEntity) == entityTypeMetadata.ClrType, "The TEntity type parameter doesn't match the entity type in the datamodel entitytype.");

			// Create a lambda expression that returns the property, and compile it
			Expression<Func<TEntity, TKey>> lambda;
			ParameterExpression param = Expression.Parameter(entityTypeMetadata.ClrType, "e");
			if (entityTypeMetadata.CountKeyProperties == 1)
			{	// Single key, use a simple expression to return the key property
				lambda = Expression.Lambda<Func<TEntity, TKey>>(Expression.Property(param, entityTypeMetadata.SingleClrKeyProperty), param);
			}
			else
			{	// Multiple keys, return an object array of the key values
				var keyProperties = entityTypeMetadata.ClrKeyProperties.Select(property => Expression.Convert(Expression.Property(param, property), typeof(object))); // TypeAs provides an (object) cast aka boxing, needed to convert value types to object
				lambda = Expression.Lambda<Func<TEntity, TKey>>(Expression.NewArrayInit(typeof(object), keyProperties), param);
			}
			Func<TEntity, TKey> func = lambda.Compile();
			
			// Store it
			Interlocked.CompareExchange(ref s_entityKeyFunc, func, null);
		}

	}
}