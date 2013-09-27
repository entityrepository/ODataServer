// -----------------------------------------------------------------------
// <copyright file="NavigationMetadata.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Diagnostics.Contracts;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer.Model
{
	public class NavigationMetadata : INavigationMetadata
	{

		internal NavigationMetadata(IEdmNavigationProperty edmNavigationProperty, IEntitySetMetadata targetEntitySet)
		{
			Contract.Assert(edmNavigationProperty != null);
			Contract.Assert(targetEntitySet != null);

			EdmNavigationProperty = edmNavigationProperty;
			TargetEntitySet = targetEntitySet;
		}

		public string NavigationPropertyName { get { return EdmNavigationProperty.Name; } }
		public bool IsCollection { get { return EdmNavigationProperty.Type.IsCollection(); } }
		public IEdmNavigationProperty EdmNavigationProperty { get; private set; }
		public IEntitySetMetadata TargetEntitySet { get; private set; }

	}
}