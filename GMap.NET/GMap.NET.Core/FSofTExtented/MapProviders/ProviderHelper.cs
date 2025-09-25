using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GMap.NET.FSofTExtented.MapProviders {
   static public class ProviderHelper {

      /// <summary>
      /// liefert ein Byte-Array aus einer Datei
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static byte[] GetBytesFromFile(string filename, int from, int length) {
         byte[] b = null;
         using (FileStream stream = File.OpenRead(filename)) {
            if (stream.Length < from + length)
               length = (int)stream.Length - from;
            if (length >= 0) {
               b = new byte[length];
               if (stream.Read(b, from, length) != length)
                  return [];
            }
         }
         return b;
      }

      /// <summary>
      /// liefert ein Image (Bitmap) zum <see cref="PureImage"/>
      /// </summary>
      /// <param name="img"></param>
      /// <returns></returns>
      public static Bitmap GetImage(PureImage img) {
         try {
            if (img != null)
               return Bitmap.FromStream(img.Data) as Bitmap;      // fkt. in Win-Forms UND Maui
         } catch (Exception ex) {
            Debug.WriteLine(nameof(MultiMapProvider) + "." + nameof(GetImage) + ": " + ex);
         }
         return null;
      }

      /// <summary>
      /// liefert ein <see cref="PureImage"/> zum Bitmap
      /// </summary>
      /// <param name="bm"></param>
      /// <returns></returns>
      public static PureImage GetPureImage(Bitmap bm) {
         try {
            if (bm != null)
               return GMapProvider.TileImageProxy.FromArray(GetImageDataArray(bm));
         } catch (Exception ex) {
            Debug.WriteLine(nameof(MultiMapProvider) + "." + nameof(GetImage) + ": " + ex);
         }
         return null;
      }

      /// <summary>
      /// erzeugt das Datenarray zum Bitmap
      /// </summary>
      /// <param name="bm"></param>
      /// <returns></returns>
      public static byte[] GetImageDataArray(Bitmap bm) {
         if (bm != null) {
            // Bitmap in PureImage umwandeln
            MemoryStream memoryStream = new MemoryStream();
            bm.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            return memoryStream.ToArray();
         }
         return null;
      }

      /// <summary>
      /// liefert den Provider (oder null) zum Providername
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public static GMapProvider? GetProvider4Providername(string? name) {
         if (name == GarminProvider.Instance.Name)
            return GarminProvider.Instance;
         else if (name == GarminKmzProvider.Instance.Name)
            return GarminKmzProvider.Instance;
         else if (name == WMSProvider.Instance.Name)
            return WMSProvider.Instance;
         else if (name == HillshadingProvider.Instance.Name)
            return HillshadingProvider.Instance;
         else if (name == MultiMapProvider.Instance.Name)
            return MultiMapProvider.Instance;
         else
            for (int p = 0; p < GMapProviders.List.Count; p++)
               if (GMapProviders.List[p].Name == name)
                  return GMapProviders.List[p];
         return null;
      }

      public static Bitmap GetArbitraryBitmap(GMapProvider provider, int width, int height, PointLatLng p1, PointLatLng p2, int zoom, MapProviderDefinition def) {
         if (provider is MultiUseBaseProvider)
            ((MultiUseBaseProvider)provider).MapProviderDefinition = def;

         GPoint gp1 = provider.Projection.FromLatLngToPixel(p1, zoom);
         GPoint gp2 = provider.Projection.FromLatLngToPixel(p2, zoom);

         GPoint pos1 = new GPoint(gp1.X / provider.Projection.TileSize.Width,
                                  gp1.Y / provider.Projection.TileSize.Height);
         GPoint pos2 = new GPoint(gp2.X / provider.Projection.TileSize.Width,
                                  gp2.Y / provider.Projection.TileSize.Height);

         if (gp2.X % provider.Projection.TileSize.Width == (provider.Projection.TileSize.Width - 1))     // Sonderfall: rechter Kartenrand
            gp2.X++;
         if (gp1.Y % provider.Projection.TileSize.Height == (provider.Projection.TileSize.Height - 1))   // Sonderfall: unterer Kartenrand
            gp1.Y++;

         // Sonderfall: genau ein Tile-Image
         if (gp1.X + provider.Projection.TileSize.Width == gp2.X &&        
             gp1.Y - provider.Projection.TileSize.Height == gp2.Y) {       
            if (gp1.X % provider.Projection.TileSize.Width == 0 &&
                gp1.Y % provider.Projection.TileSize.Height == 0) {
               if (width == provider.Projection.TileSize.Width &&
                   height == provider.Projection.TileSize.Height) {
                  PureImage img = provider.GetTileImage(new GPoint(gp1.X / provider.Projection.TileSize.Width,
                                                                   gp2.Y / provider.Projection.TileSize.Height), zoom);
                  return (Bitmap)GetImage(img);
               }
            }
         }

         // sonst:

         throw new Exception("ProviderHelper.GetArbitraryBitmap() nocht nicht vollständig implementiert.");

         Bitmap bmdest = new Bitmap(width, height);
         for (long x = pos1.X; x < pos2.X; x += provider.Projection.TileSize.Width) {
            for (long y = pos1.Y; y < pos2.Y; y += provider.Projection.TileSize.Height) {
               Bitmap bm = new Bitmap((int)provider.Projection.TileSize.Width,
                                      (int)provider.Projection.TileSize.Height);
               PureImage img = provider.GetTileImage(new GPoint(pos1.X, pos2.Y), zoom);
               if (img != null) {





               }
            }
         }



         return null;
      }


      /* Die Klasse GMapProviders enthält static alle vorhandenen Provider, abrufbar u.a. über das Prop
       * 
       *    public static List<GMapProvider> List
       * 
       * Außerdem enthält sie intern die Dictionarys 
       * 
       *    static Dictionary<Guid, GMapProvider> Hash
       *    static Dictionary<int, GMapProvider> DbHash
       *    
       * Sie "bedienen" die Funktionen
       * 
       *    public static GMapProvider TryGetProvider(Guid id)
       *    public static GMapProvider TryGetProvider(int dbId)+
       *    
       * Z.Z. wird nur die 2. Funktion verwendet.
       */


      #region spez. Zugriff auf Props und Fields über Reflection

      //static object getProperty(Type classtype, object obj, string name, BindingFlags flags = BindingFlags.Default) =>
      //   classtype.GetProperty(name, flags).GetValue(obj);

      //static void setProperty(Type classtype, object obj, string name, object value, BindingFlags flags = BindingFlags.Default) =>
      //   classtype.GetProperty(name, flags).SetValue(obj, value);

      static object getField(Type classtype, object obj, string name, BindingFlags flags = BindingFlags.Default) =>
         classtype.GetField(name, flags).GetValue(obj);

      //static void setField(Type classtype, object obj, string name, object value, BindingFlags flags) =>
      //   classtype.GetField(name, flags).SetValue(obj, value);

      static void setField(Type classtype, object obj, string name, object value) =>
         classtype.GetField(name).SetValue(obj, value);

      #endregion


      static void changeProviderList(GMapProvider provider, bool add) {
         try {
            Type typeGMapProvider = typeof(GMapProvider);
            Type typeGMapProviders = typeof(GMapProviders);
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;

            // static-Liste aller Provider
            List<GMapProvider> mapProviders = (List<GMapProvider>)getField(typeGMapProvider, provider, "MapProviders", flags);
            Dictionary<Guid, GMapProvider> hash = (Dictionary<Guid, GMapProvider>)getField(typeGMapProviders, provider, "Hash", flags);
            Dictionary<int, GMapProvider> dbHash = (Dictionary<int, GMapProvider>)getField(typeGMapProviders, provider, "DbHash", flags);

            if (add) {
               mapProviders.Add(provider);
               GMapProviders.List.Add(provider);
               hash.Add(provider.Id, provider);
               dbHash.Add(provider.DbId, provider);
            } else {
               mapProviders.Remove(provider);
               GMapProviders.List.Remove(provider);
               hash.Remove(provider.Id);
               dbHash.Remove(provider.DbId);
            }
         } catch (Exception ex) {
            throw new Exception("Der Kartenprovider '" + provider.Name + "' kann nicht " + (add ? "hinzugefügt" : "entfernt") + " werden." +
                                System.Environment.NewLine + ex.Message);
         }
      }

      /// <summary>
      /// setzt eine andere DbId
      /// </summary>
      /// <param name="dbid"></param>
      /// <returns></returns>
      static public int ChangeDbId(MultiUseBaseProvider provider, int dbid) {
         int olddbid = provider.DbId;
         writeLock();
         try {
            changeProviderList(provider, false);
            setField(typeof(MultiUseBaseProvider), provider, "DbId", dbid);
            changeProviderList(provider, true);
         } catch (Exception ex) {
            throw new Exception("Änderung DbId nicht möglich:" + Environment.NewLine + ex.Message);
         } finally {
            writeUnlock();
         }
         return olddbid;
      }

      static SemaphoreSlim semaphoreSlim4write = new SemaphoreSlim(1);

      static void writeLock() => semaphoreSlim4write.Wait();

      static void writeUnlock() => semaphoreSlim4write.Release();


#if USEHTTPCLIENT

      static string lastReferrer = string.Empty;
      static string lastUserAgent = string.Empty;
      static string lastCodedAuthorization = string.Empty;
      static IWebProxy lastWebProxy = null;
      static ICredentials lastCredentials = null;
      static HttpClient lastHttpClient = null;

      static SemaphoreSlim semaphore4HttpClient = new SemaphoreSlim(1);

      static HttpClient getHttpClient(string codedauthorization, string referrer) {
         semaphore4HttpClient.Wait();

         if (lastHttpClient == null ||
             lastReferrer != referrer ||
             lastUserAgent != GMapProvider.UserAgent ||
             lastCodedAuthorization != codedauthorization ||
             (lastWebProxy != null && !lastWebProxy.Equals(GMapProvider.WebProxy)) ||
             (lastWebProxy == null && GMapProvider.WebProxy != null) ||
             (lastCredentials != null && !lastCredentials.Equals(GMapProvider.Credential)) ||
             (lastCredentials == null && GMapProvider.Credential != null)) {

            lastReferrer = referrer;
            lastUserAgent = GMapProvider.UserAgent;
            lastCodedAuthorization = codedauthorization;
            lastWebProxy = GMapProvider.WebProxy;
            lastCredentials = GMapProvider.Credential;

            HttpClient httpClient;

            if (GMapProvider.WebProxy != null) {
               HttpClientHandler httpClientHandler = new HttpClientHandler();
               httpClientHandler.Proxy = lastWebProxy;
               if (GMapProvider.Credential != null)
                  httpClientHandler.Credentials = lastCredentials;
               httpClient = new HttpClient(httpClientHandler, true);
            } else
               httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(lastCodedAuthorization))
               // codedauthorization = "Basic " + ...
               httpClient.DefaultRequestHeaders.Authorization =
                  new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", lastCodedAuthorization.Substring(6));

            httpClient.Timeout = new TimeSpan(0, 0, 5);     // 5s für Timeout bei Antwort auf GET
            httpClient.DefaultRequestHeaders.Add("User-Agent", lastUserAgent);
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            if (!string.IsNullOrEmpty(referrer))
               httpClient.DefaultRequestHeaders.Referrer = new Uri(lastReferrer);

            //httpClient.DefaultRequestHeaders.ConnectionClose = true;

            if (lastHttpClient != null) {
               lastHttpClient.CancelPendingRequests();
               lastHttpClient.Dispose();
               GC.Collect();  // Erzwingt eine sofortige Garbage Collection für alle Generationen.
            }

            lastHttpClient = httpClient;
         }
         semaphore4HttpClient.Release();

         return lastHttpClient;
      }

      static public async Task<string> GetContentUsingHttp(string url,
                                                           string codedauthorization,
                                                           string refererUrl,
                                                           CancellationTokenSource cts = null) {
         try {
            return await getHttpString(url, codedauthorization, refererUrl, cts).ConfigureAwait(false);
         } catch (Exception ex) {
            throw new Exception(getExceptionText(ex) + Environment.NewLine + url);
         } finally {
            cts?.Dispose();
         }
      }

      static public async Task<PureImage> GetTileImageUsingHttp(string url,
                                                                string codedauthorization,
                                                                string refererUrl,
                                                                CancellationTokenSource cts = null) {
         try {

            //byte[] resp = await getHttpBytes(url, codedauthorization, refererUrl, cts);
            //MemoryStream memoryStream = new MemoryStream(resp);
            //memoryStream.Seek(0, SeekOrigin.Begin);

            //PureImage ret = GMapProvider.TileImageProxy.FromStream(memoryStream);
            //if (ret != null) {
            //   ret.Data = memoryStream;
            //   ret.Data.Position = 0;
            //} else {
            //   memoryStream.Dispose();
            //}
            //return ret;



            return GMapProvider.TileImageProxy.FromArray(await getHttpBytes(url, codedauthorization, refererUrl, cts));
         } catch (Exception ex) {
            throw new Exception(getExceptionText(ex) + Environment.NewLine + url);
         } finally {
            cts?.Dispose();
         }
      }

      static string getExceptionText(Exception ex) {
         StringBuilder sb = new(ex.Message);
         while (ex.InnerException != null) {
            sb.AppendLine("   " + ex.InnerException.Message);
            ex = ex.InnerException;
         }
         return sb.ToString();
      }

      static async Task<string> getHttpString(string url,
                                              string codedauthorization, string refererUrl,
                                              CancellationTokenSource cts = null) {
         HttpClient httpClient = getHttpClient(codedauthorization, refererUrl);
         return cts != null ?
                  await httpClient.GetStringAsync(url, cts.Token).ConfigureAwait(false) :
                  await httpClient.GetStringAsync(url).ConfigureAwait(false);
      }

      static async Task<byte[]> getHttpBytes(string url,
                                             string codedauthorization, string refererUrl,
                                             CancellationTokenSource cts = null) {
         HttpClient httpClient = getHttpClient(codedauthorization, refererUrl);
         return cts != null ?
                  await httpClient.GetByteArrayAsync(url, cts.Token) :
                  await httpClient.GetByteArrayAsync(url);
      }

#endif

   }
}
