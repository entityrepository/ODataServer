// -----------------------------------------------------------------------
// <copyright file="ODataServiceConfig.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.Routing;
using EntityRepository.ODataServer.Util;
using Microsoft.Data.Edm;

namespace EntityRepository.ODataServer
{

	public class ODataServiceConfig
		: IHttpControllerSelector
	{

		/// <summary>Starter size for collections holding EntitySets</summary>
		internal const int InitialEntitySetCapacity = 30;

		/// <summary>Name of controller key in route configuration</summary>
		internal const string ControllerKey = "controller";

		// All controllers must derive from this class.
// ReSharper disable StaticFieldInGenericType
		public static readonly Type RequiredBaseController = typeof(ODataController);
// ReSharper restore StaticFieldInGenericType

		private readonly HttpConfiguration _webApiConfig;

		private readonly IHttpControllerSelector _fallbackControllerSelector;

		// Mapping of EntitySet names to EntitySet types
		private readonly Dictionary<string, Type> _entitySets;
		// Mapping of controller names (which == EntitySet names) to controller descriptors
		private readonly Dictionary<string, HttpControllerDescriptor> _managedControllers;

		public ODataServiceConfig(HttpConfiguration webApiConfig)
		{
			Contract.Requires<ArgumentNullException>(webApiConfig != null);

			_webApiConfig = webApiConfig;

			_fallbackControllerSelector = webApiConfig.Services.GetHttpControllerSelector();

			_entitySets = new Dictionary<string, Type>(InitialEntitySetCapacity, StringComparer.OrdinalIgnoreCase);
			_managedControllers = new Dictionary<string, HttpControllerDescriptor>(InitialEntitySetCapacity, StringComparer.OrdinalIgnoreCase);

			webApiConfig.Services.Replace(typeof(IHttpControllerSelector), this);
		}

		public void AddEntitySetController(string entitySetName, Type entityType, Type controllerType)
		{
			Contract.Requires<ArgumentException>(! string.IsNullOrWhiteSpace(entitySetName));
			Contract.Requires<ArgumentNullException>(entityType != null);
			Contract.Requires<ArgumentNullException>(controllerType != null);
			Contract.Requires<ArgumentException>(RequiredBaseController.IsAssignableFrom(controllerType), "Controller types must derive from System.Web.Http.OData.ODataController");

			_entitySets.Add(entitySetName, entityType);
			_managedControllers.Add(entitySetName, new HttpControllerDescriptor(_webApiConfig, entitySetName, controllerType));
		}


		public delegate Type DbSetControllerSelector(Type entityType, Type keyType, Type dbContextType);


		public void AddDbContextControllers<TDbContext>()
			where TDbContext : DbContext
		{
			AddDbContextControllers<TDbContext>((entityType, keyType, dbContextType) => typeof(EditDbSetController<,,>).MakeGenericType(entityType, keyType, dbContextType));
		}

		public void AddDbContextControllers<TDbContext>(DbSetControllerSelector dbSetControllerSelector)
			where TDbContext : DbContext
		{
			Contract.Requires<ArgumentNullException>(dbSetControllerSelector != null);

			// All existing controllers are defined in the fallback controller
			IDictionary<string, HttpControllerDescriptor> existingControllers = _fallbackControllerSelector.GetControllerMapping();

			// TDbContext and EntityKeyHelper must be registered with the DI container
			Type dbContextType = typeof(TDbContext);
			EntityKeyHelper<TDbContext> entityKeyHelper = (EntityKeyHelper<TDbContext>) _webApiConfig.DependencyResolver.GetService(typeof(EntityKeyHelper<TDbContext>));
			TDbContext dbContext = (TDbContext) _webApiConfig.DependencyResolver.GetService(dbContextType);

			using (dbContext)
			{
				foreach (var property in GetDbSetProperties(dbContextType))
				{
					// Add a new controller for each DbSet<> that doesn't already have a controller
					string entitySetName = property.Name;
					HttpControllerDescriptor controllerDescriptor;
					if (! existingControllers.TryGetValue(entitySetName, out controllerDescriptor))
					{
						// Add a controller for this DbSet
						Type entityType = property.PropertyType.GetGenericArguments()[0];

						// Determine the key type
						Type keyType = entityKeyHelper.SingleKeyPropertyForEntity(dbContext, entityType).PropertyType;

						// Determine the controller type
						Type controllerType = dbSetControllerSelector(entityType, keyType, dbContextType);

						AddEntitySetController(entitySetName, entityType, controllerType);
					}
				}
			}
		}

		internal static IEnumerable<PropertyInfo> GetDbSetProperties(Type dbContextType)
		{
			Type dbsetTypeDefiniction = typeof(DbSet<>);
			return dbContextType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.IsGenericType(dbsetTypeDefiniction));
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

		/// <summary>
		/// Builds the <see cref="IEdmModel"/> for the odata service based on the entityset controllers that have been configured.
		/// </summary>
		/// <param name="edmNamespace"></param>
		/// <returns></returns>
		public IEdmModel BuildEdmModel(string edmNamespace = null)
		{
			ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder(_webApiConfig);
			if (edmNamespace != null)
			{
				modelBuilder.Namespace = edmNamespace;
			}

			ConfigureModelBuilder(modelBuilder);

			//var projectsConfig = modelBuilder.EntitySet<Project>("Projects");
			//projectsConfig.HasManyBinding(p => p.Owners, modelBuilder.EntitySet<User>("Users"));
			//projectsConfig.HasManyBinding(p => p.Areas, modelBuilder.EntitySet<ProjectArea>("ProjectAreas"));
			//projectsConfig.HasManyBinding(p => p.Versions, modelBuilder.EntitySet<ProjectVersion>("ProjectVersions"));
			//projectsConfig.HasManyBinding(p => p.WorkItems, modelBuilder.EntitySet<WorkItem>("WorkItems"));

			return modelBuilder.GetEdmModel();
		}

		/// <summary>
		/// If a caller wants to tweak the <see cref="ODataModelBuilder"/>, this method can be used instead of <see cref="BuildEdmModel"/>,
		/// so that the caller can modify <paramref name="modelBuilder"/> before or after the entity sets are added.
		/// </summary>
		/// <param name="modelBuilder">An <see cref="ODataModelBuilder"/> or <see cref="ODataConventionModelBuilder"/>.</param>
		public void ConfigureModelBuilder(ODataModelBuilder modelBuilder)
		{
			foreach (KeyValuePair<string, Type> kvp in _entitySets)
			{
				string entitySetName = kvp.Key;
				Type entityType = kvp.Value;
				EntityTypeConfiguration entityTypeConfig = modelBuilder.AddEntity(entityType);
				EntitySetConfiguration entitySetConfig = modelBuilder.AddEntitySet(entitySetName, entityTypeConfig);
			}
		}

	}
}
