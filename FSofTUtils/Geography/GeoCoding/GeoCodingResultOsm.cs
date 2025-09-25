//#define TESTDATA
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace FSofTUtils.Geography.GeoCoding {

   /*
https://nominatim.org/release-docs/develop/api/Overview/

The search API has the following format:

https://nominatim.openstreetmap.org/search?<params>

The search term may be specified with two different sets of parameters:

 q=<query>
   Free-form query string to search for. Free-form queries are processed first left-to-right and then right-to-left if that fails. 
   So you may search for pilkington avenue, birmingham as well as for birmingham, pilkington avenue. Commas are optional, 
   but improve performance by reducing the complexity of the search.

 street=<housenumber> <streetname>
 city=<city>
 county=<county>
 state=<state>
 country=<country>
 postalcode=<postalcode>

 Alternative query string format split into several parameters for structured requests. Structured requests are faster but are less robust 
against alternative OSM tagging schemas. Do not combine with q=<query> parameter.

https://nominatim.openstreetmap.org/search?q=leipzig&format=xml
https://nominatim.openstreetmap.org/search?q=leipzig&format=json
https://nominatim.openstreetmap.org/search?q=leipzig+schweizerbogen+2&format=json

Output format
-------------			
format=[xml|json|jsonv2|geojson|geocodejson]

Output details
--------------
 addressdetails=[0|1]
   Include a breakdown of the address into elements. (Default: 0)

 extratags=[0|1]
   Include additional information in the result if available, e.g. wikipedia link, opening hours. (Default: 0)

 namedetails=[0|1]
   Include a list of alternative names in the results. These may include language variants, references, operator and brand. (Default: 0)

Language of results
-------------------
 accept-language=<browser language string>
   Preferred language order for showing search results, overrides the value specified in the "Accept-Language" HTTP header. 
   Either use a standard RFC2616 accept-language string or a simple comma-separated list of language codes.

Result limitation
-----------------
 countrycodes=<countrycode>[,<countrycode>][,<countrycode>]...
   Limit search results to one or more countries. <countrycode> must be the ISO 3166-1alpha2 code, e.g. gb for the United Kingdom, de for Germany.
   Each place in Nominatim is assigned to one country code based on OSM country boundaries. In rare cases a place may not be in any country at all, 
   for example, in international waters.

 exclude_place_ids=<place_id,[place_id],[place_id]
   If you do not want certain OSM objects to appear in the search result, give a comma separated list of the place_ids you want to skip. 
   This can be used to retrieve additional search results. For example, if a previous query only returned a few results, then including those 
   here would cause the search to return other, less accurate, matches (if possible).

 limit=<integer>
   Limit the number of returned results. (Default: 10, Maximum: 50)

 viewbox=<x1>,<y1>,<x2>,<y2>
   The preferred area to find search results. Any two corner points of the box are accepted as long as they span a real box. x is longitude, y is latitude.

 bounded=[0|1]
   When a viewbox is given, restrict the result to items contained within that viewbox (see above). When viewbox and bounded=1 are given, 
   an amenity only search is allowed. Give the special keyword for the amenity in square brackets, e.g. [pub] and a selection of objects of this type 
   is returned. There is no guarantee that the result is complete. (Default: 0)



https://nominatim.openstreetmap.org/search?q=leipzig&format=xml

<?xml version="1.0" encoding="UTF-8" ?>
<searchresults timestamp='Thu, 30 Jun 22 07:27:28 +0000'
            attribution='Data © OpenStreetMap contributors, ODbL 1.0. http://www.openstreetmap.org/copyright'
            querystring='leipzig'
            exclude_place_ids='107622,281701349,1401393,26544942,190864965,79627603,147975267,37204025,11456432,111515458'
            more_url='https://nominatim.openstreetmap.org/search/?q=leipzig&amp;exclude_place_ids=107622%2C281701349%2C1401393%2C26544942%2C190864965%2C79627603%2C147975267%2C37204025%2C11456432%2C111515458&amp;format=xml'>
<place place_id='107622'
       osm_type='node'
       osm_id='21687149'
       place_rank='16'
       address_rank='16'
       boundingbox="51.1806321,51.5006321,12.2147329,12.5347329"
       lat='51.3406321'
       lon='12.3747329'
       display_name='Leipzig, Sachsen, 04109, Deutschland'
       class='place'
       type='city'
       importance='0.7936141721816'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_city.p.20.png'/>
<place place_id='281701349'
       osm_type='relation'
       osm_id='62649'
       place_rank='12'
       address_rank='12'
       boundingbox="51.2381704,51.4481145,12.2366519,12.5424407"
       lat='51.34309125'
       lon='12.388553022839412'
       display_name='Leipzig, Sachsen, Deutschland'
       class='boundary'
       type='administrative'
       importance='0.7936141721816'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_boundary_administrative.p.20.png'/>
<place place_id='1401393'
       osm_type='node'
       osm_id='337690835'
       place_rank='18'
       address_rank='16'
       boundingbox="46.2640235,46.3440235,28.9783844,29.0583844"
       lat='46.3040235'
       lon='29.0183844'
       display_name='Серпневе, Тарутинська селищна громада, Болградський район, Одеська область, 68523, Україна'
       class='place'
       type='town'
       importance='0.35759847691767'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_town.p.20.png'/>
<place place_id='26544942'
       osm_type='node'
       osm_id='2727158943'
       place_rank='20'
       address_rank='20'
       boundingbox="52.1657124,52.2057124,-108.6948325,-108.6548325"
       lat='52.1857124'
       lon='-108.6748325'
       display_name='Leipzig, Saskatchewan, Canada'
       class='place'
       type='hamlet'
       importance='0.35'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_village.p.20.png'/>
<place place_id='190864965'
       osm_type='way'
       osm_id='385671163'
       place_rank='18'
       address_rank='16'
       boundingbox="46.2902089,46.3172476,29.0073145,29.0364546"
       lat='46.30384425'
       lon='29.019121668778972'
       display_name='Серпневе, Тарутинська селищна громада, Болградський район, Одеська область, 68523, Україна'
       class='place'
       type='town'
       importance='0.3'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_town.p.20.png'/>
<place place_id='79627603'
       osm_type='node'
       osm_id='7606488674'
       place_rank='22'
       address_rank='20'
       boundingbox="49.1279318,49.1479318,6.0443551,6.0643551"
       lat='49.1379318'
       lon='6.0543551'
       display_name='Leipzig, Châtel-Saint-Germain, Metz, Moselle, Grand Est, France métropolitaine, 57160, France'
       class='place'
       type='farm'
       importance='0.3'/>
<place place_id='147975267'
       osm_type='way'
       osm_id='195533246'
       place_rank='19'
       address_rank='16'
       boundingbox="53.5639696,53.5745361,61.0295497,61.066875"
       lat='53.5694702'
       lon='61.04676314999999'
       display_name='Лейпциг, Лейпцигское сельское поселение, Варненский район, Челябинская область, Уральский федеральный округ, 457214, Россия'
       class='place'
       type='village'
       importance='0.275'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_village.p.20.png'/>
<place place_id='37204025'
       osm_type='node'
       osm_id='3009664791'
       place_rank='20'
       address_rank='22'
       boundingbox="54.3716556,54.3916556,18.5481457,18.5681457"
       lat='54.3816556'
       lon='18.5581457'
       display_name='Lipnik, VII Dwór, Gdańsk, województwo pomorskie, 80-281, Polska'
       class='place'
       type='neighbourhood'
       importance='0.25'/>
<place place_id='11456432'
       osm_type='node'
       osm_id='1200717880'
       place_rank='19'
       address_rank='16'
       boundingbox="53.548829,53.588829,61.029587,61.069587"
       lat='53.568829'
       lon='61.049587'
       display_name='Лейпциг, Лейпцигское сельское поселение, Варненский район, Челябинская область, Уральский федеральный округ, 457214, Россия'
       class='place'
       type='village'
       importance='0.23036154663644'
       icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_village.p.20.png'/>
<place place_id='111515458'
       osm_type='way'
       osm_id='37304668'
       place_rank='26'
       address_rank='26'
       boundingbox="-31.3819664,-31.3804932,-64.1547529,-64.149771"
       lat='-31.381143'
       lon='-64.1525123'
       display_name='Leipzig, San Nicolás, Córdoba, Municipio de Córdoba, Pedanía Capital, Departamento Capital, Córdoba, X5012, Argentina'
       class='highway'
       type='residential'
       importance='0.2'/>
</searchresults>


https://nominatim.openstreetmap.org/search?q=leipzig+schweizerbogen+18&format=xml

<?xml version="1.0" encoding="UTF-8" ?>
<searchresults timestamp='Thu, 30 Jun 22 07:55:47 +0000'
            attribution='Data © OpenStreetMap contributors, ODbL 1.0. http://www.openstreetmap.org/copyright'
            querystring='leipzig schweizerbogen 18'
            exclude_place_ids='146956095'
            more_url='https://nominatim.openstreetmap.org/search/?q=leipzig+schweizerbogen+18&amp;exclude_place_ids=146956095&amp;format=xml'>
<place place_id='146956095'
       osm_type='way'
       osm_id='190820084'
       place_rank='30'
       address_rank='30'
       boundingbox="51.3013755,51.3014725,12.4382878,12.4384228"
       lat='51.301424'
       lon='12.438355299999996'
       display_name='18, Schweizerbogen, Probstheida, Südost, Leipzig, Sachsen, 04289, Deutschland'
       class='building'
       type='house'
       importance='0.3101'/>
</searchresults>

    */


   public class GeoCodingResultOsm : GeoCodingResultBase {

      const string osmformat = "https://nominatim.openstreetmap.org/search?q={0}&format=xml";

#if TESTDATA
      const string testxml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + @"
<searchresults timestamp='Thu, 30 Jun 22 07:27:28 +0000'
               attribution='Data © OpenStreetMap contributors, ODbL 1.0. http://www.openstreetmap.org/copyright'
               querystring='leipzig'
               exclude_place_ids='107622,281701349,1401393,26544942,190864965,79627603,147975267,37204025,11456432,111515458'
               more_url='https://nominatim.openstreetmap.org/search/?q=leipzig&amp;exclude_place_ids=107622%2C281701349%2C1401393%2C26544942%2C190864965%2C79627603%2C147975267%2C37204025%2C11456432%2C111515458&amp;format=xml'>
   <place place_id='107622'
          osm_type='node'
          osm_id='21687149'
          place_rank='16'
          address_rank='16'
          boundingbox='51.1806321,51.5006321,12.2147329,12.5347329'
          lat='51.3406321'
          lon='12.3747329'
          display_name='Leipzig, Sachsen, 04109, Deutschland'
          class='place'
          type='city'
          importance='0.7936141721816'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_city.p.20.png'/>
   <place place_id = '281701349'
          osm_type='relation'
          osm_id='62649'
          place_rank='12'
          address_rank='12'
          boundingbox='51.2381704,51.4481145,12.2366519,12.5424407'
          lat='51.34309125'
          lon='12.388553022839412'
          display_name='Leipzig, Sachsen, Deutschland'
          class='boundary'
          type='administrative'
          importance='0.7936141721816'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_boundary_administrative.p.20.png'/>
   <place place_id = '1401393'
          osm_type='node'
          osm_id='337690835'
          place_rank='18'
          address_rank='16'
          boundingbox='46.2640235,46.3440235,28.9783844,29.0583844'
          lat='46.3040235'
          lon='29.0183844'
          display_name='Серпневе, Тарутинська селищна громада, Болградський район, Одеська область, 68523, Україна'
          class='place'
          type='town'
          importance='0.35759847691767'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_town.p.20.png'/>
   <place place_id = '26544942'
          osm_type='node'
          osm_id='2727158943'
          place_rank='20'
          address_rank='20'
          boundingbox='52.1657124,52.2057124,-108.6948325,-108.6548325'
          lat='52.1857124'
          lon='-108.6748325'
          display_name='Leipzig, Saskatchewan, Canada'
          class='place'
          type='hamlet'
          importance='0.35'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_village.p.20.png'/>
   <place place_id = '190864965'
          osm_type='way'
          osm_id='385671163'
          place_rank='18'
          address_rank='16'
          boundingbox='46.2902089,46.3172476,29.0073145,29.0364546'
          lat='46.30384425'
          lon='29.019121668778972'
          display_name='Серпневе, Тарутинська селищна громада, Болградський район, Одеська область, 68523, Україна'
          class='place'
          type='town'
          importance='0.3'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_town.p.20.png'/>
   <place place_id = '79627603'
          osm_type='node'
          osm_id='7606488674'
          place_rank='22'
          address_rank='20'
          boundingbox='49.1279318,49.1479318,6.0443551,6.0643551'
          lat='49.1379318'
          lon='6.0543551'
          display_name='Leipzig, Châtel-Saint-Germain, Metz, Moselle, Grand Est, France métropolitaine, 57160, France'
          class='place'
          type='farm'
          importance='0.3'/>
   <place place_id = '147975267'
          osm_type='way'
          osm_id='195533246'
          place_rank='19'
          address_rank='16'
          boundingbox='53.5639696,53.5745361,61.0295497,61.066875'
          lat='53.5694702'
          lon='61.04676314999999'
          display_name='Лейпциг, Лейпцигское сельское поселение, Варненский район, Челябинская область, Уральский федеральный округ, 457214, Россия'
          class='place'
          type='village'
          importance='0.275'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_village.p.20.png'/>
   <place place_id = '37204025'
          osm_type='node'
          osm_id='3009664791'
          place_rank='20'
          address_rank='22'
          boundingbox='54.3716556,54.3916556,18.5481457,18.5681457'
          lat='54.3816556'
          lon='18.5581457'
          display_name='Lipnik, VII Dwór, Gdańsk, województwo pomorskie, 80-281, Polska'
          class='place'
          type='neighbourhood'
          importance='0.25'/>
   <place place_id = '11456432'
          osm_type='node'
          osm_id='1200717880'
          place_rank='19'
          address_rank='16'
          boundingbox='53.548829,53.588829,61.029587,61.069587'
          lat='53.568829'
          lon='61.049587'
          display_name='Лейпциг, Лейпцигское сельское поселение, Варненский район, Челябинская область, Уральский федеральный округ, 457214, Россия'
          class='place'
          type='village'
          importance='0.23036154663644'
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_village.p.20.png'/>
   <place place_id = '111515458'
          osm_type='way'
          osm_id='37304668'
          place_rank='26'
          address_rank='26'
          boundingbox='-31.3819664,-31.3804932,-64.1547529,-64.149771'
          lat='-31.381143'
          lon='-64.1525123'
          display_name='Leipzig, San Nicolás, Córdoba, Municipio de Córdoba, Pedanía Capital, Departamento Capital, Córdoba, X5012, Argentina'
          class='highway'
          type='residential'
          importance='0.2'/>
</searchresults>
";
#endif


      public string OsmClass { get; protected set; }

      public string OsmValue { get; protected set; }


      protected GeoCodingResultOsm(string name,
                                   double lon,
                                   double lat,
                                   double boundleft,
                                   double boundrigth,
                                   double boundbottom,
                                   double boundtop,
                                   string osmclass,
                                   string osmvalue) {
         Name = name;
         Longitude = lon;
         Latitude = lat;
         BoundingLeft = boundleft;
         BoundingTop = boundtop;
         BoundingRight = boundrigth;
         BoundingBottom = boundbottom;
         OsmClass = osmclass;
         OsmValue = osmvalue;
      }

      public static new async Task<GeoCodingResultOsm[]> GetAsync(string name, double timeout = 0) {
         GeoCodingResultOsm[] result = Array.Empty<GeoCodingResultOsm>();

         if (!string.IsNullOrEmpty(name)) {
            string param = System.Net.WebUtility.UrlEncode(name.Trim());

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
                     //for (int i = 0; i < attributeCollection.Count; i++) {
                     //   XmlAttribute attribute = attributeCollection[i];
                     //   if (NsMng.LookupNamespace(attribute.LocalName) == null)
                     //      NsMng.AddNamespace(attribute.LocalName, attribute.Value);
                     //}
                  }
               }

               XPathNavigator? navigator = xmldata.CreateNavigator();

               if (navigator != null && NsMng != null) {
                  /*
   <place place_id='107622'                                                                        reference to the Nominatim internal database ID
          osm_type='node'                                                                          reference to the OSM object
          osm_id='21687149'                                                                        "
          place_rank='16'
          address_rank='16'                                                                        
          boundingbox="51.1806321,51.5006321,12.2147329,12.5347329"                                comma-separated list of min latitude, max latitude, min longitude, max longitude. The whole planet would be -90,90,-180,180.
          lat='51.3406321'                                                                         latitude and longitude of the centroid of the object
          lon='12.3747329'                                                                         "
          display_name='Leipzig, Sachsen, 04109, Deutschland'                                      full comma-separated address
          class='place'                                                                            key and value of the main OSM tag
          type='city'                                                                              key and value of the main OSM tag
          importance='0.7936141721816'                                                             computed importance rank
          icon='https://nominatim.openstreetmap.org/ui/mapicons/poi_place_city.p.20.png'/>         link to class icon (if available)
                   */
                  List<string> txt = getXmlValues("/searchresults/place/@display_name", navigator, NsMng);
                  List<string> lattxt = getXmlValues("/searchresults/place/@lat", navigator, NsMng);
                  List<string> lontxt = getXmlValues("/searchresults/place/@lon", navigator, NsMng);
                  List<string> bbtxt = getXmlValues("/searchresults/place/@boundingbox", navigator, NsMng);
                  List<string> osmclass = getXmlValues("/searchresults/place/@class", navigator, NsMng);
                  List<string> osmval = getXmlValues("/searchresults/place/@type", navigator, NsMng);

                  result = new GeoCodingResultOsm[txt.Count];
                  for (int i = 0; i < txt.Count; i++) {
                     string[] tmp = bbtxt[i].Split(',');
                     if (tmp.Length != 4)
                        tmp = new string[] { "0", "0", "0", "0" };
                     result[i] = new GeoCodingResultOsm(
                                             txt[i],
                                             Convert.ToDouble(lontxt[i], CultureInfo.InvariantCulture),
                                             Convert.ToDouble(lattxt[i], CultureInfo.InvariantCulture),
                                             Convert.ToDouble(tmp[2], CultureInfo.InvariantCulture),
                                             Convert.ToDouble(tmp[3], CultureInfo.InvariantCulture),
                                             Convert.ToDouble(tmp[0], CultureInfo.InvariantCulture),
                                             Convert.ToDouble(tmp[1], CultureInfo.InvariantCulture),
                                             osmclass[i],
                                             osmval[i]
                        );
                  }

               }
            } else throw new Exception("Error in " + nameof(GetAsync) + ":" + httpResult);
         }
         return result;
      }

   }
}
