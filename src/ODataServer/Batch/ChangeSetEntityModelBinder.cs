// -----------------------------------------------------------------------
// <copyright file="ChangeSetEntityModelBinder.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace EntityRepository.ODataServer.Batch
{
	/// <summary>
	/// Model binder to provide access to entities added earlier in a changeset with a "Content-ID" header.
	/// </summary>
	public class ChangeSetEntityModelBinder : IModelBinder
	{
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			ContentIdRecord contentIdRecord = ContentIdHelper.GetReferencedContentIdEntity(actionContext.Request);
			if (contentIdRecord == null)
			{
				// Value doesn't exist
				return false;
			}

			if (bindingContext.ModelType.IsAssignableFrom(contentIdRecord.EntityType))
			{
				bindingContext.Model = contentIdRecord.Entity;
				return true;
			}
			else
			{
				string errorMessage = string.Format("Content-ID reference ${0} contains an entity of type {1}, which could not be converted to type {2}.",
				                                    contentIdRecord.ContentId,
				                                    contentIdRecord.EntityType,
				                                    bindingContext.ModelType);
				bindingContext.ModelState.AddModelError(bindingContext.ModelName, errorMessage);
				return false;
			}
		}

		internal static bool IsChangeSetEntityParameter(HttpParameterDescriptor parameterDescriptor)
		{
			Contract.Assert(parameterDescriptor != null);

			ModelBinderAttribute modelBinderAttr = parameterDescriptor.ParameterBinderAttribute as ModelBinderAttribute;
			return (modelBinderAttr != null)
				&& (modelBinderAttr.BinderType == typeof(ChangeSetEntityModelBinder));
		}

		internal static bool ActionHasChangeSetEntityParameter(HttpActionDescriptor actionDescriptor)
		{
			Contract.Assert(actionDescriptor != null);

			var parameters = actionDescriptor.GetParameters();
			return (parameters.Count > 0) && IsChangeSetEntityParameter(parameters[0]);
		}
	}
}