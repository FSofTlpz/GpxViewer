using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSofTUtils.Geography.DEM;
using GMap.NET;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using Gpx = FSofTUtils.Geography.PoorGpx;


namespace GpxViewer {
   public class MapControl2 : MapControl {

      // GMap.NET.WindowsForms.GMapControl   <->   GMap.NET.Skia.GMapControl

      /// <summary>
      /// Overlay für die GPX-Daten
      /// </summary>
      readonly GMapOverlay GpxReadOnlyOverlay = new GMapOverlay("GPXro");
      readonly GMapOverlay GpxOverlay = new GMapOverlay("GPX");
      readonly GMapOverlay GpxSelectedPartsOverlay = new GMapOverlay("GPXselparts");

      /// <summary>
      /// zur internen Erzeugung der Garminkarten und zur (externen) Objektsuche
      /// </summary>
      GarminImageCreator.ImageCreator garminImageCreator;


      GMap.NET.WindowsForms.GMapControl gMapControl;


      PointLatLng gMapControl_FromLocalToLatLng(int ptclientX, int ptclientY) {
         return gMapControl.FromLocalToLatLng(ptclientX, ptclientY);
      }

      double gMapControl_MapProvider_Projection_GetGroundResolution(int zoom, double lat) {
         return gMapControl.MapProvider.Projection.GetGroundResolution(zoom, lat);  // Meter je Pixel
      }





      /// <summary>
      /// registriert die zu verwendenden Karten-Provider in der Liste <see cref="MapProviderDefinitions"/>
      /// </summary>
      /// <param name="providernames"></param>
      /// <param name="garmindefs"></param>
      /// <param name="wmsdefs"></param>
      /// <param name="kmzdefs"></param>
      public void MapRegisterProviders(IList<string> providernames,
                                       List<MapProviderDefinition> provdefs) {
         MapProviderDefinitions.Clear();

         if (providernames != null)
            for (int i = 0; i < providernames.Count; i++) {
               MapProviderDefinition def = provdefs[i];

               if (providernames[i] == GarminProvider.Instance.Name &&
                   def is GarminProvider.GarminMapDefinitionData) {

                  def.Provider = GarminProvider.Instance;
                  MapProviderDefinitions.Add(def as GarminProvider.GarminMapDefinitionData);

               } else if (providernames[i] == GarminKmzProvider.Instance.Name &&
                          def is GarminKmzProvider.KmzMapDefinition) {

                  def.Provider = GarminKmzProvider.Instance;
                  MapProviderDefinitions.Add(def as GarminKmzProvider.KmzMapDefinition);

               } else if (providernames[i] == WMSProvider.Instance.Name &&
                          def is WMSProvider.WMSMapDefinition) {

                  def.Provider = WMSProvider.Instance;
                  MapProviderDefinitions.Add(def as WMSProvider.WMSMapDefinition);

               } else {

                  for (int p = 0; p < GMapProviders.List.Count; p++) {
                     if (GMapProviders.List[p].Name == providernames[i]) { // Provider ist vorhanden

                        def.Provider = GMapProviders.List[p];
                        MapProviderDefinitions.Add(def);

                     }
                  }

               }
            }
      }

