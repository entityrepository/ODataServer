// -----------------------------------------------------------------------
// <copyright file="ScrumDbTestDatabaseInitializer.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Scrum.Model;

namespace Scrum.Dal.IntegrationTests
{


	/// <summary>
	/// <see cref="IDatabaseInitializer{TContext}"/> for an integration test instance of <see cref="ScrumDb"/>.
	/// </summary>
	public class ScrumDbTestDatabaseInitializer : IDatabaseInitializer<ScrumDb>
	{

		// Modified from DropCreateDatabaseIfModelChanges<TContext>
		// TODO: Move to Base.Data...
		public void InitializeDatabase(ScrumDb context)
		{
			bool flag;
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
				flag = context.Database.Exists();
			}

			if (flag)
			{
				try
				{
					if (context.Database.CompatibleWithModel(true))
					{
						return;
					}
				}
				catch (NotSupportedException /*nse*/)
				{} // Occurs when the last call to Database.Create didn't succeed; if this happens, try creating it again.

				context.Database.Delete();
			}

			context.Database.Create();
			Seed(context);
			context.SaveChanges();
		}

		protected /*override*/ void Seed(ScrumDb scrumDb)
		{
			// Add DbEnum-like values
			SeedStaticReadOnlyFieldValues<Priority>(scrumDb);
			SeedStaticReadOnlyFieldValues<Status>(scrumDb);

			User joeUser = new User { UserName = "joe", Email = "joe@domain.com" };
			scrumDb.Users.Add(joeUser);
			User gailUser = new User { UserName = "gail", Email = "gail@domain.com" };
			scrumDb.Users.Add(gailUser);

			Project infraProject = scrumDb.Projects.Create();
			infraProject.Key = "INFRA";
			infraProject.Name = "Infrastructure";
			infraProject.Description = "Infrastructure tasks for development.";
			infraProject.Owners.Add(gailUser);
			scrumDb.Projects.Add(infraProject);

			infraProject.Areas.Add(new ProjectArea { Name = "Configuration" });
			ProjectArea loggingArea = new ProjectArea { Name = "Logging" };
			loggingArea.Owners.Add(gailUser);
			infraProject.Areas.Add(loggingArea);
			ProjectArea sourceControlArea = new ProjectArea { Name = "Source Control" };
			sourceControlArea.Owners.Add(joeUser);
			infraProject.Areas.Add(sourceControlArea);

			ProjectVersion version020 = new ProjectVersion
			                            {
				                            Name = "0.2.0",
				                            ReleaseDate = new DateTime(2012, 10, 1),
				                            IsReleased = true
			                            };
			infraProject.Versions.Add(version020);
			infraProject.Versions.Add(new ProjectVersion { Name = "0.4.0" });
			infraProject.Versions.Add(new ProjectVersion { Name = "1.0" });
			infraProject.Versions.Add(new ProjectVersion { Name = "Backlog" });

			WorkItem item1 = new WorkItem
			                 {
				                 //Number = 1,
				                 Title = "Some log entries are logged twice",
				                 Creator = gailUser,
				                 Created = new DateTime(2013, 1, 12),
				                 Priority = Priority.High,
				                 Status = Status.WorkingOn,
				                 TimeEstimate = new TimeSpan(3, 0, 0),
				                 Description = "In the ingestion log, some of the log rows are logged twice."
			                 };
			infraProject.WorkItems.Add(item1);
			item1.AssignedTo.Add(gailUser);
			item1.Subscribers.Add(joeUser);
			item1.Areas.Add(loggingArea);
			item1.AffectsVersions.Add(version020);
			item1.TimeLog.Add(new WorkItemTimeLog { Worker = gailUser, TimeWorked = new TimeSpan(1, 20, 0), Comments = "Initial investigation." });


			Project dwProject = scrumDb.Projects.Create();
			dwProject.Key = "DW";
			dwProject.Name = "Data Warehouse";
			dwProject.Description = "Data Warehouse and BI development";
			dwProject.Owners.Add(gailUser);
			scrumDb.Projects.Add(dwProject);

			dwProject.Areas.Add(new ProjectArea { Name = "Staging" });
			dwProject.Areas.Add(new ProjectArea { Name = "Presentation" });
			ProjectArea reportingArea = new ProjectArea { Name = "Reporting" };
			reportingArea.Owners.Add(joeUser);
			dwProject.Areas.Add(reportingArea);

			dwProject.Versions.Add(new ProjectVersion { Name = "0.8.1" });
			dwProject.Versions.Add(new ProjectVersion { Name = "1.0" });
			dwProject.Versions.Add(new ProjectVersion { Name = "Backlog" });

			try
			{
				scrumDb.SaveChanges();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw new InvalidOperationException("Failure saving seed data", ex);
			}
		}

		// TODO: Move this to Base.Data
		public static void SeedStaticReadOnlyFieldValues<TEntity>(DbContext dbContext) where TEntity : class
		{
			DbSet<TEntity> dbSet = dbContext.Set<TEntity>();

			Type entityType = typeof(TEntity);
			var staticPropertyValues = new List<TEntity>();
			foreach (FieldInfo staticProperty in entityType.GetFields(BindingFlags.Static | BindingFlags.Public).Where(fi => fi.FieldType == entityType && fi.IsInitOnly))
			{
				TEntity staticValue = (TEntity) staticProperty.GetValue(null);
				staticPropertyValues.Add(staticValue);
			}
			dbSet.AddOrUpdate(staticPropertyValues.ToArray());
		}

	}
}
