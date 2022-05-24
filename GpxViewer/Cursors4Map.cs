using System.IO;
using System.Windows.Forms;

namespace GpxViewer {

   /// <summary>
   /// verschiedene Cursors für die Karte
   /// </summary>
   public class Cursors4Map {

      /// <summary>
      /// Standard-Kartencursor
      /// </summary>
      public Cursor Std { get; }

      /// <summary>
      /// Cursor um einen Marker zu setzen
      /// </summary>
      public Cursor SetMarker { get; }

      /// <summary>
      /// Cursor um einen Track zu zeichnen
      /// </summary>
      public Cursor DrawTrack { get; }

      /// <summary>
      /// Cursor für eine Edit-Auswahl
      /// </summary>
      public Cursor Hand { get; }

      /// <summary>
      /// Cursor für eine Split-Auswahl
      /// </summary>
      public Cursor Split { get; }

      /// <summary>
      /// Cursor für eine Concat-Auswahl
      /// </summary>
      public Cursor Concat { get; }


      public Cursors4Map(Cursor standard) {
         Std = standard;
         SetMarker = new Cursor(new MemoryStream(Properties.Resources.CursorMarker));
         DrawTrack = new Cursor(new MemoryStream(Properties.Resources.CursorDrawTrack));
         Hand = new Cursor(new MemoryStream(Properties.Resources.CursorHand));
         Split = new Cursor(new MemoryStream(Properties.Resources.CursorSplit));
         Concat = new Cursor(new MemoryStream(Properties.Resources.CursorConcat));
      }

   }
}
