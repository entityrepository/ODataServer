// // -----------------------------------------------------------------------
// <copyright file="FixedEFEdmModel.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics.Contracts;
using System.Linq;
using EntityRepository.ODataServer.Model;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;

namespace EntityRepository.ODataServer.EF
{

	/// <summary>
	/// Fixes problems in the EF Entity Data Model, so it can be used directly with Web API OData.
	/// </summary>
	/// <remarks>
	/// Currently, the only fix is fixing the Namespace for entitys, but it also involves changing all the navigation properties, base types, as well as each entity
	/// namespace.  By default EF just uses a single namespace, which is the namespace for the <c>DbContext</c> class.  Frequently that is not the correct namespace for 
	/// the entities used in the <c>DbContext</c> class.
	/// </remarks>
	internal sealed class FixedEfEdmModel : IEdmModel
	{

		private readonly IEdmModel _dbContextEdmModel;
		/// <summary>
		/// Holds a mapping of EF EdmEntityType name => fixed types in this model.
		/// </summary>
		private readonly Dictionary<string, EdmEntityTypeWrapper> _fixedTypes;
		/// <summary>
		/// Holds a mapping of entity container name -> fixed entity container
		/// </summary>
		private readonly Dictionary<string, EdmEntityContainerWrapper> _fixedContainers;
		/// <summary>
		/// Cache of all directly contained schema elements in this model
		/// </summary>
		private readonly IEnumerable<IEdmSchemaElement> _schemaElements;

		internal FixedEfEdmModel(DbContext dbContext, IEnumerable<IEntityTypeMetadata> entityTypesMetadata)
		{
			Contract.Requires<ArgumentNullException>(dbContext != null);

			_dbContextEdmModel = dbContext.GetEdmModel();
			_fixedTypes = new Dictionary<string, EdmEntityTypeWrapper>();
			_fixedContainers = new Dictionary<string, EdmEntityContainerWrapper>();
			FixUpEntityTypes(entityTypesMetadata);
			FixUpEntityContainers();
			_schemaElements = ReplaceFixedModelElements(_dbContextEdmModel.SchemaElements);
		}

		private void FixUpEntityTypes(IEnumerable<IEntityTypeMetadata> entityTypesMetadata)
		{
			// Create an EdmEntityTypeWrapper wrapping each IEdmEntityType
			foreach (var entityTypeMetadata in entityTypesMetadata)
			{
				_fixedTypes.Add(entityTypeMetadata.EdmType.FullName(), new EdmEntityTypeWrapper(entityTypeMetadata.ClrType, entityTypeMetadata.EdmType));
			}

			foreach (var edmTypeWrapper in _fixedTypes.Values)
			{
				// Set the base type of each type wrapper to the right type wrapper
				EdmEntityTypeWrapper baseTypeWrapper;
				if (TryGetEntityTypeWrapperFor(edmTypeWrapper.InnerEdmEntityType.BaseType as IEdmEntityType, out baseTypeWrapper))
				{
					edmTypeWrapper.SetBaseEntityType(baseTypeWrapper);
				}

				// Fixup all navigation properties to use the correct type wrapper
				var navigationProperties = edmTypeWrapper.InnerEdmEntityType.DeclaredNavigationProperties();
				foreach (IEdmNavigationProperty navigationProperty in navigationProperties)
				{
					var toEntityType = navigationProperty.ToEntityType();
					EdmEntityTypeWrapper toEntityTypeWrapper;
					if (! TryGetEntityTypeWrapperFor(toEntityType, out toEntityTypeWrapper))
					{
						// TODO? Log and skip the property?  Or throw?
						throw new InvalidOperationException("Navigation properties to entity types not contained in the model not supported.");
					}
					edmTypeWrapper.AddNavigationProperty(new EdmNavigationPropertyWrapper(edmTypeWrapper, navigationProperty, toEntityTypeWrapper));
				}
			}
		}

		private void FixUpEntityContainers()
		{
			foreach (var efEntityContainer in _dbContextEdmModel.SchemaElements.OfType<IEdmEntityContainer>())
			{
				EdmEntityContainerWrapper containerWrapper = new EdmEntityContainerWrapper(efEntityContainer, _fixedTypes);
				_fixedContainers.Add(containerWrapper.Name, containerWrapper);
			}

			// Fixup the NavigationMappings
			foreach (var containerWrapper in _fixedContainers.Values)
			{
				foreach (var entitySetWrapper in containerWrapper.EntitySetWrappers)
				{
					entitySetWrapper.WrapNavigationTargets(containerWrapper, _fixedTypes);
				}
			}
		}

		private bool TryGetEntityTypeWrapperFor(IEdmEntityType edmEntityType, out EdmEntityTypeWrapper entityTypeWrapper)
		{
			if (edmEntityType != null)
			{
				string typeFullName = edmEntityType.FullName();
				return _fixedTypes.TryGetValue(typeFullName, out entityTypeWrapper);
			}

			entityTypeWrapper = null;
			return false;
		}

