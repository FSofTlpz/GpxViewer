using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FSofTUtils.Geography.Garmin {

   /// <summary>
   /// Behandlung von Garmin-GDB-Dateien (z.Z. nur lesend)
   /// </summary>
   public static class GDB {

      public class Header {

         /// <summary>
         /// i.A. "MapSource" oder "BaseCamp"
         /// </summary>
         public string? Progname = "";

         public Version Version = new Version();

         /// <summary>
         /// z.B.: "Oct 14 2010", "08:33:45"
         /// </summary>
         public DateTime DateTime = new DateTime();


         public Header(BinaryReader reader) {
            /*
            Offset(h) 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
            00000000  4D 73 52 63 66 00 02 00 00 00 44 6D 00 1B 00 00  MsRcf.....Dm....
            00000010  00 41 68 02 73 71 61 00 4F 63 74 20 31 34 20 32  .Ah.sqa.Oct 14 2
            00000020  30 31 30 00 30 38 3A 33 33 3A 34 35 00 4D 61 70  010.08:33:45.Map
            00000030  53 6F 75 72 63 65 00 70 00 00 00 57 30 30 37 00  Source.p...W007.

            4D 73 52 63 66 00                            MsRcf

            02 00 00 00 
            44                                           D
            6D 00                                        m

            1B 00 00 00 
            41                                           A
            68 02 73 71 61 00                            h.sqa
            4F 63 74 20 31 34 20 32 30 31 30 00          Oct 14 2010
            30 38 3A 33 33 3A 34 35 00                   08:33:45

            4D 61 70 53 6F 75 72 63 65 00                MapSource

            Kennungsstring "MsRcf"
            D-Block
            A-Block
            Namesstring "MapSource"
            Objekte

             */

            if (reader.BaseStream.Length > 6) {
               byte[] tmp = reader.ReadBytes(6);
               if (Encoding.ASCII.GetString(tmp, 0, tmp.Length) == "MsRcf\0") {

                  ObjectHeader objheader = new ObjectHeader(reader);
                  if (objheader.ObjectType == ObjectHeader.GDBObjectType.VERSION) {
                     Version = new Version(reader, objheader);

                     objheader = new ObjectHeader(reader);
                     if (objheader.ObjectType == ObjectHeader.GDBObjectType.DATETIME) {
                        DateTime = new DateTime(reader, objheader);

                        Progname = Object.read_CString(reader, Encoding.ASCII);    // i.A. "MapSource" oder "BaseCamp"
                     }
                  }
               }
            }

         }

         public override string ToString() {
            return string.Format("Version={0}, DateTime={1}, Progname={2}", Version, DateTime, Progname);
         }

      }

      public class ObjectHeader {

         public enum GDBObjectType {
            UNKNOWN = (byte)'?',

            VERSION = (byte)'D',
            DATETIME = (byte)'A',
            WAYPOINT = (byte)'W',
            ROUTE = (byte)'R',
            TRACK = (byte)'T',
            LAST = (byte)'V',
         }

         public GDBObjectType ObjectType;

         public int Length;


         public ObjectHeader() {
            ObjectType = GDBObjectType.UNKNOWN;
            Length = 0;
         }

         public ObjectHeader(BinaryReader reader) {
            Length = reader.ReadInt32();
            ObjectType = (GDBObjectType)reader.ReadByte();
         }

         public override string ToString() {
            return string.Format("ObjectType={0}, Length={1} Bytes", ObjectType, Length);
         }

      }

      public class Object {

         public ObjectHeader ObjectHeader;

         public string Name {
            get; protected set;
         } = "";

         /// <summary>
         /// Streampos. an der die Daten (Name) beginnen (darauf bezieht sich die Objektgröße)
         /// </summary>
         protected long dataStart;


         public Object() {
            ObjectHeader = new ObjectHeader();
            Name = "";
            dataStart = 0;
         }

         public Object(BinaryReader reader, ObjectHeader header) {
            ObjectHeader = header;
            dataStart = reader.BaseStream.Position;
            string? tmp = read_CString(reader);
            Name = tmp != null ? tmp : string.Empty;
         }

         /// <summary>
         /// nur zum "Überlesen" des noch unbekannten Datenbereiches (der <see cref="BinaryReader"/> muss direkt hinter dem Namen des Objektes stehen)
         /// </summary>
         /// <param name="reader"></param>
         /// <returns></returns>
         public byte[] ReadDummy(BinaryReader reader) {
            return Name != null ? reader.ReadBytes(ObjectHeader.Length - Name.Length - 1) : [];
         }

         /// <summary>
         /// Text als UTF8 lesen
         /// </summary>
         /// <param name="reader"></param>
         /// <returns></returns>
         public static string? read_CString(BinaryReader reader) {
            return read_CString(reader, Encoding.UTF8);
         }

         /// <summary>
         /// Text-Array als UTF8 lesen
         /// </summary>
         /// <param name="reader"></param>
         /// <returns></returns>
         public static List<string> read_CStringArray(BinaryReader reader) {
            return read_CStringArray(reader, Encoding.UTF8);
         }


         /// <summary>
         /// liest den C-String einschließlich End-0 ein
         /// <para>Exception bei vorzeitigem Ende</para>
         /// </summary>
         /// <param name="reader"></param>
         /// <returns></returns>
         public static string? read_CString(BinaryReader reader, Encoding encoding) {
            List<byte> bytes = new List<byte>();
            byte b = reader.ReadByte();
            while (b != 0) {
               bytes.Add(b);
               b = reader.ReadByte();
            }
            bytes.Add(0);
            return read_CString(bytes, 0, encoding);
         }

         public static List<string> read_CStringArray(BinaryReader reader, Encoding encoding) {
            List<string> lst = new List<string>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++) {
               string? str = read_CString(reader, encoding);
               if (str != null)
                  lst.Add(str);
            }
            return lst;
         }

         /// <summary>
         /// eine mit 0-Byte abgeschlossene Byte-Liste wird als Text interpretiert
         /// </summary>
         /// <param name="buffer"></param>
         /// <param name="start"></param>
         /// <returns></returns>
         static string? read_CString(IList<byte> buffer, int start, Encoding encoding) {
            List<byte> bytes = new List<byte>();
            bool found0 = false;
            for (int i = start; i < buffer.Count; i++) {
               if (buffer[i] == 0) {
                  found0 = true;
                  break;
               } else
                  bytes.Add(buffer[i]);
            }

            // Encoding.ASCII.GetString(bytes.ToArray())

            return found0 ? encoding.GetString(bytes.ToArray()) : null;
         }

         /// <summary>
         /// liefert die Unixzeit (Anzahl der vergangenen Sekunden seit dem 1.1.1970 um 00:00)
         /// </summary>
         /// <param name="gdbtime"></param>
         /// <returns></returns>
         public static System.DateTime get_DateTime(int gdbtime) {
            return new System.DateTime(1970, 1, 1).AddSeconds(gdbtime);
         }

         public override string ToString() {
            return string.Format("Name={0}", Name);
            //return string.Format("{0}, Name={1}", ObjectHeader.ToString(), Name);
         }
      }

      public class Version : Object {

         public enum VersionKey {
            V2 = (byte)'k',
            V3 = (byte)'m',
         }


         public VersionKey GDBVersion = VersionKey.V3;


         public Version() : base() {
         }

         public Version(BinaryReader reader, ObjectHeader header) :
            base(reader, header) {
            if (Name == null || Name.Length == 0)
               throw new Exception("no Version");
            GDBVersion = (VersionKey)Name[0];
         }

         public override string ToString() {
            return string.Format("{0}, Version={1}", base.ToString(), GDBVersion);
         }

      }

      /// <summary>
      /// Daten aus dem Header (nur als Text und NICHT gleichbedeutend mit dem Dateidatum)
      /// </summary>
      public class DateTime : Object {

         public List<string> Data = new List<string>();

         public DateTime() : base() {
         }

         public DateTime(BinaryReader reader, ObjectHeader header) :
            base(reader, header) {
            string? str = read_CString(reader, Encoding.ASCII);
            if (str != null) {
               Data.Add(str);
               str = read_CString(reader, Encoding.ASCII);
               if (str != null)
                  Data.Add(str);
            }
         }

         public override string ToString() {
            return string.Format("{0}, Data={1}", base.ToString(), string.Join(";", Data));
         }

      }

      public class LastObject : Object {

         public byte Data = 1;


         public LastObject() : base() {
         }

         public LastObject(BinaryReader reader, ObjectHeader header) :
            base(reader, header) {
            Data = reader.ReadByte();
         }

         public override string ToString() {
            return string.Format("{0}, Data={1}", base.ToString(), Data);
         }

      }

      public class TrackPoint {

         /// <summary>
         /// zur Umwandlung int in Grad
         /// </summary>
         public const double INT2DEGREE = -180.0 / (1 << 31);

         public double Lat = double.MinValue;
         public double Lon = double.MinValue;
         public double Ele = double.MinValue;
         public double Depth = double.MinValue;
         public double Temp = double.MinValue;
         public System.DateTime DateTime;

         public TrackPoint() {
         }

         public TrackPoint(BinaryReader reader) {
            Lat = reader.ReadInt32() * INT2DEGREE;
            Lon = reader.ReadInt32() * INT2DEGREE;
            if (reader.ReadByte() == 1)
               Ele = reader.ReadDouble();
            DateTime = Object.get_DateTime(reader.ReadByte() == 1 ?
                                                reader.ReadInt32() :
                                                0);
            if (reader.ReadByte() == 1)
               Depth = reader.ReadDouble();
            if (reader.ReadByte() == 1)
               Temp = reader.ReadDouble();
         }

         public override string ToString() {
            return string.Format("Lat={0}°, Lon={1}°", Lat, Lon);
         }
      }

      public class Track : Object {

         public int ColorIdx = -1;

         public List<TrackPoint> Points = new List<TrackPoint>();

         public List<string> Url = new List<string>();


         public Track() :
            base() {
         }

         public Track(BinaryReader reader, ObjectHeader header, Version version) :
            base(reader, header) {

            if (reader.ReadByte() == 1)         // sonst "nicht vorhanden"
               ColorIdx = reader.ReadInt32();

            int points = reader.ReadInt32();    // Punktanzahl
            for (int i = 0; i < points; i++)
               Points.Add(new TrackPoint(reader));

            string? str;
            switch (version.GDBVersion) {
               case Version.VersionKey.V2:
                  str = read_CString(reader);
                  if (str != null)
                     Url.Add(str);
                  break;

               case Version.VersionKey.V3:
                  int urls = reader.ReadInt32();
                  for (int i = 0; i < urls; i++) {
                     str = read_CString(reader);
                     if (str != null)
                        Url.Add(str);
                  }
                  break;
            }

            long bytes = reader.BaseStream.Position - dataStart;
            if (ObjectHeader.Length != bytes)
               throw new Exception("Diff. beim Einlesen: " + (bytes - ObjectHeader.Length) + " Bytes");
         }

         public override string ToString() {
            return string.Format("{0}, Points={1}, ColorIdx={2}", base.ToString(), Points.Count, ColorIdx);
         }

      }

      public class Waypoint : Object {

         public int WaypointClass = -1;
         public string? CountryCode;
         public byte[]? Unknown22;
         public double Lat;
         public double Lon;
         public double Ele = double.MinValue;
         public string Description = string.Empty;
         public double Proximity = double.MinValue;
         public int DisplayMode = -1;
         public int ColorIdx = -1;
         public int IconIdx = -1;
         public string? City;
         public string? State;
         public string? Facility;
         public byte Unknown1a;
         public double Depth = double.MinValue;
         public string? Address;
         public byte Unknown1b;
         public int Duration = -1;
         public string? Instruction;
         public List<string>? Url;
         public int Category;
         public double Temperature = double.MinValue;
         public System.DateTime CreationTime;
         public List<string>? Tel;
         public string? Fax;
         public string? Country;
         public string? Zip;

         public byte[]? UnknownV2_2;
         public byte UnknownV2_1;
         public byte[]? UnknownV2_3;
         public string? UnknownV2;

         /*
          20 Punkte für Route
         4 Zwischenziele

          UTF8 !
*/


         public Waypoint() : base() {
         }

         public Waypoint(BinaryReader reader, ObjectHeader header, Version version) :
            base(reader, header) {

            WaypointClass = reader.ReadInt32();
            CountryCode = read_CString(reader);
            Unknown22 = reader.ReadBytes(22);
            Lat = reader.ReadInt32() * TrackPoint.INT2DEGREE;
            Lon = reader.ReadInt32() * TrackPoint.INT2DEGREE;
            if (reader.ReadByte() == 1)
               Ele = reader.ReadDouble();
            string? tmp = read_CString(reader);
            Description = tmp != null ? tmp : string.Empty;
            if (reader.ReadByte() == 1)
               Proximity = reader.ReadDouble();
            DisplayMode = reader.ReadInt32();
            ColorIdx = reader.ReadInt32();
            IconIdx = reader.ReadInt32();
            City = read_CString(reader);
            State = read_CString(reader);
            Facility = read_CString(reader);
            Unknown1a = reader.ReadByte();
            if (reader.ReadByte() == 1)
               Depth = reader.ReadDouble();

            if (version.GDBVersion == Version.VersionKey.V2) {

               UnknownV2_2 = reader.ReadBytes(2);
               UnknownV2_1 = reader.ReadByte();
               UnknownV2_3 = UnknownV2_1 == 0 ?
                                 reader.ReadBytes(3) :
                                 reader.ReadBytes(2);
               UnknownV2 = read_CString(reader);
               string? str = read_CString(reader);
               if (str != null)
                  Url = new List<string>() { str };

            } else {

               Address = read_CString(reader);
               Unknown1b = reader.ReadByte();
               Duration = reader.ReadInt32();
               Instruction = read_CString(reader);
               Url = read_CStringArray(reader);

            }

            Category = reader.ReadInt16();
            if (reader.ReadByte() == 1)
               Temperature = reader.ReadDouble();
            CreationTime = get_DateTime(reader.ReadByte() == 1 ?
                                                reader.ReadInt32() :
                                                0);
            if (version.GDBVersion != Version.VersionKey.V2) {
               Tel = read_CStringArray(reader);
               if (Tel.Count > 0)
                  Fax = read_CString(reader);
               Country = read_CString(reader);
               Zip = read_CString(reader);
            }

            long bytes = reader.BaseStream.Position - dataStart;
            if (ObjectHeader.Length != bytes)
               throw new Exception("Diff. beim Einlesen: " + (bytes - ObjectHeader.Length) + " Bytes");
         }

         public override string ToString() {
            return string.Format("{0}, Lat={1}°, Lon={2}°", base.ToString(), Lat, Lon);
         }

      }

      public class Route : Object {

         public byte display;

         public Bounds Bounds;

         public List<RoutePoint> Points = new List<RoutePoint>();

         public List<string>? Url;

         public string? Description;
         public int ColorIdx = -1;
         public byte Autoroute = 0;
         public byte[]? Unknown6;
         public byte RouteStyle;
         public int CalcType;
         public byte VehicleType;
         public int RoadSelection;
         public double[] DrivingSpeed = new double[5];
         public byte[]? Unknown8;


         public Route() :
            base() {
            Bounds = new Bounds();
         }

         public Route(BinaryReader reader, ObjectHeader header, Version version) :
            base(reader, header) {

            display = reader.ReadByte();

            if (reader.ReadByte() == 0)
               Bounds = new Bounds(reader);
            else
               Bounds = new Bounds();

            int points = reader.ReadInt32();
            for (int i = 0; i < points; i++)
               Points.Add(new RoutePoint(reader, version));

            if (version.GDBVersion == Version.VersionKey.V2) {
               string? str = read_CString(reader);
               if (str != null)
                  Url = new List<string>() { str };
            } else {
               Url = read_CStringArray(reader);
               ColorIdx = reader.ReadInt32();
               Autoroute = reader.ReadByte();
               if (Autoroute == 1) {
                  Unknown6 = reader.ReadBytes(6);
                  RouteStyle = reader.ReadByte();
                  CalcType = reader.ReadInt32();
                  VehicleType = reader.ReadByte();
                  RoadSelection = reader.ReadInt32();
                  for (int i = 0; i < DrivingSpeed.Length; i++)
                     DrivingSpeed[i] = reader.ReadDouble();
                  Unknown8 = reader.ReadBytes(8);
               }
            }
            Description = read_CString(reader);

            long bytes = reader.BaseStream.Position - dataStart;
            if (ObjectHeader.Length != bytes)
               throw new Exception("Diff. beim Einlesen: " + (bytes - ObjectHeader.Length) + " Bytes");
         }

         public override string ToString() {
            return string.Format("{0}, Points={1}", base.ToString(), Points.Count);
         }

      }

      public class Bounds {

         public double MaxLat = double.MinValue;
         public double MaxLon = double.MinValue;
         public double MaxEle = double.MinValue;
         public double MinLat = double.MinValue;
         public double MinLon = double.MinValue;
         public double MinEle = double.MinValue;

         public Bounds() { }

         public Bounds(BinaryReader reader) {
            MaxLat = reader.ReadInt32() * TrackPoint.INT2DEGREE;
            MaxLon = reader.ReadInt32() * TrackPoint.INT2DEGREE;
            if (reader.ReadByte() == 1)
               MaxEle = reader.ReadDouble();
            MinLat = reader.ReadInt32() * TrackPoint.INT2DEGREE;
            MinLon = reader.ReadInt32() * TrackPoint.INT2DEGREE;
            if (reader.ReadByte() == 1)
               MinEle = reader.ReadDouble();
         }

         public override string ToString() {
            return string.Format("Lat {0}°..{1}°, Lon {2}°..{3}°", MinLat, MaxLat, MinLon, MaxLon);
         }

      }

      public class RoutePoint {

         public string? Name;
         public int WaypointClass;
         public string? CountryCode;
         public byte[]? Unknown22;
         public byte[]? Unknown8a;
         public byte[]? Unknown8b;
         public byte[]? Unknown2;
         public byte[]? Unknown18;

         public List<double> Lat = new List<double>();
         public List<double> Lon = new List<double>();
         public List<double> Ele = new List<double>();
         public Bounds? Bounds;


         public RoutePoint() {
         }

         public RoutePoint(BinaryReader reader, Version version) {
            Name = Object.read_CString(reader);
            WaypointClass = reader.ReadInt32();
            CountryCode = Object.read_CString(reader);
            Unknown22 = reader.ReadBytes(22);

            if (reader.ReadByte() != 0) {
               Unknown8a = reader.ReadBytes(8);
               if (version.GDBVersion != Version.VersionKey.V2)
                  Unknown8b = reader.ReadBytes(8);
            }
            Unknown18 = reader.ReadBytes(18);

            int links = reader.ReadInt32();
            for (int i = 0; i < links; i++) {
               Lat.Add(reader.ReadInt32() * TrackPoint.INT2DEGREE);
               Lon.Add(reader.ReadInt32() * TrackPoint.INT2DEGREE);
               Ele.Add(reader.ReadByte() == 1 ?
                                 reader.ReadDouble() :
                                 double.MinValue);
            }

            if (reader.ReadByte() == 0) // interlink bounds 
               Bounds = new Bounds(reader);
            else
               Bounds = new Bounds();

            if (links == 0) {
               // Without links we need all information from wpt 


            }

            Unknown8a = reader.ReadBytes(8);
            if (version.GDBVersion != Version.VersionKey.V2)
               Unknown2 = reader.ReadBytes(2);
         }

         public override string ToString() {
            return string.Format("Name={0}", Name);
         }
      }


      static Object readobject(BinaryReader reader, Version version) {
         Object obj;
         ObjectHeader header = new ObjectHeader(reader);
         switch (header.ObjectType) {
            case ObjectHeader.GDBObjectType.TRACK:
               obj = new Track(reader, header, version);
               break;

            case ObjectHeader.GDBObjectType.WAYPOINT:
               obj = new Waypoint(reader, header, version);
               break;

            case ObjectHeader.GDBObjectType.ROUTE:
               obj = new Route(reader, header, version);
               break;

            case ObjectHeader.GDBObjectType.LAST:
               obj = new LastObject(reader, header);
               break;

            default:
               obj = new Object(reader, header);
               obj.ReadDummy(reader);
               break;
         }
         return obj;
      }

      public static List<Object> ReadGDBObjectList(string gdbfilename) {
         using (BinaryReader reader = new BinaryReader(File.Open(gdbfilename, FileMode.Open))) {
            return ReadGDBObjectList(reader, new Header(reader).Version);
         }
      }

      public static List<Object> ReadGDBObjectList(BinaryReader reader, Version version) {
         List<Object> lst = new List<Object>();
         Object? obj = null;
         do {
            obj = readobject(reader, version);
            if (obj != null)
               lst.Add(obj);
         } while (obj != null && obj.ObjectHeader.ObjectType != ObjectHeader.GDBObjectType.LAST);
         return lst;
      }


      // ----------------------------------------------------------------------

      // from gpsbabel-master.zip\gpsbabel-master\garmin_icon_tables.h

      class IconMapping {
         public int mpssymnum;
         public int pcxsymnum;
         public string? icon;
      };

      /* MapSource 4.13 */
      static IconMapping[] garmin_icon_table = {
           /*	  mps    pcx    desc */
           new IconMapping() { mpssymnum =  107, pcxsymnum = 16384, icon = "Airport" },
           new IconMapping() { mpssymnum =   73, pcxsymnum =  8204, icon = "Amusement Park" },
           new IconMapping() { mpssymnum =   55, pcxsymnum =   169, icon = "Ball Park" },
           new IconMapping() { mpssymnum =    6, pcxsymnum =     6, icon = "Bank" },
           new IconMapping() { mpssymnum =   13, pcxsymnum =    13, icon = "Bar" },
           new IconMapping() { mpssymnum =  104, pcxsymnum =  8244, icon = "Beach" },
           new IconMapping() { mpssymnum =    1, pcxsymnum =     1, icon = "Bell" },
           new IconMapping() { mpssymnum =   37, pcxsymnum =   150, icon = "Boat Ramp" },
           new IconMapping() { mpssymnum =   74, pcxsymnum =  8205, icon = "Bowling" },
           new IconMapping() { mpssymnum =   93, pcxsymnum =  8233, icon = "Bridge" },
           new IconMapping() { mpssymnum =   94, pcxsymnum =  8234, icon = "Building" },
           new IconMapping() { mpssymnum =   38, pcxsymnum =   151, icon = "Campground" },
           new IconMapping() { mpssymnum =   56, pcxsymnum =   170, icon = "Car" },
           new IconMapping() { mpssymnum =   75, pcxsymnum =  8206, icon = "Car Rental" },
           new IconMapping() { mpssymnum =   76, pcxsymnum =  8207, icon = "Car Repair" },
           new IconMapping() { mpssymnum =   95, pcxsymnum =  8235, icon = "Cemetery" },
           new IconMapping() { mpssymnum =   96, pcxsymnum =  8236, icon = "Church" },
           new IconMapping() { mpssymnum =   65, pcxsymnum =   179, icon = "Circle with X" },
           new IconMapping() { mpssymnum =   72, pcxsymnum =  8203, icon = "City (Capitol)" },
           new IconMapping() { mpssymnum =   71, pcxsymnum =  8200, icon = "City (Large)" },
           new IconMapping() { mpssymnum =   70, pcxsymnum =  8199, icon = "City (Medium)" },
           new IconMapping() { mpssymnum =   69, pcxsymnum =  8198, icon = "City (Small)" },
           new IconMapping() { mpssymnum =   69, pcxsymnum =  8198, icon = "Small City" },
           new IconMapping() { mpssymnum =   97, pcxsymnum =  8237, icon = "Civil" },
           new IconMapping() { mpssymnum =  119, pcxsymnum =  8262, icon = "Contact, Afro" },
           new IconMapping() { mpssymnum =  120, pcxsymnum =  8272, icon = "Contact, Alien" },
           new IconMapping() { mpssymnum =  121, pcxsymnum =  8258, icon = "Contact, Ball Cap" },
           new IconMapping() { mpssymnum =  122, pcxsymnum =  8259, icon = "Contact, Big Ears" },
           new IconMapping() { mpssymnum =  123, pcxsymnum =  8271, icon = "Contact, Biker" },
           new IconMapping() { mpssymnum =  124, pcxsymnum =  8273, icon = "Contact, Bug" },
           new IconMapping() { mpssymnum =  125, pcxsymnum =  8274, icon = "Contact, Cat" },
           new IconMapping() { mpssymnum =  126, pcxsymnum =  8275, icon = "Contact, Dog" },
           new IconMapping() { mpssymnum =  127, pcxsymnum =  8263, icon = "Contact, Dreadlocks" },
           new IconMapping() { mpssymnum =  128, pcxsymnum =  8264, icon = "Contact, Female1" },
           new IconMapping() { mpssymnum =  129, pcxsymnum =  8265, icon = "Contact, Female2" },
           new IconMapping() { mpssymnum =  130, pcxsymnum =  8266, icon = "Contact, Female3" },
           new IconMapping() { mpssymnum =  131, pcxsymnum =  8261, icon = "Contact, Goatee" },
           new IconMapping() { mpssymnum =  132, pcxsymnum =  8268, icon = "Contact, Kung-Fu" },
           new IconMapping() { mpssymnum =  133, pcxsymnum =  8276, icon = "Contact, Pig" },
           new IconMapping() { mpssymnum =  134, pcxsymnum =  8270, icon = "Contact, Pirate" },
           new IconMapping() { mpssymnum =  135, pcxsymnum =  8267, icon = "Contact, Ranger" },
           new IconMapping() { mpssymnum =  136, pcxsymnum =  8257, icon = "Contact, Smiley" },
           new IconMapping() { mpssymnum =  137, pcxsymnum =  8260, icon = "Contact, Spike" },
           new IconMapping() { mpssymnum =  138, pcxsymnum =  8269, icon = "Contact, Sumo" },
           new IconMapping() { mpssymnum =   52, pcxsymnum =   165, icon = "Controlled Area" },
           new IconMapping() { mpssymnum =   89, pcxsymnum =  8220, icon = "Convenience Store" },
           new IconMapping() { mpssymnum =   98, pcxsymnum =  8238, icon = "Crossing" },
           new IconMapping() { mpssymnum =   51, pcxsymnum =   164, icon = "Dam" },
           new IconMapping() { mpssymnum =   53, pcxsymnum =   166, icon = "Danger Area" },
           new IconMapping() { mpssymnum =   87, pcxsymnum =  8218, icon = "Department Store" },
           new IconMapping() { mpssymnum =    4, pcxsymnum =     4, icon = "Diver Down Flag 1" },
           new IconMapping() { mpssymnum =    5, pcxsymnum =     5, icon = "Diver Down Flag 2" },
           new IconMapping() { mpssymnum =   41, pcxsymnum =   154, icon = "Drinking Water" },
           new IconMapping() { mpssymnum =   63, pcxsymnum =   177, icon = "Exit" },
           new IconMapping() { mpssymnum =   77, pcxsymnum =  8208, icon = "Fast Food" },
           new IconMapping() { mpssymnum =    7, pcxsymnum =     7, icon = "Fishing Area" },
           new IconMapping() { mpssymnum =   78, pcxsymnum =  8209, icon = "Fitness Center" },
           new IconMapping() { mpssymnum =   64, pcxsymnum =   178, icon = "Flag" },
           new IconMapping() { mpssymnum =  105, pcxsymnum =  8245, icon = "Forest" },
           new IconMapping() { mpssymnum =    8, pcxsymnum =     8, icon = "Gas Station" },
           new IconMapping() { mpssymnum =  117, pcxsymnum =  8255, icon = "Geocache" },
           new IconMapping() { mpssymnum =  118, pcxsymnum =  8256, icon = "Geocache Found" },
           new IconMapping() { mpssymnum =   99, pcxsymnum =  8239, icon = "Ghost Town" },
           new IconMapping() { mpssymnum =  113, pcxsymnum = 16393, icon = "Glider Area" },
           new IconMapping() { mpssymnum =   68, pcxsymnum =  8197, icon = "Golf Course" },
           new IconMapping() { mpssymnum =    2, pcxsymnum =     2, icon = "Diamond, Green" },
           new IconMapping() { mpssymnum =   15, pcxsymnum =    15, icon = "Square, Green" },
           new IconMapping() { mpssymnum =  108, pcxsymnum = 16388, icon = "Heliport" },
           new IconMapping() { mpssymnum =    9, pcxsymnum =     9, icon = "Horn" },
           new IconMapping() { mpssymnum =   57, pcxsymnum =   171, icon = "Hunting Area" },
           new IconMapping() { mpssymnum =   44, pcxsymnum =   157, icon = "Information" },
           new IconMapping() { mpssymnum =  100, pcxsymnum =  8240, icon = "Levee" },
           new IconMapping() { mpssymnum =   12, pcxsymnum =    12, icon = "Light" },
           new IconMapping() { mpssymnum =   90, pcxsymnum =  8221, icon = "Live Theater" },
           new IconMapping() { mpssymnum =   59, pcxsymnum =   173, icon = "Lodging" },
           new IconMapping() { mpssymnum =   59, pcxsymnum =   173, icon = "Hotel" },
           new IconMapping() { mpssymnum =   20, pcxsymnum =    21, icon = "Man Overboard" },
           new IconMapping() { mpssymnum =    0, pcxsymnum =     0, icon = "Anchor" },
           new IconMapping() { mpssymnum =   43, pcxsymnum =   156, icon = "Medical Facility" },
           new IconMapping() { mpssymnum =   66, pcxsymnum =  8195, icon = "Mile Marker" },
           new IconMapping() { mpssymnum =  101, pcxsymnum =  8241, icon = "Military" },
           new IconMapping() { mpssymnum =   60, pcxsymnum =   174, icon = "Mine" },
           new IconMapping() { mpssymnum =   79, pcxsymnum =  8210, icon = "Movie Theater" },
           new IconMapping() { mpssymnum =   80, pcxsymnum =  8211, icon = "Museum" },
           new IconMapping() { mpssymnum =   21, pcxsymnum =    22, icon = "Navaid, Amber" },
           new IconMapping() { mpssymnum =   22, pcxsymnum =    23, icon = "Navaid, Black" },
           new IconMapping() { mpssymnum =   23, pcxsymnum =    24, icon = "Navaid, Blue" },
           new IconMapping() { mpssymnum =   24, pcxsymnum =    25, icon = "Navaid, Green" },
           new IconMapping() { mpssymnum =   25, pcxsymnum =    26, icon = "Navaid, Green/Red" },
           new IconMapping() { mpssymnum =   26, pcxsymnum =    27, icon = "Navaid, Green/White" },
           new IconMapping() { mpssymnum =   27, pcxsymnum =    28, icon = "Navaid, Orange" },
           new IconMapping() { mpssymnum =   28, pcxsymnum =    29, icon = "Navaid, Red" },
           new IconMapping() { mpssymnum =   29, pcxsymnum =    30, icon = "Navaid, Red/Green" },
           new IconMapping() { mpssymnum =   30, pcxsymnum =    31, icon = "Navaid, Red/White" },
           new IconMapping() { mpssymnum =   31, pcxsymnum =    32, icon = "Navaid, Violet" },
           new IconMapping() { mpssymnum =   32, pcxsymnum =    33, icon = "Navaid, White" },
           new IconMapping() { mpssymnum =   33, pcxsymnum =    34, icon = "Navaid, White/Green" },
           new IconMapping() { mpssymnum =   34, pcxsymnum =    35, icon = "Navaid, White/Red" },
           new IconMapping() { mpssymnum =  102, pcxsymnum =  8242, icon = "Oil Field" },
           new IconMapping() { mpssymnum =  115, pcxsymnum = 16395, icon = "Parachute Area" },
           new IconMapping() { mpssymnum =   46, pcxsymnum =   159, icon = "Park" },
           new IconMapping() { mpssymnum =   45, pcxsymnum =   158, icon = "Parking Area" },
           new IconMapping() { mpssymnum =   81, pcxsymnum =  8212, icon = "Pharmacy" },
           new IconMapping() { mpssymnum =   47, pcxsymnum =   160, icon = "Picnic Area" },
           new IconMapping() { mpssymnum =   82, pcxsymnum =  8213, icon = "Pizza" },
           new IconMapping() { mpssymnum =   83, pcxsymnum =  8214, icon = "Post Office" },
           new IconMapping() { mpssymnum =  109, pcxsymnum = 16389, icon = "Private Field" },
           new IconMapping() { mpssymnum =   36, pcxsymnum =    37, icon = "Radio Beacon" },
           new IconMapping() { mpssymnum =    3, pcxsymnum =     3, icon = "Diamond, Red" },
           new IconMapping() { mpssymnum =   16, pcxsymnum =    16, icon = "Square, Red" },
           new IconMapping() { mpssymnum =   10, pcxsymnum =    10, icon = "Residence" },
           new IconMapping() { mpssymnum =   10, pcxsymnum =    10, icon = "House" },
           new IconMapping() { mpssymnum =   11, pcxsymnum =    11, icon = "Restaurant" },
           new IconMapping() { mpssymnum =   54, pcxsymnum =   167, icon = "Restricted Area" },
           new IconMapping() { mpssymnum =   39, pcxsymnum =   152, icon = "Restroom" },
           new IconMapping() { mpssymnum =   84, pcxsymnum =  8215, icon = "RV Park" },
           new IconMapping() { mpssymnum =   91, pcxsymnum =  8226, icon = "Scales" },
           new IconMapping() { mpssymnum =   48, pcxsymnum =   161, icon = "Scenic Area" },
           new IconMapping() { mpssymnum =   85, pcxsymnum =  8216, icon = "School" },
           new IconMapping() { mpssymnum =  116, pcxsymnum = 16402, icon = "Seaplane Base" },
           new IconMapping() { mpssymnum =   19, pcxsymnum =    19, icon = "Shipwreck" },
           new IconMapping() { mpssymnum =   58, pcxsymnum =   172, icon = "Shopping Center" },
           new IconMapping() { mpssymnum =  112, pcxsymnum = 16392, icon = "Short Tower" },
           new IconMapping() { mpssymnum =   40, pcxsymnum =   153, icon = "Shower" },
           new IconMapping() { mpssymnum =   49, pcxsymnum =   162, icon = "Skiing Area" },
           new IconMapping() { mpssymnum =   14, pcxsymnum =    14, icon = "Skull and Crossbones" },
           new IconMapping() { mpssymnum =  110, pcxsymnum = 16390, icon = "Soft Field" },
           new IconMapping() { mpssymnum =   86, pcxsymnum =  8217, icon = "Stadium" },
           new IconMapping() { mpssymnum =  106, pcxsymnum =  8246, icon = "Summit" },
           new IconMapping() { mpssymnum =   50, pcxsymnum =   163, icon = "Swimming Area" },
           new IconMapping() { mpssymnum =  111, pcxsymnum = 16391, icon = "Tall Tower" },
           new IconMapping() { mpssymnum =   42, pcxsymnum =   155, icon = "Telephone" },
           new IconMapping() { mpssymnum =   92, pcxsymnum =  8227, icon = "Toll Booth" },
           new IconMapping() { mpssymnum =   67, pcxsymnum =  8196, icon = "TracBack Point" },
           new IconMapping() { mpssymnum =   61, pcxsymnum =   175, icon = "Trail Head" },
           new IconMapping() { mpssymnum =   62, pcxsymnum =   176, icon = "Truck Stop" },
           new IconMapping() { mpssymnum =  103, pcxsymnum =  8243, icon = "Tunnel" },
           new IconMapping() { mpssymnum =  114, pcxsymnum = 16394, icon = "Ultralight Area" },
           new IconMapping() { mpssymnum =  139, pcxsymnum =  8282, icon = "Water Hydrant" },	/* new in MapSource V5 */
           new IconMapping() { mpssymnum =   18, pcxsymnum =    18, icon = "Waypoint" },
           new IconMapping() { mpssymnum =   17, pcxsymnum =    17, icon = "Buoy, White" },
           new IconMapping() { mpssymnum =   35, pcxsymnum =    36, icon = "Dot, White" },
           new IconMapping() { mpssymnum =   88, pcxsymnum =  8219, icon = "Zoo" },

           /* Custom icons.   The spec reserves 7680-8191 for the custom
            * icons on the C units, Quest, 27xx, 276, 296,  and other units.
            * Note that firmware problems on the earlier unit result in these
            * being mangled, so be sure you're on a version from at least
            * late 2005.
            * {    -2,  7680, "Custom 0" },
            * ....
            * {    -2,  8192, "Custom 511" },
            */
           /* MapSource V6.x */

           new IconMapping() {  mpssymnum =   140, pcxsymnum =  8286, icon = "Flag, Red" },
           new IconMapping() {  mpssymnum =   141, pcxsymnum =  8284, icon = "Flag, Blue" },
           new IconMapping() {  mpssymnum =   142, pcxsymnum =  8285, icon = "Flag, Green" },
           new IconMapping() {  mpssymnum =   143, pcxsymnum =  8289, icon = "Pin, Red" },
           new IconMapping() {  mpssymnum =   144, pcxsymnum =  8287, icon = "Pin, Blue" },
           new IconMapping() {  mpssymnum =   145, pcxsymnum =  8288, icon = "Pin, Green" },
           new IconMapping() {  mpssymnum =   146, pcxsymnum =  8292, icon = "Block, Red" },
           new IconMapping() {  mpssymnum =   147, pcxsymnum =  8290, icon = "Block, Blue" },
           new IconMapping() {  mpssymnum =   148, pcxsymnum =  8291, icon = "Block, Green" },
           new IconMapping() {  mpssymnum =   149, pcxsymnum =  8293, icon = "Bike Trail" },
           new IconMapping() {  mpssymnum =   150, pcxsymnum =   181, icon = "Fishing Hot Spot Facility" },
           new IconMapping() {  mpssymnum =   151, pcxsymnum =  8249, icon = "Police Station"},
           new IconMapping() {  mpssymnum =   152, pcxsymnum =  8251, icon = "Ski Resort" },
           new IconMapping() {  mpssymnum =   153, pcxsymnum =  8252, icon = "Ice Skating" },
           new IconMapping() {  mpssymnum =   154, pcxsymnum =  8253, icon = "Wrecker" },
           new IconMapping() {  mpssymnum =   155, pcxsymnum =   184, icon = "Anchor Prohibited" },
           new IconMapping() {  mpssymnum =   156, pcxsymnum =   185, icon = "Beacon" },
           new IconMapping() {  mpssymnum =   157, pcxsymnum =   186, icon = "Coast Guard" },
           new IconMapping() {  mpssymnum =   158, pcxsymnum =   187, icon = "Reef" },
           new IconMapping() {  mpssymnum =   159, pcxsymnum =   188, icon = "Weed Bed" },
           new IconMapping() {  mpssymnum =   160, pcxsymnum =   189, icon = "Dropoff" },
           new IconMapping() {  mpssymnum =   161, pcxsymnum =   190, icon = "Dock" },
           new IconMapping() {  mpssymnum =   162, pcxsymnum =   191, icon = "Marina" },
           new IconMapping() {  mpssymnum =   163, pcxsymnum =   192, icon = "Bait and Tackle" },
           new IconMapping() {  mpssymnum =   164, pcxsymnum =   193, icon = "Stump" },

           /* New in Garmin protocol spec from June 2006.  Extracted from
            * spec and fed through some horrible awk to add ones we didn't
            * have before but normalized for consistency. */
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8359,  icon = "Asian Food" },
           new IconMapping() {  mpssymnum = 167,pcxsymnum =   8296, icon = "Circle, Blue" },
           new IconMapping() {  mpssymnum = 168,pcxsymnum =   8299, icon = "Diamond, Blue" },
           new IconMapping() {  mpssymnum = 178,pcxsymnum =   8317, icon = "Letter A, Blue" },
           new IconMapping() {  mpssymnum = 181,pcxsymnum =   8318, icon = "Letter B, Blue" },
           new IconMapping() {  mpssymnum = 184,pcxsymnum =   8319, icon = "Letter C, Blue" },
           new IconMapping() {  mpssymnum = 187,pcxsymnum =   8320, icon = "Letter D, Blue" },
           new IconMapping() {  mpssymnum = 190,pcxsymnum =   8341, icon = "Number 0, Blue" },
           new IconMapping() {  mpssymnum = 193,pcxsymnum =   8342, icon = "Number 1, Blue" },
           new IconMapping() {  mpssymnum = 196,pcxsymnum =   8343, icon = "Number 2, Blue" },
           new IconMapping() {  mpssymnum = 199,pcxsymnum =   8344, icon = "Number 3, Blue" },
           new IconMapping() {  mpssymnum = 202,pcxsymnum =   8345, icon = "Number 4, Blue" },
           new IconMapping() {  mpssymnum = 205,pcxsymnum =   8346, icon = "Number 5, Blue" },
           new IconMapping() {  mpssymnum = 208,pcxsymnum =   8347, icon = "Number 6, Blue" },
           new IconMapping() {  mpssymnum = 211,pcxsymnum =   8348, icon = "Number 7, Blue" },
           new IconMapping() {  mpssymnum = 214,pcxsymnum =   8349, icon = "Number 8, Blue" },
           new IconMapping() {  mpssymnum = 217,pcxsymnum =   8350, icon = "Number 9, Blue" },
           new IconMapping() {  mpssymnum = 171,pcxsymnum =   8302, icon = "Oval, Blue" },
           new IconMapping() {  mpssymnum = 174,pcxsymnum =   8305, icon = "Rectangle, Blue" },
           new IconMapping() {  mpssymnum = 175,pcxsymnum =   8308, icon = "Square, Blue" },
           new IconMapping() {  mpssymnum = 218,pcxsymnum =   8351, icon = "Triangle, Blue" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8254, icon = "Border Crossing (Port Of Entry)" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =   182, icon = "Bottom Conditions" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8360, icon = "Deli" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8228, icon = "Elevation point" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8229, icon = "Exit without services" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16398, icon = "First approach fix" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8250, icon = "Gambling/casino" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8232, icon = "Geographic place name, land" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8230, icon = "Geographic place name, Man-made" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8231, icon = "Geographic place name, water" },
           new IconMapping() {  mpssymnum = 166,pcxsymnum =   8295, icon = "Circle, Green" },
           new IconMapping() {  mpssymnum = 177,pcxsymnum =   8313, icon = "Letter A, Green" },
           new IconMapping() {  mpssymnum = 180,pcxsymnum =   8315, icon = "Letter B, Green" },
           new IconMapping() {  mpssymnum = 183,pcxsymnum =   8314, icon = "Letter C, Green" },
           new IconMapping() {  mpssymnum = 186,pcxsymnum =   8316, icon = "Letter D, Green" },
           new IconMapping() {  mpssymnum = 189,pcxsymnum =   8331, icon = "Number 0, Green" },
           new IconMapping() {  mpssymnum = 192,pcxsymnum =   8332, icon = "Number 1, Green" },
           new IconMapping() {  mpssymnum = 195,pcxsymnum =   8333, icon = "Number 2, Green" },
           new IconMapping() {  mpssymnum = 198,pcxsymnum =   8334, icon = "Number 3, Green" },
           new IconMapping() {  mpssymnum = 201,pcxsymnum =   8335, icon = "Number 4, Green" },
           new IconMapping() {  mpssymnum = 204,pcxsymnum =   8336, icon = "Number 5, Green" },
           new IconMapping() {  mpssymnum = 207,pcxsymnum =   8337, icon = "Number 6, Green" },
           new IconMapping() {  mpssymnum = 210,pcxsymnum =   8338, icon = "Number 7, Green" },
           new IconMapping() {  mpssymnum = 213,pcxsymnum =   8339, icon = "Number 8, Green" },
           new IconMapping() {  mpssymnum = 216,pcxsymnum =   8340, icon = "Number 9, Green" },
           new IconMapping() {  mpssymnum = 170,pcxsymnum =   8301, icon = "Oval, Green" },
           new IconMapping() {  mpssymnum = 173,pcxsymnum =   8304, icon = "Rectangle, Green" },
           new IconMapping() {  mpssymnum = 219,pcxsymnum =   8352, icon = "Triangle, Green" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16385, icon = "Intersection" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8201, icon = "Intl freeway hwy" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8202, icon = "Intl national hwy" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8361, icon = "Italian food" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8248, icon = "Large exit without services" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8247, icon = "Large Ramp intersection" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16399, icon = "Localizer Outer Marker" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16400, icon = "Missed approach point" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16386, icon = "Non-directional beacon" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =   168, icon = "Null" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =   180, icon = "Open 24 Hours" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8222, icon = "Ramp intersection" },
           new IconMapping() {  mpssymnum = 165,pcxsymnum =   8294, icon = "Circle, Red" },
           new IconMapping() {  mpssymnum = 176,pcxsymnum =   8309, icon = "Letter A, Red" },
           new IconMapping() {  mpssymnum = 179,pcxsymnum =   8310, icon = "Letter B, Red" },
           new IconMapping() {  mpssymnum = 182,pcxsymnum =   8311, icon = "Letter C, Red" },
           new IconMapping() {  mpssymnum = 185,pcxsymnum =   8312, icon = "Letter D, Red" },
           new IconMapping() {  mpssymnum = 188,pcxsymnum =   8321, icon = "Number 0, Red" },
           new IconMapping() {  mpssymnum = 191,pcxsymnum =   8322, icon = "Number 1, Red" },
           new IconMapping() {  mpssymnum = 194,pcxsymnum =   8323, icon = "Number 2, Red" },
           new IconMapping() {  mpssymnum = 197,pcxsymnum =   8324, icon = "Number 3, Red" },
           new IconMapping() {  mpssymnum = 200,pcxsymnum =   8325, icon = "Number 4, Red" },
           new IconMapping() {  mpssymnum = 203,pcxsymnum =   8326, icon = "Number 5, Red" },
           new IconMapping() {  mpssymnum = 206,pcxsymnum =   8327, icon = "Number 6, Red" },
           new IconMapping() {  mpssymnum = 209,pcxsymnum =   8328, icon = "Number 7, Red" },
           new IconMapping() {  mpssymnum = 212,pcxsymnum =   8329, icon = "Number 8, Red" },
           new IconMapping() {  mpssymnum = 215,pcxsymnum =   8330, icon = "Number 9, Red" },
           new IconMapping() {  mpssymnum = 169,pcxsymnum =   8300, icon = "Oval, Red" },
           new IconMapping() {  mpssymnum = 172,pcxsymnum =   8303, icon = "Rectangle, Red" },
           new IconMapping() {  mpssymnum = 220,pcxsymnum =   8353, icon = "Triangle, Red" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8362, icon = "Seafood" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8194, icon = "State Hwy" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8363, icon = "Steak" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8223, icon = "Street Intersection" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16401, icon = "TACAN" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =   183, icon = "Tide/Current Prediction Station" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =   191, icon = "U Marina" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =  8193, icon = "US hwy" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum =   193, icon = "U stump" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16387, icon = "VHF Omni-range" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16397, icon = "VOR-DME" },
           new IconMapping() {  mpssymnum = -1, pcxsymnum = 16396, icon = "VOR/TACAN" },

           /* This block new on 1/15 from the Mapsource 6.12 beta */
           new IconMapping() {  mpssymnum = 221, pcxsymnum = -1, icon = "Contact, Blonde" },
           new IconMapping() {  mpssymnum = 222, pcxsymnum = -1, icon = "Contact, Clown" },
           new IconMapping() {  mpssymnum = 223, pcxsymnum = -1, icon = "Contact, Glasses" },
           new IconMapping() {  mpssymnum = 224, pcxsymnum = -1, icon = "Contact, Panda" },
           new IconMapping() {  mpssymnum = 225, pcxsymnum = -1, icon = "Multi-Cache" },
           new IconMapping() {  mpssymnum = 226, pcxsymnum = -1, icon = "Letterbox Cache" },
           new IconMapping() {  mpssymnum = 227, pcxsymnum = -1, icon = "Puzzle Cache" },
           new IconMapping() {  mpssymnum = 228, pcxsymnum = -1, icon = "Library" },
           new IconMapping() {  mpssymnum = 229, pcxsymnum = -1, icon = "Ground Transportation" },
           new IconMapping() {  mpssymnum = 230, pcxsymnum = -1, icon = "City Hall" },
           new IconMapping() {  mpssymnum = 231, pcxsymnum = -1, icon = "Winery" },
           new IconMapping() {  mpssymnum = 232, pcxsymnum = -1, icon = "ATV" },
           new IconMapping() {  mpssymnum = 233, pcxsymnum = -1, icon = "Big Game" },
           new IconMapping() {  mpssymnum = 234, pcxsymnum = -1, icon = "Blind" },
           new IconMapping() {  mpssymnum = 235, pcxsymnum = -1, icon = "Blood Trail" },
           new IconMapping() {  mpssymnum = 236, pcxsymnum = -1, icon = "Cover" },
           new IconMapping() {  mpssymnum = 237, pcxsymnum = -1, icon = "Covey" },
           new IconMapping() {  mpssymnum = 238, pcxsymnum = -1, icon = "Food Source" },
           new IconMapping() {  mpssymnum = 239, pcxsymnum = -1, icon = "Furbearer" },
           new IconMapping() {  mpssymnum = 240, pcxsymnum = -1, icon = "Lodge" },
           new IconMapping() {  mpssymnum = 241, pcxsymnum = -1, icon = "Small Game" },
           new IconMapping() {  mpssymnum = 242, pcxsymnum = -1, icon = "Animal Tracks" },
           new IconMapping() {  mpssymnum = 243, pcxsymnum = -1, icon = "Treed Quarry" },
           new IconMapping() {  mpssymnum = 244, pcxsymnum = -1, icon = "Tree Stand" },
           new IconMapping() {  mpssymnum = 245, pcxsymnum = -1, icon = "Truck" },
           new IconMapping() {  mpssymnum = 246, pcxsymnum = -1, icon = "Upland Game" },
           new IconMapping() {  mpssymnum = 247, pcxsymnum = -1, icon = "Waterfowl" },
           new IconMapping() {  mpssymnum = 248, pcxsymnum = -1, icon = "Water Source" },
      };


      /// <summary>
      /// liefert den Iconnamen zur Symbolnummer oder null
      /// </summary>
      /// <param name="symnumber"></param>
      /// <returns></returns>
      public static string? GetIconName4Symbolnumber(int symnumber) {
         foreach (var item in garmin_icon_table) {
            if (item.mpssymnum == symnumber)
               return item.icon;
         }
         return null;
      }

   }
}
