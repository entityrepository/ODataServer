// -----------------------------------------------------------------------
// <copyright file="IContainerMetadataOfT.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


namespace EntityRepository.ODataServer.Model
{
	/// <summary>
	/// Marker interface to support associating <see cref="IContainerMetadata"/> with a specific type.
	/// This is useful for distinguishing classes in dependency injection when multiple container types are
	/// used in a single application.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IContainerMetadata<T> : IContainerMetadata
		where T : class
	{}

}