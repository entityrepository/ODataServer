// -----------------------------------------------------------------------
// <copyright file="UsersController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Scrum.Dal;
using Scrum.Model;

namespace Scrum.WebApi.Controllers
{
	public class UsersController : ODataController
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

		[EnableQuery]
		public IQueryable<User> Get()
		{
			return Db.Users;
		}

		[EnableQuery]
		protected SingleResult<User> GetById([FromODataUri] int id)
		{
			return SingleResult.Create(Db.Users.Where(user => user.Id == id));
		}

	}
}
