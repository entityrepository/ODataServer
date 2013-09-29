// -----------------------------------------------------------------------
// <copyright file="CreatedItemResult.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Results;
using System.Web.Http.OData.Routing;
using EntityRepository.ODataServer.Batch;
using EntityRepository.ODataServer.Model;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace EntityRepository.ODataServer.Results
{
	/// <summary>
	/// An action result for an entity that is created within a changeset, or created outside of a changeset.
	/// </summary>
	/// <typeparam name="T">The entity type.</typeparam>
	/// <remarks>This action result handles content negotiation and the HTTP prefer header and generates a location header
	/// that is the same as the edit link of the created entity.</remarks>
	public class CreatedItemResult<T> : ChangeSetItemResult<T>
	{
		private Uri _locationHeader;
		private ODataController _controller;

		/// <summary>
		/// Initializes a new instance of the <see cref="CreatedItemResult{T}"/> class.
		/// </summary>
		/// <param name="entity">The created entity.</param>
		/// <param name="controller">The controller from which to obtain the dependencies needed for execution.</param>
		public CreatedItemResult(T entity, ODataController controller)
			: base(entity, HttpStatusCode.Created, controller)
		{
			Contract.Requires<ArgumentNullException>(controller != null);

			_controller = controller;
		}
		 
		/// <summary>
		/// Gets the location header of the created entity.
		/// </summary>
		public Uri LocationHeader
		{
			get
			{
				_locationHeader = _locationHeader ?? GenerateLocationHeader();
				return _locationHeader;
			}
		}

		protected override void SetPendingResponseHeaders(HttpResponseMessage response)
		{
			base.SetPendingResponseHeaders(response);
			response.Headers.Location = LocationHeader;
		}

		protected override void SetFinalResponseHeaders(HttpResponseMessage response)
		{
			base.SetFinalResponseHeaders(response);
			// Always regenerate the LocationHeader
			_locationHeader = null;
			response.Headers.Location = LocationHeader;
		}

		internal Uri GenerateLocationHeader()
		{
			Contract.Assert(_controller.ContainerMetadata != null);

			Type clrType = Entity.GetType();
			IEntitySetMetadata entitySetMetadata = _controller.ContainerMetadata.GetEntitySetFor(clrType);
			IEntityTypeMetadata entityTypeMetadata = _controller.ContainerMetadata.GetEntityType(clrType);
			if ((entitySetMetadata == null)
			    || (entityTypeMetadata == null))
			{
				throw new InvalidOperationException("IEntitySetMetadata and/or IEntityTypeMetadata not found for entity type " + clrType.FullName);
			}

			object keyValue = entityTypeMetadata.SingleClrKeyProperty.GetValue(Entity);
			string keyString = ODataUriUtils.ConvertToUriLiteral(keyValue, ODataVersion.V3, Request.GetEdmModel());

			string oDataLink = _controller.Url.ODataLink(new EntitySetPathSegment(entitySetMetadata.Name), new KeyValuePathSegment(keyString));
			return new Uri(oDataLink);
		}

	}
}