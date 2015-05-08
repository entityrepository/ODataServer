// -----------------------------------------------------------------------
// <copyright file="ScrumDbValidation.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Xml;
using Scrum.Model;
using Xunit;
using Xunit.Extensions;

namespace Scrum.Dal.IntegrationTests
{


	public class ScrumDbValidation : ScrumDbIntegrationTestBase
	{

		public ScrumDbValidation()
		{
			EnsureIntegrationDatabaseExists();
		}

		[Fact, AutoRollback]
		public void BasicVerificationOfScrumDb()
		{
			using (ScrumDb scrumDb = new ScrumDb())
			{
				try
				{
					User user = new User { UserName = Guid.NewGuid().ToString(), Email = "test@domain.com" };

					scrumDb.Users.Add(user);
					scrumDb.SaveChanges();
				}
				catch (DbEntityValidationException entityValidationException)
				{
					var entityValidationResult = entityValidationException.EntityValidationErrors.First();
					var validationError = entityValidationResult.ValidationErrors.First();
					throw;
				}
			}
		}

	}
}
