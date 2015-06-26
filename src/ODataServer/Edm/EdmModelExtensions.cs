// // -----------------------------------------------------------------------
// <copyright file="EdmModelExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Edm
{

	/// <summary>
	/// Extension methods for managing an <see cref="IEdmModel"/> instance.
	/// </summary>
	public static class EdmModelExtensions
	{
		internal const string ClrTypeAnnotationNamespace = "http://schemas.microsoft.com/ado/2013/11/edm/customannotation";
		internal const string ClrTypeAnnotationName = "ClrType";
		internal const string UseClrTypesAnnotationName = "UseClrTypes";

		/// <summary>
		/// Removes <c>xmlns:p5="http://schemas.microsoft.com/ado/2013/11/edm/customannotation" Name="Priority" p5:ClrType="Scrum.Model.Priority, Scrum.Model, Version=0.8.1.0, Culture=neutral, PublicKeyToken=null"</c> from the EdmModel.
		/// </summary>
		/// <param name="edmModel"></param>
		public static void RemoveClrTypeAnnotations(this IEdmModel edmModel)
		{
			Contract.Requires<ArgumentNullException>(edmModel != null);

			// Clear <EntityType Name="Order" p5:ClrType="EntityRepository.ODataServer.UnitTests.EStore.Model.Order, EntityRepository.ODataServer.UnitTests, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null"
			foreach (var edmEntityType in edmModel.SchemaElements.OfType<IEdmEntityType>())
			{
				edmModel.DirectValueAnnotationsManager.SetAnnotationValue(edmEntityType, ClrTypeAnnotationNamespace, ClrTypeAnnotationName, null);
			}
			// Clear <EntityContainer ... p5:UseClrTypes="true" 
			foreach (var edmContainer in edmModel.SchemaElements.OfType<IEdmEntityContainer>())
			{
				edmModel.DirectValueAnnotationsManager.SetAnnotationValue(edmContainer, ClrTypeAnnotationNamespace, UseClrTypesAnnotationName, null);
			}
		}

		// Equivalent to model.GetEntitySetUrl(e).ToString()- which is internal
		public static string GetEntitySetUrl(this IEdmModel edmModel, IEdmEntitySet edmEntitySet)
		{
			object o = edmModel.DirectValueAnnotationsManager.GetAnnotationValue(edmEntitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "System_Web_Http_OData_Builder_EntitySetUrlAnnotation");
			if (o == null)
			{
				// throw new InvalidOperationException("Couldn't find EntitySetUrlAnnotation for EntitySet " + edmEntitySet.Name);
				return edmEntitySet.Name;
			}
			PropertyInfo propertyInfo = o.GetType().GetProperty("Url", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (propertyInfo != null)
			{
				return propertyInfo.GetValue(o).ToString();
			}
			else
			{
				return edmEntitySet.Name;
			}
		}

	}

}