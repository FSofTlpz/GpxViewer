# GpxViewer

The main purpose of GpxViewer is the readonly-view of a lot of gpx-tracks.
   
But it is also possible to create new tracks or edit a copy of the readonly-tracks.

The search for locations use https://nominatim.openstreetmap.org.

It is possible setting/changing the exif-data for pictures for geo-coordinates.

The program can read GPX-, Garmin-GDB-, Google-KMZ- and -KML-Files.

Under the hood work the GMap.NET (27.5.2022) project for using different sources for the maps. It exist additional map-providers
for offline garmin-maps, geotiffs and garmin-kmz-maps. You can also use DEM-data.

## necessary nuget-projects

**BitMiracle.LibTiff.NET**
.NET version of LibTiff library made by Bit Miracle

**ExifLibNet**
Exif metadata modification library.

**GeoAPI.CoordinateSystems**
GeoAPI.NET project provides a common framework based on OGC/ISO standards to improve interoperability among .NET GIS projects.

**NETStandard.Library**
A set of standard .NET APIs that are prescribed to be used and supported together. 

**Newtonsoft.Json**
Json.NET is a popular high-performance JSON framework for .NET

**ProjNET4GeoAPI**
.NET Spatial Reference and Projection Engine.
Proj.NET performs point-to-point coordinate conversions between geodetic coordinate systems for use in fx. Geographic Information Systems (GIS) or GPS applications. The spatial reference model used adheres to the Simple Features specification.

**System.Data.SqlClient**
Provides the data provider for SQL Server. These classes provide access to versions of SQL Server and encapsulate database-specific protocols, including tabular data stream (TDS)

**System.Data.SQLite**
The official SQLite database engine for both x86 and x64 along with the ADO.NET provider.  This package includes support for LINQ and Entity Framework 6.

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
      <deltapercent4search>1</deltapercent4search>

      <dem chachesize="16"
          cachepath="./demcachepath"
          minzoom="13"
          hillshadingazimut="315.0"
          hillshadingaltitude="45.0"
          hillshadingscale="20.0">%USERPROFILE%\daten\Gps\Data\srtm_zip</dem>

      <provider mapname="Garmin: Deutschland, erw. Basis, 12.2.2022"
             dbiddelta="0"
             zoom4display="1.0"
             tdb="%USERPROFILE%\daten\Gps\Garmin\ms7001_Deutschland, Basis, 12.2.2022\osmmap.tdb"
             typ="%USERPROFILE%\daten\Gps\Garmin\ms7001_Deutschland, Basis, 12.2.2022\fsoft3b.typ"
             textfactor="0.8"
             symbolfactor="1.0"
             linefactor="1.0"
             hillshading="true"
             hillshadingalpha="100">Garmin</provider>
      <provider mapname="Garmin: Deutschland, aio, 12.2.2022 (ohne Shading)"
              dbiddelta="1"
              zoom4display="1.0"
              tdb="%USERPROFILE%\daten\Gps\Garmin\ms7006_Deutschland, aio, 12.2.2022\osmmap.tdb"
              typ="%USERPROFILE%\daten\Gps\Garmin\ms7006_Deutschland, aio, 12.2.2022\fsoft3.typ"
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

      <providermenu lastuseditems="4">
         <item txt="Garmin-Karten">
            <subitem>0</subitem>
            <subitem>1</subitem>
         </item>
         <item txt="Microsoft-Bing-Karten">
            <subitem>5</subitem>
            <subitem>6</subitem>
            <subitem>7</subitem>
         </item>
         <item txt="Google-Karten">
            <subitem>8</subitem>
            <subitem>9</subitem>
            <subitem>10</subitem>
         </item>
         <item txt="OpenStreetMap">
            <subitem>11</subitem>
            <subitem>12</subitem>
         </item>
         <item txt="gescannte Karten">
            <subitem>3</subitem>
            <subitem>4</subitem>
         </item>
         <item no="2"/>
      </providermenu>
      
   </map>

   <tracks>
      <!-- Farben für Tracks -->
      <standard a="150" r="0" g="0" b="255" width="3.0"/>
      <standard2 a="150" r="0" g="255" b="255" width="3.0"/>
      <standard3 a="150" r="255" g="0" b="0" width="3.0"/>
      <standard4 a="150" r="0" g="255" b="0" width="3.0"/>
      <standard5 a="150" r="255" g="0" b="255" width="3.0"/>
      <marked a="255" r="0" g="255" b="255" width="3.5"/>
      <editable a="220" r="169" g="169" b="169" width="3.0"/>
      <inedit a="220" r="0" g="180" b="180" width="4.0"/>
      <selectedpart a="120" r="255" g="210" b="0" width="6.0"/>
      <helperline a="200" r="200" g="0" b="0" width="2.0"/>
      <slope>
         <slope percent="-1000" r="140" g="0" b="0"/>
         <slope percent="-20" r="255" g="0" b="0"/>
         <slope percent="-15" r="255" g="120" b="0"/>
         <slope percent="-10" r="255" g="216" b="0"/>
         <slope percent="-6" r="170" g="255" b="0"/>
         
         <slope percent="-2" r="0" g="120" b="0"/>

         <slope percent="20" r="140" g="0" b="0"/>
         <slope percent="15" r="255" g="0" b="0"/>
         <slope percent="10" r="255" g="120" b="0"/>
         <slope percent="6" r="255" g="216" b="0"/>
         <slope percent="2" r="170" g="255" b="0"/>
      </slope>
   </tracks>

   <garminsymbols>
      <group name="Marker">
         <symbol text="Waypoint"         name="Waypoint"        >../../GarminSymbols/Waypoint.png</symbol>
         <symbol text="Flag, Red"        name="Flag, Red"       offset="-7,-22">../../GarminSymbols/Flag, Red.png</symbol>
         <symbol text="Pin, Red"         name="Pin, Red"        offset="-1,-23">../../GarminSymbols/Pin, Red.png</symbol>
         <symbol text="Diamond, Red"     name="Diamond, Red"    >../../GarminSymbols/Diamond, Red.png</symbol>

...

         <symbol text="Pin purpur"         name="Google, Pin purpur"         >../../GoogleSymbols/purple-pushpin.png</symbol>
         <symbol text="Pin rot"            name="Google, Pin rot"            >../../GoogleSymbols/red-pushpin.png</symbol>
         <symbol text="Pin gelb"           name="Google, Pin gelb"           >../../GoogleSymbols/ylw-pushpin.png</symbol>
      </group>
   </garminsymbols>

</gpxview>
```
