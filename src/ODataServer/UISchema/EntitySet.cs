// // -----------------------------------------------------------------------
// <copyright file="UIEntitySet.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using EntityRepository.ODataServer.Model;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.UISchema
{

	/// <summary>
	/// JSON response model for the UI settings for an EntitySet.
	/// </summary>
	public sealed class EntitySet
	{

		private List<ValueProperty> _keyProperties;
		private List<Property> _properties;

		public EntitySet(IEntitySetMetadata entitySet, IUiMetadataSource uiMetadataSource)
		{
			Contract.Requires<ArgumentNullException>(entitySet != null);
			Contract.Requires<ArgumentNullException>(uiMetadataSource != null);

			if (entitySet.ElementTypeHierarchyMetadata.Count() != 1)
			{
				throw new NotImplementedException("Not implemented: UI Metadata for EntitySets that contain a type hierarchy");
			}

			_keyProperties = new List<ValueProperty>();
			_properties = new List<Property>();
		}

		private void InitializeFromEntitySetMetadata(IEntitySetMetadata entitySet, IUiMetadataSource uiMetadataSource)
		{
			EntitySetName = entitySet.Name;
			EntitySetDisplayName = "TODO"; // TODO: Logic for determining EntitySet display name

			var entityType = entitySet.ElementTypeHierarchyMetadata.Single();

			foreach (var propInfo in entityType.ClrKeyProperties)
			{
				_keyProperties.Add(new ValueProperty(propInfo.PropertyType, uiMetadataSource)
				                   {
					                   PropertyName = propInfo.Name,
					                   IsReadOnly = true,
					                   IsRequired = true
				                   });
			}

			foreach (var edmProperty in entityType.EdmType.Properties())
			{
				if (edmProperty is IEdmNavigationProperty)
				{
					var navProperty = entitySet.NavigationProperties.Single(p => p.NavigationPropertyName == edmProperty.Name);
				}
				else if (edmProperty is IEdmStructuralProperty)
				{

				}
				else
				{
					throw new NotImplementedException("No support provided for IEdmProperty instances that aren't a navigational property or a structural property.");
				}
			}
		}

		public string EntitySetName { get; set; }

		public string EntitySetDisplayName
		{
			get;
			set;
		}

		public Uri ODataUri { get; set; }

		public IEnumerable<Property> Properties
		{
			get
			{
			 }
		}


	}

}