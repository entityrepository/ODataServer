// -----------------------------------------------------------------------
// <copyright file="odata.svc.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Services;
using System.Data.Services.Common;
using System.ServiceModel;
using Scrum.Dal;

namespace Scrum.Web
{

	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class ScrumDataService : DataService<ScrumDb>
	{

		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{
			// Set rules to indicate which entity sets and service operations are visible, updatable, etc.
			config.SetEntitySetAccessRule("Users", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("*", EntitySetRights.All);
			config.SetEntitySetPageSize("*", 50);

#if DEBUG
			config.UseVerboseErrors = true;
#endif
			config.DataServiceBehavior.IncludeAssociationLinksInResponse = true;
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
		}

		protected override ScrumDb CreateDataSource()
		{
			ScrumDb db = new ScrumDb();

			// This is needed b/c Id values are passed back from the client, which may reference entities that aren't yet loaded.
			db.Configuration.ValidateOnSaveEnabled = false;
			return db;
		}

		protected override void OnStartProcessingRequest(ProcessRequestArgs args)
		{
			base.OnStartProcessingRequest(args);
		}

		protected override void HandleException(HandleExceptionArgs args)
		{
			base.HandleException(args);
		}

	}
}
