// -----------------------------------------------------------------------
// <copyright file="ScrumDbIntegrationTestBase.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;

namespace Scrum.Dal.IntegrationTests
{


	/// <summary>
	/// Common functionality for integration tests that use a <see cref="ScrumDb"/>.
	/// </summary>
	public abstract class ScrumDbIntegrationTestBase
	{

		protected ScrumDbIntegrationTestBase()
		{
			Database.SetInitializer(new ScrumDbTestDatabaseInitializer());
		}

		public void EnsureIntegrationDatabaseExists()
		{
			using (ScrumDb scrumDb = new ScrumDb())
			{
				scrumDb.Database.Initialize(false);
			}
		}

	}
}