      /// <summary>
      /// setzt den aktiven Karten-Provider
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="demalpha">Alpha für Hillshading</param>
      /// <param name="dem">Hilfsdaten für Höhenangaben und Hillshading</param>
      public void MapSetActivProvider(int idx, int demalpha, DemData dem = null) {
         MapProviderDefinition def = MapProviderDefinitions[idx];
         GMapProvider newprov = def.Provider;

         GpxReadOnlyOverlay.IsVisibile = GpxOverlay.IsVisibile = false;

         // ev. Zoom behandeln
         double newzoom = -1;
         if (MapZoom < def.MinZoom || def.MaxZoom < MapZoom) {
            newzoom = Math.Min(Math.Max(MapZoom, def.MinZoom), def.MaxZoom);
         }

         if (newprov is GMapProviderWithHillshade) {
            (newprov as GMapProviderWithHillshade).DEM = dem;
            (newprov as GMapProviderWithHillshade).Alpha = demalpha;
         }

         if (newprov is GarminProvider) {

            if (garminImageCreator == null)
               garminImageCreator = new GarminImageCreator.ImageCreator();

            GarminProvider.GarminMapDefinitionData gdef = def as GarminProvider.GarminMapDefinitionData;
            (newprov as GarminProvider).ChangeDbId(GarminProvider.StandardDbId + gdef.DbIdDelta);

            List<GarminImageCreator.GarminMapData> mapdata = new List<GarminImageCreator.GarminMapData>();
            for (int i = 0; i < gdef.TDBfile.Count && i < gdef.TYPfile.Count; i++) {
               mapdata.Add(new GarminImageCreator.GarminMapData(gdef.TDBfile[i],
                                                                gdef.TYPfile[i],
                                                                "",
                                                                gdef.Levels4LocalCache,
                                                                gdef.MaxSubdivs,
                                                                gdef.TextFactor,
                                                                gdef.LineFactor,
                                                                gdef.SymbolFactor));
            }

            garminImageCreator.SetGarminMapDefs(mapdata);
            (newprov as GarminProvider).GarminImageCreater = garminImageCreator;      // hier werden die Daten im Provider indirekt über den ImageCreator gesetzt

         } else if (newprov is WMSProvider) {

            (newprov as WMSProvider).ChangeDbId(WMSProvider.StandardDbId + (def as WMSProvider.WMSMapDefinition).DbIdDelta);
            (newprov as WMSProvider).SetDef(def as WMSProvider.WMSMapDefinition);

         } else if (newprov is GarminKmzProvider) {

            (newprov as GarminKmzProvider).ChangeDbId(GarminKmzProvider.StandardDbId + (def as GarminKmzProvider.KmzMapDefinition).DbIdDelta);
            (newprov as GarminKmzProvider).SetDef(def as GarminKmzProvider.KmzMapDefinition);

         }

         // jetzt wird der neue Provider und ev. auch der Zoom gesetzt
         gMapControl.MapRenderZoom2RealDevice = 1;
         if (def.Zoom4Display != 1)
            gMapControl.MapRenderZoom2RealDevice = (float)def.Zoom4Display;

         gMapControl.MapProvider = newprov;
         if (newzoom >= 0)
            MapZoom = newzoom;
         gMapControl.MinZoom = def.MinZoom;
         gMapControl.MaxZoom = def.MaxZoom;
         MapRefresh(true, false);

         GpxReadOnlyOverlay.IsVisibile = GpxOverlay.IsVisibile = true; // ohne false/true-Wechsel passt die Darstellung des Overlays manchmal nicht zur Karte

         //GpxReadOnlyOverlay.IsVisibile = GpxOverlay.IsVisibile = false;

         //if (MapProviderDefinitions[idx].Provider is GMapProviderWithHillshade) {
         //   GMapProviderWithHillshade prov = MapProviderDefinitions[idx].Provider as GMapProviderWithHillshade;
         //   prov.DEM = dem;
         //   prov.Alpha = demalpha;
         //}

         //if (MapProviderDefinitions[idx].Provider is GarminProvider) {

         //   GarminProvider prov = MapProviderDefinitions[idx].Provider as GarminProvider;
         //   if (garminImageCreater == null)
         //      garminImageCreater = new GarminImageCreator.ImageCreator();
         //   garminImageCreater.SetGarminMapDefs(MapProviderDefinitions[idx].ExtData as GarminImageCreator.GarminMapDefinitionData);
         //   prov.GarminImageCreater = garminImageCreater;
         //   gMapControl.MapProvider = prov;
         //   MapRefresh();

         //} else if (MapProviderDefinitions[idx].Provider is WMSProvider) {

         //   WMSProvider prov = MapProviderDefinitions[idx].Provider as WMSProvider;
         //   prov.SetDef(MapProviderDefinitions[idx].ExtData as WMSProvider.WMS_Data);
         //   gMapControl.MapProvider = prov;
         //   MapRefresh();

         //} else if (MapProviderDefinitions[idx].Provider is GarminKmzProvider) {

         //   GarminKmzProvider prov = MapProviderDefinitions[idx].Provider as GarminKmzProvider;
         //   prov.SetDef(MapProviderDefinitions[idx].ExtData as GarminKmzProvider.KmzMapDefinition);
         //   gMapControl.MapProvider = prov;
         //   MapRefresh();

         //} else {

         //   gMapControl.MapProvider = MapProviderDefinitions[idx].Provider;

         //}
         //GpxReadOnlyOverlay.IsVisibile = GpxOverlay.IsVisibile = true; // ohne false/true-Wechsel passt die Darstellung des Overlays manchmal nicht zur Karte
      }

