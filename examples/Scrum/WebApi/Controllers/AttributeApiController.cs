// -----------------------------------------------------------------------
// <copyright file="AttributeApiController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Http;

namespace Scrum.WebApi.Controllers
{

	/// <summary>
	/// Simple attribute-based Web API controller, just to verify that attribute-based routing doesn't conflict with
	/// the odata implementation.
	/// </summary>
	[RoutePrefix("api")]
	public class AttributeApiController : ApiController
	{

		[Route("")]
		public string[] Get()
		{
			return new[] { "value1", "value2" };
		}

	}
}
