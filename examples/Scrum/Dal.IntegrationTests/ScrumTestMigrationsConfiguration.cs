using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using Scrum.Model;


namespace Scrum.Dal.IntegrationTests
{
	/// <summary>
	/// A standard migrations config that allows data loss during updates, and provides seed data.
	/// </summary>
	public class ScrumTestMigrationsConfiguration : DbMigrationsConfiguration<ScrumDb>
	{

		public ScrumTestMigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;
		}

		protected override void Seed(ScrumDb scrumDb)
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

			WorkItem item1 = new WorkItem(infraProject, gailUser, Priority.High)
			{
				//Number = 1,
				Title = "Some log entries are logged twice",
				Created = new DateTime(2013, 1, 12),
				Status = Status.WorkingOn,
				TimeEstimate = new TimeSpan(3, 0, 0),
				Description = "In the ingestion log, some of the log rows are logged twice."
			};
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

		}

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