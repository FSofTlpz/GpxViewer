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
using System.IO;

namespace GarminCore.Files {

   /// <summary>
   /// zum Lesen und Schreiben der SRT-Datei
   /// </summary>
   public class StdFile_SRT : StdFile {

      #region Header-Daten

      public byte[] Unknown_x15 = { 1, 0 };

      public ShortDataBlock? ContentsBlock;

      public byte[] Unknown_x1D = { };

      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,
         ContentsBlock,
         DescriptionBlock,
         CharacterLookupTableBlock,
      }


      public class SortHeader {

         public UInt16 Headerlength { get; private set; }

         public UInt16 Id1;
         public UInt16 Id2;
         public UInt16 Codepage;
         public byte[] Unknown1 = { 0x02, 0x20 }; // auch 0x02 0x6F ?
         public byte[] Unknown2 = { 0x0, 0x0 };
         /// <summary>
         /// zeigt i.A. auf einen Block mit 255 3-Byte-Sätzen
         /// </summary>
         public DataBlockWithRecordsize CharTabBlock; // i.A. 0xff * 3 = 0x2fd Byte lang
         public byte[] Unknown3 = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
         public DataBlockWithRecordsize ExpansionsBlock;
         public byte[] Unknown4 = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
         /// <summary>
         /// identisch zum Offset von <see cref="CharTabBlock"/>
         /// </summary>
         public UInt32 CharTabOffset;
         public byte[] Unknown5 = { 0x0, 0x0, 0x0, 0x0 };
         public byte[] Unknown6 = { };


         public SortHeader() {
            Headerlength = 0x34;
            CharTabBlock = new DataBlockWithRecordsize();
            ExpansionsBlock = new DataBlockWithRecordsize();
         }

         public void Read(BinaryReaderWriter br) {
            Headerlength = br.Read2AsUShort();
            Id1 = br.Read2AsUShort();
            Id2 = br.Read2AsUShort();
            Codepage = br.Read2AsUShort();
            br.SetEncoding(Codepage);

            br.ReadBytes(Unknown1);
            br.ReadBytes(Unknown2);
            CharTabBlock.Read(br);
            br.ReadBytes(Unknown3);
            ExpansionsBlock.Read(br);
            br.ReadBytes(Unknown4);
            CharTabOffset = br.Read4UInt();
            br.ReadBytes(Unknown5);

            if (Headerlength > 0x34) {
               Unknown6 = new byte[Headerlength - 0x34];
               br.ReadBytes(Unknown6);
            }
         }

      }


      public string Description = "";
      public SortHeader? Sortheader;

      /// <summary>
      /// liefert den PostHeader-Datenbereich
      /// </summary>
      /// <returns></returns>
      public DataBlock? PostHeaderDataBlock { get; private set; }
      public DataBlock? DescriptionBlock { get; private set; }
      public DataBlock? CharacterLookupTableBlock { get; private set; }


      public StdFile_SRT()
         : base("SRT") {
         Sortheader = new SortHeader();
      }

      public override string ToString() {
         return string.Format("{0}; {1}",
                              base.ToString(),
                              Description);
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         Filesections.ClearSections();

         br.ReadBytes(Unknown_x15);
         ContentsBlock = new ShortDataBlock(br);

         if (Headerlength > 0x1D) {       // i.A. 0x001D; auch 0x25 gesehen mit 8 zusätzlichen Byte: 00 00 35 00 00 00 10 00

            Unknown_x1D = new byte[Headerlength - 0x1D];
            br.ReadBytes(Unknown_x1D);

         }

      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         Filesections.AddSection((int)InternalFileSections.ContentsBlock, new DataBlock(ContentsBlock));

         // GapOffset und DataOffset setzen
         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);

         if (GapOffset > HeaderOffset + Headerlength) { // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            PostHeaderDataBlock = new DataBlock(HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, PostHeaderDataBlock);
         }

         // Datenblöcke einlesen
         Filesections.ReadSections(br);

