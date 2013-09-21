// -----------------------------------------------------------------------
// <copyright file="ActionMapExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Linq;
using System.Web.Http.Controllers;

namespace EntityRepository.ODataServer.Routing
{

	/// <summary>
	/// Provides helper methods for querying an action map.
	/// </summary>
	internal static class ActionMapExtensions
	{
		public static string FindMatchingAction(this ILookup<string, HttpActionDescriptor> actionMap, string targetActionName, string fallbackActionName)
		{
			if (actionMap.Contains(targetActionName))
			{
				return targetActionName;
			}
			else if (actionMap.Contains(fallbackActionName))
			{
				return fallbackActionName;
			}

			return null;
		}

		public static string FindMatchingAction(this ILookup<string, HttpActionDescriptor> actionMap, string targetActionName)
		{
			if (actionMap.Contains(targetActionName))
			{
				return targetActionName;
			}

			return null;
		}
	}

}