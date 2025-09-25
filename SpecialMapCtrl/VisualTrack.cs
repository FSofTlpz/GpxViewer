using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Gpx = FSofTUtils.Geography.PoorGpx;
using System;
using System.Diagnostics;
#if GMAP4SKIA
//using GMap.NET.Skia;
using SkiaSharp;
#else
//using GMap.NET.WindowsForms;
#endif

namespace SpecialMapCtrl {

   /// <summary>
   /// Erweiterung der <see cref="GMap.NET.WindowsForms.GMapRoute"/> um Gpx-Daten und grafische Daten
   /// </summary>
   public class VisualTrack : MapTrack {

      class PenWithDef {
         public readonly float Width;
         public readonly Color Color;
         public readonly bool IsSimple;
         public readonly Pen Pen;

         public LineCap StartCap { get; protected set; }
         public LineCap EndCap { get; protected set; }


         public PenWithDef() : this(Color.Black, 1, true) {
            StartCap = LineCap.Flat;
            EndCap = LineCap.Flat;
         }

         public PenWithDef(Color color, float width, bool simple) {
            Width = width;
            Color = color;
            IsSimple = simple;
            Pen = new Pen(color, width) {
               DashStyle = DashStyle.Solid,
               LineJoin = LineJoin.Round,
            };
         }

         public override string ToString() {
            return string.Format("Width={0}, Color={1}", Width, Color);
         }
      }

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
         /// z.B. für "editierbarer Track"
         /// </summary>
         Editable,
         /// <summary>
         /// z.B. für "markiert/hervorgehoben"
         /// </summary>
         Marked,
         /// <summary>
         /// z.B. für "markiert für Bearbeitung"
         /// </summary>
         Marked4Edit,
         /// <summary>
         /// z.B. für "im Edit-Modus" (mit spez. Punktdarstellung)
         /// </summary>
         InEdit,
         /// <summary>
         /// z.B. für "Teil eines Tracks" (mit spez. Punktdarstellung)
         /// </summary>
         SelectedPart,

         /// <summary>
         /// z.B. für "wird gerade aufgezeichnet"
         /// </summary>
         LiveDraw,

         /// <summary>
         /// Nutzerdefiniert
         /// </summary>
         Custom,
      }

