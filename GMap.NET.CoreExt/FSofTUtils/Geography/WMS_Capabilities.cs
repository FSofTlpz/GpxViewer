using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography {
   public class WMS_Capabilities : XmlDocument {

      public class Layer {

         public class BoundingBox {

            public string Srs { get; private set; }
            public double MinX { get; private set; }
            public double MaxX { get; private set; }
            public double MinY { get; private set; }
            public double MaxY { get; private set; }

            public BoundingBox(WMS_Capabilities cap, string xpath) {
               Srs = cap.ReadValue(xpath + "/@SRS", "");
               if (Srs == "")
                  Srs = cap.ReadValue(xpath + "/@CRS", "");
               MinX = cap.ReadValue(xpath + "/@minx", 0.0);
               MinY = cap.ReadValue(xpath + "/@miny", 0.0);
               MaxX = cap.ReadValue(xpath + "/@maxx", 0.0);
               MaxY = cap.ReadValue(xpath + "/@maxy", 0.0);
            }

            public override string ToString() {
               return string.Format("Srs={0}, MinX={1}, MinY={2}, MaxX={3}, MaxY={4}", Srs, MinX, MinY, MaxX, MaxY);
            }
         }

         public string Name { get; private set; }
         public string Title { get; private set; }
         public int Queryable { get; private set; }
         public string Abstract { get; private set; }
         public List<string> Srs { get; private set; }
         public List<Layer> Sublayer { get; private set; }
         public List<BoundingBox> Bounding { get; private set; }
         public BoundingBox LatLonBounding { get; private set; }
         public double ScaleHintMin { get; private set; }
         public double ScaleHintMax { get; private set; }


         public Layer(WMS_Capabilities cap, string xpathlayer) {
            string prefix = WMS_Capabilities.MY_XMLPREFIX + ":";
            Name = cap.ReadValue(xpathlayer + "/" + prefix + "Name", "");
            Title = cap.ReadValue(xpathlayer + "/" + prefix + "Title", "");
            Queryable = cap.ReadValue(xpathlayer + "/@queryable", -1);
            Abstract = cap.ReadValue(xpathlayer + "/" + prefix + "Abstract", "");
            ScaleHintMin = cap.ReadValue(xpathlayer + "/" + prefix + "ScaleHint/@min", 0.0);
            ScaleHintMax = cap.ReadValue(xpathlayer + "/" + prefix + "ScaleHint/@max", 0.0);

            int count;

            Srs = new List<string>();
            count = cap.NodeCount(xpathlayer + "/" + prefix + "SRS");
            if (count >= 1) {
               for (int i = 1; i <= count; i++)
                  Srs.Add(cap.ReadValue(xpathlayer + (count > 1 ? "/" + prefix + "SRS[" + i.ToString() + "]" : "/" + prefix + "SRS"), ""));
            } else {
               count = cap.NodeCount(xpathlayer + "/" + prefix + "CRS");
               for (int i = 1; i <= count; i++)
                  Srs.Add(cap.ReadValue(xpathlayer + (count > 1 ? "/" + prefix + "CRS[" + i.ToString() + "]" : "/" + prefix + "CRS"), ""));
            }

            if (cap.NodeCount(xpathlayer + "/" + prefix + "LatLonBoundingBox") > 0)
               LatLonBounding = new BoundingBox(cap, xpathlayer + "/" + prefix + "LatLonBoundingBox");

            Bounding = new List<BoundingBox>();
            count = cap.NodeCount(xpathlayer + "/" + prefix + "BoundingBox");
            for (int i = 1; i <= count; i++)
               Bounding.Add(new BoundingBox(cap, xpathlayer + (count > 1 ? "/" + prefix + "BoundingBox[" + i.ToString() + "]" : "/" + prefix + "BoundingBox")));

            Sublayer = new List<Layer>();
            count = cap.NodeCount(xpathlayer + "/" + prefix + "Layer");
            for (int i = 1; i <= count; i++)
               Sublayer.Add(new Layer(cap, xpathlayer + (count > 1 ? "/" + prefix + "Layer[" + i.ToString() + "]" : "/" + prefix + "Layer")));

         }

         public override string ToString() {
            return string.Format("Name={0}, Title={1}, LatLonBounding={2}, SublayerCount={3}, Abstract={4}", Name, Title, LatLonBounding, Sublayer.Count, Abstract);
         }

      }

      public List<Layer> Maplayer { get; private set; }


      public WMS_Capabilities(string filename) {
         LoadData(filename);
      }

      public WMS_Capabilities(string capabilitiesurl, ICredentials Credentials) {
         //LoadData("../../wms1.xml");

         WebRequest request = WebRequest.Create(capabilitiesurl);
         request.Credentials = Credentials ?? CredentialCache.DefaultCredentials;
         WebResponse response = request.GetResponse();
         Debug.WriteLine(((HttpWebResponse)response).StatusDescription);
         if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK) {
            using (Stream dataStream = response.GetResponseStream()) {
               LoadData(dataStream);
            }
         } else
            throw new Exception("Error reading capabilities: " + ((HttpWebResponse)response).StatusDescription);
         response.Close();
      }

      void x() {
         //System.Xml.Schema.XmlSchema schema = new System.Xml.Schema.XmlSchema();
         //schema.Namespaces.Add("xmlns", "http://www.sample.com/file");
         //Schemas.Add(schema);

         XmlNamespaceManager nsmng = new XmlNamespaceManager(NameTable);
         //nsmng.AddNamespace("", "http://www.opengis.net/wms");
         nsmng.AddNamespace("my", "http://www.opengis.net/wms2");



         XPathNavigator nav = CreateNavigator();

         string root = DocumentElement.Name;

         Debug.WriteLine(">>> DocumentElement.Name=" + DocumentElement.Name);
         if (nsmng != null) {
            Debug.WriteLine(">>> HasNamespace(my)=" + nsmng.HasNamespace("my").ToString());

            //   //do {
            //   //   foreach (String prefix in nsmng) {
            //   //      Console.WriteLine(">>> Prefix={0}, Namespace={1}", prefix, nsmng.LookupNamespace(prefix));
            //   //   }
            //   //}
            //   //while (nsmng.PopScope());


            //   //nsmng.PushScope();
         }
         try { Debug.WriteLine(">>> root=" + nav.Select("/" + root).Count.ToString()); } catch { }
         try { Debug.WriteLine(">>> my:root=" + nav.Select("/my:" + root).Count.ToString()); } catch { }
         try { Debug.WriteLine(">>> NsMng root=" + nav.Select("/" + root, nsmng).Count.ToString()); } catch { }
         try { Debug.WriteLine(">>> NsMng my:root=" + nav.Select("/my:" + root, nsmng).Count.ToString()); } catch { }

      }



      void LoadData(string filename) {
         //string txt = File.ReadAllText(filename);
         //MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(txt));
         //LoadData(stream);
         LoadData(new StreamReader(filename, true).BaseStream);
      }

      /// <summary>
      /// liest aus dem Stream ab der akt. Position
      /// </summary>
      /// <param name="xmlstream"></param>
      void LoadData(Stream xmlstream) {
         //xmlstream.Seek(0, SeekOrigin.Begin);
         Load(xmlstream);
         NsMng = AddNamespace(MY_XMLPREFIX);
         navigator = CreateNavigator();
         ReadData();
      }


      const string MY_XMLPREFIX = "my";
      const string PATH_CAPS = "/*"; //":WMS_Capabilities";
      const string PATH_LAYER = PATH_CAPS + "/" + MY_XMLPREFIX + ":Capability/" + MY_XMLPREFIX + ":Layer/" + MY_XMLPREFIX + ":Layer";

      void ReadData() {
         Maplayer = new List<Layer>();
         int count = NodeCount(PATH_LAYER);
         for (int i = 1; i <= count; i++)
            Maplayer.Add(new Layer(this,
                                   count > 1 ?
                                             PATH_LAYER + "[" + i.ToString() + "]" :
                                             PATH_LAYER));
      }

      public string CapabilitiesVersion {
         get {
            return ReadValue(PATH_CAPS + "/@version", "");
         }
      }

      public string ServiceName {
         get {
            return ReadValue(PATH_CAPS + "/my:Service/my:Name", "");
         }
      }

      public string ServiceTitle {
         get {
            return ReadValue(PATH_CAPS + "/my:Service/my:Title", "");
         }
      }

      public string[] Mapformats {
         get {
            return ReadString(PATH_CAPS + "/my:Capability/my:Request/my:GetMap/my:Format");
         }
      }

      #region XPath-Funktionen zur Datenabfrage

      /// <summary>
      /// interner NamespaceManager
      /// </summary>
      XmlNamespaceManager NsMng;

      /// <summary>
      /// interner Standard-Navigator
      /// </summary>
      XPathNavigator navigator;


      /// <summary>
      /// fügt einen Namespace für XPATH an
      /// </summary>
      /// <param name="sPrefix"></param>
      /// <param name="sUrl">wenn null, dann wird die interne Namespace-URI verwendet</param>
      /// <returns>Namespacemanager</returns>
      public XmlNamespaceManager AddNamespace(string sPrefix, string sUrl = null) {
         if (string.IsNullOrEmpty(sUrl))
            sUrl = DocumentElement.NamespaceURI;
         XmlNamespaceManager NsMng = new System.Xml.XmlNamespaceManager(NameTable);
         NsMng.AddNamespace(sPrefix, sUrl);
         return NsMng;
      }

      // z.B.:
      // ReadValue("/bookstore/book[@genre=\"novel\"]/author/last-name");
      // ReadValue("/bookstore/book/@genre");

      /// <summary>
      /// typisierter Inhalt eines Nodes mit dem Node-Namen
      /// </summary>
      public class XPathResult {

         /// <summary>
         /// Name of the current node without any namespace prefix
         /// </summary>
         public string Localname { get; private set; }
         /// <summary>
         /// boxed object of the most appropriate .NET Framework type
         /// </summary>
         public object Content { get; private set; }

         public XPathResult(string localname, object content) {
            Localname = localname;
            Content = content;
         }

         public override string ToString() {
            return string.Format("{0}: {1}", Localname, Content);
         }

      }


      /// <summary>
      /// wählt den/die Knoten aus oder löst eine Exception aus
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      XPathNodeIterator NavigatorSelect(string xpath) {
         return NsMng != null ?
                     navigator.Select(xpath, NsMng) :
                     navigator.Select(xpath);
      }

      /// <summary>
      /// Existiert der XPath?
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <returns></returns>
      public bool ExistXPath(string xpath) {
         try {
            return NavigatorSelect(xpath).Count > 0;
         } catch { }
         return false;
      }

      /// <summary>
      /// liefert die Anzahl der Knoten, die auf 'xpath' passen
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public int NodeCount(string xpath) {
         return NavigatorSelect(xpath).Count;
      }

      /// <summary>
      /// liefert den XML-Text für eine weitere Analyse
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public string[] XReadOuterXml(string xpath) {
         string[] ret = null;
         XPathNodeIterator nodes = navigator.Select(xpath);
         if (nodes.Count == 0)
            return null;
         ret = new string[nodes.Count];
         int i = 0;
         while (nodes.MoveNext())
            ret[i++] = nodes.Current.OuterXml;
         return ret;
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath' als Object-Array
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public object[] ReadValueAsObject(string xpath) {
         object[] ret = null;
         try {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes.Count == 0)
               return null;
            ret = new object[nodes.Count];
            int i = 0;
            while (nodes.MoveNext())
               ret[i++] = nodes.Current.TypedValue;
         } catch { }
         return ret;
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public string ReadValue(string xpath, string defvalue) {
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return defvalue;
         return o[0].ToString();
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public bool ReadValue(string xpath, bool defvalue) {
         bool ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            ret = Convert.ToBoolean(o[0]);            // 'true' / 'false'
         } catch {
            try {
               ret = Convert.ToDouble(o[0]) != 0.0;   // alle Zahlen != 0 als true
            } catch { }
         }
         return ret;
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public int ReadValue(string xpath, int defvalue) {
         int ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            ret = Convert.ToInt32(o[0]);
         } catch { }
         return ret;
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public uint ReadValue(string xpath, uint defvalue) {
         uint ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            ret = Convert.ToUInt32(o[0]);
         } catch { }
         return ret;
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public double ReadValue(string xpath, double defvalue) {
         double ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            if (o[0].GetType() == Type.GetType("System.String"))     // Mono erkennt z.Z. NICHT den Typ Double --> Umwandlung aus String
               ret = Convert.ToDouble(o[0], System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            else
               ret = Convert.ToDouble(o[0]);
         } catch { }
         return ret;
      }

      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <returns></returns>
      public string[] ReadString(string xpath) {
         string[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new string[o.Length];
            for (int i = 0; i < o.Length; i++)
               ret[i] = o[i].ToString();
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">XPath</param>
      /// <returns></returns>
      public int[] ReadInt(string xpath, int defvalue) {
         int[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new int[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToInt32(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">XPath</param>
      /// <returns></returns>
      public uint[] ReadUInt(string xpath, uint defvalue) {
         uint[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new uint[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToUInt32(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">XPath</param>
      /// <returns></returns>
      public bool[] ReadBool(string xpath, bool defvalue) {
         bool[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new bool[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToBoolean(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <param name="defvalue">XPath</param>
      /// <returns></returns>
      public double[] ReadDouble(string xpath, double defvalue) {
         double[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new double[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToDouble(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }

      /// <summary>
      /// liefert alle Attribut-Wert-Paare für jeden per xpath adressierten Knoten
      /// <para>Das jeweilige Dictionary ist leer, wenn keine Attribute ex.</para>
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public List<Dictionary<string, string>> ReadAttributes(string xpath) {
         List<Dictionary<string, string>> attr = null;
         try {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes != null && nodes.Count > 0) {
               attr = new List<Dictionary<string, string>>();
               while (nodes.MoveNext()) {
                  attr.Add(new Dictionary<string, string>());
                  navigator.MoveTo(nodes.Current);
                  if (navigator.MoveToFirstAttribute()) {         // es gibt Attribute
                     attr[attr.Count - 1].Add(navigator.Name, navigator.Value);
                     while (navigator.MoveToNextAttribute())
                        attr[attr.Count - 1].Add(navigator.Name, navigator.Value);
                  }
               }
            }
         } catch { }
         return attr;
      }

      /// <summary>
      /// liefert, wenn der XPath eindeutig ist, den gesamten XML-Text, sonst null
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public string GetXmlText(string xpath) {
         int count = 0;
         XPathNodeIterator it = NavigatorSelect(xpath);
         count = it.Count;
         if (count == 1) {
            it.MoveNext();
            XPathNavigator nav = it.Current;
            return nav.OuterXml;
         }
         return null;
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath' als <see cref="XPathResult"/>-Array
      /// <para>sinnvoll, wenn mehrere Werte gleichzeitig mit * und/oder @* abgefragt werden</para>
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public XPathResult[] ReadValues(string xpath) {
         XPathResult[] ret = null;
         try {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes.Count == 0)
               return null;
            ret = new XPathResult[nodes.Count];
            int i = 0;
            while (nodes.MoveNext())
               ret[i++] = new XPathResult(nodes.Current.LocalName, nodes.Current.TypedValue);
         } catch { }
         return ret;
      }


      /* Problem:
       * XPATH mag keine (') und (") innerhalb der Argumente. Wenn ein (') auftaucht könnte man die Zeichenketten in (") einschließen
       * bzw. wenn (") enthalten ist können die Zeichenkette in (') eingeschlossen werden. Problematisch wird es, wenn sowohl (') als
       * auch (") in einer Zeichenkette auftauchen können.
       * Dann kann man die Funktion concate() verwenden. Die Originalzeichenkette wird zerlegt, so dass die enthaltenen (') und (") jeweils
       * einzeln behandelt werden. (') werden in (") und (") in (') eingeschlossen. Aufwendig, aber wohl die einzige Möglichkeit.
       * ACHTUNG!
       * Sonderzeichen (&) usw. müssen NICHT umgewandelt werden.
       */
      /// <summary>
      /// 
      /// </summary>
      /// <param name="sXPathQueryString"></param>
      /// <returns></returns>
      public static string GenerateConcatForXPath(string sXPathQueryString) {
         string returnString = string.Empty;
         string searchString = sXPathQueryString;
         char[] quoteChars = new char[] { '\'', '"' };

         int quotePos = searchString.IndexOfAny(quoteChars);
         if (quotePos == -1)
            returnString = "'" + searchString + "'";
         else {
            returnString = "concat(";
            while (quotePos != -1) {
               string subString = searchString.Substring(0, quotePos);
               returnString += "'" + subString + "', ";
               if (searchString.Substring(quotePos, 1) == "'")
                  returnString += "\"'\", ";
               else
                  returnString += "'\"', ";
               searchString = searchString.Substring(quotePos + 1, searchString.Length - quotePos - 1);
               quotePos = searchString.IndexOfAny(quoteChars);
            }
            returnString += "'" + searchString + "')";
         }
         return returnString;
      }

      #endregion

      public override string ToString() {
         return string.Format("Version={0}, Name={1}, Title={2}, LayerCount={3}", CapabilitiesVersion, ServiceName, ServiceTitle, Maplayer.Count);
      }

   }
}
