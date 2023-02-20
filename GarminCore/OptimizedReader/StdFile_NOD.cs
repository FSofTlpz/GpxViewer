namespace GarminCore.OptimizedReader {
   /// <summary>
   /// Infos über die Verbindungsknoten der Highways (z.Z. nur Behandlung von Rohdaten)
   /// </summary>
   public class StdFile_NOD : StdFile {

      #region Header-Daten

      /*
      The NOD file that contains routing information.

      NOD1 contains several groups of routing nodes. NOD1 contains links back to NET (and NET contains links to NOD2).
      NOD2 contains road data with links into NOD1.

       */


      /// <summary>
      /// NOD1-Section (Nodes)
      /// </summary>
      DataBlock Nod1Nodes;

      /// <summary>
      /// Bit 0: always set, meaning ?
      /// Bit 1: Enable turn restrictions
      /// Bit 2-4: ?
      /// Bit 5-7: Distance multiplier, effects predicted travel time
      /// Bit 8: Drive on left
      /// Bit 9: unknown, but seen.
      /// Bit 11: unknown, but seen.
      /// Bit 13: unknown, but seen.
      /// Bit 15: unknown, but seen.
      /// </summary>
      public ushort Flags;

      /// <summary>
      /// Unknown. 0x0000
      /// Values seen: 1, 17, 22, 23, 32, 36, 528
      /// </summary>
      byte[] Unknown_0x1F = new byte[2];

      /// <summary>
      /// Align. Values seen: 0, 1, 4, 6 (bei MKGMAP immer 6)
      /// </summary>
      public byte Align;

      /// <summary>
      /// pointer multiplier (bei MKGMAP immer 0)
      /// </summary>
      public byte Mult1;

      /// <summary>
      /// bei MKGMAP immer 5
      /// </summary>
      public ushort TableARecordLen;

      /// <summary>
      /// NOD2-Section (Road data with links into NOD1)
      /// </summary>
      public DataBlock Nod2RoadData;

      /// <summary>
      /// Unknown. (bei MKGMAP immer 0)
      /// 2 Byte: 0
      /// 2 Byte: Values seen: 0, 28, 34, 38, 40
      /// </summary>
      byte[] Unknown_0x2D = new byte[4];

      /// <summary>
      /// NOD3-Section (Boundary nodes)
      /// </summary>
      DataBlockWithRecordsize Nod3BoundaryNodes;

      /// <summary>
      /// Unknown 0x00 and 0x0200 spotted  (bei MKGMAP immer 2)
      /// </summary>
      public byte[] Unknown_0x3B = new byte[4];


      /// <summary>
      /// NOD4-Section (High class boundary)
      /// </summary>
      public DataBlock Nod4HighClassBoundary;

      public int[] ClassBoundaries = new int[5];



      /// <summary>
      /// Unknown, 12 bytes 0x00
      /// </summary>
      byte[] Unknown_0x5B = new byte[12];

      /// <summary>
      /// NOD5-Section
      /// </summary>
      DataBlock Nod5;

      /// <summary>
      /// Unknown. 0x02 spotted
      /// </summary>
      byte[] Unknown_0x6F = new byte[2];

      /// <summary>
      /// NOD6-Section
      /// </summary>
      DataBlockWithRecordsize Nod6;

      /// <summary>
      /// Unknown 0x02 spotted 
      /// </summary>
      byte[] Unknown_0x7B = new byte[4];

      #endregion


      public StdFile_NOD()
         : base("NOD") {
         Nod1Nodes = new DataBlock();
         
         Flags = 0x207; // + ev. das "Drive on left"-Flag
         Align = 6;
         Mult1 = 0;
         TableARecordLen = 5;
         Nod2RoadData = new DataBlock();
         Nod3BoundaryNodes = new DataBlockWithRecordsize();
         Unknown_0x3B[0] = 2;
         Nod4HighClassBoundary = new DataBlock();
         Nod5 = new DataBlock();
         Nod6 = new DataBlockWithRecordsize();
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         readCommonHeader(br, Type);

         Nod1Nodes.Read(br);
         Flags = br.Read2AsUShort();
         br.ReadBytes(Unknown_0x1F);
         Align = br.ReadByte();
         Mult1 = br.ReadByte();
         TableARecordLen = br.Read2AsUShort();
         Nod2RoadData.Read(br);
         br.ReadBytes(Unknown_0x2D);
         Nod3BoundaryNodes.Read(br);
         br.ReadBytes(Unknown_0x3B);

         if (Headerlength > 0x3F) {
            Nod4HighClassBoundary.Read(br);     // 0x3F, 8Byte
            for (int i = 0; i < ClassBoundaries.Length; i++) { // 0x47, 5*4 Bytes
               ClassBoundaries[i] = br.Read4Int();
               if (i > 0)
                  ClassBoundaries[i] += ClassBoundaries[i - 1];
            }
            br.ReadBytes(Unknown_0x5B);
            Nod5.Read(br);
            br.ReadBytes(Unknown_0x6F);
            Nod6.Read(br);
            br.ReadBytes(Unknown_0x7B);
         }

      }

      public override void ReadMinimalSections(BinaryReaderWriter br) {

      }

   }
}
