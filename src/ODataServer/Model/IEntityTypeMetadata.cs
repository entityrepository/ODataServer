// -----------------------------------------------------------------------
// <copyright file="IEntityTypeMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Storage-independent model of entity type metadata.
	/// </summary>
	public interface IEntityTypeMetadata
	{

		IEdmEntityType EdmType { get; }
		Type ClrType { get; }

		int CountKeyProperties { get; }
		IEnumerable<IEdmStructuralProperty> EdmKeyProperties { get; }
		IEnumerable<PropertyInfo> ClrKeyProperties { get; }

		PropertyInfo SingleClrKeyProperty { get; }
	}
}