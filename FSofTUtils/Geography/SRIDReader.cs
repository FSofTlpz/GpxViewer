using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;

namespace FSofTUtils.Geography {
   public class SRIDReader {

      class WktString {
         /// <summary>
         /// Well-known ID
         /// </summary>
         public int WktId;
         /// <summary>
         /// Well-known Text
         /// </summary>
         public string? Wkt;
      }

      static ProjNet.CoordinateSystems.CoordinateSystemFactory csfactory = new ProjNet.CoordinateSystems.CoordinateSystemFactory();


      private const string Filename = @"..\..\SRID.csv";

      /// <summary>
      /// Enumerates all SRID's in the SRID.csv file.
      /// </summary>
      /// <returns>Enumerator</returns>
      static IEnumerable<WktString> ReadSridsFromFile(string? filename = null) {
         if (string.IsNullOrWhiteSpace(filename))
            filename = Filename;

         using (var sr = File.OpenText(filename)) {
            while (!sr.EndOfStream) {
               WktString? wkt = GetDataFromText(sr.ReadLine());
               if (wkt == null)
                  continue;
               yield return wkt;
            }
            sr.Close();
         }
      }

      /// <summary>
      /// Enumerates all SRID's in the text.
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      static IEnumerable<WktString> ReadSridsFromText(string txt) {
         string[] lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
         foreach (var line in lines) {
            WktString? wkt = GetDataFromText(line);
            if (wkt == null)
               continue;
            yield return wkt;
         }
      }

      static WktString? GetDataFromText(string? txt) {
         if (string.IsNullOrEmpty(txt))
            return null;

         var split = txt.IndexOf(';');
         if (split < 0)
            return null;

         var wkt = new WktString {
            WktId = int.Parse(txt.Substring(0, split)),
            Wkt = txt.Substring(split + 1)
         };

         return wkt;
      }


      /// <summary>
      /// Gets a coordinate system from the SRID.csv file
      /// </summary>
      /// <param name="id">EPSG ID</param>
      /// <returns>Coordinate system, or null if SRID was not found.</returns>
      public static GeoAPI.CoordinateSystems.ICoordinateSystem? GetCSbyID(int id) {
         foreach (var wkt in ReadSridsFromFile(null))
            if (wkt.WktId == id)
               return csfactory.CreateFromWkt(wkt.Wkt);
         return null;
      }

      /// <summary>
      /// Gets a coordinate system from the SRID-Resource
      /// </summary>
      /// <param name="id">EPSG ID</param>
      /// <param name="rescontainer">Typobjekt der Ressource, z.B. typeof(MyAssembly.Properties.Resources)</param>
      /// <param name="sridresname">Name der Ressource</param>
      /// <returns>Coordinate system, or null if SRID was not found.</returns>
      public static GeoAPI.CoordinateSystems.ICoordinateSystem? GetCSbyIDFromResource(int id, Type rescontainer, string sridresname = "SRID") {
         //string txt = GMapTest.Properties.Resources.SRID;
         //string txt = GMapTest.Properties.Resources.ResourceManager.GetString("SRID");
         //ResourceManager ResManager = new ResourceManager("GMapTest.Properties.Resources", assembly);
         //ResourceManager ResManager = new ResourceManager(rescontainer);

         string? txt = new ResourceManager(rescontainer).GetString("SRID");

         if (txt != null)
            foreach (var wkt in ReadSridsFromText(txt))
               if (wkt.WktId == id)
                  return csfactory.CreateFromWkt(wkt.Wkt);
         return null;
      }

      public static GeoAPI.CoordinateSystems.ICoordinateSystem? GetCSbyIDFromInternalFile(int id) {
         string? assemblyname = typeof(SRIDReader).AssemblyQualifiedName;
         if (assemblyname != null) {
            string typename = assemblyname.Split(',')[0].Trim(); // FSofTUtils.Geography.SRIDReader
            int p = typename.LastIndexOf('.');
            if (p > 0) {
               using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typename.Substring(0, p) + ".SRID.csv")) {
                  if (stream != null) {
                     using (TextReader reader = new StreamReader(stream)) {
                        string txt = reader.ReadToEnd();
                        foreach (var wkt in ReadSridsFromText(txt))
                           if (wkt.WktId == id)
                              return csfactory.CreateFromWkt(wkt.Wkt);
                     }
                  }
               }
            }
         }
         return null;
      }

   }
}
