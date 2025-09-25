# GpxViewer

The main purpose of GpxViewer is the readonly-view of a lot of gpx-tracks.
   
But it is also possible to create new tracks or edit a copy of the readonly-tracks.

The search for locations use https://nominatim.openstreetmap.org.

It is possible setting/changing the exif-data for pictures for geo-coordinates.

The program can read GPX-, Garmin-GDB-, Google-KMZ- and -KML-Files.

Under the hood work the GMap.NET (2.1.6) project for using different sources for the maps. It exist additional map-providers
for offline garmin-maps, geotiffs and garmin-kmz-maps. You can also use DEM-data.
For further information see: https://github.com/judero01col/GMap.NET/wiki

FSofTUtils, GarminCore, GarminImageCreator, GMap.NET.Core, SpecialMapCtrl and 
GpxViewer.Common are also used in <a href="https://github.com/FSofTlpz/TrackEddi.Maui">TrackEddi</a>!

Additional parts are insert as source code:

* parts from Unclassified.UI http://unclassified.software/source for selecting colors
* ExifLibrary https://github.com/oozcitak/exiflibrary

A short Documentation is <a href="./Doc/GpxViewer.pdf">here</a>.

