// // -----------------------------------------------------------------------
// <copyright file="ODataFeatureTests.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Data.Entity;
using EntityRepository.ODataServer.UnitTests.EStore.DataAccess;
using EntityRepository.ODataServer.UnitTests.EStore.Model;
using Simple.OData.Client;
using Xunit;
using Xunit.Abstractions;

namespace EntityRepository.ODataServer.UnitTests
{

	/// <summary>
	/// Exercises odata features.
	/// </summary>
	public sealed class ODataFeatureTests : BaseODataWebTest
	{

		public ODataFeatureTests(ITestOutputHelper testOutputHelper)
			: base(testOutputHelper)
		{}

		[Fact]
		public void InheritanceCastSupported()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<EStoreDb>());

			using (var server = CreateTestServer(new EStore.IocConfig()))
			{
				var client = CreateODataClient(server);

				// Add
				var addTask = client.For<Product>().As<MusicTrackProduct>().Set(new MusicTrackProduct()
				                                                  {
					                                                  Album = "Rattle and Hum",
					                                                  Artist = "U2",
					                                                  Name = "Desire"
																  }).InsertEntryAsync(CancelTest);
				addTask.Wait(CancelTest);
				Assert.True(addTask.IsCompleted);
				var musicTrackProduct = addTask.Result;
				Assert.Equal("U2", musicTrackProduct.Artist);
				Assert.True(0 < musicTrackProduct.Id);

				// Add
				var product2 = client.For<Product>().Set(new Product()
				{
					Name = "Of Mouse and Men"
				}).InsertEntryAsync(CancelTest).Result;
				Assert.True(0 < product2.Id);

				// Query
				var musicTrackProducts = client.For<Product>().As<MusicTrackProduct>().FindEntriesAsync(CancelTest).Result;
				Assert.Single(musicTrackProducts);
			}
		}
		

	}

}