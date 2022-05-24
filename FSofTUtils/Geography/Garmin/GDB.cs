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
         public string Progname = "";

         public Version Version;

         /// <summary>
         /// z.B.: "Oct 14 2010", "08:33:45"
         /// </summary>
         public DateTime DateTime;


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
            Name = read_CString(reader);
         }

         /// <summary>
         /// nur zum "Überlesen" des noch unbekannten Datenbereiches (der <see cref="BinaryReader"/> muss direkt hinter dem Namen des Objektes stehen)
         /// </summary>
         /// <param name="reader"></param>
         /// <returns></returns>
         public byte[] ReadDummy(BinaryReader reader) {
            return reader.ReadBytes(ObjectHeader.Length - Name.Length - 1);
         }

         /// <summary>
         /// Text als UTF8 lesen
         /// </summary>
         /// <param name="reader"></param>
         /// <returns></returns>
         public static string read_CString(BinaryReader reader) {
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
         public static string read_CString(BinaryReader reader, Encoding encoding) {
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
            for (int i = 0; i < count; i++)
               lst.Add(read_CString(reader, encoding));
            return lst;
         }

         /// <summary>
         /// eine mit 0-Byte abgeschlossene Byte-Liste wird als Text interpretiert
         /// </summary>
         /// <param name="buffer"></param>
         /// <param name="start"></param>
         /// <returns></returns>
         static string read_CString(IList<byte> buffer, int start, Encoding encoding) {
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
            Data.Add(read_CString(reader, Encoding.ASCII));
            Data.Add(read_CString(reader, Encoding.ASCII));
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

            switch (version.GDBVersion) {
               case Version.VersionKey.V2:
                  Url.Add(read_CString(reader));
                  break;

               case Version.VersionKey.V3:
                  int urls = reader.ReadInt32();
                  for (int i = 0; i < urls; i++)
                     Url.Add(read_CString(reader));
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
         public string CountryCode;
         public byte[] Unknown22;
         public double Lat;
         public double Lon;
         public double Ele = double.MinValue;
         public string Description;
         public double Proximity = double.MinValue;
         public int DisplayMode = -1;
         public int ColorIdx = -1;
         public int IconIdx = -1;
         public string City;
         public string State;
         public string Facility;
         public byte Unknown1a;
         public double Depth = double.MinValue;
         public string Address;
         public byte Unknown1b;
         public int Duration = -1;
         public string Instruction;
         public List<string> Url;
         public int Category;
         public double Temperature = double.MinValue;
         public System.DateTime CreationTime;
         public List<string> Tel;
         public string Fax;
         public string Country;
         public string Zip;

         public byte[] UnknownV2_2;
         public byte UnknownV2_1;
         public byte[] UnknownV2_3;
         public string UnknownV2;

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
            Description = read_CString(reader);
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
               Url = new List<string>() { read_CString(reader) };

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

         public List<string> Url;

         public string Description;
         public int ColorIdx = -1;
         public byte Autoroute = 0;
         public byte[] Unknown6;
         public byte RouteStyle;
         public int CalcType;
         public byte VehicleType;
         public int RoadSelection;
         public double[] DrivingSpeed = new double[5];
         public byte[] Unknown8;


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
               Url = new List<string>() { read_CString(reader) };
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

         public string Name;
         public int WaypointClass;
         public string CountryCode;
         public byte[] Unknown22;
         public byte[] Unknown8a;
         public byte[] Unknown8b;
         public byte[] Unknown2;
         public byte[] Unknown18;

         public List<double> Lat = new List<double>();
         public List<double> Lon = new List<double>();
         public List<double> Ele = new List<double>();
         public Bounds Bounds;


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
         Object obj = null;
         do {
            obj = readobject(reader, version);
            lst.Add(obj);
         } while (obj.ObjectHeader.ObjectType != ObjectHeader.GDBObjectType.LAST);
         return lst;
      }

   }
}
