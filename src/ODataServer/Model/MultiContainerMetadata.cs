// -----------------------------------------------------------------------
// <copyright file="MultiContainerMetadata.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;

namespace EntityRepository.ODataServer.Model
{

	/// <summary>
	/// Base class for combining multiple inner <see cref="IContainerMetadata"/> objects into a single container.
	/// For example, multiple <c>DbContext</c> classes can be exposed within a single OData container.
	/// </summary>
	public class MultiContainerMetadata<T> : IContainerMetadata<T>, IEdmModel, IEdmEntityContainer
		where T : class
	{

		private readonly IContainerMetadata[] _innerContainers;
		private readonly IEnumerable<IEdmSchemaElement> _schemaElements;

		public MultiContainerMetadata(params IContainerMetadata[] innerContainers)
		{
			Contract.Requires<ArgumentNullException>(innerContainers != null);
			Contract.Requires<ArgumentException>(innerContainers.Length > 0);
			Contract.Requires<ArgumentNullException>(innerContainers.All(c => c != null));

			Name = typeof(T).Name;
			Namespace = typeof(T).Namespace;
			_innerContainers = innerContainers;

			// Cache the schema elements to improve perf
			_schemaElements = _innerContainers
				.SelectMany(c => c.EdmModel.SchemaElements.Where(elt => !(elt is IEdmEntityContainer)))
				.Concat(new IEdmSchemaElement[] { (IEdmEntityContainer) this })
				.Distinct();
		}

		#region IContainerMetadata

		public string Name { get; set; }
		public string Namespace { get; set; }
		public IEdmModel EdmModel { get { return this; } }
		public IEdmEntityContainer EdmContainer { get { return this; } }

		public IEnumerable<IEntityTypeMetadata> EntityTypes
		{
			get { return _innerContainers.SelectMany(c => c.EntityTypes); }
		}

		public IEnumerable<IEntitySetMetadata> EntitySets
		{
			get { return _innerContainers.SelectMany(c => c.EntitySets); }
		}

		#endregion
		#region IEdmModel

		public IEdmEntityContainer FindDeclaredEntityContainer(string name)
		{
			if (string.Equals(name, Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return this;
			}
			foreach (var innerContainer in _innerContainers)
			{
				if (string.Equals(name, innerContainer.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					return innerContainer.EdmContainer;
				}
			}
			return null;
		}

		public IEdmSchemaType FindDeclaredType(string qualifiedName)
		{
			foreach (var innerContainer in _innerContainers)
			{
				IEdmSchemaType edmType = innerContainer.EdmModel.FindDeclaredType(qualifiedName);
				if (edmType != null)
				{
					return edmType;
				}
			}
			return null;
		}

		public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
		{
			return _innerContainers.SelectMany(c => c.EdmModel.FindDeclaredFunctions(qualifiedName)).Distinct();
		}

		public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
		{
			foreach (var innerContainer in _innerContainers)
			{
				IEdmValueTerm edmValueTerm = innerContainer.EdmModel.FindDeclaredValueTerm(qualifiedName);
				if (edmValueTerm != null)
				{
					return edmValueTerm;
				}
			}
			return null;
		}

		public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
		{
			return _innerContainers.SelectMany(c => c.EdmModel.FindDeclaredVocabularyAnnotations(element)).Distinct();
		}

		public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
		{
			return _innerContainers.SelectMany(c => c.EdmModel.FindDirectlyDerivedTypes(baseType)).Distinct();
		}

		public IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get { return _schemaElements; }
		}

		public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get { return _innerContainers.SelectMany(c => c.EdmModel.VocabularyAnnotations).Distinct(); }
		}

		public IEnumerable<IEdmModel> ReferencedModels
		{
			get { return _innerContainers.SelectMany(c => c.EdmModel.ReferencedModels).Distinct(); }
		}

		public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
		{
			get { return _innerContainers.First().EdmModel.DirectValueAnnotationsManager; }
		}

		#endregion
		#region IEdmEntityContainer

		public IEdmEntitySet FindEntitySet(string setName)
		{
			IEntitySetMetadata esm = this.GetEntitySet(setName);
			return esm == null ? null : esm.EdmEntitySet;
		}

		public IEnumerable<IEdmFunctionImport> FindFunctionImports(string functionName)
		{
			return _innerContainers.SelectMany(c => c.EdmContainer.FindFunctionImports(functionName)).Distinct();
		}

		public IEnumerable<IEdmEntityContainerElement> Elements
		{
			get { return _innerContainers.SelectMany(c => c.EdmContainer.Elements); }
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get { return EdmSchemaElementKind.EntityContainer; }
		}

		#endregion

	}


}

