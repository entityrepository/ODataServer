// -----------------------------------------------------------------------
// <copyright file="IModule.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using SimpleInjector;


namespace EntityRepository.ODataServer.Ioc
{
	/// <summary>
	/// A module for exposing a set of related IoC configuration.
	/// </summary>
	public interface IModule
	{

		/// <summary>
		/// Registers IoC settings in <paramref name="container"/>.
		/// </summary>
		/// <param name="container">A SimpleInjector <see cref="Container"/>.</param>
		void RegisterServices(Container container);

	}

}
