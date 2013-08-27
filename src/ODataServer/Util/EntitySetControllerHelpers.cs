// -----------------------------------------------------------------------
// <copyright file="EntitySetControllerHelpers.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace EntityRepository.ODataServer.Util
{
	/// <summary>
	/// Helper class for <see cref="System.Web.Http.OData.EntitySetController{TEntity,TKey}"/> and <see cref="AsyncEntitySetController{TEntity,TKey}"/> that contains shared logic.
	/// </summary>
	internal static class EntitySetControllerHelpers
	{

		internal const string PreferHeaderName = "Prefer";
		internal const string PreferenceAppliedHeaderName = "Preference-Applied";
		internal const string ReturnContentHeaderValue = "return-content";
		internal const string ReturnNoContentHeaderValue = "return-no-content";
		internal const string UsEnglish = "en-us";

		/// <summary>
		/// Holds a reference to the generic type definition for HttpRequestMessageExtensions.CreateResponse<T>()
		/// </summary>
		private static readonly MethodInfo s_createResponseMethodDef;

		/// <summary>
		/// Holds a reference to the generic type definition for SingleResult.Create<T>(IQueryable<T> queryable)
		/// </summary>
		private static readonly MethodInfo s_createSingleResultMethodDef;

		static EntitySetControllerHelpers()
		{
			Expression<Func<HttpRequestMessage, HttpResponseMessage>> expr = (request) => request.CreateResponse(HttpStatusCode.OK, default(object));
			s_createResponseMethodDef = (expr.Body as MethodCallExpression).Method.GetGenericMethodDefinition();

			//s_createSingleResultMethodDef = typeof(SingleResult).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
		}

		public static ODataPath GetODataPath(ApiController controller)
		{
			return controller.Request.GetODataPath();
		}

		public static ODataQueryOptions<TEntity> CreateQueryOptions<TEntity>(ApiController controller)
		{
			ODataQueryContext context = new ODataQueryContext(controller.Request.GetEdmModel(), typeof(TEntity));
			return new ODataQueryOptions<TEntity>(context, controller.Request);
		}

		public static HttpResponseException NotImplementedResponseException(ApiController controller, string requestType)
		{
			throw new HttpResponseException(controller.Request.CreateResponse(HttpStatusCode.NotImplemented,
			                                                                  new ODataError
			                                                                  {
				                                                                  Message = string.Format("{0} does not support {1} requests.", controller.GetType().FullName, requestType),
				                                                                  MessageLanguage = UsEnglish,
				                                                                  ErrorCode = requestType + " requests not supported"
			                                                                  }));
		}

		public static HttpResponseMessage CreateSingleEntityResponse(this HttpRequestMessage request, IEnumerable enumerable)
		{
			IQueryable queryable = enumerable as IQueryable;
			// SingleResult is only available in the prerelease web API odata
			//if (queryable != null)
			//{	// Use SingleResult<T>
			//	SingleResult singleResult = s_createSingleResultMethodDef.MakeGenericMethod(queryable.ElementType).Invoke(null, new object[] { queryable }) as SingleResult;
			//	return request.CreateResponse(HttpStatusCode.OK, singleResult);
			//}
			//else
			//{
			IEnumerator enumerator = enumerable.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return request.CreateResponse(HttpStatusCode.NotFound);
			}
			object entity = enumerator.Current;
			if (enumerator.MoveNext())
			{
				return request.CreateResponse(HttpStatusCode.InternalServerError,
				                              new ODataError
				                              {
					                              Message = string.Format("More than 1 entity returned for {0}.", request.RequestUri),
					                              MessageLanguage = UsEnglish,
					                              ErrorCode = "Multiple entities returned for single entity method"
				                              });
			}
			if (entity == null)
			{
				return request.CreateResponse(HttpStatusCode.NotFound);
			}
			return request.CreateResponseFromRuntimeType(HttpStatusCode.OK, entity);
			//}
		}

		public static HttpResponseMessage CreateResponseFromRuntimeType(this HttpRequestMessage request, HttpStatusCode statusCode, object entity)
		{
			Type entityType = entity.GetType();
			return s_createResponseMethodDef.MakeGenericMethod(entityType).Invoke(null, new[] { request, statusCode, entity }) as HttpResponseMessage;
		}

		//[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		//public static HttpResponseMessage GetByKeyResponse<TEntity>(HttpRequestMessage request, TEntity entity)
		//{
		//	if (entity == null)
		//	{
		//		return request.CreateResponse(HttpStatusCode.NotFound);
		//	}
		//	else
		//	{
		//		return request.CreateResponse(HttpStatusCode.OK, entity);
		//	}
		//}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		public static HttpResponseMessage PostResponse<TEntity, TKey>(ApiController controller, TEntity createdEntity, TKey entityKey)
		{
			HttpResponseMessage response = null;
			HttpRequestMessage request = controller.Request;
			if (RequestPrefersReturnNoContent(request))
			{
				response = request.CreateResponse(HttpStatusCode.NoContent);
				response.Headers.Add(PreferenceAppliedHeaderName, ReturnNoContentHeaderValue);
			}
			else
			{
				response = request.CreateResponse(HttpStatusCode.Created, createdEntity);
			}

			ODataPath odataPath = request.GetODataPath();
			if (odataPath == null)
			{
				throw new InvalidOperationException("Location header missing OData path");
			}

			EntitySetPathSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetPathSegment;
			if (entitySetSegment == null)
			{
				throw new InvalidOperationException("Location header does not start with EntitySet");
			}

			response.Headers.Location = new Uri(controller.Url.ODataLink(
			                                                             entitySetSegment,
			                                                             new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(entityKey, ODataVersion.V3))));
			return response;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		public static HttpResponseMessage PutResponse<TEntity>(HttpRequestMessage request, TEntity updatedEntity)
		{
			if (RequestPrefersReturnContent(request))
			{
				HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, updatedEntity);
				response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
				return response;
			}
			else
			{
				return request.CreateResponse(HttpStatusCode.NoContent);
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		public static HttpResponseMessage PatchResponse<TEntity>(HttpRequestMessage request, TEntity patchedEntity)
		{
			if (RequestPrefersReturnContent(request))
			{
				HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, patchedEntity);
				response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
				return response;
			}
			else
			{
				return request.CreateResponse(HttpStatusCode.NoContent);
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		public static HttpResponseException UnmappedRequestResponse(ApiController controller, ODataPath odataPath)
		{
			return new HttpResponseException(
				controller.Request.CreateResponse(
				                                  HttpStatusCode.NotImplemented,
				                                  new ODataError
				                                  {
					                                  Message = string.Format("{0} does not support {1} requests.", controller.GetType().FullName, odataPath.PathTemplate),
					                                  MessageLanguage = UsEnglish,
					                                  ErrorCode = "Request not supported"
				                                  }));
		}

		//[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		//public static HttpResponseException GetKeyNotImplementedResponse(HttpRequestMessage request)
		//{
		//	return new HttpResponseException(
		//		request.CreateResponse(
		//			HttpStatusCode.NotImplemented,
		//			new ODataError
		//			{
		//				Message = SRResources.EntitySetControllerUnsupportedGetKey,
		//				MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
		//				ErrorCode = Microsoft.Data.OData.Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "POST")
		//			}));
		//}

		//[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		//public static HttpResponseException GetEntityByKeyNotImplementedResponse(HttpRequestMessage request)
		//{
		//	return new HttpResponseException(
		//		request.CreateResponse(
		//			HttpStatusCode.NotImplemented,
		//			new ODataError
		//			{
		//				Message = SRResources.EntitySetControllerUnsupportedGetByKey,
		//				MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
		//				ErrorCode = SRResources.EntitySetControllerUnsupportedGetByKeyErrorCode
		//			}));
		//}

		/// <summary>
		/// Returns whether or not the request prefers content to be returned.
		/// </summary>
		/// <returns><c>true</c> if the request has a Prefer header value for "return-content", <c>false</c> otherwise</returns>
		private static bool RequestPrefersReturnContent(HttpRequestMessage request)
		{
			IEnumerable<string> preferences = null;
			if (request.Headers.TryGetValues(PreferHeaderName, out preferences))
			{
				return preferences.Contains(ReturnContentHeaderValue);
			}
			return false;
		}

		/// <summary>
		/// Returns whether or not the request prefers no content to be returned.
		/// </summary>
		/// <returns><c>true</c> if the request has a Prefer header value for "return-no-content", <c>false</c> otherwise</returns>
		private static bool RequestPrefersReturnNoContent(HttpRequestMessage request)
		{
			IEnumerable<string> preferences = null;
			if (request.Headers.TryGetValues(PreferHeaderName, out preferences))
			{
				return preferences.Contains(ReturnNoContentHeaderValue);
			}
			return false;
		}

	}
}