		private IEnumerable<T> ReplaceFixedModelElements<T>(IEnumerable<T> elements) where T : class, IEdmElement
		{
			return elements.Select(inputElement =>
			                       {
				                       var inputEntityType = inputElement as IEdmEntityType;
				                       if (inputEntityType != null)
				                       {
					                       EdmEntityTypeWrapper fixedType;
					                       if (TryGetEntityTypeWrapperFor(inputEntityType, out fixedType))
					                       {
						                       return fixedType as T;
					                       }
				                       }

				                       var inputContainer = inputElement as IEdmEntityContainer;
				                       if (inputContainer != null)
				                       {
					                       EdmEntityContainerWrapper fixedContainer;
					                       if (_fixedContainers.TryGetValue(inputContainer.Name, out fixedContainer))
					                       {
						                       return fixedContainer as T;
					                       }
				                       }

				                       return inputElement;
			                       });
		}

		#region IEdmModel

		public IEdmEntityContainer FindDeclaredEntityContainer(string name)
		{
			EdmEntityContainerWrapper container;
			_fixedContainers.TryGetValue(name, out container);
			return container;
		}

		public IEdmSchemaType FindDeclaredType(string qualifiedName)
		{
			EdmEntityTypeWrapper typeWrapper;
			if (_fixedTypes.TryGetValue(qualifiedName, out typeWrapper))
			{
				return typeWrapper;
			}
			else
			{
				return _dbContextEdmModel.FindDeclaredType(qualifiedName);
			}
		}

		public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
		{
			return _dbContextEdmModel.FindDeclaredFunctions(qualifiedName);
		}

		public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
		{
			return _dbContextEdmModel.FindDeclaredValueTerm(qualifiedName);
		}

		public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
		{
			return _dbContextEdmModel.FindDeclaredVocabularyAnnotations(element);
		}

		public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
		{
			return ReplaceFixedModelElements(_dbContextEdmModel.FindDirectlyDerivedTypes(baseType));
		}

		public IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get { return _schemaElements; }
		}

