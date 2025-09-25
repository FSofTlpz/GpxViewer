namespace GarminCore.Files {
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
      public DataBlock Nod1Nodes;

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
      public byte[] Unknown_0x1F = new byte[2];

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
      public byte[] Unknown_0x2D = new byte[4];

      /// <summary>
      /// NOD3-Section (Boundary nodes)
      /// </summary>
      public DataBlockWithRecordsize Nod3BoundaryNodes;

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
      public byte[] Unknown_0x5B = new byte[12];

      /// <summary>
      /// NOD5-Section
      /// </summary>
      public DataBlock Nod5;

      /// <summary>
      /// Unknown. 0x02 spotted
      /// </summary>
      public byte[] Unknown_0x6F = new byte[2];

      /// <summary>
      /// NOD6-Section
      /// </summary>
      public DataBlockWithRecordsize Nod6;

      /// <summary>
      /// Unknown 0x02 spotted 
      /// </summary>
      public byte[] Unknown_0x7B = new byte[4];

      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,
         Nod1,
         Nod2,
         Nod3,
         Nod4,
         Nod5,
         Nod6,
      }



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
         base.ReadCommonHeader(br, Type);

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

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         Filesections.AddSection((int)InternalFileSections.Nod1, Nod1Nodes);
         Filesections.AddSection((int)InternalFileSections.Nod2, Nod2RoadData);
         Filesections.AddSection((int)InternalFileSections.Nod3, Nod3BoundaryNodes);
         Filesections.AddSection((int)InternalFileSections.Nod4, Nod4HighClassBoundary);
         Filesections.AddSection((int)InternalFileSections.Nod5, Nod5);
         Filesections.AddSection((int)InternalFileSections.Nod6, Nod6);
         if (GapOffset > HeaderOffset + Headerlength) // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));

         // Datenblöcke einlesen
         Filesections.ReadSections(br);

         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);
      }

      protected override void DecodeSections() {
         if (Locked != 0) {
            RawRead = true;
            return;
         }

         // Datenblöcke "interpretieren"
         //int filesectiontype;

         //filesectiontype = (int)InternalFileSections.Nod1;
         //if (Filesections.GetLength(filesectiontype) > 0) {
         //   Decode_Nodes(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
         //   Filesections.RemoveSection(filesectiontype);
         //}





         RawRead = true; // besser geht es noch nicht
      }


      //void Decode_Nodes(BinaryReaderWriter br, DataBlock block) {

      //}





      public override void Encode_Sections() {
         //SetData2Filesection((int)InternalFileSections.Nod1, true);

      }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         switch ((InternalFileSections)filesectiontype) {
            case InternalFileSections.Nod1:
               //Encode_Nod1(bw);
               break;
         }
      }

      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.PostHeaderData, pos++);
         Filesections.SetOffset((int)InternalFileSections.Nod1, pos++);
         Filesections.SetOffset((int)InternalFileSections.Nod2, pos++);
         Filesections.SetOffset((int)InternalFileSections.Nod3, pos++);
         Filesections.SetOffset((int)InternalFileSections.Nod4, pos++);
         Filesections.SetOffset((int)InternalFileSections.Nod5, pos++);
         Filesections.SetOffset((int)InternalFileSections.Nod6, pos++);

         Filesections.AdjustSections(DataOffset);     // lückenlos ausrichten

         Nod1Nodes = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Nod1));
         Nod2RoadData = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Nod2));
         Nod3BoundaryNodes = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Nod3));
         Nod4HighClassBoundary = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Nod4));
         Nod5 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Nod5));
         Nod6 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Nod6));
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            // Header-Daten schreiben
            Nod1Nodes.Write(bw);
            bw.Write(Flags);
            bw.Write(Unknown_0x1F);
            bw.Write(Align);
            bw.Write(Mult1);
            bw.Write(TableARecordLen);
            Nod2RoadData.Write(bw);
            bw.Write(Unknown_0x2D);
            Nod3BoundaryNodes.Write(bw);
            bw.Write(Unknown_0x3B);

            Nod4HighClassBoundary.Write(bw);
            for (int i = 0; i < ClassBoundaries.Length; i++)
               bw.Write(ClassBoundaries[i] - (i > 0 ? ClassBoundaries[i - 1] : 0));



            bw.Write(Unknown_0x5B);
            Nod5.Write(bw);
            bw.Write(Unknown_0x6F);
            Nod6.Write(bw);
            bw.Write(Unknown_0x7B);

         }
      }


   }
}
