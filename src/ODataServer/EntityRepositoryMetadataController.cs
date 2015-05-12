// // -----------------------------------------------------------------------
// <copyright file="ODataMetadataController.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.OData.Extensions;
using System.Web.Http.Tracing;
using System.Xml;
using EntityRepository.ODataServer.Edm;
using EntityRepository.ODataServer.Model;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace EntityRepository.ODataServer
{

	/// <summary>
	/// Replaces <see cref="System.Web.Http.OData.ODataMetadataController"/> 
	/// </summary>
	[System.Web.Http.OData.ODataFormatting]
	[System.Web.Http.OData.ODataRouting]
	[ApiExplorerSettings(IgnoreApi = true)]
	public sealed class EntityRepositoryMetadataController : ApiController
	{
		private static readonly Version s_defaultEdmxVersion = new Version(1, 0);

		private readonly IContainerMetadata _containerMetadata;

		public EntityRepositoryMetadataController(IContainerMetadata containerMetadata)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);

			_containerMetadata = containerMetadata;
		}

		/// <summary>
		/// Generates the OData $metadata document.
		/// </summary>
		/// <param name="modelMetadata"><c>true</c> to expose the model metadata; <c>false</c> to use the web API odata model (the default for Web API odata).</param>
		/// <param name="includeClrInfo">Currently has not effect</param>
		/// <returns>The <see cref="IEdmModel"/> representing $metadata.</returns>
		public HttpResponseMessage GetMetadata(bool modelMetadata = true, bool includeClrInfo = false)
		{
			var request = Request;
			var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
			response.Headers.Add("DataServiceVersion", "3.0");

			// Write the Metadata EDMX to a 
			var edmModel = GetModel(modelMetadata);
			IEnumerable<EdmError> writeErrors;

			if (includeClrInfo == false)
			{
				edmModel.RemoveClrTypeAnnotations();
			}

			//response.Content = new PushStreamContent((stream, httpContent, transportContext) =>
			//										 {
			//											 using (XmlWriter xmlWriter = XmlWriter.Create(stream))
			//											 {
			//												 EdmxWriter.TryWriteEdmx(edmModel, xmlWriter, EdmxTarget.OData, out writeErrors);
			//											 }

			//											 if (writeErrors.Any())
			//											 {
			//												 var traceWriter = Configuration.Services.GetTraceWriter();
			//												 traceWriter.Error(request,
			//																	typeof(EntityRepositoryMetadataController).FullName,
			//																	"EDMX write errors:\r\n" + string.Join("\r\n", writeErrors));
			//											 }
			//										 }, "application/xml");

			var memStream = new MemoryStream(4096);
			Encoding xmlEncoding;
			using (XmlWriter xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings() { Indent = true }))
			{
				EdmxWriter.TryWriteEdmx(edmModel, xmlWriter, EdmxTarget.OData, out writeErrors);
				xmlEncoding = xmlWriter.Settings.Encoding;
			}

			if (writeErrors.Any())
			{
				var traceWriter = Configuration.Services.GetTraceWriter();
				traceWriter.Error(request,
								   typeof(EntityRepositoryMetadataController).FullName,
								   "EDMX write errors:\r\n" + string.Join("\r\n", writeErrors));
			}

			memStream.Seek(0, SeekOrigin.Begin);
			response.Content = new StreamContent(memStream);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
			response.Content.Headers.ContentType.CharSet = xmlEncoding.WebName;
			return response;
		}

		/// <summary>
		/// Generates the OData service document.
		/// </summary>
		/// <returns>The service document for the service.</returns>
		public ODataWorkspace GetServiceDocument()
		{
			IEdmModel model = GetModel();
			ODataWorkspace workspace = new ODataWorkspace();
			IEdmEntityContainer container = model.EntityContainers().Single();
			IEnumerable<IEdmEntitySet> entitysets = container.EntitySets();

			IEnumerable<ODataResourceCollectionInfo> collections = entitysets.Select(
				e => GetODataResourceCollectionInfo(model.GetEntitySetUrl(e), e.Name));
			workspace.Collections = collections;

			return workspace;
		}

		private static ODataResourceCollectionInfo GetODataResourceCollectionInfo(string url, string name)
		{
			ODataResourceCollectionInfo info = new ODataResourceCollectionInfo
			{
				Name = name, // Required for JSON light support
				Url = new Uri(url, UriKind.Relative)
			};

			info.SetAnnotation<AtomResourceCollectionMetadata>(new AtomResourceCollectionMetadata { Title = name });

			return info;
		}

		private IEdmModel GetModel(bool useModelMetadata = true)
		{
			IEdmModel model;
			if (useModelMetadata)
			{
				model = _containerMetadata.EdmModel;
			}
			else
			{
				// Default web API odata impl:
				model = Request.ODataProperties().Model;
				if (model == null)
				{
					throw new InvalidOperationException("Request must have an OData model");
				}
			}
			model.SetEdmxVersion(s_defaultEdmxVersion);

			return model;
		}

	}

}