      /// <summary>
      /// zeichnet die Karte neu
      /// </summary>
      /// <param name="clearmemcache">löscht auch den Cache im Hauptspeicher (Die Tiles in diesem Cache haben KEINE DbId!))</param>
      /// <param name="clearcache">löscht auch den Cache auf HD und/oder Server</param>
      public void MapRefresh(bool clearmemcache, bool clearcache) {
         //if (gMapControl.MapProvider is WMSProvider ||
         //    gMapControl.MapProvider is GarminKmzProvider ||
         //    gMapControl.MapProvider is GarminProvider)
         //   gMapControl.Manager.MemoryCache.Clear(); // Cache muss gelöscht werden

         if (clearmemcache)
            gMapControl.Manager.MemoryCache.Clear(); // Die Tiles in diesem Cache haben KEINE DbId!

         if (clearcache) {
            if (gMapControl.Manager.PrimaryCache != null) {
               gMapControl.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, gMapControl.MapProvider.DbId);
            }
            if (gMapControl.Manager.SecondaryCache != null) {
               gMapControl.Manager.SecondaryCache.DeleteOlderThan(DateTime.Now, gMapControl.MapProvider.DbId);
            }
         }

         gMapControl.ReloadMap();
         gMapControl.Refresh();
      }

      /// <summary>
      /// setzt die Kartenpos. (Mittelpunkt) und den Zoom
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="centerlon"></param>
      /// <param name="centerlat"></param>
      public void MapSetLocationAndZoom(double zoom, double centerlon, double centerlat) {
         gMapControl.Position = new GMap.NET.PointLatLng(centerlat, centerlon);
         MapZoom = zoom;
      }

      /// <summary>
      /// Sicht auf die Karte prozentual zur Größe des Sichtfenster verschieben
      /// </summary>
      /// <param name="dxpercent">-1..0..1; prozentual zur Breite des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach rechts</param>
      /// <param name="dypercent">-1..0..1; prozentual zur Höhe des Sichtfenster; ein positiver Wert verschiebt das Sichtfenster nach oben</param>
      public void MapMoveView(double dxpercent, double dypercent) {
         gMapControl.Position = new GMap.NET.PointLatLng(gMapControl.Position.Lat + gMapControl.ViewArea.HeightLat * dypercent,
                                                         gMapControl.Position.Lng + gMapControl.ViewArea.WidthLng * dxpercent);
      }


      /// <summary>
      /// zum Bereich (durch Lat/lon begrenzt) zoomen
      /// </summary>
      /// <param name="topleft"></param>
      /// <param name="bottomright"></param>
      public void MapZoomToRange(PointD topleft, PointD bottomright) {
         gMapControl.SetZoomToFitRect(new RectLatLng(topleft.Y,
                                                     topleft.X,
                                                     Math.Abs(topleft.X - bottomright.X),
                                                     Math.Abs(topleft.Y - bottomright.Y))); // Ecke links-oben, Breite, Höhe
      }

