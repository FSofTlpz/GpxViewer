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
using System.Text;

namespace GarminCore.DskImg {

   /// <summary>
   /// zum Einlesen des IMG-Headers und Verwalten der wichtigen Daten
   /// </summary>
   public class Header {

      // siehe auch: mkgmap\src\uk\me\parabola\imgfmt\sys\ImgHeader.java

      /// <summary>
      /// offensichtlich fest definierte Größe des Header-Blocks
      /// </summary>
      public const int SECTOR_BLOCKSIZE = 512;
      public const string SIGNATURE = "DSKIMG";
      public const string MAPFILEIDENTIFIER = "GARMIN";
      public const UInt16 BOOTSIGNATURE = 0xaa55;

      /// <summary>
      /// 1, wenn die Daten XOR't sind
      /// </summary>
      public byte XOR;
      /// <summary>
      /// Monat des letzten Updates [1...12]
      /// </summary>
      public byte UpdateMonth;
      byte _UpdateYear;
      /// <summary>
      /// Jahr des letzten Updates 
      /// </summary>
      public int UpdateYear {
         get {
            // laut John Mechalas: Update year (+1900 for val >= 0x63, +2000 for val <= 0x62)
            // aber das ist eigentlich sinnlos
            //return _UpdateYear <= 98 ? _UpdateYear + 2000 : _UpdateYear + 1900;
            return _UpdateYear + 1900;
         }
         set {
            _UpdateYear = (byte)(value - 1900);
         }
      }
      /// <summary>
      /// 1, wenn von Mapsource oder Basecamp erzeugt
      /// </summary>
      public byte MapsourceFlag;
      /// <summary>
      /// scheint nicht ausgewertet zu werden (?); von Mapsource / Basecamp nicht gesetzt
      /// </summary>
      public byte Checksum;
      /// <summary>
      /// Datum der Kartenerzeugung
      /// </summary>
      public DateTime CreationDate;
      /// <summary>
      /// Kartenname Teil 1 (indirekt über <see cref="Description"/> setzen)
      /// </summary>
      public string Description1 { get; private set; } = "";
      /// <summary>
      /// Kartenname Teil 2 (indirekt über <see cref="Description"/> setzen)
      /// </summary>
      public string Description2 { get; private set; } = "";
      /// <summary>
      /// vollständiger Kartenname
      /// </summary>
      public string Description {
         get {
            return (Description1 + Description2).Trim();
         }
         set {
            if (value.Length > 20) {
               Description1 = value.Substring(0, 20);
               Description2 = value.Substring(20);
               if (Description2.Length > 30)
                  Description2 = Description2.Substring(0, 30);
               else
                  if (Description2.Length < 30)
                  Description2 += new string(' ', 30 - Description2.Length);
            } else {
               Description1 = value;
               if (Description1.Length < 20)
                  Description1 += new string(' ', 20 - Description1.Length);
               Description2 = new string(' ', 30);
            }
         }
      }
      /// <summary>
      /// Exponent der FAT-Blockgröße (i.A. 9)
      /// </summary>
      public byte BlocksizeExp1;
      /// <summary>
      /// Zusatz-Exponent der Datei-Blockgröße (i.A. 0)
      /// </summary>
      public byte BlocksizeExp2;
      /// <summary>
      /// Blocklänge des Headers und der FAT (i.A. 512); ergibt sich aus <see cref="BlocksizeExp1"/>
      /// </summary>
      public int FATBlockLength {
         get {
            return 0x1 << BlocksizeExp1;
         }
         set {
            for (byte i = 1; i <= 255; i++)
               if (0x1 << i >= value) {
                  BlocksizeExp1 = i;
                  break;
               }
         }
      }
      /// <summary>
      /// Blocklänge des Dateibereiches (i.A. 512); ergibt sich aus <see cref="BlocksizeExp1"/> und <see cref="BlocksizeExp2"/>
      /// </summary>
      public int FileBlockLength {
         get {
            return 0x1 << (BlocksizeExp1 + BlocksizeExp2);
         }
         set {
            for (byte i = 0; i <= 255; i++)
               if (0x1 << (i + BlocksizeExp1) >= value) {
                  BlocksizeExp2 = i;
                  break;
               }
         }
      }
      /// <summary>
      /// Länge des Headers (ergibt sich aus der Anzahl <see cref="HeadSectors"/> und der (festen) <see cref="SECTOR_BLOCKSIZE"/>)
      /// </summary>
      public int HeaderLength {
         get {
            return SECTOR_BLOCKSIZE * HeadSectors;
         }
      }

