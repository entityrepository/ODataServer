// // -----------------------------------------------------------------------
// <copyright file="IUIMetadataSource.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;

namespace EntityRepository.ODataServer.UISchema
{

	/// <summary>
	/// Used to determine UI metadata from CLR types.
	/// </summary>
	public interface IUiMetadataSource
	{

		string GetJavaScriptType(TypeCode primitiveType);

		string GetTypeScriptType(Type type);

		string GetEditorType(Type type);

	}

}