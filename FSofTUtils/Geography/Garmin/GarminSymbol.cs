using System;
using System.Drawing;
using System.IO;

namespace FSofTUtils.Geography.Garmin {
   public class GarminSymbol : IDisposable {

      public string Text {
         get; protected set;
      }

      public string Name {
         get; protected set;
      }

      public string Group {
         get; protected set;
      }

      public Bitmap Bitmap {
         get; protected set;
      }

      public int OffsetX {
         get; protected set;
      }

      public int OffsetY {
         get; protected set;
      }

      public GarminSymbol(string name, string group, string text, string filename, int offsetx, int offsety) {
         Name = name;
         Group = group;
         Text = text;
         using (Bitmap tmp = new Bitmap(filename)) {
            Bitmap = new Bitmap(tmp);
         }
         if (offsetx != int.MinValue &&
             offsety != int.MinValue) {
            OffsetX = offsetx;
            OffsetY = offsety;
         } else {
            OffsetX = -Bitmap.Width / 2;
            OffsetY = -Bitmap.Height / 2;
         }
      }

      public override string ToString() {
         return Name + " [" + Group + "] - " + Text;
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
               Bitmap?.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}
