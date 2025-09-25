using FSofTUtilsAssembly = FSofTUtils;
#if ANDROID || ANDROID2
namespace TrackEddi.Common {
#if ANDROID2

   public class AppData {

      public AppData() {
         LastFullSaveFilename = "";
      }

      #region private Funktionen

      static bool Get(string name, bool def) {
         return Preferences.Get(name, def);
      }

      static string Get(string name, string def) {
         return Preferences.Get(name, def).Trim();
      }

      static int Get(string name, int def) {
         return Preferences.Get(name, def);
      }

      static double Get(string name, double def) {
         return Preferences.Get(name, def);
      }

      static void Set(string name, int value) => Preferences.Set(name, value);

      static void Set(string name, double value) => Preferences.Set(name, value);

      static void Set(string name, bool value) => Preferences.Set(name, value);

      static void Set(string name, string value) => Preferences.Set(name, value);

      static void SetList<T>(string name, List<T> lst, string separator = "\n") {
         Set(name, string.Join(separator, lst));
      }

      static List<T> GetList<T>(string name, string separator = "\n") {
         List<T> lst = new List<T>();
         string txt = Get(name, string.Empty);
         if (!string.IsNullOrEmpty(txt))
            foreach (string item in txt.Split(new string[] { separator }, StringSplitOptions.None)) {
               lst.Add((T)Convert.ChangeType(item, typeof(T)));
            }
         return lst;
      }

      //static int TextAsInt(string txt) {
      //   try {
      //      return Convert.ToInt32(txt);
      //   } catch {
      //      return -1;
      //   }
      //}

      //static string IntAsText(int v) {
      //   return v >= 0 ? v.ToString() : "";
      //}

      //static double TextAsDouble(string txt) {
      //   try {
      //      return Convert.ToDouble(txt);
      //   } catch {
      //      return -1;
      //   }
      //}

      //static string DoubleAsText(double v) {
      //   return v >= 0 ? v.ToString() : "";
      //}

      //static void RemoveData(string name) {
      //   if (Application.Current.Properties.ContainsKey(name))
      //      Application.Current.Properties.Remove(name);
      //}

      // ======================================================

      #endregion

#else
   public class AppData : FSofTUtilsAssembly.AppData {

      public AppData(string name, string? folder = null) : base(name, false, "persist.xml", folder) {
         LastFullSaveFilename = "";
      }

#endif

#else
namespace GpxViewer.Common {

      public class AppData : FSofTUtilsAssembly.AppData {

      public AppData(string name, string? folder = null) : base(name, false, "persist.xml", folder) { }
#endif

#if !ANDROID || (ANDROID && !ANDROID2)
      #region private Funktionen

      string? Get(string name, string def) => data != null ? data.Get(name, def) : def;

      bool Get(string name, bool def) => data != null ? data.Get(name, def) : def;

      int Get(string name, int def) => data != null ? data.Get(name, def) : def;

      double Get(string name, double def) => data != null ? data.Get(name, def) : def;

      List<T> GetList<T>(string name, string separator = "\n") {
         List<T>? lst = data?.GetList<T>(name, separator);
         return lst != null ? lst : new List<T>();
      }

      void Set(string name, object value) => data?.Set(name, value);

      void SetList<T>(string name, List<T> lst, string separator = "\n") =>
         data?.SetList(name, lst, separator);

      #endregion

#endif

      /// <summary>
      /// bei Programmbeendigung verwendeter Kartenname
      /// </summary>
      public string LastMapname {
         get => Get(nameof(LastMapname), "") ?? string.Empty;
         set => Set(nameof(LastMapname), value);
      }

      public List<string> LastUsedMapnames {
         get => GetList<string>(nameof(LastUsedMapnames));
         set => SetList(nameof(LastUsedMapnames), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendeter Zoom
      /// </summary>
      public double LastZoom {
         get => Get(nameof(LastZoom), 14.0);
         set => Set(nameof(LastZoom), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendete Latitude
      /// </summary>
      public double LastLatitude {
         get => Get(nameof(LastLatitude), 51.25);
         set => Set(nameof(LastLatitude), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendete Longitude
      /// </summary>
      public double LastLongitude {
         get => Get(nameof(LastLongitude), 12.33);
         set => Set(nameof(LastLongitude), value);
      }

      /// <summary>
      /// Liste der gespeicherten Positionen (Zoom, Position, Name)
      /// </summary>
      public List<string> PositionList {
         get => GetList<string>(nameof(PositionList));
         set => SetList(nameof(PositionList), value);
      }

      /// <summary>
      /// Liste der gespeicherten GPX-"Vereinfachungen"
      /// </summary>
      public List<string> SimplifyDatasetList {
         get => GetList<string>(nameof(SimplifyDatasetList));
         set => SetList(nameof(SimplifyDatasetList), value);
      }

      /// <summary>
      /// Wurden die akt. Gpx-Daten geändert (d.h. noch ungespeichert)?
      /// </summary>
      public bool GpxDataChanged {
         get => Get(nameof(GpxDataChanged), false);
         set => Set(nameof(GpxDataChanged), value);
      }

      public List<bool> VisibleStatusTrackList {
         get => GetList<bool>(nameof(VisibleStatusTrackList));
         set => SetList(nameof(VisibleStatusTrackList), value);
      }

      public List<bool> VisibleStatusMarkerList {
         get => GetList<bool>(nameof(VisibleStatusMarkerList));
         set => SetList(nameof(VisibleStatusMarkerList), value);
      }

      public bool IsCreated {
         get => Get(nameof(IsCreated), false);
         set => Set(nameof(IsCreated), value);
      }

#if ANDROID

      /// <summary>
      /// Anzeige von GPX-Infos
      /// </summary>
      public bool ShowInfo {
         get => Get(nameof(ShowInfo), true);
         set => Set(nameof(ShowInfo), value);
      }

      /// <summary>
      /// letzter verwendeter Pfad für Öffnen oder Speichern einer GPX-Datei
      /// </summary>
      public string LastLoadSavePath {
         get => Get(nameof(LastLoadSavePath), "") ?? string.Empty;
         set => Set(nameof(LastLoadSavePath), value);
      }

      /// <summary>
      /// letzter Dateiname zum speichern einer GPX-Datei
      /// </summary>
      public string LastFullSaveFilename {
         get => Get(nameof(LastFullSaveFilename), "") ?? string.Empty;
         set => Set(nameof(LastFullSaveFilename), value);
      }

      public string LastSearchPattern {
         get => Get(nameof(LastSearchPattern), "") ?? string.Empty;
         set => Set(nameof(LastSearchPattern), value);
      }

      public List<string> LastSearchResults {
         get => GetList<string>(nameof(LastSearchResults));
         set => SetList(nameof(LastSearchResults), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendete Latitude der letzten Location
      /// </summary>
      public double LastLocationLatitude {
         get => Get(nameof(LastLocationLatitude), 51.25);
         set => Set(nameof(LastLocationLatitude), value);
      }

      /// <summary>
      /// bei Programmbeendigung verwendete Longitude der letzten Location
      /// </summary>
      public double LastLocationLongitude {
         get => Get(nameof(LastLocationLongitude), 12.33);
         set => Set(nameof(LastLocationLongitude), value);
      }

      public string LastGpxSearchPath {
         get => Get(nameof(LastGpxSearchPath), "") ?? string.Empty;
         set => Set(nameof(LastGpxSearchPath), value);
      }

#else

      public string LastPicturePath {
         get => Get(nameof(LastPicturePath), "") ?? string.Empty;
         set => Set(nameof(LastPicturePath), value);
      }

#endif

   }
}
