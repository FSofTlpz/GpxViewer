using FSofTUtils.Geography.GeoCoding;
using GMap.NET.FSofTExtented.MapProviders;
using SpecialMapCtrl;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

#if ANDROID
using System.Drawing;

namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif
   public class GpxWorkbench {

      // kann zusätzlich das UIHelper.SetBusyStatusEvent (auch mit null als Page) auslösen

      #region Events

      public class LoadEventArgs {

         public enum Reason {
            ReadXml,
            ReadGDB,
            ReadKml,
            InsertWaypoints,
            InsertTracks,
            InsertWaypoint,
            InsertTrack,

            SplitMultiSegmentTracks,
            RemoveEmptyTracks,
            RebuildTrackList,
            RebuildMarkerList,

            ReadIsReady
         }

         public Reason LoadReason;


         public LoadEventArgs(Reason reason) => LoadReason = reason;

      }

      public static event EventHandler<LoadEventArgs>? LoadInfoEvent;

      /// <summary>
      /// ein neuer Marker sollte eingefügt werden
      /// </summary>
      public event EventHandler<EditHelper.MarkerEventArgs>? MarkerShouldInsertEvent;

      /// <summary>
      /// die Anzeige eines Tracks wird ein- oder ausgeschaltet
      /// </summary>
      public event EventHandler<EditHelper.TrackEventArgs>? TrackEditShowEvent;

      #endregion

      #region Props

#if ANDROID
      /// <summary>
      /// Masterpage
      /// </summary>
      readonly MainPage mainpage;
#endif

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      readonly FSofTUtils.Geography.DEM.DemData? Dem = null;

      readonly EditHelper editHelper;

      readonly MapCtrl map;

      /// <summary>
      /// alle akt. GPX-Daten
      /// </summary>
      public readonly GpxData Gpx;

      // "Abkürzungen"

      public string InternalFilename => Gpx.GpxFilename ?? string.Empty;

      public int TrackCount => Gpx.TrackList.Count;

      public int MarkerCount => Gpx.MarkerList.Count;

      public List<Track> TrackList => Gpx.TrackList;

      public List<Marker> MarkerList => Gpx.MarkerList;

      public int TrackIndex(Track track) => Gpx.TrackIndex(track);

      public int MarkerIndex(Marker marker) => Gpx.MarkerIndex(marker);

      public List<bool> VisibleStatusMarkerList {
         get {
            List<bool> lst = [];
            foreach (var marker in Gpx.MarkerList)
               lst.Add(marker.IsVisible);
            return lst;
         }
      }

      public List<bool> VisibleStatusTrackList {
         get {
            List<bool> lst = [];
            foreach (var track in Gpx.TrackList)
               lst.Add(track.IsVisible);
            return lst;
         }
      }

      public bool DataChanged {
         get => Gpx.GpxDataChanged;
         set => Gpx.GpxDataChanged = value;
      }

      /// <summary>
      /// Anzahl der akt. moch nicht gespeicherten Livetrack-Punkte (nur ext. Verwendung)
      /// </summary>
      public int UnsavedLivetrackPoints = 0;

      /// <summary>
      /// Ist ein Track gerade in Bearbeitung?
      /// </summary>
      public bool TrackIsInWork => editHelper.TrackIsInWork;

      /// <summary>
      /// akt. bearbeiteter <see cref="Track"/> (oder null)
      /// </summary>
      public Track? TrackInEdit => editHelper.TrackInEdit;

      public bool MarkerIsInWork => editHelper.MarkerIsInWork;

      /// <summary>
      /// Ist eine <see cref="Marker"/> oder <see cref="Track"/> in Bearbeitung?
      /// </summary>
      public bool InWork => editHelper != null && (MarkerIsInWork || TrackIsInWork);

      DateTime FileDateTime = DateTime.MinValue;

      #endregion


      public GpxWorkbench(
#if ANDROID
                          MainPage page,
#endif
                          MapCtrl map,
                          FSofTUtils.Geography.DEM.DemData? dem,
                          string workbenchfile,
                          System.Drawing.Color colHelperLine,
                          float widthHelperLine,
                          System.Drawing.Color colTrack,
                          float widthTrack,
                          double symbolzoomfactor,
                          bool datachanged) {
#if ANDROID
         mainpage = page;
#endif
         this.map = map;
         Dem = dem;

         Gpx = load(workbenchfile, widthTrack, colTrack, symbolzoomfactor, datachanged);

         editHelper = new EditHelper(map, Gpx, colHelperLine, widthHelperLine);
         editHelper.MarkerShouldInsertEvent += (object? sender, EditHelper.MarkerEventArgs ea) => {
            MarkerShouldInsertEvent?.Invoke(sender, ea);
         };
         editHelper.TrackEditShowEvent += (object? sender, EditHelper.TrackEventArgs ea) => {
            TrackEditShowEvent?.Invoke(sender, ea);
         };
      }

      GpxData load(string gpxworkbenchfile,
                     double trackwidth,
                     System.Drawing.Color trackcolor,
                     double symbolzoomfactor,
                     bool datachanged) {
         GpxData gpx = new();                // Gleich hier global setzen weil ShowTrack() das benötigt!!!
         gpx.ExtLoadEvent += Gpx_LoadInfoEvent;
         gpx.TrackColor = trackcolor;
         gpx.TrackWidth = trackwidth;
         gpx.GpxFileEditable = true;
         gpx.GpxFilename = gpxworkbenchfile;
         if (File.Exists(gpxworkbenchfile)) {
            UIHelper.SetBusyStatus(null);
            List<System.Drawing.Color> trackcolors = gpx.Load(gpxworkbenchfile, true, trackcolor);
            for (int i = 0; i < gpx.TrackList.Count && i < trackcolors.Count; i++)
               gpx.TrackList[i].LineColor = trackcolors[i];
            UIHelper.SetBusyStatus(null, false);
            LoadInfoEvent?.Invoke(this, new LoadEventArgs(LoadEventArgs.Reason.ReadIsReady));
            FileDateTime = File.GetLastWriteTime(gpxworkbenchfile);
         }
         foreach (Marker marker in gpx.MarkerList)
            marker.Symbolzoom = symbolzoomfactor;
         gpx.GpxDataChanged = datachanged;
         gpx.ExtLoadEvent -= Gpx_LoadInfoEvent;
         return gpx;
      }

      private void Gpx_LoadInfoEvent(object? sender, GpxData.ExtLoadEventArgs e) {
         if (LoadInfoEvent != null) {
            LoadEventArgs? lea = null;
            switch (e.LoadReason) {
               case GpxData.ExtLoadEventArgs.Reason.ReadXml:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.ReadXml);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.ReadGDB:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.ReadGDB);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.ReadKml:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.ReadKml);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.InsertWaypoints:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.InsertWaypoints);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.InsertTracks:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.InsertTracks);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.InsertWaypoint:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.InsertWaypoint);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.InsertTrack:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.InsertTrack);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.SplitMultiSegmentTracks:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.SplitMultiSegmentTracks);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.RemoveEmptyTracks:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.RemoveEmptyTracks);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.RebuildTrackList:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.RebuildTrackList);
                  break;
               case GpxData.ExtLoadEventArgs.Reason.RebuildMarkerList:
                  lea = new LoadEventArgs(LoadEventArgs.Reason.RebuildMarkerList);
                  break;
            }
            if (lea != null)
               LoadInfoEvent.Invoke(this, lea);
         }
      }

      #region Hilfslinien zeichnen

      /// <summary>
      /// zeichnet eine Hilfslinie vom Ende des akt. bearbeiteten Tracks zum Clientpunkt
      /// </summary>
      /// <param name="g"></param>
      /// <param name="ptClient"></param>
      public void TrackDrawDestinationLine(Graphics g, System.Drawing.Point ptClient) =>
         editHelper.DrawHelperLine2LastTrackPoint(g, ptClient);

      /// <summary>
      /// zeichnet eine Hilfslinie vom Clientpunkt zum nächstgelegenen Trackpunkt des akt. bearbeiteten Tracks
      /// </summary>
      /// <param name="g"></param>
      /// <param name="ptClient"></param>
      public void TrackDrawNextTrackPoint(Graphics g, System.Drawing.Point ptClient) =>
         editHelper.DrawHelperLine2NextTrackPoint(g, ptClient);

      /// <summary>
      /// zeichnet eine Hilfslinie vom akt. bearbeiteten Track zum anzuhängenden Track
      /// </summary>
      /// <param name="g"></param>
      /// <param name="trackappend"></param>
      public void TrackDrawConcatLine(Graphics g, Track trackappend) =>
         editHelper.DrawHelperLine2NextTrack(g, trackappend);

      //public void ChangeHelperLineColor(System.Drawing.Color col) => editHelper.HelperLineColor = col;

      //public void ChangeHelperLineWidth(float width) => editHelper.HelperLineWidth = width;

      #endregion

      #region Trackbearbeitung

      /// <summary>
      /// start der Trackbearbeitung für diesen Track (oder einen neuen Track)
      /// </summary>
      /// <param name="cancellast">bei true wird eine ev. noch laufende Aktion abgebrochen</param>
      /// <param name="track">bei null neur Track</param>
      public bool TrackStartEdit(bool cancellast, Track? track) => editHelper.TrackEdit_Start(cancellast, track);

      /// <summary>
      /// fügt an den akt. bearbeiteten Track einen Punkt an
      /// <para>Falls kein Track bearbeitet wird, wird ein neuer Track erzeugt.</para>
      /// </summary>
      /// <param name="clientpt"></param>
      public bool TrackAddPoint(System.Drawing.Point clientpt) {
         if (!TrackIsInWork)
            editHelper.TrackEdit_Start(true, null);
         return editHelper.TrackEdit_AppendPoint(clientpt, Dem);
      }

      /// <summary>
      /// entfernt den letzten Punkt aus dem akt. bearbeiteten Track
      /// </summary>
      public bool TrackRemoveLastPoint() => editHelper.TrackEdit_RemoveLastPoint();

      /// <summary>
      /// löscht aus dem akt. bearbeiteten Track den nächstgelegenen Trackpunkt
      /// </summary>
      /// <param name="clientpt"></param>
      public bool TrackRemoveNextPoint(System.Drawing.Point clientpt) => editHelper.TrackEdit_RemoveNextPoint(clientpt);

      /// <summary>
      /// beendet das Zeichnen des akt. Tracks
      /// </summary>
      public void TrackEndEdit(bool cancel) {
         if (TrackIsInWork) {
            Track? t = editHelper.TrackInEdit;
            editHelper.TrackEdit_End(cancel);
            t?.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
         }
      }

      /// <summary>
      /// akt. bearbeiteter Track wird am nächstgelegenen Trackpunkt getrennt
      /// </summary>
      /// <param name="clientpt"></param>
      /// <param name="cancel">Operation abbrechen</param>
      public void TrackEndEdit(System.Drawing.Point clientpt, bool cancel) {
         if (TrackIsInWork) {
            Track? t = editHelper.TrackInEdit;
            editHelper.TrackEdit_End(clientpt, out _, cancel);
            t?.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
         }
      }

      /// <summary>
      /// an den akt. bearbeiteten Track wird ein anderer Track angehängt
      /// </summary>
      /// <param name="appendedtrack"></param>
      /// <param name="cancel">Operation abbrechen</param>
      public void TrackEndEdit(Track? appendedtrack, bool cancel) {
         if (TrackIsInWork) {
            Track? t = editHelper.TrackInEdit;
            editHelper.TrackEdit_End(cancel ? null : appendedtrack, cancel);
            t?.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
         }
      }

      #endregion

      /// <summary>
      /// löscht den Track
      /// </summary>
      /// <param name="track"></param>
      public void TrackRemove(Track track) => editHelper.Remove(track);

      /// <summary>
      /// liefert den Track aus der Trackliste
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public Track? GetTrack(int idx) => 0 <= idx && idx < TrackCount ? Gpx.TrackList[idx] : null;

      /// <summary>
      /// liefert die Liste der akt. Trackfarben
      /// <para>
      /// Wenn ein Track die Farbe <see cref="VisualTrack.EditableColor"/> hat, wird dafür <see cref="System.Drawing.Color.Empty"/>
      /// (schwarz, volltransparent) geliefert.
      /// </para>
      /// </summary>
      /// <returns></returns>
      public System.Drawing.Color[] GetTrackColors() {
         System.Drawing.Color[] trackcolor = new System.Drawing.Color[TrackCount];
         for (int i = 0; i < TrackCount; i++) {
            Track t = TrackList[i];
            // MS: ... For example, Black and FromArgb(0,0,0) are not considered equal, since Black is a named color and FromArgb(0,0,0) is not.
            //    => ToArgb() ist nötig
            trackcolor[i] = VisualTrack.EditableColor.ToArgb() == t.LineColor.ToArgb() ?
                                    System.Drawing.Color.Empty :
                                    t.LineColor;
         }
         return trackcolor;
      }

      #region Marker bearbeiten

      public void MarkerStartEdit(bool cancellast, Marker? marker) =>
         editHelper.MarkerEdit_Start(cancellast, marker);

      public void MarkerEndEdit(System.Drawing.Point clientpt, bool cancel) =>
         editHelper.MarkerEdit_End(clientpt, Dem, cancel);

      #endregion

      public Marker? MarkerInsertCopy(Marker orgmarker, int pos = 0) => editHelper.InsertCopy(orgmarker, pos);

      public Marker? GetMarker(int idx) => 0 <= idx && idx < MarkerCount ? Gpx.MarkerList[idx] : null;

      public void MarkerRemove(Marker marker) => editHelper.Remove(marker);


      #region Infos für geografischen Punkt

      /// <summary>
      /// holt Namensvorschläge für die Koordinaten aus einer Garminkarte oder der OSM
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public async Task<string[]?> GetNamesForGeoPointAsync(double lon, double lat) {
         string[]? names = null;

         int providx = map.M_ActualMapIdx;
         if (0 <= providx && providx < map.M_ProviderDefinitions.Count) {
            names = map.M_ProviderDefinitions[providx].Provider is GarminProvider ?
                        await getNamesForGeoPointFromGarminAsync(lon, lat) :
                        await getNamesForGeoPointFromOSMAsync(lon, lat);
         }

         return names;
      }

      async Task<string[]?> getNamesForGeoPointFromGarminAsync(double lon, double lat) {
         string[]? names = null;
         List<GarminImageCreator.SearchObject> info = await map.M_GetGarminObjectInfosAsync(map.M_LonLat2Client(lon, lat), 10, 10);
         if (info.Count > 0) {
            names = new string[info.Count];
            for (int i = 0; i < info.Count; i++)
               names[i] = !string.IsNullOrEmpty(info[i].Name) ?
                                       info[i].Name :
                                       info[i].TypeName;
         }
         return names;
      }

      static async Task<string[]?> getNamesForGeoPointFromOSMAsync(double lon, double lat) {
         string[]? names = null;
         GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = await GeoCodingReverseResultOsm.GetAsync(lon, lat, 10);
         if (geoCodingReverseResultOsms.Length > 0) {
            names = new string[geoCodingReverseResultOsms.Length];
            for (int i = 0; i < geoCodingReverseResultOsms.Length; i++)
               names[i] = geoCodingReverseResultOsms[i].Name;
         }
         return names;
      }

      #endregion

      /// <summary>
      /// synchron speichern
      /// </summary>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns></returns>
      public void Save(bool withxmlcolor = false) {
         UIHelper.SetBusyStatus(null);

         Gpx.SaveWithLock(InternalFilename,
                  string.Empty,
                  true,
                  GetTrackColors(),
                  FSofTUtils.Geography.GpxFileGarmin.STDGPXVERSION,
                  withxmlcolor);

         //for (int i = 0; i < TrackCount; i++)
         //   TrackList[i].LineColor = trackcolor[i];

         UIHelper.SetBusyStatus(null, false);
         Gpx.GpxDataChanged = false;
         UnsavedLivetrackPoints = 0;
         FileDateTime = DateTime.Now;
      }

      /// <summary>
      /// asynchron speichern
      /// </summary>
      /// <param name="withxmlcolor">wenn true dann XML-Farbe speichern</param>
      /// <returns></returns>
      public async Task SaveAsync(bool withxmlcolor) {
         UIHelper.SetBusyStatus(null);
         await Gpx.SaveAsyncWithLock(
                     InternalFilename,
                     string.Empty,
                     true,
                     GetTrackColors(),
                     FSofTUtils.Geography.GpxFileGarmin.STDGPXVERSION,
                     withxmlcolor);
         UIHelper.SetBusyStatus(null, false);
         Gpx.GpxDataChanged = false;
         UnsavedLivetrackPoints = 0;
         FileDateTime = DateTime.Now;
      }

      /// <summary>
      /// Marker neu anzeige (UpdateVisualMarker(mapCtrl))
      /// </summary>
      /// <param name="marker"></param>
      public void RefreshOnMap(Marker marker) => editHelper.RefreshOnMap(marker);

