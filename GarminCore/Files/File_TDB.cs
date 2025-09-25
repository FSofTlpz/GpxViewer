/*
Copyright (C) 2015 Frank Stinner

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 3 of the License, or (at your 
option) any later version. 

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General 
Public License for more details. 

You should have received a copy of the GNU General Public License along 
with this program; if not, see <http://www.gnu.org/licenses/>. 


Dieses Programm ist freie Software. Sie können es unter den Bedingungen 
der GNU General Public License, wie von der Free Software Foundation 
veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß 
Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version. 

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es 
Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne 
die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN 
BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License. 

Sie sollten ein Exemplar der GNU General Public License zusammen mit 
diesem Programm erhalten haben. Falls nicht, siehe 
<http://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GarminCore.Files {

   /// <summary>
   /// nur für Mapsource/Basemap nötig;
   /// enthält eine Beschreibung, ein Copyright, Verweise auf die Teilkarten und einen Verweis auf die Overview-Karte
   /// </summary>
   public class File_TDB {


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
         public Typ ID;
         /// <summary>
         /// Länge des nachfolgenden Blocks
         /// </summary>
         public UInt16 Length;


         public BlockHeader(Typ typ = Typ.Unknown, UInt16 length = 0) {
            ID = typ;
            Length = length;
         }

         public BlockHeader(BlockHeader header) {
            ID = header.ID;
            Length = header.Length;
         }

         public void Read(BinaryReaderWriter br) {
            byte b = 0;
            try {
               b = br.ReadByte();
               ID = (Typ)b;
            } catch {
               throw new Exception("Unbekannter Block-Typ: 0x" + b.ToString("X"));
            }

            Length = br.Read2AsUShort();
         }

         public void Write(BinaryReaderWriter wr) {
            wr.Write((byte)ID);
            wr.Write(Length);
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
         public UInt16 ProductID;
         /// <summary>
         /// ID der Karte
         /// </summary>
         public UInt16 FamilyID;
         public UInt16 TDBVersion;
         /// <summary>
         /// Name für eine ganze Serie von Karten
         /// </summary>
         public string MapSeriesName;
         public UInt16 ProductVersion;
         /// <summary>
         /// Name der Karte innerhalb der Kartenserie
         /// </summary>
         public string MapFamilyName;

         // 41 weitere Byte, z. T. unbekannte Daten für Headertyp 4.07

         public byte Unknown1;            // locked ?
         /// <summary>
         /// bei einem Maßstab mit dieser Bitanzahl oder wenigers Bits wird die Overviewmap als oberster Layer angezeigt
         /// </summary>
         public byte MaxCoordbits4Overview;
         public byte Unknown2;
         public byte Unknown3;
         public byte Unknown4;
         public byte Unknown5;
         public byte Unknown6;
         public byte Unknown7;
         public byte Unknown8;
         public byte Unknown9;
         public byte HighestRoutable;
         public byte Unknown10;
         public byte Unknown11;
         public byte Unknown12;
         public byte Unknown13;
         public byte Unknown14;
         public byte Unknown15;
         public byte Unknown16;
         public byte Unknown17;
         public byte Unknown18;
         public byte Unknown19;
         public byte Unknown20;
         public byte Unknown21;
         public byte Unknown22;
         public byte Unknown23;
         public byte Unknown24;
         public byte Unknown25;
         public byte Unknown26;
         public byte Unknown27;
         public byte Unknown28;
         public UInt32 CodePage;
         public UInt32 Unknown29;
         public byte Routable;
         /// <summary>
         /// Sets a flag in tdb file which marks set mapset as having contour lines and allows showing profile in MapSource. Default is 0 which means disabled. 
         /// </summary>
         public byte HasProfileInformation;
         public byte HasDEM;

         // 20 weitere Byte für Headertyp 4.11

         public byte Unknown30;
         public byte Unknown31;
         public byte Unknown32;
         public byte Unknown33;
         public byte Unknown34;
         public byte Unknown35;
         public byte Unknown36;
         public byte Unknown37;
         public byte Unknown38;
         public byte Unknown39;
         public byte Unknown40;
         public byte Unknown41;
         public byte Unknown42;
         public byte Unknown43;
         public byte Unknown44;
         public byte Unknown45;
         public byte Unknown46;
         public byte Unknown47;
         public byte Unknown48;
         public byte Unknown49;

         protected BlockHeader blh;


         public Header(BlockHeader blh) {
            this.blh = new BlockHeader(blh);

            ProductID = 1;
            FamilyID = 0;
            TDBVersion = 407;
            MapSeriesName = "";
            ProductVersion = 0x100;
            MapFamilyName = "";

            Unknown1 = 0;
            MaxCoordbits4Overview = 0x12;
            Unknown2 = 1;
            Unknown3 = 1;
            Unknown4 = 1;
            Unknown5 = 0;
            Unknown6 = 0;
            Unknown7 = 0;
            Unknown8 = 0;
            Unknown9 = 0;
            HighestRoutable = 0x18;
            Unknown10 = 0;
            Unknown11 = 0;
            Unknown12 = 0;
            Unknown13 = 0;
            Unknown14 = 0;
            Unknown15 = 0;
            Unknown16 = 0;
            Unknown17 = 0;
            Unknown18 = 0;
            Unknown19 = 0;
            Unknown20 = 0;
            Unknown21 = 0;
            Unknown22 = 0;
            Unknown23 = 0;
            Unknown24 = 0;
            Unknown25 = 0;
            Unknown26 = 0;
            Unknown27 = 0;
            Unknown28 = 0;
            CodePage = 1252;
            Unknown29 = 10000;
            Routable = 1;
            HasProfileInformation = 1;
            HasDEM = 0;

            Unknown30 = 0;
            Unknown31 = 0;
            Unknown32 = 0;
            Unknown33 = 0;
            Unknown34 = 0;
            Unknown35 = 0;
            Unknown36 = 0;
            Unknown37 = 0;
            Unknown38 = 0;
            Unknown39 = 0;
            Unknown30 = 0;
            Unknown31 = 0;
            Unknown32 = 0;
            Unknown33 = 0;
            Unknown34 = 0;
            Unknown35 = 0;
            Unknown36 = 0;
            Unknown37 = 0;
            Unknown38 = 0;
            Unknown39 = 0;
         }

         /// <summary>
         /// liest die Blockdaten (Segmente) ein
         /// </summary>
         /// <param name="br"></param>
         public void ReadData(BinaryReaderWriter br) {
            ProductID = br.Read2AsUShort();
            FamilyID = br.Read2AsUShort();
            TDBVersion = br.Read2AsUShort();
            MapSeriesName = br.ReadString();
            ProductVersion = br.Read2AsUShort();
            MapFamilyName = br.ReadString();

            if (TDBVersion >= 407) {
               Unknown1 = br.ReadByte();
               MaxCoordbits4Overview = br.ReadByte();
               Unknown2 = br.ReadByte();
               Unknown3 = br.ReadByte();
               Unknown4 = br.ReadByte();
               Unknown5 = br.ReadByte();
               Unknown6 = br.ReadByte();
               Unknown7 = br.ReadByte();
               Unknown8 = br.ReadByte();
               Unknown9 = br.ReadByte();
               HighestRoutable = br.ReadByte();
               Unknown10 = br.ReadByte();
               Unknown11 = br.ReadByte();
               Unknown12 = br.ReadByte();
               Unknown13 = br.ReadByte();
               Unknown14 = br.ReadByte();
               Unknown15 = br.ReadByte();
               Unknown16 = br.ReadByte();
               Unknown17 = br.ReadByte();
               Unknown18 = br.ReadByte();
               Unknown19 = br.ReadByte();
               Unknown20 = br.ReadByte();
               Unknown21 = br.ReadByte();
               Unknown22 = br.ReadByte();
               Unknown23 = br.ReadByte();
               Unknown24 = br.ReadByte();
               Unknown25 = br.ReadByte();
               Unknown26 = br.ReadByte();
               Unknown27 = br.ReadByte();
               Unknown28 = br.ReadByte();
               CodePage = br.Read4UInt();
               Unknown29 = br.Read4UInt();
               Routable = br.ReadByte();
               HasProfileInformation = br.ReadByte();
               HasDEM = br.ReadByte();

               if (TDBVersion >= 411) {
                  Unknown30 = br.ReadByte();
                  Unknown31 = br.ReadByte();
                  Unknown32 = br.ReadByte();
                  Unknown33 = br.ReadByte();
                  Unknown34 = br.ReadByte();
                  Unknown35 = br.ReadByte();
                  Unknown36 = br.ReadByte();
                  Unknown37 = br.ReadByte();
                  Unknown38 = br.ReadByte();
                  Unknown39 = br.ReadByte();
                  Unknown40 = br.ReadByte();
                  Unknown41 = br.ReadByte();
                  Unknown42 = br.ReadByte();
                  Unknown43 = br.ReadByte();
                  Unknown44 = br.ReadByte();
                  Unknown45 = br.ReadByte();
                  Unknown46 = br.ReadByte();
                  Unknown47 = br.ReadByte();
                  Unknown48 = br.ReadByte();
                  Unknown49 = br.ReadByte();
               }

            }
         }

         /// <summary>
         /// schreibt den Blockheader und die Blockdaten
         /// </summary>
         /// <param name="wr"></param>
         public void Write(BinaryReaderWriter wr) {
            if (TDBVersion == 411)
               blh.Length = (UInt16)(0x4d + MapSeriesName.Length + MapFamilyName.Length);
            else if (TDBVersion == 407)
               blh.Length = (UInt16)(0x33 + MapSeriesName.Length + MapFamilyName.Length);
            else {
               TDBVersion = 300;
               blh.Length = (UInt16)(0xa + MapSeriesName.Length + MapFamilyName.Length);
            }

            blh.Write(wr);
            wr.Write(ProductID);
            wr.Write(FamilyID);
            wr.Write(TDBVersion);
            wr.WriteString(MapSeriesName);
            wr.Write(ProductVersion);
            wr.WriteString(MapFamilyName);

            if (TDBVersion >= 407) {
               wr.Write(Unknown1);
               wr.Write(MaxCoordbits4Overview);
               wr.Write(Unknown2);
               wr.Write(Unknown3);
               wr.Write(Unknown4);
               wr.Write(Unknown5);
               wr.Write(Unknown6);
               wr.Write(Unknown7);
               wr.Write(Unknown8);
               wr.Write(Unknown9);
               wr.Write(HighestRoutable);
               wr.Write(Unknown10);
               wr.Write(Unknown11);
               wr.Write(Unknown12);
               wr.Write(Unknown13);
               wr.Write(Unknown14);
               wr.Write(Unknown15);
               wr.Write(Unknown16);
               wr.Write(Unknown17);
               wr.Write(Unknown18);
               wr.Write(Unknown19);
               wr.Write(Unknown20);
               wr.Write(Unknown21);
               wr.Write(Unknown22);
               wr.Write(Unknown23);
               wr.Write(Unknown24);
               wr.Write(Unknown25);
               wr.Write(Unknown26);
               wr.Write(Unknown27);
               wr.Write(Unknown28);
               wr.Write(CodePage);
               wr.Write(Unknown29);
               wr.Write(Routable);
               wr.Write(HasProfileInformation);
               wr.Write(HasDEM);

               if (TDBVersion >= 411) {
                  wr.Write(Unknown30);
                  wr.Write(Unknown31);
                  wr.Write(Unknown32);
                  wr.Write(Unknown33);
                  wr.Write(Unknown34);
                  wr.Write(Unknown35);
                  wr.Write(Unknown36);
                  wr.Write(Unknown37);
                  wr.Write(Unknown38);
                  wr.Write(Unknown39);
                  wr.Write(Unknown40);
                  wr.Write(Unknown41);
                  wr.Write(Unknown42);
                  wr.Write(Unknown43);
                  wr.Write(Unknown44);
                  wr.Write(Unknown45);
                  wr.Write(Unknown46);
                  wr.Write(Unknown47);
                  wr.Write(Unknown48);
                  wr.Write(Unknown49);
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
         public List<Segment> Segments;

         protected BlockHeader blh;


         /// <summary>
         /// ein einzelnes Copyright-Segment
         /// </summary>
         public class Segment {

            /// <summary>
            /// Art der Daten (siehe CODE_...-Konstanten)
            /// </summary>
            public CopyrightCodes CopyrightCode;
            /// <summary>
            /// Wo sollen die Daten angezeigt werden?
            /// </summary>
            public WhereCodes WhereCode;
            /// <summary>
            /// zusätzliche Daten (i.A. 0, nur bei BMP ein Skalierungsfaktor)
            /// </summary>
            public UInt16 ExtraProperties;
            /// <summary>
            /// Text
            /// </summary>
            public string Copyright;

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

            public void Read(BinaryReaderWriter br) {
               byte b = br.ReadByte();
               CopyrightCode = CopyrightCodes.Unknown;
               try {
                  CopyrightCode = (CopyrightCodes)b;
               } catch { }

               b = br.ReadByte();
               WhereCode = WhereCodes.Unknown;
               try {
                  WhereCode = (WhereCodes)b;
               } catch { }

               ExtraProperties = br.Read2AsUShort();

               Copyright = br.ReadString();
            }

            public void Write(BinaryReaderWriter wr) {
               wr.Write((byte)CopyrightCode);
               wr.Write((byte)WhereCode);
               wr.Write(ExtraProperties);
               wr.WriteString(Copyright);
            }

            /// <summary>
            /// akt. Länge des Datenblocks
            /// </summary>
            /// <returns></returns>
            public UInt16 Length() {
               return (UInt16)(1 + 1 + 2 + Copyright.Length + 1);
            }

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
         /// <param name="br"></param>
         public void ReadData(BinaryReaderWriter br) {
            UInt16 len = 0;

            Segments.Clear();
            while (len < blh.Length) {
               Segment seg = new Segment();
               seg.Read(br);
               len += seg.Length();
               Segments.Add(seg);
            }
         }

         /// <summary>
         /// schreibt den Blockheader und die Blockdaten
         /// </summary>
         /// <param name="wr"></param>
         public void Write(BinaryReaderWriter wr) {
            blh.Length = 0;
            for (int i = 0; i < Segments.Count; i++)
               blh.Length += Segments[i].Length();
            blh.Write(wr);

            for (int i = 0; i < Segments.Count; i++)
               Segments[i].Write(wr);
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
         public UInt32 Mapnumber;
         /// <summary>
         /// Overview maps do not have parent maps, so the parent map is generally set to 0x00000000. (Muss NICHT dem Dateinamen entsprechen!)
         /// <para>"TOPO Deutschland v3.gmap" verweist auf 0x8AC2E630, verwendet aber .\TPDEUEC3\BASEMAP.xxx; der Verweis auf .\TPDEUEC3 erfolgt wohl nur über die Registry und/oder die Datei info.xml</para>
         /// <para>MKGMAP verweist z.B. auf 0x042D07E0=70060000, verwendet aber .\osmmap.img mit den Dateien 70060000.xxx; der Verweis auf .\osmmap.img erfolgt nur über die Registry</para>
         /// </summary>
         public UInt32 ParentMapnumber;

         /// <summary>
         /// nördliche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         public Int32 north { get; private set; }
         /// <summary>
         /// nördliche Begrenzung
         /// </summary>
         public double North {
            get {
               return north * DEGREE_FACTOR;
            }
            set {
               north = (int)(Math.Round(value / DEGREE_FACTOR));
            }
         }

         /// <summary>
         /// östliche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         public Int32 east { get; private set; }
         /// <summary>
         /// östliche Begrenzung
         /// </summary>
         public double East {
            get {
               return east * DEGREE_FACTOR;
            }
            set {
               east = (int)(Math.Round(value / DEGREE_FACTOR));
            }
         }

         /// <summary>
         /// südiche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         public Int32 south { get; private set; }
         /// <summary>
         /// südiche Begrenzung
         /// </summary>
         public double South {
            get {
               return south * DEGREE_FACTOR;
            }
            set {
               south = (int)(Math.Round(value / DEGREE_FACTOR));
            }
         }

         /// <summary>
         /// westliche Begrenzung (vermutlich in 2^32/360° = 4294967296/360)
         /// </summary>
         public Int32 west { get; private set; }
         /// <summary>
         /// westliche Begrenzung
         /// </summary>
         public double West {
            get {
               return west * DEGREE_FACTOR;
            }
            set {
               west = (int)(Math.Round(value / DEGREE_FACTOR));
            }
         }

         /// <summary>
         /// Beschreibungstext
         /// </summary>
         public string Description;

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
         /// <param name="br"></param>
         public void ReadData(BinaryReaderWriter br) {
            Mapnumber = br.Read4UInt();
            ParentMapnumber = br.Read4UInt();
            north = br.Read4Int();
            east = br.Read4Int();
            south = br.Read4Int();
            west = br.Read4Int();
            Description = br.ReadString();
         }

         /// <summary>
         /// schreibt den Blockheader und die Blockdaten
         /// </summary>
         /// <param name="wr"></param>
         public void Write(BinaryReaderWriter wr) {
            if (blh.Length == 0)
               blh.Length = (ushort)(4 + // Mapnumber
                                     4 + // ParentMapnumber
                                     4 + 4 + 4 + 4 + // 4x Koordinaten
                                     Description.Length + 1 // Description + 0-Byte
                  );
            blh.Write(wr);
            wr.Write(Mapnumber);
            wr.Write(ParentMapnumber);
            wr.Write(north);
            wr.Write(east);
            wr.Write(south);
            wr.Write(west);
            wr.WriteString(Description);
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

         /// <summary>
         /// Sollte immer 1 größer als <see cref="SubCount"/> sein  (?)
         /// </summary>
         public UInt16 Unknown1;
         /// <summary>
         /// Anzahl der Subfiles (daraus ergibt sich die Arraygröße für die Dateigrößen)
         /// </summary>
         public UInt16 SubCount;
         /// <summary>
         /// Liste der Dateigrößen in Byte (zu den jeweiligen Dateinamen)
         /// </summary>
         public List<UInt32> DataSize;
         public byte HasCopyright;
         public byte Unknown2;
         public byte Unknown3;
         public byte Unknown4;
         public byte Unknown5;
         public byte Unknown6;
         public byte Unknown7;
         /// <summary>
         /// Liste der zugehörigen Dateinamen
         /// </summary>
         public List<string> Name;
         public byte Unknown8;
         public byte Unknown9;

         public TileMap(BlockHeader blh)
            : base(blh) {
            Unknown1 = 0;
            SubCount = 0;
            HasCopyright = 0x01;
            Unknown2 = 0xc3;
            Unknown3 = 0x00;
            Unknown4 = 0xff;
            Unknown5 =
            Unknown6 =
            Unknown7 =
            Unknown8 =
            Unknown9 = 0;

            DataSize = new List<uint>();
            Name = new List<string>();
         }

         /// <summary>
         /// liest die Blockdaten ein
         /// </summary>
         /// <param name="br"></param>
         public new void ReadData(BinaryReaderWriter br) {
            base.ReadData(br);

            int readlen = 6 * 4 + Description.Length + 1;   // in der Basisklasse gelesen

            Unknown1 = br.Read2AsUShort();
            SubCount = br.Read2AsUShort();

            readlen += 4;

            DataSize.Clear();
            for (int i = 0; i < SubCount; i++) {
               DataSize.Add(br.Read4UInt());
               readlen += 4;
            }

            if (blh.Length - readlen < 7)
               return;

            HasCopyright = br.ReadByte();
            Unknown2 = br.ReadByte();
            Unknown3 = br.ReadByte();
            Unknown4 = br.ReadByte();
            Unknown5 = br.ReadByte();
            Unknown6 = br.ReadByte();
            Unknown7 = br.ReadByte();

            readlen += 7;

            Name.Clear();
            for (int i = 0; i < SubCount; i++) {
               Name.Add(br.ReadString());
               readlen += Name[Name.Count - 1].Length + 1;
            }

            if (blh.Length - readlen < 2)
               return;

            Unknown8 = br.ReadByte();
            Unknown9 = br.ReadByte();
         }

         /// <summary>
         /// schreibt den Blockheader und die Blockdaten
         /// </summary>
         /// <param name="wr"></param>
         public new void Write(BinaryReaderWriter wr) {
            BinaryReaderWriter wrtmp = new BinaryReaderWriter();

            base.Write(wrtmp);

            SubCount = (UInt16)DataSize.Count;
            Unknown1 = (UInt16)(SubCount + 1);     // das macht MKGMAP (ohne Erklärung) so 

            wrtmp.Write(Unknown1);
            wrtmp.Write(SubCount);

            for (int i = 0; i < DataSize.Count; i++)
               wrtmp.Write(DataSize[i]);

            wrtmp.Write(HasCopyright);
            wrtmp.Write(Unknown2);
            wrtmp.Write(Unknown3);
            wrtmp.Write(Unknown4);
            wrtmp.Write(Unknown5);
            wrtmp.Write(Unknown6);
            wrtmp.Write(Unknown7);

            for (int i = 0; i < Name.Count; i++)
               wrtmp.WriteString(Name[i]);

            wrtmp.Write(Unknown8);
            wrtmp.Write(Unknown9);

            wrtmp.Seek(1);
            wrtmp.Write((short)(wrtmp.Length - 3)); // Länge des Datenbereiches schreiben
            wrtmp.Seek(0);

            wr.Write(wrtmp.ToArray());

            wrtmp.Dispose();

         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString());
            for (int i = 0; i < Name.Count && i < DataSize.Count; i++)
               sb.Append(", " + Name[i] + " (" + DataSize[i].ToString() + " Bytes)");
            return sb.ToString();
         }

      }

      public class Description {

         public byte Unknown1;

         public string Text;

         protected BlockHeader blh;


         public Description(BlockHeader blh) {
            this.blh = new BlockHeader(blh);
            Unknown1 = 0xc3;
            Text = "";
         }

         /// <summary>
         /// liest die Blockdaten ein
         /// </summary>
         /// <param name="br"></param>
         public void ReadData(BinaryReaderWriter br) {
            Unknown1 = br.ReadByte();
            Text = br.ReadString();
         }

         /// <summary>
         /// schreibt den Blockheader und die Blockdaten
         /// </summary>
         /// <param name="wr"></param>
         public void Write(BinaryReaderWriter wr) {
            blh.Length = (ushort)(1 + // Unknown1
                                  Text.Length +
                                  1); // 0-Byte
            blh.Write(wr);
            wr.Write(Unknown1);
            wr.WriteString(Text);
         }

         public override string ToString() {
            return Text;
         }

      }

      public class PseudoCRC {

         // A,B,C,D is a standard crc32 sum of the rest of the file.
         // (Andrzej Popowsk)

         public UInt32 crc {
            get {
               return (UInt32)(A << 24) +
                      (UInt32)(B << 16) +
                      (UInt32)(C << 8) +
                      (UInt32)D;
            }
            set {
               A = (byte)((value >> 24) & 0xff);
               B = (byte)((value >> 16) & 0xff);
               C = (byte)((value >> 8) & 0xff);
               D = (byte)(value & 0xff);
            }
         }

         public UInt16 Unknown1;
         public byte A { get; private set; }
         public UInt32 Unknown2;
         public UInt16 Unknown3;
         public byte B { get; private set; }
         public UInt16 Unknown4;
         public byte C { get; private set; }
         public UInt32 Unknown5;
         public byte D { get; private set; }
         public UInt16 Unknown6;

         protected BlockHeader blh;


         public PseudoCRC(BlockHeader blh) {
            this.blh = new BlockHeader(blh);
            Unknown1 = Unknown3 = Unknown4 = Unknown6 = 0;
            Unknown2 = Unknown5 = 0;
            A = B = C = D = 0;
         }

         /// <summary>
         /// liest die Blockdaten ein
         /// </summary>
         /// <param name="br"></param>
         public void ReadData(BinaryReaderWriter br) {
            Unknown1 = br.Read2AsUShort();
            A = br.ReadByte();
            Unknown2 = br.Read4UInt();
            Unknown3 = br.Read2AsUShort();
            B = br.ReadByte();
            Unknown4 = br.Read2AsUShort();
            C = br.ReadByte();
            Unknown5 = br.Read4UInt();
            D = br.ReadByte();
            Unknown6 = br.Read2AsUShort();
         }

         /// <summary>
         /// schreibt den Blockheader und die Blockdaten
         /// </summary>
         /// <param name="wr"></param>
         public void Write(BinaryReaderWriter wr) {
            blh.Length = 20;
            blh.Write(wr);
            wr.Write(Unknown1);
            wr.Write(A);
            wr.Write(Unknown2);
            wr.Write(Unknown3);
            wr.Write(B);
            wr.Write(Unknown4);
            wr.Write(C);
            wr.Write(Unknown5);
            wr.Write(D);
            wr.Write(Unknown6);
         }

         public byte[] GetBytes() {
            byte[] b = new byte[20];
            b[0] = (byte)(Unknown1 & 0xFF);
            b[1] = (byte)((Unknown1 >> 8) & 0xFF);
            b[2] = A;
            b[3] = (byte)(Unknown2 & 0xFF);
            b[4] = (byte)((Unknown2 >> 8) & 0xFF);
            b[5] = (byte)((Unknown2 >> 16) & 0xFF);
            b[6] = (byte)((Unknown2 >> 24) & 0xFF);
            b[7] = (byte)(Unknown3 & 0xFF);
            b[8] = (byte)((Unknown3 >> 8) & 0xFF);
            b[9] = B;
            b[10] = (byte)(Unknown4 & 0xFF);
            b[11] = (byte)((Unknown4 >> 8) & 0xFF);
            b[12] = C;
            b[13] = (byte)(Unknown5 & 0xFF);
            b[14] = (byte)((Unknown5 >> 8) & 0xFF);
            b[15] = (byte)((Unknown5 >> 16) & 0xFF);
            b[16] = (byte)((Unknown5 >> 24) & 0xFF);
            b[17] = D;
            b[18] = (byte)(Unknown6 & 0xFF);
            b[19] = (byte)((Unknown6 >> 8) & 0xFF);
            return b;
         }

         public override string ToString() {
            return string.Format("A={0}, B={1}, C={2}, D={3}, crc={4}", A, B, C, D, crc);
         }

      }


      public Header Head;
      public SegmentedCopyright Copyright;
      public Description Mapdescription;
      public OverviewMap Overviewmap;
      public List<TileMap> Tilemap;
      public PseudoCRC Crc { get; private set; }

      /// <summary>
      /// liefert nach dem Einlesen die Reihenfolge der Blöcke
      /// </summary>
      public List<BlockHeader.Typ> BlockHeaderTypList { get; private set; }
      /// <summary>
      /// liefert nach dem Einlesen die Länge der Blöcke
      /// </summary>
      public List<int> BlockLength { get; private set; }


      public File_TDB() {
         BlockHeaderTypList = new List<BlockHeader.Typ>();
         BlockLength = new List<int>();
         Head = new Header(new BlockHeader(BlockHeader.Typ.Header, 0));
         Copyright = new SegmentedCopyright(new BlockHeader(BlockHeader.Typ.Copyright, 0));
         Overviewmap = new OverviewMap(new BlockHeader(BlockHeader.Typ.Overviewmap, 0));
         Tilemap = new List<TileMap>();
         Crc = new PseudoCRC(new BlockHeader(BlockHeader.Typ.Crc, 0));
         Mapdescription = new Description(new BlockHeader(BlockHeader.Typ.Description, 0));
      }

      /// <summary>
      /// lese die Daten aus einer TDB-Datei ein
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         BlockHeader blh = new BlockHeader();
         Tilemap.Clear();
         BlockHeaderTypList.Clear();

         br.Seek(0);
         blh.Read(br);
         if (blh.ID != BlockHeader.Typ.Header)
            throw new Exception("Keine TDB-Datei.");
         Head = new Header(blh);
         Head.ReadData(br);
         BlockHeaderTypList.Add(blh.ID);
         BlockLength.Add(blh.Length);

         do {
            blh.Read(br);
            BlockHeaderTypList.Add(blh.ID);
            BlockLength.Add(blh.Length);
            switch (blh.ID) {
               case BlockHeader.Typ.Copyright:
                  Copyright = new SegmentedCopyright(new BlockHeader(blh));
                  Copyright.ReadData(br);
                  break;

               case BlockHeader.Typ.Overviewmap:
                  Overviewmap = new OverviewMap(new BlockHeader(blh));
                  Overviewmap.ReadData(br);
                  break;

               case BlockHeader.Typ.Tilemap:
                  TileMap dm = new TileMap(new BlockHeader(blh));
                  dm.ReadData(br);
                  Tilemap.Add(dm);
                  break;

               case BlockHeader.Typ.Description:
                  Mapdescription = new Description(new BlockHeader(blh));
                  Mapdescription.ReadData(br);
                  break;

               case BlockHeader.Typ.Crc:
                  Crc = new PseudoCRC(new BlockHeader(blh));
                  Crc.ReadData(br);
                  break;

               default:    // unbekannter Block
                  br.Position += blh.Length;
                  break;
            }
         } while (br.Position < br.Length);
      }

      /// <summary>
      /// schreibe die aktuellen Daten als TDB-Datei
      /// </summary>
      /// <param name="wr"></param>
      public void Write(BinaryReaderWriter wr) {
         // alle Daten zunächst nur in einen MemoryStream schreiben und danach mit dessen Daten die CRC berechnen
         MemoryStream mem = new MemoryStream();
         using (BinaryReaderWriter bw = new BinaryReaderWriter(mem)) {

            Head.Write(bw);

            Copyright.Write(bw);

            if (Head.TDBVersion >= 407)
               Mapdescription.Write(bw);

            Overviewmap.Write(bw);

            for (int i = 0; i < Tilemap.Count; i++)
               Tilemap[i].Write(bw);

            wr.Write(mem.ToArray()); // Ausgabe in den Ziel-Stream

            if (Head.TDBVersion >= 407) {
               // CRC32 berechnen
               CRC32 crc32 = new CRC32();
               mem.Seek(0, SeekOrigin.Begin);
               long pos = mem.Length;
               for (long i = 0; i < pos; i++)
                  crc32.update((byte)mem.ReadByte());
               Crc.crc = crc32.Value;
               Crc.Write(wr); // Ausgabe in den Ziel-Stream

            }
         }

      }


   }
}
