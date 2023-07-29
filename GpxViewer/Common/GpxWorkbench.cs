using FSofTUtils.Geography.GeoCoding;
using GMap.NET.CoreExt.MapProviders;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Drawing;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;
using System.IO;
using System.Threading.Tasks;
#if Android
using GMap.NET.Skia;
using Xamarin.Forms;
#else

#endif

#if Android
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif
   public class GpxWorkbench {

#if Android
      /// <summary>
      /// Masterpage
      /// </summary>
      MainPage mainpage;
#endif

      public class LoadEventArgs {

         public string Info;

         public LoadEventArgs(string info) {
            Info = info;
         }

      }


      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      FSofTUtils.Geography.DEM.DemData Dem = null;

      EditHelper editHelper = null;

      MapCtrl Map;

      /// <summary>
      /// alle akt. GPX-Daten
      /// </summary>
      public readonly GpxAllExt Gpx;

      // "Abkürzungen"

      public string InternalFilename => Gpx.GpxFilename;

      public int TrackCount => Gpx.TrackList.Count;

      public int MarkerCount => Gpx.MarkerList.Count;

      public List<Track> TrackList => Gpx.TrackList;

      public List<Marker> MarkerList => Gpx.MarkerList;

      public int TrackIndex(Track track) => Gpx.TrackIndex(track);

      public int MarkerIndex(Marker marker) => Gpx.MarkerIndex(marker);

      public List<bool> VisibleStatusMarkerList {
         get {
            List<bool> lst = new List<bool>();
            foreach (var marker in Gpx.MarkerList)
               lst.Add(marker.IsVisible);
            return lst;
         }
      }

      public List<bool> VisibleStatusTrackList {
         get {
            List<bool> lst = new List<bool>();
            foreach (var track in Gpx.TrackList)
               lst.Add(track.IsVisible);
            return lst;
         }
      }

      public bool DataChanged {
         get => Gpx.GpxDataChanged;
         set => Gpx.GpxDataChanged = value;
      }


      public static event EventHandler<LoadEventArgs> LoadInfoEvent;

      public event EventHandler RefreshProgramStateEvent;

      /// <summary>
      /// ein neuer Marker sollte eingefügt werden
      /// </summary>
      public event EventHandler<EditHelper.MarkerEventArgs> MarkerShouldInsertEvent;

      /// <summary>
      /// die Anzeige eines Tracks wird ein- oder ausgeschaltet
      /// </summary>
      public event EventHandler<EditHelper.TrackEventArgs> TrackEditShowEvent;

      /// <summary>
      /// akt. bearbeiteter <see cref="Track"/> (oder null)
      /// </summary>
      public Track TrackInEdit =>
         editHelper?.TrackInEdit;

      /// <summary>
      /// Ist eine <see cref="Marker"/> oder <see cref="Track"/> in Bearbeitung?
      /// </summary>
      public bool InWork =>
         editHelper.MarkerInWork || editHelper.TrackInWork;

      /// <summary>
      /// public nur für Android
      /// </summary>
#if Android
      public
#endif
      Track MarkedTrack = null;



      public GpxWorkbench(
#if Android
                          MainPage page,
#endif
                          MapCtrl map,
                          FSofTUtils.Geography.DEM.DemData dem,
                          string workbenchfile,
                          System.Drawing.Color colHelperLine,
                          float widthHelperLine,
                          System.Drawing.Color colTrack,
                          float widthTrack,
                          double symbolzoomfactor,
                          bool datachanged) {
#if Android
         mainpage = page;
#endif
         Map = map;
         Dem = dem;

         Gpx = load(workbenchfile, widthTrack, colTrack, symbolzoomfactor, datachanged);

         if (editHelper == null) {
            editHelper = new EditHelper(map, Gpx, colHelperLine, widthHelperLine);
            editHelper.MarkerShouldInsertEvent += (object sender, EditHelper.MarkerEventArgs ea) => {
               MarkerShouldInsertEvent?.Invoke(sender, ea);
            };
            editHelper.TrackEditShowEvent += (object sender, EditHelper.TrackEventArgs ea) => {
               TrackEditShowEvent?.Invoke(sender, ea);
            };
            editHelper.RefreshProgramStateEvent += (object sender, EventArgs ea) => {
               RefreshProgramStateEvent?.Invoke(sender, ea);
            };
         }
      }

      GpxAllExt load(string gpxworkbenchfile,
                     double trackwidth,
                     System.Drawing.Color trackcolor,
                     double symbolzoomfactor,
                     bool datachanged) {
         GpxAllExt gpx = new GpxAllExt();                // Gleich hier global setzen weil ShowTrack() das benötigt!!!
         gpx.LoadInfoEvent += Gpx_LoadInfoEvent;
         gpx.TrackColor = trackcolor;
         gpx.TrackWidth = trackwidth;
         gpx.GpxFileEditable = true;
         if (File.Exists(gpxworkbenchfile)) {
            gpx.Load(gpxworkbenchfile, true);
            LoadInfoEvent?.Invoke(this, new LoadEventArgs("Workbenchdatei geladen"));
            gpx.GpxFilename = gpxworkbenchfile;

            foreach (Marker marker in gpx.MarkerList)
               marker.Symbolzoom = symbolzoomfactor;
         } else
            gpx.GpxFilename = gpxworkbenchfile;
         gpx.GpxDataChanged = datachanged;
         gpx.LoadInfoEvent -= Gpx_LoadInfoEvent;
         return gpx;
      }

      private void Gpx_LoadInfoEvent(object sender, GpxAllExt.LoadEventArgs e) {
         LoadInfoEvent?.Invoke(this, new LoadEventArgs(e.Info));
      }

      public void TrackDrawDestinationLine(Graphics g, System.Drawing.Point destpt) => editHelper.TrackDrawDestinationLine(g, destpt);

      public void TrackDrawSplitPoint(Graphics g, System.Drawing.Point destpt) => editHelper.TrackDrawSplitPoint(g, destpt);

      public void TrackDrawConcatLine(Graphics g, Track trackappend) => editHelper.TrackDrawConcatLine(g, trackappend);

      /// <summary>
      /// fügt an den akt. bearbeiteten Track einen Punkt an
      /// <para>Falls kein Track bearbeitet wird, wird ein neuer Track erzeugt.</para>
      /// </summary>
      /// <param name="clientpt"></param>
      public void TrackAddPoint(System.Drawing.Point clientpt) {
         if (!editHelper.TrackInWork)
            editHelper.TrackEditStart();
         editHelper.TrackEditDraw_AppendPoint(clientpt, Dem);
      }

      /// <summary>
      /// entfernt den letzten Punkt aus dem akt. bearbeiteten Track
      /// </summary>
      public void TrackRemovePoint() {
         if (editHelper.TrackInWork)
            editHelper.TrackEditDraw_RemoveLastPoint();
      }

      public void TrackEndDraw() {
         if (editHelper.TrackInWork) {
            Track t = editHelper.TrackInEdit;
            editHelper.TrackEditEndDraw();
            t.UpdateVisualTrack(Map); // "echte" Farbe statt Farbe für editierbare Tracks
         }
         MarkedTrack = null;
      }

      public Track GetTrack(int idx) => 0 <= idx && idx < TrackCount ? Gpx.TrackList[idx] : null;

      public void TrackRemove(Track track) => editHelper.Remove(track);


      public Marker MarkerInsertCopy(Marker orgmarker, int pos = 0) => editHelper.InsertCopy(orgmarker, pos);

      public Marker GetMarker(int idx) => 0 <= idx && idx < MarkerCount ? Gpx.MarkerList[idx] : null;

      public void MarkerRemove(Marker marker) => editHelper.Remove(marker);

      public void RefreshOnMap(Marker marker) => editHelper.RefreshOnMap(marker);


      /// <summary>
      /// holt Namensvorschläge für die Koordinaten aus einer Garminkarte oder der OSM
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public string[] GetNamesForGeoPoint(double lon, double lat) {
         string[] names = null;

         int providx = Map.SpecMapActualMapIdx;
         if (0 <= providx && providx < Map.SpecMapProviderDefinitions.Count) {
            names = Map.SpecMapProviderDefinitions[providx].Provider is GarminProvider ?
                        getNamesForGeoPointFromGarmin(lon, lat) :
                        getNamesForGeoPointFromOSM(lon, lat);
         }

         return names;
      }

      string[] getNamesForGeoPointFromGarmin(double lon, double lat) {
         string[] names = null;
         List<GarminImageCreator.SearchObject> info = Map.SpecMapGetGarminObjectInfos(Map.SpecMapLonLat2Client(lon, lat), 10, 10);
         if (info.Count > 0) {
            names = new string[info.Count];
            for (int i = 0; i < info.Count; i++)
               names[i] = !string.IsNullOrEmpty(info[i].Name) ?
                                       info[i].Name :
                                       info[i].TypeName;
         }
         return names;
      }

      string[] getNamesForGeoPointFromOSM(double lon, double lat) {
         string[] names = null;
         GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = GeoCodingReverseResultOsm.Get(lon, lat);
         if (geoCodingReverseResultOsms.Length > 0) {
            names = new string[geoCodingReverseResultOsms.Length];
            for (int i = 0; i < geoCodingReverseResultOsms.Length; i++)
               names[i] = geoCodingReverseResultOsms[i].Name;
         }
         return names;
      }

      public async Task<string[]> GetNamesForGeoPointAsync(double lon, double lat) {
         string[] names = null;
         await Task.Run(() => names = GetNamesForGeoPoint(lon, lat));
         return names;
      }


      public void Save() {
         Gpx.Save(Gpx.GpxFilename, "", true);
         Gpx.GpxDataChanged = false;
      }


      #region NUR Android

      public void RefreshMarkerWaypoint(Marker marker) {
         int idx = Gpx.MarkerIndex(marker);
         if (idx >= 0) {
            Gpx.GpxDataChanged = true;
            Gpx.Waypoints[idx] = marker.Waypoint;
            RefreshOnMap(marker);
         }
      }

      public void RefreshTrackProps(Track track, Track trackchanged) {
         track.LineColor = trackchanged.LineColor;
         track.GpxTrack.Name = trackchanged.GpxTrack.Name;
         track.GpxTrack.Description = trackchanged.GpxTrack.Description;
         track.GpxTrack.Comment = trackchanged.GpxTrack.Comment;
         track.GpxTrack.Source = trackchanged.GpxTrack.Source;
      }

      public void StartMarkerMove(Marker marker) {
         editHelper.MarkerEditStart(marker);
         Map.SpecMapRefresh(true, false, false);
      }

      public void MarkerNew(System.Drawing.Point clientpoint) {
         editHelper.MarkerEditStart();
         // löst EditMarkerHelper_MarkerShouldInsertEvent() aus:
         editHelper.MarkerEditSetNewPos(clientpoint, Dem);
      }

      public void StartTrackDraw(Track track = null) {
         if (editHelper.TrackInWork)
            editHelper.TrackEditEndDraw();
         editHelper.TrackEditStart(track);
         MarkedTrack = null;
      }

#if Android
      /// <summary>
      /// Abbruch oder falls noch kein Marker "InWork" neuer Marker, sonst neue Position für den Maker "InWork"
      /// </summary>
      /// <param name="dest"></param>
      /// <param name="cancel"></param>
      public async void EndMarker(System.Drawing.Point dest, bool cancel = false) {
         if (!cancel) {
            if (editHelper.MarkerInWork)        // move marked marker
               editHelper.MarkerEditSetNewPos(dest, Dem);
            else
               await mainpage.SetNewMarker(dest);        // set new marker
         } else {
            if (editHelper.MarkerInWork)
               editHelper.MarkerEditEnd();
         }
      }

      public void EndTrackSplit(bool cancel = false) {
         if (editHelper.TrackInWork) {
            Track t = editHelper.TrackInEdit;
            if (!cancel) {
               Track newtrack = editHelper.TrackEndSplit(mainpage.ClientMapCenter);
               t.UpdateVisualTrack(Map);           // "echte" Farbe statt Farbe für editierbare Tracks
               if (Gpx.TrackList.Count > 0) {      // letzten Track noch sichtbar machen
                  newtrack.IsMarked4Edit = false;
                  mainpage.ShowTrack(newtrack);
                  newtrack.UpdateVisualTrack(Map);
               }
            } else {
               editHelper.TrackEndSplit(System.Drawing.Point.Empty);
               t.UpdateVisualTrack(Map);           // "echte" Farbe statt Farbe für editierbare Tracks
            }
         }
         MarkedTrack = null;
      }
#endif

      public void EndTrackConcat(bool cancel = false) {
         if (editHelper.TrackInWork &&
             MarkedTrack != null) {
            if (!cancel) {
               editHelper.TrackEndConcat(MarkedTrack);
            } else {
               editHelper.TrackEndConcat(null);
            }
         }
         MarkedTrack = null;
      }

#if Android

      /// <summary>
      /// Wenn eine Editieraktion läuft, wird gefragt ob diese abgebrochen werden soll.
      /// </summary>
      /// <returns>true wenn keine Editieraktion mehr aktiv ist</returns>
      public
#if Android
         async Task<bool>
#else
         bool
#endif
         Cancel() {
         bool canceled = false;
         bool inwork = false;
         if (editHelper.MarkerInWork) {
            inwork = true;
            if (
#if Android
                await UIHelper.ShowYesNoQuestion_StdIsYes(mainpage,
#else
                UIHelper.ShowYesNoQuestion_IsYes(
#endif
                   "Abbrechen?", "Marker setzen/bearbeiten")) {
               editHelper.MarkerEditEnd(true);
               MarkedTrack = null;
               canceled = true;
            }
         } else if (editHelper.TrackInWork) {
            inwork = true;
            if (
#if Android
                await UIHelper.ShowYesNoQuestion_StdIsYes(mainpage,
#else


                UIHelper.ShowYesNoQuestion_IsYes(
#endif
                   "Abbrechen?", "Track bearbeiten")) {
               Track t = editHelper.TrackInEdit;
               switch (mainpage.ProgramState) {
                  //case ProgState.Edit_TrackMark4Edit:
                  //case ProgState.Edit_TrackMark4Split:
                  //case ProgState.Edit_TrackMark4Concat:
                  //   editHelper.EditEndDraw();
                  //   break;

                  case MainPage.ProgState.Edit_TrackDraw:
                     editHelper.TrackEditEndDraw(true);
                     t.UpdateVisualTrack(Map); // "echte" Farbe statt Farbe für editierbare Tracks
                     break;

                  case MainPage.ProgState.Edit_TrackSplit:
                     editHelper.TrackEndSplit(System.Drawing.Point.Empty);
                     t.UpdateVisualTrack(Map); // "echte" Farbe statt Farbe für editierbare Tracks
                     break;

                  case MainPage.ProgState.Edit_TrackConcat:
                     editHelper.TrackEndConcat(null);
                     t.UpdateVisualTrack(Map); // "echte" Farbe statt Farbe für editierbare Tracks
                     break;
               }
               MarkedTrack = null;
               canceled = true;
            }
         }
         return !inwork || canceled;
      }

      ///// <summary>
      ///// zeichnet die Hilfslinie
      ///// </summary>
      ///// <param name="e"></param>
      //public void MapDrawOnTop(GMapControl.DrawExtendedEventArgs e) {
      //   switch (mainpage.ProgramState) {
      //      case MainPage.ProgState.Edit_Marker:
      //         editHelper.TrackDrawDestinationLine(e.Graphics, mainpage.ClientMapCenter);
      //         break;

      //      case MainPage.ProgState.Edit_TrackDraw:
      //         editHelper.TrackDrawDestinationLine(e.Graphics, mainpage.ClientMapCenter);
      //         break;

      //      case MainPage.ProgState.Edit_TrackSplit:
      //         editHelper.TrackDrawSplitPoint(e.Graphics, mainpage.ClientMapCenter);
      //         break;

      //      case MainPage.ProgState.Edit_TrackConcat:
      //         if (MarkedTrack != null)
      //            editHelper.TrackDrawConcatLine(e.Graphics, MarkedTrack);
      //         break;

      //   }
      //}
#endif

      /// <summary>
      /// Dateiinhalt anhängen oder als neuer Inhalt der <see cref="GpxWorkbench"/>
      /// </summary>
      /// <param name="page"></param>
      /// <param name="file"></param>
      /// <param name="append"></param>
      /// <param name="linewidth"></param>
      /// <param name="symbolzoomfactor"></param>
      /// <returns></returns>
#if Android
      public async Task Load(
                  Page page,
#else
      public void Load(
#endif
                  string file,
                  bool append,
                  double linewidth,
                  double symbolzoomfactor) {
#if Android
         await IOHelper.Load(
                           page,
#else
         IOHelper.Load(
#endif
                           Gpx,
                           file,
                           append,
                           linewidth,
                           symbolzoomfactor);
      }

      public void VisualRefresh() =>
         Gpx.VisualRefresh();



      #endregion

      #region NUR GpxViewer

      public void TrackChangePositionInList(int oldidx, int newidx) => editHelper.TrackChangeOrder(oldidx, newidx);

      public bool IsThisTrackInWork(Track track) => editHelper.TrackIsInWork(track);

      public bool TrackIsInWork => editHelper.TrackInWork;

      public void TrackStartEdit(Track track = null) => editHelper.TrackEditStart(track);

      public void TrackEndSplit(System.Drawing.Point clientpt) {
         if (editHelper.TrackInWork)
            editHelper.TrackEndSplit(clientpt);
      }

      public void TrackEndConcat(Track appendedtrack, bool cancel = false) {
         if (editHelper.TrackInWork &&
             appendedtrack != null) {
            if (!cancel) {
               editHelper.TrackEndConcat(appendedtrack);
            } else {
               editHelper.TrackEndConcat(null);
            }
         }
         MarkedTrack = null;
      }

      public Track TrackInsertCopy(Track orgtrack, int pos = 0) => editHelper.InsertCopy(orgtrack, pos, true);

      /// <summary>
      /// löscht alle "sichtbaren" <see cref="Track"/>
      /// </summary>
      public void RemoveVisibleTracks() {
         for (int i = TrackCount - 1; 0 <= i; i--) {
            Track track = GetTrack(i);
            if (track.IsVisible)
               TrackRemove(track);
         }
      }

      public void SetTrackColor(Track track, System.Drawing.Color newcol) {
         track.LineColor = newcol;
         if (0 <= TrackIndex(track))
            DataChanged = true;
      }



      public void MarkerDrawDestinationLine(Graphics g, System.Drawing.Point clientpt) => editHelper.MarkerEditDrawDestinationLine(g, clientpt);

      public void MarkerChangePositionInList(int oldidx, int newidx) => editHelper.MarkerChangeOrder(oldidx, newidx);

      public void MarkerStartEdit(Marker marker = null) => editHelper.MarkerEditStart(marker);

      public bool MarkerIsInWork() => editHelper.MarkerInWork;

      public void MarkerEndEdit() => editHelper.MarkerEditEnd();

      public void MarkerEndEdit(System.Drawing.Point clientpt) {
         editHelper.MarkerEditSetNewPos(clientpt, Dem);
         editHelper.MarkerEditEnd();
      }

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
            Marker marker = GetMarker(i);
            if (marker.IsVisible)
               MarkerRemove(marker);
         }
      }

      public List<Marker> VisibleMarkers() {
         List<Marker> lst = new List<Marker>();
         foreach (var t in Gpx.MarkerList)
            if (t.IsVisible)
               lst.Add(t);
         return lst;
      }

      public List<Track> VisibleTracks() {
         List<Track> lst = new List<Track>();
         foreach (var t in Gpx.TrackList)
            if (t.IsVisible)
               lst.Add(t);
         return lst;
      }



      public void RefreshCursor() => editHelper.RefreshCursor();

      public void InEditRefresh() => editHelper.Refresh();

      /// <summary>
      /// ändert die <see cref="Track.Trackname"/> und <see cref="Marker.Text"/> bei Bedarf, so dass sie eindeutig sind
      /// </summary>
      /// <param name="changedmarker">Indexliste der geänderten <see cref="Marker"/></param>
      /// <param name="changedtracks">Indexliste der geänderten <see cref="Track"/></param>
      /// <returns></returns>
      public bool SetUniqueNames4TracksAndMarkers(out List<int> changedmarker, out List<int> changedtracks) {
         changedmarker = new List<int>();
         changedtracks = new List<int>();
         SortedSet<string> testnames = new SortedSet<string>();

         for (int i = 0; i < MarkerCount; i++) {
            Marker marker = GetMarker(i);
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

         testnames.Clear();
         for (int i = 0; i < TrackCount; i++) {
            Track track = GetTrack(i);
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

         return changedmarker.Count + changedtracks.Count > 0;
      }

      public double GetHeight(System.Drawing.Point clientpt) => editHelper.GetHeight(clientpt, Dem);

      public void ChangeHelperLineColor(System.Drawing.Color col) => editHelper.HelperLineColor = col;

      public void ChangeHelperLineWidth(float width) => editHelper.HelperLineWidth = width;

      #endregion


   }
}