// -----------------------------------------------------------------------
// <copyright file="ProjectsController.cs" company="PrecisionDemand">
// Copyright (c) 2013 PrecisionDemand.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Scrum.Dal;
using Scrum.Model;

namespace Scrum.WebApi.Controllers
{
	public class ProjectsController : EntitySetController<Project, int>
	{

		private ScrumDb _db;

		protected ScrumDb Db
		{
			get
			{
				if (_db == null)
				{
					_db = new ScrumDb();
				}
				return _db;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_db != null)
				{
					_db.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		[Queryable(AllowedQueryOptions = AllowedQueryOptions.All)]
		public override IQueryable<Project> Get()
		{
			return Db.Projects.Include(p => p.Areas).Include(p => p.Versions);
		}

		protected override Project GetEntityByKey(int key)
		{
			return Db.Projects.Find(key);
		}

		public override System.Net.Http.HttpResponseMessage HandleUnmappedRequest(System.Web.Http.OData.Routing.ODataPath odataPath)
		{
			return base.HandleUnmappedRequest(odataPath);
		}
	}
}
