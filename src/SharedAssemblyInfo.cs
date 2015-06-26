// -----------------------------------------------------------------------
// <copyright file="SharedAssemblyInfo.cs" company="EntityRepository Contributors" year="2013-2014">
// This software is part of the EntityRepository library.
// Copyright © 2013-2014 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("EntityRepository Contributors")]
[assembly: AssemblyProduct("EntityRepository")]
[assembly: AssemblyCopyright("Copyright © 2013-2015 EntityRepository Contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

// Guideline: Informational/semver increments on every build.  Assembly version should only increment
// on breaking changes, aka major version changes.
[assembly: AssemblyVersion("0.9.0.0")]
[assembly: AssemblyFileVersion("0.9.1.0")]

// Semantic version (http://semver.org) . First 3 numbers must match first 3 numbers of AssemblyVersion and AssemblyFileVersion.
[assembly: AssemblyInformationalVersion("0.9.1-beta")]
