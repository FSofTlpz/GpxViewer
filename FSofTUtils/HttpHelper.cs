using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSofTUtils {
   public class HttpHelper {

      static HttpClient? httpClient = null;

      static void createHttpClient(int defaulttimeout = 100) {
         if (httpClient != null) {
            httpClient.CancelPendingRequests();
            httpClient.Dispose();
            GC.Collect();  // Erzwingt eine sofortige Garbage Collection für alle Generationen.
         }
         httpClient = new HttpClient() {
            Timeout = new TimeSpan(0, 0, defaulttimeout),    // default value is 100,000 milliseconds (100 seconds)

         };

         // This will add the Connection: close header to all requests sent with this HttpClient instance.
         // Every request will open a connection and close it when done. Be careful with this.
         httpClient.DefaultRequestHeaders.ConnectionClose = true;
         httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
      }

      /// <summary>
      /// liefert den Ergebnistext und den Statuscode zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="timeout">Timeout in s (falls größer 0 und kleiner Standard)</param>
      /// <returns></returns>
      static public async Task<(System.Net.HttpStatusCode?, string)> GetStringAsync(string uri, double timeout = 0) {
         CancellationTokenSource? cts = null;

         if (httpClient == null)
            createHttpClient();

         if (httpClient != null && 0 < timeout && timeout < httpClient.Timeout.TotalSeconds)
            cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
         return await GetStringAsync(uri, cts);
      }

      /// <summary>
      /// liefert den Statuscode, den Text einer ev. auftretenden Exception und das Byte-Array zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="timeout">Timeout in s (falls größer 0 und kleiner Standard)</param>
      /// <returns></returns>
      static public async Task<(System.Net.HttpStatusCode?, string, byte[])> GetBytesAsync(string uri, double timeout = 0) {
         CancellationTokenSource? cts = null;

         if (httpClient == null)
            createHttpClient();

         if (httpClient != null && 0 < timeout && timeout < httpClient.Timeout.TotalSeconds)
            cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
         return await GetBytesAsync(uri, cts);
      }

      /// <summary>
      /// liefert den Ergebnistext und den Statuscode zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// <para>
      /// Mit der CancellationTokenSource kann die Anforderung (aber NICHT das Einlesen der Daten) abgebrochen werden.
      /// </para>
      /// <para>
      /// ACHTUNG: Die CancellationTokenSource wird INTERN disposed!
      /// </para>
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="cts">zum möglichen Abbrechen der Anforderung (auch Timeout wenn kleiner Standard) oder null</param>
      /// <returns></returns>
      static public async Task<(System.Net.HttpStatusCode?, string)> GetStringAsync(string uri,
                                                                                    CancellationTokenSource? cts = null) {
         if (httpClient == null)
            createHttpClient();
         (System.Net.HttpStatusCode? status, string text, byte[] bytes) = await getDataAsync(httpClient, uri, true, cts);
         return (status, text);
      }

      /// <summary>
      /// liefert den Ergebnistext und den Statuscode zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// <para>
      /// Mit der CancellationTokenSource kann die Anforderung (aber NICHT das Einlesen der Daten) abgebrochen werden.
      /// </para>
      /// <para>
      /// ACHTUNG: Die CancellationTokenSource wird INTERN disposed!
      /// </para>
      /// </summary>
      /// <param name="client"></param>
      /// <param name="uri"></param>
      /// <param name="cts">zum möglichen Abbrechen der Anforderung (auch Timeout wenn kleiner Standard) oder null</param>
      /// <returns></returns>
      static public async Task<(System.Net.HttpStatusCode?, string)> GetStringAsync(HttpClient? client,
                                                                                    string uri,
                                                                                    CancellationTokenSource? cts = null) {
         (System.Net.HttpStatusCode? status, string text, byte[] bytes) = await getDataAsync(client, uri, true, cts);
         return (status, text);
      }

      /// <summary>
      /// liefert den Statuscode, den Text einer ev. auftretenden Exception und das Byte-Array zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// <para>
      /// Mit der CancellationTokenSource kann die Anforderung (aber NICHT das Einlesen der Daten) abgebrochen werden.
      /// </para>
      /// <para>
      /// ACHTUNG: Die CancellationTokenSource wird INTERN disposed!
      /// </para>
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="cts">zum möglichen Abbrechen der Anforderung (auch Timeout wenn kleiner Standard) oder null</param>
      /// <returns></returns>
      static public async Task<(System.Net.HttpStatusCode?, string, byte[])> GetBytesAsync(string uri,
                                                                                           CancellationTokenSource? cts = null) {
         if (httpClient == null)
            createHttpClient();
         return await getDataAsync(httpClient, uri, false, cts);
      }

      /// <summary>
      /// liefert den Statuscode, den Text einer ev. auftretenden Exception und das Byte-Array zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// <para>
      /// Mit der CancellationTokenSource kann die Anforderung (aber NICHT das Einlesen der Daten) abgebrochen werden.
      /// </para>
      /// <para>
      /// ACHTUNG: Die CancellationTokenSource wird INTERN disposed!
      /// </para>
      /// </summary>
      /// <param name="client"></param>
      /// <param name="uri"></param>
      /// <param name="cts">zum möglichen Abbrechen der Anforderung (auch Timeout wenn kleiner Standard) oder null</param>
      /// <returns></returns>
      static public async Task<(System.Net.HttpStatusCode?, string, byte[])> GetBytesAsync(HttpClient? client,
                                                                                           string uri,
                                                                                           CancellationTokenSource? cts = null)
         => await getDataAsync(client, uri, false, cts);

      /// <summary>
      /// liefert den Statuscode, den Text bzw. den Text einer ev. auftretenden Exception und das Byte-Array zum URI
      /// <para>
      /// Tritt intern eine Exception auf wird als Code null und der Exception-Text geliefert.
      /// </para>
      /// <para>
      /// Mit der CancellationTokenSource kann die Anforderung (aber NICHT das Einlesen der Daten) abgebrochen werden.
      /// </para>
      /// <para>
      /// ACHTUNG: Die CancellationTokenSource wird INTERN disposed!
      /// </para>
      /// </summary>
      /// <param name="client"></param>
      /// <param name="uri"></param>
      /// <param name="asstring"></param>
      /// <param name="cts">zum möglichen Abbrechen der Anforderung (auch Timeout wenn kleiner Standard) oder null</param>
      /// <returns></returns>
      static async Task<(System.Net.HttpStatusCode?, string, byte[])> getDataAsync(
               HttpClient? client,
               string uri,
               bool asstring,
               CancellationTokenSource? cts = null) {
         System.Net.HttpStatusCode? statuscode = null;
         string resultstring = string.Empty;
         byte[] resultbytes = Array.Empty<byte>();

         try {
            if (client != null) {
               //if (timeout > 0) 
               //   httpClient.Timeout = new TimeSpan(0, 0, timeout);      NICHT mehr möglich ("This instance has already started one or more requests. Properties can only be modified before sending the first request.")

               using (var result = cts != null ?
                                             await client.GetAsync(
                                                uri,
                                                HttpCompletionOption.ResponseHeadersRead,
                                                cts.Token) :
                                             await client.GetAsync(
                                                uri,
                                                HttpCompletionOption.ResponseHeadersRead)) {
                  // Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.
                  // (true if StatusCode was in the range 200-299)
                  //result.EnsureSuccessStatusCode();
                  statuscode = result.StatusCode;
                  if (asstring)
                     resultstring = await result.Content.ReadAsStringAsync();
                  else
                     resultbytes = await result.Content.ReadAsByteArrayAsync();
               }
            }

         } catch (Exception ex) {
            StringBuilder sb = new(ex.Message);
            while (ex.InnerException != null) {
               sb.AppendLine("   " + ex.InnerException.Message);
               ex = ex.InnerException;
            }
            resultstring = sb.ToString();
            statuscode = null;
         } finally {
            cts?.Dispose();
         }

         return (statuscode, resultstring, resultbytes);
      }

   }
}
