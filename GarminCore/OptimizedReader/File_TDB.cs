using System;
using System.Collections.Generic;
using System.Text;

namespace GarminCore.OptimizedReader {
   internal class File_TDB {
      /// <summary>
      /// zur Behandlung des Blockheaders (Typ und Datenlänge, 3 Byte)
      /// <para>Damit wird beschrieben, welche Daten folgen und wie groß dieser Datenbereich ist.</para>
      /// </summary>
      public class BlockHeader {

         /// <summary>
         /// bekannte ID-Typen
         /// </summary>
         public enum Typ : byte {
            Header = 0x50,
            Copyright = 0x44,
            Overviewmap = 0x42,
            Tilemap = 0x4c,
            Description = 0x52,
            Crc = 0x54,

            Unknown = 0xff,
         }

         /// <summary>
         /// Block-ID (0x50 für Head)
         /// </summary>
         public Typ ID { get; protected set; }

         /// <summary>
         /// Länge des nachfolgenden Blocks
         /// </summary>
         public UInt16 Length { get; protected set; }


         public BlockHeader(Typ typ = Typ.Unknown, UInt16 length = 0) {
            ID = typ;
            Length = length;
         }

         public BlockHeader(BlockHeader header) {
            ID = header.ID;
            Length = header.Length;
         }

         public void Read(BinaryReaderWriter reader) {
            byte b = 0;
            try {
               b = reader.ReadByte();
               ID = (Typ)b;
            } catch {
               throw new Exception("Unbekannter Block-Typ: 0x" + b.ToString("X"));
            }

            Length = reader.Read2AsUShort();
         }

         public override string ToString() {
            return string.Format("ID={0}, Length={1}", ID, Length);
         }

      }

      /// <summary>
      /// immer der 1. Datenblock
      /// </summary>
      public class Header {

         /// <summary>
         /// ID, i.A. 1
         /// </summary>
         public UInt16 ProductID { get; protected set; }
         /// <summary>
         /// ID der Karte
         /// </summary>
         public UInt16 FamilyID { get; protected set; }
         public UInt16 TDBVersion { get; protected set; }
         /// <summary>
         /// Name für eine ganze Serie von Karten
         /// </summary>
         public string MapSeriesName { get; protected set; }
         public UInt16 ProductVersion { get; protected set; }
         /// <summary>
         /// Name der Karte innerhalb der Kartenserie
         /// </summary>
         public string MapFamilyName { get; protected set; }

         // 41 weitere Byte, z. T. unbekannte Daten für Headertyp 4.07

         /// <summary>
         /// bei einem Maßstab mit dieser Bitanzahl oder wenigers Bits wird die Overviewmap als oberster Layer angezeigt
         /// </summary>
         public byte MaxCoordbits4Overview { get; protected set; }
         public byte HighestRoutable { get; protected set; }
         public UInt32 CodePage { get; protected set; }

         public byte Routable { get; protected set; }
         /// <summary>
         /// Sets a flag in tdb file which marks set mapset as having contour lines and allows showing profile in MapSource. Default is 0 which means disabled. 
         /// </summary>
         public byte HasProfileInformation { get; protected set; }
         public byte HasDEM { get; protected set; }

         protected BlockHeader blh;


         public Header(BlockHeader blh) {
            this.blh = new BlockHeader(blh);

            ProductID = 1;
            FamilyID = 0;
            TDBVersion = 407;
            MapSeriesName = "";
            ProductVersion = 0x100;
            MapFamilyName = "";

            MaxCoordbits4Overview = 0x12;
            HighestRoutable = 0x18;
            CodePage = 1252;
            Routable = 1;
            HasProfileInformation = 1;
            HasDEM = 0;
         }

         /// <summary>
         /// liest die Blockdaten (Segmente) ein
         /// </summary>
         /// <param name="reader"></param>
         public void ReadData(BinaryReaderWriter reader) {
            ProductID = reader.Read2AsUShort();
            FamilyID = reader.Read2AsUShort();
            TDBVersion = reader.Read2AsUShort();
            MapSeriesName = reader.ReadString();
            ProductVersion = reader.Read2AsUShort();
            MapFamilyName = reader.ReadString();

            if (TDBVersion >= 407) {
               reader.Position++;
               MaxCoordbits4Overview = reader.ReadByte();
               reader.Position += 8;
               HighestRoutable = reader.ReadByte();
               reader.Position += 19;
               CodePage = reader.Read4UInt();
               reader.Position += 4;
               Routable = reader.ReadByte();
               HasProfileInformation = reader.ReadByte();
               HasDEM = reader.ReadByte();

               if (TDBVersion >= 411) {
                  reader.Position += 20;
               }
            }
         }

