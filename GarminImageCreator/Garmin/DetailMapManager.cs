//#define GETDATASEQUENTIEL           // Daten NACHEINANDER ausliefern (ohne Multithreading einfacher zu debuggen)

using GarminCore;
using GarminImageCreator.Garmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GarminImageCreator {
   /// <summary>
   /// für alle Tilemaps einer gesamten Garmin-Karte
   /// </summary>
   public class DetailMapManager : IDisposable {

      /// <summary>
      /// Kartenverzeichnis
      /// </summary>
      protected readonly string? mapDirectory;

      OptimizedGeoDataReader geoDataReader;


      /// <summary>
      /// verwaltet die Daten für eine Garmin-Karte
      /// <para>Mit den Caches erhöht sich die Lesegeschwindigkeit aber auch der Hauptspeicherbedarf.</para>
      /// </summary>
      /// <param name="tdbfilename">Pfad der Garmin-TDB-Datei</param>
      /// <param name="cachepath">Pfad für einen Datencache</param>
      /// <param name="validlevels4cachepath">gültige Level für den Cache im Dateisystem</param>
      /// <param name="subdivcachesize">Größe für den Cache der Subdivs im Hauptspeicher</param>
      /// <param name="tilemapreadercachesize">Größe für den Cache der Tilemap-Reader im Hauptspeicher</param>
      /// <param name="maxsubdiv">max. Anzahl Subdivs je Bild (unbegrenzt wenn kleiner 1)</param>
      public DetailMapManager(string tdbfilename) {
         mapDirectory = Path.GetDirectoryName(tdbfilename);
         geoDataReader = new OptimizedGeoDataReader(mapDirectory != null ? mapDirectory : ".", Path.GetFileName(tdbfilename));
      }

      /// <summary>
      /// aus der Auflösung auf die min. notwendige Bitanzahl schließen (max. 24, real. min. 10?)
      /// </summary>
      /// <param name="groundresolution">Meter je Bildschirmpixel</param>
      /// <returns></returns>
      static int getDesiredBits4Resolution(double groundresolution) {
         /*
          * 1 << (24 - coordbits)        
          * ---------------------- * 360.0
          *        1 << 24                
          *        
          *  1 Garmin-RawUnit steht für:
          *  
          *  bits          Grad                       m am Äquator (Umfang 40075,017km)
          *  24            0,000021457672119140625       2,388657152652740478515625
          *  23            0,00004291534423828125        4,77731430530548095703125
          *  22            0,0000858306884765625         9,5546286106109619140625
          *  21            0,000171661376953125         19,109257221221923828125
          *  20            0,00034332275390625          38,21851444244384765625
          *  19            0,0006866455078125           76,4370288848876953125
          *  18            0,001373291015625           152,874057769775390625
          *  17            0,00274658203125            305,74811553955078125
          *  16            0,0054931640625             611,4962310791015625
          *  15            0,010986328125             1222,992462158203125
          *  14            0,02197265625              2445,98492431640625
          *  13            0,0439453125               4891,9698486328125
          *  12            0,087890625                9783,939697265625
          *  11            0,17578125                19567,87939453125
          *  10            0,3515625                 39135,7587890625
          *  
          *  Bei 24 Bit können Punkte also nur in einem Raster von etwa 239 cm dargestellt werden.
          */
         //if (groundresolution < 10) return 24;
         //else if (groundresolution < 20) return 23;
         //else if (groundresolution < 40) return 22;
         //else if (groundresolution < 80) return 21;
         //else if (groundresolution < 160) return 20;
         //else if (groundresolution < 320) return 19;
         //else if (groundresolution < 640) return 18;
         //else if (groundresolution < 1280) return 17;
         //else if (groundresolution < 2560) return 16;
         //else if (groundresolution < 5120) return 15;
         //else if (groundresolution < 10240) return 14;
         //else if (groundresolution < 20480) return 13;
         //else if (groundresolution < 40960) return 12;
         //else if (groundresolution < 81920) return 11;

         //if (groundresolution < 5) return 24;
         //else if (groundresolution < 10) return 23;
         //else if (groundresolution < 20) return 22;
         //else if (groundresolution < 40) return 21;
         //else if (groundresolution < 80) return 20;
         //else if (groundresolution < 160) return 19;
         //else if (groundresolution < 320) return 18;
         //else if (groundresolution < 640) return 17;
         //else if (groundresolution < 1280) return 16;
         //else if (groundresolution < 2560) return 15;
         //else if (groundresolution < 5120) return 14;
         //else if (groundresolution < 10240) return 13;
         //else if (groundresolution < 20480) return 12;
         //else if (groundresolution < 40960) return 11;

         if (groundresolution < 2.5) return 24;
         else if (groundresolution < 5) return 23;
         else if (groundresolution < 10) return 22;
         else if (groundresolution < 20) return 21;
         else if (groundresolution < 40) return 20;
         else if (groundresolution < 80) return 19;
         else if (groundresolution < 160) return 18;
         else if (groundresolution < 320) return 17;
         else if (groundresolution < 640) return 16;
         else if (groundresolution < 1280) return 15;
         else if (groundresolution < 2560) return 14;
         else if (groundresolution < 5120) return 13;
         else if (groundresolution < 10240) return 12;
         else if (groundresolution < 20480) return 11;


         return 10;
      }

      /// <summary>
      /// liefert die geogr. Breite des Kartenmittelpunktes
      /// </summary>
      /// <returns></returns>
      public double GetMapCenterLat() => geoDataReader.CenterLat;

#if GETDATASEQUENTIEL
      readonly object lock4getdatasequentiel = new object();
#endif

      /// <summary>
      /// liefert alle geogr. Daten deren umgebendes Rechteck den Bereich berühren in nach Typen sortierten Listen
      /// </summary>
      /// <param name="bound"></param>
      /// <param name="groundresolution"></param>
      /// <param name="pointlst"></param>
      /// <param name="linelst"></param>
      /// <param name="arealst"></param>
      /// <param name="cancel">wird true, wenn Abbruch gewünscht</param>
      /// <return></return>
      public bool GetAllData(Bound bound,
                             double groundresolution,
                             out SortedList<int, List<GeoPoint>> pointlst,
                             out SortedList<int, List<GeoPoly>> linelst,
                             out SortedList<int, List<GeoPoly>> arealst,
                             CancellationToken? cancellationToken) {
         bool result = false;
#if GETDATASEQUENTIEL
         lock (lock4getdatasequentiel) {
#endif

         pointlst = new SortedList<int, List<GeoPoint>>();
         linelst = new SortedList<int, List<GeoPoly>>();
         arealst = new SortedList<int, List<GeoPoly>>();

         if (geoDataReader.Read(bound,
                                getDesiredBits4Resolution(groundresolution),
                                out List<GeoPoly> Areas,
                                out List<GeoPoly> Lines,
                                out List<GeoPoint> Points,
                                true,
                                cancellationToken)) {

            // Ergebnislisten füllen
            foreach (var point in Points) {
               if (!pointlst.ContainsKey(point.Type))
                  pointlst.Add(point.Type, new List<GeoPoint>());
               pointlst[point.Type].Add(point);
            }

            foreach (var poly in Areas) {
               if (!arealst.ContainsKey(poly.Type))
                  arealst.Add(poly.Type, new List<GeoPoly>());
               arealst[poly.Type].Add(poly);
            }

            foreach (var poly in Lines) {
               if (!linelst.ContainsKey(poly.Type))
                  linelst.Add(poly.Type, new List<GeoPoly>());
               linelst[poly.Type].Add(poly);
            }

            Areas.Clear();
            Lines.Clear();
            Points.Clear();

            result = true;
         }

#if GETDATASEQUENTIEL
         }
#endif

         //GC.Collect();
         return result;
      }

      public override string ToString() {
         return string.Format("{0}", mapDirectory);
      }

      ~DetailMapManager() {
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
               geoDataReader?.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
