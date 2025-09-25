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

namespace GarminCore.Files {

   /*
http://wiki.openstreetmap.org/wiki/OSM_Map_On_Garmin/MDR_Subfile_Format


MDR1 Subtile List
MDR4 POI List
MDR5 City and ZIP List
MDR6 ShortForm Gazetteer
MDR7 Sorted Roads List
MDR9 POI Chapter-list
MDR10 POI Category-based Pick-lists
MDR11 Sorted POI Master List
MDR15 Sorted Strings List
MDR17 String Compression-Lists
        1 The MDR17 subsection length encoding
        2 The MDR17 subsection header
        3 The MDR17 subsection body
MDR19 (unknown)
MDR30/31 Geographical Words Table
MDR32/33 StreetAddress Words Table

MDR1 Subtile List
=================
This section lists all subtiles that are referenced by this MDR file. The index numbers (counting from 1) of the subtiles listed 
in this section are used in various other MDR sections to specify the subtile in which a given item might be found.

4 	Subtile file name coded into a long. If bit 1 of the 'flags' field was set in the MDR1 header, then a 'printf' format 
   of "I%07X.GMP" will create the subtile's name. Otherwise the subtile is held in the set of files "%08ld.{NOD,LBL,etc}".
4 	Optional - only appears if bit 0 of the 'flags' field was set in the MDR1 header. Seems to be an absolute pointer 
   (within the MDR file) to tile-specific data.

	public void writeSectData(ImgFileWriter writer) {
		boolean revIndex = (getExtraValue() & 1) != 0;
		for (Mdr1Record rec : maps) {
			writer.putInt(rec.getMapNumber());
			if (revIndex)
				writer.putInt(rec.getIndexOffset());
		}
	} 
   
MDR4 POI List
=============
This section lists all the POI types that will appear in the map and (presumably) is used to build the menu structure in the 
GUI of the GPS units and MapSource. 

1 	POI major type.
1 	Often seems to be 0x00 (apparently is some sort of index, used by MDR8/9/10/11??).
1 	POI subtype

	public void writeSectData(ImgFileWriter writer) {
		List<Mdr4Record> list = new ArrayList<Mdr4Record>(poiTypes);
		Collections.sort(list);

		for (Mdr4Record r : list) {
			writer.put((byte) r.getType());
			writer.put((byte) r.getUnknown());
			writer.put((byte) r.getSubtype());
		}
	} 
   
   public class Mdr4Record implements Comparable<Mdr4Record> {
      private int type;
      private int subtype;
      private int unknown;

      public int compareTo(Mdr4Record o) {
         int t1 = ((type<<8) + subtype) & 0xffff;
         int t2 = ((o.type<<8) + o.subtype) & 0xffff;
         if (t1 == t2)
            return 0;
         else if (t1 < t2)
            return -1;
         else
            return 1;
      } 
      ...

MDR5 City and ZIP List
======================
The MDR5 section contains a sorted list of all cities. 

1 or 2   (depends on length of the MDR1 section) 	
         1-based index to the subtile that provides this entry (from MDR1). 
1- 4     (given by bits [1:0] of the MDR5 'flags' plus one) 	
         Ignoring the most significant 2 bits gives us a city number in its subtile's ".LBL" file.
         The most significant bit is set if this entry has a matching entry in the Shortform Gazetteer (MDR6).
         Not seen the next most significant bit set. 
3 	      Offset to the street list in MDR7. Ignore the most significant bit though - this seems to be a flag indicating 
         the length of the item being pointed-at (8 or 9 bytes so it seems).
3 	      Offset 	When Flags Byte #3 bit 7 is set, this is a zero-based offset into MDR20.
1 	      Flags byte #2 	When Flags Byte #3 bit 7 is set, seems to indicate the structure of the MDR20 data being pointed-to.
1 	      Flags byte #3 	Bit 7 seems to be set only if the MDR20 offset is valid. 

	public void writeSectData(ImgFileWriter writer) {
		int size20 = getSizes().getMdr20Size();
		Mdr5Record lastCity = null;
		boolean hasString = hasFlag(0x8);
		boolean hasRegion = hasFlag(0x4);
		Collator collator = getConfig().getSort().getCollator();
		for (Mdr5Record city : cities) {
			int gci = city.getGlobalCityIndex();
			addIndexPointer(city.getMapIndex(), gci);

			// Work out if the name is the same as the previous one and set
			// the flag if so.
			int flag = 0;
			int mapIndex = city.getMapIndex();
			int region = city.getRegionIndex();

			// Set the no-repeat flag if the name/region is different
			if (!city.isSameByName(collator, lastCity)) {
				flag = 0x800000;
				lastCity = city;
			}

			// Write out the record
			putMapIndex(writer, mapIndex);
			putLocalCityIndex(writer, city.getCityIndex());
			writer.put3(flag | city.getLblOffset());
			if (hasRegion)
				writer.putChar((char) region);
			if (hasString)
				putStringOffset(writer, city.getStringOffset());
			putN(writer, size20, city.getMdr20());
		}
	} 
   
   

MDR6 ShortForm Gazetteer (Ortsverzeichnis)
========================
The MDR6 section is a global sorted list of all cities. This is much the same as with the Extended Gazetteer (above), but 
there are no duplicate entries, it's just an array containing a reference to the given city's subtile file and the number of 
the city within that tile. 

1 	Subtile number. 	   This is the "1-based" index to a subtile record in the MDR1 array.
ev. 2                      It may be that a given .IMG file is composed of more than 256 subtiles, in which case the 
                           following alternative may exist, with the "Record Length" field of the MDR6 header set to "4". 
                           This has not been confirmed yet. 
2 	City number 	      (In that subtile's LBL subfile??) 

	public void writeSectData(ImgFileWriter writer) {
		int zipSize = getSizes().getZipSize();

		List<SortKey<Mdr6Record>> sortKeys = MdrUtils.sortList(getConfig().getSort(), zips);

		boolean hasString = hasFlag(0x4);
		int record = 1;
		for (SortKey<Mdr6Record> key : sortKeys) {
			Mdr6Record z = key.getObject();
			addIndexPointer(z.getMapIndex(), record++);

			putMapIndex(writer, z.getMapIndex());
			putN(writer, zipSize, z.getZipIndex());
			if (hasString)
				putStringOffset(writer, z.getStringOffset());
		}
	} 

MDR7 Sorted Roads List
======================
This is a global sorted list of Road Names. 

1 or 2 	Subtile index from MDR1.
3 	      Road pointer offset into the subtile's NET file. The top bit, 0x800000, is a flag
3 	      (Optional) Pointer to a label in MDR15. If this entry is missing, then the table at subsection #1 of MDR17 
         will contain indexes to the elements of this table instead. 

MDR9 POI Chapter-list
=====================
MDR9 contains a one byte index (starting at 1 and counting up) followed by a 2 or 3 byte reference into MDR10. The reference 
is to the first record of that 'chapter' in MDR10.
For simple maps not requiring a chapter-list, then just the bytes "01 01 00 00" (i.e. index 0x01, reference 0x000001) seems 
to be enough, which treats MDR10 as one single chapter.

MDR10 POI Category-based Pick-lists
===================================
It currently looks like MDR10 might have the same record length as MDR9.
MDR10 contains a one byte POI subtype followed by a 2 or 3 byte reference to a POI record-number in MDR11. Because the 
POI records in MDR11 are sorted alphabetically, the POI record-numbers in any given chapter of MDR10 will be in ascending 
order to preserve the alphabetical sorting of items in that chapter.
The top bit of the record-number is a flag. If it is set, then the POI name is unique, if not then the POI name is the 
same as the previous one. 

MDR11 Sorted POI Master List
============================
MDR11 references all the POIs in a map in alphabetical order with no regard of POI type. Functionality for filtering them 
on a type basis is provided by MDR9/MDR10 and MDR18/MDR19. 

1 or 2 	Subtile index from MDR1.
1        (probably possible for this to be two bytes, even three) 	POI number in that subtile.
2 	      Subsection for that POI in its subtile (or maybe city-number within the subtile??)
3 	      Offset to this POI in that subtile's .LBL file
2 	      Zeroes??
3 	      (Optional) Pointer to this POI's name in the MDR15 string-list. 

MDR15 Sorted Strings List
=========================
MDR15 consists merely of a number of concatenated null-terminated strings, sorted alphabetically. These are (optionally) 
referenced from the MDR5, MDR7 and MDR11 sections. 

MDR17 String Compression-Lists
        1 The MDR17 subsection length encoding
        2 The MDR17 subsection header
        3 The MDR17 subsection body

MDR19 (unknown)
===============
MDR19 seems to have a record length of 3 and the same number of records as MDR11. The entries are 3byte unique indexes 
(probably into MDR11). This table can be seen as a permutation table for the MDR11 entries. The highest bit (23) seems 
to be a flag. 

MDR30/31 Geographical Words Table
=================================

MDR32/33 StreetAddress Words Table
==================================
 
    */



