using GMap.NET;
using System;
using System.Drawing;

#if !GMAP4SKIA
using System.Runtime.Serialization;
#endif

namespace SpecialMapCtrl {

   /// <summary>
   /// marker
   /// </summary>
   public abstract class MapMarker : IDisposable {

      /// <summary>
      /// mode of tooltip
      /// </summary>
      public enum MarkerTooltipMode {
         OnMouseOver,
         Never,
         Always,
      }


      MapOverlay? _overlay;

      /// <summary>
      /// <see cref="MapOverlay"/> dem der <see cref="MapMarker"/> zugeordnet ist oder null
      /// </summary>
      public MapOverlay? Overlay {
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
                  Overlay.Control.M_UpdateMarkerLocalPosition(this);
            }
         }
      }

      Rectangle _activearea;

      /// <summary>
      /// lokaler Bereich (in client-Koordinaten) auf den z.B. ein Click als Click auf den <see cref="MapMarker"/> interpretiert wird
      /// (i.A. ist das auch die Bildfläche)
      /// <para>Die Location ist die <see cref="ActiveClientPosition"/>.</para>
      /// </summary>
      public Rectangle ActiveClientArea {
         get => _activearea;
      }

      /// <summary>
      /// lokale Position (in client-Koordinaten) entsprechend Position (== Location von <see cref="ActiveClientArea"/>)
      /// </summary>
      public Point ActiveClientPosition {
         get => _activearea.Location;
         set {
            //if (_activearea.Location != value) {
            //   _activearea.X = value.X;
            //   _activearea.Y = value.Y;
            //   _activearea.Location = value;
            //   if (Overlay != null &&
            //       Overlay.Control != null &&
            //       !Overlay.Control.HoldInvalidation)
            //      Overlay.Control.Map_CoreInvalidate();
            //}
            SetActiveClientPosition(value.X, value.Y);
         }
      }

      public void SetActiveClientPosition(int xclient, int yclient) {
         if (_activearea.X != xclient ||
             _activearea.Y != yclient) {
            _activearea.X = xclient;
            _activearea.Y = yclient;
            if (Overlay != null &&
                Overlay.Control != null &&
                !Overlay.Control.M_HoldInvalidation)
               Overlay.Control.M_CoreInvalidate();
         }
      }

      /// <summary>
      /// Größe des lokalen Bereiches (in client-Koordinaten) <see cref="ActiveClientArea"/>
      /// </summary>
      public Size ActiveClientSize {
         get => _activearea.Size;
         set => _activearea.Size = value;
      }

      Point _offset;

      /// <summary>
      /// Offset zur <see cref="ActiveClientPosition"/> für die Darstellung
      /// </summary>
      public Point LocalOffset {
         get => _offset;
         set {
            if (_offset != value) {
               _offset = value;

               if (IsVisible &&
                   Overlay != null &&
                   Overlay.Control != null)
                  Overlay.Control.M_UpdateMarkerLocalPosition(this);
            }
         }
      }


      /// <summary>
      /// lokale Position (in client-Koordinaten) des ToolTip (i.A. <see cref="ActiveClientArea"/>.Location - <see cref="LocalOffset"/>)
      /// </summary>
      public Point LocalToolTipPosition => IsOnClientVisible ? new Point(ActiveClientPosition.X + ActiveClientSize.Width / 2,
                                                                         ActiveClientPosition.Y + ActiveClientSize.Height / 2) :
                                                               new Point(0, 0);

      public MapToolTip? ToolTip;

      public MarkerTooltipMode ToolTipMode = MarkerTooltipMode.OnMouseOver;

      string _toolTipText = string.Empty;

      public string ToolTipText {
         get => _toolTipText;
         set {
            if (ToolTip == null &&
                !string.IsNullOrEmpty(value))
               ToolTip = new ToolTips.MapRoundedToolTip(this);
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
                     Overlay.Control.M_UpdateMarkerLocalPosition(this);
                  else
                     if (Overlay.Control.M_IsMouseOverMarker) {
                     Overlay.Control.M_IsMouseOverMarker = false;
                     Overlay.Control.M_RestoreCursorOnLeave();
                  }

                  if (!Overlay.Control.M_HoldInvalidation)
                     Overlay.Control.M_CoreInvalidate();
               }
            }
         }
      }

      /// <summary>
      /// actual visible on client
      /// </summary>
      public bool IsOnClientVisible => int.MinValue < ActiveClientPosition.X && int.MinValue < ActiveClientPosition.Y;

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


      public MapMarker(PointLatLng pos) {
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

