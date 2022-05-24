using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Gpx = FSofTUtils.Geography.PoorGpx;
#if GMAP4SKIA
using GMap.NET.Skia;
#else
using GMap.NET.WindowsForms;
#endif

namespace SmallMapControl {

   /// <summary>
   /// Erweiterung der <see cref="GMap.NET.WindowsForms.GMapRoute"/> um Gpx-Daten und grafische Daten
   /// </summary>
   public class VisualTrack : GMapRoute {


      /// <summary>
      /// Anfang-Darstellung der Track-Polylinie
      /// </summary>
      static readonly CustomLineCap TrackStartCap;

      /// <summary>
      /// Ende-Darstellung der Track-Polylinie
      /// </summary>
      static readonly AdjustableArrowCap TrackEndCap;


      class PenWithDef {
         public readonly float Width;
         public readonly Color Color;
         public readonly bool IsSimple;
         public readonly Pen Pen;

         public PenWithDef() : this(Color.Black, 1, true) { }

         public PenWithDef(Color color, float width, bool simple) {
            Width = width;
            Color = color;
            IsSimple = simple;
            Pen = simple ?
                     CreateSimplePen(color, width) :
                     CreateExtPen(color, width);
         }

         public static Pen CreateSimplePen(Color col, float width) {
            return new Pen(col, width) {
               DashStyle = DashStyle.Solid,
               LineJoin = LineJoin.Round,
            };
         }

         public static Pen CreateExtPen(Color col, float width) {
            return new Pen(col, width) {
               DashStyle = DashStyle.Solid,
               LineJoin = LineJoin.Round,
               CustomStartCap = TrackStartCap,
               CustomEndCap = TrackEndCap,
            };
         }

         public override string ToString() {
            return string.Format("Width={0}, Color={1}", Width, Color);
         }
      }
      /*
            static PenDef[] predefPenData = new PenDef[] {
               new PenDef(VisualStyle.Standard, _standardWidth, _standardColor,false),
               new PenDef(VisualStyle.Marked, _markedWidth, _markedColor,false),
               new PenDef(VisualStyle.Editable, _editableWidth, _editableColor,false),
               new PenDef(VisualStyle.InEdit, _inEditableWidth, _inEditableColor,false),
               new PenDef(VisualStyle.SelectedPart, _selectedPartWidth, _selectedPartColor,false),
            };
      */

      // Pen/Brush für "special effects"

      static readonly Brush BrushFXPointSelected = new SolidBrush(Color.Yellow);
      static readonly Pen PenFXPointSelected = new Pen(Color.Black, 1);
      static readonly Brush BrushFXPointEditable = new SolidBrush(Color.FromArgb(150, Color.Black));

      /// <summary>
      /// Sammlung aller vordef. Pens
      /// </summary>
      static readonly Dictionary<VisualStyle, PenWithDef> predefPen;

