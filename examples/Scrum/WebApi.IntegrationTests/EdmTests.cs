// // -----------------------------------------------------------------------
// <copyright file="EdmTests.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using EntityRepository.ODataServer.EF;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Scrum.Dal;
using Xunit;
using Xunit.Extensions;
using EdmxWriter = System.Data.Entity.Infrastructure.EdmxWriter;

namespace Scrum.WebApi.IntegrationTests
{

	/// <summary>
	/// Testing EDM (entity data model) handling.
	/// </summary>
	public sealed class EdmTests
	{

		[Fact]
		public void DumpScrumDbEdmx()
		{
			using (var db = new ScrumDb())
			using (XmlWriter writer = XmlWriter.Create(Console.Out))
			{
				EdmxWriter.WriteEdmx(db, writer);
			}
		}

		[Theory]
		[InlineData(EdmxTarget.OData)]
		[InlineData(EdmxTarget.EntityFramework)]
		public void DumpRoundTrippedEdmx(EdmxTarget edmxTarget)
		{
			IEnumerable<EdmError> writeErrors;

			var sb = new StringBuilder(2048);
			var xmlSettings = new XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true };
			using (var db = new ScrumDb())
			using (XmlWriter writer = XmlWriter.Create(sb, xmlSettings))
			{
				var edmModel = db.GetEdmModel();
				Microsoft.Data.Edm.Csdl.EdmxWriter.TryWriteEdmx(edmModel, writer, edmxTarget, out writeErrors);
			}
			Console.WriteLine(sb.ToString());

			if (writeErrors.Any())
			{
				Console.WriteLine();
				Console.WriteLine("EDMX write errors:");
				foreach (var error in writeErrors)
				{
					Console.WriteLine(error.ToString());
				}
			}
		}



	}

}