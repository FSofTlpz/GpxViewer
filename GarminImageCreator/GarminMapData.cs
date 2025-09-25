using GarminImageCreator.Garmin;
using System;

namespace GarminImageCreator {

   /// <summary>
   /// zur Verwaltung der Garmin-Tiles einer einzelnen Karte und der zugehörigen grafischen Definitionen
   /// </summary>
   public class GarminMapData : IDisposable {

      /// <summary>
      /// TDB-Datei
      /// </summary>
      public readonly string TdbFilename;

      /// <summary>
      /// TYP-Datei
      /// </summary>
      public readonly string TypFilename;

      /// <summary>
      /// graf. Def. aller Objektarten der Karte
      /// </summary>
      public readonly GarminGraphicData GraphicData;

      /// <summary>
      /// zur Ermittlung der <see cref="DetailMapExt"/> dieser Karte
      /// </summary>
      public readonly DetailMapManager DetailMapManager;


      /// <summary>
      /// Daten zur Verwaltung einer einzelnen Garminkarte
      /// </summary>
      /// <param name="tdbfile">TDB-Datei</param>
      /// <param name="typfile">TYP-Datei</param>
      /// <param name="fontname">Name des Fonts</param>
      /// <param name="textfactor">Vergrößerungsfaktor für Texte</param>
      /// <param name="linefactor">Verbreiterungsfaktor für Linien</param>
      /// <param name="symbolfactor">Vergrößerungsfaktor für Symbole</param>
      public GarminMapData(string tdbfile,
                           string typfile,
                           string fontname,
                           double textfactor = 1.0,
                           double linefactor = 1.0,
                           double symbolfactor = 1.0) {
         TdbFilename = tdbfile;
         TypFilename = typfile;
         GraphicData = new GarminGraphicData(typfile, fontname, textfactor, linefactor, symbolfactor);
         DetailMapManager = new DetailMapManager(TdbFilename);
      }

      public GarminMapData(GarminMapData gmd) {
         TdbFilename = gmd.TdbFilename;
         TypFilename = gmd.TypFilename;
         GraphicData = gmd.GraphicData;
         DetailMapManager = gmd.DetailMapManager;
      }


      /// <summary>
      /// liefert die akt. geogr. Breite der Mitte des Bereiches vom <see cref="DetailMapManager"/>
      /// </summary>
      /// <returns></returns>
      public double GetMapCenterLat() {
         return DetailMapManager.GetMapCenterLat();
      }

      ~GarminMapData() {
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
               GraphicData.Dispose();
               DetailMapManager.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