         public override string ToString() {
            return string.Format("TDBVersion={0}, ProductID={1}, FamilyID={2}, MapSeriesName={3}, MapFamilyName={4}, CodePage={5}, LowestMapLevel={6}",
               TDBVersion, ProductID, FamilyID, MapSeriesName, MapFamilyName, CodePage, MaxCoordbits4Overview);
         }

      }

      /// <summary>
      /// Copyright-Texte
      /// <para>Der gesamte Text kann aus mehreren Segmenten bestehen.</para>
      /// </summary>
      public class SegmentedCopyright {

         /// <summary>
         /// Segmente
         /// </summary>
         public List<Segment> Segments { get; protected set; }

         protected BlockHeader blh;


         /// <summary>
         /// ein einzelnes Copyright-Segment
         /// </summary>
         public class Segment {

            /// <summary>
            /// Art der Daten (siehe CODE_...-Konstanten)
            /// </summary>
            public CopyrightCodes CopyrightCode { get; protected set; }
            /// <summary>
            /// Wo sollen die Daten angezeigt werden?
            /// </summary>
            public WhereCodes WhereCode { get; protected set; }
            /// <summary>
            /// zusätzliche Daten (i.A. 0, nur bei BMP ein Skalierungsfaktor)
            /// </summary>
            public UInt16 ExtraProperties { get; protected set; }
            /// <summary>
            /// Text
            /// </summary>
            public string Copyright { get; protected set; }

            public enum CopyrightCodes {
               /// <summary>
               /// Source information text string.  Describes what data sources were used in generating the map.
               /// </summary>
               SourceInformation = 0x00,
               /// <summary>
               /// Copyright information from the map manufacturer.
               /// </summary>
               CopyrightInformation = 0x06,
               /// <summary>
               /// A filename that contains a BMP image to be printed along with the map.
               /// </summary>
               Bmp = 0x07,

               Unknown = 0xffff,
            }

            public enum WhereCodes {
               /// <summary>
               /// The copyright text is printed in the “product information” screen in MapSource. This value has no meaning for bitmap images.
               /// </summary>
               ProductInformation = 0x01,
               /// <summary>
               /// The copyright text or bitmap image should be printed when a map is printed from MapSource.
               /// </summary>
               Printing = 0x02,
               /// <summary>
               /// The copyright text should be printed on both the “product information” screen and any printed maps.
               /// </summary>
               ProductInformationAndPrinting = 0x03,

               Unknown = 0xffff,
            }


            public Segment() {
               CopyrightCode = CopyrightCodes.CopyrightInformation;
               WhereCode = WhereCodes.ProductInformation;
               ExtraProperties = 0;
               Copyright = "";
            }

            public Segment(Segment seg) {
               CopyrightCode = seg.CopyrightCode;
               WhereCode = seg.WhereCode;
               ExtraProperties = seg.ExtraProperties;
               Copyright = seg.Copyright;
            }

            public Segment(CopyrightCodes cc, WhereCodes wc, string txt, UInt16 extra = 0) {
               CopyrightCode = cc;
               WhereCode = wc;
               ExtraProperties = extra;
               Copyright = txt;
            }

            public void Read(BinaryReaderWriter reader) {
               byte b = reader.ReadByte();
               CopyrightCode = CopyrightCodes.Unknown;
               try {
                  CopyrightCode = (CopyrightCodes)b;
               } catch { }

               b = reader.ReadByte();
               WhereCode = WhereCodes.Unknown;
               try {
                  WhereCode = (WhereCodes)b;
               } catch { }

               ExtraProperties = reader.Read2AsUShort();

               Copyright = reader.ReadString();
            }

            /// <summary>
            /// akt. Länge des Datenblocks
            /// </summary>
            /// <returns></returns>
            public UInt16 Length() => (ushort)(1 + 1 + 2 + Copyright.Length + 1);

            public override string ToString() {
               return string.Format("CopyrightCode={0}, WhereCode={1}, ExtraProperties={2}, Copyright={3}", CopyrightCode, WhereCode, ExtraProperties, Copyright);
            }

         }

         public SegmentedCopyright(BlockHeader blh) {
            this.blh = new BlockHeader(blh);
            Segments = new List<Segment>();
         }

         /// <summary>
         /// liest die Blockdaten (Segmente) ein
         /// </summary>
         /// <param name="reader"></param>
         public void ReadData(BinaryReaderWriter reader) {
            UInt16 len = 0;

            Segments.Clear();
            while (len < blh.Length) {
               Segment seg = new Segment();
               seg.Read(reader);
               len += seg.Length();
               Segments.Add(seg);
            }
         }
      }

