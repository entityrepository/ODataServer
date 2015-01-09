// // -----------------------------------------------------------------------
// <copyright file="BaseWebTest.cs" company="Adap.tv">
// Copyright (c) 2014 Adap.tv.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Scrum.WebApi.IntegrationTests
{

	/// <summary>
	/// Provides standard functionality for tests that exercise Scrum.WebApi.
	/// </summary>
	public abstract class BaseScrumODataTest
	{
		/// <summary>The mime/type string for JSON.</summary>
		protected const string JsonContentType = "application/json";
		/// <summary>The mime/type string for XML.</summary>
		protected const string XmlContentType = "text/xml";

		/// <summary>
		/// Returns a new <see cref="TestServer"/>, configured with Scrum.WebApi startup code.
		/// Requests against the test server are processed directly in memory without going over the network.
		/// </summary>
		/// <param name="appName"></param>
		/// <returns></returns>
		/// <remarks>
		/// Note that the returned <see cref="TestServer"/> should be disposed.
		/// </para>
		/// </remarks>
		protected TestServer CreateTestScrumServer(string appName = null)
		{
			if (appName == null)
			{
				appName = GetType().FullName;
			}

			return TestServer.Create(app =>
			{
				app.Properties["host.AppName"] = appName;
				app.Properties["Adap.InUnitTest"] = true;

				// Add any "beginning of request pipeline" test-specific configuration here.

				// Use the normal webapp config
				new OwinStartup().Configuration(app);
			});
		}

		/// <summary>
		/// Send a GET request for <paramref name="url"/> to <paramref name="server"/>, return the response body as a <see cref="string"/>.
		/// </summary>
		/// <param name="server">The <see cref="TestServer"/>.</param>
		/// <param name="url">The URL to request - can be a host-scoped URL (starting with a "/"), or an absolute URL.</param>
		/// <param name="requiredMediaType">The required media type - eg "text/xml" - or <c>null</c> if no media type is required.</param>
		/// <returns>The response body as a string.</returns>
		/// <remarks>
		/// This is a unit test method - it <c>Assert</c>s that the response is successful, the mime/type is correct,
		/// and the response body is not <c>null</c> or whitespace.
		/// </remarks>
		protected string RequestGetString(TestServer server, string url, string requiredMediaType = null)
		{
			using (var client = server.HttpClient)
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
				if (requiredMediaType != null)
				{
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(requiredMediaType));
				}

				HttpResponseMessage response = client.SendAsync(request).Result;
				Assert.True(response.IsSuccessStatusCode);

				if (requiredMediaType != null)
				{
					Assert.Equal(requiredMediaType, response.Content.Headers.ContentType.MediaType);
				}

				string body = response.Content.ReadAsStringAsync().Result;
				Assert.False(string.IsNullOrWhiteSpace(body));
				return body;
			}
		}

		protected JContainer RequestGetJson(TestServer server, string url)
		{
			string responseBody = RequestGetString(server, url, JsonContentType);
			if (responseBody.StartsWith("["))
			{
				return JArray.Parse(responseBody);
			}
			else
			{
				return JObject.Parse(responseBody);			
			}
		}

	}

}