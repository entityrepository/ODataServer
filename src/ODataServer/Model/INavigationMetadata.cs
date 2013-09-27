// -----------------------------------------------------------------------
// <copyright file="INavigationMetadata.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Encapsulates a navigation property, plus the entityset that it references.
	/// </summary>
	public interface INavigationMetadata
	{
		string NavigationPropertyName { get; }

		bool IsCollection { get; }

		/// <summary>
		/// Gets the navigation property.
		/// </summary>
		IEdmNavigationProperty EdmNavigationProperty { get; }

		/// <summary>
		/// Gets the target entityset.
		/// </summary>
		IEntitySetMetadata TargetEntitySet { get; }
	}
}