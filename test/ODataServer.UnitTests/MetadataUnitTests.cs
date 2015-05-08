// // -----------------------------------------------------------------------
// <copyright file="EntityDataModelTests.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012-2015 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EntityRepository.ODataServer.EF;
using EntityRepository.ODataServer.UnitTests.EStore;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.OData;
using Xunit;

namespace EntityRepository.ODataServer.UnitTests
{

	/// <summary>
	/// Exercises our handling of EDM metadata.
	/// </summary>
	public sealed class EntityDataModelTests
	{
		[Fact]
		public void DumpDbContextEntityDataModel()
		{
			StringWriter sw = new StringWriter();
			using (Db db = new Db())
			using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true }))
			{
				IEdmModel edm = db.GetEdmModel();
				IEnumerable<EdmError> edmErrors;
				bool success = edm.TryWriteCsdl(xmlWriter, out edmErrors);
				Assert.True(success);
			}

			Console.WriteLine(sw);
		}
	}

}