      /// <summary>
      /// liefert den Index des aktiven Providers in der <see cref="MapProviderDefinitions"/>-Liste
      /// </summary>
      /// <returns></returns>
      public int GetActiveProviderIdx() {
         if (gMapControl.MapProvider != null &&
             MapProviderDefinitions != null)
            for (int i = 0; i < MapProviderDefinitions.Count; i++) {
               if (gMapControl.Equals(MapProviderDefinitions[i].Provider))
                  return i;
            }
         return -1;
      }

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="provider"></param>
      /// <returns>Anzahl der Tiles</returns>
      public int ClearCache(GMapProvider provider = null) {
         int count = 0;
         if (provider == null) {

            count += GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, null);           // i.A. lokal (SQLite)
            if (GMaps.Instance.SecondaryCache != null)                                 // auf dem Server
               count += GMaps.Instance.SecondaryCache.DeleteOlderThan(DateTime.Now, null);

         } else {

            count += GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, provider.DbId);  // i.A. lokal (SQLite)
            if (GMaps.Instance.SecondaryCache != null)                                 // auf dem Server
               count += GMaps.Instance.SecondaryCache.DeleteOlderThan(DateTime.Now, GarminProvider.Instance.DbId);

         }
         return count;
      }

      /// <summary>
      /// löscht den lokalen SQLite- und, falls vorhanden, den Server-Map-Cache
      /// </summary>
      /// <param name="idx">bezieht sich auf die Liste der <see cref="MapProviderDefinitions"/>; falls negativ, wird alles gelöscht</param>
      /// <returns></returns>
      public int ClearCache(int idx) {
         return ClearCache(idx < 0 ? null : MapProviderDefinitions[idx].Provider);
      }

      /// <summary>
      /// löscht den Map-Cache im Hauptspeicher
      /// </summary>
      public void ClearMemoryCache() {
         GMaps.Instance.MemoryCache.Clear();
      }

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich
      /// </summary>
      /// <param name="minlon"></param>
      /// <param name="maxlon"></param>
      /// <param name="minlat"></param>
      /// <param name="maxlat"></param>
      /// <returns></returns>
      public List<Marker> MapGetPictureMarkersInArea(double minlon, double maxlon, double minlat, double maxlat) {
         List<Marker> markerlst = new List<Marker>();
         foreach (GMap.NET.WindowsForms.GMapMarker marker in GpxReadOnlyOverlay.Markers) {
            if (marker is VisualMarker &&
                (marker as VisualMarker).RealMarker.Markertype == Marker.MarkerType.Foto)
               if (minlon <= marker.Position.Lng && marker.Position.Lng <= maxlon &&
                   minlat <= marker.Position.Lat && marker.Position.Lat <= maxlat) {
                  markerlst.Add((marker as VisualMarker).RealMarker);
               }
         }
         return markerlst;
      }

      /// <summary>
      /// liefert eine Liste aller Foto-Marker im Bereich um den Client-Punkt herum
      /// </summary>
      /// <param name="localcenter"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<Marker> MapGetPictureMarkersAround(Point localcenter, int deltax, int deltay) {
         // Distanz um den akt. Punkt (1.5 x Markerbildgröße)
         PointLatLng lefttop = gMapControl_FromLocalToLatLng(localcenter.X - deltax / 2,
                                                             localcenter.Y - deltay / 2);
         PointLatLng rightbottom = gMapControl_FromLocalToLatLng(localcenter.X + deltax / 2,
                                                                 localcenter.Y + deltay / 2);
         return MapGetPictureMarkersInArea(lefttop.Lng, rightbottom.Lng, rightbottom.Lat, lefttop.Lat);
      }

      /// <summary>
      /// liefert für eine Garmin-Karte Infos über Objekte in der Nähe des Punktes
      /// </summary>
      /// <param name="ptclient"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns></returns>
      public List<GarminImageCreator.SearchObject> MapGetGarminObjectInfos(Point ptclient, int deltax, int deltay) {
         List<GarminImageCreator.SearchObject> info = new List<GarminImageCreator.SearchObject>();
         if (gMapControl.MapProvider is GarminProvider) {
            PointLatLng ptlatlon = gMapControl_FromLocalToLatLng(ptclient.X, ptclient.Y);
            PointLatLng ptdelta = gMapControl_FromLocalToLatLng(ptclient.X + deltax, ptclient.Y + deltay);
            double groundresolution = gMapControl_MapProvider_Projection_GetGroundResolution((int)MapZoom, ptlatlon.Lat);  // Meter je Pixel
            info = garminImageCreator.GetObjectInfo(ptlatlon.Lng,
                                                    ptlatlon.Lat,
                                                    ptdelta.Lng - ptlatlon.Lng,
                                                    ptlatlon.Lat - ptdelta.Lat,
                                                    groundresolution);

         }
         return info;
      }

      void collectionInsert<T>(GMap.NET.ObjectModel.ObservableCollectionThreadSafe<T> collection, T item, int idx) {
         collection.Add(item);
         if (0 <= idx && idx < collection.Count - 1)
            collection.Move(collection.Count - 1, idx);
      }

      int collectionIndexOf<T>(GMap.NET.ObjectModel.ObservableCollectionThreadSafe<T> collection, T item) {
         if (item != null)
            for (int idx = 0; idx < collection.Count; idx++)
               if (collection[idx].Equals(item))
                  return idx;
         return -1;
      }

      #region Tracks

      enum TrackLayer {
         Readonly,
         Editable,
         SelectedParts,
      }


      /// <summary>
      /// zeigt einen <see cref="Track"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vt"></param>
      /// <param name="on"></param>
      /// <param name="posttrack">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowTrack(Track track, bool on, Track posttrack) {
         if (track != null) {
            if (on) {

               if (track.VisualTrack == null)
                  track.UpdateVisualTrack();
               MapShowVisualTrack(track.VisualTrack,
                                  true,
                                  track.IsEditable ? TrackLayer.Editable : TrackLayer.Readonly,
                                  posttrack != null ?
                                          posttrack.VisualTrack :
                                          null);

            } else {

               if (track.IsVisible)
                  MapShowVisualTrack(track.VisualTrack,
                                     false,
                                     track.IsEditable ? TrackLayer.Editable : TrackLayer.Readonly);

            }
         }
      }

      /// <summary>
      /// zeigt alle <see cref="Track"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="tracks"></param>
      /// <param name="on"></param>
      public void MapShowTrack(IList<Track> tracks, bool on) {
         for (int i = tracks.Count - 1; i >= 0; i--) {
            MapShowTrack(tracks[i],
                         on,
                         on && i > 0 ? tracks[i - 1] : null);
         }
      }

      /// <summary>
      /// zeigt einen <see cref="VisualTrack"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vt"></param>
      /// <param name="on"></param>
      /// <param name="layer"></param>
      /// <param name="postvt">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      void MapShowVisualTrack(VisualTrack vt, bool on, TrackLayer layer, VisualTrack postvt = null) {
         if (vt != null &&
             vt.Points.Count > 0) {
            if (on) {

               GMapOverlay ov = GpxReadOnlyOverlay;
               switch (layer) {
                  case TrackLayer.Editable:
                     ov = GpxOverlay;
                     break;

                  case TrackLayer.SelectedParts:
                     ov = GpxSelectedPartsOverlay;
                     break;
               }

               vt.IsVisible = true;
               if (!ov.Routes.Contains(vt))
                  collectionInsert(ov.Routes, vt, collectionIndexOf(ov.Routes, postvt));

            } else {

               if (vt.Overlay != null &&
                   vt.IsVisible) {
                  vt.IsVisible = false;
                  vt.Overlay.Routes.Remove(vt);
               }

            }
         }
      }

      #region selektierte Teil-Tracks

      /// <summary>
      /// akt. selektierte Teil-Tracks
      /// </summary>
      Dictionary<Track, List<VisualTrack>> selectedPartsOfTracks = new Dictionary<Track, List<VisualTrack>>();


      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="idxlst"></param>
      public void MapShowSelectedParts(Track mastertrack, IList<int> idxlst) {
         List<List<Gpx.GpxTrackPoint>> parts = null;
         if (idxlst != null &&
             idxlst.Count > 0) {

            parts = new List<List<Gpx.GpxTrackPoint>>();
            int partstart = 0;
            while (partstart < idxlst.Count) {
               int partend;
               for (partend = partstart + 1; partend < idxlst.Count; partend++) {
                  if (idxlst[partend - 1] + 1 != idxlst[partend]) { // NICHT der nachfolgende Index
                     partend--;
                     break;
                  }
               }
               if (idxlst.Count <= partend)
                  partend--;

               List<Gpx.GpxTrackPoint> ptlst = new List<Gpx.GpxTrackPoint>();
               for (int idx = partstart; idx <= partend; idx++)
                  ptlst.Add(mastertrack.GpxSegment.Points[idxlst[idx]]);
               parts.Add(ptlst);

               partstart = partend + 1;
            }

         }
         mapShowSelectedParts(mastertrack, parts);
      }


      /// <summary>
      /// zeigt die Liste der Punktfolgen als Tracks mit besonderem Stil an
      /// </summary>
      /// <param name="mastertrack"></param>
      /// <param name="ptlst"></param>
      void mapShowSelectedParts(Track mastertrack, List<List<Gpx.GpxTrackPoint>> ptlst) {
         if (mastertrack == null) { // alle VisualTrack entfernen

            foreach (var track in selectedPartsOfTracks.Keys)
               mapHideSelectedPartsOfTrack(track);
            selectedPartsOfTracks.Clear();

         } else {

            mapHideSelectedPartsOfTrack(mastertrack); // alle VisualTrack dieses Tracks entfernen

            if (ptlst != null) {
               if (!selectedPartsOfTracks.TryGetValue(mastertrack, out List<VisualTrack> pseudotracklist)) {
                  pseudotracklist = new List<VisualTrack>();
                  selectedPartsOfTracks.Add(mastertrack, pseudotracklist);
               }

               for (int part = 0; part < ptlst.Count; part++) {
                  VisualTrack pseudotrack = new VisualTrack(new Track(ptlst[part], ""), "", VisualTrack.VisualStyle.SelectedPart);
                  pseudotracklist.Add(pseudotrack);
                  MapShowVisualTrack(pseudotrack,
                                     true,
                                     TrackLayer.SelectedParts);
               }
            } else
               selectedPartsOfTracks.Remove(mastertrack);

         }
      }

      /// <summary>
      /// entfernt alle <see cref="VisualTrack"/> für die Selektion dieses Tracks
      /// </summary>
      /// <param name="track"></param>
      void mapHideSelectedPartsOfTrack(Track track) {
         if (selectedPartsOfTracks.TryGetValue(track, out List<VisualTrack> pseudotracklist)) {
            for (int i = pseudotracklist.Count - 1; i >= 0; i--) {
               MapShowVisualTrack(pseudotracklist[i], false, TrackLayer.SelectedParts);
               pseudotracklist[i].Dispose();
            }
            pseudotracklist.Clear();
         }
      }

      #endregion

      /// <summary>
      /// liefert alle aktuell angezeigten Tracks
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Track> MapGetVisibleTracks(bool onlyeditable) {
         List<Track> lst = new List<Track>();
         if (!onlyeditable)
            foreach (var item in GpxReadOnlyOverlay.Routes) {
               if (item is VisualTrack &&
                   (item as VisualTrack).RealTrack != null)
                  lst.Add((item as VisualTrack).RealTrack);
            }
         foreach (var item in GpxOverlay.Routes) {
            if (item is VisualTrack &&
                (item as VisualTrack).RealTrack != null)
               lst.Add((item as VisualTrack).RealTrack);
         }
         return lst;
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Track"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Track"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool MapChangeEditableTrackDrawOrder(IList<Track> trackorder) {
         bool changed = false;

         List<Track> visibletracks = MapGetVisibleTracks(true);
         List<Track> neworder = new List<Track>();
         foreach (Track track in trackorder) {
            if (track.IsVisible &&
                visibletracks.Contains(track))
               neworder.Add(track);
         }

         if (neworder.Count != visibletracks.Count)
            changed = true;
         else {
            for (int i = 0; i < neworder.Count; i++) {
               if (!neworder[i].Equals(visibletracks[i])) {
                  changed = true;
                  break;
               }
            }
         }

         if (changed) {
            GpxOverlay.Routes.Clear();
            foreach (Track t in neworder) {
               GpxOverlay.Routes.Add(t.VisualTrack);
            }
         }

         return changed;
      }

      #endregion

      #region Marker

      /// <summary>
      /// zeigt einen <see cref="Marker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="on"></param>
      /// <param name="postmarker">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowMarker(Marker marker, bool on, Marker postmarker = null) {
         if (marker != null) {
            if (on) {

               if (!marker.IsVisible)
                  marker.IsVisible = true;
               MapShowVisualMarker(marker.VisualMarker,
                                   true,
                                   marker.IsEditable,
                                   postmarker != null ?
                                          postmarker.VisualMarker :
                                          null);

            } else {

               if (marker.IsVisible)
                  MapShowVisualMarker(marker.VisualMarker,
                                      false,
                                      marker.IsEditable);

            }
         }
      }

      /// <summary>
      /// zeigt alle <see cref="Marker"/> auf der Karte an oder entfernt sie aus der Karte
      /// </summary>
      /// <param name="markers"></param>
      /// <param name="on"></param>
      public void MapShowMarker(IList<Marker> markers, bool on) {
         for (int i = 0; i < markers.Count; i++) {
            MapShowMarker(markers[i],
                          on,
                          on && i < markers.Count - 1 ? markers[i - 1] : null);
         }
      }

      /// <summary>
      /// zeigt einen <see cref="VisualMarker"/> auf der Karte an oder entfernt ihn aus der Karte
      /// </summary>
      /// <param name="vm"></param>
      /// <param name="on"></param>
      /// <param name="toplayer"></param>
      /// <param name="postvm">Nachfolger (liegt beim Zeichnen "darüber"); bei null immer an letzter Stelle</param>
      public void MapShowVisualMarker(VisualMarker vm, bool on, bool toplayer, VisualMarker postvm = null) {
         if (vm != null)
            if (on) {

               GMapOverlay ov = toplayer ?
                                       GpxOverlay :
                                       GpxReadOnlyOverlay;
               vm.IsVisible = true;
               if (!ov.Markers.Contains(vm))
                  collectionInsert(ov.Markers, vm, collectionIndexOf(ov.Markers, postvm));

            } else {

               if (vm.Overlay != null &&
                   vm.IsVisible) {
                  vm.IsVisible = false;
                  vm.Overlay.Markers.Remove(vm);
               }

            }
      }

      /// <summary>
      /// liefert alle aktuell angezeigten Marker
      /// </summary>
      /// <param name="onlyeditable">nur editierbare</param>
      /// <returns></returns>
      public List<Marker> MapGetVisibleMarkers(bool onlyeditable) {
         List<Marker> lst = new List<Marker>();
         if (!onlyeditable)
            foreach (var item in GpxReadOnlyOverlay.Markers) {
               if (item is VisualMarker)
                  lst.Add((item as VisualMarker).RealMarker);
            }
         foreach (var item in GpxOverlay.Markers) {
            if (item is VisualMarker)
               lst.Add((item as VisualMarker).RealMarker);
         }
         return lst;
      }

      /// <summary>
      /// die Reihenfolge der Anzeige der editierbaren <see cref="Marker"/> wird ev. angepasst
      /// </summary>
      /// <param name="trackorder">gewünschte Reihenfolge (kann auch nichtangezeigte <see cref="Marker"/> enthalten)</param>
      /// <returns>true, wenn verändert</returns>
      public bool MapChangeEditableMarkerDrawOrder(IList<Marker> markerorder) {
         bool changed = false;

         List<Marker> visiblemarkers = MapGetVisibleMarkers(true);
         List<Marker> neworder = new List<Marker>();
         foreach (Marker m in markerorder) {
            if (m.IsVisible &&
                visiblemarkers.Contains(m))
               neworder.Add(m);
         }

         if (neworder.Count != visiblemarkers.Count)
            changed = true;
         else {
            for (int i = 0; i < neworder.Count; i++) {
               if (!neworder[i].Equals(visiblemarkers[i])) {
                  changed = true;
                  break;
               }
            }
         }

         if (changed) {
            GpxOverlay.Markers.Clear();
            foreach (Marker m in neworder) {
               GpxOverlay.Markers.Add(m.VisualMarker);
            }
         }

         return changed;
      }

      #endregion

   }
}
