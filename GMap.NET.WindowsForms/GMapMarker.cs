using System;
using System.Drawing;
using System.Runtime.Serialization;

#if !GMAP4SKIA
namespace GMap.NET.WindowsForms {
#else
namespace GMap.NET.Skia {
#endif
   /// <summary>
   /// GMap.NET marker
   /// </summary>
   [Serializable]
   public abstract class GMapMarker : IDisposable {

      /// <summary>
      /// mode of tooltip
      /// </summary>
      public enum MarkerTooltipMode {
         OnMouseOver,
         Never,
         Always,
      }


      GMapOverlay _overlay;

      /// <summary>
      /// <see cref="GMapOverlay"/> dem der <see cref="GMapMarker"/> zugeordnet ist oder null
      /// </summary>
      public GMapOverlay Overlay {
         get => _overlay;
         internal set => _overlay = value;
      }

      private PointLatLng _position;

      /// <summary>
      /// geografische Position
      /// </summary>
      public PointLatLng Position {
         get => _position;
         set {
            if (_position != value) {
               _position = value;

               if (IsVisible &&
                   Overlay != null && Overlay.Control != null)
                  Overlay.Control.Map_UpdateMarkerLocalPosition(this);
            }
         }
      }

      Rectangle _area;

      /// <summary>
      /// lokaler Bereich (in client-Koordinaten) auf den z.B. ein Click als Click auf den <see cref="GMapMarker"/> interpretiert wird
      /// <para>Die Location ist die <see cref="LocalPosition"/>.</para>
      /// </summary>
      public Rectangle LocalArea {
         get => _area;
      }

      /// <summary>
      /// lokale Position (in client-Koordinaten) entsprechend Position (== Location von <see cref="LocalArea"/>)
      /// </summary>
      public Point LocalPosition {
         get => _area.Location;
         set {
            if (_area.Location != value) {
               _area.Location = value;
               if (Overlay != null &&
                   Overlay.Control != null &&
                   !Overlay.Control.HoldInvalidation)
                  Overlay.Control.Map_CoreInvalidate();
            }
         }
      }

      /// <summary>
      /// Größe des lokalen Bereiches <see cref="LocalArea"/>
      /// </summary>
      public Size LocalSize {
         get => _area.Size;
         set => _area.Size = value;
      }

      Point _offset;

      /// <summary>
      /// Offset zur <see cref="LocalPosition"/> für die Darstellung
      /// </summary>
      public Point LocalOffset {
         get => _offset;
         set {
            if (_offset != value) {
               _offset = value;

               if (IsVisible &&
                   Overlay != null &&
                   Overlay.Control != null)
                  Overlay.Control.Map_UpdateMarkerLocalPosition(this);
            }
         }
      }


      /// <summary>
      /// lokale Position (in client-Koordinaten) des ToolTip (i.A. <see cref="LocalArea"/>.Location - <see cref="LocalOffset"/>)
      /// </summary>
      public Point LocalToolTipPosition => new Point(LocalPosition.X + LocalOffset.X,
                                                     LocalPosition.Y + LocalOffset.Y);

      public GMapToolTip ToolTip;

      public MarkerTooltipMode ToolTipMode = MarkerTooltipMode.OnMouseOver;

      string _toolTipText;

      public string ToolTipText {
         get => _toolTipText;
         set {
            if (ToolTip == null &&
                !string.IsNullOrEmpty(value))
               ToolTip = new ToolTips.GMapRoundedToolTip(this);
            _toolTipText = value;
         }
      }


      private bool _visible = true;

      /// <summary>
      /// is marker visible
      /// </summary>
      public bool IsVisible {
         get => _visible;
         set {
            if (value != _visible) {
               _visible = value;

               if (Overlay != null &&
                   Overlay.Control != null) {
                  if (_visible)
                     Overlay.Control.Map_UpdateMarkerLocalPosition(this);
                  else
                     if (Overlay.Control.Map_IsMouseOverMarker) {
                     Overlay.Control.Map_IsMouseOverMarker = false;
                     Overlay.Control.RestoreCursorOnLeave();
                  }

                  if (!Overlay.Control.HoldInvalidation)
                     Overlay.Control.Map_CoreInvalidate();
               }
            }
         }
      }

      /// <summary>
      /// if true, marker will be rendered even if it's outside current view
      /// </summary>
      public bool DisableRegionCheck;

      /// <summary>
      /// can maker receive input
      /// </summary>
      public bool IsHitTestVisible = true;

      private bool _isMouseOver;

      /// <summary>
      /// is mouse over marker
      /// </summary>
      public bool IsMouseOver {
         get => _isMouseOver;
         internal set => _isMouseOver = value;
      }


      public GMapMarker(PointLatLng pos) {
         Position = pos;
      }

      public virtual void OnRender(Graphics g) { }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public virtual void Dispose() {
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
               if (ToolTip != null) {
                  _toolTipText = null;
                  ToolTip.Dispose();
                  ToolTip = null;
               }
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}

