using FSofTUtils;

#if ANDROID
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif

   /// <summary>
   /// zum lesen einer Liste von GPX-Dateien
   /// </summary>
   class GpxListReader {

      const string XML_ROOT = "gpxlist";

      const string XML_NAME = "name";

      const string XML_GROUP = "group";
      const string XML_GROUPNAME = "@name";

      const string XML_GPX = "gpxfile";
      const string XML_GPXNAME = "@name";
      const string XML_GPXFILENAME = "@gpxfile";
      const string XML_GPXTRACKCOLORNO = "@trackcolorno";
      const string XML_GPXPICTUREGPXFILENAME = "@picturegpxfile";


      SimpleXmlDocument2 list;

      /// <summary>
      /// Anzahl der Listengruppen
      /// </summary>
      public int Groups {
         get {
            SimpleXmlDocument2.XPathResult[]? xPathResults = list.ReadValues("/" + XML_ROOT + "/" + XML_GROUP);
            return xPathResults != null ? xPathResults.Length : 0;
         }
      }

      /// <summary>
      /// Name der Listendatei
      /// </summary>
      public string Name =>
               list.ReadValue("/" + XML_ROOT + "/" + XML_NAME, "");

      public GpxListReader(string filename) {
         list = new SimpleXmlDocument2(filename, XML_ROOT);
         list.Validating = false;
         list.LoadData();
      }

      /// <summary>
      /// Name der Listengruppe
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string GroupName(int idx) {
         return list.ReadValue("/" + XML_ROOT + "/" + XML_GROUP + "[" + (idx + 1).ToString() + "]/" + XML_GROUPNAME, "");
      }

      /// <summary>
      /// Anzahl der GPX-Dateien in der Listengruppe
      /// </summary>
      /// <param name="group"></param>
      /// <returns></returns>
      public int GpxFiles(int group) {
         SimpleXmlDocument2.XPathResult[]? xPathResults = list.ReadValues("/" + XML_ROOT + "/" + XML_GROUP + "[" + (group + 1).ToString() + "]/" + XML_GPX);
         return xPathResults != null ? xPathResults.Length : 0;
      }

      /// <summary>
      /// symbolischer Name der GPX-Datei in der Listengruppe
      /// </summary>
      /// <param name="group"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string GpxName(int group, int idx) {
         return list.ReadValue("/" + XML_ROOT + "/" + XML_GROUP + "[" + (group + 1).ToString() + "]/" + XML_GPX + "[" + (idx + 1).ToString() + "]/" + XML_GPXNAME, "");
      }

      /// <summary>
      /// Name der GPX-Datei in der Listengruppe
      /// </summary>
      /// <param name="group"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string GpxFilename(int group, int idx) {
         return list.ReadValue("/" + XML_ROOT + "/" + XML_GROUP + "[" + (group + 1).ToString() + "]/" + XML_GPX + "[" + (idx + 1).ToString() + "]/" + XML_GPXFILENAME, "");
      }

      /// <summary>
      /// Name der Trackfarbe zur GPX-Datei in der Listengruppe
      /// </summary>
      /// <param name="group"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public int GpxTrackcolorNo(int group, int idx) {
         return list.ReadValue("/" + XML_ROOT + "/" + XML_GROUP + "[" + (group + 1).ToString() + "]/" + XML_GPX + "[" + (idx + 1).ToString() + "]/" + XML_GPXTRACKCOLORNO, 1);
      }

      /// <summary>
      /// Name der Bilder-GPX-Datei zur GPX-Datei in der Listengruppe
      /// </summary>
      /// <param name="group"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string GpxPictureGpxFilename(int group, int idx) {
         return list.ReadValue("/" + XML_ROOT + "/" + XML_GROUP + "[" + (group + 1).ToString() + "]/" + XML_GPX + "[" + (idx + 1).ToString() + "]/" + XML_GPXPICTUREGPXFILENAME, "");
      }

   }
}
