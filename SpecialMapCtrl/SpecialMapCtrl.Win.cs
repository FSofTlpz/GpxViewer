#if !GMAP4SKIA
using GMap.NET;
using GMap.NET.FSofTExtented;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace SpecialMapCtrl {

   public partial class SpecialMapCtrl {

      #region Event-Funktionen für Standard-Windows-Control-Events

      protected override void OnCreateControl() {
         try {
            base.OnCreateControl();

            if (!g_IsDesignerHosted) {
               var f = ParentForm;
               if (f != null) {
                  while (f.ParentForm != null)
                     f = f.ParentForm;

                  if (f != null)
                     f.FormClosing += ParentForg_FormClosing;
               }
            }
         } catch (Exception ex) {
            if (M_ExceptionThrown != null)
               M_ExceptionThrown.Invoke(ex);
            else
               throw;
         }
      }

      void ParentForg_FormClosing(object? sender, FormClosingEventArgs e) {
         //if (e.CloseReason == CloseReason.WindowsShutDown ||
         //    e.CloseReason == CloseReason.TaskManagerClosing)
         g_Manager.CancelTileCaching();
         g_core.Dispose();
      }

      //protected override void OnKeyDown(KeyEventArgs e) => base.OnKeyDown(e);

      //protected override void OnKeyUp(KeyEventArgs e) => base.OnKeyUp(e);

      bool _mouseIn;

      protected override void OnMouseEnter(EventArgs e) {
         base.OnMouseEnter(e);

         if (!g_DisableFocusOnMouseEnter)
            Focus();

         _mouseIn = true;
      }

      protected override void OnMouseLeave(EventArgs e) {
         base.OnMouseLeave(e);
         _mouseIn = false;
      }

      protected override void OnMouseWheel(MouseEventArgs e) {
         base.OnMouseWheel(e);

         if (M_MouseWheelZoomEnabled &&
             _mouseIn &&
             (!M_IsMouseOverMarker || g_IgnoreMarkerOnMouseWheel) &&
             !g_core.IsDragging) {
            if (g_core.MouseLastZoom.X != e.X && g_core.MouseLastZoom.Y != e.Y) {
               switch (M_MouseWheelZoomType) {
                  case MouseWheelZoomType.MousePositionAndCenter:
                  case MouseWheelZoomType.MousePositionWithoutCenter:
                     g_core.SetPositionDirect(fromLocalToLatLng(e.X, e.Y));
                     break;
                  case MouseWheelZoomType.ViewCenter:
                     g_core.SetPositionDirect(fromLocalToLatLng(Width / 2, Height / 2));
                     break;
               }
               g_core.MouseLastZoom = new GPoint(e.X, e.Y);
            }

            // set mouse position to map center
            if (M_MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter) {
               if (!GMaps.Instance.IsRunningOnMono) {
                  var p = PointToScreen(new Point(Width / 2, Height / 2));
                  PublicCore.SetMousePosition(p.X, p.Y);
               }
            }

            g_core.MouseWheelZooming = true;

            if (e.Delta > 0) {
               if (!g_InvertedMouseWheelZooming)
                  M_SetZoom((int)M_Zoom + 1);
               else
                  M_SetZoom((int)(M_Zoom + 0.99) - 1);
            } else if (e.Delta < 0) {
               if (!g_InvertedMouseWheelZooming)
                  M_SetZoom((int)(M_Zoom + 0.99) - 1);
               else
                  M_SetZoom((int)M_Zoom + 1);
            }

            g_core.MouseWheelZooming = false;
         }
      }

      protected override void OnMouseClick(MouseEventArgs e) {
         base.OnMouseClick(e);
         onMouseClick(e, false, false, out _, out _, out _);
      }

      protected override void OnMouseDoubleClick(MouseEventArgs e) {
         base.OnMouseDoubleClick(e);
         onMouseClick(e, true, false, out _, out _, out _);
      }

      protected override void OnMouseDown(MouseEventArgs e) {
         base.OnMouseDown(e);
         onMouseDown(e);
      }

      protected override void OnMouseUp(MouseEventArgs e) {
         base.OnMouseUp(e);
         onMouseUp(e);
      }

      protected override void OnMouseMove(MouseEventArgs e) {
         base.OnMouseMove(e);
         onMouseMove(e);
      }

      protected override void OnSizeChanged(EventArgs e) {
         base.OnSizeChanged(e);
         onSizeChanged(e);
      }

      #endregion

   }
}
#endif
