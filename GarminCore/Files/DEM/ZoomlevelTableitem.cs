using System;
using System.IO;

namespace GarminCore.Files.DEM {

   /// <summary>
   /// Tabelleneintrag für die Zoomlevel-Tabelle
   /// </summary>
   public class ZoomlevelTableitem {

      const ulong DEG_UNIT_FACTOR = 1UL << 32;

      /// <summary>
      /// spez. Type (i.A. 0, aber auch 1 gesehen)
      /// </summary>
      public byte SpecType { get; set; }
      /// <summary>
      /// Nummer des Eintrages (0, ...)
      /// </summary>
      public byte No { get; set; }
      /// <summary>
      /// Anzahl der Datenpunkte waagerecht
      /// </summary>
      public int PointsHoriz { get; set; }
      /// <summary>
      /// Anzahl der Datenpunkte senkrecht
      /// </summary>
      public int PointsVert { get; set; }
      /// <summary>
      /// Höhe -1 der letzten Zeile
      /// </summary>
      public int LastRowHeight { get; set; }
      /// <summary>
      /// Breite -1 der letzten Spalte
      /// </summary>
      public int LastColWidth { get; set; }
      /// <summary>
      /// unbekannt auf 0x12
      /// </summary>
      public short Unknown12 { get; set; }
      /// <summary>
      /// größter Subtile-Index waagerecht (Anzahl -1)
      /// </summary>
      public int MaxIdxHoriz { get; set; }
      /// <summary>
      /// größter Subtile-Index senkrecht (Anzahl -1)
      /// </summary>
      public int MaxIdxVert { get; set; }
      /// <summary>
      /// Struktur des Subtile-Tabelleneintrags (Länge der einzelnen Elemente)
      /// </summary>
      public short Structure { get; private set; }
      /// <summary>
      /// 1..3
      /// </summary>
      public int Structure_OffsetSize {
         get {
            return 1 + (Structure & 0x3);
         }
         set {
            switch (value) {
               case 1: Structure = (short)((Structure & 0xFFFC)); break;
               case 2: Structure = (short)((Structure & 0xFFFC) | 0x1); break;
               case 3: Structure = (short)((Structure & 0xFFFC) | 0x2); break;
               case 4: Structure = (short)((Structure & 0xFFFC) | 0x3); break;
            }
         }
      }
      /// <summary>
      /// 1, 2
      /// </summary>
      public int Structure_BaseheightSize {
         get {
            return 1 + ((Structure >> 2) & 0x1);
         }
         set {
            switch (value) {
               case 1: Structure = (short)((Structure & 0xFFFB)); break;
               case 2: Structure = (short)((Structure & 0xFFFB) | 0x4); break;
            }
         }
      }
      /// <summary>
      /// 1, 2
      /// </summary>
      public int Structure_DiffSize {
         get {
            return 1 + ((Structure >> 3) & 0x1);
         }
         set {
            switch (value) {
               case 1: Structure = (short)((Structure & 0xFFF7)); break;
               case 2: Structure = (short)((Structure & 0xFFF7) | 0x8); break;
            }
         }
      }
      /// <summary>
      /// 0, 1
      /// </summary>
      public int Structure_CodingtypeSize {
         get {
            return (Structure & 0x10) >> 4;
         }
         set {
            switch (value) {
               case 0: Structure = (short)((Structure & 0xFFEF)); break;
               case 1: Structure = (short)((Structure & 0xFFEF) | 0x10); break;
            }
         }
      }
      /// <summary>
      /// Länge des Subtile-Tabelleneintrags
      /// </summary>
      public short SubtileTableitemSize {
         get {
            return (short)(Structure_OffsetSize + Structure_BaseheightSize + Structure_DiffSize + Structure_CodingtypeSize);
         }
      }
      /// <summary>
      /// Pointer auf die Subtile-Tabelle (bezogen auf den Dateianfang)
      /// </summary>
      public uint PtrSubtileTable { get; set; }
      /// <summary>
      /// Pointer auf den Höhendatenbereich (bezogen auf den Dateianfang)
      /// </summary>
      public uint PtrHeightdata { get; set; }

