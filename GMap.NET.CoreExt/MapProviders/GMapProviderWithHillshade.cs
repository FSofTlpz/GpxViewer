using FSofTUtils.Drawing;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace GMap.NET.CoreExt.MapProviders {
   abstract public class GMapProviderWithHillshade : GMapProvider {

      /// <summary>
      /// Damit muss die Karte gezeichnet werden.
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      protected abstract Bitmap GetBitmap(int width, int height, PointLatLng p1, PointLatLng p2, int zoom);

      /// <summary>
      /// ev. zusätzliche Ausgaben (nach dem Hillshading)
      /// </summary>
      /// <param name="bm"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="zoom"></param>
      //protected virtual void PostDrawBitmap(Bitmap bm, PointLatLng p1, PointLatLng p2, int zoom) { }


      FSofTUtils.Geography.DEM.DemData _dem;

      /// <summary>
      /// setzt oder liefert threadsicher das DEM-Verwaltungsobjekt
      /// </summary>
      public FSofTUtils.Geography.DEM.DemData DEM {
         get {
            return Interlocked.Exchange(ref _dem, _dem);
         }
         set {
            Interlocked.Exchange(ref _dem, value);
         }
      }

      int _alpha = 100;

      /// <summary>
      /// setzt oder liefert threadsicher den Alpha-Wert für das Hillshading
      /// </summary>
      public int Alpha {
         get {
            return Interlocked.Exchange(ref _alpha, _alpha);
         }
         set {
            Interlocked.Exchange(ref _alpha, (value & 0xFF));
         }
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="p1">links-unten</param>
      /// <param name="p2">rechts-oben</param>
      /// <param name="zoom">Zoomstufe</param>
      /// <returns></returns>
      protected PureImage getPureImage(int width, int height, PointLatLng p1, PointLatLng p2, int zoom) {
         Bitmap bm = GetBitmap(width, height, p1, p2, zoom);
         if (bm != null) {
            // Bitmap in PureImage umwandeln
            MemoryStream memoryStream = new MemoryStream();
            bm.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            return GetTileImageFromArray(memoryStream.ToArray());
         }
         return null;
      }

      static protected Task DrawHillshadeAsync(FSofTUtils.Geography.DEM.DemData dem,
                                               Bitmap bm,
                                               double left,
                                               double bottom,
                                               double right,
                                               double top,
                                               int alpha,
                                               CancellationToken cancellationToken) {
         Task t = Task.Run(() => {
            DrawHillshade(dem, bm, left, bottom, right, top, alpha, cancellationToken);
         });
         return t;
      }

      /// <summary>
      /// zeichnet das Hillshading über die Karte
      /// </summary>
      /// <param name="dem"></param>
      /// <param name="bm"></param>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <param name="right"></param>
      /// <param name="top"></param>
      /// <param name="alpha"></param>
      static protected void DrawHillshade(FSofTUtils.Geography.DEM.DemData dem,
                                          Bitmap bm,
                                          double left,
                                          double bottom,
                                          double right,
                                          double top,
                                          int alpha,
                                          CancellationToken cancellationToken) {
         byte[] shadings = dem.GetShadingValueArray(left, bottom, right, top, bm.Width, bm.Height, cancellationToken);
         if (shadings != null) {
            uint[] pixel = new uint[bm.Width * bm.Height];
            for (int i = 0; i < shadings.Length; i++)
               pixel[i] = BitmapHelper.GetUInt4Color(alpha, shadings[i], shadings[i], shadings[i]);

            using (Bitmap bmhs = BitmapHelper.CreateBitmap32(bm.Width, bm.Height, pixel)) {
               using (Graphics canvas = Graphics.FromImage(bm)) {
                  canvas.DrawImage(bmhs, 0, 0);
               }
            }
         }
      }

      /// <summary>
      /// setzt eine andere DbId
      /// </summary>
      /// <param name="dbid"></param>
      /// <returns></returns>
      public int ChangeDbId(int dbid) {
         int olddbid = DbId;
         unregisterProvider(this);
         setField(typeof(GMapProvider), this, "DbId", dbid);
         registerProvider(this);
         return olddbid;
      }

      public static byte[] GetBytesFromFile(string filename, int from, int length) {
         byte[] b = null;
         using (FileStream stream = File.OpenRead(filename)) {
            if (stream.Length < from + length)
               length = (int)stream.Length - from;
            if (length >= 0) {
               b = new byte[length];
               stream.Read(b, from, length);
            }
         }
         return b;
      }


      #region spez. Zugriff auf Props und Fields über Reflection

      static protected object getProperty(Type classtype, object obj, string name, BindingFlags flags = BindingFlags.Default) {
         return classtype.GetProperty(name, flags).GetValue(obj);
      }

      static protected void setProperty(Type classtype, object obj, string name, object value, BindingFlags flags = BindingFlags.Default) {
         classtype.GetProperty(name, flags).SetValue(obj, value);
      }

      static protected object getField(Type classtype, object obj, string name, BindingFlags flags = BindingFlags.Default) {
         return classtype.GetField(name, flags).GetValue(obj);
      }

      static protected void setField(Type classtype, object obj, string name, object value, BindingFlags flags) {
         classtype.GetField(name, flags).SetValue(obj, value);
      }

      static protected void setField(Type classtype, object obj, string name, object value) {
         classtype.GetField(name).SetValue(obj, value);
      }

      #endregion

      /// <summary>
      /// fügt den Provider an die Liste der vordef. Provider an
      /// <para>Tritt dabei ein Fehler auf, erfolgt eine Exception.</para>
      /// </summary>
      /// <param name="provider"></param>
      static protected void registerProvider(GMapProvider provider) {
         try {
            List<GMapProvider> mapProviders = (List<GMapProvider>)getField(typeof(GMapProvider), provider, "MapProviders", BindingFlags.Static | BindingFlags.NonPublic);
            mapProviders.Add(provider);

            List<GMapProvider> List = (List<GMapProvider>)getProperty(typeof(GMapProviders), provider, "List", BindingFlags.Static | BindingFlags.Public);
            List.Add(provider);

            Dictionary<Guid, GMapProvider> Hash = (Dictionary<Guid, GMapProvider>)getField(typeof(GMapProviders), provider, "Hash", BindingFlags.Static | BindingFlags.NonPublic);
            Hash.Add(provider.Id, provider);

            Dictionary<int, GMapProvider> DbHash = (Dictionary<int, GMapProvider>)getField(typeof(GMapProviders), provider, "DbHash", BindingFlags.Static | BindingFlags.NonPublic);
            DbHash.Add(provider.DbId, provider);
         } catch (Exception ex) {
            throw new Exception("Der Kartenprovider '" + provider.Name + "' kann nicht registriert werden." + System.Environment.NewLine + ex.Message);
         }
      }

      static protected void unregisterProvider(GMapProvider provider) {
         try {
            List<GMapProvider> mapProviders = (List<GMapProvider>)getField(typeof(GMapProvider), provider, "MapProviders", BindingFlags.Static | BindingFlags.NonPublic);
            mapProviders.Remove(provider);

            List<GMapProvider> List = (List<GMapProvider>)getProperty(typeof(GMapProviders), provider, "List", BindingFlags.Static | BindingFlags.Public);
            List.Remove(provider);

            Dictionary<Guid, GMapProvider> Hash = (Dictionary<Guid, GMapProvider>)getField(typeof(GMapProviders), provider, "Hash", BindingFlags.Static | BindingFlags.NonPublic);
            Hash.Remove(provider.Id);

            Dictionary<int, GMapProvider> DbHash = (Dictionary<int, GMapProvider>)getField(typeof(GMapProviders), provider, "DbHash", BindingFlags.Static | BindingFlags.NonPublic);
            DbHash.Remove(provider.DbId);
         } catch (Exception ex) {
            throw new Exception("Der Kartenprovider '" + provider.Name + "' kann nicht deregistriert werden." + System.Environment.NewLine + ex.Message);
         }
      }

   }
}
