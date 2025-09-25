//#define TESTDATA

using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography.GeoCoding {
   public class GeoCodingResultGeocodeFarm : GeoCodingResultBase {

      /*
         https://geocode.farm/geocoding/free-api-documentation/

       USAGE LIMITS:
           Free Users Have 250 Per Day Limit and 4 Requests Per Second Limit (IP Based)
      
      The following URL format should be used: https://www.geocode.farm/v3/[json|xml]/[forward|reverse]/?parameters
      https://www.geocode.farm/v3/json/forward/?addr=leipzig
      https://www.geocode.farm/v3/json/forward/?addr=leipzig&count=10
      https://www.geocode.farm/v3/xml/forward/?addr=leipzig&country=us
      
      ACHTUNG!
      Scheinbar wird in der Variante ohne API-Key immer nur 1 Ergebnis (das "beste"?) geliefert (z.B. für "leipzig").

       */


      const string urlformat = "https://www.geocode.farm/v3/xml/forward/?addr={0}&count=10";

#if TESTDATA
      const string testxml = "<?xml version=\"1.0\"?>" + @"
<geocoding_results>
	<LEGAL_COPYRIGHT>
		<copyright_notice>Copyright (c) 2022 Geocode.Farm - All Rights Reserved.</copyright_notice>
		<copyright_logo>https://www.geocode.farm/images/logo.png</copyright_logo>
		<terms_of_service>https://www.geocode.farm/policies/terms-of-service/</terms_of_service>
		<privacy_policy>https://www.geocode.farm/policies/privacy-policy/</privacy_policy>
	</LEGAL_COPYRIGHT>
	<STATUS>
		<access>FREE_USER, ACCESS_GRANTED</access>
		<status>SUCCESS</status>
		<address_provided>leipzig</address_provided>
		<result_count>1</result_count>
	</STATUS>
	<ACCOUNT>
		<ip_address>95.90.179.33</ip_address>
		<distribution_license>NONE, UNLICENSED</distribution_license>
		<usage_limit>250</usage_limit>
		<used_today>16</used_today>
		<used_total>16</used_total>
		<first_used>04 Jul 2022</first_used>
	</ACCOUNT>
	<RESULTS>
		<result>
			<result_number>1</result_number>
			<formatted_address>Leipzig, ND, United States</formatted_address>
			<accuracy>EXACT_MATCH</accuracy>
			<ADDRESS>
				<locality>Leipzig</locality>
				<admin_2>Grant County</admin_2>
				<admin_1>ND</admin_1>
				<country>United States</country>
			</ADDRESS>
			<LOCATION_DETAILS>
				<elevation>UNAVAILABLE</elevation>
				<timezone_long>UNAVAILABLE</timezone_long>
				<timezone_short>America/Denver</timezone_short>
			</LOCATION_DETAILS>
			<COORDINATES>
				<latitude>46.5160676211924</latitude>
				<longitude>-101.819271668206</longitude>
			</COORDINATES>
			<BOUNDARIES>
				<northeast_latitude>46.4575347900761</northeast_latitude>
				<northeast_longitude>-101.924011230166</northeast_longitude>
				<southwest_latitude>46.5451698303762</southwest_latitude>
				<southwest_longitude>-101.796867370166</southwest_longitude>
			</BOUNDARIES>
		</result>
	</RESULTS>
	<STATISTICS>
		<https_ssl>DISABLED, INSECURE</https_ssl>
		<time_taken>0.39896202087402</time_taken>
	</STATISTICS>
</geocoding_results>";
#endif


      protected GeoCodingResultGeocodeFarm(string name,
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

      public static new async Task<GeoCodingResultGeocodeFarm[]> GetAsync(string name, double timeout = 0) {
         GeoCodingResultGeocodeFarm[] result = System.Array.Empty<GeoCodingResultGeocodeFarm>();

         if (!string.IsNullOrEmpty(name)) {
            string param = System.Net.WebUtility.UrlEncode(name.Trim());

#if TESTDATA
            System.Net.HttpStatusCode? status2 = System.Net.HttpStatusCode.OK;
            string httpResult = testxml;
#else
            (System.Net.HttpStatusCode? status, string httpResult) = await HttpHelper.GetStringAsync(string.Format(urlformat, param),
                                                                                                     timeout);
#endif
            if (status != null &&
             status == System.Net.HttpStatusCode.OK &&
             !string.IsNullOrEmpty(httpResult)) {
               XmlDocument xmldata = new XmlDocument();
               xmldata.LoadXml(httpResult);

               XmlNamespaceManager? NsMng = null;
               NsMng = new XmlNamespaceManager(xmldata.NameTable);

               XPathNavigator? navigator = xmldata.CreateNavigator();
               if (navigator != null && NsMng != null) {
                  if (getXmlValue("/geocoding_results/STATUS/status", navigator, NsMng) == "SUCCESS") {
                     int count = getXmlValueAsInt("/geocoding_results/STATUS/result_count", navigator, NsMng);
                     result = new GeoCodingResultGeocodeFarm[count];

                     for (int i = 1; i <= count; i++) {
                        string mainpath = "/geocoding_results/RESULTS/result/result_number[\"" + i + "\"]";

                        // ACHTUNG: Top und Bottom sind vertauscht!
                        string? adr = getXmlValue(mainpath + "/../formatted_address", navigator, NsMng);
                        if (adr != null)
                           result[i - 1] = new GeoCodingResultGeocodeFarm(
                                                            adr,
                                                            getXmlValueAsDouble(mainpath + "/../COORDINATES/longitude", navigator, NsMng),
                                                            getXmlValueAsDouble(mainpath + "/../COORDINATES/latitude", navigator, NsMng),
                                                            getXmlValueAsDouble(mainpath + "/../BOUNDARIES/northeast_longitude", navigator, NsMng),
                                                            getXmlValueAsDouble(mainpath + "/../BOUNDARIES/southwest_longitude", navigator, NsMng),
                                                            getXmlValueAsDouble(mainpath + "/../BOUNDARIES/northeast_latitude", navigator, NsMng),
                                                            getXmlValueAsDouble(mainpath + "/../BOUNDARIES/southwest_latitude", navigator, NsMng));
                     }
                  }
               }
            }
         }
         return result;
      }
   }
}
