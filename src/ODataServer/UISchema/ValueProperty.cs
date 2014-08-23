// // -----------------------------------------------------------------------
// <copyright file="ValueProperty.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Web.Http.Metadata;

namespace EntityRepository.ODataServer.UISchema
{

	/// <summary>
	/// JSON response model for a property that holds a value (string, int, bool).
	/// </summary>
	public sealed class ValueProperty : Property
	{


		//public ValueProperty()
		//{}

		public ValueProperty(Type propertyType, IUiMetadataSource uiMetadataSource)
			: base(propertyType, uiMetadataSource)
		{}

		public string TypeScriptType
		{
			get {}
		}

		public string JavaScriptType
		{
			get
			{
			}
		}

		public string EditorType
		{
			get
			{
			}
		}

		private string MapToScriptType(bool useTypeScript)
		{

		}
	}


}