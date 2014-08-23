// -----------------------------------------------------------------------
// <copyright file="UISchemaController.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Metadata;
using EntityRepository.ODataServer.Model;

namespace EntityRepository.ODataServer.UISchema
{
	/// <summary>
	/// Returns UI schema info for the entitysets exposed via OData.
	/// </summary>
	[RoutePrefix("ui-schema")]
	public class UiSchemaController : ApiController
	{

		private readonly IContainerMetadata _containerMetadata;
		private readonly ModelMetadataProvider _modelMetadataProvider;
		private readonly IUiMetadataSource _uiMetadataSource;

		public UiSchemaController(IContainerMetadata containerMetadata, ModelMetadataProvider modelMetadataProvider, IUiMetadataSource uiMetadataSource)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(modelMetadataProvider != null);
			Contract.Requires<ArgumentNullException>(uiMetadataSource != null);

			_containerMetadata = containerMetadata;
			_modelMetadataProvider = modelMetadataProvider;
			_uiMetadataSource = uiMetadataSource;
		}

		[Route("{entitySetName}"), HttpGet]
		public EntitySet GetEntitySetSchema(string entitySetName)
		{
			var entitySet = _containerMetadata.GetEntitySet(entitySetName);
			if (entitySet == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			//IEnumerable<ModelMetadata> propertyMetadatas = _modelMetadataProvider.GetMetadataForProperties(null, entitySet.ElementTypeMetadata.ClrType);
			//return propertyMetadatas.Select(propMetadata => new Property(propMetadata));

			return new EntitySet(entitySet, _uiMetadataSource);
		}

	}
}
