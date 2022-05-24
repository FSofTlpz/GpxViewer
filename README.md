# GpxViewer

The main purpose of GpxViewer is the readonly-view of a lot of gpx-tracks.
   
But it is also possible to create new tracks or edit a copy of the readonly-tracks.

Under the hood work the GMap.NET project for using different sources for the maps. It exist additional map-providers
for offline garmin-maps, geotiffs and garmin-kmz-maps. You can also use DEM-data.

### necessary nuget-projects

*BitMiracle.LibTiff.NET*
.NET version of LibTiff library made by Bit Miracle

*GeoAPI.CoordinateSystems*
GeoAPI.NET project provides a common framework based on OGC/ISO standards to improve interoperability among .NET GIS projects.

*Microsoft.Data.Sqlite*
Microsoft.Data.Sqlite is a lightweight ADO.NET provider for SQLite.

*NETStandard.Library*
A set of standard .NET APIs that are prescribed to be used and supported together. 

*Newtonsoft.Json*
Json.NET is a popular high-performance JSON framework for .NET

*ProjNET4GeoAPI*
.NET Spatial Reference and Projection Engine.
Proj.NET performs point-to-point coordinate conversions between geodetic coordinate systems for use in fx. Geographic Information Systems (GIS) or GPS applications. The spatial reference model used adheres to the Simple Features specification.

*SQLitePCLRaw.bundle_e_sqlite3*
This 'batteries-included' bundle brings in SQLitePCLRaw.core and the necessary stuff for certain common use cases.  Call SQLitePCL.Batteries.Init().  Policy of this bundle: e_sqlite3 included

*System.Data.SQLite*
Provides the data provider for SQL Server. These classes provide access to versions of SQL Server and encapsulate database-specific protocols, including tabular data stream (TDS)

*System.Security.Principal.Windows*
Provides classes for retrieving the current Windows user and for interacting with Windows users and groups.
