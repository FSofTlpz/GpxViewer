//#define ONLY_PRINT_PREVIEW       // zum Testen der Druckerausgabe 'True'

using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geography.PoorGpx;
using FSofTUtils.Geometry;
using FSofTUtils.Threading;
using GMap.NET.FSofTExtented.MapProviders;
using GpxViewer.Common;
using SpecialMapCtrl;
using System.Drawing.Printing;
using System.Text;
using Gpx = FSofTUtils.Geography.PoorGpx;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace GpxViewer {
   /// <summary>
   /// Programmfunktionen die außer über <see cref="UIHelper"/> nicht auf die spez. UI zugreifen
   /// <para>
   /// Zusätzlich werden z.T. Objekte vom Typ <see cref="GpxWorkbench"/>,  <see cref="SpecialMapCtrl.SpecialMapCtrl"/>,
   /// <see cref="ReadOnlyGpxControl"/> und <see cref="EditableGpxControl"/> 
   /// (<see cref="Config"/>, <see cref="AppData"/>) 
   /// benötigt.
   /// </para>
   /// </summary>
   public class ProgCore {

      /// <summary>
      /// falls eine Garmin-Karte angezeigt wird, werden Infos für Garmin-Objekte im Bereich des Client-Punktes ermittelt
      /// sonst für OSM-Objekte
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="config"></param>
      /// <param name="mapCtrl"></param>
      /// <returns></returns>
      internal static async Task<(List<GarminImageCreator.SearchObject>?, List<string>?)> GetObjectinfoAsync(
               Point ptclient,
               Config? config,
               MapCtrl mapCtrl) {
         bool isGarmin = mapCtrl.M_ProviderDefinitions[mapCtrl.M_GetActiveProviderIdx()].Provider is GarminProvider;
         List<GarminImageCreator.SearchObject>? garmininfo = null;
         List<string>? stdinfo = null;

         if (isGarmin) {
            int delta = (Math.Min(mapCtrl.ClientSize.Height, mapCtrl.ClientSize.Width) * config.DeltaPercent4Search) / 100;
            garmininfo = mapCtrl.M_GetGarminObjectInfos(ptclient, delta, delta);

         } else {
            stdinfo = [];
            PointD pt = mapCtrl.M_Client2LonLat(ptclient);
            GeoCodingReverseResultOsm[] result = await GeoCodingReverseResultOsm.GetAsync(pt.X, pt.Y);
            foreach (GeoCodingReverseResultOsm item in result)
               stdinfo.Add(item.Name);
         }
         return (garmininfo, stdinfo);
      }

      #region Zoom

      public static double GetMapZoomTS(MapCtrl mapCtrl) {
         object? obj = ThreadsafeInvoker.InvokeControlPropertyReader(mapCtrl, nameof(mapCtrl.M_Zoom));
         return obj == null ? 0 : (double)obj;
      }

      public static void SetMapZoomTS(MapCtrl mapCtrl, double zoom) {
         zoom = Math.Min(mapCtrl.M_MaxZoom, Math.Max(mapCtrl.M_MinZoom, zoom));
         ThreadsafeInvoker.InvokeControlMethodCall(mapCtrl, nameof(mapCtrl.M_SetZoomAsync), [zoom]);
      }

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <returns></returns>
      static int showWorkbenchMarker(Marker marker, bool visible, MapCtrl mapCtrl, GpxWorkbench? gpxWorkbench) {
         if (marker.IsVisible == visible)
            return -1;
         mapCtrl.M_ShowMarker(marker,
                               visible,
                               visible ? gpxWorkbench?.Gpx.NextVisibleMarker(marker) : null);
         //editableTracklistControl1.ShowMarker(editablemarker, visible);
         GpxData? gpx = marker.GpxDataContainer;
         if (gpx != null) {
            gpx.GpxDataChanged = true;
            return gpx.MarkerIndex(marker);
         }
         return -1;
      }

      /// <summary>
      /// den Zoom und den Mittelpunkt der Karte setzen
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="mapCtrl"></param>
      public static async Task SetMapLocationAndZoomAsync(double zoom, double lon, double lat, MapCtrl mapCtrl) =>
        await mapCtrl.M_SetLocationAndZoomAsync(zoom, lon, lat);

      /// <summary>
      /// liefert den aktuellen Zoom und Mittelpunkt der Karte
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="mapCtrl"></param>
      /// <returns>Zoom</returns>
      public static double GetMapLocationAndZoom(out double lon, out double lat, MapCtrl mapCtrl) {
         lon = mapCtrl.M_CenterLon;
         lat = mapCtrl.M_CenterLat;
         return GetMapZoomTS(mapCtrl);
      }

      /// <summary>
      /// zoomt auf die Tracks der Liste
      /// </summary>
      /// <param name="tracklst"></param>
      /// <param name="mapCtrl"></param>
      /// <returns></returns>
      public static async Task ZoomToTracksAsync(IList<Track> tracklst, MapCtrl mapCtrl) {
         if (tracklst != null &&
             tracklst.Count > 0) {
            Gpx.GpxBounds? bounds = tracklst[0].Bounds;
            if (bounds != null) {
               for (int i = 1; i < tracklst.Count; i++)
                  if (tracklst[i].Bounds != null)
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                     bounds.Union(tracklst[i].Bounds);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
               await mapCtrl.M_ZoomToRangeAsync(new PointD(bounds.MinLon, bounds.MaxLat), new PointD(bounds.MaxLon, bounds.MinLat), false);
            }
         }
      }

      /// <summary>
      /// zoomt auf die Marker der Liste
      /// </summary>
      /// <param name="markerlst"></param>
      /// <param name="mapCtrl"></param>
      public static async Task ZoomToMarkersAsync(List<Marker> markerlst, MapCtrl mapCtrl) {
         if (markerlst != null &&
             markerlst.Count > 0) {
            GpxBounds bounds = new();
            foreach (var item in markerlst) {
               bounds.Union(item.Waypoint);
            }
            await mapCtrl.M_ZoomToRangeAsync(new PointD(bounds.MinLon, bounds.MaxLat),
                                                  new PointD(bounds.MaxLon, bounds.MinLat),
                                                  false);
         }
      }

      #endregion

      #region Kartenbereich anzeigen

      /// <summary>
      /// neuen Marker in der Workbench an dieser Position setzen
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="name"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      static void insertNewWorkbenchMarker(
               double lon,
               double lat,
               string? name,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench) {
         if (!string.IsNullOrEmpty(name)) {
            Marker? marker = gpxWorkbench?.MarkerInsertCopy(new Marker(new Gpx.GpxWaypoint(lon, lat) { Name = name },
                                                                       Marker.MarkerType.EditableStandard,
                                                                       string.Empty));
            if (marker != null)
               mapCtrl.M_ShowMarker(marker, true);
         }
      }

      /// <summary>
      /// zur Kartenposition gehen und ev. Marker setzen
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="name"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <returns></returns>
      public static async Task GotoPointAndSetNewWorkbenchMarker(
               double lon,
               double lat,
               string? name,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench) {
         await SetMapLocationAndZoomAsync(GetMapZoomTS(mapCtrl), lon, lat, mapCtrl);
         insertNewWorkbenchMarker(lon, lat, name, mapCtrl, gpxWorkbench);
      }

      /// <summary>
      /// zum Kartenbereich gehen und ev. Marker setzen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="name"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <returns></returns>
      public static async Task GotoMapareaAndSetNewWorkbenchMarker(
               PointD topleft,
               PointD bottomright,
               double lon,
               double lat,
               string? name,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench) {
         await mapCtrl.M_ZoomToRangeAsync(topleft, bottomright, false);
         insertNewWorkbenchMarker(lon, lat, name, mapCtrl, gpxWorkbench);
      }

      #endregion

      /// <summary>
      /// setzt die gewünschte Karte, den Zoom und die Position
      /// </summary>
      /// <param name="mapidx">Index für die <see cref="SpecialMapCtrl.SpecialMapCtrl.SpecMapProviderDefinitions"/></param>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="dem"></param>
      /// <param name="providxpaths"></param>
      /// <param name="zoom4display"></param>
      /// <param name="mapCtrl"></param>
      /// <returns></returns>
      public static async Task SetProviderWithZoomPositionAsync(
               int mapidx,
               double zoom,
               double lon,
               double lat,
               DemData? dem,
               List<int[]> providxpaths,
               double zoom4display,
               MapCtrl mapCtrl) {
         try {
            // Zoom und Pos. einstellen
            if (zoom != GetMapZoomTS(mapCtrl) ||
                lon != mapCtrl.M_CenterLon ||
                lat != mapCtrl.M_CenterLat)
               await mapCtrl.M_SetLocationAndZoomAsync(zoom, lon, lat);

            if (0 <= mapidx &&
                mapidx < providxpaths.Count &&
                mapidx != mapCtrl.M_CenterLon) {     // andere Karte anzeigen
               bool hillshade = false;
               byte hillshadealpha = 0;
               bool hillshadeisactiv = dem != null ?
                                          GetMapZoomTS(mapCtrl) >= dem.MinimalZoom :
                                          false;

               if (0 <= mapidx) {
                  mapCtrl.M_ClearWaitingTaskList();

                  MapProviderDefinition mapProviderDefinition = mapCtrl.M_ProviderDefinitions[mapidx];
                  if (mapProviderDefinition.ProviderName == GarminProvider.Instance.Name) {
                     mapCtrl.M_CancelTileBuilds();
                     hillshadealpha = (mapProviderDefinition as GarminProvider.GarminMapDefinition).HillShadingAlpha;
                     hillshade = (mapProviderDefinition as GarminProvider.GarminMapDefinition).HillShading;
                  } else if (mapProviderDefinition.ProviderName == GarminKmzProvider.Instance.Name) {
                     mapCtrl.M_CancelTileBuilds();
                  }
               }
               if (dem != null) {
                  dem.WithHillshade = hillshade;
                  dem.IsActiv = hillshadeisactiv;
               }
               await mapCtrl.M_SetActivProviderAsync(mapidx, hillshadealpha, dem, zoom4display);
            }
         } catch (Exception ex) {
            UIHelper.ShowExceptionMessage(null, "Fehler bei " + nameof(SetProviderWithZoomPositionAsync), ex, null, false);
         }
      }

      /// <summary>
      /// das Laden der Karte abbrechen
      /// </summary>
      /// <param name="mapCtrl"></param>
      public static void CancelMapLoading(MapCtrl mapCtrl) {
         if (mapCtrl is null)
            throw new ArgumentNullException(nameof(mapCtrl));

         mapCtrl.M_ClearWaitingTaskList();
         mapCtrl.M_CancelTileBuilds();
      }

      /// <summary>
      /// Teil eines Tracks besonders darstellen
      /// </summary>
      /// <param name="track"></param>
      /// <param name="ptidx"></param>
      /// <param name="mapCtrl"></param>
      public static void ShowSelectedPart4Track(Track track, List<int> ptidx, MapCtrl mapCtrl) {
         mapCtrl.M_ShowSelectedParts(track, null);
         mapCtrl.M_ShowSelectedParts(track, ptidx);
      }

      #region Infos zu Tracks/Marker anzeigen bzw. liefern

      /// <summary>
      /// liefert Infos zum nächstliegenden zum Point liegenden <see cref="Track"/>-Punkt des <see cref="Track"/>
      /// </summary>
      /// <param name="track"></param>
      /// <param name="ptclient"></param>
      /// <param name="mapCtrl"></param>
      /// <returns></returns>
      public static string GetTrackPointInfo(Track track, Point ptclient, MapCtrl mapCtrl) {
         int idx = track.GetNearestPtIdx(mapCtrl.M_Client2LonLat(ptclient));
         if (idx >= 0) {
            Gpx.GpxTrackPoint? pt = track.GetGpxPoint(idx);
            if (pt != null) {
               StringBuilder sb = new();

               sb.AppendFormat("nächstliegender Trackpunkt:");
               sb.AppendLine();
               sb.AppendFormat("Lng {0:F6}°, Lat {1:F6}°", pt.Lon, pt.Lat);
               if (pt.Elevation != Gpx.BaseElement.NOTVALID_DOUBLE)
                  sb.AppendFormat(", Höhe {0:F0} m", pt.Elevation);
               sb.AppendLine();
               sb.AppendLine();
               double length = track.Length(0, idx);
               sb.AppendFormat("Streckenlänge bis zum Punkt: {0:F1} km ({1:F0} m)", length / 1000, length);
               sb.AppendLine();
               if (pt.Time != Gpx.BaseElement.NOTVALID_TIME) {
                  sb.AppendLine(pt.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
               }
               sb.AppendLine();

               sb.AppendFormat("Gesamtlänge: {0:F1} km ({1:F0} m)", track.StatLength / 1000, track.StatLength);
               sb.AppendLine();
               if (track.StatMinDateTimeIdx >= 0 &&
                   track.StatMaxDateTimeIdx > track.StatMinDateTimeIdx) {
                  TimeSpan ts = track.StatMaxDateTime.Subtract(track.StatMinDateTime);
                  sb.AppendFormat("Gesamt Datum/Zeit: {0} .. {1} (Dauer: {2} Stunden)",
                                  track.StatMinDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                                  track.StatMaxDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                                  ts.ToString(@"h\:mm\:ss"));
                  sb.AppendLine();
                  sb.AppendFormat("Durchschnittsgeschwindigkeit: {0:F1} km/h", track.StatLengthWithTime / ts.TotalSeconds * 3.6);
                  sb.AppendLine();
               }

               return sb.ToString();
            }
         }
         return string.Empty;
      }

      /// <summary>
      /// liefert die Infos zu einem Standard-Marker
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      public static string GetStdMarkerInfo(Marker marker) {
         StringBuilder sb = new();
         sb.AppendFormat("Lng {0:F6}°, Lat {1:F6}°", marker.Longitude, marker.Latitude);
         sb.AppendLine();
         if (marker.Elevation != BaseElement.NOTVALID_DOUBLE) {
            sb.AppendFormat("Höhe {0:F0} m", marker.Elevation);
            sb.AppendLine();
         }
         if (marker.Waypoint.Time != BaseElement.NOTVALID_TIME)
            sb.AppendLine(marker.Waypoint.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
         return sb.ToString();
      }

      static string getMiniMarkerInfo(Marker? marker) {
         StringBuilder sb = new();
         if (marker != null) {
            sb.AppendLine(marker.Text);
            sb.AppendFormat("Länge {0:F6}°, Breite {1:F6}°", marker.Latitude, marker.Longitude);
            sb.AppendLine();
            if (marker.Elevation != BaseElement.NOTUSE_DOUBLE)
               sb.AppendLine(string.Format("Höhe {0}m", marker.Elevation));
            if (!string.IsNullOrEmpty(marker.Waypoint.Description))
               sb.AppendLine(marker.Waypoint.Description);
            if (!string.IsNullOrEmpty(marker.Waypoint.Comment))
               sb.AppendLine(marker.Waypoint.Comment);
            if (marker.Waypoint.Time != BaseElement.NOTVALID_TIME)
               sb.AppendLine(marker.Waypoint.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
            return sb.ToString();
         }
         return string.Empty;
      }

      static string getMiniTrackInfo(Track? track) {
         if (track != null/* && track.IsVisible*/)
            return track.GetSimpleStatsText();
         return string.Empty;
      }

      static string getMiniTrackInfo(IList<Track> tracks) {
         string info = string.Empty;
         if (tracks != null && tracks.Count > 0) {
            double len = 0;
            int pts = 0;
            foreach (Track track in tracks) {
               len += track.Length();
               pts += track.GpxSegment.Points.Count;
            }
            info = string.Format("{0} Punkte, {1:F1} km ({2:F0} m)",
                                 pts,
                                 len / 1000,
                                 len);
         }
         return info;
      }

      /// <summary>
      /// liefert Mini-Info für einen RO-Track
      /// </summary>
      /// <param name="track"></param>
      /// <param name="roCtrl"></param>
      public static void ShowMiniReadonlyTrackInfo(Track? track, ReadOnlyGpxControl roCtrl) =>
         ShowMiniReadonlyTrackInfo(track != null ?
                                             [track] :
                                             Array.Empty<Track>(),
                                   roCtrl);

      /// <summary>
      /// liefert Mini-Info für RO-Tracks
      /// </summary>
      /// <param name="tracks"></param>
      /// <param name="roCtrl"></param>
      public static void ShowMiniReadonlyTrackInfo(IList<Track> tracks, ReadOnlyGpxControl roCtrl) {
         roCtrl.InfotextClear();
         string info = string.Empty;
         if (tracks != null && tracks.Count > 0)
            info = getMiniTrackInfo(tracks);
         roCtrl.InfoTextAppend(info);
      }

      static void showMiniEditableTrackInfo(EditableGpxControl rwCtrl) =>
        ShowMiniEditableTrackInfo(rwCtrl.SelectedTrack, rwCtrl);

      /// <summary>
      /// liefert Mini-Info für RW-Tracks
      /// </summary>
      /// <param name="track"></param>
      /// <param name="rwCtrl"></param>
      public static void ShowMiniEditableTrackInfo(Track? track, EditableGpxControl rwCtrl) {
         rwCtrl.InfotextClear();
         if (track != null/* && track.IsVisible*/)
            rwCtrl.InfoTextAppend(getMiniTrackInfo(track));
      }

      /// <summary>
      /// liefert Mini-Info für RW-Marker
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="rwCtrl"></param>
      public static void ShowMiniEditableMarkerInfo(Marker? marker, EditableGpxControl rwCtrl) {
         rwCtrl.InfotextClear();
         if (marker != null)
            rwCtrl.InfoTextAppend(getMiniMarkerInfo(marker));
      }

      #endregion

      /// <summary>
      /// zeigt einen Tooltip für die Tracks an
      /// </summary>
      /// <param name="tracks"></param>
      /// <param name="toolTip"></param>
      /// <param name="mapCtrl"></param>
      public static void ShowToolTip4Tracks(List<Track> tracks, ToolTip toolTip, MapCtrl mapCtrl) {
         if (tracks.Count > 0) {
            string txt = string.Empty;
            for (int i = 0; i < tracks.Count; i++) {
               if (i > 0)
                  txt += Environment.NewLine;
               txt += string.Format("{0} [{1:F1} km / {2:F0} m]",
                                    tracks[i].VisualName,
                                    tracks[i].StatLength / 1000,
                                    tracks[i].StatLength);
            }
            toolTip.Show(txt,
                         mapCtrl,
                         mapCtrl.M_LastMouseLocation.X + 10,
                         mapCtrl.M_LastMouseLocation.Y - 10);
         } else
            toolTip.Hide(mapCtrl);
      }

      #region Tracks/Marker anzeigen oder verbergen

      /// <summary>
      /// alle Objekte in der <see cref="GpxWorkbench"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="on"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="roCtrl"></param>
      /// <param name="rwCtrl"></param>
      public static void ShowAllGpxObjectsInWorkbench(
               bool on,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               ReadOnlyGpxControl roCtrl,
               EditableGpxControl rwCtrl) {
         if (gpxWorkbench != null) {
            foreach (Track track in gpxWorkbench.TrackList)
               ShowTrack(track,
                         on,
                         mapCtrl,
                         roCtrl,
                         rwCtrl);
            foreach (Marker marker in gpxWorkbench.MarkerList) {
               ShowWorkbenchMarker(marker, on, mapCtrl, gpxWorkbench, rwCtrl);
               rwCtrl.ShowMarker(marker, on);
            }
         }
      }

      /// <summary>
      /// fügt den Track (sichtbar) zum Overlay der Karte in der richtigen Ebene hinzu oder 
      /// entfernt ihn (aber nur bei Veränderung des Sichtbarkeitsstatus)
      /// <para>Außerdem wird der Status der zugehörigen Control (Checkbox) angepasst.</para>
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="roCtrl"></param>
      /// <param name="rwCtrl"></param>
      public static void ShowTrack(Track? track,
                          bool visible,
                          MapCtrl mapCtrl,
                          ReadOnlyGpxControl roCtrl,
                          EditableGpxControl rwCtrl) {
         if (track != null &&
             track.IsVisible != visible) {
            mapCtrl.M_ShowTrack(track,
                                     visible,
                                     visible ? NextVisibleTrack(track,
                                                                roCtrl,
                                                                rwCtrl) : null);

            // Control-Status anpassen
            if (track.IsEditable) {
               rwCtrl.ShowTrack(track, visible);
               if (track.GpxDataContainer != null)
                  track.GpxDataContainer.GpxDataChanged = true;
            } else
               roCtrl.ShowTrack(track, visible);
         }
      }

      /// <summary>
      /// liefert den nächsten (darüber liegenden sichtbaren) Track
      /// </summary>
      /// <param name="track"></param>
      /// <param name="roCtrl"></param>
      /// <param name="rwCtrl"></param>
      /// <returns></returns>
      public static Track? NextVisibleTrack(Track? track, ReadOnlyGpxControl roCtrl, EditableGpxControl rwCtrl) =>
         track == null ? null :
            track.IsEditable ?
                     track.GpxDataContainer != null ?
                        track.GpxDataContainer.NextVisibleTrack(track) :
                        null :
                     roCtrl.NextVisibleTrack(track);

      public static void ShowMarker(
               Marker? marker,
               bool visible,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               ReadOnlyGpxControl roCtrl) {
         if (marker != null &&
             marker.IsVisible != visible &&
             gpxWorkbench != null)
            mapCtrl.M_ShowMarker(marker,
                                      visible,
                                      visible ? NextVisibleMarker(marker, gpxWorkbench, roCtrl) : null);
      }

      public static void ShowMarker(
               Marker? marker,
               bool visible,
               MapCtrl mapCtrl,
               ReadOnlyGpxControl roCtrl,
               EditableGpxControl rwCtrl) {
         if (marker != null &&
             marker.IsVisible != visible) {
            mapCtrl.M_ShowMarker(marker,
                                     visible,
                                     visible ? nextVisibleMarker(marker,
                                                                 roCtrl,
                                                                 rwCtrl) : null);

            //// Control-Status anpassen
            //if (marker.IsEditable) {
            //   rwCtrl.ShowMarker(marker, visible);
            //   if (marker.GpxDataContainer != null)
            //      marker.GpxDataContainer.GpxDataChanged = true;
            //} else
            //   roCtrl.ShowMarker(marker, visible);
         }
      }

      static Marker? nextVisibleMarker(Marker? marker, ReadOnlyGpxControl roCtrl, EditableGpxControl rwCtrl) =>
         marker == null ? null :
            marker.IsEditable ?
                     marker.GpxDataContainer != null ?
                        marker.GpxDataContainer.NextVisibleMarker(marker) :
                        null :
                     roCtrl.NextVisibleMarker(marker);

      /// <summary>
      /// aller Markers des <see cref="GpxData"/>-Objektes anzeigen oder verbergen entsprechend 
      /// <see cref="MarkersVisible"/> aus <see cref="GpxData"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="roCtrl"></param>
      public static void ShowAllMarker4GpxObject(
               GpxData gpx,
               bool visible,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               ReadOnlyGpxControl roCtrl) {
         if (gpx != null) {
            for (int i = 0; i < gpx.MarkerList.Count; i++)
               ShowMarker(gpx.MarkerList[i], visible, mapCtrl, gpxWorkbench, roCtrl);
         }
      }

      /// <summary>
      /// aller Fotos des <see cref="GpxData"/>-Objektes anzeigen oder verbergen entsprechend <see cref="PicturesVisible"/> aus <see cref="GpxData"/>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="visible"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="roCtrl"></param>
      public static void ShowAllFotoMarker4GpxObject(
               GpxData gpx,
               bool visible,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               ReadOnlyGpxControl roCtrl) {
         if (gpx != null) {
            for (int i = 0; i < gpx.MarkerListPictures.Count; i++)
               ShowMarker(gpx.MarkerListPictures[i], visible, mapCtrl, gpxWorkbench, roCtrl);
         }
      }

      /// <summary>
      /// liefert den nächsten (darüber liegenden sichtbaren) Marker
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="roCtrl"></param>
      /// <returns></returns>
      public static Marker? NextVisibleMarker(
               Marker marker,
               GpxWorkbench? gpxWorkbench,
               ReadOnlyGpxControl roCtrl) =>
         marker.IsEditable ?
            gpxWorkbench?.Gpx.NextVisibleMarker(marker) :
            roCtrl.NextVisibleMarker(marker);

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="rwCtrl"></param>
      public static void showWorkbenchMarker(
               int idx,
               bool visible,
               GpxWorkbench gpxWorkbench,
               MapCtrl mapCtrl,
               EditableGpxControl rwCtrl) {
         if (0 <= idx && idx < gpxWorkbench.MarkerCount) {
            Marker? marker = gpxWorkbench.GetMarker(idx);
            if (marker != null)
               ShowWorkbenchMarker(marker, visible, mapCtrl, gpxWorkbench, rwCtrl);
         }
      }

      /// <summary>
      /// editierbaren Marker anzeigen oder verbergen
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="rwCtrl"></param>
      /// <returns></returns>
      public static int ShowWorkbenchMarker(
               Marker marker,
               bool visible,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               EditableGpxControl rwCtrl) {
         int idx = showWorkbenchMarker(marker, visible, mapCtrl, gpxWorkbench);
         if (idx >= 0)
            rwCtrl.ShowMarker(marker, visible);
         return idx;
      }

      #endregion

      #region Funktionen für Tracks

      /// <summary>
      /// erzeugt eine bearbeitbare Kopie der <see cref="Track"/> und fügt sie in die Liste ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="roCtrl"></param>
      /// <param name="rwCtrl"></param>
      /// <returns></returns>
      public static Track? CloneTrack2GpxWorkbench(
                  Track? orgtrack,
                  GpxWorkbench? gpxWorkbench,
                  MapCtrl mapCtrl,
                  ReadOnlyGpxControl roCtrl,
                  EditableGpxControl rwCtrl) {
         if (orgtrack != null &&
             gpxWorkbench != null) {
            Track newtrack = gpxWorkbench.TrackInsertCopy(orgtrack);
            ShowTrack(newtrack,
                      true,
                      mapCtrl,
                      roCtrl,
                      rwCtrl); // sichtbar!
            return newtrack;
         }
         return null;
      }

      /// <summary>
      /// liefert den ersten markierten Track oder null
      /// </summary>
      /// <param name="thisnot">wenn ungleich null, dann wird NICHT dieser Track geliefert</param>
      /// <param name="onlyeditable">wenn true, dann nur editierbare Tracks berücksichtigen</param>
      /// <param name="highlightedTrackSegments"></param>
      /// <returns></returns>
      public static Track? FirstMarkedTrack(Track? thisnot, bool onlyeditable, List<Track> highlightedTrackSegments) {
         Track? track = null;
         for (int i = 0; i < highlightedTrackSegments.Count; i++) {
            if ((thisnot == null ||
                 !highlightedTrackSegments[i].Equals(thisnot)) &&
                 (!onlyeditable ||
                  highlightedTrackSegments[i].IsEditable)) {
               track = highlightedTrackSegments[i];
               break;
            }
         }
         return track;
      }

      /// <summary>
      /// erzeugt einen "vereinfachten" Track zum vorgegebenen Track
      /// </summary>
      /// <param name="track"></param>
      /// <param name="appData"></param>
      /// <param name="gpxWorkbench"></param>
      public static void SimplifyTrack(Track track, Common.AppData? appData, GpxWorkbench? gpxWorkbench) {
         if (appData != null &&
             gpxWorkbench != null) {
            FormTrackSimplificationcs dlg = new() {
               SrcTrack = track,
            };
            FormTrackSimplificationcs.SimplificationDataList.Clear();
            FormTrackSimplificationcs.SimplificationDataList.AddRange(appData.SimplifyDatasetList);

            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.DestTrack != null &&
                dlg.DestTrack.GpxSegment.Points.Count > 1) {

               int orgidx = gpxWorkbench.TrackIndex(track);
               gpxWorkbench.TrackInsertCopy(dlg.DestTrack, orgidx); // "über" dem Originaltrack
            }

            appData.SimplifyDatasetList = FormTrackSimplificationcs.SimplificationDataList;
         }
      }

      /// <summary>
      /// einen Track in der Karte und der entsprechenden Liste markieren
      /// </summary>
      /// <param name="track"></param>
      /// <param name="roCtrl"></param>
      /// <param name="rwCtrl"></param>
      public static void TrackMarking(Track track, ReadOnlyGpxControl roCtrl, EditableGpxControl rwCtrl) {
         if (track != null) {
            if (track.IsEditable)
               rwCtrl.SelectTrack(track);
            else
               roCtrl.SelectTrack(track);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="gpxWorkbench"></param>
      /// <returns></returns>
      public static string GetQuestionText4DeleteAllVisibleGpxObjectsInWorkbench(GpxWorkbench? gpxWorkbench) {
         if (gpxWorkbench != null) {
            int visibletracks = gpxWorkbench.VisibleTracks().Count;
            int visiblemarker = gpxWorkbench.VisibleMarkers().Count;
            if (visibletracks + visiblemarker > 0) {
               string txt;
               if (visibletracks + visiblemarker == 1)
                  txt = "Soll der angezeigte " + (visibletracks == 1 ? "Track" : "Marker") + " gelöscht werden?";
               else {
                  txt = "Soll wirklich ALLE " + (visibletracks + visiblemarker) + " angezeigten Objekte (";
                  if (visibletracks == 1)
                     txt += "1 Track";
                  else if (1 < visibletracks)
                     txt += visibletracks + " Tracks";
                  if (0 < visibletracks && 0 < visiblemarker)
                     txt += ", ";
                  if (visiblemarker == 1)
                     txt += "1 Marker";
                  else if (1 < visiblemarker)
                     txt += visiblemarker + " Marker";
                  txt += ") gelöscht werden?";
               }
               return txt;
            }
         }
         return string.Empty;
      }

      #endregion

      #region Editfunktionen für Marker und Tracks

      /// <summary>
      /// den am nächsten liegenden Trackpunkt des akt. bearbeiteten Tracks entfernen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      /// <returns></returns>
      public static bool EditRemoveNearestTrackpoint(Point ptclient, GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         bool end = false;
         Track? t = gpxWorkbench.TrackInEdit;
         gpxWorkbench.TrackRemoveNextPoint(ptclient);
         if (t.GpxSegment.Points.Count == 0) {  // Track ohne Punkte -> entfernen
            gpxWorkbench.TrackEndEdit(true);
            gpxWorkbench.TrackRemove(t);
            end = true;
         }
         showMiniEditableTrackInfo(rwCtrl);
         return end;
      }

      /// <summary>
      /// Trackverbindung zum 1. hervorgehobenen Track beenden
      /// </summary>
      /// <param name="highlightedTracks"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      /// <returns></returns>
      public static bool EditEndTrackConcat(List<Track> highlightedTracks, GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         bool handled = false;
         if (gpxWorkbench != null) {
            Track? markedtrack = FirstMarkedTrack(gpxWorkbench.TrackInEdit, true, highlightedTracks);
            if (markedtrack != null) {
               gpxWorkbench.TrackEndEdit(markedtrack, false);
               showMiniEditableTrackInfo(rwCtrl);
               handled = true;
            }
         }
         return handled;
      }

      /// <summary>
      /// Tracktrennung beenden
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void EditEndTrackSplit(Point ptclient, GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         gpxWorkbench?.TrackEndEdit(ptclient, false);
         showMiniEditableTrackInfo(rwCtrl);
      }

      /// <summary>
      /// einen Trackpunkt an den akt. bearbeiteten Track anfügen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void EditAddTrackpoint(Point ptclient, GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         gpxWorkbench?.TrackAddPoint(ptclient);
         showMiniEditableTrackInfo(rwCtrl);
      }

      /// <summary>
      /// einen neuen oder vorhandenen Marker an diese Position setzen
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void EditSetMarker2Position(Point ptclient, GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         if (gpxWorkbench != null) {
            if (!gpxWorkbench.MarkerIsInWork)
               gpxWorkbench.MarkerStartEdit(true, null);
            gpxWorkbench.MarkerEndEdit(ptclient, false);
         }
      }

      /// <summary>
      /// letzten Punkt des Tracks beim zeichnen eines Tracks entfernen
      /// </summary>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void EditRemoveLastPointFromTrack(GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         gpxWorkbench.TrackRemoveLastPoint();
         showMiniEditableTrackInfo(rwCtrl);
      }

      /// <summary>
      /// zeichnen eines Tracks regulär beenden
      /// </summary>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void EditEndDrawTrack(GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         gpxWorkbench?.TrackEndEdit(false);
         showMiniEditableTrackInfo(rwCtrl);
      }

      #endregion

      /// <summary>
      /// erzeugt eine bearbeitbare Kopie des <see cref="Marker"/> und fügt sie in die Liste ein
      /// </summary>
      /// <param name="orgmarker"></param>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void CloneMarker2GpxWorkbench(
               Marker orgmarker,
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               EditableGpxControl rwCtrl) {
         if (orgmarker != null &&
             gpxWorkbench != null) {
            Marker? m = gpxWorkbench.MarkerInsertCopy(orgmarker, 0);
            m.Text += " (Kopie)";
            rwCtrl.SetMarkerNameAndImage(m, m.Text);
            showWorkbenchMarker(0, true, gpxWorkbench, mapCtrl, rwCtrl);
         }
      }

      /// <summary>
      /// (ev.) alle editierbaren Tracks und Marker löschen
      /// </summary>
      /// <param name="mapCtrl"></param>
      /// <param name="gpxWorkbench"></param>
      /// <param name="roCtrl"></param>
      /// <param name="rwCtrl"></param>
      public static void RemoveTracksAndMarkerInWorkbench(
               MapCtrl mapCtrl,
               GpxWorkbench? gpxWorkbench,
               ReadOnlyGpxControl roCtrl,
               EditableGpxControl rwCtrl) {
         if (gpxWorkbench != null &&
             gpxWorkbench.TrackCount > 0 || gpxWorkbench.MarkerCount > 0) {

            StringBuilder sb = new();
            if (gpxWorkbench.TrackCount > 1)
               sb.AppendFormat("Sollen die {0} Tracks", gpxWorkbench.TrackCount);
            else if (gpxWorkbench.TrackCount == 1)
               sb.Append("Soll der Tracks");

            if (gpxWorkbench.MarkerCount > 1)
               sb.AppendFormat("{0}die {1} Markierungen",
                               gpxWorkbench.TrackCount > 0 ? " und " : "Sollen ",
                               gpxWorkbench.MarkerCount);
            else if (gpxWorkbench.MarkerCount == 1)
               sb.AppendFormat("{0}die Markierung",
                               gpxWorkbench.TrackCount > 0 ? " und " : "Soll ");

            sb.Append(" wirklich entfernt werden?");
            sb.AppendLine();

            if (gpxWorkbench.DataChanged) {
               sb.AppendLine("Die Daten wurden noch nicht gespeichert!");
            } else {
               sb.AppendLine("Die Daten wurden schon in der Datei '" + gpxWorkbench.InternalFilename + "' gespeichert.");
            }

            if (UIHelper.ShowYesNoQuestion_RealYes(sb.ToString(), "alle Tracks und/oder Markierungen entfernen")) {

               if (gpxWorkbench.TrackIsInWork)
                  gpxWorkbench.TrackEndEdit(true);

               if (gpxWorkbench.MarkerIsInWork)
                  gpxWorkbench.MarkerEndEdit(false);

               for (int i = gpxWorkbench.TrackCount - 1; i >= 0; i--) {
                  Track? track = gpxWorkbench.GetTrack(i);
                  if (track != null) {
                     ShowTrack(track,
                               false,
                               mapCtrl,
                               roCtrl,
                               rwCtrl);
                     gpxWorkbench.TrackRemove(track);
                  }
               }

               for (int i = gpxWorkbench.MarkerCount - 1; i >= 0; i--) {
                  Marker? marker = gpxWorkbench.GetMarker(i);
                  if (marker != null) {
                     ShowMarker(marker, false, mapCtrl, gpxWorkbench, roCtrl);
                     gpxWorkbench.MarkerRemove(marker);
                  }
               }

               gpxWorkbench.Save(true);
            }
         }
      }

      /// <summary>
      /// bei Bedarf eindeutige Namen für Marker und Tracks erzeugen
      /// </summary>
      /// <param name="gpxWorkbench"></param>
      /// <param name="rwCtrl"></param>
      public static void UniqueNamesInWorkbench(GpxWorkbench? gpxWorkbench, EditableGpxControl rwCtrl) {
         if (gpxWorkbench != null &&
             gpxWorkbench.SetUniqueNames4TracksAndMarkers(out List<int> markerlst, out List<int> tracklst)) {
            foreach (int idx in markerlst) {
               Marker? marker = gpxWorkbench.GetMarker(idx);
               if (marker != null)
                  rwCtrl.SetMarkerNameAndImage(marker, marker.Text);
            }
            foreach (int idx in tracklst) {
               Track? track = gpxWorkbench.GetTrack(idx);
               if (track != null)
                  rwCtrl.SetTrackNameAndImage(track, track.Trackname);
            }
         }
      }

      #region Karte drucken

      static PageSettings? pageSettings = null;
      static PrinterSettings? printerSettings = null;
      static PrintDialog? printDialog = null;

      /// <summary>
      /// Umrechnung mm in 1/100 Zoll
      /// </summary>
      /// <param name="mm"></param>
      /// <returns></returns>
      static double mm2inch100(double mm) => mm * 100 / 25.4;

      /// <summary>
      /// drucken einer Seite
      /// </summary>
      /// <param name="e"></param>
      /// <param name="pd"></param>
      static void documentPrintPage(PrintPageEventArgs e, Image img) {
         e.HasMorePages = false;                         // sicherheitshalber erstmal die Druckerei beenden
         try {

            float fwidth = (float)e.MarginBounds.Width / img.Width;
            float fheight = (float)e.MarginBounds.Height / img.Height;
            float f = Math.Min(fwidth, fheight);
            e.Graphics.DrawImage(img, e.MarginBounds.Left, e.MarginBounds.Top, f * img.Width, f * img.Height);
            e.HasMorePages = false;

         } catch (Exception ex) {
            UIHelper.ShowErrorMessage(ex.Message, "Fehler beim Drucken");
         }
      }

      public static void PrintMap(Image img) {
         try {
            pageSettings ??= new PageSettings() {
               Color = true,
               Margins = new Margins((int)Math.Round(mm2inch100(10)),
                                        (int)Math.Round(mm2inch100(10)),
                                        (int)Math.Round(mm2inch100(10)),
                                        (int)Math.Round(mm2inch100(10))),
               Landscape = true,
            };
            printerSettings ??= new PrinterSettings();

            PrintDocument pdoc = new() {
               PrinterSettings = printerSettings,
               DocumentName = "Karte"
            };
            pdoc.PrintPage += (doc, args) => documentPrintPage(args, img);

            PageSetupDialog psd = new() {
               AllowMargins = true,
               AllowOrientation = true,
               AllowPaper = true,
               AllowPrinter = true,          // fkt. seit Vista nicht mehr (obsolet by MS)
               EnableMetric = true,
               Document = pdoc,
               PrinterSettings = printerSettings,
               PageSettings = pageSettings,
            };
            if (psd.ShowDialog() == DialogResult.OK) {

               pageSettings = psd.PageSettings;
               printerSettings = psd.PrinterSettings;

               pdoc.PrinterSettings = printerSettings;
               pdoc.DefaultPageSettings = pageSettings;

               if (printDialog == null)
                  printDialog = new PrintDialog() {
                     AllowPrintToFile = false,
                     AllowCurrentPage = false,     // Option "akt. Seite"
                     AllowSelection = false,       // Option "Markierung"
                     AllowSomePages = false,       // Option "Seitenauswahl"
                     UseEXDialog = true,
                     PrinterSettings = printerSettings,
                     Document = pdoc,
                  };
               else {
                  printDialog.PrinterSettings = printerSettings;
                  printDialog.Document = pdoc;
               }

               FormPrintPreview formPrintPreview = new() {
                  Document = printDialog.Document,
               };
               if (formPrintPreview.ShowDialog() == DialogResult.OK) {  // das Bild wurde gezeichnet und der Dialog mit OK beantwortet
                  if (printDialog.ShowDialog() == DialogResult.OK) {    // u.a. auch mit Druckerauswahl
                     printDialog.Document.Print();
                     printerSettings = printDialog.PrinterSettings;
                  }

                  //#if ONLY_PRINT_PREVIEW
                  //               PrintPreviewDialog PrevDlg = new PrintPreviewDialog() {

                  //                  Document = printDialog.Document,
                  //                  WindowState = FormWindowState.Maximized,
                  //                  ShowInTaskbar = false,
                  //                  ShowIcon = false,
                  //               };
                  //               PrevDlg.ShowDialog(this);
                  //               PrevDlg = null;

                  //               if (printDialog.ShowDialog() == DialogResult.OK) {
                  //                  //printDialog.Document.Print();
                  //                  printerSettings = printDialog.PrinterSettings;
                  //               }
                  //#else
                  //               if (printDialog.ShowDialog() == DialogResult.OK) {    // u.a. auch mit Druckerauswahl
                  //                  printDialog.Document.Print();
                  //                  printerSettings = printDialog.PrinterSettings;
                  //               }
                  //#endif

               }
            }

         } catch (Exception ex) {
            UIHelper.ShowExceptionError(ex);
         }

      }

      #endregion

   }
}