      byte _HeadSectors;
      /// <summary>
      /// Anzahl der Header-Sektoren (bis zum Beginn der FAT; Blockgröße wie FAT)
      /// </summary>
      public byte HeadSectors {
         get {
            return _HeadSectors;
         }
         set {
            if (value > 0 &&
                (SECTOR_BLOCKSIZE * value) % FileBlockLength == 0) {
               _HeadSectors = value;
               Unknown_x200 = new byte[HeaderLength - 0x200];
            }
         }
      }

      /// <summary>
      /// i.A. 256
      /// </summary>
      public UInt16 HeadsPerCylinder;
      /// <summary>
      /// i.A. 32
      /// </summary>
      public UInt16 SectorsPerTrack;
      /// <summary>
      /// i.A. größer 800
      /// </summary>
      public UInt16 Cylinders;
      /// <summary>
      /// immer (?) identisch mit <see cref="HeadsPerCylinder"/>
      /// </summary>
      public UInt16 HeadsPerCylinder2;
      /// <summary>
      /// immer (?) identisch mit <see cref="SectorsPerTrack"/>
      /// </summary>
      public UInt16 SectorsPerTrack2;
      /// <summary>
      /// Anzahl der Blöcke + 1 für die gesamte (!) IMG-Datei (max 0xFFFF); Blocklänge ist <see cref="FileBlockLength"/>
      /// </summary>
      public UInt16 Blocks4Img;

      /// <summary>
      /// immer 0
      /// </summary>
      public byte StartHeadNumber4Partition;
      /// <summary>
      /// immer 1 (1-basiert)
      /// </summary>
      public byte StartSectorNumber4Partition;
      /// <summary>
      /// immer 0
      /// </summary>
      public byte StartCylinderNumber4Partition;
      /// <summary>
      /// immer 0 (?)
      /// </summary>
      public byte Systemtyp;
      /// <summary>
      /// Kopf für den letzten Sektor (0-basiert)
      /// </summary>
      public byte LastHeadNumber4Partition;
      /// <summary>
      /// Sektor für den letzten Sektor (1-basiert); nur die untersten 6 Bit; Bit 6 und 7 als Bit 8 und 9 für Zylinder!
      /// </summary>
      public byte LastSectorNumber4Partition;
      /// <summary>
      /// Zylinder für den letzten Sektor (0-basiert); Bit 8 und 9 von Bit 6 und 7 des Sektors!
      /// </summary>
      public byte LastCylinderNumber4Partition;
      /// <summary>
      /// immer 0 (?)
      /// </summary>
      public UInt32 RelativeSectors;
      /// <summary>
      /// Nummer des letzten Sektors der IMG-Datei (1-basiert!) (aus <see cref="Blocks4Img"/> bestimmt)
      /// </summary>
      public UInt32 LastSectorNumber4IMG;

      // unbekannte Bereiche als Byte oder Byte-Array
      public byte[] Unknown_x01;
      public byte[] Unknown_x0c;
      public byte[] Unknown_x16;
      public byte[] Unknown_x1e;
      public byte[] Unknown_x47;
      public byte[] Unknown_x83;
      public byte[] Unknown_x1ce;
      public byte[]? Unknown_x200;


