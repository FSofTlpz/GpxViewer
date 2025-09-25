using GMap.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpecialMapCtrl {

   /// <summary>
   ///     GMap.NET polygon
   /// </summary>
   public class MapPolygon : MapRoute, IDisposable {
      private bool _visible = true;

      /// <summary>
      /// is polygon visible
      /// </summary>
      public bool IsVisible {
         get => _visible;
         set {
            if (value != _visible) {
               _visible = value;

               if (Overlay != null &&
                   Overlay.Control != null) {
                  if (_visible)
                     Overlay.Control.M_UpdatePolygonLocalPosition(this);
                  else if (Overlay.Control.M_IsMouseOverPolygon) {
                     Overlay.Control.M_IsMouseOverPolygon = false;
                     Overlay.Control.M_RestoreCursorOnLeave();
                  }
                  if (!Overlay.Control.M_HoldInvalidation)
                     Overlay.Control.M_CoreInvalidate();
               }
            }
         }
      }

      /// <summary>
      /// can receive input
      /// </summary>
      public bool IsHitTestVisible = false;

      private bool _isMouseOver;

      /// <summary>
      /// is mouse over
      /// </summary>
      public bool IsMouseOver {
         get => _isMouseOver;
         internal set => _isMouseOver = value;
      }

      public MapOverlay? Overlay { get; set; }

      /// <summary>
      /// Indicates whether the specified point is contained within this System.Drawing.Drawing2D.GraphicsPath
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      internal bool IsInsideLocal(int x, int y) {
         if (_graphicsPath != null)
            return _graphicsPath.IsVisible(x, y);
         return false;
      }

      GraphicsPath? _graphicsPath;

      internal void UpdateGraphicsPath() {
         if (_graphicsPath == null)
            _graphicsPath = new GraphicsPath();
         else
            _graphicsPath.Reset();

         var pnts = new Point[LocalPoints.Count];
         for (int i = 0; i < LocalPoints.Count; i++) {
            var p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
            pnts[pnts.Length - 1 - i] = p2;
         }

         if (pnts.Length > 2)
            _graphicsPath.AddPolygon(pnts);
         else if (pnts.Length == 2)
            _graphicsPath.AddLines(pnts);
      }

      public virtual void OnRender(Graphics g) {
         if (IsVisible &&
             _graphicsPath != null) {
            g.FillPath(Fill, _graphicsPath);
            g.DrawPath(Stroke, _graphicsPath);
         }
      }

      public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
      [NonSerialized] public Pen Stroke = DefaultStroke;

      public static readonly Brush DefaultFill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));

      /// <summary>
      /// background color
      /// </summary>
      [NonSerialized] public Brush Fill = DefaultFill;

      public readonly List<GPoint> LocalPoints = new List<GPoint>();

      static MapPolygon() {
         DefaultStroke.LineJoin = LineJoin.Round;
         DefaultStroke.Width = 5;
      }

      public MapPolygon(List<PointLatLng> points, string name)
          : base(points, name) {
         LocalPoints.Capacity = Points.Count;
      }

      /// <summary>
      ///     checks if point is inside the polygon,
      ///     info.: http://greatmaps.codeplex.com/discussions/279437#post700449
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public bool IsInside(PointLatLng p) {
         int count = Points.Count;

         if (count < 3)
            return false;

         bool result = false;

         for (int i = 0, j = count - 1; i < count; i++) {
            var p1 = Points[i];
            var p2 = Points[j];

            if (p1.Lat < p.Lat && p2.Lat >= p.Lat || p2.Lat < p.Lat && p1.Lat >= p.Lat)
               if (p1.Lng + (p.Lat - p1.Lat) / (p2.Lat - p1.Lat) * (p2.Lng - p1.Lng) < p.Lng)
                  result = !result;

            j = i;
         }

         return result;
      }

      #region IDisposable Members

      bool _disposed;

      public virtual void Dispose() {
         if (!_disposed) {
            _disposed = true;

            LocalPoints.Clear();

            if (_graphicsPath != null) {
               _graphicsPath.Dispose();
               _graphicsPath = null;
            }

            Clear();
         }
      }

      #endregion
   }
}
