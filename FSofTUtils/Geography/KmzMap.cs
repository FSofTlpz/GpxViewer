using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography {

   /// <summary>
   /// zum Lesen einer KMZ-Datei, die georef. Bilder als Karte enthält
   /// </summary>
   public class KmzMap : IDisposable {

      public string? Name { get; protected set; }

      public readonly double North, South, East, West;


      class Tile {
         public string? Path;
         public double North, South, East, West;

         public double DeltaLon => East - West;

         public double DeltaLat => North - South;

         /// <summary>
         /// Ex. eine Schnittmenge?
         /// </summary>
         /// <param name="bound"></param>
         /// <returns></returns>
         public Tile? Intersection(Tile bound) {
            double l1 = West;
            double r1 = East;
            if (r1 < l1)
               r1 += 360;
            double l2 = bound.West;
            double r2 = bound.East;
            if (r2 < l2)
               r2 += 360;

            double l = -360;
            if (l1 <= l2 && l2 <= r1)
               l = l2;
            else if (l2 <= l1 && l1 <= r2)
               l = l1;

            if (l > -360) {
               double r = Math.Min(r1, r2);
               double b1 = South;
               double t1 = North;
               double b2 = bound.South;
               double t2 = bound.North;

               double b = -360;
               if (b1 <= b2 && b2 <= t1)
                  b = b2;
               else if (b2 <= b1 && b1 <= t2)
                  b = b1;

               if (b > -360)
                  return new Tile() {
                     East = l,
                     West = r,
                     South = b,
                     North = Math.Min(t1, t2),
                  };
            }
            return null;
         }

         /// <summary>
         /// liefert den Pixelindex für die geograf. Länge (0 steht für den linken Rand)
         /// </summary>
         /// <param name="width">Pixelanzahl für die gesamte Breite</param>
         /// <param name="lon"></param>
         /// <returns></returns>
         public double Pixel4Lon(int width, double lon) => (lon - West) / (DeltaLon / width);

         /// <summary>
         /// liefert den Pixelindex für die geograf. Breite (0 steht für den unteren Rand)
         /// </summary>
         /// <param name="height">Pixelanzahl für die gesamte Höhe</param>
         /// <param name="lat"></param>
         /// <returns></returns>
         public double Pixel4Lat(int height, double lat) => (lat - South) / (DeltaLat / height);

         public override string ToString() {
            return string.Format("{0}; {1}°..{2}° / {3}°..{4}°",
                                 Path,
                                 West,
                                 East,
                                 South,
                                 North);
         }

      }


      readonly object readlock = new object();

      readonly ZipArchive kmz;

      /// <summary>
      /// Liste der Tiles (Bilder)
      /// </summary>
      readonly List<Tile> tiles = new List<Tile>();

      XmlDocument? kmzDoc;
      XPathNavigator? kmzNavigator;
      XmlNamespaceManager? kmzNsMng;


      public KmzMap(string kmzfile) {
         kmz = ZipFile.OpenRead(kmzfile);
         int direntry = -1;
         for (int i = 0; i < kmz.Entries.Count; i++) {
            ZipArchiveEntry entry = kmz.Entries[i];
            if (Path.GetFileName(entry.Name) == entry.Name &&
                Path.GetExtension(entry.Name) == ".kml") {        // 1. KML-Datei im Wurzelverzeichnis
               direntry = i;

               kmzDoc = new XmlDocument();
               kmzDoc.Load(kmz.Entries[i].Open());
               kmzNavigator = kmzDoc.CreateNavigator();
               kmzNsMng = new XmlNamespaceManager(kmzDoc.NameTable);
               kmzNsMng.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

               /*
               <?xml version="1.0" encoding="UTF-8"?>
               <kml xmlns="http://www.opengis.net/kml/2.2" xmlns:gx="http://www.google.com/kml/ext/2.2" xmlns:kml="http://www.opengis.net/kml/2.2" xmlns:atom="http://www.w3.org/2005/Atom">
               <Document>
                  <Name>Spreewald</Name>
                  <GroundOverlay id="1">
                     <name>tiles</name>
                     <Icon>
                        <href>tiles/tile0001.jpg</href>
                     </Icon>
                     <LatLonBox>
                        <north>51.9168312</north>
                        <south>51.8692251</south>
                        <east>13.9921679</east>
                        <west>13.9354338</west>
                     </LatLonBox>
                  </GroundOverlay>
                */

               Name = readValue("/kml:kml/kml:Document/kml:Name", "");

               string[]? imgpaths = readString("/kml:kml/kml:Document/kml:GroundOverlay/kml:Icon/kml:href");
               if (imgpaths != null)
                  for (int j = 0; j < imgpaths.Length; j++) {
                     tiles.Add(new Tile() {
                        Path = imgpaths[j],
                        North = readValue("/kml:kml/kml:Document/kml:GroundOverlay[" + (j + 1).ToString() + "]/kml:LatLonBox/kml:north", 0.0),
                        South = readValue("/kml:kml/kml:Document/kml:GroundOverlay[" + (j + 1).ToString() + "]/kml:LatLonBox/kml:south", 0.0),
                        East = readValue("/kml:kml/kml:Document/kml:GroundOverlay[" + (j + 1).ToString() + "]/kml:LatLonBox/kml:east", 0.0),
                        West = readValue("/kml:kml/kml:Document/kml:GroundOverlay[" + (j + 1).ToString() + "]/kml:LatLonBox/kml:west", 0.0),
                     });

                     North = Math.Max(North, tiles[tiles.Count - 1].North);
                     South = Math.Min(South, tiles[tiles.Count - 1].South);
                     West = Math.Max(West, tiles[tiles.Count - 1].West);
                     East = Math.Min(East, tiles[tiles.Count - 1].East);
                  }
               break;
            }
         }
         if (direntry < 0)
            throw new Exception("Keine Verzeichnis-KML-Datei vorhanden.");
      }

      #region XML-Daten lesen

      /// <summary>
      /// liefert die Daten entsprechend 'xpath' als Object-Array
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      object[]? readValueAsObject(string xpath) {
         object[]? ret = null;
         try {
            if (kmzNavigator != null) {
               XPathNodeIterator nodes = kmzNavigator.Select(xpath, kmzNsMng);
               if (nodes.Count == 0)
                  return null;
               ret = new object[nodes.Count];
               int i = 0;
               while (nodes.MoveNext() && nodes.Current != null)
                  ret[i++] = nodes.Current.TypedValue;
            }
         } catch { }
         return ret;
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      string readValue(string xpath, string defvalue) {
         object[]? o = readValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return defvalue;
#pragma warning disable CS8603 // Mögliche Nullverweisrückgabe.
         return o[0].ToString();
#pragma warning restore CS8603 // Mögliche Nullverweisrückgabe.
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      double readValue(string xpath, double defvalue) {
         double ret = defvalue;
         object[]? o = readValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            if (o[0].GetType() == Type.GetType("System.String"))     // Mono erkennt z.Z. NICHT den Typ Double --> Umwandlung aus String
               ret = Convert.ToDouble(o[0], System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            else
               ret = Convert.ToDouble(o[0]);
         } catch { }
         return ret;
      }

      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <returns></returns>
      string[]? readString(string xpath) {
         string[]? ret = null;
         object[]? o = readValueAsObject(xpath);
         if (o != null) {
            ret = new string[o.Length];
            for (int i = 0; i < o.Length; i++)
#pragma warning disable CS8601 // Mögliche Nullverweiszuweisung.
               ret[i] = o[i].ToString();
#pragma warning restore CS8601 // Mögliche Nullverweiszuweisung.
         }
         return ret;
      }

      #endregion

      public Bitmap GetImage(double west, double east, double south, double north, int width, int height) {
         Bitmap bm = new Bitmap(width, height);

         Graphics graphics = Graphics.FromImage(bm);
         graphics.Clear(Color.White);

         Tile desttile = new Tile() {
            Path = "",
            North = north,
            South = south,
            East = east,
            West = west,
         };

         for (int i = 0; i < tiles.Count; i++) { // entsprechend der Reihenfolge der GroundOverlay's
            Tile? intersect = desttile.Intersection(tiles[i]);
            if (intersect != null) {
               lock (readlock) {    // MIST: Das Lesen ist NICHT threadsafe!
                  using (Stream stream = kmz.Entries[i].Open()) {
                     using (Bitmap kmzbm = new Bitmap(stream)) {
                        if (kmzbm != null) {
                           double srcleft = tiles[i].Pixel4Lon(kmzbm.Width, intersect.East);
                           double srcright = tiles[i].Pixel4Lon(kmzbm.Width, intersect.West);
                           double srcbottom = tiles[i].Pixel4Lat(kmzbm.Height, intersect.South);
                           double srctop = tiles[i].Pixel4Lat(kmzbm.Height, intersect.North);

                           double dstleft = desttile.Pixel4Lon(bm.Width, intersect.East);
                           double dstright = desttile.Pixel4Lon(bm.Width, intersect.West);
                           double dstbottom = desttile.Pixel4Lat(bm.Height, intersect.South);
                           double dsttop = desttile.Pixel4Lat(bm.Height, intersect.North);

                           RectangleF srcRect = new RectangleF((float)Math.Round(srcleft),
                                                               kmzbm.Height - (float)Math.Round(srctop),
                                                               (float)Math.Round(srcright - srcleft),
                                                               (float)Math.Round(srctop - srcbottom));

                           RectangleF dstRect = new RectangleF((float)Math.Round(dstleft),
                                                               bm.Height - (float)Math.Round(dsttop),
                                                               (float)Math.Round(dstright - dstleft),
                                                               (float)Math.Round(dsttop - dstbottom));

                           graphics.DrawImage(kmzbm, dstRect, srcRect, GraphicsUnit.Pixel);
                        }
                     }
                  }
               }
            }
         }

         return bm;
      }

      public override string ToString() {
         return string.Format("{0} Tiles; {1}°..{2}° / {3}°..{4}°",
                              tiles.Count,
                              East,
                              West,
                              South,
                              North);
      }

      ~KmzMap() {
         Dispose(false);
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               if (kmz != null)
                  kmz.Dispose();
               if (tiles != null)
                  tiles.Clear();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
