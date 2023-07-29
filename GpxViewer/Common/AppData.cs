#if Android
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace TrackEddi.Common {

   public class AppData {

      public AppData() {
         LastFullSaveFilename = "";
      }

      #region private Funktionen

      static bool Get(string name, bool def) {
         if (Application.Current.Properties.ContainsKey(name))
            return Convert.ToBoolean(Application.Current.Properties[name]);
         return def;
      }

      static string Get(string name, string def) {
         if (Application.Current.Properties.ContainsKey(name))
            return Application.Current.Properties[name].ToString().Trim();
         return def;
      }

      static int Get(string name, int def) {
         if (Application.Current.Properties.ContainsKey(name))
            try {
               return Convert.ToInt32(Application.Current.Properties[name]);
            } catch { }
         return def;
      }

      static double Get(string name, double def) {
         if (Application.Current.Properties.ContainsKey(name))
            try {
               return Convert.ToDouble(Application.Current.Properties[name]);
            } catch { }
         return def;
      }

      static void Set(string name, object value) {
         Application.Current.Properties[name] = value;
      }

      static void SetList<T>(string name, List<T> lst, string separator = "\n") {
         Set(name, string.Join(separator, lst));
      }

      static List<T> GetList<T>(string name, string separator = "\n") {
         List<T> lst = new List<T>();
         string txt = Get(name, null);
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
using FSofTUtilsAssembly = FSofTUtils;
using System.Collections.Generic;

namespace GpxViewer.Common {
   public class AppData : FSofTUtilsAssembly.AppData {

      public AppData(string name, string folder = null) : base(name, false, "persist.xml", folder) { }

      #region private Funktionen

      string Get(string name, string def) =>
         data.Get(name, def);

      bool Get(string name, bool def) =>
         data.Get(name, def);

      int Get(string name, int def) =>
         data.Get(name, def);

      double Get(string name, double def) =>
         data.Get(name, def);

      List<T> GetList<T>(string name, string separator = "\n") =>
        data.GetList<T>(name, separator);

      void Set(string name, object value) =>
         data.Set(name, value);

      void SetList<T>(string name, List<T> lst, string separator = "\n") =>
         data.SetList(name, lst, separator);

      #endregion

#endif

      /// <summary>
      /// bei Programmbeendigung verwendeter Kartenname
      /// </summary>
      public string LastMapname {
         get => Get(nameof(LastMapname), "");
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

#if Android

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
         get => Get(nameof(LastLoadSavePath), "");
         set => Set(nameof(LastLoadSavePath), value);
      }

      /// <summary>
      /// letzter Dateiname zum speichern einer GPX-Datei
      /// </summary>
      public string LastFullSaveFilename {
         get => Get(nameof(LastFullSaveFilename), "");
         set => Set(nameof(LastFullSaveFilename), value);
      }

      public string LastSearchPattern {
         get => Get(nameof(LastSearchPattern), "");
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

#else

      public string LastPicturePath {
         get => Get(nameof(LastPicturePath), "");
         set => Set(nameof(LastPicturePath), value);
      }

#endif

   }
}
