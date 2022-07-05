using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace GMap.NET.WindowsForms
{
    /// <summary>
    ///     GMap.NET route
    /// </summary>
    [Serializable]
    public class GMapRoute : MapRoute, ISerializable, IDeserializationCallback, IDisposable
    {
        GMapOverlay _overlay;

        public GMapOverlay Overlay
        {
            get
            {
                return _overlay;
            }
            internal set
            {
                _overlay = value;
            }
        }

        private bool _visible = true;

        /// <summary>
        ///     is marker visible
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (value != _visible)
                {
                    _visible = value;

                    if (Overlay != null && Overlay.Control != null)
                    {
                        if (_visible)
                        {
                            Overlay.Control.UpdateRouteLocalPosition(this);
                        }
                        else
                        {
                            if (Overlay.Control.IsMouseOverRoute)
                            {
                                Overlay.Control.IsMouseOverRoute = false;
                                Overlay.Control.RestoreCursorOnLeave();
                            }
                        }

                        {
                            if (!Overlay.Control.HoldInvalidation)
                            {
                                Overlay.Control.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     can receive input
        /// </summary>
        public bool IsHitTestVisible = false;

        private bool _isMouseOver;

        /// <summary>
        ///     is mouse over
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }
            internal set
            {
                _isMouseOver = value;
            }
        }

        /// <summary>
        ///     Indicates whether the specified point is contained within this System.Drawing.Drawing2D.GraphicsPath
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsInside(int x, int y)
        {
            if (_graphicsPath != null)
            {
                return _graphicsPath.IsOutlineVisible(x, y, Stroke);
            }

            return false;
        }

        GraphicsPath _graphicsPath;
#if ORGUPDATEGRAPHICSPATH
        internal void UpdateGraphicsPath()
        {
            if (_graphicsPath == null)
            {
                _graphicsPath = new GraphicsPath();
            }
            else
            {
                _graphicsPath.Reset();
            }

            {
                for (int i = 0; i < LocalPoints.Count; i++)
                {
                    var p2 = LocalPoints[i];

                    if (i == 0)
                    {
                        _graphicsPath.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                    }
                    else
                    {
                        var p = _graphicsPath.GetLastPoint();
                        _graphicsPath.AddLine(p.X, p.Y, p2.X, p2.Y);
                    }
                }
            }
        }
#else
      internal void UpdateGraphicsPath() {
         if (_graphicsPath == null) 
            _graphicsPath = new GraphicsPath();
         else 
            _graphicsPath.Reset();
         GPoint offset = Overlay.Control.Core.RenderOffset;

         var points = new List<List<PointF>>();
         var actpoints = new List<PointF>();

         if (LocalPoints.Count > 1) {
            GPoint lefttop = new GPoint(-5, -5);
            GPoint rightbottom = new GPoint(Overlay.Control.ClientSize.Width + 5, Overlay.Control.ClientSize.Height + 5);

            var p1 = LocalPoints[0];
            for (int i = 1; i < LocalPoints.Count; i++) {
               var p2 = LocalPoints[i];

               if (localSegmentIsNecessary(p1.X + offset.X,
                                           p1.Y + offset.Y,
                                           p2.X + offset.X,
                                           p2.Y + offset.Y,
                                           lefttop,
                                           rightbottom)) {
                  if (actpoints.Count > 0)
                     actpoints.Add(new PointF(p2.X, p2.Y));
                  else {
                     actpoints.Add(new PointF(p1.X, p1.Y));
                     actpoints.Add(new PointF(p2.X, p2.Y));
                  }
               } else {
                  if (actpoints.Count > 0)
                     points.Add(actpoints);
                  actpoints = new List<PointF>();
               }

               p1 = p2;
            }
            if (actpoints.Count > 0)
               points.Add(actpoints);
         }

         for (int i = 0; i < points.Count; i++) {
            actpoints = points[i];
            if (actpoints.Count > 0) {
               var _partialGraphicsPath = new GraphicsPath();
               _partialGraphicsPath.AddLines(actpoints.ToArray());
               _graphicsPath.AddPath(_partialGraphicsPath, false);
            }
         }
      }

      /* Areas:
       * 
       *  0 | 1  | 2
       * ---+----+---
       *  3 | 4  | 5
       * ---+----+---
       *  6 | 7  | 8
       * 
       */

      /// <summary>
      /// very simple test for visibility of the segment (crossing client area?)
      /// </summary>
      /// <param name="p1x"></param>
      /// <param name="p1y"></param>
      /// <param name="p2x"></param>
      /// <param name="p2y"></param>
      /// <param name="lefttop"></param>
      /// <param name="rightbottom"></param>
      /// <returns></returns>
      bool localSegmentIsNecessary(long p1x, long p1y, long p2x, long p2y, GPoint lefttop, GPoint rightbottom) {
         int area1 = area4localpoint(p1x, p1y, lefttop, rightbottom);
         if (area1 != 4) {  // not in client area
            int area2 = area4localpoint(p2x, p2y, lefttop, rightbottom);
            if (area2 != 4) {
               switch (area1) {
                  case 0:
                     if (area2 == 0 ||
                         area2 == 1 ||
                         area2 == 2 ||
                         area2 == 3 ||
                         area2 == 6)
                        return false;
                     break;

                  case 2:
                     if (area2 == 0 ||
                         area2 == 1 ||
                         area2 == 2 ||
                         area2 == 5 ||
                         area2 == 8)
                        return false;
                     break;

                  case 6:
                     if (area2 == 0 ||
                         area2 == 3 ||
                         area2 == 6 ||
                         area2 == 7 ||
                         area2 == 8)
                        return false;
                     break;

                  case 8:
                     if (area2 == 6 ||
                         area2 == 7 ||
                         area2 == 8 ||
                         area2 == 5 ||
                         area2 == 2)
                        return false;
                     break;

                  case 1:
                     if (area2 == 0 ||
                         area2 == 1 ||
                         area2 == 2)
                        return false;
                     break;

                  case 3:
                     if (area2 == 0 ||
                         area2 == 3 ||
                         area2 == 6)
                        return false;
                     break;

                  case 5:
                     if (area2 == 2 ||
                         area2 == 5 ||
                         area2 == 8)
                        return false;
                     break;

                  case 7:
                     if (area2 == 6 ||
                         area2 == 7 ||
                         area2 == 8)
                        return false;
                     break;
               }
            }
         }
         return true;
      }

      /// <summary>
      /// get actual areanumber for point (0..8) (4 is the lefttop/rightbottom area)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="lefttop"></param>
      /// <param name="rightbottom"></param>
      /// <returns></returns>
      int area4localpoint(long x, long y, GPoint lefttop, GPoint rightbottom) {
         if (x < lefttop.X) {
            if (y < lefttop.Y)
               return 0;
            if (y < rightbottom.Y)
               return 3;
            else
               return 6;
         } else if (x < rightbottom.X) {
            if (y < lefttop.Y)
               return 1;
            if (y < rightbottom.Y)
               return 4;
            else
               return 7;
         } else {
            if (y < lefttop.Y)
               return 2;
            if (y < rightbottom.Y)
               return 5;
            else
               return 8;
         }
      }

#endif

      public virtual void OnRender(Graphics g)
        {
            if (IsVisible)
            {
                if (_graphicsPath != null)
                {
                    g.DrawPath(Stroke, _graphicsPath);
                }
            }
        }

        public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(144, Color.MidnightBlue));

        /// <summary>
        ///     specifies how the outline is painted
        /// </summary>
        [NonSerialized] public Pen Stroke = DefaultStroke;

        public readonly List<GPoint> LocalPoints = new List<GPoint>();

        static GMapRoute()
        {
            DefaultStroke.LineJoin = LineJoin.Round;
            DefaultStroke.Width = 5;
        }

        public GMapRoute(string name)
            : base(name)
        {
        }

        public GMapRoute(IEnumerable<PointLatLng> points, string name)
            : base(points, name)
        {
        }

        public GMapRoute(MapRoute oRoute)
            : base(oRoute)
        {
        }


#region ISerializable Members

        // Temp store for de-serialization.
        private GPoint[] deserializedLocalPoints;

        /// <summary>
        ///     Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the
        ///     target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">
        ///     The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this
        ///     serialization.
        /// </param>
        /// <exception cref="T:System.Security.SecurityException">
        ///     The caller does not have the required permission.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Visible", IsVisible);
            info.AddValue("LocalPoints", LocalPoints.ToArray());
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GMapRoute" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected GMapRoute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            //this.Stroke = Extensions.GetValue<Pen>(info, "Stroke", new Pen(Color.FromArgb(144, Color.MidnightBlue)));
            IsVisible = Extensions.GetStruct(info, "Visible", true);
            deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
        }

#endregion

#region IDeserializationCallback Members

        /// <summary>
        ///     Runs when the entire object graph has been de-serialized.
        /// </summary>
        /// <param name="sender">
        ///     The object that initiated the callback. The functionality for this parameter is not currently
        ///     implemented.
        /// </param>
        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            // Accounts for the de-serialization being breadth first rather than depth first.
            LocalPoints.AddRange(deserializedLocalPoints);
            LocalPoints.Capacity = Points.Count;
        }

#endregion

#region IDisposable Members

        bool _disposed;

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                LocalPoints.Clear();

                if (_graphicsPath != null)
                {
                    _graphicsPath.Dispose();
                    _graphicsPath = null;
                }

                Clear();
            }
        }

#endregion
    }

    public delegate void RouteClick(GMapRoute item, MouseEventArgs e);

    public delegate void RouteDoubleClick(GMapRoute item, MouseEventArgs e);

    public delegate void RouteEnter(GMapRoute item);

    public delegate void RouteLeave(GMapRoute item);
}