      public Header() {
         BlocksizeExp1 = 9;      // -> 512 Byte
         BlocksizeExp2 = 2;      // -> 2048 Byte

         // Werte gesehen bei MS und BC: 6602752 Sektoren -> 3224 MB
         Cylinders = 806;
         HeadsPerCylinder = HeadsPerCylinder2 = 256;
         SectorsPerTrack = SectorsPerTrack2 = 32;

         StartCylinderNumber4Partition = LastCylinderNumber4Partition = 0;
         StartHeadNumber4Partition = LastHeadNumber4Partition = 0;
         StartSectorNumber4Partition = LastSectorNumber4Partition = 1;
         RelativeSectors = LastSectorNumber4IMG = 0;

         Description = "Test";
         CreationDate = DateTime.Now;

         XOR = 0;
         UpdateMonth = (byte)CreationDate.Month;
         UpdateYear = CreationDate.Year;
         MapsourceFlag = 0;
         Checksum = 0;

         _HeadSectors = 8;         // -> 2kB

         Unknown_x01 = new byte[9];
         Unknown_x0c = new byte[2];
         Unknown_x16 = new byte[2] { 0x00, 0x02 };
         Unknown_x1e = new byte[27];
         Unknown_x47 = new byte[2];
         Unknown_x83 = new byte[0x13c];
         Unknown_x1ce = new byte[0x30];
         Unknown_x200 = null;
      }

      public Header(BinaryReaderWriter br)
         : this() {
         Read(br);
      }

      public void Read(BinaryReaderWriter br) {
         // 0x0
         /* Before you can look at an IMG file, you must XOR it with the XOR byte (the first byte in the file). Some maps are not XOR’d (the first byte is 0x00). */
         br.XOR = XOR = br.ReadByte();

         // 0x01
         br.ReadBytes(Unknown_x01);

         // 0x0a
         UpdateMonth = br.ReadByte();
         // 0x0b
         _UpdateYear = br.ReadByte();

         // 0x0c
         br.ReadBytes(Unknown_x0c);

         // 0x0e
         // Mapsource flag, 1 - file created by Mapsource, 0 - Garmin map visible in Basecamp and Homeport
         MapsourceFlag = br.ReadByte();

         /* 0x0f sum % 256
          * The checksum at 0xF is calculated by summing all bytes, save for the checksum byte itself, in the
            map file and then multiplying by -1. The lowest byte in this product becomes the checksum byte
            at 0xF. Note that this checksum is apparently not validated by MapSource, since you can modify
            the contents of IMG files directly with a hex editor and they will still work even if the checksum
            is not updated. */
         Checksum = br.ReadByte();

         // 0x10
         string tmp = Encoding.ASCII.GetString(br.ReadBytes(SIGNATURE.Length));
         if (tmp != SIGNATURE)
            throw new Exception("Das ist keine Garmin-IMG-Datei.");

         // 0x16
         br.ReadBytes(Unknown_x16);       // (Size of each FAT, in sectors, for FAT12/16; 0 for FAT32 )

         // 0x18
         SectorsPerTrack = br.Read2AsUShort();
         // 0x1a
         HeadsPerCylinder = br.Read2AsUShort();
         // 0x1c
         Cylinders = br.Read2AsUShort();

         // 0x1e
         br.ReadBytes(Unknown_x1e);

         // 0x39
         CreationDate = new DateTime(br.Read2AsUShort(),
                                     br.ReadByte(),
                                     br.ReadByte(),
                                     br.ReadByte(),
                                     br.ReadByte(),
                                     br.ReadByte());

         // 0x40
         _HeadSectors = br.ReadByte();

         // 0x41
         tmp = Encoding.ASCII.GetString(br.ReadBytes(MAPFILEIDENTIFIER.Length));
         if (tmp != MAPFILEIDENTIFIER)
            throw new Exception("Das ist keine Garmin-IMG-Datei.");

         // 0x47
         br.ReadBytes(Unknown_x47);

         // 0x49
         Description1 = Encoding.ASCII.GetString(br.ReadBytes(20));

         // 0x5d
         HeadsPerCylinder2 = br.Read2AsUShort();
         // 0x5f
         SectorsPerTrack2 = br.Read2AsUShort();

         // 0x61
         /* Most notable are the two byte values (at 0x61 and 0x62, here named as E1 and E2) which are
            used to set the block size for the file. They represent powers of two, and the block size is set via
            the formula 2E1+E2 . E1 appears to always be 0x09, setting the minimum block size to 512 bytes. */
         BlocksizeExp1 = br.ReadByte();
         // 0x62
         BlocksizeExp2 = br.ReadByte();

         // 0x63
         Blocks4Img = br.Read2AsUShort();

         // 0x65
         Description2 = Encoding.ASCII.GetString(br.ReadBytes(30));

         // 0x83
         br.ReadBytes(Unknown_x83);

         // "partition table"

         // 0x1bf
         StartHeadNumber4Partition = br.ReadByte();
         // 0x1c0
         StartSectorNumber4Partition = br.ReadByte();
         // 0x1c1
         StartCylinderNumber4Partition = br.ReadByte();

         // 0x1c2
         Systemtyp = br.ReadByte();

         // 0x1c3
         LastHeadNumber4Partition = br.ReadByte();
         // 0x1c4
         LastSectorNumber4Partition = br.ReadByte();
         // 0x1c5
         LastCylinderNumber4Partition = br.ReadByte();

         // 0x1c6
         RelativeSectors = br.Read4UInt();
         // 0x1ca
         LastSectorNumber4IMG = br.Read4UInt();

         // 0x1ce
         br.ReadBytes(Unknown_x1ce);

         // 0x1fe
         if (br.Read2AsUShort() != BOOTSIGNATURE)
            throw new Exception("Das ist keine Garmin-IMG-Datei.");

         // 0x200
         Unknown_x200 = new byte[(_HeadSectors - 1) * SECTOR_BLOCKSIZE];
         br.ReadBytes(Unknown_x200);

      }