      #region static Vars/Props

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
         get => predefPen[VisualStyle.Standard].Color;
         set {
            VisualStyle vs = VisualStyle.Standard;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 2 (bei <see cref="VisualStyle.Standard2"/>)
      /// </summary>
      public static Color StandardColor2 {
         get => predefPen[VisualStyle.Standard2].Color;
         set {
            VisualStyle vs = VisualStyle.Standard2;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 3 (bei <see cref="VisualStyle.Standard3"/>)
      /// </summary>
      public static Color StandardColor3 {
         get => predefPen[VisualStyle.Standard3].Color;
         set {
            VisualStyle vs = VisualStyle.Standard3;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 4 (bei <see cref="VisualStyle.Standard4"/>)
      /// </summary>
      public static Color StandardColor4 {
         get => predefPen[VisualStyle.Standard4].Color;
         set {
            VisualStyle vs = VisualStyle.Standard4;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardfarbe 5 (bei <see cref="VisualStyle.Standard5"/>)
      /// </summary>
      public static Color StandardColor5 {
         get => predefPen[VisualStyle.Standard5].Color;
         set {
            VisualStyle vs = VisualStyle.Standard5;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Standardbreite (bei <see cref="VisualStyle.Standard"/>)
      /// </summary>
      public static float StandardWidth {
         get => predefPen[VisualStyle.Standard].Width;
         set {
            VisualStyle vs = VisualStyle.Standard;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Markierungsfarbe (bei <see cref="VisualStyle.Marked"/>)
      /// </summary>
      public static Color MarkedColor {
         get => predefPen[VisualStyle.Marked].Color;
         set {
            VisualStyle vs = VisualStyle.Marked;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Markierungsbreite (bei <see cref="VisualStyle.Marked"/>)
      /// </summary>
      public static float MarkedWidth {
         get => predefPen[VisualStyle.Marked].Width;
         set {
            VisualStyle vs = VisualStyle.Marked;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierfarbe (bei <see cref="VisualStyle.Editable"/>)
      /// </summary>
      public static Color EditableColor {
         get => predefPen[VisualStyle.Editable].Color;
         set {
            VisualStyle vs = VisualStyle.Editable;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierbreite (bei <see cref="VisualStyle.Editable"/>)
      /// </summary>
      public static float EditableWidth {
         get => predefPen[VisualStyle.Editable].Width;
         set {
            VisualStyle vs = VisualStyle.Editable;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierfarbe (bei <see cref="VisualStyle.Marked4Edit"/>)
      /// </summary>
      public static Color Marked4EditColor {
         get => predefPen[VisualStyle.Marked4Edit].Color;
         set {
            VisualStyle vs = VisualStyle.Marked4Edit;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierbreite (bei <see cref="VisualStyle.Marked4Edit"/>)
      /// </summary>
      public static float Marked4EditWidth {
         get => predefPen[VisualStyle.Marked4Edit].Width;
         set {
            VisualStyle vs = VisualStyle.Marked4Edit;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierfarbe (bei <see cref="VisualStyle.InEdit"/>)
      /// </summary>
      public static Color InEditableColor {
         get => predefPen[VisualStyle.InEdit].Color;
         set {
            VisualStyle vs = VisualStyle.InEdit;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Editierbreite (bei <see cref="VisualStyle.InEdit"/>)
      /// </summary>
      public static float InEditableWidth {
         get => predefPen[VisualStyle.InEdit].Width;
         set {
            VisualStyle vs = VisualStyle.InEdit;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Farbe (bei <see cref="VisualStyle.SelectedPart"/>)
      /// </summary>
      public static Color SelectedPartColor {
         get => predefPen[VisualStyle.SelectedPart].Color;
         set {
            VisualStyle vs = VisualStyle.SelectedPart;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Breite (bei <see cref="VisualStyle.SelectedPart"/>)
      /// </summary>
      public static float SelectedPartWidth {
         get => predefPen[VisualStyle.SelectedPart].Width;
         set {
            VisualStyle vs = VisualStyle.SelectedPart;
            predefPen[vs] = new PenWithDef(predefPen[vs].Color, value, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Farbe für Aufzeichnung (bei <see cref="VisualStyle.LiveDraw"/>)
      /// </summary>
      public static Color LiveDrawColor {
         get => predefPen[VisualStyle.LiveDraw].Color;
         set {
            VisualStyle vs = VisualStyle.LiveDraw;
            predefPen[vs] = new PenWithDef(value, predefPen[vs].Width, predefPen[vs].IsSimple);
         }
      }

      /// <summary>
      /// Breite für Aufzeichnung (bei <see cref="VisualStyle.LiveDraw"/>)
      /// </summary>
      public static float LiveDrawWidth {
         get => predefPen[VisualStyle.LiveDraw].Width;
         set {
            VisualStyle vs = VisualStyle.LiveDraw;
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

      public static List<(int, Pen)> SlopePens;

      #endregion

      public bool IsSlopeVisible {
         get => RealTrack != null ? RealTrack.IsSlopeVisible : false;
      }

      /// <summary>
      /// Gpx-Daten
      /// </summary>
      public Track? RealTrack { get; private set; } = null;

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

      LineCap startCap = LineCap.Flat;
      LineCap endCap = LineCap.Flat;

      /// <summary>
      /// Anstiege in Prozent
      /// <para>sollte bei Verwendung genau <see cref="LocalPolyline.Count"/> Elemente haben</para>
      /// </summary>
      List<int> segmentSlope = new List<int>();


      static VisualTrack() {
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
            { VisualStyle.LiveDraw,     new PenWithDef(Color.FromArgb(255, Color.Black), 3.0F, false) },
            // Standard-Pen für markierte Tracks
            { VisualStyle.Marked,       new PenWithDef(Color.FromArgb(255, Color.Cyan), 3.0F, false) },
            // Standard-Pen für editierbare Tracks
            { VisualStyle.Editable,     new PenWithDef(Color.FromArgb(100, Color.Black), 3.0F, false) },
            { VisualStyle.Marked4Edit,  new PenWithDef(Color.FromArgb(120, Color.OrangeRed), 4.0F, false) },
            // Standard-Pen für gerade editierten Track
            { VisualStyle.InEdit,       new PenWithDef(Color.FromArgb(120, Color.OrangeRed), 4.0F, false) },
            // Standard-Pen für einen Teil-Track
            { VisualStyle.SelectedPart, new PenWithDef(Color.FromArgb(120, Color.Orange), 6.0F, true) },
         };

         float penwidth = 2;
         SlopePens = new List<(int, Pen)> {
            (-1000, new Pen(Color.DarkRed, penwidth) { StartCap = LineCap.Round, EndCap = LineCap.Round }),
            (-20, new Pen(Color.Red, penwidth) { StartCap = LineCap.Round, EndCap = LineCap.Round }),
            (-15, new Pen(Color.OrangeRed, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (-10, new Pen(Color.Orange, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (-5, new Pen(Color.Yellow, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (-2, new Pen(Color.DarkGreen, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (2, new Pen(Color.Yellow, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (5, new Pen(Color.Orange, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (10, new Pen(Color.OrangeRed, penwidth) { StartCap= LineCap.Round, EndCap= LineCap.Round}),
            (15, new Pen(Color.Red, penwidth) { StartCap = LineCap.Round, EndCap = LineCap.Round }),
            (20, new Pen(Color.DarkRed, penwidth) { StartCap = LineCap.Round, EndCap = LineCap.Round }),
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
         base(convertPoints(track.GpxSegment != null ? track.GpxSegment.Points : null), name) {
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
                         VisualStyle style = VisualStyle.Standard) : this(track, name, style) {
         RealTrack = track;
         init(style);
         // nur falls der Style änderbar ist, werden Farbe und Breite übernommen
         if (IsChangeableStyle(style))
            SetVisualStyle(coltrack, widthtrack);
         //Debug.WriteLine("### NEW " + ToString() + " " + (Overlay != null ? Overlay.Id : "null"));
      }

      void init(VisualStyle style) {
         IsHitTestVisible = true;
         IsVisible = false;

         if (IsSlopeVisible)
            calculateSlope();

         SetVisualStyle(style);
      }

      /// <summary>
      /// Ist die Farbe/Linienbreite für diesen Stil änderbar?
      /// </summary>
      /// <param name="style"></param>
      /// <returns></returns>
      public static bool IsChangeableStyle(VisualStyle style) => style == VisualStyle.Standard ||
                                                                 style == VisualStyle.Standard2 ||
                                                                 style == VisualStyle.Standard3 ||
                                                                 style == VisualStyle.Standard4 ||
                                                                 style == VisualStyle.Standard5 ||
                                                                 style == VisualStyle.Editable;

      /// <summary>
      /// erzeugt die Liste der Anstiege
      /// </summary>
      void calculateSlope() {
         segmentSlope.Clear();
         if (RealTrack != null && RealTrack.GpxSegment != null)
            for (int i = 1; i < RealTrack.GpxSegment.Points.Count; i++)
               segmentSlope.Add((int)Math.Round(getSlope(RealTrack.GpxSegment.Points[i - 1], RealTrack.GpxSegment.Points[i])));
      }

      /// <summary>
      /// liefert den Anstieg in Prozent
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <returns></returns>
      double getSlope(Gpx.GpxTrackPoint p1, Gpx.GpxTrackPoint p2) {
         if (p1.Elevation != Gpx.BaseElement.NOTUSE_DOUBLE &&
             p2.Elevation != Gpx.BaseElement.NOTUSE_DOUBLE) {
            FSofTUtils.Geography.GeoHelper.Wgs84ShortXYDelta(p1.Lon, p2.Lon, p1.Lat, p2.Lat, out double deltax, out double deltay);
            double distance = Math.Sqrt(deltax * deltax + deltay * deltay);
            if (distance != 0)
               return 100 * (p2.Elevation - p1.Elevation) / distance;
         }
         return 0;
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
      /// konvertiert die Daten eines <see cref="Gpx.GpxTrackPoint"/>-Liste in eine <see cref="GMap.NET.PointLatLng"/>-Liste
      /// </summary>
      /// <param name="segment"></param>
      /// <returns></returns>
      static GMap.NET.PointLatLng[] convertPoints(Gpx.ListTS<Gpx.GpxTrackPoint>? gpxptlst) {
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
            setCaps(predefPen[Visualstyle].IsSimple);
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
            if (Stroke.Color.ToArgb() != col.ToArgb() ||
                Stroke.Width != (float)width) {
               if (!actualPenIsPredefined())
                  Stroke.Dispose();
               PenWithDef penWithDef = new PenWithDef(col, (float)width, false);
               Stroke = penWithDef.Pen;
               setCaps(false);
               Visualstyle = VisualStyle.Custom;
               Refresh();
            }
         }
      }

      void setCaps(bool simple) {
         if (simple) {
            startCap =
            endCap = LineCap.Flat;
         } else {
            startCap = LineCap.RoundAnchor;
            endCap = LineCap.ArrowAnchor;
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

      public static void SetSlopeValues(IList<Color> cols, IList<int> percent) {
         if (1 < cols.Count && 1 < percent.Count) {
            float penwidth = 2;
            SlopePens.Clear();
            for (int i = 0; i < cols.Count && i < percent.Count; i++)
               SlopePens.Add((percent[i],
                              new Pen(cols[i], penwidth) { StartCap = LineCap.Round, EndCap = LineCap.Round }));
         }
      }

      /// <summary>
      /// für spezielle zusätzliche Effekte
      /// </summary>
      /// <param name="g"></param>
      public override void OnRender(Graphics g) {
         base.OnRender(g);

         if (IsVisible &&
             visualPolylines.Count > 0) {
            // "special effects"
            switch (Visualstyle) {
               case VisualStyle.SelectedPart:
                  foreach (var polyline in visualPolylines) {
                     for (int i = 0; i < polyline.Length; i++) {
                        g.FillEllipse(BrushFXPointSelected, polyline[i].X - 3, polyline[i].Y - 3, 7, 7);
                        g.DrawEllipse(PenFXPointSelected, polyline[i].X - 3, polyline[i].Y - 3, 7, 7);
                     }
                  }
                  break;

               case VisualStyle.InEdit:
                  foreach (var polyline in visualPolylines) {
                     for (int i = 0; i < polyline.Length; i++) {     // alle Punkte extra kennzeichnen
                        g.FillRectangle(BrushFXPointEditable,
                                        polyline[i].X - Stroke.Width / 2,
                                        polyline[i].Y - Stroke.Width / 2,
                                        Stroke.Width,
                                        Stroke.Width);
                     }
                  }
                  break;

               default:

                  if (IsSlopeVisible &&
                      0 < SlopePens.Count) {
                     if (segmentSlope.Count != LocalPolyline.Length - 1)
                        calculateSlope();

                     for (int p = 0; p < visualParts.Count; p++) {
                        int startidx = visualParts[p].Item1;
                        int count = visualParts[p].Item2;
                        for (int i = startidx; i < startidx + count - 1; i++) {
                           Pen pen = pen4Slope(segmentSlope[i]);
                           if (pen != null)
                              drawSlopeSign(g,
                                            pen,
                                            LocalPolyline[i].X, LocalPolyline[i].Y,
                                            LocalPolyline[i + 1].X, LocalPolyline[i + 1].Y,
                                            0 <= segmentSlope[i]);
                        }
                     }
                  }
                  break;
            }
         }
      }

      Pen pen4Slope(int slope) {
         for (int sp = SlopePens.Count - 1; sp >= 0; sp--)
            if (SlopePens[sp].Item1 <= slope)
               return SlopePens[sp].Item2;
         return SlopePens[0].Item2;
      }

      void drawSlopeSign(Graphics graphics, Pen pen, float fromX, float fromY, float endX, float endY, bool positive = true) {
         //graphics.DrawLine(pen, fromX, fromY, endX, endY); // farbige Linie

         float x = fromX + (endX - fromX) / 2;
         float y = fromY + (endY - fromY) / 2;

         drawArrow(graphics,
                   Color.Empty,
                   pen,
                   Stroke.Width * 4,
                   positive ? fromX : endX,
                   positive ? fromY : endY,
                   x,
                   y);

         //graphics.DrawEllipse(pen,
         //                     x - Stroke.Width * 0.7F,
         //                     y - Stroke.Width * 0.7F,
         //                     Stroke.Width * 1.4F,
         //                     Stroke.Width * 1.4F
         //   );
      }

      void drawArrow(Graphics graphics,
                     Color fillcolor,
                     Pen? pen,
                     float basewidth,
                     float fromX, float fromY,
                     float endX, float endY) {
         float radians = (float)Math.Atan2(endY - fromY, endX - fromX);    // Rotation
#if GMAP4SKIA
         SKMatrix orgMatrix = graphics.SKCanvas.TotalMatrix;
         SKMatrix rotMatrix = SKMatrix.CreateRotation(radians, endX, endY);
         graphics.SKCanvas.SetMatrix(graphics.SKCanvas.TotalMatrix.PreConcat(rotMatrix));    // jetzt neues KS: Linie kommt "von links" und endet in (xend, yend)
#else
         Matrix orgMatrix = graphics.Transform;    // Erzeugt eine Kopie der Matrix!
         Matrix newMatrix = graphics.Transform;    // Erzeugt eine Kopie der Matrix!
         newMatrix.RotateAt((float)(180 * radians / Math.PI), new PointF(endX, endY), MatrixOrder.Prepend);
         graphics.Transform = newMatrix;
#endif
         if (pen == null) {
            SolidBrush brush = new SolidBrush(fillcolor);
            graphics.FillPolygon(brush, new PointF[] {
                                                      new PointF(endX, endY),
                                                      new PointF(endX - basewidth / 2, endY - basewidth / 6),
                                                      new PointF(endX - basewidth / 2, endY + basewidth / 6),
                                                      new PointF(endX, endY)});
            brush.Dispose();
         } else
            graphics.DrawLines(pen, new PointF[] {
                                                      new PointF(endX - basewidth / 3, endY - basewidth / 2),
                                                      new PointF(endX, endY),
                                                      new PointF(endX - basewidth / 3, endY + basewidth / 2)});
         //graphics.DrawLine(pen, endX, endY - basewidth, endX, endY + basewidth);

#if GMAP4SKIA
         graphics.SKCanvas.SetMatrix(orgMatrix);
#else
         graphics.Transform = orgMatrix;
#endif
      }

      /// <summary>
      /// zum Zeichnen spezieller Linienenden
      /// </summary>
      /// <param name="graphics"></param>
      /// <param name="color"></param>
      /// <param name="linewidth"></param>
      /// <param name="fromX"></param>
      /// <param name="fromY"></param>
      /// <param name="endX"></param>
      /// <param name="endY"></param>
      /// <param name="isStartCap"></param>
      protected override void OnSpecialCapDraw(Graphics graphics, Color color, float linewidth, float fromX, float fromY, float endX, float endY, bool isStartCap) {
         //base.OnSpecialCapDraw(graphics, color, linewidth, fromX, fromY, endX, endY, isStartCap);

         LineCap cap = LineCap.Flat;

         if (isStartCap) {
            if (startCap == LineCap.RoundAnchor) {
               cap = startCap;
            }
         } else {
            if (endCap == LineCap.ArrowAnchor) {
               cap = endCap;
            }
         }

         switch (cap) {
            case LineCap.RoundAnchor: {
                  SolidBrush brush = new SolidBrush(color);
                  graphics.FillEllipse(brush, endX - linewidth, endY - linewidth, 2 * linewidth, 2 * linewidth);
                  brush.Dispose();
               }
               break;

            case LineCap.ArrowAnchor: {
                  drawArrow(graphics, color, null, linewidth * 8, fromX, fromY, endX, endY);
               }
               break;
         }
      }

      public override string ToString() {
         return string.Format("Visualstyle={0}, Color={1}, Width={2}, GpxTrackData=[{3}]", Visualstyle, Color, Width, RealTrack);
      }

   }
}
