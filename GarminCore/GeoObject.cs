using System;

namespace GarminCore {
   public abstract class GeoObject : IDisposable {

      int _type;

      /// <summary>
      /// vollständiger Typ (erweitert, Haupt- und Subtyp bilden eine max. 5stellige Hex-Zahl)
      /// </summary>
      public int Type {
         get {
            return _type;
         }
         set {
            _type = value;
         }
      }

      /// <summary>
      /// Haupttyp 0x00..0xFF
      /// </summary>
      public int MainType {
         get {
            return (_type & 0xFF00) >> 8;
         }
      }

      /// <summary>
      /// Subtyp 0x00..0xFF
      /// </summary>
      public int SubType {
         get {
            return _type & 0xFF;
         }
      }

      /// <summary>
      /// Text des Objektes
      /// </summary>
      public string? Text { get; set; }


      public GeoObject(int type, string? txt) {
         Type = type;
         Text = txt;
      }

      public override string ToString() {
         return string.Format("0x{0:x}", Type) + (!string.IsNullOrEmpty(Text) ? ", " + Text : "");
      }

      ~GeoObject() {
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

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}
