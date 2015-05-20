// // -----------------------------------------------------------------------
// <copyright file="EntityDataModelTests.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.UnitTests.EStore.DataAccess;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Xunit;
using Xunit.Abstractions;

namespace EntityRepository.ODataServer.UnitTests
{

	/// <summary>
	/// Exercises our handling of EDM metadata.
	/// </summary>
	public sealed class EntityDataModelTests
	{

		private readonly ITestOutputHelper _testOutput;

		public EntityDataModelTests(ITestOutputHelper testOutputHelper)
		{
			_testOutput = testOutputHelper;
		}

		[Fact]
		public void DumpDbContextEntityDataModel()
		{
			StringWriter sw = new StringWriter();
			using (EStoreDb db = new EStoreDb())
			using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true }))
			{
				IEdmModel edm = db.GetEdmModel();
				IEnumerable<EdmError> edmErrors;
				bool success = edm.TryWriteCsdl(xmlWriter, out edmErrors);
				Assert.True(success);
			}

			_testOutput.WriteLine(sw.ToString());
		}

	}

}