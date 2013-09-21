// -----------------------------------------------------------------------
// <copyright file="EntityTypeMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using EntityRepository.ODataServer.Util;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	public class EntityTypeMetadata : IEntityTypeMetadata
	{

		private readonly IEdmEntityType _edmStructuredType;
		private readonly Type _clrType;
		private readonly PropertyInfo[] _clrKeyProperties;
		private readonly Func<object, object> _entityKeyFunction;

		internal EntityTypeMetadata(IEdmEntityType edmStructuredType, Type clrType, PropertyInfo[] clrKeyProperties)
		{
			Contract.Assert(edmStructuredType != null);
			Contract.Assert(clrType != null);
			Contract.Assert(clrKeyProperties != null);
			Contract.Assert(clrKeyProperties.Length >= 1);

			_edmStructuredType = edmStructuredType;
			_clrType = clrType;
			_clrKeyProperties = clrKeyProperties;
			_entityKeyFunction = GetUntypedEntityKeyFunction(this);
		}

		public Type ClrType { get { return _clrType; } }

		public IEdmEntityType EdmType { get { return _edmStructuredType; } }

		public int CountKeyProperties { get { return _clrKeyProperties.Length; } }
		public IEnumerable<IEdmStructuralProperty> EdmKeyProperties { get { return _edmStructuredType.DeclaredKey; } }
		public IEnumerable<PropertyInfo> ClrKeyProperties { get { return _clrKeyProperties; } }

		public Func<object, object> EntityKeyFunction { get { return _entityKeyFunction; } } 

		public PropertyInfo SingleClrKeyProperty
		{
			get
			{
				Contract.Assert(CountKeyProperties == 1);

				return _clrKeyProperties[0];
			}
		}

		private static Func<object, object> GetUntypedEntityKeyFunction(IEntityTypeMetadata entityTypeMetadata)
		{
			Contract.Assert(entityTypeMetadata != null);

			// Build a parameterized EntityKeyFunctions<TEntity, TKey> to obtain the key function
			Type genericEntityKeyFunctions = typeof(EntityKeyFunctions<,>).MakeGenericType(entityTypeMetadata.ClrType, entityTypeMetadata.SingleClrKeyProperty.PropertyType);
			return genericEntityKeyFunctions.InvokeStaticMethod("GetUntypedEntityKeyFunction", entityTypeMetadata) as Func<object, object>;
		}
	}
}