   /// <summary>
   /// zum Lesen und Schreiben der MDR-Datei
   /// </summary>
   public class StdFile_MDR : StdFile {

      #region Header-Daten

      // Daten für den vermutlich kürzesten Header

      public UInt16 Codepage;
      public UInt16 SortId1;
      public UInt16 SortId2;
      public byte[] Unknown_x1B = { 0x0E, 0 };

      public DataBlockWithRecordsize? Mdr1;
      public byte[] Unknown_x27 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr2;
      public byte[] Unknown_x35 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr3;
      public byte[] Unknown_x43 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr4;
      public byte[] Unknown_x51 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr5;
      public byte[] Unknown_x5F = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr6;
      public byte[] Unknown_x6D = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr7;
      public byte[] Unknown_x7B = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr8;
      public byte[] Unknown_x89 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr9;
      public byte[] Unknown_x97 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlock? Mdr10;
      public byte[] Unknown_xA3 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr11;
      public byte[] Unknown_xB1 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr12;
      public byte[] Unknown_xBF = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr13;
      public byte[] Unknown_xCD = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr14;
      public byte[] Unknown_xDB = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlock? Mdr15;
      public byte Unknown_xE7 = 0x0;
      public DataBlockWithRecordsize? Mdr16;
      public byte[] Unknown_xF2 = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlock? Mdr17;
      public byte[] Unknown_xFE = { 0x0, 0x0, 0x0, 0x0 };
      public DataBlockWithRecordsize? Mdr18;
      public byte[] Unknown_x10C = { 0x0, 0x0, 0x0, 0x0 };
      public byte[] Unknown_x110 = { };

