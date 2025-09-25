using SpecialMapCtrl.NET.ObjectModel;
using System;
using System.Drawing;

namespace SpecialMapCtrl {

   /// <summary>
   /// overlay
   /// </summary>
   public class MapOverlay : IDisposable {

      bool _isVisibile = true;

      /// <summary>
      /// is overlay visible
      /// </summary>
      public bool IsVisibile {
         get => _isVisibile;
         set {
            if (value != _isVisibile) {
               _isVisibile = value;

               if (Control != null) {
                  if (_isVisibile) {
                     Control.M_HoldInvalidation = true;
                     ForceUpdate();
                     Control.M_Refresh();
                  } else {
                     if (Control.M_IsMouseOverMarker)
                        Control.M_IsMouseOverMarker = false;

                     if (Control.M_IsMouseOverPolygon)
                        Control.M_IsMouseOverPolygon = false;

                     if (Control.M_IsMouseOverTrack)
                        Control.M_IsMouseOverTrack = false;

                     Control.M_RestoreCursorOnLeave();

                     if (!Control.M_HoldInvalidation)
                        Control.M_CoreInvalidate();
                  }
               }
            }
         }
      }

      bool _isHitTestVisible = true;

      /// <summary>
      /// HitTest visibility for entire overlay
      /// </summary>
      public bool IsHitTestVisible {
         get => _isHitTestVisible;
         set => _isHitTestVisible = value;
      }

      bool _isZoomSignificant = true;

      /// <summary>
      /// if false don't consider contained objects when box zooming
      /// </summary>
      public bool IsZoomSignificant {
         get => _isZoomSignificant;
         set => _isZoomSignificant = value;
      }

      /// <summary>
      /// overlay Id
      /// </summary>
      public string Id = string.Empty;

      /// <summary>
      /// list of markers, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<MapMarker> Markers = new ObservableCollectionThreadSafe<MapMarker>();

      /// <summary>
      /// list of routes, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<MapTrack> Tracks = new ObservableCollectionThreadSafe<MapTrack>();

      /// <summary>
      /// list of polygons, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<MapPolygon> Polygons = new ObservableCollectionThreadSafe<MapPolygon>();

      SpecialMapCtrl? _control;

      public SpecialMapCtrl? Control {
         get => _control;
         internal set => _control = value;
      }


      public MapOverlay() {
         CreateEvents();
      }

      public MapOverlay(string id) {
         Id = id;
         CreateEvents();
      }

      void CreateEvents() {
         Markers.CollectionChanged += Markers_CollectionChanged;
         Tracks.CollectionChanged += Tracks_CollectionChanged;
         Polygons.CollectionChanged += Polygons_CollectionChanged;
      }

      void ClearEvents() {
         Markers.CollectionChanged -= Markers_CollectionChanged;
         Tracks.CollectionChanged -= Tracks_CollectionChanged;
         Polygons.CollectionChanged -= Polygons_CollectionChanged;
      }

      public void Clear() {
         Markers.Clear();
         Tracks.Clear();
         Polygons.Clear();
      }

      void Polygons_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
         if (e.NewItems != null) {
            foreach (MapPolygon obj in e.NewItems) {
               if (obj != null) {
                  obj.Overlay = this;
                  if (Control != null)
                     Control.M_UpdatePolygonLocalPosition(obj);
               }
            }
         }

         if (Control != null) {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) {
               if (Control.M_IsMouseOverPolygon) {
                  Control.M_IsMouseOverPolygon = false;
                  Control.M_RestoreCursorOnLeave();
               }
            }

            if (!Control.M_HoldInvalidation)
               Control.M_CoreInvalidate();
         }
      }

      void Tracks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
         if (e.NewItems != null) {
            foreach (MapTrack obj in e.NewItems) {
               if (obj != null) {
                  obj.Overlay = this;
                  if (Control != null)
                     Control.M_UpdateTrackLocalPosition(obj);
               }
            }
         }

         if (Control != null) {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) {
               if (Control.M_IsMouseOverTrack) {
                  Control.M_IsMouseOverTrack = false;
                  Control.M_RestoreCursorOnLeave();
               }
            }

            if (!Control.M_HoldInvalidation)
               Control.M_CoreInvalidate();
         }
      }

      void Markers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
         if (e.NewItems != null) {
            foreach (MapMarker obj in e.NewItems) {
               if (obj != null) {
                  obj.Overlay = this;
                  if (Control != null)
                     Control.M_UpdateMarkerLocalPosition(obj);
               }
            }
         }

         if (Control != null) {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) {
               if (Control.M_IsMouseOverMarker) {
                  Control.M_IsMouseOverMarker = false;
                  Control.M_RestoreCursorOnLeave();
               }
            }

            if (!Control.M_HoldInvalidation)
               Control.M_CoreInvalidate();
         }
      }

      /// <summary>
      ///     updates local positions of objects
      /// </summary>
      internal void ForceUpdate() {
         if (Control != null) {
            foreach (var obj in Markers) {
               if (obj.IsVisible)
                  Control.M_UpdateMarkerLocalPosition(obj);
            }

            foreach (var obj in Polygons) {
               if (obj.IsVisible)
                  Control.M_UpdatePolygonLocalPosition(obj);
            }

            foreach (var obj in Tracks) {
               if (obj.IsVisible)
                  Control.M_UpdateTrackLocalPosition(obj);
            }
         }
      }

      /// <summary>
      /// renders objects/routes/polygons
      /// </summary>
      /// <param name="g"></param>
      public virtual void OnRender(Graphics g) {
         if (Control != null) {
            if (Control.M_TracksEnabled)
               foreach (var r in Tracks)
                  if (r.IsVisible)
                     r.OnRender(g);

            if (Control.M_PolygonsEnabled)
               foreach (var r in Polygons)
                  if (r.IsVisible)
                     r.OnRender(g);

            if (Control.M_MarkersEnabled)
               foreach (var m in Markers)
                  if (m.IsVisible || m.DisableRegionCheck)
                     m.OnRender(g);
         }
      }

      public virtual void OnRenderToolTips(Graphics g) {
         if (Control != null) {
            if (Control.M_MarkersEnabled) {
               foreach (var m in Markers)
                  if (m.ToolTip != null &&
                     m.IsVisible)
                     if (!string.IsNullOrEmpty(m.ToolTipText) &&
                         (m.ToolTipMode == MapMarker.MarkerTooltipMode.Always ||
                          m.ToolTipMode == MapMarker.MarkerTooltipMode.OnMouseOver && m.IsMouseOver))
                        m.ToolTip.OnRender(g);
            }
         }
      }

      #region IDisposable Members

      bool _disposed;

      public void Dispose() {
         if (!_disposed) {
            _disposed = true;

            ClearEvents();

            foreach (var m in Markers)
               m.Dispose();

            foreach (var r in Tracks)
               r.Dispose();

            foreach (var p in Polygons)
               p.Dispose();

            Clear();
         }
      }

      #endregion
   }
}
