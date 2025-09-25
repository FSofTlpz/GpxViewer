using GpxViewer.Common;
using GpxViewer.PictureEdit;
using SpecialMapCtrl;
using static GpxViewer.PictureEdit.PictureManager;
using static GpxViewer.PictureEdit.PictureManager.PictureDataListEventArgs;
using Gpx = FSofTUtils.Geography.PoorGpx;

namespace GpxViewer {
   internal class PhotoEdit {

      SpecialMapCtrl.SpecialMapCtrl mapCtrl;
      ReadOnlyGpxControl roCtrl;
      PictureManager pictMng;
      GpxWorkbench gpxWorkbench;

      Action setProgState4Pos;


      public string lastFilename4OnShowExtern { get; protected set; } = string.Empty;

      public PhotoEdit(SpecialMapCtrl.SpecialMapCtrl mapControl,
                       ReadOnlyGpxControl readOnlyGpxControl,
                       PictureManager pictureManager,
                       GpxWorkbench gpxWorkbench,
                       Action setProgState4Pos) {
         mapCtrl = mapControl;
         roCtrl = readOnlyGpxControl;
         pictMng = pictureManager;
         this.gpxWorkbench = gpxWorkbench;
         this.setProgState4Pos = setProgState4Pos;
      }

      #region Setting Geodata for Picture

      Marker? geoTaggingMarker = null;

      TimeSpan offsetPictureTime2GpxTime = new(0);

      bool getGpxLocation4Timestamp(DateTime timestamp, out double lat, out double lon) {
         lat = lon = double.MinValue;

         List<Track> tracks = new();
         tracks.AddRange(roCtrl.GetVisibleTracks());
         if (gpxWorkbench != null)
            tracks.AddRange(gpxWorkbench.VisibleTracks());

         if (tracks.Count == 0) {
            MessageBox.Show("Es sind keine (sichtbaren) Tracks für die Suche vorhanden.",
                            "Achtung",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
            return false;
         }

         // Offset ermitteln (GPX i.A. in UTC, Bilddatum aus EXIF in lokaler Zeit)
         FormOffsetPictureGpx formOffsetPictureGpx = new() { Offset = offsetPictureTime2GpxTime };
         formOffsetPictureGpx.ShowDialog();
         offsetPictureTime2GpxTime = formOffsetPictureGpx.Offset;
         timestamp = timestamp.Subtract(offsetPictureTime2GpxTime);

         Track? srctrack = null;
         foreach (var track in tracks) {
            if (track.IsVisible) {
               Gpx.GpxTrackPoint p1 = track.GpxSegment.Points[0];
               for (int i = 1; i < track.GpxSegment.Points.Count; i++) {
                  Gpx.GpxTrackPoint p2 = track.GpxSegment.Points[i];
                  if (Gpx.BaseElement.ValueIsValid(p1.Time) &&
                      Gpx.BaseElement.ValueIsValid(p2.Time) &&
                      p1.Time <= timestamp && timestamp <= p2.Time) {
                     TimeSpan ts1 = timestamp.Subtract(p1.Time);
                     TimeSpan ts2 = p2.Time.Subtract(p1.Time);
                     double f = ts1.TotalMilliseconds / ts2.TotalMilliseconds;
                     lat = p1.Lat + f * (p2.Lat - p1.Lat);
                     lon = p1.Lon + f * (p2.Lon - p1.Lon);
                     srctrack = track;
                     break;
                  } else
                     p1 = p2;
               }
            }
            if (lat != double.MinValue)
               break;
         }
         if (lat != double.MinValue) {
            if (MessageBox.Show("Sollen die gefundenen Koordinaten aus dem Track \"" + srctrack.VisualName + "\" übernommen werden?",
                                "Koordinaten übernehmen",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
               return true;
            return false;
         }

         MessageBox.Show("Für den Zeitpunkt " + timestamp.ToString("G") + " UTC" + Environment.NewLine + "wurden keine passenden GPX-Daten gefunden.",
                         "Achtung",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Information);
         return false;
      }

      public void setFotoPosition(double lon, double lat) {
         if (geoTaggingMarker != null)
            pictMng.SetPositionExtern(geoTaggingMarker.Text, lon, lat);
      }

      public void removeGeoTaggingMarker(string filename) {
         if (geoTaggingMarker != null) {
            //ev. über geoTaggingMarker.Text==filename filtern

            mapCtrl.M_ShowMarker(geoTaggingMarker, false);
            geoTaggingMarker = null;
         }
      }

      async void showGeoTaggingMarker(object? sender, PictureDataEventArgs e) {
         //Debug.WriteLine("!!! OnShowExtern: " + e);
         removeGeoTaggingMarker(e.Filename);    // falls noch einer angezeigt wird
         geoTaggingMarker = new Marker(new Gpx.GpxWaypoint(double.IsNaN(e.Longitude) ? mapCtrl.M_CenterLon : e.Longitude,
                                                           double.IsNaN(e.Latitude) ? mapCtrl.M_CenterLon : e.Latitude),
                                       Marker.MarkerType.GeoTagging,
                                       null) {
            Text = e.Filename
         };
         ProgCore.ShowMarker(geoTaggingMarker, true, mapCtrl, gpxWorkbench, roCtrl);
         if (geoTaggingMarker.Longitude != mapCtrl.M_CenterLon ||
             geoTaggingMarker.Latitude != mapCtrl.M_CenterLat)
            await mapCtrl.M_SetLocationAndZoomAsync(ProgCore.GetMapZoomTS(mapCtrl), geoTaggingMarker.Longitude, geoTaggingMarker.Latitude);
      }

      #endregion


      /// <summary>
      /// Die Auswahl von (Bild-)Dateien wurde beendet.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void PictureManagerDeselectPictures(object? sender, PictureDataListEventArgs e) {
         foreach (PictureData pd in e.PictureDatas) {
            if (!string.IsNullOrEmpty(lastFilename4OnShowExtern) &&
                pd.Filename == lastFilename4OnShowExtern) {
               // Die externe Anzeige von (Bild-)Dateien sollte beendet werden.
               removeGeoTaggingMarker(pd.Filename);
               break;
            }
         }
      }

      /// <summary>
      /// Für die (Bild-)Datei werden neue Geodaten gewünscht.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void PictureManagerNeedNewData(object? sender, PictureDataEventArgs e) {
         if ((lastFilename4OnShowExtern == null ||
              lastFilename4OnShowExtern != e.Filename) &&
              e.Latitude != double.MinValue &&
              e.Longitude != double.MinValue)
            PictureManagerShowExtern(sender, e);
         // Für die (Bild-)Dateien werden neue Geodaten gewünscht.
         if (e.Latitude != double.MinValue &&
             e.Latitude != double.MinValue)
            setProgState4Pos();
         //formMain.programState = ProgState.Set_PicturePosition;
         else {
            if (getGpxLocation4Timestamp(e.Timestamp, out double lat, out double lon))
               pictMng.SetPositionExtern(e.Filename, lon, lat);
         }
      }

      /// <summary>
      /// Eine (Bild-)Datei sollte extern angezeigt werden.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void PictureManagerShowExtern(object? sender, PictureDataEventArgs e) {
         if (!string.IsNullOrEmpty(lastFilename4OnShowExtern))
            //   OnHideExtern?.Invoke(this, new PictureDataEventArgs(lastFilename4OnShowExtern, 0, 0));
            removeGeoTaggingMarker(lastFilename4OnShowExtern);

         lastFilename4OnShowExtern = e.Filename;

         // (Bild-)Dateien sollte extern angezeigt werden.
         showGeoTaggingMarker(this, e);
      }

   }
}
