// -----------------------------------------------------------------------
// <copyright file="ScrumDb.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Scrum.Model;

namespace Scrum.Dal
{

	/// <summary>
	/// DbContext for the Scrum example.
	/// </summary>
	/// <remarks>
	/// Runs against EF 6.0.0 in Scrum.Dal project; runs agains EF 5.0.0 in Scrum.WcfDataService project.
	/// </remarks>
	public class ScrumDb : DbContext
	{

// For EF 5.0, set a null DB initializer, so we can use the same database between EF 5 and EF 6.
#if ! EF6
		static ScrumDb()
		{
			Database.SetInitializer<ScrumDb>(null);	
		}
#endif

		public const string DatabaseName = "Scrum";

		public ScrumDb()
			: base(DatabaseName)
		{
			Configuration.AutoDetectChangesEnabled = false;
			Configuration.LazyLoadingEnabled = false;
			Configuration.ProxyCreationEnabled = false;

			// By default, this is on, but should be disabled for data services server context.
			Configuration.ValidateOnSaveEnabled = true;

#if DEBUG && EF6
			if (Debugger.IsAttached)
			{
				Database.Log = s => Debug.WriteLine(s);
			}
#endif

			AttachDbEnums();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void AttachDbEnums()
		{
			if (! Database.Exists())
			{
				// Skip this when the database doesn't exist.
				return;
			}

			foreach (var priority in Scrum.Model.Priority.All)
			{
				Priority.Attach(priority);
			}
			foreach (var status in Scrum.Model.Status.All)
			{
				Status.Attach(status);
			}
		}

		protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
		{
			// TODO: We could do our custom validation here, and turn validation back on...
			DbEntityValidationResult validationResult = base.ValidateEntity(entityEntry, items);
			if (! validationResult.IsValid)
			{
				Debug.WriteLine("Error validating {0} ({1}) :", validationResult.Entry.Entity, validationResult.Entry.State);
				foreach (var validationError in validationResult.ValidationErrors)
				{
					Debug.WriteLine("  {0} : {1}", validationError.PropertyName, validationError.ErrorMessage);
				}
			}
			return validationResult;
		}

		public override int SaveChanges()
		{
			ChangeTracker.DetectChanges();
			return base.SaveChanges();
		}

		public DbSet<Client> Clients { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<ProjectArea> ProjectAreas { get; set; }
		public DbSet<ProjectVersion> ProjectVersions { get; set; }
		public DbSet<Sprint> Sprints { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<WorkItem> WorkItems { get; set; }
		public DbSet<WorkItemMessage> WorkItemMessages { get; set; }
		public DbSet<WorkItemPropertyChange> WorkItemPropertyChanges { get; set; }
		public DbSet<WorkItemTimeLog> WorkItemTimeLog { get; set; }

		public DbSet<Priority> Priority { get; set; }
		public DbSet<Status> Status { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<WorkItemTimeLog>().ToTable("WorkItemTimeLog");

			var workItemConfig = modelBuilder.Entity<WorkItem>();
			workItemConfig.HasMany(workItem => workItem.AssignedTo).WithMany()
			              .Map(manyToManyConfig => manyToManyConfig.ToTable("WorkItemAssignedTo"));
			workItemConfig.HasMany(workItem => workItem.Subscribers).WithMany()
			              .Map(manyToManyConfig => manyToManyConfig.ToTable("WorkItemSubscribers"));
			workItemConfig.HasMany(workItem => workItem.AffectsVersions).WithMany()
			              .Map(manyToManyConfig => manyToManyConfig.ToTable("WorkItemAffectsVersions"));
			workItemConfig.HasMany(workItem => workItem.FixVersions).WithMany()
			              .Map(manyToManyConfig => manyToManyConfig.ToTable("WorkItemFixVersions"));
			workItemConfig.HasMany(workItem => workItem.Areas).WithMany()
			              .Map(manyToManyConfig => manyToManyConfig.ToTable("WorkItemAreas"));

			modelBuilder.Entity<Project>().HasMany(project => project.Owners).WithMany()
			            .Map(manyToManyConfig => manyToManyConfig.ToTable("ProjectOwners"));

			modelBuilder.Entity<ProjectArea>().HasMany(projectArea => projectArea.Owners).WithMany()
			            .Map(manyToManyConfig => manyToManyConfig.ToTable("ProjectAreaOwners"));

			modelBuilder.Entity<WorkItemMessage>().HasRequired(m => m.Author).WithMany().WillCascadeOnDelete(false);

			modelBuilder.Entity<WorkItemPropertyChange>().HasRequired(c => c.Author).WithMany().WillCascadeOnDelete(false);
			modelBuilder.Entity<WorkItemPropertyChange>().HasRequired(c => c.WorkItem).WithMany();

			modelBuilder.Entity<WorkItemTimeLog>().HasRequired(l => l.Worker).WithMany().WillCascadeOnDelete(false);

			// For the DbEnum subclasses, turn off autoincrement so IDs of 0 can work
			modelBuilder.Entity<Priority>().Property(priority => priority.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			modelBuilder.Entity<Status>().Property(status => status.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

			// Required, since there's no way to selectively disable many-to-many cascade delete.
			modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

		}

	}


}