      #endregion

      /// <summary>
      /// liefert den PostHeader-Datenbereich
      /// </summary>
      /// <returns></returns>
      public DataBlock? PostHeaderDataBlock { get; private set; }

      enum InternalFileSections {
         PostHeaderData = 0,
         Mdr1,
         Mdr2,
         Mdr3,
         Mdr4,
         Mdr5,
         Mdr6,
         Mdr7,
         Mdr8,
         Mdr9,
         Mdr10,
         Mdr11,
         Mdr12,
         Mdr13,
         Mdr14,
         Mdr15,
         Mdr16,
         Mdr17,
         Mdr18,
      }


      public StdFile_MDR()
         : base("MDR") {
      }

      public override string ToString() {
         return string.Format("{0}; Headerlänge: 0x{1:X}", base.ToString(), Headerlength);
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         Filesections.ClearSections();

         Codepage = br.Read2AsUShort();
         br.SetEncoding(Codepage);

         SortId1 = br.Read2AsUShort();
         SortId2 = br.Read2AsUShort();
         br.ReadBytes(Unknown_x1B);
         Mdr1 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x27);
         Mdr2 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x35);
         Mdr3 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x43);
         Mdr4 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x51);
         Mdr5 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x5F);
         Mdr6 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x6D);
         Mdr7 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x7B);
         Mdr8 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x89);
         Mdr9 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x97);
         Mdr10 = new DataBlock(br);
         br.ReadBytes(Unknown_xA3);
         Mdr11 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_xB1);
         Mdr12 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_xBF);
         Mdr13 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_xCD);
         Mdr14 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_xDB);
         Mdr15 = new DataBlock(br);
         Unknown_xE7 = br.ReadByte();
         Mdr16 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_xF2);
         Mdr17 = new DataBlock(br);
         br.ReadBytes(Unknown_xFE);
         Mdr18 = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x10C);

         if (Headerlength > 0x110) {

            Unknown_x110 = new byte[Headerlength - 0x110];
            br.ReadBytes(Unknown_x110);

         }

      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         Filesections.AddSection((int)InternalFileSections.Mdr1, new DataBlockWithRecordsize(Mdr1));
         Filesections.AddSection((int)InternalFileSections.Mdr2, new DataBlockWithRecordsize(Mdr2));
         Filesections.AddSection((int)InternalFileSections.Mdr3, new DataBlockWithRecordsize(Mdr3));
         Filesections.AddSection((int)InternalFileSections.Mdr4, new DataBlockWithRecordsize(Mdr4));
         Filesections.AddSection((int)InternalFileSections.Mdr5, new DataBlockWithRecordsize(Mdr5));
         Filesections.AddSection((int)InternalFileSections.Mdr6, new DataBlockWithRecordsize(Mdr6));
         Filesections.AddSection((int)InternalFileSections.Mdr7, new DataBlockWithRecordsize(Mdr7));
         Filesections.AddSection((int)InternalFileSections.Mdr8, new DataBlockWithRecordsize(Mdr8));
         Filesections.AddSection((int)InternalFileSections.Mdr9, new DataBlockWithRecordsize(Mdr9));
         Filesections.AddSection((int)InternalFileSections.Mdr10, new DataBlock(Mdr10));
         Filesections.AddSection((int)InternalFileSections.Mdr11, new DataBlockWithRecordsize(Mdr11));
         Filesections.AddSection((int)InternalFileSections.Mdr12, new DataBlockWithRecordsize(Mdr12));
         Filesections.AddSection((int)InternalFileSections.Mdr13, new DataBlockWithRecordsize(Mdr13));
         Filesections.AddSection((int)InternalFileSections.Mdr14, new DataBlockWithRecordsize(Mdr14));
         Filesections.AddSection((int)InternalFileSections.Mdr15, new DataBlock(Mdr15));
         Filesections.AddSection((int)InternalFileSections.Mdr16, new DataBlockWithRecordsize(Mdr16));
         Filesections.AddSection((int)InternalFileSections.Mdr17, new DataBlock(Mdr17));
         Filesections.AddSection((int)InternalFileSections.Mdr18, new DataBlockWithRecordsize(Mdr18));

         // GapOffset und DataOffset setzen
         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);

         if (GapOffset > HeaderOffset + Headerlength) { // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            PostHeaderDataBlock = new DataBlock(HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, PostHeaderDataBlock);
         }

         // Datenblöcke einlesen
         Filesections.ReadSections(br);
      }

      protected override void DecodeSections() {

         RawRead = true; // besser geht es noch nicht

         if (Locked != 0) {
            RawRead = true;
            return;
         }

         // Datenblöcke "interpretieren"
         int filesectiontype;

         filesectiontype = (int)InternalFileSections.Mdr1;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype));
            bl.Offset = 0;
            //Decode_Mdr1(Filesections.GetSectionDataReader(filesectiontype), bl);
            //Filesections.RemoveSection(filesectiontype);
         }

         // usw.

         filesectiontype = (int)InternalFileSections.Mdr10;
         if (Filesections.GetLength(filesectiontype) > 0) {
            //Decode_Mdr10(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            //Filesections.RemoveSection(filesectiontype);
         }

         // usw.

      }

      public override void Encode_Sections() {
         SetData2Filesection((int)InternalFileSections.Mdr1, true);
         SetData2Filesection((int)InternalFileSections.Mdr2, true);
         SetData2Filesection((int)InternalFileSections.Mdr3, true);
         SetData2Filesection((int)InternalFileSections.Mdr4, true);
         SetData2Filesection((int)InternalFileSections.Mdr5, true);
         SetData2Filesection((int)InternalFileSections.Mdr6, true);
         SetData2Filesection((int)InternalFileSections.Mdr7, true);
         SetData2Filesection((int)InternalFileSections.Mdr8, true);
         SetData2Filesection((int)InternalFileSections.Mdr9, true);
         SetData2Filesection((int)InternalFileSections.Mdr10, true);
         SetData2Filesection((int)InternalFileSections.Mdr11, true);
         SetData2Filesection((int)InternalFileSections.Mdr12, true);
         SetData2Filesection((int)InternalFileSections.Mdr13, true);
         SetData2Filesection((int)InternalFileSections.Mdr14, true);
         SetData2Filesection((int)InternalFileSections.Mdr15, true);
         SetData2Filesection((int)InternalFileSections.Mdr16, true);
         SetData2Filesection((int)InternalFileSections.Mdr17, true);
         SetData2Filesection((int)InternalFileSections.Mdr18, true);
      }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         switch ((InternalFileSections)filesectiontype) {
            case InternalFileSections.Mdr1:
               Encode_Mdr1(bw);
               break;

            // usw.

            case InternalFileSections.Mdr10:
               Encode_Mdr10(bw);
               break;

            // usw.

         }
      }

      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.PostHeaderData, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr1, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr2, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr3, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr4, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr5, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr6, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr7, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr8, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr9, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr10, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr11, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr12, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr13, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr14, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr15, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr16, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr17, pos++);
         Filesections.SetOffset((int)InternalFileSections.Mdr18, pos++);

         Filesections.AdjustSections(DataOffset);     // lückenlos ausrichten

         Mdr1 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr1));
         Mdr2 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr2));
         Mdr3 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr3));
         Mdr4 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr4));
         Mdr5 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr5));
         Mdr6 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr6));
         Mdr7 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr7));
         Mdr8 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr8));
         Mdr9 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr9));
         Mdr10 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Mdr10));
         Mdr11 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr11));
         Mdr12 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr12));
         Mdr13 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr13));
         Mdr14 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr14));
         Mdr15 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Mdr15));
         Mdr16 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr16));
         Mdr17 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.Mdr17));
         Mdr18 = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Mdr18));
      }

      #region Decodierung der Datenblöcke

      void Decode_Mdr1(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         if (br != null && block != null && block.Length > 0) {


            throw new Exception("Decode_Mdr1() ist noch nicht implementiert.");


         }
      }

      void Decode_Mdr10(BinaryReaderWriter br, DataBlock block) {
         if (br != null && block != null && block.Length > 0) {


            throw new Exception("Decode_Mdr10() ist noch nicht implementiert.");


         }
      }

      #endregion

      #region Encodierung der Datenblöcke

      void Encode_Mdr1(BinaryReaderWriter bw) {
         if (bw != null) {


            throw new Exception("Encode_Mdr1() ist noch nicht implementiert.");


         }
      }

      void Encode_Mdr10(BinaryReaderWriter bw) {
         if (bw != null) {


            throw new Exception("Encode_Mdr10() ist noch nicht implementiert.");


         }
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            bw.Write(Codepage);
            bw.Write(SortId1);
            bw.Write(SortId2);
            bw.Write(Unknown_x1B);
            Mdr1?.Write(bw);
            bw.Write(Unknown_x27);
            Mdr2?.Write(bw);
            bw.Write(Unknown_x35);
            Mdr3?.Write(bw);
            bw.Write(Unknown_x43);
            Mdr4?.Write(bw);
            bw.Write(Unknown_x51);
            Mdr5?.Write(bw);
            bw.Write(Unknown_x5F);
            Mdr6?.Write(bw);
            bw.Write(Unknown_x6D);
            Mdr7?.Write(bw);
            bw.Write(Unknown_x7B);
            Mdr8?.Write(bw);
            bw.Write(Unknown_x89);
            Mdr9?.Write(bw);
            bw.Write(Unknown_x97);
            Mdr10?.Write(bw);
            bw.Write(Unknown_xA3);
            Mdr11?.Write(bw);
            bw.Write(Unknown_xB1);
            Mdr12?.Write(bw);
            bw.Write(Unknown_xBF);
            Mdr13?.Write(bw);
            bw.Write(Unknown_xCD);
            Mdr14?.Write(bw);
            bw.Write(Unknown_xDB);
            Mdr15?.Write(bw);
            Unknown_xE7 = bw.ReadByte();
            Mdr16?.Write(bw);
            bw.Write(Unknown_xF2);
            Mdr17?.Write(bw);
            bw.Write(Unknown_xFE);
            Mdr18?.Write(bw);
            bw.Write(Unknown_x10C);

            if (Headerlength > 0x110) {

               Unknown_x110 = new byte[Headerlength - 0x110];
               bw.Write(Unknown_x110);

            }
         }
      }

      #endregion

   }

}