      /// <summary>
      /// die Overviewkarte
      /// </summary>
      public class OverviewMap {

         // nach dem Blockheader 6*4=24 Byte + variabler Text

         // 360.0 / (1 << 32)
         const double DEGREE_FACTOR = 360.0 / 4294967296;


         /// <summary>
         /// eindeutige Kachelnummer (z.B. Name des Verzeichnisses der TRE, LBL usw. Dateien oder Name der IMG-Datei)
         /// <para>0x234F1D9 kann für das Verzeichnis 37024217 stehen</para>
         /// </summary>
         public UInt32 Mapnumber { get; private set; }
         /// <summary>
         /// Overview maps do not have parent maps, so the parent map is generally set to 0x00000000. (Muss NICHT dem Dateinamen entsprechen!)
         /// <para>"TOPO Deutschland v3.gmap" verweist auf 0x8AC2E630, verwendet aber .\TPDEUEC3\BASEMAP.xxx; der Verweis auf .\TPDEUEC3 erfolgt wohl nur über die Registry und/oder die Datei info.xml</para>
         /// <para>MKGMAP verweist z.B. auf 0x042D07E0=70060000, verwendet aber .\osmmap.img mit den Dateien 70060000.xxx; der Verweis auf .\osmmap.img erfolgt nur über die Registry</para>
         /// </summary>
         public UInt32 ParentMapnumber { get; private set; }

         /// <summary>
         /// nördliche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         protected Int32 north;
         /// <summary>
         /// nördliche Begrenzung
         /// </summary>
         public double North {
            get => north * DEGREE_FACTOR;
            protected set => north = (int)(Math.Round(value / DEGREE_FACTOR));
         }

         /// <summary>
         /// östliche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         protected Int32 east;
         /// <summary>
         /// östliche Begrenzung
         /// </summary>
         public double East {
            get => east * DEGREE_FACTOR;
            protected set => east = (int)(Math.Round(value / DEGREE_FACTOR));
         }

         /// <summary>
         /// südiche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         protected Int32 south;
         /// <summary>
         /// südiche Begrenzung
         /// </summary>
         public double South {
            get => south * DEGREE_FACTOR;
            protected set => south = (int)(Math.Round(value / DEGREE_FACTOR));
         }

         /// <summary>
         /// westliche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         protected Int32 west;
         /// <summary>
         /// westliche Begrenzung
         /// </summary>
         public double West {
            get => west * DEGREE_FACTOR;
            protected set => west = (int)(Math.Round(value / DEGREE_FACTOR));
         }

         /// <summary>
         /// Beschreibungstext
         /// </summary>
         public string Description { get; private set; }

         protected BlockHeader blh;


         public OverviewMap(BlockHeader blh) {
            this.blh = new BlockHeader(blh);
            Mapnumber = ParentMapnumber = 0;
            North = West = South = East = 0;
            Description = "";
         }

         /// <summary>
         /// liest die Blockdaten ein
         /// </summary>
         /// <param name="reader"></param>
         public void ReadData(BinaryReaderWriter reader) {
            Mapnumber = reader.Read4UInt();
            ParentMapnumber = reader.Read4UInt();
            north = reader.Read4Int();
            east = reader.Read4Int();
            south = reader.Read4Int();
            west = reader.Read4Int();
            Description = reader.ReadString();
         }

         public override string ToString() {
            return string.Format("Mapnumber=0x{0:x}, Lon={1}...{2}, Lat={3}...{4}, Description={5}",
               Mapnumber,
               West, East, South, North,
               Description);
         }

      }

      /// <summary>
      /// eine Kartenkachel
      /// </summary>
      public class TileMap : OverviewMap {

         // nach dem Blockheader 
         //       6*4=24 Byte + variabler Text           (wie Overview)
         //       + 4 Byte
         //       + x*4 Byte Länge + 
         //       + 7 Byte
         //       + x*Text
         //       + 2 x 0x00 ???

         UInt16 Unknown1;

         /// <summary>
         /// Anzahl der Subfiles (daraus ergibt sich die Arraygröße für die Dateigrößen)
         /// </summary>
         public UInt16 SubCount { get; protected set; }

         /// <summary>
         /// Liste der Dateigrößen in Byte (zu den jeweiligen Dateinamen)
         /// </summary>
         public List<UInt32> DataSize { get; protected set; }
 
         public byte HasCopyright { get; protected set; }

         /// <summary>
         /// Liste der zugehörigen Dateinamen
         /// </summary>
         public List<string> Name { get; protected set; }

         public TileMap(BlockHeader blh)
            : base(blh) {
            Unknown1 = 0;
            SubCount = 0;
            HasCopyright = 0x01;
            DataSize = new List<uint>();
            Name = new List<string>();
         }