		public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get { return _dbContextEdmModel.VocabularyAnnotations; }
		}

		public IEnumerable<IEdmModel> ReferencedModels
		{
			get { return _dbContextEdmModel.ReferencedModels; }
		}

		public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
		{
			get { return _dbContextEdmModel.DirectValueAnnotationsManager; }
		}

		#endregion


		/// <summary>
		/// Wraps and fixes an <see cref="IEdmEntityType"/>.
		/// </summary>
		private class EdmEntityTypeWrapper : IEdmEntityType
		{

			private readonly IEdmEntityType _innerEdmType;
			private readonly Type _clrType;
			private EdmEntityTypeWrapper _baseType;
			private readonly HashSet<EdmNavigationPropertyWrapper> _declaredNavigationProperties; 

			public EdmEntityTypeWrapper(Type clrType, IEdmEntityType edmType)
			{
				Contract.Requires<ArgumentNullException>(clrType != null);
				Contract.Requires<ArgumentNullException>(edmType != null);

				_clrType = clrType;
				_innerEdmType = edmType;
				_declaredNavigationProperties = new HashSet<EdmNavigationPropertyWrapper>();
			}

			internal IEdmEntityType InnerEdmEntityType
			{
				get { return _innerEdmType; }
			}

			internal void SetBaseEntityType(EdmEntityTypeWrapper baseType)
			{
				_baseType = baseType;
			}

			internal void AddNavigationProperty(EdmNavigationPropertyWrapper navigationPropertyWrapper)
			{
				_declaredNavigationProperties.Add(navigationPropertyWrapper);
			}

			#region IEdmEntityType

			public EdmTypeKind TypeKind
			{
				get { return _innerEdmType.TypeKind; }
			}

			public IEdmProperty FindProperty(string name)
			{
				IEdmProperty navigationProperty = _declaredNavigationProperties.FirstOrDefault(p => p.Name == name);
				if (navigationProperty != null)
				{
					return navigationProperty;
				}
				else
				{
					if (_baseType != null)
					{
						navigationProperty = _baseType.FindProperty(name);
						if (navigationProperty != null)
						{
							return navigationProperty;
						}
					}

					return _innerEdmType.FindProperty(name);
				}
			}

			public bool IsAbstract
			{
				get { return _innerEdmType.IsAbstract; }
			}

			public bool IsOpen
			{
				get { return _innerEdmType.IsOpen; }
			}

			public IEdmStructuredType BaseType
			{
				get { return _baseType; }
			}

			public IEnumerable<IEdmProperty> DeclaredProperties
			{
				get { return _innerEdmType.DeclaredProperties.Where(prop => ! (prop is IEdmNavigationProperty)).Concat(_declaredNavigationProperties); }
			}

			public string Name
			{
				get { return _clrType.Name; }
			}

			public EdmSchemaElementKind SchemaElementKind
			{
				get { return _innerEdmType.SchemaElementKind; }
			}

			public string Namespace
			{
				get { return _clrType.Namespace; }
			}

			public EdmTermKind TermKind
			{
				get { return _innerEdmType.TermKind; }
			}

			public IEnumerable<IEdmStructuralProperty> DeclaredKey
			{
				get { return _innerEdmType.DeclaredKey; }
			}

			#endregion

			public override string ToString()
			{
				return Namespace + "." + Name;
			}

		}


		/// <summary>
		/// Wraps and fixes an <see cref="IEdmEntityContainer"/>
		/// </summary>
		private class EdmEntityContainerWrapper : IEdmEntityContainer
		{

			private readonly IEdmEntityContainer _innerEntityContainer;
			private readonly Dictionary<string, EdmEntitySetWrapper> _fixedEntitySets;

			public EdmEntityContainerWrapper(IEdmEntityContainer innerEntityContainer, Dictionary<string, EdmEntityTypeWrapper> fixedTypes)
			{
				Contract.Requires<ArgumentNullException>(innerEntityContainer != null);
				Contract.Requires<ArgumentNullException>(fixedTypes != null);

				_innerEntityContainer = innerEntityContainer;
				_fixedEntitySets = new Dictionary<string, EdmEntitySetWrapper>();

				// Create all the fixed EntitySets
				foreach (IEdmEntitySet innerEntitySet in _innerEntityContainer.EntitySets())
				{
					EdmEntityTypeWrapper fixedElementType;
					if (fixedTypes.TryGetValue(innerEntitySet.ElementType.FullName(), out fixedElementType))
					{
						var entitySetWrapper = new EdmEntitySetWrapper(innerEntitySet, this, fixedElementType, fixedTypes);
						_fixedEntitySets.Add(entitySetWrapper.Name, entitySetWrapper);
					}
				}
			}

			public IEnumerable<EdmEntitySetWrapper> EntitySetWrappers
			{
				get { return _fixedEntitySets.Values; }
			}

			#region IEdmEntityContainer

			public string Name
			{
				get { return _innerEntityContainer.Name; }
			}

			public EdmSchemaElementKind SchemaElementKind
			{
				get { return EdmSchemaElementKind.EntityContainer; }
			}

			public string Namespace
			{
				get { return _innerEntityContainer.Namespace; }
			}

			public IEdmEntitySet FindEntitySet(string setName)
			{
				EdmEntitySetWrapper fixedEntitySet;
				_fixedEntitySets.TryGetValue(setName, out fixedEntitySet);
				return fixedEntitySet;
			}

			public IEnumerable<IEdmFunctionImport> FindFunctionImports(string functionName)
			{
				return _innerEntityContainer.FindFunctionImports(functionName);
			}

			public IEnumerable<IEdmEntityContainerElement> Elements
			{
				get { return _innerEntityContainer.Elements.Where(elt => !(elt is IEdmEntitySet)).Concat(_fixedEntitySets.Values); }
			}

			#endregion
		}


		/// <summary>
		/// Wraps and fixes an <see cref="IEdmEntitySet"/>.
		/// </summary>
		private class EdmEntitySetWrapper : IEdmEntitySet
		{

			private readonly IEdmEntitySet _innerEntitySet;
			private readonly EdmEntityContainerWrapper _container;
			private readonly EdmEntityTypeWrapper _elementType;
			private IEdmNavigationTargetMapping[] _navigationTargetMappings;

			public EdmEntitySetWrapper(IEdmEntitySet innerEntitySet, 
				EdmEntityContainerWrapper container, 
				EdmEntityTypeWrapper elementType, 
				Dictionary<string, EdmEntityTypeWrapper> fixedTypes)
			{
				Contract.Requires<ArgumentNullException>(innerEntitySet != null);
				Contract.Requires<ArgumentNullException>(container != null);
				Contract.Requires<ArgumentNullException>(elementType != null);

				_innerEntitySet = innerEntitySet;
				_container = container;
				_elementType = elementType;
			}

			/// <summary>
			/// Part 2 of initialization - must be run after all wrapped EntitySets are initialized.
			/// </summary>
			public void WrapNavigationTargets(EdmEntityContainerWrapper container, Dictionary<string, EdmEntityTypeWrapper> fixedTypes)
			{
				var navigationMappings = new List<IEdmNavigationTargetMapping>();
				foreach (var innerNavigationMapping in _innerEntitySet.NavigationTargets)
				{
					var innerDeclaringType = innerNavigationMapping.NavigationProperty.DeclaringType as IEdmEntityType;
					EdmEntityTypeWrapper declaringType;
					if ((innerDeclaringType != null)
						&& (fixedTypes.TryGetValue(innerDeclaringType.FullName(), out declaringType)))
					{
						var propName = innerNavigationMapping.NavigationProperty.Name;
						var navigationProperty = declaringType.DeclaredNavigationProperties().Single(navProp => navProp.Name == propName);
						var innerTargetEntitySet = innerNavigationMapping.TargetEntitySet;
						var targetEntitySet = container.FindEntitySet(innerTargetEntitySet.Name);
						navigationMappings.Add(new EdmNavigationTargetMapping(navigationProperty, targetEntitySet));
					}
					else
					{
						throw new InvalidOperationException("IEdmNavigationTargetMapping could not be initialized for " + innerNavigationMapping.NavigationProperty.Name + " - perhaps we don't have correct support for complex types?");
					}
				}
				_navigationTargetMappings = navigationMappings.ToArray();
			}


			#region IEdmEntitySet

			public string Name
			{
				get { return _innerEntitySet.Name; }
			}

			public EdmContainerElementKind ContainerElementKind
			{
				get { return EdmContainerElementKind.EntitySet; }
			}

			public IEdmEntityContainer Container
			{
				get { return _container; }
			}

			public IEdmEntitySet FindNavigationTarget(IEdmNavigationProperty navigationProperty)
			{
				var navigationMapping = _navigationTargetMappings.FirstOrDefault(mapping => mapping.NavigationProperty.Name == navigationProperty.Name);
				return navigationMapping == null ? null : navigationMapping.TargetEntitySet;
			}

			public IEdmEntityType ElementType
			{
				get { return _elementType; }
			}

			public IEnumerable<IEdmNavigationTargetMapping> NavigationTargets
			{
				get { return _navigationTargetMappings; }
			}

			#endregion

		}


		/// <summary>
		/// Wraps and fixes an <see cref="IEdmNavigationProperty"/>.
		/// </summary>
		private class EdmNavigationPropertyWrapper : IEdmNavigationProperty
		{

			private readonly EdmEntityTypeWrapper _declaringType;
			private readonly IEdmNavigationProperty _innerNavigationProperty;
			private readonly EdmEntityTypeWrapper _toType;
			private EdmNavigationPropertyWrapper _partner;

			internal EdmNavigationPropertyWrapper(EdmEntityTypeWrapper declaringType, IEdmNavigationProperty innerNavigationProperty, EdmEntityTypeWrapper toType)
			{
				Contract.Requires<ArgumentNullException>(declaringType != null);
				Contract.Requires<ArgumentNullException>(innerNavigationProperty != null);
				Contract.Requires<ArgumentNullException>(toType != null);

				_declaringType = declaringType;
				_innerNavigationProperty = innerNavigationProperty;
				_toType = toType;
			}

			#region IEdmNavigationProperty and related

			public string Name
			{
				get { return _innerNavigationProperty.Name; }
			}

			public EdmPropertyKind PropertyKind
			{
				get { return EdmPropertyKind.Navigation; }
			}

			public IEdmTypeReference Type
			{
				get { return _innerNavigationProperty.Type; }
			}

			public IEdmStructuredType DeclaringType
			{
				get { return _declaringType; }
			}

			private void SetPartner(EdmNavigationPropertyWrapper parter)
			{
				if (ReferenceEquals(_partner, parter))
				{
					return;
				}
				if (_partner == null)
				{
					_partner = parter;
				}
				else
				{
					throw new InvalidOperationException("NavigationProperty.Partner cannot be changed to a new instance");
				}
			}

			public IEdmNavigationProperty Partner
			{
				get
				{
					if (_partner != null)
					{
						return _partner;
					}

					// Find partner navigation property on demand
					var innerPartner = _innerNavigationProperty.Partner;
					if (innerPartner == null)
					{
						return null;
					}
					var declaredPartner = _toType.FindProperty(innerPartner.Name) as EdmNavigationPropertyWrapper;
					if (declaredPartner != null)
					{
						_partner = declaredPartner;
					}
					else
					{	// Use an undeclared partner
						_partner = new EdmNavigationPropertyWrapper(_toType, innerPartner, _declaringType);
					}
					_partner.SetPartner(this);
					return _partner;
				}
			}

			public EdmOnDeleteAction OnDelete
			{
				get { return _innerNavigationProperty.OnDelete; }
			}

			public bool IsPrincipal
			{
				get { return _innerNavigationProperty.IsPrincipal; }
			}

			public IEnumerable<IEdmStructuralProperty> DependentProperties
			{
				get { return _innerNavigationProperty.DependentProperties; }
			}

			public bool ContainsTarget
			{
				get { return _innerNavigationProperty.ContainsTarget; }
			}

			#endregion
		}

	}

}