      /// <summary>
      /// schreibt den Header (<see cref="Blocks4Img"/> muss korrekt für die Dateilänge gesetzt sein)
      /// </summary>
      /// <param name="wr"></param>
      public void Write(BinaryReaderWriter wr) {
         CalculateGeometry();

         wr.Write(XOR);
         wr.Write(Unknown_x01);
         wr.Write(UpdateMonth);
         wr.Write(_UpdateYear);
         wr.Write(Unknown_x0c);
         wr.Write(MapsourceFlag);
         wr.Write(Checksum);
         wr.Write(Encoding.ASCII.GetBytes(SIGNATURE));
         wr.Write(Unknown_x16);
         wr.Write(SectorsPerTrack);
         wr.Write(HeadsPerCylinder);
         wr.Write(Cylinders);
         wr.Write(Unknown_x1e);
         wr.Write((UInt16)CreationDate.Year);
         wr.Write((byte)CreationDate.Month);
         wr.Write((byte)CreationDate.Day);
         wr.Write((byte)CreationDate.Hour);
         wr.Write((byte)CreationDate.Minute);
         wr.Write((byte)CreationDate.Second);
         wr.Write(_HeadSectors);
         wr.Write(Encoding.ASCII.GetBytes(MAPFILEIDENTIFIER));
         wr.Write(Unknown_x47);
         wr.Write(Encoding.ASCII.GetBytes(Description1));
         wr.Write(HeadsPerCylinder2);
         wr.Write(SectorsPerTrack2);
         wr.Write(BlocksizeExp1);
         wr.Write(BlocksizeExp2);
         wr.Write(Blocks4Img);
         wr.Write(Encoding.ASCII.GetBytes(Description2));
         wr.Write(Unknown_x83);
         wr.Write(StartHeadNumber4Partition);
         wr.Write(StartSectorNumber4Partition);
         wr.Write(StartCylinderNumber4Partition);
         wr.Write(Systemtyp);
         wr.Write(LastHeadNumber4Partition);
         wr.Write(LastSectorNumber4Partition);
         wr.Write(LastCylinderNumber4Partition);
         wr.Write(RelativeSectors);
         wr.Write(LastSectorNumber4IMG);
         wr.Write(Unknown_x1ce);
         wr.Write(BOOTSIGNATURE);

         Unknown_x200 = new byte[(_HeadSectors - 1) * SECTOR_BLOCKSIZE];
         wr.Write(Unknown_x200);
      }

