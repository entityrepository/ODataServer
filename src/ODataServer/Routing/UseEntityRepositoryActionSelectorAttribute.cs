// -----------------------------------------------------------------------
// <copyright file="UseEntityRepositoryActionSelectorAttribute.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using EntityRepository.ODataServer.Model;

namespace EntityRepository.ODataServer.Routing
{

	/// <summary>
	/// A controller-level attribute that can be used to enable OData action selection including support for the extensions in this library.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class UseEntityRepositoryActionSelectorAttribute : Attribute, IControllerConfiguration
	{
		/// <summary>
		/// Callback invoked to set per-controller overrides for this controllerDescriptor.
		/// </summary>
		/// <param name="controllerSettings">The controller settings to initialize.</param>
		/// <param name="controllerDescriptor">The controller descriptor. Note that the <see
		/// cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> can be associated with the derived
		/// controller type given that <see cref="T:System.Web.Http.Controllers.IControllerConfiguration" /> is
		/// inherited.</param>
		public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
		{
			if (controllerSettings == null)
			{
				throw new ArgumentNullException("controllerSettings");
			}

			if (controllerDescriptor == null)
			{
				throw new ArgumentNullException("controllerDescriptor");
			}

			ServicesContainer services = controllerSettings.Services;
			Contract.Assert(services != null);

			IContainerMetadata containerMetadata = controllerDescriptor.GetContainerMetadata();

			// Replace the action selector with one that is based on the OData routing conventions
			IHttpActionSelector originalActionSelector = services.GetActionSelector();
			IHttpActionSelector actionSelector;
			if (containerMetadata != null)
			{
				// ContainerMetadata was stored with the HttpControllerDescriptor - so use our "special" ActionSelector
				actionSelector = new EntityRepositoryActionSelector(containerMetadata, originalActionSelector);
			}
			else
			{
				// No ContainerMetadata stored with the HttpControllerDescriptor - so use the standard odata ActionSelector
				actionSelector = new ODataActionSelector(originalActionSelector);
			}
			controllerSettings.Services.Replace(typeof(IHttpActionSelector), actionSelector);
		}

	}}