         // Pos. der anderen beiden Datenblöcke ermitteln ...
         Decode_ContentsBlock(Filesections.GetSectionDataReader((int)InternalFileSections.ContentsBlock), new DataBlock(0, Filesections.GetLength((int)InternalFileSections.ContentsBlock)));
         // ... und einlesen
         Filesections.Read((int)InternalFileSections.DescriptionBlock, br);
         Filesections.Read((int)InternalFileSections.CharacterLookupTableBlock, br);
      }

      protected override void DecodeSections() {
         // Datenblöcke "interpretieren"
         int filesectiontype;

         filesectiontype = (int)InternalFileSections.DescriptionBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_DescriptionBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
         }

         filesectiontype = (int)InternalFileSections.CharacterLookupTableBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_CharacterLookupTableBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
         }

      }

      public override void Encode_Sections() {
         SetData2Filesection((int)InternalFileSections.DescriptionBlock, true);
         SetData2Filesection((int)InternalFileSections.CharacterLookupTableBlock, true);
      }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         switch ((InternalFileSections)filesectiontype) {
            case InternalFileSections.ContentsBlock:
               Encode_ContentsBlock(bw);
               break;
            case InternalFileSections.DescriptionBlock:
               Encode_DescriptionBlock(bw);
               break;
            case InternalFileSections.CharacterLookupTableBlock:
               Encode_CharacterLookupTableBlock(bw);
               break;
         }
      }

      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.ContentsBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.DescriptionBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.CharacterLookupTableBlock, pos++);

         Filesections.AdjustSections(DataOffset);     // lückenlos ausrichten

         // mit den endgültigen Offsets setzen
         SetData2Filesection((int)InternalFileSections.ContentsBlock, true);

         DataBlockWithRecordsize? tmp = Filesections.GetPosition((int)InternalFileSections.ContentsBlock);
         if (tmp != null)
            ContentsBlock = new ShortDataBlock(tmp.Offset, (ushort)tmp.Length);
      }


      #region Decodierung der Datenblöcke

      void Decode_ContentsBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);
            DescriptionBlock = new DataBlock(br);
            CharacterLookupTableBlock = new DataBlock(br);
            Filesections.AddSection((int)InternalFileSections.DescriptionBlock, DescriptionBlock);
            Filesections.AddSection((int)InternalFileSections.CharacterLookupTableBlock, CharacterLookupTableBlock);
         }
      }

      void Decode_DescriptionBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);
            Description = br.ReadString((int)block.Length);
         }
      }

      void Decode_CharacterLookupTableBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);
            Sortheader?.Read(br);
            if (Locked == 0) {


               //throw new Exception("Decode_CharacterLookupTableBlock() ist noch nicht implementiert.");


            }
         }
      }

      #endregion

      #region Encodierung der Datenblöcke

      void Encode_ContentsBlock(BinaryReaderWriter bw) {
         if (bw != null) {
            DataBlockWithRecordsize? blk;
            blk = Filesections.GetPosition((int)InternalFileSections.DescriptionBlock);
            if (blk != null)
               ((DataBlock)blk).Write(bw);
            blk = Filesections.GetPosition((int)InternalFileSections.CharacterLookupTableBlock);
            if (blk != null)
               ((DataBlock)blk).Write(bw);
         }
      }

      void Encode_DescriptionBlock(BinaryReaderWriter bw) {
         if (bw != null) {
            bw.WriteString(Description);
         }
      }

      void Encode_CharacterLookupTableBlock(BinaryReaderWriter bw) {
         if (bw != null) {


            throw new Exception("Encode_CharacterLookupTableBlock() ist noch nicht implementiert.");


         }
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            bw.Write(Unknown_x15);
            ContentsBlock?.Write(bw);

            if (Headerlength > 0x1D) {       // auch 0x25 gesehen mit 8 zusätzlichen Byte: 00 00 35 00 00 00 10 00

               bw.Write(Unknown_x1D);

            }
         }
      }

      #endregion

   }

}
