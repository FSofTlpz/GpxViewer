using FSofTUtils.Geography.PoorGpx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FSofTUtils.Geography {
   public class KmlWriter {
      // https://developers.google.com/kml/documentation/kmlreference
      // https://developers.google.com/kml/documentation/kmlreference#placemark

      /*
      KML-Dateien scheinen viele Möglichkeiten zu bieten, Tracks und Waypoints zu speichern.
      Auch die Struktur für Folder scheint beliebig zu sein.
      Insofern dürfte die Konvertierung Kml -> Gpx relativ schwierig sein.
      Vermutlich kann man dann nur nach Placemarks mit einer passenden Geometrie im XML-Baum suchen.
      Die beste Umsetzung für Tracks scheint mit gx-Erweiterung möglich zu sein.
      Nur dann sind auch Timestamps für die einzelnen Trackpunkte möglich.
      */

      // This is the default name for writing a KML file to a new archive, however, the default file for reading from an archive is the first file in the table of contents 
      // that ends with ".kml".
      private const string defaultKmlFilename = "doc.kml";


      class XPaths {

         public static string Root => "/x:kml"; 

         public static string Gdal_Document => Root + "/x:Document[@id=\"root_doc\"]"; 

         /// <summary>
         /// 
         /// </summary>
         /// <param name="folderidx">Index 1,..</param>
         /// <returns></returns>
         public static string Gdal_DocumentFolder(int folderidx) => 
            Gdal_Document + "/x:Folder[" + folderidx.ToString() + "]";

         /// <summary>
         /// 
         /// </summary>
         /// <param name="folderidx">Index 1,..</param>
         /// <param name="placemarkidx">Index 1,..</param>
         /// <returns></returns>
         public static string Gdal_Placemark4Folder(int folderidx, int placemarkidx) =>
            Gdal_DocumentFolder(folderidx) + "/Placemark[" + placemarkidx.ToString() + "]";

      }


      SimpleXmlDocument2? kml;


      /// <summary>
      /// schreibt eine KML-Datei in der Form von GDAL; dabei fehlt leider die Zeit für die Trackpunkte
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="gpx"></param>
      /// <param name="formatted"></param>
      /// <param name="cola"></param>
      /// <param name="colr"></param>
      /// <param name="colg"></param>
      /// <param name="colb"></param>
      /// <param name="width"></param>
      public void Write_gdal(string filename,
                             GpxAll gpx,
                             bool formatted,
                             IList<Color> trackcolor,
                             IList<uint> width) {
         createbasekml_gdal(filename);

         int trackcount = gpx.Tracks.Count;
         for (int t = 0; t < trackcount; t++) {
            Color col = (trackcolor != null && t < trackcolor.Count) ? trackcolor[t] : Color.Black;
            insertTrack_gdal_gx(gpx.Tracks[t],
                                col.A,
                                col.R,
                                col.G,
                                col.B,
                                (width != null && t < width.Count) ? width[t] : 4);
         }

         int waypointcount = gpx.Waypoints.Count;
         for (int w = 0; w < waypointcount; w++) {
            GpxWaypoint pt = gpx.Waypoints[w];
            insertWaypoint_gdal(pt);
         }

         writekml(filename,
                  Path.GetExtension(filename).ToLower() == ".kmz",
                  formatted);
      }

      string codedTimeStamp(DateTime dt) => dt.ToString("s") + "Z";

      string codedText(string txt) => BaseElement.XmlEncode(txt);

      void createbasekml_gdal(string filename) {
         kml = new SimpleXmlDocument2(filename, "kml") {
            Validating = false,
            XsdFilename = null
         };
         kml.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                     "<kml xmlns=\"http://www.opengis.net/kml/2.2\" " +
                     "xmlns:gx=\"http://www.google.com/kml/ext/2.2\" " +      // KML Extension gx
                     "xmlns:kml=\"http://www.opengis.net/kml/2.2\" " +
                     "xmlns:atom=\"http://www.w3.org/2005/Atom\">" +
                     "</kml>");
         kml.AddNamespace("x");


         kml.InsertXmlText(XPaths.Root,
                           "<Document id=\"root_doc\" />",
                           SimpleXmlDocument2.InsertPosition.AppendChild);
         kml.InsertXmlText(XPaths.Gdal_Document,
                           "<Folder />",
                           SimpleXmlDocument2.InsertPosition.AppendChild);
         kml.InsertXmlText(XPaths.Gdal_DocumentFolder(1),
                           "<name>tracks</name>",
                           SimpleXmlDocument2.InsertPosition.AppendChild);
         kml.InsertXmlText(XPaths.Gdal_Document,
                           "<Folder />",
                           SimpleXmlDocument2.InsertPosition.AppendChild);
         kml.InsertXmlText(XPaths.Gdal_DocumentFolder(2),
                           "<name>waypoints</name>",
                           SimpleXmlDocument2.InsertPosition.AppendChild);

         kml.InsertXmlText(XPaths.Gdal_Document,
                           "<Schema name=\"tracks\" id=\"tracks\">" +
                           "<SimpleField name=\"cmt\" type=\"string\"/>" +
                           "<SimpleField name=\"desc\" type=\"string\"/>" +
                           "<SimpleField name=\"src\" type=\"string\"/>" +
                           "<SimpleField name=\"link1_href\" type=\"string\"/>" +
                           "<SimpleField name=\"link1_text\" type=\"string\"/>" +
                           "<SimpleField name=\"link1_type\" type=\"string\"/>" +
                           "<SimpleField name=\"link2_href\" type=\"string\"/>" +
                           "<SimpleField name=\"link2_text\" type=\"string\"/>" +
                           "<SimpleField name=\"link2_type\" type=\"string\"/>" +
                           "<SimpleField name=\"number\" type=\"int\"/>" +
                           "<SimpleField name=\"type\" type=\"string\"/>" +
                           "</Schema>",
                           SimpleXmlDocument2.InsertPosition.AppendChild);

         kml.InsertXmlText(XPaths.Gdal_Document,
                           "<Schema name=\"waypoints\" id=\"waypoints\">" +
                           "<SimpleField name=\"ele\" type=\"float\"/>" +
                           "<SimpleField name=\"time\" type=\"string\"/>" +
                           "<SimpleField name=\"magvar\" type=\"float\"/>" +
                           "<SimpleField name=\"geoidheight\" type=\"float\"/>" +
                           "<SimpleField name=\"cmt\" type=\"string\"/>" +
                           "<SimpleField name=\"desc\" type=\"string\"/>" +
                           "<SimpleField name=\"src\" type=\"string\"/>" +
                           "<SimpleField name=\"link1_href\" type=\"string\"/>" +
                           "<SimpleField name=\"link1_text\" type=\"string\"/>" +
                           "<SimpleField name=\"link1_type\" type=\"string\"/>" +
                           "<SimpleField name=\"link2_href\" type=\"string\"/>" +
                           "<SimpleField name=\"link2_text\" type=\"string\"/>" +
                           "<SimpleField name=\"link2_type\" type=\"string\"/>" +
                           "<SimpleField name=\"sym\" type=\"string\"/>" +
                           "<SimpleField name=\"type\" type=\"string\"/>" +
                           "<SimpleField name=\"fix\" type=\"string\"/>" +
                           "<SimpleField name=\"sat\" type=\"int\"/>" +
                           "<SimpleField name=\"hdop\" type=\"float\"/>" +
                           "<SimpleField name=\"vdop\" type=\"float\"/>" +
                           "<SimpleField name=\"pdop\" type=\"float\"/>" +
                           "<SimpleField name=\"ageofdgpsdata\" type=\"float\"/>" +
                           "<SimpleField name=\"dgpsid\" type=\"int\"/>" +
                           "<SimpleField name=\"gpxx_WaypointExtension\" type=\"string\"/>" +
                           "</Schema>",
                           SimpleXmlDocument2.InsertPosition.AppendChild);
      }

      void insertTrack_gdal(GpxTrack track,
                            uint cola,
                            uint colr,
                            uint colg,
                            uint colb,
                            uint width) {
         /*
            <Placemark>
               <name>W 20200909, Ruhpolding, 7,7km</name>
               <Style>
                  <LineStyle>
                     <color>cc0055ff</color>
                     <width>4</width>
                  </LineStyle>
                  <PolyStyle>
                     <fill>0</fill>
                  </PolyStyle>
               </Style>
               <MultiGeometry>
                  <LineString>
                     <coordinates>12.6433829404414,47.7585958968848 12.6434474810958,47.7587980683893 12.6438846811652,47.7587441727519 ... </coordinates>
                  </LineString>
               </MultiGeometry>
            </Placemark>
          */
         StringBuilder sb = new StringBuilder();
         sb.Append("<Placemark>");
         sb.Append("<name>" + track.Name + "</name>");
         if (!string.IsNullOrEmpty(track.Description))
            sb.Append("<description>" + track.Description + "</description>");
         sb.Append(buildLineStyle(cola, colr, colg, colb, width));
         sb.Append(buildMultiGeometry(track));
         sb.Append("</Placemark>");

         kml?.InsertXmlText(XPaths.Gdal_DocumentFolder(1),
                            sb.ToString(),
                            SimpleXmlDocument2.InsertPosition.AppendChild);
      }

      void insertTrack_gdal_gx(GpxTrack track,
                               uint cola,
                               uint colr,
                               uint colg,
                               uint colb,
                               uint width) {

         StringBuilder sb_multitrack = buildMultitrack(track, out DateTime dtMin, out DateTime dtMax);

         StringBuilder sb = new StringBuilder();
         sb.Append("<Placemark>");

         sb.Append("<name>" + codedText(track.Name) + "</name>");
         if (BaseElement.ValueIsValid(dtMin) ||
             BaseElement.ValueIsValid(dtMax)) {
            sb.Append("<TimeSpan>");
            if (BaseElement.ValueIsValid(dtMin))
               sb.Append("<begin>" + DateTime2String(dtMin) + "</begin>");
            if (BaseElement.ValueIsValid(dtMax))
               sb.Append("<end>" + DateTime2String(dtMax) + "</end>");
            sb.Append("</TimeSpan>");
         }
         if (!string.IsNullOrEmpty(track.Description))
            sb.Append("<description>" + codedText(track.Description) + "</description>");
         sb.Append(buildLineStyle(cola, colr, colg, colb, width));
         sb.Append(sb_multitrack);
         sb.Append("</Placemark>");

         kml?.InsertXmlText(XPaths.Gdal_DocumentFolder(1),
                            sb.ToString(),
                            SimpleXmlDocument2.InsertPosition.AppendChild);
      }


      StringBuilder buildLineStyle(uint cola,
                                   uint colr,
                                   uint colg,
                                   uint colb,
                                   uint width) {
         StringBuilder sb = new StringBuilder();
         sb.Append("<Style>");
         sb.Append("<LineStyle>");
         sb.Append("<color>" +
                   cola.ToString("x2") +
                   colb.ToString("x2") +
                   colg.ToString("x2") +
                   colr.ToString("x2") +
                   "</color>");
         sb.Append("<width>" + width.ToString() + "</width>");
         sb.Append("</LineStyle>");
         sb.Append("<PolyStyle><fill>false</fill></PolyStyle>");
         sb.Append("</Style>");
         return sb;
      }

      /// <summary>
      /// erzeugt den Code für die Trackkoordinaten als gx:MultiTrack
      /// </summary>
      /// <param name="track"></param>
      /// <param name="dtMin"></param>
      /// <param name="dtMax"></param>
      /// <returns></returns>
      StringBuilder buildMultitrack(GpxTrack track, out DateTime dtMin, out DateTime dtMax) {
         /*
         <gx:Track>
         <when>2010-11-14T10:38:37Z</when>
         <when>2010-11-14T10:38:44Z</when>
         <when>2010-11-14T10:38:51Z</when>
         <when>2010-11-14T10:38:53Z</when>
         <gx:coord>13.0614286847413,47.0498898904771,1607.56108587361</gx:coord>
         <gx:coord>13.0613650660962,47.0500645693392,1607.18971291631</gx:coord>
         <gx:coord>13.0612696800381,47.0502318721265,1606.83617557488</gx:coord>
         <gx:coord>13.0612037144601,47.0503962412477,1606.48827516032</gx:coord>
         <gx:coord>13.0610411893576,47.0506552420557,1605.93244319095</gx:coord>
         </gx:Track>


         <gx:MultiTrack>
           <gx:Track>...</gx:Track>            <!-- one or more gx:Track elements -->
         </gx:MultiTrack>
          */
         StringBuilder sb = new StringBuilder();
         StringBuilder sb_when = new StringBuilder();
         StringBuilder sb_coord = new StringBuilder();
         dtMin = BaseElement.NOTVALID_TIME;
         dtMax = BaseElement.NOTVALID_TIME;
         sb.Append("<gx:MultiTrack>");
         for (int s = 0; s < track.Segments.Count; s++) {
            sb.Append("<gx:Track>");
            sb_when.Clear();
            sb_coord.Clear();
            for (int p = 0; p < track.Segments[s].Points.Count; p++) {
               GpxTrackPoint pt = track.Segments[s].Points[p];

               if (BaseElement.ValueIsValid(pt.Time)) {
                  if (BaseElement.ValueIsValid(dtMin)) {
                     if (dtMin > pt.Time)
                        dtMin = pt.Time;
                  } else
                     dtMin = pt.Time;

                  if (BaseElement.ValueIsValid(dtMax)) {
                     if (dtMax < pt.Time)
                        dtMax = pt.Time;
                  } else
                     dtMax = pt.Time;
               }

               if (BaseElement.ValueIsValid(pt.Time))
                  sb_when.Append("<when>" +
                                 (BaseElement.ValueIsValid(pt.Time) ?
                                    DateTime2String(pt.Time) :
                                    "") +
                                 "</when>");

               sb_coord.Append("<gx:coord>" +
                              Double2String(pt.Lon) + "," +
                              Double2String(pt.Lat) +
                              (BaseElement.ValueIsValid(pt.Elevation) ?
                                 "," + Double2String(pt.Elevation) :
                                 "") +
                              "</gx:coord>");
            }

            sb.Append(sb_when);
            sb.Append(sb_coord);
            sb.Append("</gx:Track>");
         }
         sb.Append("</gx:MultiTrack>");
         return sb;
      }

      /// <summary>
      /// erzeugt den Code für die Trackkoordinaten als MultiGeometry (ohne Zeitangabe je Trackpunkt!)
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      StringBuilder buildMultiGeometry(GpxTrack track) {
         /*
            <MultiGeometry>
               <LineString>
                  <coordinates>12.6433829404414,47.7585958968848 12.6434474810958,47.7587980683893 ... </coordinates>
               </LineString>
            </MultiGeometry>
          */
         StringBuilder sb = new StringBuilder();
         sb.Append("<MultiGeometry>");
         for (int s = 0; s < track.Segments.Count; s++) {
            sb.Append("<LineString><coordinates>");
            for (int p = 0; p < track.Segments[s].Points.Count; p++) {
               GpxTrackPoint pt = track.Segments[s].Points[p];
               sb.Append(" ");
               sb.Append(Double2String(pt.Lon));
               sb.Append(",");
               sb.Append(Double2String(pt.Lat));
               if (BaseElement.ValueIsValid(pt.Elevation)) {
                  sb.Append(",");
                  sb.Append(Double2String(pt.Elevation));
               }
            }
            sb.Append("</coordinates></LineString>");
         }
         sb.Append("</MultiGeometry>");
         return sb;
      }

      void insertWaypoint_gdal(GpxWaypoint pt) {
         StringBuilder sb = new StringBuilder();
         sb.Append("<Placemark>");
         sb.Append("<name>" + codedText(pt.Name) + "</name>");
         sb.Append("<ExtendedData>");
         sb.Append("<SchemaData schemaUrl=\"#waypoints\">");
         if (BaseElement.ValueIsValid(pt.Elevation))
            sb.Append("<SimpleData name=\"ele\">" + Double2String(pt.Elevation) + "</SimpleData>");
         if (BaseElement.ValueIsValid(pt.Time))
            sb.Append("<SimpleData name=\"time\">" + codedTimeStamp(pt.Time) + "</SimpleData>");
         if (!string.IsNullOrEmpty(pt.Comment))
            sb.Append("<SimpleData name=\"cmt\">" + codedText(pt.Comment) + "</SimpleData>");
         if (!string.IsNullOrEmpty(pt.Description))
            sb.Append("<SimpleData name=\"desc\">" + codedText(pt.Description) + "</SimpleData>");
         sb.Append("</SchemaData>");
         sb.Append("</ExtendedData>");
         sb.Append("<Point><coordinates>" + Double2String(pt.Lon) + "," + Double2String(pt.Lat) + "</coordinates></Point>");
         sb.Append("</Placemark>");
         /*
            <Placemark>
               <name>Hotel Hirschhaus</name>
               <ExtendedData>
                  <SchemaData schemaUrl="#waypoints">
                     <SimpleData name="ele">681</SimpleData>
                     <SimpleData name="time">2020/09/16 15:57:52+00</SimpleData>
                     <SimpleData name="cmt">38a</SimpleData>
                     <SimpleData name="desc">38a</SimpleData>
                     <SimpleData name="sym">Lodging</SimpleData>
                     <SimpleData name="gpxx_WaypointExtension">&lt;gpxx:DisplayMode&gt;SymbolAndName&lt;/gpxx:DisplayMode&gt;   </SimpleData>
                  </SchemaData>
               </ExtendedData>
               <Point>
                  <coordinates>12.6435506623238,47.7586564142257</coordinates>
               </Point>
            </Placemark>
          */
         kml?.InsertXmlText(XPaths.Gdal_DocumentFolder(2),
                            sb.ToString(),
                            SimpleXmlDocument2.InsertPosition.AppendChild);
      }

      /// <summary>
      /// schreibt die KML-Daten als Datei
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="zipped"></param>
      /// <param name="formatted"></param>
      void writekml(string filename, bool zipped, bool formatted) {
         if (zipped) {
            using (FileStream zipstream = new FileStream(filename, FileMode.Create)) {
               using (ZipArchive archive = new ZipArchive(zipstream, ZipArchiveMode.Update)) {
                  ZipArchiveEntry file = archive.CreateEntry(defaultKmlFilename);
                  using (Stream writer = file.Open()) {
                     kml?.SaveData(null, true, writer);
                  }
               }
            }
         } else
            kml?.SaveData(filename, formatted);
      }

      /// <summary>
      /// liefert Datum und Zeit im Format für die GPX-Datei
      /// </summary>
      /// <param name="dt"></param>
      /// <returns></returns>
      static string DateTime2String(DateTime dt) => dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");

      /// <summary>
      /// liefert eine double-Zahl als Text
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      static string Double2String(double v) => v.ToString(CultureInfo.InvariantCulture);

      #region INFOS

      /*
<Placemark>
<name> New point</name> 
<TimeSpan> 
<begin>2010-12-20T03:00:00Z</begin>
<end>2010-12-21T02:59:59Z</end>
</TimeSpan>
<description>This is a new point to learn KML format.</description>

UTC:
<TimeStamp>
<when>1997-07-16T07:30:15Z</when>
</TimeStamp>


extension of the OGC KML 2.2 standard:
<Placemark>
<name>gx-Track</name>
<styleUrl>#msn_track-0</styleUrl>
<gx:Track>
<when>2010-11-14T10:38:37Z</when>
<when>2010-11-14T10:38:44Z</when>
<when>2010-11-14T10:38:51Z</when>
<when>2010-11-14T10:38:53Z</when>
<when>2010- ...
<gx:coord>13.938957 51.077421 280.8</gx:coord>
<gx:coord>13.939102 51.077421 280.8</gx:coord>
<gx:coord>13.939245 51.077424 280.8</gx:coord>
<gx:coord>13.939392 51.082424 280.8</gx:coord>
<gx:coord>13.9 ...
</gx:Track>
</Placemark>

<gx:coord>
A coordinate value consisting of three values for longitude, latitude, and altitude, with no comma separators. For example:
<gx:coord>-122.207881 37.371915 156.000000</gx:coord>
Note that the syntax for the <gx:coord> element is different from the syntax for the <coordinates> element

      */

      /*
erzeugt mit GDAL:

<?xml version="1.0" encoding="utf-8" ?>
<kml xmlns="http://www.opengis.net/kml/2.2">
<Document id="root_doc">
<Schema name="tracks" id="tracks">
   <SimpleField name="cmt" type="string"/>
   <SimpleField name="desc" type="string"/>
   <SimpleField name="src" type="string"/>
   <SimpleField name="link1_href" type="string"/>
   <SimpleField name="link1_text" type="string"/>
   <SimpleField name="link1_type" type="string"/>
   <SimpleField name="link2_href" type="string"/>
   <SimpleField name="link2_text" type="string"/>
   <SimpleField name="link2_type" type="string"/>
   <SimpleField name="number" type="int"/>
   <SimpleField name="type" type="string"/>
</Schema>
<Folder>
   <name>tracks</name>
   <Placemark>
      <name>W 20200909, Ruhpolding, 7,7km</name>
      <Style>
         <LineStyle>
            <color>cc0055ff</color>
            <width>4</width>
         </LineStyle>
         <PolyStyle>
            <fill>0</fill>
         </PolyStyle>
      </Style>
      <MultiGeometry>
         <LineString>
            <coordinates>12.6433829404414,47.7585958968848 12.6434474810958,47.7587980683893 12.6438846811652,47.7587441727519 12.6436768099666,47.7579266857356 12.6434833556414,47.7570507768542 12.6430174056441,47.7554473187774 12.6427808683366,47.7552184928209 12.6423006691039,47.7546839788556 12.6412432920188,47.753268359229 12.6412628218532,47.7531868033111 12.6410584710538,47.7528283931315 12.6412214152515,47.7529631741345 12.6408706326038,47.7527125552297 12.6405179221183,47.7523652091622 12.6404211111367,47.7522378880531 12.6403354480863,47.751941671595 12.640329496935,47.7518570143729 12.6404750067741,47.7518277615309 12.6408428885043,47.7518417593092 12.6411820203066,47.7517873607576 12.6413078326732,47.7517728600651 12.6413505803794,47.7517808228731 12.6413750555366,47.7517605386674 12.6415212359279,47.7517426013947 12.6418765448034,47.7517640590668 12.6420889422297,47.7517548389733 12.6425604242831,47.7517944853753 12.6428205147386,47.751939157024 12.6429329998791,47.7519389055669 12.6430459879339,47.7520330343395 12.6431420445442,47.7521474473178 12.6431687828153,47.7522818092257 12.6431153900921,47.7524647861719 12.643117653206,47.7526487689465 12.6429291442037,47.7530354261398 12.64289310202,47.7534027211368 12.6429056748748,47.7535250969231 12.6429418008775,47.753746798262 12.6430398691446,47.753894906491 12.6431541983038,47.754029519856 12.6432530209422,47.7540923841298 12.6433703675866,47.7542603574693 12.6434385124594,47.7543129120022 12.6434651669115,47.7543163485825 12.6439419295639,47.7547389641404 12.6441737730056,47.7549085300416 12.6443772017956,47.7550842985511 12.6448051817715,47.7553739771247 12.6452326588333,47.7556163817644 12.6456632371992,47.7557935751975 12.6456978544593,47.7557824272662 12.6458648219705,47.7555762324482 12.645977223292,47.7555182296783 12.6460942346603,47.7555323112756 12.6463238988072,47.7556537650526 12.6465449295938,47.7557292021811 12.6466492842883,47.7557993587106 12.6468948740512,47.7558834291995 12.6470994763076,47.7559831738472 12.6474180724472,47.7560965809971 12.6475241873413,47.7561198826879 12.6478480640799,47.7562597766519 12.6479255966842,47.7563523128629 12.6480799913406,47.7564454358071 12.6482311170548,47.7565817255527 12.6483021955937,47.7567382995039 12.6482908800244,47.7568356972188 12.648308146745,47.7568702306598 12.6485325302929,47.7570574823767 12.6486260723323,47.7571858931333 12.6487122382969,47.7572640124708 12.6488073728979,47.7575485780835 12.6489120628685,47.7577370870858 12.6489997375757,47.758052079007 12.6490304153413,47.7580825053155 12.6490729115903,47.7580768056214 12.649016501382,47.7580709382892 12.6488911081105,47.7580236643553 12.6489489432424,47.7580825891346 12.6490154955536,47.7582570165396 12.648958414793,47.7585536520928 12.6489552296698,47.7588212862611 12.6489623542875,47.7589482720941 12.6490340195596,47.7591973822564 12.6491466723382,47.7594012301415 12.649288745597,47.7595660183579 12.649418246001,47.7596541959792 12.6495762448758,47.7597294654697 12.6496448088437,47.7597885578871 12.649748492986,47.7598376758397 12.6498452201486,47.7599057368934 12.6498796697706,47.7599646616727 12.6499219145626,47.7599739655852 12.6499737147242,47.7600125223398 12.6499701105058,47.760057868436 12.6498880516738,47.7601735386997 12.6500747166574,47.760322149843 12.6502567715943,47.7603482175618 12.6503393333405,47.7604106627405 12.6504438556731,47.7604316174984 12.6506554987282,47.7604234032333 12.6507989130914,47.7604395803064 12.651191605255,47.7606166899204 12.651337031275,47.7607177756727 12.6514843851328,47.7607592660934 12.6516701281071,47.7608441747725 12.6518293004483,47.7609373815358 12.6521252654493,47.7612218633294 12.6523296162486,47.7613826282322 12.6524704322219,47.7615398727357 12.6525397505611,47.7615521103144 12.652880307287,47.7615043334663 12.6531808823347,47.7614853903651 12.6533113047481,47.7615336701274 12.6536120474339,47.7614746615291 12.6537635084242,47.7614740747958 12.6541660912335,47.7615191694349 12.6542142871767,47.7615370228887 12.654526764527,47.7615796867758 12.6547157764435,47.7615721430629 12.6549579296261,47.7615188341588 12.6551481150091,47.7614336740226 12.6553262304515,47.7612699754536 12.6556371152401,47.7611780259758 12.6558412984014,47.7611456718296 12.6560587249696,47.761210128665 12.6562601421028,47.7612431533635 12.6562746427953,47.7612249646336 12.6563470624387,47.7612825483084 12.6564174704254,47.7613009884953 12.6565025467426,47.7613684628159 12.6564859505743,47.7614159882069 12.6564158778638,47.7614698000252 12.6563529297709,47.7615622524172 12.6561049092561,47.7617498394102 12.6559375226498,47.7619165554643 12.6558875665069,47.7620016317815 12.6557990536094,47.7620810084045 12.6555713173002,47.7621781546623 12.6554724946618,47.7622427791357 12.6553898490965,47.7623100019991 12.6553523819894,47.7623799070716 12.6551632024348,47.7625872753561 12.6550628710538,47.762917522341 12.655076701194,47.7632298320532 12.6550067123026,47.7633962966502 12.6549209654331,47.7639776654541 12.6548358052969,47.7643167134374 12.6546908821911,47.7646679151803 12.6546161156148,47.7649907860905 12.6544837653637,47.765103019774 12.6543582044542,47.7651804685593 12.6540676876903,47.7654296625406 12.6538127940148,47.765771895647 12.6534781884402,47.766049169004 12.6530080474913,47.7663489058614 12.652872512117,47.7664573676884 12.6526934746653,47.7665494009852 12.6523916423321,47.7667605411261 12.6522009540349,47.7668576035649 12.6520154625177,47.7671454381198 12.6518953498453,47.7672719210386 12.651733411476,47.7673989906907 12.6516041625291,47.7675586659461 12.6514771766961,47.7677960414439 12.6512279827148,47.7679842989892 12.6511147432029,47.7680034935474 12.6509849075228,47.7680679503828 12.6508749369532,47.768192589283 12.6507574226707,47.7682915795594 12.6507315225899,47.7684997860342 12.6506572589278,47.7687319647521 12.6503790635616,47.7691211365163 12.6503239106387,47.7691664826125 12.6503345556557,47.76934501715 12.6502623874694,47.7694907784462 12.6501814182848,47.7699011564255 12.6499744690955,47.7703689504415 12.6498106867075,47.7705307211727 12.6497582998127,47.7705444674939 12.6494982093573,47.7708525862545 12.6494073495269,47.7708971779794 12.64927550219,47.7712156902999 12.6490896753967,47.7714525628835 12.6488249748945,47.7716699056327 12.6487173512578,47.7717390563339 12.648654570803,47.7717582508922 12.648355755955,47.7719520404935 12.6481969188899,47.7720957901329 12.6481638103724,47.7721107937396 12.6479970104992,47.7721163257957 12.6477024704218,47.7722138911486 12.6476484071463,47.7722616679966 12.6475460641086,47.7725069224834 12.6475132070482,47.7725419588387 12.6473196689039,47.7725852932781 12.6473024021834,47.7726059127599 12.6471021585166,47.7724391967058 12.6469027530402,47.7723659388721 12.6469015795738,47.7723440621048 12.6469263061881,47.7723871450871 12.6468937844038,47.7724045794457 12.6468682195991,47.7723687887192 12.6468000747263,47.7723687887192 12.6466851588339,47.7722839638591 12.646616930142,47.7721880748868 12.646563872695,47.7719686366618 12.6466264016926,47.7718353644013 12.6467644516379,47.7717562392354 12.6470558904111,47.771535879001 12.6471376139671,47.771501513198 12.6473549567163,47.7715108171105 12.6475068368018,47.7713606134057 12.6475743949413,47.7711212262511 12.6476556155831,47.7709938213229 12.6477073319256,47.7708262670785 12.6479031331837,47.7703739795834 12.6480428595096,47.7702334988862 12.6481782272458,47.7701398730278 12.6485286746174,47.76994231157 12.648694133386,47.7697957120836 12.6489654555917,47.7696221228689 12.6491848938167,47.7693414967507 12.6492509432137,47.7691858448088 12.6493185013533,47.7691012714058 12.6495008077472,47.7690080646425 12.6500197313726,47.7683001291007 12.6501459628344,47.7679673675448 12.6501848548651,47.7676512859762 12.6502998545766,47.7675210312009 12.6506157685071,47.7673004195094 12.6508926227689,47.7670543268323 12.6509788725525,47.7670040354133 12.6511743385345,47.7668442763388 12.65127341263,47.766686277464 12.6514596585184,47.7665413543582 12.6516345888376,47.7664201520383 12.6521473936737,47.7662005461752 12.6522886287421,47.7661052439362 12.6523911394179,47.7660105284303 12.652494572103,47.7657683752477 12.6526511460543,47.7656413055956 12.6528449356556,47.765539214015 12.6530260685831,47.7653687261045 12.653456479311,47.7650655526668 12.6534794457257,47.7650091424584 12.6535402145237,47.7649801410735 12.6537614129484,47.7649321127683 12.6539238542318,47.7648789715022 12.6539723854512,47.7648350503296 12.6539788395166,47.7646863553673 12.6539393607527,47.7643853612244 12.653736518696,47.7638276293874 12.653884543106,47.7636418864131 12.6539202500135,47.7634220290929 12.6539787556976,47.7633130643517 12.6540624909103,47.7633157465607 12.6539997942746,47.7632934506983 12.6539725530893,47.7632033452392 12.6540820207447,47.7630045264959 12.6539706252515,47.7628915384412 12.6538835372776,47.7628503832966 12.6538894046098,47.7627233974636 12.6538405381143,47.7626322861761 12.6535926852375,47.7623934019357 12.6532195229083,47.762193409726 12.6528736017644,47.761907838285 12.6527395751327,47.7618443034589 12.6523448713124,47.761437529698 12.652058461681,47.7612014114857 12.6519458089024,47.7610836457461 12.65187070705,47.7610408142209 12.6515407953411,47.7609388064593 12.6511814631522,47.7607513871044 12.6509833987802,47.7605989202857 12.6508803851902,47.7605508919805 12.6506529841572,47.7605512272567 12.6505763735622,47.7605296019465 12.65047528781,47.760467492044 12.6501792389899,47.76034059003 12.6499707810581,47.7602895442396 12.6499090064317,47.7603135164827 12.649806747213,47.76042801328 12.6497490797192,47.7605244889855 12.6497494988143,47.7606241498142 12.6496687810868,47.7607128303498 12.6494708005339,47.7607543207705 12.6493832934648,47.760924724862 12.6492765918374,47.7609595097601 12.649188246578,47.7610101364553 12.6491138152778,47.7610903512686 12.6490128133446,47.761112479493 12.6489218696952,47.7611092943698 12.6488822232932,47.7611316740513 12.6488960534334,47.7611952926964 12.6488348655403,47.7610879205167 12.648863364011,47.7609862480313 12.648532865569,47.7608970645815 12.648947769776,47.761212810874 12.6489218696952,47.7611918561161 12.648939974606,47.7611887548119 12.6490760128945,47.7612767647952 12.6490600034595,47.7614128869027 12.6491536293179,47.7614655252546 12.6491605862975,47.761588236317 12.6491097919643,47.7617998793721 12.6490900944918,47.7617644239217 12.649064194411,47.7617702074349 12.6489797886461,47.7619482390583 12.6489374600351,47.7619864605367 12.6486850809306,47.7620353270322 12.6484067179263,47.7621993608773 12.6482623815536,47.7622533403337 12.648036070168,47.7622480597347 12.6478475611657,47.7622603811324 12.6475449744612,47.7622178848833 12.6470262184739,47.762291142717 12.6469034235924,47.7622908912599 12.6467973086983,47.7623594552279 12.6464832387865,47.7624108362943 12.6463470328599,47.7624897100031 12.6461959071457,47.7626309450716 12.6459871139377,47.7626568451524 12.6457802485675,47.7626412548125 12.645607246086,47.76267176494 12.6455499138683,47.762745777145 12.6454372610897,47.7627601940185 12.645099721849,47.7627425082028 12.6449706405401,47.7627686597407 12.644922779873,47.7627199608833 12.6449436508119,47.762684840709 12.6448880787939,47.7627077233046 12.6448597479612,47.7626396622509 12.6448376197368,47.7626353036612 12.6448824629188,47.7626270893961 12.6449346821755,47.7626629639417 12.6449692994356,47.7627731859684 12.6450901664793,47.7627581823617 12.6454598922282,47.7628386486322 12.6455090939999,47.7628078870475 12.6458431966603,47.7629272453487 12.6459612138569,47.7630917821079 12.6459255069494,47.7632212825119 12.6458329707384,47.7633180934936 12.645715540275,47.763385232538 12.6456903945655,47.7634395472705 12.6456331461668,47.7634770981967 12.6455409452319,47.7634796965867 12.6448586583138,47.7605314459652 12.644806355238,47.7604309469461 12.6447993144393,47.7602425217628 12.6446607615799,47.7599153760821 12.6446264795959,47.7598723769188 12.6446345262229,47.7598308864981 12.6445640344173,47.7597251068801 12.6444445922971,47.7596841193736 12.6442522276193,47.7595387771726 12.6441777125001,47.7594600711018 12.6440641377121,47.7592561393976 12.6438694261014,47.7589979767799 12.6438320428133,47.7588440850377 12.6437685079873,47.7587722521275 12.6436961721629,47.7587659657001 12.6436247583479,47.7587851602584 12.6436263509095,47.7588077075779 12.6437078230083,47.7588137425482 12.6435323897749,47.7587489504367 12.6434621494263,47.7587043587118 12.6433592196554,47.7585430070758</coordinates>
         </LineString>
      </MultiGeometry>
   </Placemark>
</Folder>
<Folder>
   <name>waypoints</name>
   <Placemark>
      <name>Hotel Hirschhaus</name>
      <ExtendedData>
         <SchemaData schemaUrl="#waypoints">
            <SimpleData name="ele">681</SimpleData>
            <SimpleData name="time">2020/09/16 15:57:52+00</SimpleData>
            <SimpleData name="cmt">38a</SimpleData>
            <SimpleData name="desc">38a</SimpleData>
            <SimpleData name="sym">Lodging</SimpleData>
            <SimpleData name="gpxx_WaypointExtension">&lt;gpxx:DisplayMode&gt;SymbolAndName&lt;/gpxx:DisplayMode&gt;   </SimpleData>
         </SchemaData>
      </ExtendedData>
      <Point>
         <coordinates>12.6435506623238,47.7586564142257</coordinates>
      </Point>
   </Placemark>
</Folder>
<Schema name="waypoints" id="waypoints">
   <SimpleField name="ele" type="float"/>
   <SimpleField name="time" type="string"/>
   <SimpleField name="magvar" type="float"/>
   <SimpleField name="geoidheight" type="float"/>
   <SimpleField name="cmt" type="string"/>
   <SimpleField name="desc" type="string"/>
   <SimpleField name="src" type="string"/>
   <SimpleField name="link1_href" type="string"/>
   <SimpleField name="link1_text" type="string"/>
   <SimpleField name="link1_type" type="string"/>
   <SimpleField name="link2_href" type="string"/>
   <SimpleField name="link2_text" type="string"/>
   <SimpleField name="link2_type" type="string"/>
   <SimpleField name="sym" type="string"/>
   <SimpleField name="type" type="string"/>
   <SimpleField name="fix" type="string"/>
   <SimpleField name="sat" type="int"/>
   <SimpleField name="hdop" type="float"/>
   <SimpleField name="vdop" type="float"/>
   <SimpleField name="pdop" type="float"/>
   <SimpleField name="ageofdgpsdata" type="float"/>
   <SimpleField name="dgpsid" type="int"/>
   <SimpleField name="gpxx_WaypointExtension" type="string"/>
</Schema>
</Document>
</kml>
      */

      /*
von GoogleEarth:
<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2" xmlns:gx="http://www.google.com/kml/ext/2.2" xmlns:kml="http://www.opengis.net/kml/2.2" xmlns:atom="http://www.w3.org/2005/Atom">
<Folder>
	<name>Temporäre Orte</name>
	<open>1</open>

        ==> für Lesen: Folder mit name="tracks" und name="waypoints" suchen und dann deren Placemarks verwenden


	<Document id="root_doc">
		<name>test1.kmz</name>
		<Schema name="waypoints" id="waypoints">
			<SimpleField type="float" name="ele"></SimpleField>
			<SimpleField type="string" name="time"></SimpleField>
			<SimpleField type="float" name="magvar"></SimpleField>
			<SimpleField type="float" name="geoidheight"></SimpleField>
			<SimpleField type="string" name="cmt"></SimpleField>
			<SimpleField type="string" name="desc"></SimpleField>
			<SimpleField type="string" name="src"></SimpleField>
			<SimpleField type="string" name="link1_href"></SimpleField>
			<SimpleField type="string" name="link1_text"></SimpleField>
			<SimpleField type="string" name="link1_type"></SimpleField>
			<SimpleField type="string" name="link2_href"></SimpleField>
			<SimpleField type="string" name="link2_text"></SimpleField>
			<SimpleField type="string" name="link2_type"></SimpleField>
			<SimpleField type="string" name="sym"></SimpleField>
			<SimpleField type="string" name="type"></SimpleField>
			<SimpleField type="string" name="fix"></SimpleField>
			<SimpleField type="int" name="sat"></SimpleField>
			<SimpleField type="float" name="hdop"></SimpleField>
			<SimpleField type="float" name="vdop"></SimpleField>
			<SimpleField type="float" name="pdop"></SimpleField>
			<SimpleField type="float" name="ageofdgpsdata"></SimpleField>
			<SimpleField type="int" name="dgpsid"></SimpleField>
			<SimpleField type="string" name="gpxx_WaypointExtension"></SimpleField>
		</Schema>
		<Folder>
			<name>tracks</name>
			<Placemark>
				<name>W 20200909, Ruhpolding, 7,7km</name>
				<Style>
					<LineStyle>
						<color>cc0055ff</color>
						<width>4</width>
					</LineStyle>
					<PolyStyle>
						<fill>0</fill>
					</PolyStyle>
				</Style>
				<MultiGeometry>
					<LineString>
						<coordinates>
							12.6433829404414,47.7585958968848,0 12.6434474810958,47.7587980683893,0 12.6438846811652,47.7587441727519,0 12.6436768099666,47.75792668573559,0 12.6434833556414,47.7570507768542,0 12.6430174056441,47.7554473187774,0 12.6427808683366,47.7552184928209,0 12.6423006691039,47.7546839788556,0 12.6412432920188,47.753268359229,0 12.6412628218532,47.7531868033111,0 12.6410584710538,47.7528283931315,0 12.6412214152515,47.7529631741345,0 12.6408706326038,47.75271255522971,0 12.6405179221183,47.75236520916219,0 12.6404211111367,47.7522378880531,0 12.6403354480863,47.751941671595,0 12.640329496935,47.7518570143729,0 12.6404750067741,47.7518277615309,0 12.6408428885043,47.7518417593092,0 12.6411820203066,47.7517873607576,0 12.6413078326732,47.7517728600651,0 12.6413505803794,47.75178082287311,0 12.6413750555366,47.7517605386674,0 12.6415212359279,47.7517426013947,0 12.6418765448034,47.7517640590668,0 12.6420889422297,47.75175483897329,0 12.6425604242831,47.75179448537529,0 12.6428205147386,47.751939157024,0 12.6429329998791,47.7519389055669,0 12.6430459879339,47.7520330343395,0 12.6431420445442,47.7521474473178,0 12.6431687828153,47.7522818092257,0 12.6431153900921,47.75246478617191,0 12.643117653206,47.7526487689465,0 12.6429291442037,47.7530354261398,0 12.64289310202,47.7534027211368,0 12.6429056748748,47.75352509692311,0 12.6429418008775,47.753746798262,0 12.6430398691446,47.753894906491,0 12.6431541983038,47.754029519856,0 12.6432530209422,47.75409238412981,0 12.6433703675866,47.7542603574693,0 12.6434385124594,47.7543129120022,0 12.6434651669115,47.7543163485825,0 12.6439419295639,47.75473896414041,0 12.6441737730056,47.7549085300416,0 12.6443772017956,47.75508429855109,0 12.6448051817715,47.7553739771247,0 12.6452326588333,47.7556163817644,0 12.6456632371992,47.7557935751975,0 12.6456978544593,47.7557824272662,0 12.6458648219705,47.7555762324482,0 12.645977223292,47.7555182296783,0 12.6460942346603,47.7555323112756,0 12.6463238988072,47.7556537650526,0 12.6465449295938,47.7557292021811,0 12.6466492842883,47.7557993587106,0 12.6468948740512,47.7558834291995,0 12.6470994763076,47.7559831738472,0 12.6474180724472,47.7560965809971,0 12.6475241873413,47.7561198826879,0 12.6478480640799,47.7562597766519,0 12.6479255966842,47.7563523128629,0 12.6480799913406,47.7564454358071,0 12.6482311170548,47.7565817255527,0 12.6483021955937,47.7567382995039,0 12.6482908800244,47.7568356972188,0 12.648308146745,47.7568702306598,0 12.6485325302929,47.7570574823767,0 12.6486260723323,47.75718589313329,0 12.6487122382969,47.7572640124708,0 12.6488073728979,47.75754857808351,0 12.6489120628685,47.7577370870858,0 12.6489997375757,47.758052079007,0 12.6490304153413,47.7580825053155,0 12.6490729115903,47.75807680562139,0 12.649016501382,47.75807093828921,0 12.6488911081105,47.7580236643553,0 12.6489489432424,47.7580825891346,0 12.6490154955536,47.7582570165396,0 12.648958414793,47.75855365209281,0 12.6489552296698,47.7588212862611,0 12.6489623542875,47.7589482720941,0 12.6490340195596,47.7591973822564,0 12.6491466723382,47.7594012301415,0 12.649288745597,47.7595660183579,0 12.649418246001,47.75965419597919,0 12.6495762448758,47.7597294654697,0 12.6496448088437,47.7597885578871,0 12.649748492986,47.75983767583971,0 12.6498452201486,47.75990573689341,0 12.6498796697706,47.7599646616727,0 12.6499219145626,47.7599739655852,0 12.6499737147242,47.7600125223398,0 12.6499701105058,47.760057868436,0 12.6498880516738,47.7601735386997,0 12.6500747166574,47.760322149843,0 12.6502567715943,47.7603482175618,0 12.6503393333405,47.7604106627405,0 12.6504438556731,47.7604316174984,0 12.6506554987282,47.7604234032333,0 12.6507989130914,47.7604395803064,0 12.651191605255,47.7606166899204,0 12.651337031275,47.7607177756727,0 12.6514843851328,47.7607592660934,0 12.6516701281071,47.7608441747725,0 12.6518293004483,47.7609373815358,0 12.6521252654493,47.7612218633294,0 12.6523296162486,47.7613826282322,0 12.6524704322219,47.7615398727357,0 12.6525397505611,47.7615521103144,0 12.652880307287,47.7615043334663,0 12.6531808823347,47.7614853903651,0 12.6533113047481,47.7615336701274,0 12.6536120474339,47.76147466152911,0 12.6537635084242,47.7614740747958,0 12.6541660912335,47.7615191694349,0 12.6542142871767,47.7615370228887,0 12.654526764527,47.7615796867758,0 12.6547157764435,47.7615721430629,0 12.6549579296261,47.7615188341588,0 12.6551481150091,47.7614336740226,0 12.6553262304515,47.76126997545361,0 12.6556371152401,47.7611780259758,0 12.6558412984014,47.7611456718296,0 12.6560587249696,47.761210128665,0 12.6562601421028,47.76124315336351,0 12.6562746427953,47.76122496463359,0 12.6563470624387,47.7612825483084,0 12.6564174704254,47.7613009884953,0 12.6565025467426,47.7613684628159,0 12.6564859505743,47.76141598820691,0 12.6564158778638,47.7614698000252,0 12.6563529297709,47.7615622524172,0 12.6561049092561,47.76174983941019,0 12.6559375226498,47.76191655546431,0 12.6558875665069,47.7620016317815,0 12.6557990536094,47.7620810084045,0 12.6555713173002,47.7621781546623,0 12.6554724946618,47.7622427791357,0 12.6553898490965,47.7623100019991,0 12.6553523819894,47.7623799070716,0 12.6551632024348,47.7625872753561,0 12.6550628710538,47.76291752234101,0 12.655076701194,47.76322983205319,0 12.6550067123026,47.7633962966502,0 12.6549209654331,47.7639776654541,0 12.6548358052969,47.7643167134374,0 12.6546908821911,47.7646679151803,0 12.6546161156148,47.7649907860905,0 12.6544837653637,47.765103019774,0 12.6543582044542,47.76518046855931,0 12.6540676876903,47.7654296625406,0 12.6538127940148,47.765771895647,0 12.6534781884402,47.76604916900401,0 12.6530080474913,47.7663489058614,0 12.652872512117,47.76645736768841,0 12.6526934746653,47.7665494009852,0 12.6523916423321,47.76676054112609,0 12.6522009540349,47.7668576035649,0 12.6520154625177,47.7671454381198,0 12.6518953498453,47.7672719210386,0 12.651733411476,47.7673989906907,0 12.6516041625291,47.7675586659461,0 12.6514771766961,47.7677960414439,0 12.6512279827148,47.7679842989892,0 12.6511147432029,47.7680034935474,0 12.6509849075228,47.7680679503828,0 12.6508749369532,47.768192589283,0 12.6507574226707,47.7682915795594,0 12.6507315225899,47.7684997860342,0 12.6506572589278,47.7687319647521,0 12.6503790635616,47.76912113651629,0 12.6503239106387,47.7691664826125,0 12.6503345556557,47.76934501715,0 12.6502623874694,47.7694907784462,0 12.6501814182848,47.7699011564255,0 12.6499744690955,47.7703689504415,0 12.6498106867075,47.7705307211727,0 12.6497582998127,47.7705444674939,0 12.6494982093573,47.7708525862545,0 12.6494073495269,47.7708971779794,0 12.64927550219,47.7712156902999,0 12.6490896753967,47.7714525628835,0 12.6488249748945,47.77166990563269,0 12.6487173512578,47.7717390563339,0 12.648654570803,47.7717582508922,0 12.648355755955,47.7719520404935,0 12.6481969188899,47.7720957901329,0 12.6481638103724,47.7721107937396,0 12.6479970104992,47.7721163257957,0 12.6477024704218,47.77221389114861,0 12.6476484071463,47.7722616679966,0 12.6475460641086,47.7725069224834,0 12.6475132070482,47.7725419588387,0 12.6473196689039,47.7725852932781,0 12.6473024021834,47.7726059127599,0 12.6471021585166,47.7724391967058,0 12.6469027530402,47.7723659388721,0 12.6469015795738,47.7723440621048,0 12.6469263061881,47.7723871450871,0 12.6468937844038,47.7724045794457,0 12.6468682195991,47.7723687887192,0 12.6468000747263,47.7723687887192,0 12.6466851588339,47.7722839638591,0 12.646616930142,47.7721880748868,0 12.646563872695,47.77196863666181,0 12.6466264016926,47.7718353644013,0 12.6467644516379,47.7717562392354,0 12.6470558904111,47.771535879001,0 12.6471376139671,47.771501513198,0 12.6473549567163,47.7715108171105,0 12.6475068368018,47.7713606134057,0 12.6475743949413,47.7711212262511,0 12.6476556155831,47.7709938213229,0 12.6477073319256,47.7708262670785,0 12.6479031331837,47.77037397958339,0 12.6480428595096,47.7702334988862,0 12.6481782272458,47.7701398730278,0 12.6485286746174,47.76994231157,0 12.648694133386,47.7697957120836,0 12.6489654555917,47.7696221228689,0 12.6491848938167,47.76934149675071,0 12.6492509432137,47.76918584480881,0 12.6493185013533,47.76910127140581,0 12.6495008077472,47.7690080646425,0 12.6500197313726,47.7683001291007,0 12.6501459628344,47.7679673675448,0 12.6501848548651,47.7676512859762,0 12.6502998545766,47.76752103120089,0 12.6506157685071,47.7673004195094,0 12.6508926227689,47.7670543268323,0 12.6509788725525,47.76700403541331,0 12.6511743385345,47.76684427633881,0 12.65127341263,47.76668627746399,0 12.6514596585184,47.7665413543582,0 12.6516345888376,47.76642015203829,0 12.6521473936737,47.7662005461752,0 12.6522886287421,47.7661052439362,0 12.6523911394179,47.7660105284303,0 12.652494572103,47.76576837524771,0 12.6526511460543,47.76564130559559,0 12.6528449356556,47.765539214015,0 12.6530260685831,47.7653687261045,0 12.653456479311,47.76506555266679,0 12.6534794457257,47.7650091424584,0 12.6535402145237,47.76498014107351,0 12.6537614129484,47.7649321127683,0 12.6539238542318,47.76487897150219,0 12.6539723854512,47.76483505032961,0 12.6539788395166,47.7646863553673,0 12.6539393607527,47.76438536122441,0 12.653736518696,47.7638276293874,0 12.653884543106,47.7636418864131,0 12.6539202500135,47.7634220290929,0 12.6539787556976,47.7633130643517,0 12.6540624909103,47.7633157465607,0 12.6539997942746,47.7632934506983,0 12.6539725530893,47.76320334523921,0 12.6540820207447,47.76300452649589,0 12.6539706252515,47.7628915384412,0 12.6538835372776,47.7628503832966,0 12.6538894046098,47.7627233974636,0 12.6538405381143,47.76263228617609,0 12.6535926852375,47.7623934019357,0 12.6532195229083,47.762193409726,0 12.6528736017644,47.761907838285,0 12.6527395751327,47.7618443034589,0 12.6523448713124,47.76143752969801,0 12.652058461681,47.7612014114857,0 12.6519458089024,47.7610836457461,0 12.65187070705,47.7610408142209,0 12.6515407953411,47.7609388064593,0 12.6511814631522,47.7607513871044,0 12.6509833987802,47.7605989202857,0 12.6508803851902,47.7605508919805,0 12.6506529841572,47.7605512272567,0 12.6505763735622,47.7605296019465,0 12.65047528781,47.760467492044,0 12.6501792389899,47.76034059003,0 12.6499707810581,47.7602895442396,0 12.6499090064317,47.7603135164827,0 12.649806747213,47.76042801327999,0 12.6497490797192,47.7605244889855,0 12.6497494988143,47.7606241498142,0 12.6496687810868,47.7607128303498,0 12.6494708005339,47.7607543207705,0 12.6493832934648,47.760924724862,0 12.6492765918374,47.7609595097601,0 12.649188246578,47.7610101364553,0 12.6491138152778,47.7610903512686,0 12.6490128133446,47.761112479493,0 12.6489218696952,47.76110929436981,0 12.6488822232932,47.76113167405129,0 12.6488960534334,47.7611952926964,0 12.6488348655403,47.7610879205167,0 12.648863364011,47.7609862480313,0 12.648532865569,47.76089706458151,0 12.648947769776,47.76121281087399,0 12.6489218696952,47.7611918561161,0 12.648939974606,47.76118875481189,0 12.6490760128945,47.76127676479519,0 12.6490600034595,47.7614128869027,0 12.6491536293179,47.7614655252546,0 12.6491605862975,47.761588236317,0 12.6491097919643,47.7617998793721,0 12.6490900944918,47.7617644239217,0 12.649064194411,47.7617702074349,0 12.6489797886461,47.7619482390583,0 12.6489374600351,47.7619864605367,0 12.6486850809306,47.7620353270322,0 12.6484067179263,47.7621993608773,0 12.6482623815536,47.7622533403337,0 12.648036070168,47.7622480597347,0 12.6478475611657,47.76226038113241,0 12.6475449744612,47.76221788488329,0 12.6470262184739,47.762291142717,0 12.6469034235924,47.7622908912599,0 12.6467973086983,47.7623594552279,0 12.6464832387865,47.7624108362943,0 12.6463470328599,47.76248971000309,0 12.6461959071457,47.7626309450716,0 12.6459871139377,47.7626568451524,0 12.6457802485675,47.7626412548125,0 12.645607246086,47.76267176494,0 12.6455499138683,47.762745777145,0 12.6454372610897,47.76276019401849,0 12.645099721849,47.7627425082028,0 12.6449706405401,47.76276865974069,0 12.644922779873,47.7627199608833,0 12.6449436508119,47.762684840709,0 12.6448880787939,47.7627077233046,0 12.6448597479612,47.7626396622509,0 12.6448376197368,47.7626353036612,0 12.6448824629188,47.7626270893961,0 12.6449346821755,47.7626629639417,0 12.6449692994356,47.7627731859684,0 12.6450901664793,47.7627581823617,0 12.6454598922282,47.7628386486322,0 12.6455090939999,47.7628078870475,0 12.6458431966603,47.7629272453487,0 12.6459612138569,47.7630917821079,0 12.6459255069494,47.7632212825119,0 12.6458329707384,47.7633180934936,0 12.645715540275,47.76338523253799,0 12.6456903945655,47.76343954727049,0 12.6456331461668,47.7634770981967,0 12.6455409452319,47.7634796965867,0 12.6448586583138,47.7605314459652,0 12.644806355238,47.7604309469461,0 12.6447993144393,47.7602425217628,0 12.6446607615799,47.75991537608211,0 12.6446264795959,47.7598723769188,0 12.6446345262229,47.7598308864981,0 12.6445640344173,47.7597251068801,0 12.6444445922971,47.7596841193736,0 12.6442522276193,47.75953877717261,0 12.6441777125001,47.75946007110179,0 12.6440641377121,47.7592561393976,0 12.6438694261014,47.7589979767799,0 12.6438320428133,47.7588440850377,0 12.6437685079873,47.7587722521275,0 12.6436961721629,47.7587659657001,0 12.6436247583479,47.75878516025841,0 12.6436263509095,47.7588077075779,0 12.6437078230083,47.7588137425482,0 12.6435323897749,47.7587489504367,0 12.6434621494263,47.7587043587118,0 12.6433592196554,47.75854300707579,0 
						</coordinates>
					</LineString>
				</MultiGeometry>
			</Placemark>
		</Folder>
		<Folder>
			<name>waypoints</name>
			<Placemark>
				<name>Hotel Hirschhaus</name>
				<ExtendedData>
					<SchemaData schemaUrl="#waypoints">
						<SimpleData name="ele">681</SimpleData>
						<SimpleData name="time">2020/09/16 15:57:52+00</SimpleData>
						<SimpleData name="cmt">38a</SimpleData>
						<SimpleData name="desc">38a</SimpleData>
						<SimpleData name="sym">Lodging</SimpleData>
						<SimpleData name="gpxx_WaypointExtension"><![CDATA[<gpxx:DisplayMode>SymbolAndName</gpxx:DisplayMode>]]></SimpleData>
					</SchemaData>
				</ExtendedData>
				<Point>
					<coordinates>12.6435506623238,47.7586564142257,0</coordinates>
				</Point>
			</Placemark>
		</Folder>
	</Document>
</Folder>
</kml>
        
        
       */

      /*
<kml>
   <Document>

        ==> für Lesen: Placemark suchen mit LineString oder Point


      <Placemark>
         <description></description>
         <name>2020-09-10 15:39:51</name>
         <LineString>
            <coordinates>12.6438696776,47.7585162688,659.94 12.6438788977,47.7585053723,659.94 
      
            </coordinates>
         </LineString>
      </Placemark>
      <Placemark>
         <description></description>
         <name>2020-09-11 15:45:49</name>
         <LineString>
            <coordinates>12.6440743636,47.7587717492,747.47 12.6440573484,47.7587192785,747.31 12.6440373156,47.7586430032,747.24        

         </LineString>
      </Placemark>
   </Document>
</kml>       
       */

      #endregion
   }
}
