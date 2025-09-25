using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography.GeoCoding {
   public abstract class GeoCodingResultBase {

      /* Es gibt eine größere Anzahl von Anbietern für Geocoding. Üblicherweise wird eine bestimmte URL mit entsprechenden
       * Parametern gesendet. Als Antwort kommt i.A. eine XML- oder JSON-Datei mit den Koordinaten und ev. weiteren Infos 
       * zum gesuchten Objekt.
       * 
       * Die meisten Dienste sind allerdings prinzipiell kostenpflichtig, auch wenn eine begrenzte Anzahl von Abfragen praktisch
       * kostenlos ist. Üblicherweise muss man sich einen "Applicationkey" oder ähnliches beschaffen, der als Parameter mit der URL
       * mitgeschickt wird.
       * 
       * Bei einigen Versuchen, einen solchen Key zu erhalten, wurden immer irgendwann auch Kreditkartendaten verlangt. Das galt 
       * auch für die kostenlosen Zugriffe.
       * 
       * Unklar ist z.T., welche Datenbasis die Dienste verwenden. Offensichtlich werden u.a. auch OSM-Daten verwendet, die man
       * aber auch direkt abrufen kann (siehe GeoCodingResultOsm).
       */

      public string Name { get; protected set; } = "";

      public double Latitude { get; protected set; } = 0;

      public double Longitude { get; protected set; } = 0;

      public double BoundingLeft { get; protected set; } = 0;

      public double BoundingRight { get; protected set; } = 0;

      public double BoundingTop { get; protected set; } = 0;

      public double BoundingBottom { get; protected set; } = 0;

      public static Task<GeoCodingResultBase[]>? GetAsync(string name, double timeout = 0) { return null; }

      public static Task<GeoCodingResultBase[]>? GetAsync(double lon, double lat, double timeout = 0) { return null; }

      /// <summary>
      /// wählt den/die Knoten aus oder löst eine Exception aus
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      protected static XPathNodeIterator navigatorSelect(XPathNavigator navigator, XmlNamespaceManager nsMng, string xpath) {
         return nsMng != null ?
                     navigator.Select(xpath, nsMng) :
                     navigator.Select(xpath);
      }

      protected static string? getXmlValue(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         XPathNodeIterator ni = navigatorSelect(navigator, NsMng, xpath);
         if (ni != null && ni.Count > 0) {
            if (ni.MoveNext())
               if (ni.Current != null)
                  return ni.Current.Value.ToString();
         }
         return null;
      }

      protected static List<string> getXmlValues(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         List<string> lst = new List<string>();
         foreach (var item in navigatorSelect(navigator, NsMng, xpath))
            if (item != null) {
               string? str = item.ToString();
               if (str != null)
                  lst.Add(str);
            }
         return lst;
      }

      protected static double getXmlValueAsDouble(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         string? val = getXmlValue(xpath, navigator, NsMng);
         if (!string.IsNullOrEmpty(val))
            return Convert.ToDouble(val, CultureInfo.InvariantCulture);
         return double.MinValue;
      }

      protected static int getXmlValueAsInt(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         string? val = getXmlValue(xpath, navigator, NsMng);
         if (!string.IsNullOrEmpty(val))
            return Convert.ToInt32(val);
         return int.MinValue;
      }


      //static HttpClient? client = null;

      //static void createHttpClient() {
      //   if (client != null) {
      //      client.CancelPendingRequests();
      //      client.Dispose();
      //      GC.Collect();  // Erzwingt eine sofortige Garbage Collection für alle Generationen.
      //   }
      //   client = new HttpClient() {
      //      Timeout = new TimeSpan(0, 0, 60),    // default value is 100,000 milliseconds (100 seconds)

      //   };

      //   // This will add the Connection: close header to all requests sent with this HttpClient instance.
      //   // Every request will open a connection and close it when done. Be careful with this.
      //   client.DefaultRequestHeaders.ConnectionClose = true;
      //   client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

      //   // httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");



      //   //@"Mozilla/5.0 (compatible; AcmeInc/1.0)";
      //}

      //static async Task<byte[]> httpGetBin(string uri, int timeoutms = -1) {
      //   byte[] resultbytes = [];

      //   if (client == null)
      //      createHttpClient();

      //   if (client != null) {
      //      if (timeoutms > 0)
      //         client.Timeout = new TimeSpan(timeoutms * 10000);

      //      using (var cts = new CancellationTokenSource()) {
      //         using (var result = await client.GetAsync(uri,
      //                                                   HttpCompletionOption.ResponseHeadersRead
      //                                                   //,
      //                                                   //cts.Token
      //                                                   )) {
      //            // Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.
      //            // (true if StatusCode was in the range 200-299)
      //            result.EnsureSuccessStatusCode();

      //            resultbytes = await result.Content.ReadAsByteArrayAsync();
      //         }
      //      }
      //   }

      //   return resultbytes;
      //}

      //static public byte[] HttpGetBin(string uri, int timeoutms = -1) =>
      //   httpGetBin(uri, timeoutms).Result;

      //static public string HttpGetString(string uri, int timeoutms = -1) =>
      //   Encoding.UTF8.GetString(httpGetBin(uri, timeoutms).Result);


      ///// <summary>
      ///// liefert den Text eines HTTP-Get
      ///// </summary>
      ///// <param name="requeststring"></param>
      ///// <returns></returns>
      //protected static string HttpGetString2(string requeststring) {
      //   string result = null;

      //   System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(requeststring);
      //   request.Credentials = System.Net.CredentialCache.DefaultCredentials;
      //   request.UserAgent = @"Mozilla/5.0 (compatible; AcmeInc/1.0)";
      //   //request.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
      //   //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
      //   System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
      //   if (response != null) {
      //      using (System.IO.StreamReader readStream = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
      //         result = readStream.ReadToEnd();
      //      }
      //      response.Close();
      //   }

      //   return result;
      //}




      /*
      Nominatim Usage Policy (aka Geocoding Policy)
      https://operations.osmfoundation.org/policies/nominatim/

      u.a.:
            - No heavy uses (an absolute maximum of 1 request per second).
            - Provide a valid HTTP Referer or User-Agent identifying the application (stock User-Agents as set by http libraries will not do).

      hat lange fkt.:
         request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
      aber liefert jetzt HTTP-403
      deshalb:

         request.UserAgent = @"Mozilla/5.0 (compatible; AcmeInc/1.0)";

         https://help.openstreetmap.org/questions/83008/why-i-receive-error-403-from-api
         ... you should use an UserAgent var httpClient = new HttpClient(); httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)") ...


      */


      ///// <summary>
      ///// liefert den Text eines HTTP-Get
      ///// </summary>
      ///// <param name="requeststring"></param>
      ///// <returns></returns>
      //protected static string httpGet(string requeststring) {
      //   string result = null;

      //   HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requeststring);
      //   request.Credentials = CredentialCache.DefaultCredentials;
      //   request.UserAgent = @"Mozilla/5.0 (compatible; AcmeInc/1.0)";
      //   //request.Headers["User-Agent"] = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
      //   //request.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
      //   //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
      //   HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      //   if (response != null) {
      //      using (StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
      //         result = readStream.ReadToEnd();
      //      }
      //      response.Close();
      //   }

      //   HttpClient client = new HttpClient();
      //   client.


      //   return result;
      //}

      public override string ToString() {
         return string.Format("[{0}], lon={1:F6}, lat={2:F6}", Name, Longitude, Latitude);
      }
   }
}
