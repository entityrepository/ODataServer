// -----------------------------------------------------------------------
// <copyright file="RenamedReflectedHttpActionDescriptor.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Reflection;
using System.Web.Http.Controllers;

namespace EntityRepository.ODataServer.Routing
{
	/// <summary>
	/// Adds support for renaming an action to <see cref="ReflectedHttpActionDescriptor"/>.
	/// </summary>
	internal class RenamedReflectedHttpActionDescriptor : ReflectedHttpActionDescriptor
	{

		private readonly string _actionName;

		public RenamedReflectedHttpActionDescriptor(HttpControllerDescriptor controllerDescriptor, MethodInfo methodInfo, string actionName)
			: base(controllerDescriptor, methodInfo)
		{
			_actionName = actionName;
		}

		public override string ActionName
		{
			get
			{
				return _actionName;
			}
		}
	}
}