      /// <summary>
      /// Standardfarbe (bei <see cref="VisualStyle.Standard"/>)
      /// </summary>
      public static Color StandardColor {
         get {
            return predefPen[VisualStyle.Standard].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Standard;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 2 (bei <see cref="VisualStyle.Standard2"/>)
      /// </summary>
      public static Color StandardColor2 {
         get {
            return predefPen[VisualStyle.Standard2].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Standard2;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 3 (bei <see cref="VisualStyle.Standard3"/>)
      /// </summary>
      public static Color StandardColor3 {
         get {
            return predefPen[VisualStyle.Standard3].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Standard3;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 4 (bei <see cref="VisualStyle.Standard4"/>)
      /// </summary>
      public static Color StandardColor4 {
         get {
            return predefPen[VisualStyle.Standard4].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Standard4;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 5 (bei <see cref="VisualStyle.Standard5"/>)
      /// </summary>
      public static Color StandardColor5 {
         get {
            return predefPen[VisualStyle.Standard5].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Standard5;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardbreite (bei <see cref="VisualStyle.Standard"/>)
      /// </summary>
      public static float StandardWidth {
         get {
            return predefPen[VisualStyle.Standard].Width;
         }
         set {
            VisualStyle vs = VisualStyle.Standard;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Markierungsfarbe (bei <see cref="VisualStyle.Marked"/>)
      /// </summary>
      public static Color MarkedColor {
         get {
            return predefPen[VisualStyle.Marked].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Marked;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Markierungsbreite (bei <see cref="VisualStyle.Marked"/>)
      /// </summary>
      public static float MarkedWidth {
         get {
            return predefPen[VisualStyle.Marked].Width;
         }
         set {
            VisualStyle vs = VisualStyle.Marked;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierfarbe (bei <see cref="VisualStyle.Editable"/>)
      /// </summary>
      public static Color EditableColor {
         get {
            return predefPen[VisualStyle.Editable].Color;
         }
         set {
            VisualStyle vs = VisualStyle.Editable;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierbreite (bei <see cref="VisualStyle.Editable"/>)
      /// </summary>
      public static float EditableWidth {
         get {
            return predefPen[VisualStyle.Editable].Width;
         }
         set {
            VisualStyle vs = VisualStyle.Editable;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierfarbe (bei <see cref="VisualStyle.InEdit"/>)
      /// </summary>
      public static Color InEditableColor {
         get {
            return predefPen[VisualStyle.InEdit].Color;
         }
         set {
            VisualStyle vs = VisualStyle.InEdit;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierbreite (bei <see cref="VisualStyle.InEdit"/>)
      /// </summary>
      public static float InEditableWidth {
         get {
            return predefPen[VisualStyle.InEdit].Width;
         }
         set {
            VisualStyle vs = VisualStyle.InEdit;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Farbe (bei <see cref="VisualStyle.SelectedPart"/>)
      /// </summary>
      public static Color SelectedPartColor {
         get {
            return predefPen[VisualStyle.SelectedPart].Color;
         }
         set {
            VisualStyle vs = VisualStyle.SelectedPart;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Breite (bei <see cref="VisualStyle.SelectedPart"/>)
      /// </summary>
      public static float SelectedPartWidth {
         get {
            return predefPen[VisualStyle.SelectedPart].Width;
         }
         set {
            VisualStyle vs = VisualStyle.SelectedPart;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /*
       * Zugriff auf Pen kann beim Multithreading off. zu Problemen führen!
       * 
            /// <summary>
            /// Standardfarbe (bei <see cref="VisualStyle.Standard"/>)
            /// </summary>
            public static Color StandardColor {
               get {
                  return PenStandard.Color;
               }
            }
            /// <summary>
            /// Standardbreite (bei <see cref="VisualStyle.Standard"/>)
            /// </summary>
            public static double StandardWidth {
               get {
                  return PenStandard.Width;
               }
            }

            /// <summary>
            /// Editierfarbe (bei <see cref="VisualStyle.Editable"/>)
            /// </summary>
            public static Color EditableColor {
               get {
                  return PenEditable.Color;
               }
            }
            /// <summary>
            /// Editierbreite (bei <see cref="VisualStyle.Editable"/>)
            /// </summary>
            public static double EditableWidth {
               get {
                  return PenEditable.Width;
               }
            }

            /// <summary>
            /// Editierfarbe (bei <see cref="VisualStyle.InEdit"/>)
            /// </summary>
            public static Color InEditableColor {
               get {
                  return PenInEdit.Color;
               }
            }
            /// <summary>
            /// Editierbreite (bei <see cref="VisualStyle.InEdit"/>)
            /// </summary>
            public static double InEditableWidth {
               get {
                  return PenInEdit.Width;
               }
            }
      */

      public enum VisualStyle {
         /// <summary>
         /// noch nicht festgelegt
         /// </summary>
         notdefined,
         /// <summary>
         /// Standard
         /// </summary>
         Standard,
         /// <summary>
         /// Standard 2
         /// </summary>
         Standard2,
         /// <summary>
         /// Standard 3
         /// </summary>
         Standard3,
         /// <summary>
         /// Standard 4
         /// </summary>
         Standard4,
         /// <summary>
         /// Standard 5
         /// </summary>
         Standard5,
         /// <summary>
         /// markiert
         /// </summary>
         Marked,
         /// <summary>
         /// editierbar
         /// </summary>
         Editable,
         /// <summary>
         /// im Edit-Modus
         /// </summary>
         InEdit,
         /// <summary>
         /// Teil
         /// </summary>
         SelectedPart,

         /// <summary>
         /// Nutzerdefiniert
         /// </summary>
         Custom,
      }


      /// <summary>
      /// Gpx-Daten
      /// </summary>
      public Track RealTrack { get; private set; } = null;

      /// <summary>
      /// akt. Darstellung
      /// </summary>
      public VisualStyle Visualstyle { get; private set; } = VisualStyle.notdefined;

      /// <summary>
      /// Linienfarbe
      /// </summary>
      public Color Color {
         get {
            return Stroke.Color;
         }
         set {
            SetVisualStyle(value, Width);
         }
      }

      /// <summary>
      /// Linienbreite
      /// </summary>
      public double Width {
         get {
            return Stroke.Width;
         }
         set {
            SetVisualStyle(Color, value);
         }
      }


      static VisualTrack() {
         GraphicsPath hPath = new GraphicsPath();
         hPath.AddEllipse(-1.5F, -1.5F, 3, 3);
         TrackStartCap = new CustomLineCap(hPath, null);

         TrackEndCap = new AdjustableArrowCap(3, 3);

         predefPen = new Dictionary<VisualStyle, PenWithDef>() {
            // Standard-Pen
            { VisualStyle.Standard,     new PenWithDef(Color.FromArgb(100, Color.Blue), 3.0F, false) },
            // Standard-Pen 2
            { VisualStyle.Standard2,    new PenWithDef(Color.FromArgb(100, Color.Blue), 3.0F, false) },
            // Standard-Pen 3
            { VisualStyle.Standard3,    new PenWithDef(Color.FromArgb(100, Color.Blue), 3.0F, false) },
            // Standard-Pen 4
            { VisualStyle.Standard4,    new PenWithDef(Color.FromArgb(100, Color.Blue), 3.0F, false) },
            // Standard-Pen 5
            { VisualStyle.Standard5,    new PenWithDef(Color.FromArgb(100, Color.Blue), 3.0F, false) },
            // Standard-Pen für markierte Tracks
            { VisualStyle.Marked,       new PenWithDef(Color.FromArgb(255, Color.Cyan), 3.0F, false) },
            // Standard-Pen für editierbare Tracks
            { VisualStyle.Editable,     new PenWithDef(Color.FromArgb(100, Color.Black), 3.0F, false) },
            // Standard-Pen für gerade editierten Track
            { VisualStyle.InEdit,       new PenWithDef(Color.FromArgb(120, Color.OrangeRed), 4.0F, false) },
            // Standard-Pen für einen Teil-Track
            { VisualStyle.SelectedPart, new PenWithDef(Color.FromArgb(120, Color.Orange), 6.0F, true) },
         };
      }

      /// <summary>
      /// erzeugt den <see cref="VisualTrack"/> zum <see cref="Track"/> 
      /// </summary>
      /// <param name="track"></param>
      /// <param name="name"></param>
      /// <param name="style"></param>
      /// <param name="mapControl"></param>
      public VisualTrack(Track track,
                         string name,
                         VisualStyle style = VisualStyle.Standard) :
         base(convertPoints(track.GpxSegment.Points), name) {
         RealTrack = track;
         init(style);
      }

      /// <summary>
      /// erzeugt den <see cref="VisualTrack"/> zum <see cref="Track"/>
      /// </summary>
      /// <param name="track"></param>
      /// <param name="name"></param>
      /// <param name="coltrack"></param>
      /// <param name="widthtrack"></param>
      /// <param name="style"></param>
      /// <param name="mapControl"></param>
      public VisualTrack(Track track,
                         string name,
                         Color coltrack,
                         double widthtrack,
                         VisualStyle style = VisualStyle.Standard) :
         base(convertPoints(track.GpxSegment.Points), name) {
         RealTrack = track;
         init(style);
         // nur falls der Style änderbar ist, werden Farbe und Breite übernommen
         if (style == VisualStyle.Standard ||
             style == VisualStyle.Editable)
            SetVisualStyle(coltrack, widthtrack);
      }

      public VisualTrack(Track track) :
         base(convertPoints(track.GpxSegment.Points), track.VisualTrack != null ? track.VisualTrack.Name : "") {
         if (track.VisualTrack != null) {
            SetVisualStyle(track.VisualTrack.Visualstyle);
            SetVisualStyle(track.VisualTrack.Color, track.VisualTrack.Width);

         }
      }

      void init(VisualStyle style) {
         IsHitTestVisible = true;
         IsVisible = false;

         SetVisualStyle(style);
      }

      bool actualPenIsPredefined() {
         foreach (var item in predefPen) {
            if (Stroke == item.Value.Pen)
               return true;
         }
         return false;
      }

      /// <summary>
      /// konvertiert die Daten eines <see cref="Gpx.GpxTrackPoint"/>-Liste in eine <see cref="GMap.NET.PointLatLng"/>-Liste
      /// </summary>
      /// <param name="segment"></param>
      /// <returns></returns>
      static GMap.NET.PointLatLng[] convertPoints(IList<Gpx.GpxTrackPoint> gpxptlst) {
         if (gpxptlst != null) {
            GMap.NET.PointLatLng[] gmappt = new GMap.NET.PointLatLng[gpxptlst.Count];
            for (int i = 0; i < gpxptlst.Count; i++) {
               gmappt[i].Lat = gpxptlst[i].Lat;
               gmappt[i].Lng = gpxptlst[i].Lon;
            }
            return gmappt;
         }
         return new GMap.NET.PointLatLng[0];
      }

      /// <summary>
      /// Stil setzen
      /// </summary>
      /// <param name="style"></param>
      public void SetVisualStyle(VisualStyle style) {
         if (Visualstyle != style) {
            Visualstyle = style;
            Stroke = predefPen[Visualstyle].Pen;
            Refresh();
         }
      }

      /// <summary>
      /// ev. nutzerdefinierten Stil setzen
      /// </summary>
      /// <param name="col"></param>
      /// <param name="width"></param>
      public void SetVisualStyle(Color col, double width) {
         if (width >= 0) {
            if (Stroke.Color != col ||
                Stroke.Width != (float)width) {
               if (actualPenIsPredefined()) { // neu erzeugen
                  Stroke = PenWithDef.CreateExtPen(col, (float)width);
               } else { // nur anpassen
                  //Stroke.Color = col;
                  //Stroke.Width = (float)width;
                  Stroke.Dispose();
                  Stroke = PenWithDef.CreateExtPen(col, (float)width);
               }
               Visualstyle = VisualStyle.Custom;
               Refresh();
            }
         }
      }

      /// <summary>
      /// (Pseudo-)Refresh der Anzeige
      /// </summary>
      public void Refresh() {
         if (IsVisible) {
            IsVisible = false;
            IsVisible = true;
         }
      }

      /// <summary>
      /// für spezielle zusätzliche Effekte
      /// </summary>
      /// <param name="g"></param>
      public override void OnRender(Graphics g) {
         base.OnRender(g);

         // "special effects"
         switch (Visualstyle) {
            case VisualStyle.SelectedPart:
               if (LocalPoints != null) {
                  for (int i = 0; i < LocalPoints.Count; i++) {
                     g.FillEllipse(BrushFXPointSelected, LocalPoints[i].X - 3, LocalPoints[i].Y - 3, 7, 7);
                     g.DrawEllipse(PenFXPointSelected, LocalPoints[i].X - 3, LocalPoints[i].Y - 3, 7, 7);
                  }
               }
               break;

            case VisualStyle.InEdit:
               if (LocalPoints != null) {
                  for (int i = 0; i < LocalPoints.Count; i++) {
                     g.FillRectangle(BrushFXPointEditable, LocalPoints[i].X - 1, LocalPoints[i].Y - 1, 2, 2);
                  }
               }
               break;
         }
      }

      public override string ToString() {
         return string.Format("Visualstyle={0}, Color={1}, Width={2}, GpxTrackData=[{3}]", Visualstyle, Color, Width, RealTrack);
      }

   }
}
