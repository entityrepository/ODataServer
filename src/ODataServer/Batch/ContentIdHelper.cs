// -----------------------------------------------------------------------
// <copyright file="ContentIdHelper.cs" company="EntityRepository Contributors" years="2012-2013">
// This software is part of the EntityRepository library.
// Copyright © 2012-2013 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace EntityRepository.ODataServer.Batch
{
	/// <summary>
	/// Helper functions for handling <c>Content-ID</c> references and functionality.
	/// </summary>
	internal static class ContentIdHelper
	{
		/// <summary>
		/// Parses <paramref name="s"/> as a single content-ID reference.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="contentId"></param>
		/// <returns></returns>
		internal static bool TryParseContentIdReference(string s, out int contentId)
		{
			s = s.Trim();
			if ((s.Length >= 2)
				&& (s[0] == '$')
				&& int.TryParse(s.Substring(1), out contentId)
				&& string.Equals(s, "$" + contentId))
			{
				return true;
			}

			contentId = 0;
			return false;
		}

		/// <summary>
		/// Extracts inline content-ID references.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="contentIds"></param>
		/// <returns></returns>
		internal static bool TryExtractContentIdReferences(string s, out List<int> contentIds)
		{
			int ichPrefix = s.IndexOf('$');
			if (ichPrefix < 0)
			{
				contentIds = null;
				return false;
			}

			List<int> results = new List<int>();
			while (ichPrefix >= 0)
			{
				int ichDigit = ichPrefix + 1;
				int contentId = 0;
				while ((ichDigit < s.Length)
					&& char.IsDigit(s[ichDigit]))
				{
					int digitValue = s[ichDigit] - '0';
					contentId = 10 * contentId + digitValue;
					ichDigit++;
				}

				if (contentId > 0)
				{
					results.Add(contentId);
				}
				ichPrefix = s.IndexOf('$', ichDigit);
			}

			if (results.Count > 0)
			{
				contentIds = results;
				return true;
			}
			else
			{
				contentIds = null;
				return false;
			}
		}

		internal static bool ResolveContentIdReferenceInRequestUrl(HttpRequestMessage request)
		{
			ContentIdRecord referencedContentIdRecord = GetReferencedContentIdEntity(request);
			if (referencedContentIdRecord == null)
			{
				return false;
			}

			// Save the initial Uri (eg http://localhost/odata/$46/Messages ) before the Content-ID references are replaced.
			request.SaveInitialChangeSetRequestUri();

			string originalUri = request.RequestUri.OriginalString;
			string refKey = "$" + referencedContentIdRecord.ContentId;
			int ichRef = originalUri.IndexOf(refKey);
			Contract.Assert(ichRef >= 0);

			// As location headers MUST be absolute URL's, we can ignore everything 
			// before the $content-id while resolving it.
			string updatedUri = referencedContentIdRecord.Location + originalUri.Substring(ichRef + refKey.Length);
			request.RequestUri = new Uri(updatedUri);
			return true;
		}

		internal static ContentIdRecord GetReferencedContentIdEntity(HttpRequestMessage requestMessage)
		{
			Contract.Assert(requestMessage != null);

			ChangeSetContext changeSetContext = requestMessage.GetChangeSetContext();
			string initialRequestUri = requestMessage.GetInitialChangeSetRequestUri();
			List<int> contentIds;
			if ((changeSetContext == null)
				|| (initialRequestUri == null)
				|| !TryExtractContentIdReferences(initialRequestUri, out contentIds))
			{
				return null;
			}

			Contract.Assert(contentIds.Count == 1);

			int requestContentId = contentIds[0];
			return changeSetContext.GetContentIdRecord(requestContentId);
		}

		internal static bool RequestHasContentIdReference(HttpRequestMessage requestMessage)
		{
			return GetReferencedContentIdEntity(requestMessage) != null;
		}		 

	}
}