﻿using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rhino.PlugIns;

// Plug-in Description Attributes - all of these are optional.
// These will show in Rhino's option dialog, in the tab Plug-ins.
[assembly: PlugInDescription(DescriptionType.Address, "-")]
[assembly: PlugInDescription(DescriptionType.Country, "Germany")]
[assembly: PlugInDescription(DescriptionType.Email, "lando.schumpich@gmail.com")]
[assembly: PlugInDescription(DescriptionType.Phone, "-")]
[assembly: PlugInDescription(DescriptionType.Fax, "-")]
[assembly: PlugInDescription(DescriptionType.Organization, "-")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "https://github.com/DerLando/RhinoBridge")]
[assembly: PlugInDescription(DescriptionType.WebSite, "http://landoarch.de")]

// Icons should be Windows .ico files and contain 32-bit images in the following sizes: 16, 24, 32, 48, and 256.
// This is a Rhino 6-only description.
[assembly: PlugInDescription(DescriptionType.Icon, "RhinoBridge.EmbeddedResources.plugin-utility.ico")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RhinoBridge")]

// This will be used also for the plug-in description.
[assembly: AssemblyDescription(@"RhinoBridge allows for imports into Rhino from Quixel Bridge, using the 'Custom Socket Export' option.
It uses a modified version of the Quixel 'bridge-c-sharp-plugin' hosted at https://github.com/Quixel/Bridge-C-Sharp-Plugin,
which in itself uses Newtonsoft.Json hosted at https://github.com/JamesNK/Newtonsoft.Json.
All software used and the plugIn itself is licensed under the MIT License.
url: https://github.com/DerLando/RhinoBridge")]

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("RhinoBridge")]
[assembly: AssemblyCopyright("Copyright © Lando Schumpich 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("49ad7eeb-8f5d-4536-b554-97a6012c0bd5")] // This will also be the Guid of the Rhino plug-in

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

[assembly: AssemblyVersion("0.2.1")]
[assembly: AssemblyFileVersion("1.0.0")]

// Make compatible with Rhino Installer Engine
[assembly: AssemblyInformationalVersion("0.2.1")]
