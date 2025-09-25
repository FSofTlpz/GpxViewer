//#define TESTDATA
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography.GeoCoding {
   public class GeoCodingReverseResultOsm : GeoCodingResultBase {

      const string osmformat = "https://nominatim.openstreetmap.org/reverse?format=xml&lat={0}&lon={1}";

#if TESTDATA


      // querystring='format=xml&lat=51.33869553&lon=12.37915635&zoom=18&addressdetails=1'>

      const string testxml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + @"
<reversegeocode timestamp='Thu, 01 Sep 22 09:07:30 +0000' attribution='Data © OpenStreetMap contributors, ODbL 1.0. http://www.openstreetmap.org/copyright' querystring='format=xml lat=51.33869553 lon=12.37915635 zoom=18 addressdetails=1'>
   <result place_id='114635262' osm_type='way' osm_id='35479085' ref='Neues Augusteum' 
lat='51.338663999999994' lon='12.379095359883735' 
boundingbox='51.3383478,51.3389836,12.3786593,12.3795236' 
place_rank='30' address_rank='30'>
Neues Augusteum, Augustusplatz, Leipzig-Zentrum, Mitte, Leipzig, Sachsen, 04109, Deutschland
</result>
   <addressparts>
      <building>Neues Augusteum</building>
      <road>Augustusplatz</road>
      <suburb>Leipzig-Zentrum</suburb>
      <city_district>Mitte</city_district>
      <city>Leipzig</city>
      <state>Sachsen</state>
      <ISO3166-2-lvl4>DE-SN</ISO3166-2-lvl4>
      <postcode>04109</postcode>
      <country>Deutschland</country>
      <country_code>de</country_code>
   </addressparts>
</reversegeocode>
";
#endif

      protected GeoCodingReverseResultOsm(string name,
                                          double lon,
                                          double lat,
                                          double boundleft,
                                          double boundrigth,
                                          double boundbottom,
                                          double boundtop) {
         Name = name;
         Longitude = lon;
         Latitude = lat;
         BoundingLeft = boundleft;
         BoundingTop = boundtop;
         BoundingRight = boundrigth;
         BoundingBottom = boundbottom;
      }

      public static new async Task<GeoCodingReverseResultOsm[]> GetAsync(double lon, double lat, double timeout = 0) {
         GeoCodingReverseResultOsm[] result = Array.Empty<GeoCodingReverseResultOsm>();

         string[] param = new string[] {
                              System.Net.WebUtility.UrlEncode(lat.ToString(CultureInfo.InvariantCulture)),
                              System.Net.WebUtility.UrlEncode(lon.ToString(CultureInfo.InvariantCulture)),
                          };

#if TESTDATA
         System.Net.HttpStatusCode? status2 = System.Net.HttpStatusCode.OK;
         string httpResult = testxml;
#else
         (System.Net.HttpStatusCode? status, string httpResult) = await HttpHelper.GetStringAsync(string.Format(osmformat, param),
                                                                                                  timeout);
#endif

         if (status != null &&
             status == System.Net.HttpStatusCode.OK &&
             !string.IsNullOrEmpty(httpResult)) {
            XmlDocument xmldata = new XmlDocument();
            xmldata.LoadXml(httpResult);

            XmlNamespaceManager? NsMng = null;
            if (xmldata.DocumentElement != null) {
               XmlAttributeCollection attributeCollection = xmldata.DocumentElement.Attributes;
               if (attributeCollection.Count > 0) {
                  NsMng = new XmlNamespaceManager(xmldata.NameTable);
               }
            }

            XPathNavigator? navigator = xmldata.CreateNavigator();

            if (navigator != null && NsMng != null) {
               List<string> txt = getXmlValues("/reversegeocode", navigator, NsMng);
               List<string> resulttxt = getXmlValues("/reversegeocode/result", navigator, NsMng);
               List<string> lattxt = getXmlValues("/reversegeocode/result/@lat", navigator, NsMng);
               List<string> lontxt = getXmlValues("/reversegeocode/result/@lon", navigator, NsMng);
               List<string> bbtxt = getXmlValues("/reversegeocode/result/@boundingbox", navigator, NsMng);

               result = new GeoCodingReverseResultOsm[txt.Count];
               for (int i = 0; i < txt.Count; i++) {
                  string[] tmp = bbtxt[i].Split(',');
                  if (tmp.Length != 4)
                     tmp = new string[] { "0", "0", "0", "0" };
                  result[i] = new GeoCodingReverseResultOsm(
                                          string.IsNullOrEmpty(resulttxt[i]) ? txt[i] : resulttxt[i],
                                          Convert.ToDouble(lontxt[i], CultureInfo.InvariantCulture),
                                          Convert.ToDouble(lattxt[i], CultureInfo.InvariantCulture),
                                          Convert.ToDouble(tmp[2], CultureInfo.InvariantCulture),
                                          Convert.ToDouble(tmp[3], CultureInfo.InvariantCulture),
                                          Convert.ToDouble(tmp[0], CultureInfo.InvariantCulture),
                                          Convert.ToDouble(tmp[1], CultureInfo.InvariantCulture)
                     );
               }

            }
         } else throw new Exception("Error in " + nameof(GetAsync) + ":" + httpResult);
         return result;
      }

   }
}
