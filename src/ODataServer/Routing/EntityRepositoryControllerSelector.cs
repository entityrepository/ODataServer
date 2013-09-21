// -----------------------------------------------------------------------
// <copyright file="EntityRepositoryControllerSelector.cs" company="EntityRepository Contributors" years="2013">
// This software is part of the EntityRepository library.
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace EntityRepository.ODataServer.Routing
{
	public class EntityRepositoryControllerSelector : IHttpControllerSelector
	{
		/// <summary>Name of controller key in route configuration</summary>
		internal const string ControllerKey = "controller";

		/// <summary>
		/// Installs an <see cref="EntityRepositoryControllerSelector"/> as the top level <see cref="IHttpControllerSelector"/> in <paramref name="webApiConfig"/>.
		/// </summary>
		/// <param name="webApiConfig"></param>
		/// <param name="oDataServerConfigurer"></param>
		/// <returns></returns>
		public static EntityRepositoryControllerSelector Install(HttpConfiguration webApiConfig, ODataServerConfigurer oDataServerConfigurer)
		{
			Contract.Requires<ArgumentNullException>(webApiConfig != null);
			Contract.Requires<ArgumentNullException>(oDataServerConfigurer != null);

			var instance = new EntityRepositoryControllerSelector(webApiConfig.Services, oDataServerConfigurer);
			if (instance._fallbackControllerSelector is EntityRepositoryControllerSelector)
			{
				// Skip duplicate installation
				return instance._fallbackControllerSelector as EntityRepositoryControllerSelector;
			}

			webApiConfig.Services.Replace(typeof(IHttpControllerSelector), instance);
			return instance;
		}

		private readonly IHttpControllerSelector _fallbackControllerSelector;

		// Mapping of controller names (which == EntitySet names) to controller descriptors
		private readonly Dictionary<string, HttpControllerDescriptor> _managedControllers;

		private EntityRepositoryControllerSelector(ServicesContainer servicesContainer, ODataServerConfigurer oDataServerConfigurer)
		{
			_fallbackControllerSelector = servicesContainer.GetHttpControllerSelector();
			_managedControllers = new Dictionary<string, HttpControllerDescriptor>(ODataServerConfigurer.InitialEntitySetCapacity, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entitySetName"></param>
		/// <param name="httpControllerDescriptor"></param>
		/// <exception cref="ArgumentException">If <paramref name="entitySetName"/> already exists in the collection.</exception>
		public void AddController(string entitySetName, HttpControllerDescriptor httpControllerDescriptor)
		{
			Contract.Requires<ArgumentException>(! string.IsNullOrWhiteSpace(entitySetName));
			Contract.Requires<ArgumentNullException>(httpControllerDescriptor != null);

			if (_fallbackControllerSelector.GetControllerMapping().ContainsKey(entitySetName))
			{
				throw new ArgumentException(string.Format("HttpControllerDescriptor with name '{0}' already exists in this Web API.", entitySetName));
			}

			_managedControllers.Add(entitySetName, httpControllerDescriptor);
		}

		/// <summary>
		/// Returns the mapping of names to <see cref="HttpControllerDescriptor"/> from the default <see cref="IHttpControllerSelector"/>'s mapping.
		/// </summary>
		public IDictionary<string, HttpControllerDescriptor> GetFallbackControllerMapping()
		{
			return _fallbackControllerSelector.GetControllerMapping();
		}

		public virtual string GetControllerName(HttpRequestMessage request)
		{
			Contract.Requires<ArgumentNullException>(request != null);

			IHttpRouteData routeData = request.GetRouteData();
			if (routeData == null)
			{
				return null;
			}

			// Look up controller in route data
			object controllerName = null;
			routeData.Values.TryGetValue(ControllerKey, out controllerName);
			return controllerName == null ? "" : controllerName.ToString();
		}

		#region IHttpControllerSelector Members

		public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
		{
			// Combine _fallbackControllerSelector's controller list and _managedControllers
			var controllerMapping = new Dictionary<string, HttpControllerDescriptor>(_fallbackControllerSelector.GetControllerMapping(), StringComparer.OrdinalIgnoreCase);
			foreach (var kvp in _managedControllers)
			{
				controllerMapping.Add(kvp.Key, kvp.Value);
			}
			return controllerMapping;
		}

		public HttpControllerDescriptor SelectController(HttpRequestMessage request)
		{
			string controllerName = GetControllerName(request);
			if (String.IsNullOrEmpty(controllerName))
			{
				throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound,
																			string.Format("Resource not found: {0}", request.RequestUri)));
			}

			HttpControllerDescriptor controllerDescriptor;
			if (_managedControllers.TryGetValue(controllerName, out controllerDescriptor))
			{
				return controllerDescriptor;
			}
			else
			{
				return _fallbackControllerSelector.SelectController(request);
			}
		}

		#endregion

	}
}