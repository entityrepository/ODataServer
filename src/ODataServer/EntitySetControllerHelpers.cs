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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using EntityRepository.ODataServer.Model;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace EntityRepository.ODataServer
{
	/// <summary>
	/// Helper class for <see cref="System.Web.Http.OData.EntitySetController{TEntity,TKey}"/> and <see cref="AsyncEntitySetController{TEntity,TKey}"/> that contains shared logic.
	/// </summary>
	public static class EntitySetControllerHelpers
	{

		internal const string PreferHeaderName = "Prefer";
		internal const string PreferenceAppliedHeaderName = "Preference-Applied";
		internal const string ReturnContentHeaderValue = "return-content";
		internal const string ReturnNoContentHeaderValue = "return-no-content";
		internal const string UsEnglish = "en-us";

		/// <summary>
		/// Holds a reference to the generic type definition for <c>HttpRequestMessageExtensions.CreateResponse&lt;T&gt;()</c>.
		/// </summary>
		private static readonly MethodInfo s_createResponseMethodDef;

		/// <summary>
		/// Holds a reference to the generic type definition for SingleResult.Create<T>(IQueryable<T> queryable)
		/// </summary>
		private static readonly MethodInfo s_createSingleResultMethodDef;

		static EntitySetControllerHelpers()
		{
			Expression<Func<HttpRequestMessage, HttpResponseMessage>> expr = (request) => request.CreateResponse(HttpStatusCode.OK, default(object));
			s_createResponseMethodDef = ((MethodCallExpression) expr.Body).Method.GetGenericMethodDefinition();

			s_createSingleResultMethodDef = typeof(SingleResult).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
		}

		public static HttpResponseException NotImplementedResponseException(ApiController controller, string requestType)
		{
			return new HttpResponseException(controller.Request.CreateResponse(HttpStatusCode.NotImplemented,
			                                                                  new ODataError
			                                                                  {
				                                                                  Message = string.Format("{0} does not support {1} requests.", controller.GetType().FullName, requestType),
				                                                                  MessageLanguage = UsEnglish,
				                                                                  ErrorCode = requestType + " requests not supported"
			                                                                  }));
		}

		public static HttpResponseMessage CreateSingleEntityResponseFromRuntimeType(this HttpRequestMessage request, IEnumerable enumerable)
		{
			//IQueryable queryable = enumerable as IQueryable;
			//if (queryable != null)
			//{	// Use SingleResult<T>
			//	SingleResult singleResult = s_createSingleResultMethodDef.MakeGenericMethod(queryable.ElementTypeMetadata).Invoke(null, new object[] { queryable }) as SingleResult;
			//	return request.CreateResponseFromRuntimeType(HttpStatusCode.OK, singleResult);
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
				return request.CreateResponseFromRuntimeType(HttpStatusCode.OK, entity);
			//}
		}

		public static HttpResponseMessage CreateSingleEntityResponse<TEntity>(this HttpRequestMessage request, TEntity entity)
			where TEntity : class
		{
			if (entity == null)
			{
				return request.CreateResponse(HttpStatusCode.NotFound);
			}
			else
			{
				return request.CreateResponse(HttpStatusCode.OK, entity);
			}
		}

		public static HttpResponseMessage CreateSingleEntityProjectedResponse<TEntity>(this HttpRequestMessage request, IEnumerable<TEntity> enumerable, Func<TEntity, object> projectionFunction)
			where TEntity : class
		{
			IEnumerator<TEntity> enumerator = enumerable.GetEnumerator();
			if (! enumerator.MoveNext())
			{
				return request.CreateResponse(HttpStatusCode.NotFound);
			}
			TEntity entity = enumerator.Current;
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
			return request.CreateResponseFromRuntimeType(HttpStatusCode.OK, projectionFunction(entity));
		}
		
		public static HttpResponseMessage CreateResponseFromRuntimeType(this HttpRequestMessage request, HttpStatusCode statusCode, object entity)
		{
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Ensures(Contract.Result<HttpResponseMessage>() != null);

			if (entity == null)
			{
				return request.CreateResponse(HttpStatusCode.NotFound);
			}

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

		//[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		//public static HttpResponseMessage PostResponse<TEntity, TKey>(ApiController controller, TEntity createdEntity, TKey entityKey)
		//{
		//	HttpResponseMessage response = null;
		//	HttpRequestMessage request = controller.Request;
		//	if (RequestPrefersReturnNoContent(request))
		//	{
		//		response = request.CreateResponse(HttpStatusCode.NoContent);
		//		response.Headers.Add(PreferenceAppliedHeaderName, ReturnNoContentHeaderValue);
		//	}
		//	else
		//	{
		//		response = request.CreateResponse(HttpStatusCode.Created, createdEntity);
		//	}

		//	ODataPath odataPath = request.GetODataPath();
		//	if (odataPath == null)
		//	{
		//		throw new InvalidOperationException("Location header missing OData path");
		//	}

		//	EntitySetPathSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetPathSegment;
		//	if (entitySetSegment == null)
		//	{
		//		throw new InvalidOperationException("Location header does not start with EntitySet");
		//	}

		//	response.Headers.Location = new Uri(controller.Url.ODataLink(
		//																 entitySetSegment,
		//																 new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(entityKey, ODataVersion.V3))));
		//	return response;
		//}

		//[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		//public static HttpResponseMessage PutResponse<TEntity>(HttpRequestMessage request, TEntity updatedEntity)
		//{
		//	if (RequestPrefersReturnContent(request))
		//	{
		//		HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, updatedEntity);
		//		response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
		//		return response;
		//	}
		//	else
		//	{
		//		return request.CreateResponse(HttpStatusCode.NoContent);
		//	}
		//}

		//[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
		//public static HttpResponseMessage PatchResponse<TEntity>(HttpRequestMessage request, TEntity patchedEntity)
		//{
		//	if (RequestPrefersReturnContent(request))
		//	{
		//		HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, patchedEntity);
		//		response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
		//		return response;
		//	}
		//	else
		//	{
		//		return request.CreateResponse(HttpStatusCode.NoContent);
		//	}
		//}

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

		public static object GetKeyFor<TEntity>(this IContainerMetadata containerMetadata, TEntity entity)
		{
			Contract.Requires<ArgumentNullException>(containerMetadata != null);
			Contract.Requires<ArgumentNullException>(entity != null);

			IEntityTypeMetadata entityTypeMetadata = containerMetadata.GetEntityType(typeof(TEntity));
			return entityTypeMetadata.EntityKeyFunction(entity);
		}

		public static bool AreEmpty(this ODataQueryOptions queryOptions)
		{
			Contract.Requires<ArgumentNullException>(queryOptions != null);

			ODataRawQueryOptions rawValues = queryOptions.RawValues;
			return (rawValues.Filter == null)
			       && (rawValues.Expand == null)
			       && (rawValues.OrderBy == null)
			       && (rawValues.Format == null)
			       && (rawValues.InlineCount == null)
			       && (rawValues.Select == null)
			       && (rawValues.Skip == null)
				   && (rawValues.SkipToken == null)
				   && (rawValues.Top == null);
		}

		public static bool ParseSingleEntityLink(this ODataController oDataController, Uri link, out IEdmEntitySet entitySet, out object key)
		{
			Contract.Requires<ArgumentNullException>(oDataController != null);
			Contract.Requires<ArgumentNullException>(link != null);
			HttpRequestMessage request = oDataController.Request;

			// Get the route that was used for this request.
			IHttpRoute route = request.GetRouteData().Route;

			// Create an equivalent self-hosted route. 
			IHttpRoute newRoute = new HttpRoute(route.RouteTemplate,
				new HttpRouteValueDictionary(route.Defaults),
				new HttpRouteValueDictionary(route.Constraints),
				new HttpRouteValueDictionary(route.DataTokens), route.Handler);

			// Create a fake GET request for the link URI.
			var tmpRequest = new HttpRequestMessage(HttpMethod.Get, link);

			// Send this request through the routing process.
			var routeData = newRoute.GetRouteData(request.GetRequestContext().VirtualPathRoot, tmpRequest);

			// If the GET request matches the route, use the path segments to find the key.
			if (routeData != null)
			{
				ODataPath path = tmpRequest.ODataProperties().Path;
				if (path.PathTemplate == "~/entityset/key")
				{
					entitySet = path.EntitySet;
					var keySegment = path.Segments.OfType<KeyValuePathSegment>().FirstOrDefault();
					if (keySegment != null)
					{
						// Convert the segment into the key type.
						key = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, ODataVersion.V3, request.ODataProperties().Model, entitySet.GetSingleKeyType());
						return true;
					}
				}
			}

			entitySet = null;
			key = null;
			return false;
		}

		public static IEdmTypeReference GetSingleKeyType(this IEdmEntitySet entitySet)
		{
			Contract.Requires<ArgumentNullException>(entitySet != null);

			IEnumerable<IEdmStructuralProperty> keys = entitySet.ElementType.Key();
			if (keys.Count() != 1)
			{
				throw new InvalidOperationException(string.Format("EntitySet {0} has {1} key properties declared; a single key property is required.", entitySet.Name, keys.Count()));
			}

			return keys.Single().Type;
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
		public static bool RequestPrefersReturnContent(HttpRequestMessage request)
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
		public static bool RequestPrefersReturnNoContent(HttpRequestMessage request)
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