#if ANDROID
      // --- nur in Android verwendet

      public void RefreshMarkerWaypoint(Marker marker) {
         int idx = Gpx.MarkerIndex(marker);
         if (idx >= 0) {
            Gpx.GpxDataChanged = true;
            Gpx.Waypoints[idx] = marker.Waypoint;
            RefreshOnMap(marker);
         }
      }

#else
      // --- nur in Windows verwendet

      #region Tracks

      /// <summary>
      /// Track auf eine andere Listenposition schieben
      /// </summary>
      /// <param name="oldidx"></param>
      /// <param name="newidx"></param>
      public void TrackChangePositionInList(int oldidx, int newidx) => editHelper.TrackChangeOrder(oldidx, newidx);

      /// <summary>
      /// Ist dieser Track gerade in Bearbeitung?
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public bool IsThisTrackInWork(Track track) => editHelper.ThisTrackIsInWork(track);

      /// <summary>
      /// fügt eine Kopie des Tracks in die Trackliste ein
      /// </summary>
      /// <param name="orgtrack"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      public Track TrackInsertCopy(Track orgtrack, int pos = 0) => editHelper.InsertCopy(orgtrack, pos, true);

      /// <summary>
      /// löscht alle "sichtbaren" <see cref="Track"/>
      /// </summary>
      public void RemoveVisibleTracks() {
         for (int i = TrackCount - 1; 0 <= i; i--) {
            Track? track = GetTrack(i);
            if (track != null && track.IsVisible)
               TrackRemove(track);
         }
      }

      /// <summary>
      /// setzt die Farbe des Tracks
      /// </summary>
      /// <param name="track"></param>
      /// <param name="newcol"></param>
      public void SetTrackColor(Track track, System.Drawing.Color newcol) {
         track.LineColor = newcol;
         if (0 <= TrackIndex(track))
            DataChanged = true;
      }

      public List<Track> VisibleTracks() {
         List<Track> lst = [];
         foreach (var t in Gpx.TrackList)
            if (t.IsVisible)
               lst.Add(t);
         return lst;
      }

      #endregion

      #region Marker

      public void MarkerDrawDestinationLine(Graphics g, System.Drawing.Point clientpt) => editHelper.DrawHelperLine2NewMarkerPosition(g, clientpt);

      public void MarkerChangePositionInList(int oldidx, int newidx) => editHelper.MarkerChangeOrder(oldidx, newidx);

      public void MarkerEndEdit(bool cancel) => editHelper.MarkerEdit_End(cancel);

      public bool MarkerReplaceWaypoint(Marker orgmarker, Marker markerwithnewwaypoint) {
         int idx = MarkerIndex(orgmarker);
         if (0 <= idx && idx < MarkerCount) {
            Gpx.Waypoints[idx] = markerwithnewwaypoint.Waypoint;
            DataChanged = true;
            RefreshOnMap(orgmarker);
            return true;
         }
         return false;
      }

      /// <summary>
      /// löscht alle "sichtbaren" <see cref="Marker"/>
      /// </summary>
      public void RemoveVisibleMarkers() {
         for (int i = MarkerCount - 1; 0 <= i; i--) {
            Marker? marker = GetMarker(i);
            if (marker != null && marker.IsVisible)
               MarkerRemove(marker);
         }
      }

      public List<Marker> VisibleMarkers() {
         List<Marker> lst = [];
         foreach (var t in Gpx.MarkerList)
            if (t.IsVisible)
               lst.Add(t);
         return lst;
      }

      #endregion

      /// <summary>
      /// ändert die <see cref="Track.Trackname"/> und <see cref="Marker.Text"/> bei Bedarf, so dass sie eindeutig sind
      /// </summary>
      /// <param name="changedmarker">Indexliste der geänderten <see cref="Marker"/></param>
      /// <param name="changedtracks">Indexliste der geänderten <see cref="Track"/></param>
      /// <returns></returns>
      public bool SetUniqueNames4TracksAndMarkers(out List<int> changedmarker, out List<int> changedtracks) {
         changedmarker = [];
         changedtracks = [];
         SortedSet<string> testnames = [];

         for (int i = 0; i < MarkerCount; i++) {
            Marker? marker = GetMarker(i);
            if (marker != null) {
               string name = marker.Text;
               int no = 2;
               while (testnames.Contains(name)) {
                  name = marker.Text + " (" + no++ + ")";
               }
               if (marker.Text != name) {
                  marker.Text = name;
                  changedmarker.Add(i);
                  RefreshOnMap(marker);
                  DataChanged = true;
               }
               testnames.Add(name);
            }
         }

         testnames.Clear();
         for (int i = 0; i < TrackCount; i++) {
            Track? track = GetTrack(i);
            if (track != null) {
               string name = track.Trackname;
               int no = 2;
               while (testnames.Contains(name)) {
                  name = track.Trackname + " (" + no++ + ")";
               }
               if (track.Trackname != name) {
                  track.Trackname = name;
                  changedtracks.Add(i);
                  InEditRefresh();
                  DataChanged = true;
               }
               testnames.Add(name);
            }
         }

         return changedmarker.Count + changedtracks.Count > 0;
      }

      public void RefreshCursor() => editHelper.RefreshCursor();

      public void InEditRefresh() => editHelper.Refresh();

      public double GetHeight(System.Drawing.Point clientpt) => editHelper.GetHeight(clientpt, Dem);

#endif

   }
}