      public int west { get; private set; }
      /// <summary>
      /// westliche Grenze der Kachel
      /// </summary>
      public double West {
         get {
            return Unit2Degree(west);
         }
         set {
            west = Degree2Unit(value);
         }
      }

      public int north { get; private set; }
      /// <summary>
      /// nördliche Grenze der Kachel
      /// </summary>
      public double North {
         get {
            return Unit2Degree(north);
         }
         set {
            north = Degree2Unit(value);
         }
      }

      public int pointDistanceHoriz { get; private set; }
      /// <summary>
      /// waagerechter Abstand zwischen den Datenpunkten
      /// </summary>
      public double PointDistanceHoriz {
         get {
            return Unit2Degree(pointDistanceHoriz);
         }
         set {
            pointDistanceHoriz = Degree2Unit(value);
         }
      }

      public int pointDistanceVert { get; private set; }
      /// <summary>
      /// senkrechter Abstand zwischen den Datenpunkten
      /// </summary>
      public double PointDistanceVert {
         get {
            return Unit2Degree(pointDistanceVert);
         }
         set {
            pointDistanceVert = Degree2Unit(value);
         }
      }

      /// <summary>
      /// kleinste Höhe, d.h. kleinste Basishöhe eines Subtiles
      /// </summary>
      public short MinHeight { get; set; }
      /// <summary>
      /// größte Höhe, d.h. größte Basishöhe eines Subtiles
      /// </summary>
      public ushort MaxHeight { get; set; }

      public int SubtileCount {
         get {
            return (1 + MaxIdxHoriz) * (1 + MaxIdxVert);
         }
      }


      public ZoomlevelTableitem() {
         SpecType = 0;
         No = 0;
         PointsHoriz = PointsVert = 64;
         LastRowHeight = 64;
         LastColWidth = 64;
         Unknown12 = 0;
         MaxIdxHoriz = MaxIdxVert = 0;
         Structure = 0;
         Structure_OffsetSize = 3;
         Structure_BaseheightSize = 2;
         Structure_DiffSize = 2;
         Structure_CodingtypeSize = 1;
         West = 12.0;
         North = 54.0;
         PointDistanceHoriz = PointDistanceVert = 0.00028;
         MinHeight = 0;
         MaxHeight = 0;
      }

      public void Read(BinaryReaderWriter br, UInt16 recordlen) {
         if (recordlen >= 0x3C) {
            SpecType = br.ReadByte();
            No = br.ReadByte();
            PointsHoriz = br.Read4Int();
            PointsVert = br.Read4Int();
            LastRowHeight = br.Read4Int();
            LastColWidth = br.Read4Int();
            Unknown12 = br.Read2AsShort();
            MaxIdxHoriz = br.Read4Int();
            MaxIdxVert = br.Read4Int();
            Structure = br.Read2AsShort();
            short tmp = br.Read2AsShort();   // SubtileTableitemSize: ergibt sich schon aus Structure
            PtrSubtileTable = br.Read4UInt();
            PtrHeightdata = br.Read4UInt();
            west = br.Read4Int();
            north = br.Read4Int();
            pointDistanceVert = br.Read4Int();
            pointDistanceHoriz = br.Read4Int();
            MinHeight = br.Read2AsShort();
            MaxHeight = br.Read2AsUShort();
         }
      }

      public void Write(BinaryReaderWriter w) {
         w.Write(SpecType);
         w.Write(No);
         w.Write(PointsHoriz);
         w.Write(PointsVert);
         w.Write(LastRowHeight);
         w.Write(LastColWidth);
         w.Write(Unknown12);
         w.Write(MaxIdxHoriz);
         w.Write(MaxIdxVert);
         w.Write(Structure);
         w.Write(SubtileTableitemSize);
         w.Write(PtrSubtileTable);
         w.Write(PtrHeightdata);
         w.Write(west);
         w.Write(north);
         w.Write(pointDistanceVert);
         w.Write(pointDistanceHoriz);
         w.Write(MinHeight);
         w.Write(MaxHeight);
      }

      public static int Degree2Unit(double degree) {
         return (int)(degree / 360.0 * DEG_UNIT_FACTOR);
      }

      public static double Unit2Degree(int unit) {
         return unit * 360.0 / DEG_UNIT_FACTOR;
      }

   }
}
