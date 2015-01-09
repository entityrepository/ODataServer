// // -----------------------------------------------------------------------
// <copyright file="Fact.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


namespace Scrum.WebApi.IntegrationTests.xunitExt
{

	/// <summary>
	/// Overrides some of the default values for a Fact.
	/// </summary>
	public sealed class FactAttribute : Xunit.FactAttribute
	{
		/// <summary>
		/// Default test timeout.
		/// </summary>
		public const int DefaultTimeOut = 20 * 1000;

		public FactAttribute()
		{
			Timeout = DefaultTimeOut;
		}
	}

}