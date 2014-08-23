// // -----------------------------------------------------------------------
// <copyright file="Property.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Web.Http.Metadata;

namespace EntityRepository.ODataServer.UISchema
{

	/// <summary>
	/// Base class for UI settings for a single property in a domain model.
	/// </summary>
	public abstract class Property
	{

		protected readonly Type _propertyType;
		protected readonly IUiMetadataSource _uiMetadataSource;

		public Property(Type propertyType, IUiMetadataSource uiMetadataSource)
		{
			Contract.Requires<ArgumentNullException>(propertyType != null);
			Contract.Requires<ArgumentNullException>(uiMetadataSource != null);

			_propertyType = propertyType;
			_uiMetadataSource = uiMetadataSource;
		}

		protected Property(ModelMetadata modelMetadata, IUiMetadataSource uiMetadataSource)
		{
			Contract.Requires<ArgumentNullException>(modelMetadata != null);
			Contract.Requires<ArgumentNullException>(uiMetadataSource != null);

			_propertyType = modelMetadata.ModelType;
			_uiMetadataSource = uiMetadataSource;

			PropertyName = modelMetadata.PropertyName;
			DisplayName = modelMetadata.GetDisplayName();
			Description = modelMetadata.Description;
			IsReadOnly = modelMetadata.IsReadOnly;
			IsRequired = true; // TODO
		}

		public string PropertyName { get; set; }

		public string DisplayName { get; set; }

		public string Description { get; set; }

		public bool IsReadOnly { get; set; }
		public bool IsRequired { get; set; }

		public string ClrType
		{
			get { return _propertyType.FullName; }
		}

		public virtual string TypeScriptType
		{
			get { return _uiMetadataSource.GetTypeScriptType(_propertyType); }
		}

		public virtual string JavaScriptType
		{
			get { return _uiMetadataSource.GetJavaScriptType(_propertyType); }
		}

		public virtual string EditorType
		{
			get { return _uiMetadataSource.GetEditorType(_propertyType); }
		}

	}

}