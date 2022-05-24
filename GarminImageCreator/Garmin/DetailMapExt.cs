using System.Collections.Generic;
using GarminCore;
using GarminCore.Files;

namespace GarminImageCreator.Garmin {
   public class DetailMapExt : DetailMap {

      /// <summary>
      /// identifiziert die <see cref="DetailMapExt"/> eindeutig über die Tilemap-Nummer und die Subdiv-Indexnummer
      /// </summary>
      public readonly DetailMapIdentifier ID;


      public DetailMapExt(uint mapnumber,
                          int subdivindex,
                          int level,
                          Bound bound) : 
         base() {
         ID = new DetailMapIdentifier(mapnumber, subdivindex);
         Level = level;
         Bound = new Bound(bound);
      }

      /// <summary>
      /// wird durch Einlesen aus einer Garmindatei erzeugt
      /// </summary>
      /// <param name="mapnumber"></param>
      /// <param name="subdivindex"></param>
      /// <param name="tre"></param>
      /// <param name="lbl"></param>
      /// <param name="rgn"></param>
      /// <param name="net"></param>
      public DetailMapExt(uint mapnumber, 
                          int subdivindex,
                          StdFile_TRE tre,
                          StdFile_LBL lbl,
                          StdFile_RGN rgn,
                          StdFile_NET net) : 
         base(subdivindex, tre, lbl, rgn, net) {
         ID = new DetailMapIdentifier(mapnumber, subdivindex);
      }

      /// <summary>
      /// setzt die Gebietsliste neu
      /// </summary>
      /// <param name="areas"></param>
      public void SetAreas(IList<DetailMap.GeoPoly> areas) {
         Areas = new DetailMap.GeoPoly[areas.Count];
         areas.CopyTo(Areas, 0);
      }
      /// <summary>
      /// setzt die Linienliste neu
      /// </summary>
      /// <param name="lines"></param>
      public void SetLines(IList<DetailMap.GeoPoly> lines) {
         Lines = new DetailMap.GeoPoly[lines.Count];
         lines.CopyTo(Lines, 0);
      }
      /// <summary>
      /// setzt die Punktliste neu
      /// </summary>
      /// <param name="points"></param>
      public void SetPoints(IList<DetailMap.GeoPoint> points) {
         Points = new DetailMap.GeoPoint[points.Count];
         points.CopyTo(Points, 0);
      }

      /// <summary>
      /// fügt die betroffenen Gebiete an die Liste an
      /// </summary>
      /// <param name="list"></param>
      /// <param name="bound"></param>
      public void AddAreas2List(List<DetailMap.GeoPoly> list, Bound bound) {
         foreach (DetailMap.GeoPoly poly in Areas)
            if (poly != null)
               if (bound.IsOverlapped(poly.Bound))
                  list.Add(poly);
      }
      /// <summary>
      /// fügt die betroffenen Linien an die Liste an
      /// </summary>
      /// <param name="list"></param>
      /// <param name="bound"></param>
      public void AddLines2List(List<DetailMap.GeoPoly> list, Bound bound) {
         foreach (DetailMap.GeoPoly poly in Lines)
            if (poly != null)
               if (bound.IsOverlapped(poly.Bound))
                  list.Add(poly);
      }
      /// <summary>
      /// fügt die betroffenen Punkte an die Liste an
      /// </summary>
      /// <param name="list"></param>
      /// <param name="bound"></param>
      public void AddPoints2List(List<DetailMap.GeoPoint> list, Bound bound) {
         foreach (DetailMap.GeoPoint point in Points)
            if (point != null)
               if (bound.IsEnclosed(point.Point.X, point.Point.Y))
                  list.Add(point);
      }

   }
}