      /// <summary>
      /// passende HDD-Geometrie "erfinden"
      /// </summary>
      void CalculateGeometry() {
         LastSectorNumber4IMG = (uint)(Blocks4Img * (FileBlockLength / SECTOR_BLOCKSIZE)) + 1; // Gesamtanzahl der Sektoren der IMG-Datei (1-basierte Zählung)

         // eine brauchbare CHS-Geometry "erfinden":
         SectorsPerTrack = 32;   // max. 6 Bit verwendbar
         HeadsPerCylinder = 128;
         Cylinders = 0x400;
         bool find = false;
         for (ushort h = 16; h <= 256 && !find; h *= 2) {
            for (byte s = 4; s <= 32 && !find; s *= 2) {
               for (ushort c = 32; c <= 1024 && !find; c *= 2) {
                  if (c == 1024)
                     c--;
                  if (s * h * c > LastSectorNumber4IMG) {
                     HeadsPerCylinder = h;
                     SectorsPerTrack = s;
                     Cylinders = c;
                     find = true;
                  }
               }
            }
         }
         SectorsPerTrack2 = SectorsPerTrack;
         HeadsPerCylinder2 = HeadsPerCylinder;

         Systemtyp = 0;
         RelativeSectors = 0;

         // Start- und Endwerte der Partition setzen
         StartCylinderNumber4Partition = 0; // konstante Werte, da nur eine Partition
         StartHeadNumber4Partition = 0;
         StartSectorNumber4Partition = 1;

         // LBA-Adresse des letzten Sektors (lineare Zählung der LBA-Sektoren ab 0, 1, ...)
         Lba2CHS(LastSectorNumber4IMG - 1, SectorsPerTrack, HeadsPerCylinder, out int C, out int H, out int S);
         LastCylinderNumber4Partition = (byte)(C & 0xFF);
         LastHeadNumber4Partition = (byte)H;
         LastSectorNumber4Partition = (byte)(((C & 0x300) >> 2) | (S & 0x3F)); // Bit 8,9 von C -> 6,7 und Bit 0..5 von S
      }

      #region Umrechnung LBA - CHS

      /*
       * LBA: "lineare Adressierung" der Sektoren 0-basiert
       * CHS: Adressierung über die Nummer des Zylinders (Spur/Track) 0-basiert, des Schreib-/Lesekopfes 0-basiert und des Sektors auf dem Zylinder 1-basiert
       */

      /// <summary>
      /// Berechnung der CS-Werte aus den Daten der Partitionstabelle: 
      /// <para>Der C(ylinder) wird mit 10 Bit angegeben, der S(ector) mit 6 Bit. Die Bits 6 und 7 des "Sektors" werden als Bit 8 und 9 des Cylinders interpretiert.</para>
      /// </summary>
      /// <param name="raw_sec"></param>
      /// <param name="sc2"></param>
      /// <param name="cylinder"></param>
      /// <param name="sector"></param>
      public void GetCHSFromPartitionTable(byte raw_sec, byte raw_cyl, out int cylinder, out int sector) {
         sector = raw_sec & 0x3f;
         cylinder = ((raw_sec & 0xc0) << 2) | raw_cyl;
      }

      /// <summary>
      /// Berechnung der CHS-Werte aus der 'logical block address'
      /// </summary>
      /// <param name="lba">logical block address</param>
      /// <param name="sectorsPerTrack"></param>
      /// <param name="headsPerCylinder"></param>
      /// <param name="cylinder"></param>
      /// <param name="head"></param>
      /// <param name="sector"></param>
      public void Lba2CHS(uint lba, uint sectorsPerTrack, uint headsPerCylinder, out int cylinder, out int head, out int sector) {
         head = (int)((lba / sectorsPerTrack) % headsPerCylinder);
         sector = (int)((lba % sectorsPerTrack) + 1);
         cylinder = (int)(lba / (sectorsPerTrack * headsPerCylinder));
      }

      /// <summary>
      /// Berechnung der 'logical block address' aus den CHS-Werten
      /// </summary>
      /// <param name="cylinder">C</param>
      /// <param name="head">H</param>
      /// <param name="sector">S</param>
      /// <param name="sectorsPerTrack"></param>
      /// <param name="headsPerCylinder"></param>
      /// <returns>logical block address</returns>
      public int CHS2Lba(int cylinder, int head, int sector, int sectorsPerTrack, int headsPerCylinder) {
         return (cylinder * headsPerCylinder + head) * sectorsPerTrack + (sector - 1);
      }

      #endregion

      public override string ToString() {
         return string.Format("Description {0}, BlockLength={1}, BlockLengthFiledata={2}, FATStart={3}",
            Description,
            FATBlockLength,
            FileBlockLength,
            HeaderLength);
      }

   }

}
