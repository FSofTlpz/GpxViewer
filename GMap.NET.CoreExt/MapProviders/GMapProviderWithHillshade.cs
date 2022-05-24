﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FSofTUtils.Drawing;
using GMap.NET.MapProviders;

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

         // Bitmap in PureImage umwandeln
         MemoryStream memoryStream = new MemoryStream();
         bm.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
         return GetTileImageFromArray(memoryStream.ToArray());
      }

      static protected Task DrawHillshadeAsync(FSofTUtils.Geography.DEM.DemData dem,
                                               Bitmap bm,
                                               double left,
                                               double bottom,
                                               double rigth,
                                               double top,
                                               int alpha = 100) {
         Task t = Task.Run(() => {
            DrawHillshade(dem, bm, left, bottom, rigth, top, alpha);
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
      /// <param name="rigth"></param>
      /// <param name="top"></param>
      /// <param name="alpha"></param>
      static protected void DrawHillshade(FSofTUtils.Geography.DEM.DemData dem,
                                          Bitmap bm,
                                          double left,
                                          double bottom,
                                          double rigth,
                                          double top,
                                          int alpha = 100) {
         double deltalon = (rigth - left) / bm.Width;
         double deltalat = -(bottom - top) / bm.Height;

         //Bitmap bmhs = new Bitmap(bm.Width, bm.Height);
         //for (int y = 0; y < bm.Width; y++)
         //   for (int x = 0; x < bm.Height; x++) {
         //      byte s = dem.GetShadingValue(left + x * deltalon, top - y * deltalat);

         //      bmhs.SetPixel(x, y, Color.FromArgb(alpha, s, s, s));
         //      //bmhs.SetPixel(x, y, Color.FromArgb(255 - s, s, s, s));
         //      //bmhs.SetPixel(x, y, Color.FromArgb(255 - s, 120, 120, 120));
         //   }

         // etwa 10..15% schneller:
         uint[] pixel = new uint[bm.Width * bm.Height];
         for (int y = 0; y < bm.Width; y++)
            for (int x = 0; x < bm.Height; x++) {
               byte s = dem.GetShadingValue(left + x * deltalon, top - y * deltalat);
               pixel[x + y * bm.Width] = BitmapHelper.GetUInt4Color(alpha, s, s, s);
            }
         Bitmap bmhs = BitmapHelper.CreateBitmap32(bm.Width, bm.Height, pixel);

         Graphics canvas = Graphics.FromImage(bm);
         canvas.DrawImage(bmhs, 0, 0);

         canvas.Flush();
         canvas.Dispose();
         bmhs.Dispose();
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