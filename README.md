# GpxViewer

The main purpose of GpxViewer is the readonly-view of a lot of gpx-tracks.
   
But it is also possible to create new tracks or edit a copy of the readonly-tracks.

Under the hood work the GMap.NET (27.5.2022) project for using different sources for the maps. It exist additional map-providers
for offline garmin-maps, geotiffs and garmin-kmz-maps. You can also use DEM-data.

The program can read GPX-, Garmin-GDB-, Google-KMZ- and -KML-Files.

### necessary nuget-projects

**BitMiracle.LibTiff.NET**
.NET version of LibTiff library made by Bit Miracle

**GeoAPI.CoordinateSystems**
GeoAPI.NET project provides a common framework based on OGC/ISO standards to improve interoperability among .NET GIS projects.

**Microsoft.Data.Sqlite**
Microsoft.Data.Sqlite is a lightweight ADO.NET provider for SQLite.

**NETStandard.Library**
A set of standard .NET APIs that are prescribed to be used and supported together. 

**Newtonsoft.Json**
Json.NET is a popular high-performance JSON framework for .NET

**ProjNET4GeoAPI**
.NET Spatial Reference and Projection Engine.
Proj.NET performs point-to-point coordinate conversions between geodetic coordinate systems for use in fx. Geographic Information Systems (GIS) or GPS applications. The spatial reference model used adheres to the Simple Features specification.

**SQLitePCLRaw.bundle_e_sqlite3**
This 'batteries-included' bundle brings in SQLitePCLRaw.core and the necessary stuff for certain common use cases.  Call SQLitePCL.Batteries.Init().  Policy of this bundle: e_sqlite3 included

**System.Data.SQLite**
Provides the data provider for SQL Server. These classes provide access to versions of SQL Server and encapsulate database-specific protocols, including tabular data stream (TDS)

**System.Security.Principal.Windows**
Provides classes for retrieving the current Windows user and for interacting with Windows users and groups.

Also used:

ColorWheel, ColorSelector, ColorFader from Yves Goergen (http://unclassified.software/source/colorwheel ...)


### example for gpxviewer.xml

```
<?xml version="1.0" encoding="Windows-1252"?>
<gpxview xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="gpxview.xsd">

	<minimaltrackpointdistance x="14" y="14"/>

	<proxy>
    <!-- falls Internet nur über Proxy erreichbar ist -->
    <proxyname></proxyname>
    <proxyport></proxyport>
    <proxyuser></proxyuser>
    <proxypassword></proxypassword>
  </proxy>

  <map>
    <cachelocation>.</cachelocation>
    <!-- bei false zusätzlich Cache in "%LOCALAPPDATA%\GMap.NET\TileDBv5\en\Data.gmdb" bzw. in cachelocation -->
    <serveronly>false</serveronly>
    <!-- Index des Providers beim Start -->
    <startprovider>0</startprovider>
    <!-- Mittelpunkt der Karte beim Start -->
    <startlatitude>51.25</startlatitude>
    <startlongitude>12.33</startlongitude>
    <!--Zoom beim Start-->
    <startzoom>14</startzoom>

    <dem chachesize="16"
         hillshadingazimut="315.0"
         hillshadingaltitude="45.0"
         hillshadingscale="2500.0"
         hillshadingz="1.0">%USERPROFILE%\daten\Gps\Data\srtm_zip</dem>

    <provider mapname="Garmin: Deutschland, erw. Basis, 23.7.2020"
             dbiddelta="0"
             zoom4display="1.0"
             tdb="%USERPROFILE%\daten\Gps\Garmin\ms7007_Deutschland, erw. Basis, 23.7.2020\osmmap.tdb"
             typ="%USERPROFILE%\daten\Gps\Garmin\ms7007_Deutschland, erw. Basis, 23.7.2020\fsoft3.typ"
             maxsubdiv="1000"
             textfactor="0.8"
             symbolfactor="1.0"
             linefactor="1.0"
             hillshading="true"
             hillshadingalpha="100">Garmin</provider>
    <provider mapname="Garmin: Deutschland, aio, 23.7.2020"
              dbiddelta="1"
              zoom4display="1.0"
              tdb="%USERPROFILE%\daten\Gps\Garmin\ms7006_Deutschland, aio, 23.7.2020\osmmap.tdb"
              typ="%USERPROFILE%\daten\Gps\Garmin\ms7006_Deutschland, aio, 23.7.2020\fsoft3b.typ"
              maxsubdiv="1000000"
              textfactor="0.8"
              symbolfactor="1.0"
              linefactor="1.0">Garmin</provider>

    <provider mapname="online: Geodienste Sachsen"
              dbiddelta="0"
              zoom4display="1.0"
              url="https://geodienste.sachsen.de/wms_geosn_webatlas-sn/guest?"
              version="1.1.1"
              srs="srs=EPSG:4326"
              format="png"
              layers="Siedlung,Vegetation,Gewaesser,Verkehr,Beschriftung"
              extended="">WMS</provider>

    <provider mapname="Spreewald"
              dbiddelta="0"
              zoom4display="1.0"
              kmzfile="./Sonstiges/Spreewald_8.kmz">GarminKMZ</provider>

    <provider mapname="Berchtesgaden"
              dbiddelta="1"
              zoom4display="1.0"
              kmzfile="./Sonstiges/Berchtesgaden.kmz">GarminKMZ</provider>

    <provider mapname="online: MS Bing">BingMap</provider>
    <provider mapname="online: MS Bing Hybrid">BingHybridMap</provider>
    <provider mapname="online: MS Bing Satellit">BingSatelliteMap</provider>
    <provider mapname="online: Google">GoogleMap</provider>
    <provider mapname="online: Google Oberfläche">GoogleTerrainMap</provider>
    <provider mapname="online: Google Satellit">GoogleSatelliteMap</provider>
    <provider mapname="online: OpenStreetMap">OpenStreetMap</provider>
    <provider mapname="online: OpenStreetMap Fahrradkarte">OpenCycleLandscapeMap</provider>
  </map>

  <tracks>
    <!-- colors for tracks -->
    <standard a="150" r="0" g="0" b="255" width="3.0"/>
    <standard2 a="150" r="0" g="255" b="255" width="3.0"/>
    <standard3 a="150" r="255" g="0" b="0" width="3.0"/>
    <standard4 a="150" r="0" g="255" b="0" width="3.0"/>
    <standard5 a="150" r="255" g="0" b="255" width="3.0"/>
    <marked a="255" r="0" g="255" b="255" width="3.5"/>
    <editable a="220" r="169" g="169" b="169" width="3.0"/>
    <inedit a="220" r="0" g="180" b="180" width="4.0"/>
    <selectedpart a="120" r="255" g="210" b="0" width="6.0"/>
  </tracks>

</gpxview>
```
