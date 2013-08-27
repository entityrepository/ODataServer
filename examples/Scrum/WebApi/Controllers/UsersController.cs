// -----------------------------------------------------------------------
// <copyright file="UsersController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using Scrum.Dal;
using Scrum.Model;

namespace Scrum.WebApi.Controllers
{
	public class UsersController : EntitySetController<User, int>
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

		[Queryable]
		public override IQueryable<User> Get()
		{
			return Db.Users;
		}

		protected override User GetEntityByKey(int key)
		{
			return Db.Users.Find(key);
		}

	}
}