         /// <summary>
         /// liest die Blockdaten ein
         /// </summary>
         /// <param name="reader"></param>
         public new void ReadData(BinaryReaderWriter reader) {
            base.ReadData(reader);

            int readlen = 6 * 4 + Description.Length + 1;   // in der Basisklasse gelesen

            Unknown1 = reader.Read2AsUShort();
            SubCount = reader.Read2AsUShort();

            readlen += 4;

            DataSize.Clear();
            for (int i = 0; i < SubCount; i++) {
               DataSize.Add(reader.Read4UInt());
               readlen += 4;
            }

            if (blh.Length - readlen < 7)
               return;

            HasCopyright = reader.ReadByte();
            reader.Position += 7;

            readlen += 7;

            Name.Clear();
            for (int i = 0; i < SubCount; i++) {
               Name.Add(reader.ReadString());
               readlen += Name[Name.Count - 1].Length + 1;
            }

            if (blh.Length - readlen < 2)
               return;

            reader.Position += 2;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString());
            for (int i = 0; i < Name.Count && i < DataSize.Count; i++)
               sb.Append(", " + Name[i] + " (" + DataSize[i].ToString() + " Bytes)");
            return sb.ToString();
         }

      }

      public class Description {

         public string Text { get; protected set; }

         protected BlockHeader blh;


         public Description(BlockHeader blh) {
            this.blh = new BlockHeader(blh);
            Text = "";
         }

         /// <summary>
         /// liest die Blockdaten ein
         /// </summary>
         /// <param name="reader"></param>
         public void ReadData(BinaryReaderWriter reader) {
            reader.Position++;
            Text = reader.ReadString();
         }

         public override string ToString() {
            return Text;
         }

      }


      public Header Head { get; protected set; }

      public SegmentedCopyright Copyright { get; protected set; }

      public Description Mapdescription { get; protected set; }

      public OverviewMap Overviewmap { get; protected set; }

      public List<TileMap> Tilemap { get; protected set; }

      /// <summary>
      /// liefert nach dem Einlesen die Reihenfolge der Blöcke
      /// </summary>
      List<BlockHeader.Typ> BlockHeaderTypList;

      /// <summary>
      /// liefert nach dem Einlesen die Länge der Blöcke
      /// </summary>
      List<int> BlockLength;


      public File_TDB() {
         BlockHeaderTypList = new List<BlockHeader.Typ>();
         BlockLength = new List<int>();
         Head = new Header(new BlockHeader(BlockHeader.Typ.Header, 0));
         Copyright = new SegmentedCopyright(new BlockHeader(BlockHeader.Typ.Copyright, 0));
         Overviewmap = new OverviewMap(new BlockHeader(BlockHeader.Typ.Overviewmap, 0));
         Tilemap = new List<TileMap>();
         Mapdescription = new Description(new BlockHeader(BlockHeader.Typ.Description, 0));
      }

      /// <summary>
      /// lese die Daten aus einer TDB-Datei ein
      /// </summary>
      /// <param name="reader"></param>
      public void Read(BinaryReaderWriter reader) {
         BlockHeader blkheader = new BlockHeader();
         Tilemap.Clear();
         BlockHeaderTypList.Clear();

         reader.Seek(0);
         blkheader.Read(reader);
         if (blkheader.ID != BlockHeader.Typ.Header)
            throw new Exception("Keine TDB-Datei.");
         Head = new Header(blkheader);
         Head.ReadData(reader);
         BlockHeaderTypList.Add(blkheader.ID);
         BlockLength.Add(blkheader.Length);

         do {
            blkheader.Read(reader);
            BlockHeaderTypList.Add(blkheader.ID);
            BlockLength.Add(blkheader.Length);
            switch (blkheader.ID) {
               case BlockHeader.Typ.Copyright:
                  Copyright = new SegmentedCopyright(new BlockHeader(blkheader));
                  Copyright.ReadData(reader);
                  break;

               case BlockHeader.Typ.Overviewmap:
                  Overviewmap = new OverviewMap(new BlockHeader(blkheader));
                  Overviewmap.ReadData(reader);
                  break;

               case BlockHeader.Typ.Tilemap:
                  TileMap dm = new TileMap(new BlockHeader(blkheader));
                  dm.ReadData(reader);
                  Tilemap.Add(dm);
                  break;

               case BlockHeader.Typ.Description:
                  Mapdescription = new Description(new BlockHeader(blkheader));
                  Mapdescription.ReadData(reader);
                  break;

               default:
                  reader.Position += blkheader.Length;
                  break;
            }
         } while (reader.Position < reader.Length);
      }

   }
}
