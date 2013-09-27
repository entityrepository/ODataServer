// -----------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Threading.Tasks;

namespace EntityRepository.ODataServer.Util
{
	/// <summary>
	/// Helper methods for Task Parallel Library (TPL).
	/// </summary>
	internal static class TaskExtensions
	{

		public static Task EnsureStarted(this Task task)
		{
			if (task.Status == TaskStatus.Created)
			{
				task.Start();
			}
			return task;
		}

	}
}