// // -----------------------------------------------------------------------
// <copyright file="ScrumODataTests.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Scrum.WebApi.IntegrationTests
{

	/// <summary>
	/// Exercises the OData API from Scrum.WebApi
	/// </summary>
	public sealed class ScrumODataTests : BaseScrumODataTest
	{

		[xunitExt.Fact]
		public void GetExpandQueryWorks()
		{
			using (var server = CreateTestScrumServer())
			{
				var requestBuilder = server.CreateRequest("/odata/WorkItems()?$expand=Areas/Owners,TimeLog,Subscribers,AssignedTo,Status,Priority");
				var response = requestBuilder.GetAsync().Result;
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				Assert.Equal(JsonContentType, response.Content.Headers.ContentType.MediaType);
				Assert.True(response.Content.Headers.ContentLength > 1200);
			}				
		}


		/// <summary>
		/// Ensure that non-batch change requests succeed through OWIN
		/// </summary>
		[xunitExt.Fact]
		public void NonBatchChangeRequestsAgainstTestServer()
		{
			using (var testServer = CreateTestScrumServer())
			{
				TestNonBatchChangeRequests(testServer.HttpClient);
			}
		}

		/// <summary>
		/// Ensure that non-batch change requests succeed through IIS
		/// </summary>
		[xunitExt.Fact(Skip = "Disabled b/c IIS server setup not implemented; this test can be manually run.")]
		public void NonBatchChangeRequestsAgainstIis()
		{
			using (var httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:27704") })
			{
				TestNonBatchChangeRequests(httpClient);
			}
		}

		private void TestNonBatchChangeRequests(HttpClient httpClient)
		{
			// Add a ProjectArea
			var postBody = new StringContent("{\"Description\": \"Integration Test project area\",\"Name\":\"Int Test Project Area\",\"ProjectId\":1}");
			postBody.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			postBody.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("odata", "minimalmetadata"));
			var response = httpClient.PostAsync("/odata/ProjectAreas", postBody).Result;
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.Equal(JsonContentType, response.Content.Headers.ContentType.MediaType);
			var projectAreaLocation = response.Headers.Location;

			// Ensure that the location contains the ProjectArea
			response = httpClient.GetAsync(projectAreaLocation.AbsolutePath).Result;
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(JsonContentType, response.Content.Headers.ContentType.MediaType);
			Assert.Contains("\"Int Test Project Area\"", response.Content.ReadAsStringAsync().Result);

			// Delete the ProjectArea
			response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, projectAreaLocation.AbsolutePath)).Result;
			Assert.Equal(response.StatusCode, response.StatusCode);
		}
	}

}