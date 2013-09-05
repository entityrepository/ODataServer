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
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	public class EntityTypeMetadata : IEntityTypeMetadata
	{

		private readonly IEdmEntityType _edmStructuredType;
		private readonly Type _clrType;
		private readonly PropertyInfo[] _clrKeyProperties;

		internal EntityTypeMetadata(IEdmEntityType edmStructuredType, Type clrType, PropertyInfo[] clrKeyProperties)
		{
			Contract.Requires<ArgumentNullException>(edmStructuredType != null);
			Contract.Requires<ArgumentNullException>(clrType != null);
			Contract.Requires<ArgumentNullException>(clrKeyProperties != null);

			_edmStructuredType = edmStructuredType;
			_clrType = clrType;
			_clrKeyProperties = clrKeyProperties;
		}

		public Type ClrType { get { return _clrType; } }

		public IEdmEntityType EdmType { get { return _edmStructuredType; } }

		public int CountKeyProperties { get { return _clrKeyProperties.Length; } }
		public IEnumerable<IEdmStructuralProperty> EdmKeyProperties { get { return _edmStructuredType.DeclaredKey; } }
		public IEnumerable<PropertyInfo> ClrKeyProperties { get { return _clrKeyProperties; } }

		public PropertyInfo SingleClrKeyProperty
		{
			get
			{
				Contract.Requires<InvalidOperationException>(CountKeyProperties == 1);
				return _clrKeyProperties[0];
			}
		}

	}
}