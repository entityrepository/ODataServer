// -----------------------------------------------------------------------
// <copyright file="IContainerMetadata.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Storage-independent model describing the structure of a container or context of entity sets and entity types.
	/// </summary>
	[ContractClass(typeof(ContainerMetadataContract))]
	public interface IContainerMetadata
	{
		string Name { get; set; }
		string Namespace { get; set; }

		IEdmModel EdmModel { get; }
		IEdmEntityContainer EntityContainer { get; }

		IEnumerable<IEntityTypeMetadata> EntityTypes { get; }
		IEnumerable<IEntitySetMetadata> EntitySets { get; }

	}

	[ContractClassFor(typeof(IContainerMetadata))]
	internal abstract class ContainerMetadataContract : IContainerMetadata
	{


		public string Name
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value));
				throw new NotImplementedException();
			}
		}

		public string Namespace
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				Contract.Requires<ArgumentException>(value != null);
				throw new NotImplementedException();
			}
		}

		public Type ContainerType
		{
			get
			{
				Contract.Ensures(Contract.Result<Type>() != null);
				throw new NotImplementedException();
			}
		}

		public IEdmModel EdmModel
		{
			get
			{
				Contract.Ensures(Contract.Result<IEdmModel>() != null);
				throw new NotImplementedException();
			}
		}

		public IEdmEntityContainer EntityContainer
		{
			get
			{
				Contract.Ensures(Contract.Result<IEdmEntityContainer>() != null);
				throw new NotImplementedException();
			}
		}

		public IEnumerable<IEntityTypeMetadata> EntityTypes
		{
			get
			{
				Contract.Ensures(Contract.Result<IEnumerable<IEntityTypeMetadata>>() != null);
				throw new NotImplementedException();
			}
		}

		public IEnumerable<IEntitySetMetadata> EntitySets
		{
			get
			{
				Contract.Ensures(Contract.Result<IEnumerable<IEntitySetMetadata>>() != null);
				throw new NotImplementedException();
			}
		}

	}
}