using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
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

      public static GeoCodingResultBase[] Get(string name) { return null; }

      public static GeoCodingResultBase[] Get(double lon, double lat) { return null; }

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

      protected static string getXmlValue(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         XPathNodeIterator ni = navigatorSelect(navigator, NsMng, xpath);
         if (ni != null && ni.Count > 0) {
            if (ni.MoveNext())
               return ni.Current.Value.ToString();
         }
         return null;
      }

      protected static List<string> getXmlValues(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         List<string> lst = new List<string>();
         foreach (var item in navigatorSelect(navigator, NsMng, xpath))
            if (item != null)
               lst.Add(item.ToString());
         return lst;
      }

      protected static double getXmlValueAsDouble(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         string val = getXmlValue(xpath, navigator, NsMng);
         if(!string.IsNullOrEmpty(val))
            return Convert.ToDouble(val, CultureInfo.InvariantCulture);
         return double.MinValue;
      }

      protected static int getXmlValueAsInt(string xpath, XPathNavigator navigator, XmlNamespaceManager NsMng) {
         string val = getXmlValue(xpath, navigator, NsMng);
         if (!string.IsNullOrEmpty(val))
            return Convert.ToInt32(val);
         return int.MinValue;
      }

      /// <summary>
      /// liefert den Text eines HTTP-Get
      /// </summary>
      /// <param name="requeststring"></param>
      /// <returns></returns>
      protected static string httpGet(string requeststring) {
         string result = null;

         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requeststring);
         request.Credentials = CredentialCache.DefaultCredentials;
         request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
         //request.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
         //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
         HttpWebResponse response = (HttpWebResponse)request.GetResponse();
         if (response != null) {
            using (StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
               result = readStream.ReadToEnd();
            }
            response.Close();
         }

         return result;
      }

      public override string ToString() {
         return string.Format("[{0}], lon={1:F6}, lat={2:F6}", Name, Longitude, Latitude);
      }
   }
}
