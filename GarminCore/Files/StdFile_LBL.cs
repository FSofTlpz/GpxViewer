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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GarminCore.Files {

   /// <summary>
   /// zum Lesen und Schreiben der LBL-Datei (enthält i.W. alle Texte, z.T. Tabellen auf deren Inhalt per 1-basiertem Index zugegriffen wird)
   /// </summary>
   public class StdFile_LBL : StdFile {

      #region Header-Daten

      /// <summary>
      /// Datenbereich für die Texte / Labels (0x15)
      /// </summary>
      public DataBlock? TextBlock { get; private set; }
      /// <summary>
      /// 2er Potenz für den internen Offset-Multiplikator (0x1D) 
      /// </summary>
      public byte DataOffsetMultiplier { get; private set; }
      /// <summary>
      /// Art der Text-Codierung (i.A. 0x09) (0x1E)
      /// </summary>
      public byte EncodingType;
      /// <summary>
      /// Datenbereich für die Ländertabelle (0x1F)
      /// </summary>
      public DataBlockWithRecordsize? CountryBlock { get; private set; }
      public byte[] Unknown_0x29 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Regionentabelle (0x2D)
      /// </summary>
      public DataBlockWithRecordsize? RegionBlock { get; private set; }
      public byte[] Unknown_0x37 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Städtetabelle (0x3B)
      /// </summary>
      public DataBlockWithRecordsize? CityBlock { get; private set; }
      public byte[] Unknown_0x45 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für POI ... ? (0x49)
      /// </summary>
      public DataBlockWithRecordsize? POIIndexBlock { get; private set; }
      public byte[] Unknown_0x53 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für POI-Eigenschaftstabelle (0x57)
      /// </summary>
      public DataBlock? POIPropertiesBlock { get; private set; }
      /// <summary>
      /// 2er Potenz für den internen Offset-Multiplikator (0x5F) 
      /// </summary>
      public byte POIOffsetMultiplier { get; private set; }
      /// <summary>
      /// globale Maske für die vorhandenen POI-Eigenschaften (0x60)
      /// </summary>
      public POIFlags POIGlobalMask { get; set; }
      public byte[] Unknown_0x61 = { 0, 0, 0 };
      /// <summary>
      /// Datenbereich für POI ... ? (0x64)
      /// </summary>
      public DataBlockWithRecordsize? POITypeIndexBlock { get; private set; }
      public byte[] Unknown_0x6E = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Postleitzahltabelle (0x72)
      /// </summary>
      public DataBlockWithRecordsize? ZipBlock { get; private set; }
      public byte[] Unknown_0x7C = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Highwaytabelle (für Routing?) (0x80)
      /// </summary>
      public DataBlockWithRecordsize? HighwayWithExitBlock { get; private set; }
      public byte[] Unknown_0x8A = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Ein- und Ausfahrtentabelle (für Routing?) (0x8E)
      /// </summary>
      public DataBlockWithRecordsize? ExitBlock { get; private set; }
      public byte[] Unknown_0x98 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die zusätzliche Highwaytabelle (für Routing?) (0x9C)
      /// </summary>
      public DataBlockWithRecordsize? HighwayExitBlock { get; private set; }
      public byte[] Unknown_0xA6 = { 0, 0, 0, 0 };

      // --------- Headerlänge > 170 Byte

      /// <summary>
      /// Codepage für die Texte (0xAA)
      /// </summary>
      public UInt16 Codepage;
      /// <summary>
      /// ? (0xAC)
      /// </summary>
      public UInt16 ID1;
      /// <summary>
      /// ? (0xAE)
      /// </summary>
      public UInt16 ID2;
      /// <summary>
      /// Datenbereich für Text, der die Sortierung beschreibt (0xB0)
      /// </summary>
      public DataBlock SortDescriptorDefBlock { get; private set; }
      /// <summary>
      /// ? (0xB8)
      /// </summary>
      public DataBlockWithRecordsize Lbl13Block { get; private set; }
      public byte[] Unknown_0xC2 = { 0, 0 };

      /// <summary>
      /// ? (0xC4)
      /// </summary>
      public DataBlockWithRecordsize TidePredictionBlock { get; private set; }
      public byte[] Unknown_0xCE = { 0, 0 };

      public DataBlock UnknownBlock_0xD0 { get; private set; }
      public byte[] Unknown_0xD8 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0xDE { get; private set; }
      public byte[] Unknown_0xE6 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0xEC { get; private set; }
      public byte[] Unknown_0xF4 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0xFA { get; private set; }
      public byte[] Unknown_0x102 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x108 { get; private set; }
      public byte[] Unknown_0x110 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x116 { get; private set; }
      public byte[] Unknown_0x11E = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x124 { get; private set; }
      public byte[] Unknown_0x12C = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x132 { get; private set; }
      public byte[] Unknown_0x13A = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x140 { get; private set; }
      public byte[] Unknown_0x148 = { 0, 0, 0, 0, 0, 0 };

      public DataBlock UnknownBlock_0x14E { get; private set; }
      public byte[] Unknown_0x156 = { 0, 0, 0, 0 };

      public DataBlock UnknownBlock_0x15A { get; private set; }
      public byte[] Unknown_0x162 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x168 { get; private set; }
      public byte[] Unknown_0x170 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x176 { get; private set; }
      public byte[] Unknown_0x17E = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x184 { get; private set; }
      public byte[] Unknown_0x18C = { 0, 0, 0, 0, 0, 0 };

      public DataBlock UnknownBlock_0x192 { get; private set; }

      public DataBlock UnknownBlock_0x19A { get; private set; }
      public byte[] Unknown_0x1A2 = { 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x1A6 { get; private set; }
      public byte[] Unknown_0x1AE = { 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x1B2 { get; private set; }
      public byte[] Unknown_0x1BA = { 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x1BE { get; private set; }
      public byte[] Unknown_0x1C6 = { 0, 0, 0, 0 };

      public DataBlock UnknownBlock_0x1CA { get; private set; }
      public byte[] Unknown_0x1D2 = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x1D8 { get; private set; }
      public byte[] Unknown_0x1E0 = { 0, 0, 0, 0, 0, 0 };

      public DataBlock UnknownBlock_0x1E6 { get; private set; }
      public byte[] Unknown_0x1EE = { 0, 0, 0, 0 };

      public DataBlock UnknownBlock_0x1F2 { get; private set; }
      public byte[] Unknown_0x1FA = { 0, 0, 0, 0, 0, 0 };
      public DataBlock UnknownBlock_0x200 { get; private set; }
      public byte[] Unknown_0x208 = { 0, 0, 0, 0, 0, 0, 0, 0 };

      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,
         TextBlock,
         CountryBlock,
         RegionBlock,
         CityBlock,
         POIIndexBlock,
         POIPropertiesBlock,
         POITypeIndexBlock,
         ZipBlock,
         HighwayWithExitBlock,
         ExitBlock,
         HighwayExitBlock,
         SortDescriptorDefBlock,
         Lbl13Block,
         TidePredictionBlock,
         UnknownBlock_0xD0,
         UnknownBlock_0xDE,
         UnknownBlock_0xEC,
         UnknownBlock_0xFA,
         UnknownBlock_0x108,
         UnknownBlock_0x116,
         UnknownBlock_0x124,
         UnknownBlock_0x132,
         UnknownBlock_0x140,
         UnknownBlock_0x14E,
         UnknownBlock_0x15A,
         UnknownBlock_0x168,
         UnknownBlock_0x176,
         UnknownBlock_0x184,
         UnknownBlock_0x192,
         UnknownBlock_0x19A,
         UnknownBlock_0x1A6,
         UnknownBlock_0x1B2,
         UnknownBlock_0x1BE,
         UnknownBlock_0x1CA,
         UnknownBlock_0x1D8,
         UnknownBlock_0x1E6,
         UnknownBlock_0x1F2,
         UnknownBlock_0x200,
      }

      #region Datensatzklassen

      /// <summary>
      /// Datensatz in der Highway-Zufahrt/Abzweig-Tabelle
      /// </summary>
      public class HighwayWithExitRecord : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Offset für den Namen des Highways
         /// </summary>
         public UInt32 TextOffset;

         /// <summary>
         /// ergibt mit 3 multipliziert den Offset des 1. Exits im Block der <see cref="HighwayExitDefRecord"/>
         /// <para></para>
         /// </summary>
         public UInt16 FirstExitOffset;

         /// <summary>
         /// unknown (setting any of 0x3f stops exits being found)
         /// </summary>
         public byte Unknown1;

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 6;


         public HighwayWithExitRecord() : this(0, 0, 0) { }

         public HighwayWithExitRecord(UInt32 TextOffset, UInt16 FirstExitOffset, byte Unknown1 = 0) {
            this.TextOffset = TextOffset;
            this.FirstExitOffset = FirstExitOffset;
            this.Unknown1 = Unknown1;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            TextOffset = br.Read3AsUInt();
            FirstExitOffset = br.Read2AsUShort();
            Unknown1 = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write3(TextOffset);
            bw.Write(FirstExitOffset);
            bw.Write(Unknown1);
         }

         public override string ToString() {
            return string.Format("TextOffset {0}, FirstExitOffset {1}, Unknown1 {2}", TextOffset, FirstExitOffset, Unknown1);
         }
      }

      /// <summary>
      /// korrespondierender Datensatz zur Highway-Zufahrt/Abzweig-Tabelle
      /// </summary>
      public class HighwayExitDefRecord : BinaryReaderWriter.DataStruct {

         public byte Unknown1;

         /// <summary>
         /// Region
         /// </summary>
         public UInt16 RegionIndex;

         /// <summary>
         /// Liste der Exit-Punkte
         /// </summary>
         public List<ExitPoint> ExitList;

         public uint DataLength {
            get {
               return (uint)(3 + ExitList.Count * 3);
            }
         }

         public struct ExitPoint {
            /// <summary>
            /// 1-basierter Index des Punktes in der Punkteliste des Subdiv (+ alle Städte, d.h. Typen kleiner 0x0C  ??? WARUM ???)
            /// </summary>
            public byte PointIndexInRGN;
            /// <summary>
            /// Subdivnummer
            /// </summary>
            public UInt16 SubdivisionNumberInRGN;

            public ExitPoint(byte PointIndex = 0, UInt16 SubdivisionNumber = 0) {
               this.PointIndexInRGN = PointIndex;
               this.SubdivisionNumberInRGN = SubdivisionNumber;
            }

            public override string ToString() {
               return string.Format("Index {0}, SubdivisionNumber {1}", PointIndexInRGN, SubdivisionNumberInRGN);
            }
         }

         public HighwayExitDefRecord()
            : this(0, 0) { }

         public HighwayExitDefRecord(UInt16 RegionIndex, byte Unknown1 = 0) {
            this.Unknown1 = Unknown1;
            this.RegionIndex = RegionIndex;
            ExitList = new List<ExitPoint>();
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            if (extdata != null) {
               Unknown1 = br.ReadByte();
               RegionIndex = br.Read2AsUShort();
               ExitList = new List<ExitPoint>();
               int len = (int)extdata;
               len -= 3;
               while (len >= 3) {
                  ExitPoint ep = new ExitPoint {
                     PointIndexInRGN = br.ReadByte(),
                     SubdivisionNumberInRGN = br.Read2AsUShort()
                  };
                  ExitList.Add(ep);
                  len -= 3;
               }
            }
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write(Unknown1);
            bw.Write(RegionIndex);
            for (int i = 0; i < ExitList.Count; i++) {
               bw.Write(ExitList[i].PointIndexInRGN);
               bw.Write(ExitList[i].SubdivisionNumberInRGN);
            }
         }

         public override string ToString() {
            return string.Format("RegionIndex {0}, Exits {1}", RegionIndex, ExitList.Count);
         }
      }

      /// <summary>
      /// Datensatz einer Tabelle, die die Haupttypen der Punkte und den Startindex in der <see cref="PointIndexRecord"/>-Tabelle enthält
      /// <para>Der Index gilt nicht für erweiterte Typen und wahrscheinlich auch nicht für "Stadt"-Typen (kleiner 0x12). Die "Stadt"-Typen werden direkt im Subdiv
      /// als Index-Punkte gespeichert.</para>
      /// <para>(von MKGMAP mit make-poi-index erzeugt, aber: "Generate the POI index (not yet useful).)"</para>
      /// </summary>
      public class PointTypeIndexRecord : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// POI-Typ
         /// </summary>
         public byte PointType;

         /// <summary>
         /// 1-basierter Index für die <see cref="PointIndexRecord"/>-Tabelle, ab der dieser <see cref="PointType"/> steht
         /// </summary>
         public UInt32 StartIdx;

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 4;

         public PointTypeIndexRecord() : this(0, 0) { }

         public PointTypeIndexRecord(byte POIType, UInt32 StartIdx) {
            this.PointType = POIType;
            this.StartIdx = StartIdx;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            PointType = br.ReadByte();
            StartIdx = br.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write(PointType);
            bw.Write3(StartIdx);
         }

         public override string ToString() {
            return string.Format("POIType 0x{0:x}, Count {1}", PointType, StartIdx);
         }
      }

      /// <summary>
      /// Datensatz einer Tabelle, die die Punkte mit dem Index in den Punktlisten der Subdivs und ihren Subtyp enthält
      /// <para>(von MKGMAP mit make-poi-index erzeugt, aber: "Generate the POI index (not yet useful).)"</para>
      /// </summary>
      public class PointIndexRecord : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// 1-basierter Index des Punktes in der Punktliste des Subdiv
         /// </summary>
         public byte PointIndexInRGN;

         /// <summary>
         /// 1-basierter Index des Subdiv
         /// </summary>
         public UInt16 SubdivisionNumberInRGN;

         /// <summary>
         /// Subtyp des Punktes (Haupttyp steht im <see cref="PointTypeIndexRecord"/>)
         /// </summary>
         public byte SubType;

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 4;

         public PointIndexRecord() : this(0, 0, 0) { }

         public PointIndexRecord(byte POIIndex, byte SubType, UInt16 SubdivisionNumber) {
            this.PointIndexInRGN = POIIndex;
            this.SubdivisionNumberInRGN = SubdivisionNumber;
            this.SubType = SubType;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            PointIndexInRGN = br.ReadByte();
            SubdivisionNumberInRGN = br.Read2AsUShort();
            SubType = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write(PointIndexInRGN);
            bw.Write(SubdivisionNumberInRGN);
            bw.Write(SubType);
         }

         public override string ToString() {
            return string.Format("POIIndex {0}, SubdivisionNumber {1}, SubType 0x{2:x}", PointIndexInRGN, SubdivisionNumberInRGN, SubType);
         }
      }

      /// <summary>
      /// Datensatz für die Tabelle der zusätzlichen POI-Daten
      /// </summary>
      public class PointDataRecord : BinaryReaderWriter.DataStruct {

         [Flags]
         public enum SpecPropFlags : byte {
            StreetNumberEncoded = 0x01,
            PhoneNumberEncoded = 0x02,
            ShortCityIndex = 0x10,
            ShortZipIndex = 0x20,
            ShortExitHighwayIndex = 0x40,
            ShortExitIndex = 0x80,
         }

         /// <summary>
         /// 3-Byte-Daten
         /// </summary>
         UInt32 _data;
         SpecPropFlags _SpecPropFlags;
         public POIFlags _internalPropMask { get; private set; }
         public byte[]? _streetnumber_encoded { get; private set; }
         UInt32 _streetnumberoffset;
         UInt32 _streetoffset;
         UInt16 _cityindex;
         UInt16 _zipindex;
         public byte[]? _phonenumber_encoded { get; private set; }
         UInt32 _phonenumberoffset;
         UInt32 _ExitOffset;
         UInt16 _ExitHighwayIndex;
         UInt16 _ExitIndex;


         /// <summary>
         /// Offset für die Namensliste (3 Byte, aber nur Bit 0..22, d.h. max. 0x7FFFFF)
         /// </summary>
         public UInt32 TextOffset {
            get {
               return _data & 0x7FFFFF;         // Bit 0..22
            }
            set {
               _data = (_data & 0x800000) | (value & 0x7FFFFF);
            }
         }

         /// <summary>
         /// ein Hausnummernoffset wird gesetzt oder geliefert (<see cref="StreetNumber"/> ist dann ungültig)
         /// </summary>
         public UInt32 StreetNumberOffset {
            get {
               return StreetNumberIsSet && !StreetNumberIsCoded ? _streetnumberoffset : UInt32.MaxValue;
            }
            set {
               if (value == UInt32.MaxValue)
                  StreetNumberIsSet = false;
               else {
                  StreetNumberIsSet = true;
                  StreetNumberIsCoded = false;
                  _streetnumberoffset = value & 0xFFFFFF;
                  _streetnumber_encoded = null;
               }
            }
         }

         /// <summary>
         /// eine intern codierte Hausnummer wird gesetzt oder geliefert (<see cref="StreetNumberOffset"/> ist dann ungültig)
         /// </summary>
         public string? StreetNumber {
            get {
               if (StreetNumberIsCoded && _streetnumber_encoded != null) {
                  return DecodeString11(_streetnumber_encoded);
               }
               return null;
            }
            set {
               if (value != null) {
                  StreetNumberIsSet = true;
                  StreetNumberIsCoded = true;
                  _streetnumber_encoded = EncodeString11(value);
               } else {
                  StreetNumberIsSet = false;
                  _streetnumber_encoded = null;
               }
            }
         }

         /// <summary>
         /// ein Offset für den Straßennamen wird gesetzt oder geliefert
         /// </summary>
         public UInt32 StreetOffset {
            get {
               return StreetIsSet ? _streetoffset : UInt32.MaxValue;
            }
            set {
               if (value == UInt32.MaxValue)
                  StreetIsSet = false;
               else {
                  StreetIsSet = true;
                  _streetoffset = (UInt32)(value & 0xFFFFFF);
               }
            }
         }

         /// <summary>
         /// ein Index für den Städtenamen wird gesetzt oder geliefert (1basiert ?)
         /// </summary>
         public UInt16 CityIndex {
            get {
               return CityIsSet ?
                           (UseShortCityIndex ? (UInt16)(_cityindex & 0xFF) : _cityindex) :
                           UInt16.MaxValue;
            }
            set {
               if (value != UInt16.MaxValue) {
                  CityIsSet = true;
                  if (UseShortCityIndex)
                     _cityindex = (UInt16)(value & 0xFF);
                  else
                     _cityindex = value;
               } else
                  CityIsSet = false;
            }
         }

         /// <summary>
         /// ein Index für den Städtenamen wird gesetzt oder geliefert (1basiert ?)
         /// </summary>
         public UInt16 ZipIndex {
            get {
               return ZipIsSet ?
                           (UseShortZipIndex ? (UInt16)(_zipindex & 0xFF) : _zipindex) :
                           UInt16.MaxValue;
            }
            set {
               if (value != UInt16.MaxValue) {
                  ZipIsSet = true;
                  if (UseShortZipIndex)
                     _zipindex = (UInt16)(value & 0xFF);
                  else
                     _zipindex = value;
               } else
                  ZipIsSet = false;
            }
         }

         /// <summary>
         /// ein Telefonnummernoffset wird gesetz oder geliefert (<see cref="PhoneNumber"/> ist dann ungültig)
         /// </summary>
         public UInt32 PhoneNumberOffset {
            get {
               return PhoneIsSet && !PhoneNumberIsCoded ? _phonenumberoffset : UInt32.MaxValue;
            }
            set {
               if (value == UInt32.MaxValue)
                  PhoneIsSet = false;
               else {
                  PhoneIsSet = true;
                  PhoneNumberIsCoded = false;
                  _phonenumberoffset = value & 0xFFFFFF;
                  _phonenumber_encoded = null;
               }
            }
         }

         /// <summary>
         /// eine intern codierte Telefonnummer wird gesetz oder geliefert (<see cref="PhoneNumberOffset"/> ist dann ungültig)
         /// </summary>
         public string? PhoneNumber {
            get {
               if (PhoneNumberIsCoded && _phonenumber_encoded != null) {
                  return DecodeString11(_phonenumber_encoded);
               }
               return null;
            }
            set {
               if (value != null) {
                  PhoneIsSet = true;
                  PhoneNumberIsCoded = true;
                  _phonenumber_encoded = EncodeString11(value);
               } else {
                  PhoneIsSet = false;
                  _phonenumber_encoded = null;
               }
            }
         }

         /// <summary>
         /// ein Index für den Straßennamen aus der <see cref="HighwayWithExitRecord"/>-Tabelle der Abfahrt wird gesetzt oder geliefert
         /// </summary>
         public UInt16 ExitHighwayIndex {
            get {
               return ExitIsSet ?
                           (UseShortExitHighwayIndex ? (UInt16)(_ExitHighwayIndex & 0xFF) : _ExitHighwayIndex) :
                           UInt16.MaxValue;
            }
            set {
               if (value != UInt16.MaxValue) {
                  ExitIsSet = true;
                  if (UseShortExitHighwayIndex)
                     _ExitHighwayIndex = (UInt16)(value & 0xFF);
                  else
                     _ExitHighwayIndex = value;
               } else
                  ExitIsSet = false;
            }
         }

         #region NICHT GETESTET

         /// <summary>
         /// der Offset für die Abfahrt wird gesetzt oder geliefert
         /// </summary>
         public UInt16 ExitOffset {
            get {
               if (ExitIsSet)
                  return (UInt16)(_ExitOffset & 0x7FFFFF);
               else
                  return UInt16.MaxValue;
            }
            set {
               if (value != UInt16.MaxValue) {
                  ExitIsSet = true;
                  _ExitOffset = (UInt16)(value & 0x7FFFFF);
               } else
                  ExitIsSet = false;
            }
         }

         /// <summary>
         /// ein Index der Abfahrt wird gesetzt oder geliefert
         /// </summary>
         public UInt16 ExitIndex {
            get {
               return ExitIndexIsSet ?
                           (UseShortExitIndex ? (UInt16)(_ExitIndex & 0xFF) : _ExitIndex) :
                           UInt16.MaxValue;
            }
            set {
               if (value != UInt16.MaxValue) {
                  ExitIsSet = true;
                  ExitIndexIsSet = true;
                  if (UseShortExitIndex)
                     _ExitIndex = (UInt16)(value & 0xFF);
                  else
                     _ExitIndex = value;
               } else
                  ExitIndexIsSet = false;
            }
         }

         #endregion

         /// <summary>
         /// Ist die Hausnummer gesetzt?
         /// </summary>
         public bool StreetNumberIsSet {
            get {
               return (_internalPropMask & POIFlags.street_num) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.street_num;
               else
                  _internalPropMask &= ~POIFlags.street_num;
            }
         }
         /// <summary>
         /// Ist der Straßenname gesetzt?
         /// </summary>
         public bool StreetIsSet {
            get {
               return (_internalPropMask & POIFlags.street) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.street;
               else
                  _internalPropMask &= ~POIFlags.street;
            }
         }
         /// <summary>
         /// Ist die Stadt gesetzt?
         /// </summary>
         public bool CityIsSet {
            get {
               return (_internalPropMask & POIFlags.city) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.city;
               else
                  _internalPropMask &= ~POIFlags.city;
            }
         }
         /// <summary>
         /// Ist die Postleitzahl gesetzt?
         /// </summary>
         public bool ZipIsSet {
            get {
               return (_internalPropMask & POIFlags.zip) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.zip;
               else
                  _internalPropMask &= ~POIFlags.zip;
            }
         }
         /// <summary>
         /// Ist die Telefonnummer gesetzt?
         /// </summary>
         public bool PhoneIsSet {
            get {
               return (_internalPropMask & POIFlags.phone) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.phone;
               else
                  _internalPropMask &= ~POIFlags.phone;
            }
         }
         /// <summary>
         /// Ist die Abfahrt gesetzt?
         /// </summary>
         public bool ExitIsSet {
            get {
               return (_internalPropMask & POIFlags.exit) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.exit;
               else
                  _internalPropMask &= ~POIFlags.exit;
            }
         }
         /// <summary>
         /// Ist der Abfahrtindex gesetzt?
         /// </summary>
         public bool ExitIndexIsSet {
            get {
               return ExitIsSet && (_ExitOffset & 0x800000) != 0;
            }
            private set {
               if (value)
                  _ExitOffset |= 0x800000;
               else
                  _ExitOffset &= 0x7FFFFF;
            }
         }
         /// <summary>
         /// Ist ... gesetzt?
         /// </summary>
         public bool TidePredictionIsSet {
            get {
               return (_internalPropMask & POIFlags.tide_prediction) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.tide_prediction;
               else
                  _internalPropMask &= ~POIFlags.tide_prediction;
            }
         }
         /// <summary>
         /// Ist ... gesetzt?
         /// </summary>
         public bool UnknownIsSet {
            get {
               return (_internalPropMask & POIFlags.unknown) != 0;
            }
            private set {
               if (value)
                  _internalPropMask |= POIFlags.unknown;
               else
                  _internalPropMask &= ~POIFlags.unknown;
            }
         }
         /// <summary>
         /// Ist die Hausnummer gesetzt und intern codiert?
         /// </summary>
         public bool StreetNumberIsCoded {
            get {
               return StreetNumberIsSet && (_SpecPropFlags & SpecPropFlags.StreetNumberEncoded) != 0;
            }
            private set {
               if (value)
                  _SpecPropFlags |= SpecPropFlags.StreetNumberEncoded;
               else
                  _SpecPropFlags &= ~SpecPropFlags.StreetNumberEncoded;
            }
         }
         /// <summary>
         /// Ist die Telefonnummer gesetzt und intern codiert?
         /// </summary>
         public bool PhoneNumberIsCoded {
            get {
               return PhoneIsSet && (_SpecPropFlags & SpecPropFlags.PhoneNumberEncoded) != 0;
            }
            private set {
               if (value)
                  _SpecPropFlags |= SpecPropFlags.PhoneNumberEncoded;
               else
                  _SpecPropFlags &= ~SpecPropFlags.PhoneNumberEncoded;
            }
         }

         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für Städte verwendet wird
         /// </summary>
         public bool UseShortCityIndex {
            get {
               return (_SpecPropFlags & SpecPropFlags.ShortCityIndex) != 0;
            }
            set {
               if (value)
                  _SpecPropFlags |= SpecPropFlags.ShortCityIndex;
               else
                  _SpecPropFlags &= ~SpecPropFlags.ShortCityIndex;
            }
         }
         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für Postleitzahlen verwendet wird
         /// </summary>
         public bool UseShortZipIndex {
            get {
               return (_SpecPropFlags & SpecPropFlags.ShortZipIndex) != 0;
            }
            set {
               if (value)
                  _SpecPropFlags |= SpecPropFlags.ShortZipIndex;
               else
                  _SpecPropFlags &= ~SpecPropFlags.ShortZipIndex;
            }
         }
         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für die Abfahrt-Straße verwendet wird
         /// </summary>
         public bool UseShortExitHighwayIndex {
            get {
               return (_SpecPropFlags & SpecPropFlags.ShortExitHighwayIndex) != 0;
            }
            set {
               if (value)
                  _SpecPropFlags |= SpecPropFlags.ShortExitHighwayIndex;
               else
                  _SpecPropFlags &= ~SpecPropFlags.ShortExitHighwayIndex;
            }
         }
         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für die Abfahrt verwendet wird
         /// </summary>
         public bool UseShortExitIndex {
            get {
               return (_SpecPropFlags & SpecPropFlags.ShortExitIndex) != 0;
            }
            set {
               if (value)
                  _SpecPropFlags |= SpecPropFlags.ShortExitIndex;
               else
                  _SpecPropFlags &= ~SpecPropFlags.ShortExitIndex;
            }
         }

         /// <summary>
         /// liefert die Datensatzlänge in Bytes
         /// </summary>
         public uint DataLength(POIFlags POIGlobalFlags) {
            int len = 3;
            if (POIGlobalFlags != _internalPropMask)
               len += 1;
            if (StreetNumberIsSet && _streetnumber_encoded != null)
               len += StreetNumberIsCoded ? _streetnumber_encoded.Length : 3;
            if (StreetIsSet)
               len += 3;
            if (CityIsSet)
               len += UseShortCityIndex ? 1 : 2;
            if (ZipIsSet)
               len += UseShortZipIndex ? 1 : 2;
            if (PhoneIsSet && _phonenumber_encoded != null)
               len += PhoneNumberIsCoded ? _phonenumber_encoded.Length : 3;
            if (ExitIsSet) {
               len += 3;
               len += UseShortExitHighwayIndex ? 1 : 2;
               if (ExitIndexIsSet)
                  len += UseShortExitIndex ? 1 : 2;
            }
            return (uint)len;
         }

         /// <summary>
         /// Ex. eine eigene Flag-Maske? (Bit 23 von <see cref="TextOffset"/>)
         /// </summary>
         public bool HasLocalProperties {
            get {
               return (_data & 0x800000) != 0;     // Bit 23
            }
            set {
               if (value)
                  _data |= 0x800000;
               else
                  _data &= 0x7FFFFF;
            }
         }

         /// <summary>
         /// "komprimiert" die lokale Maske für die Speicherung
         /// </summary>
         /// <param name="POIGlobalFlags"></param>
         /// <param name="POILocalFlags"></param>
         /// <returns></returns>
         byte CompressedPOIFlags(POIFlags POIGlobalFlags, POIFlags POILocalFlags) {
            int flag = 0;
            int j = 0;
            /* the local POI flag is really tricky if a bit is not set in the global mask
               we have to skip this bit in the local mask. In other words the meaning of the local bits
               change influenced by the global bits */
            for (byte i = 0; i < 6; i++) {               // nur Bits 0..5 werden verwendet
               int mask = 1 << i;
               if ((mask & (int)POIGlobalFlags) == mask) {
                  if ((mask & (int)POILocalFlags) == mask)
                     flag |= (1 << j);
                  j++;
               }
            }
            flag |= 0x80; // gpsmapedit asserts for this bit set
            return (byte)flag;
         }

         /// <summary>
         /// "entkomprimiert" die gespeicherte lokale Maske
         /// </summary>
         /// <param name="POIGlobalFlags"></param>
         /// <param name="POILocalFlags"></param>
         /// <returns></returns>
         POIFlags DecompressedPOIFlags(POIFlags POIGlobalFlags, POIFlags POILocalFlags) {
            int flag = 0;
            int j = 0;
            for (byte i = 0; i < 6; i++) {               // nur Bits 0..5 werden verwendet
               int mask = 1 << i;
               if ((mask & (int)POIGlobalFlags) == mask) {
                  int mask2 = 1 << j;
                  if ((mask2 & (int)POILocalFlags) == mask2)
                     flag |= mask;
                  j++;
               }
            }
            return (POIFlags)flag;
         }


         /// <summary>
         /// liefert den Text zu <see cref="TextOffset"/>
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns>null, wenn der Offset ungültig ist</returns>
         public string? GetText(StdFile_LBL lbl, bool withctrl) {
            return lbl.GetText(TextOffset, withctrl);
         }


         public PointDataRecord() {
            _data = 0;
            _internalPropMask = 0;
            _SpecPropFlags = 0;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="br"></param>
         /// <param name="extdata">Bit 0..7 globale POIFlags, Bit 8..15 SpecPropFlags</param>
         public override void Read(BinaryReaderWriter br, object? extdata) {
            if (extdata != null) {
               UInt16 dat = (UInt16)extdata;
               POIFlags POIGlobalFlags = (POIFlags)(0xFF & dat);
               _SpecPropFlags = (SpecPropFlags)(0xFF & (dat >> 8));

               _data = br.Read3AsUInt();

               if (HasLocalProperties)
                  _internalPropMask = DecompressedPOIFlags(POIGlobalFlags, (POIFlags)br.ReadByte());
               else
                  _internalPropMask = POIGlobalFlags;

               if (StreetNumberIsSet) {
                  byte v = br.ReadByte();
                  if ((v & 0x80) != 0) {        // vom 1. Byte mit gesetztem 7. Bit bis zum nächsten Byte mit gesetztem 7. Bit
                     List<byte> lst = new List<byte> {
                     v
                  };
                     do {
                        v = br.ReadByte();
                        lst.Add(v);
                     } while ((v & 0x80) == 0);
                     StreetNumber = DecodeString11(lst);
                  } else {
                     br.Seek(-1, SeekOrigin.Current);
                     StreetNumberOffset = br.Read3AsUInt();
                  }
               }

               if (StreetIsSet)
                  StreetOffset = br.Read3AsUInt();

               if (CityIsSet)
                  if (UseShortCityIndex)
                     _cityindex = br.ReadByte();
                  else
                     _cityindex = br.Read2AsUShort();

               if (ZipIsSet)
                  if (UseShortZipIndex)
                     _zipindex = br.ReadByte();
                  else
                     _zipindex = br.Read2AsUShort();

               if (PhoneIsSet) {
                  byte v = br.ReadByte();
                  if ((v & 0x80) == 0x80) {
                     List<byte> lst = new List<byte> {
                     v
                  };
                     do {
                        v = br.ReadByte();
                        lst.Add(v);
                     } while ((v & 0x80) == 0);
                     PhoneNumber = DecodeString11(lst);
                  } else {
                     br.Seek(-1, SeekOrigin.Current);
                     PhoneNumberOffset = br.Read3AsUInt();
                  }
               }

               if (ExitIsSet) {
                  _ExitOffset = br.Read3AsUInt();

                  /*    3 Byte
                   *    Bit 0 .. 21 für Index
                   *    Bit 22      OvernightParking
                   *    Bit 23      facilities defined
                   *    n Bytes     highwayIndex (n abh. von der max. Anzahl der Highways)
                   *    n Bytes     exitFacilityIndex (n abh. von der max. Anzahl der ExitFacility)
                   */

                  if (UseShortExitHighwayIndex)
                     _ExitHighwayIndex = br.ReadByte();
                  else
                     _ExitHighwayIndex = br.Read2AsUShort();

                  if (ExitIndexIsSet) {
                     if (UseShortExitIndex)
                        _ExitIndex = br.ReadByte();
                     else
                        _ExitIndex = br.Read2AsUShort();
                  }
               }

               if ((_internalPropMask & POIFlags.tide_prediction) != 0)
                  throw new Exception("Die Behandlung von Bit 6 in der POI-Maske ist noch nicht implementiert.");

               if ((_internalPropMask & POIFlags.unknown) != 0)
                  throw new Exception("Die Behandlung von Bit 7 in der POI-Maske ist noch nicht implementiert.");
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="bw"></param>
         /// <param name="extdata">globale POIFlags</param>
         public override void Write(BinaryReaderWriter bw, object? extdata) {
            if (extdata != null) {
               POIFlags POIGlobalFlags = (POIFlags)((byte)extdata);

               bw.Write3(_data);

               if (_internalPropMask != POIGlobalFlags)
                  bw.Write(CompressedPOIFlags(POIGlobalFlags, _internalPropMask));

               if (StreetNumberIsSet)
                  if (StreetNumberIsCoded && _streetnumber_encoded != null)
                     bw.Write(_streetnumber_encoded);
                  else
                     bw.Write3(_streetnumberoffset);

               if (StreetIsSet)
                  bw.Write3(StreetOffset);

               if (CityIsSet)
                  if (UseShortCityIndex)
                     bw.Write((byte)CityIndex);
                  else
                     bw.Write(CityIndex);

               if (ZipIsSet)
                  if (UseShortZipIndex)
                     bw.Write((byte)ZipIndex);
                  else
                     bw.Write(ZipIndex);

               if (PhoneIsSet)
                  if (PhoneNumberIsCoded && _phonenumber_encoded != null)
                     bw.Write(_phonenumber_encoded);
                  else
                     bw.Write3(_phonenumberoffset);


               if (ExitIsSet) {
                  // 3 Byte val (Bit 23: ex. Exit-facilities; Bit 22: OvernightParking, Bit 0..21: Exit description label)
                  // 2 oder 3 Byte Highway-Index (der globalen Index)
                  // 1 oder 2 Byte Exit-facilitie-Index (der globalen Index)

                  bw.Write3(_ExitOffset);

                  if (UseShortExitHighwayIndex)
                     bw.Write((byte)ExitHighwayIndex);
                  else
                     bw.Write(ExitHighwayIndex);

                  if (ExitIndexIsSet)
                     if (UseShortExitIndex)
                        bw.Write((byte)ExitIndex);
                     else
                        bw.Write(ExitIndex);

               }

               if ((_internalPropMask & POIFlags.tide_prediction) != 0)
                  throw new Exception("Die Behandlung von Bit 6 in der POI-Maske ist noch nicht implementiert.");

               if ((_internalPropMask & POIFlags.unknown) != 0)
                  throw new Exception("Die Behandlung von Bit 7 in der POI-Maske ist noch nicht implementiert.");

            }
         }

         #region Encodierung/Codierung für Haus- und Telefonnummern

         /* Codierung:
          * Es sind nur die Ziffern 0..9 und das Zeichen '-' erlaubt.
          * Die Ziffern 0..9 werden mit den Zahlen 0..9 codiert, '-' als 10.
          * Jedes Byte enthält 2 Zeichen: 11 * z1 + z2. Damit wird der Wert in einem Byte max. 120 = 0x78. Das 7. Bit bleibt also immer unbenutzt und wird
          * deshalb als Anfangs- und Endekennung verwendet.
          * Wird eine ungerade Anzahl von Zeichen codiert, wird am Ende noch der Wert 10 angefügt. Er wird an dieser Stelle aber als ungültig gewertet.
          * Da Start- und Endekennung (Bit 7) nicht unterschieden werden können, muss der codierte Wert immer min. 2 Byte umfassen.
          */

         /// <summary>
         /// dekodiert die Bytefolge
         /// </summary>
         /// <param name="array"></param>
         /// <returns></returns>
         string DecodeString11(IList<byte> array) {
            if (array == null || array.Count == 0)
               return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Count; i++) {
               byte b = (byte)(array[i] & 0x7F);
               int v1 = b / 11;
               int v2 = b % 11;
               if (0 <= v1 && v1 <= 10) {
                  sb.Append(DecodeChar11(v1));
                  sb.Append(DecodeChar11(v2));
               }
            }
            return sb.ToString().TrimEnd('-');
         }

         /// <summary>
         /// Encode a string as base 11.
         /// </summary>
         /// <param name="str"></param>
         /// <returns>If the string is not all numeric (or A) then null is returned.</returns>
         byte[]? EncodeString11(String str) {
            // remove surrounding whitespace to increase chance for simple encoding
            String number = str.Trim();

            byte[] encodedNumber = new byte[(number.Length / 2) + 2];

            int i = 0;
            int j = 0;
            while (i < number.Length) {

               int c1 = EncodeChar11(number[i++]);

               int c2;
               if (i < number.Length)
                  c2 = EncodeChar11(number[i++]);
               else
                  c2 = 10;

               // Only 0-9 and - allowed
               if (c1 < 0 || c1 > 10 || c2 < 0 || c2 > 10)
                  return null;

               // Encode as base 11
               int val = c1 * 11 + c2;

               // first byte needs special marking with 0x80
               // If this is not set would be treated as label pointer
               if (j == 0)
                  val |= 0x80;

               encodedNumber[j++] = (byte)val;
            }
            if (j == 0)
               return null;

            if (j == 1)
               encodedNumber[j++] = (byte)0xf8;
            else
               encodedNumber[j - 1] |= 0x80;

            byte[] buff = new byte[j];
            for (i = 0; i < j; i++)
               buff[i] = encodedNumber[i];

            return buff;
         }

         /// <summary>
         /// Convert the characters '0' to '9' and '-' to a number 0-10 (base 11). 
         /// </summary>
         /// <param name="ch"></param>
         /// <returns></returns>
         int EncodeChar11(char ch) {
            if ('0' <= ch && ch <= '9')
               return (ch - '0');
            if (ch == '-')
               return 10;
            return -1;
         }

         string DecodeChar11(int v) {
            switch (v) {
               case 0: return "0";
               case 1: return "1";
               case 2: return "2";
               case 3: return "3";
               case 4: return "4";
               case 5: return "5";
               case 6: return "6";
               case 7: return "7";
               case 8: return "8";
               case 9: return "9";
               default: return "-";
            }
         }

         #endregion

         public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("TextOffset {0}", TextOffset);

            if (StreetIsSet)
               sb.AppendFormat(", StreetOffset {0}", StreetOffset);

            if (StreetNumberIsSet)
               if (StreetNumberIsCoded)
                  sb.AppendFormat(", StreetNumber {0}", StreetNumber);
               else
                  sb.AppendFormat(", StreetNumOffset {0}", StreetNumberOffset);

            if (CityIsSet)
               sb.AppendFormat(", CityIndex {0}", CityIndex);

            if (ZipIsSet)
               sb.AppendFormat(", ZipIndex {0}", ZipIndex);

            if (PhoneIsSet)
               if (PhoneNumberIsCoded)
                  sb.AppendFormat(", PhoneNumber {0}", PhoneNumber);
               else
                  sb.AppendFormat(", PhoneNumberOffset {0}", PhoneNumberOffset);

            if (ExitIndexIsSet) {
               sb.AppendFormat(", ExitOffset {0}", ExitOffset);
               sb.AppendFormat(", ExitHighwayIndex {0}", ExitHighwayIndex);
               if (ExitIndexIsSet)
                  sb.AppendFormat(", ExitIndex {0}", ExitIndex);
            }

            return sb.ToString();
         }

      }

      /// <summary>
      /// Datensatz in der Tabelle Motorway-Exit-Facilities
      /// </summary>
      public class ExitRecord : BinaryReaderWriter.DataStruct {

         UInt32 _data;

         public byte Facilities;

         /// <summary>
         /// Offset für die Namensliste
         /// </summary>
         public UInt32 TextOffsetInLBL {
            get {
               return _data & 0x3fffff;      // Bit 0..21
            }
            set {
               _data = (value & 0x3fffff) | (_data & 0xFFC00000);
            }
         }

         /// <summary>
         /// last facility for this exit
         /// </summary>
         public bool LastFacilitie {
            get {
               return (_data & 0x800000) != 0;     // Bit 23
            }
            set {
               if (value)
                  _data |= 0x800000;
               else
                  _data &= 0x7FFFFF;
            }
         }

         /// <summary>
         /// nur 4 Bit
         /// </summary>
         public byte Type {
            get {
               return (byte)((_data >> 24) & 0xF);    // Bit 24..27
            }
            set {
               _data = (_data & 0xF0FFFFFF) | ((UInt32)value << 24);
            }
         }

         /// <summary>
         /// nur 3 Bit
         /// </summary>
         public byte Direction {
            get {
               return (byte)((_data >> 29) & 0x7);    // Bit 29..31
            }
            set {
               _data = (_data & 0x1FFFFFFF) | ((UInt32)value << 29);
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 5;


         public ExitRecord() {
            _data = 0;
            Facilities = 0;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            _data = br.Read4UInt();
            Facilities = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write(_data);
            bw.Write(Facilities);
         }

         public override string ToString() {
            return string.Format("Offset {0}, LastFacilitie {1}, Type 0x{2:x}, Direction 0x{3:x}",
               TextOffsetInLBL,
               LastFacilitie,
               Type,
               Direction);
         }

      }

      /// <summary>
      /// Datensatz in der PLZ-Tabelle
      /// </summary>
      public class ZipRecord : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Offset für die Namensliste
         /// </summary>
         public UInt32 TextOffsetInLBL;

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 3;

         public ZipRecord()
            : this(0) { }

         public ZipRecord(UInt32 offset) {
            TextOffsetInLBL = offset;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            TextOffsetInLBL = br.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write3(TextOffsetInLBL);
         }

         /// <summary>
         /// liefert den Text für die PLZ oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetText(StdFile_LBL lbl, bool withctrl) {
            return lbl.GetText(TextOffsetInLBL, withctrl);
         }

         public override string ToString() {
            return string.Format("Offset {0}", TextOffsetInLBL);
         }

      }

      /// <summary>
      /// Datensatz in der Städte-Tabelle für eine Stadt und eine Region bzw. ein Land (Verweis in die <see cref="CountryRecord"/>- oder <see cref="RegionAndCountryRecord"/>-Tabelle; 5 Byte)
      /// </summary>
      public class CityAndRegionOrCountryRecord : BinaryReaderWriter.DataStruct {

         // Die Daten verweisen entweder als Offset in den Textbereich der LBL-Datei oder als Subdivisionnummer und Punktindex auf einen Punkt der RGN-Datei.

         UInt32 _Data;

         /// <summary>
         /// Offset für die Namensliste; nur gültig wenn <see cref="IsPointInRGN"/> false ist
         /// </summary>
         public UInt32 TextOffsetInLBL {
            get {
               return IsPointInRGN ? 0 : _Data;
            }
            set {
               _Data = value & 0xffffff;
               IsPointInRGN = false;
            }
         }

         /// <summary>
         /// nur gültig wenn <see cref="IsPointInRGN"/> true ist (Bit 0..7)
         /// </summary>
         public byte PointIndexInRGN {
            get {
               return (byte)(IsPointInRGN ? _Data & 0xff : 0);
            }
            set {
               _Data = ((UInt32)value << 16) | (_Data & 0xffff);
               IsPointInRGN = true;
            }
         }

         /// <summary>
         /// nur gültig wenn <see cref="IsPointInRGN"/> true ist (Bit 8..23)
         /// </summary>
         public UInt16 SubdivisionNumberInRGN {
            get {
               return (UInt16)(IsPointInRGN ? (_Data >> 8) & 0xffff : 0);
            }
            set {
               _Data = (_Data & 0xff0000) | (UInt32)value;
               IsPointInRGN = true;
            }
         }

         /// <summary>
         /// Bit 0..13 für den Region/Country-Index, Bit 14 als Flag für Region/Country, Bit 15 als Flag für POI oder Text-Offset
         /// </summary>
         UInt16 _Info;

         /// <summary>
         /// setzt oder liefert ob entweder <see cref="PointIndexInRGN"/> und <see cref="SubdivisionNumberInRGN"/> oder <see cref="TextOffsetInLBL"/> gültig ist
         /// </summary>
         public bool IsPointInRGN {
            get {
               return Bit.IsSet(_Info, 15);
            }
            set {
               Bit.Set(_Info, 15, value);
            }
         }

         /// <summary>
         /// setzt oder liefert ob <see cref="RegionOrCountryIndex"/> einen Region- oder Country-Index liefert (Bit 14)
         /// </summary>
         public bool IsCountry {
            get {
               return Bit.IsSet(_Info, 14);
            }
            set {
               Bit.Set(_Info, 14, value);
            }
         }

         /// <summary>
         /// liefert oder setzt Werte von 0 .. 0x3FFF (Bit 0 .. 13)
         /// </summary>
         public UInt16 RegionOrCountryIndex {
            get {
               return (UInt16)(_Info & 0x3fff);
            }
            set {
               _Info = (UInt16)((_Info & 0xc000) & (value & 0x3fff));
            }
         }

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 5;

         public CityAndRegionOrCountryRecord() {
            _Data = 0;
            _Info = 0;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            _Data = br.Read3AsUInt();
            _Info = br.Read2AsUShort();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write3(_Data);
            bw.Write(_Info);
         }

         /// <summary>
         /// liefert den Text für die Stadt
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="rgn">nur nötig, wenn <see cref="IsPointInRGN"/> gesetzt ist</param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetCityText(StdFile_LBL lbl, StdFile_RGN rgn, bool withctrl) {
            if (IsPointInRGN) {
               if (rgn != null) {
                  StdFile_RGN.RawPointData? pd = rgn.GetPoint1(SubdivisionNumberInRGN + 1, PointIndexInRGN);       // 'SubdivisionNumberInRGN + 1' ?????
                  if (!(pd is null))      // wegen Überladung '!=' diese ungewöhnliche Bedingung
                     return pd.GetText(lbl, withctrl);
               } else
                  throw new Exception("GetCityText: IsPointInRGN but rgn is null");
            } else
               return lbl.GetText(TextOffsetInLBL, withctrl);
            return null;
         }

         /// <summary>
         /// liefert den Text für die Region
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetRegionText(StdFile_LBL lbl, bool withctrl) {
            return !IsCountry ?
                        lbl.GetRegionText_FromRegionAndCountryList(RegionOrCountryIndex - 1, withctrl) :
                        null;
         }

         /// <summary>
         /// liefert den Text für das Land
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetCountryText(StdFile_LBL lbl, bool withctrl) {
            if (!IsCountry) // dann Umweg über Region
               return lbl.GetCountryText_FromRegionAndCountryList(RegionOrCountryIndex - 1, withctrl);
            else
               return lbl.GetCountryText_FromCountryList(RegionOrCountryIndex - 1, withctrl); // 1.basierter Index !
         }


         public override string ToString() {
            return IsPointInRGN ?
               string.Format("RegionOrCountryIndex {2}, RegionIsCountry {3}, POIIndex {0}, SubdivisionNumber {1}", PointIndexInRGN, SubdivisionNumberInRGN, RegionOrCountryIndex, IsCountry) :
               string.Format("RegionOrCountryIndex {1}, RegionIsCountry {2}, TextOffset {0}", TextOffsetInLBL, RegionOrCountryIndex, IsCountry);
         }

      }

      /// <summary>
      /// Datensatz in der Regionen-Tabelle (mit Verweis in die <see cref="CountryRecord"/>-Tabelle)
      /// </summary>
      public class RegionAndCountryRecord : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Index in die Landesliste (1-basiert)
         /// </summary>
         public UInt16 CountryIndex;

         /// <summary>
         /// Offset für die Namensliste
         /// </summary>
         public UInt32 TextOffset;

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 5;

         public RegionAndCountryRecord()
            : this(0, 0) { }

         public RegionAndCountryRecord(UInt16 countryIndex, UInt32 offset) {
            CountryIndex = countryIndex;
            TextOffset = offset;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            CountryIndex = br.Read2AsUShort();
            TextOffset = br.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write(CountryIndex);
            bw.Write3(TextOffset);
         }

         /// <summary>
         /// liefert den Text der Region oder null (Offset 0 liefert i.A. null)
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetRegionText(StdFile_LBL lbl, bool withctrl) {
            if (TextOffset > 0)
               return lbl.GetText(TextOffset, withctrl);
            return null;
         }

         /// <summary>
         /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null)
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetCountryText(StdFile_LBL lbl, bool withctrl) {
            return lbl.GetCountryText_FromCountryList(CountryIndex - 1, withctrl); // 1.basierter Index !
         }

         public override string ToString() {
            return string.Format("CountryIndex {0}, Offset {1}", CountryIndex, TextOffset);
         }

      }

      /// <summary>
      /// Datensatz in der Ländertabelle
      /// </summary>
      public class CountryRecord : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Offset für die Namensliste
         /// </summary>
         public UInt32 TextOffsetInLBL;

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 3;

         public CountryRecord()
            : this(0) { }

         public CountryRecord(UInt32 offset) {
            TextOffsetInLBL = offset;
         }

         public override void Read(BinaryReaderWriter br, object? extdata) {
            TextOffsetInLBL = br.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            bw.Write3(TextOffsetInLBL);
         }

         /// <summary>
         /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null)
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetText(StdFile_LBL lbl, bool withctrl) {
            if (TextOffsetInLBL > 0)
               return lbl.GetText(TextOffsetInLBL, withctrl);
            return null;
         }

         public override string ToString() {
            return string.Format("Offset {0}", TextOffsetInLBL);
         }

      }

      #endregion

      public abstract class TextBag {

         /// <summary>
         /// intern verwendeter Offset-Faktor (2er-Potenz)
         /// </summary>
         public int OffsetMultiplier { get; protected set; }

         /// <summary>
         /// Größe des benötigten Datenbereiches
         /// </summary>
         public int Length { get; protected set; }

         /// <summary>
         /// Codec um die Bytelänge des Textes zum Speichern zu ermitteln
         /// </summary>
         public LabelCodec Codec { get; protected set; }

         /// <summary>
         /// Anzahl der Texteinträge
         /// </summary>
         public virtual int Count {
            get {
               return 0;
            }
         }


         public TextBag(LabelCodec codec, int offsetMultiplier = 1) {
            OffsetMultiplier = 1;
            for (int i = 7; i >= 0; i--)        // höchste 2er-Potenz suchen und übernehmen
               if (((0x01 << i) & offsetMultiplier) != 0) {
                  OffsetMultiplier = 0x01 << i;
                  break;
               }
            Codec = codec;
         }

         public abstract void Clear();

         public abstract int Insert(string text, int realoffset = -1);

         /// <summary>
         /// liefert alle Offsets
         /// </summary>
         /// <returns></returns>
         public abstract int[] Offsets();

         /// <summary>
         /// liefert den Text zum Offset oder null
         /// </summary>
         /// <param name="offset"></param>
         /// <returns></returns>
         public abstract string? Text(int offset);

         /// <summary>
         /// liefert den als Bytefolge encodierten Text
         /// </summary>
         /// <param name="offset"></param>
         /// <returns></returns>
         public abstract byte[]? EncodedText(int offset);

         /// <summary>
         /// liefert den Offset zum Text oder -1
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public abstract int Offset(string text);

         public override string ToString() {
            return string.Format("Texte {0}, Multiplikator {1}, Datenlänge {2}, Codec {3}",
                                    Count,
                                    OffsetMultiplier,
                                    Length,
                                    Codec.ToString());
         }

      }

      /// <summary>
      /// zum Sammeln aller verwendeten Texte / Labels (nur aus der LBL gelesen und dann RO; nicht zum Encodieren geeignet)
      /// </summary>
      public class TextBagRO : TextBag {

         /// <summary>
         /// Anzahl der Texteinträge
         /// </summary>
         public override int Count {
            get {
               return frozendata != null ?
                           frozendata.Length :
                           0;
            }
         }

         public bool IsFrozen { get; protected set; }


         class Data : IComparable<Data> {
            public int Offset;
            public string? Text;

            public Data(int offset, string? text) {
               Offset = offset;
               Text = text;
            }

            public int CompareTo(Data? other) {
               if (other == null)
                  return 1;
               return Offset.CompareTo(other.Offset);
            }

            public override string ToString() {
               return Offset.ToString() + ": " + Text;
            }
         }

         /// <summary>
         /// temp. Liste zum Einsammeln der Daten
         /// </summary>
         List<Data> temptext;

         /// <summary>
         /// endgültiges sortiertes Datenarray nach dem "Einfrieren"
         /// </summary>
         Data[]? frozendata;


         /// <summary>
         /// erzeugt eine leere Textsammlung (nur für das einmalige Einlesen der Daten aus einer LBL-Datei)
         /// </summary>
         /// <param name="codec"></param>
         /// <param name="offsetMultiplier">Multiplikator (2er-Potenz, max. 128)</param>
         public TextBagRO(LabelCodec codec, int offsetMultiplier = 1) :
            base(codec, offsetMultiplier) {
            Length = 0;
            temptext = new List<Data>();
         }

         /// <summary>
         /// löscht den gesamten Dateninhalt
         /// </summary>
         public override void Clear() {
            freezeCheck();
            frozendata = Array.Empty<Data>();
            Length = 0;
         }


         /// <summary>
         /// fügt bei Bedarf den Text ein und liefert immer 0
         /// </summary>
         /// <param name="text"></param>
         /// <param name="realoffset">"echter" Offset (ohne Multiplikator); da sollte man sich sehr sicher sein</param>
         /// <returns>negativ, wenn die Liste voll ist</returns>
         public override int Insert(string text, int realoffset) {
            if (IsFrozen)
               throw new Exception("TextBagRO ist \"eingefroren\".");
            temptext.Add(new Data(realoffset, text));
            return 0;
         }

         /// <summary>
         /// liefert alle Offsets
         /// </summary>
         /// <returns></returns>
         public override int[] Offsets() {
            freezeCheck();
            int[] ret = new int[Count];
            if (frozendata != null)
               for (int i = 0; i < Count; i++)
                  ret[i] = frozendata[i].Offset;
            return ret;
         }

         /// <summary>
         /// liefert den Text zum Offset oder null
         /// </summary>
         /// <param name="offset"></param>
         /// <returns></returns>
         public override string? Text(int offset) {
            freezeCheck();
            int idx = frozendata != null ? Array.BinarySearch(frozendata, new Data(offset * OffsetMultiplier, null)) : -1;
            return idx >= 0 && frozendata != null ?
                     frozendata[idx].Text :
                     null;
         }

         /// <summary>
         /// liefert den als Bytefolge encodierten Text
         /// </summary>
         /// <param name="offset"></param>
         /// <returns></returns>
         public override byte[]? EncodedText(int offset) {
            freezeCheck();
            string? text = Text(offset);
            return text != null ?
                     Codec.Encode(text) :
                     null;
         }

         /// <summary>
         /// liefert den Offset zum Text oder -1
         /// <para>Langsam (lineare Suche).</para>
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public override int Offset(string text) {
            freezeCheck();
            if (frozendata != null)
               foreach (var item in frozendata) {
                  if (item.Text == text)
                     return item.Offset;
               }
            return -1;
         }

         void freezeCheck() {
            if (!IsFrozen)
               throw new Exception("TextBagRO ist noch nicht \"eingefroren\".");
         }

         /// <summary>
         /// danach kein <see cref="Insert"/>() mehr möglich
         /// </summary>
         public void Freeze() {
            IsFrozen = true;

            frozendata = new Data[temptext.Count];
            temptext.CopyTo(frozendata);
            temptext.Clear();
            Array.Sort(frozendata); // eigentlich nicht nötig, weil die Texte schon in der richtigen Reihenfolge eingelesen sein sollten
         }

      }

      /// <summary>
      /// zum Sammeln aller verwendeten Texte / Labels
      /// </summary>
      public class TextBagRW : TextBag {

         /// <summary>
         /// nächster (echter) Offset
         /// </summary>
         int nextoffset;
         /// <summary>
         /// Liste zum Suchen von Text zum Offset
         /// </summary>
         Dictionary<int, string>? textlist;
         /// <summary>
         /// Liste zum Suchen vom Offset zum Text
         /// </summary>
         Dictionary<string, int>? offsetlist;

         /// <summary>
         /// Anzahl der Texteinträge
         /// </summary>
         public override int Count => textlist != null ? textlist.Count : 0;


         /// <summary>
         /// erzeugt eine leere Textsammlung
         /// </summary>
         /// <param name="codec"></param>
         /// <param name="offsetMultiplier">Multiplikator (2er-Potenz, max. 128)</param>
         public TextBagRW(LabelCodec codec, int offsetMultiplier = 1) :
            base(codec, offsetMultiplier) {
            textlist = new Dictionary<int, string>();
            offsetlist = new Dictionary<string, int>();
            nextoffset = 0;
            Length = 0;
         }

         /// <summary>
         /// erzeugt eine Textsammlung durch die Übernahme aller Offsets und Texte aus der Liste
         /// <para>ACHTUNG: Die Offsets werden NICHT überprüft. Der Multiplikator muss korrekt sein.</para>
         /// </summary>
         /// <param name="codec"></param>
         /// <param name="offsetMultiplier"></param>
         /// <param name="LabeltextList"></param>
         public TextBagRW(LabelCodec codec, int offsetMultiplier, Dictionary<UInt32, string> LabeltextList)
            : this(codec, offsetMultiplier) {
            // alle Daten werden ohne Prüfung übernommen
            foreach (var item in LabeltextList) {
               int offset = (int)item.Key * offsetMultiplier;
               textlist?.Add(offset, item.Value);
               offsetlist?.Add(item.Value, offset);
               nextoffset = Math.Max(nextoffset, offset);
            }
            if (textlist != null && textlist.Count > 0)
               nextoffset += codec.Encode(textlist[nextoffset]).Length + 1;
         }

         /// <summary>
         /// erzeugt eine Kopie
         /// </summary>
         /// <param name="tb"></param>
         public TextBagRW(TextBagRW tb) :
            base(tb.Codec, tb.OffsetMultiplier) {
            nextoffset = tb.nextoffset;
            if (tb.textlist != null && textlist != null)
               foreach (var item in tb.textlist)
                  textlist.Add(item.Key, item.Value);
            if (tb.offsetlist != null && offsetlist != null)
               foreach (var item in tb.offsetlist)
                  offsetlist.Add(item.Key, item.Value);

         }

         /// <summary>
         /// löscht den gesamten Dateninhalt
         /// </summary>
         public override void Clear() {
            textlist?.Clear();
            offsetlist?.Clear();
            nextoffset = 0;
            Length = 0;
         }

         /// <summary>
         /// fügt bei Bedarf den Text ein und liefert den Offset
         /// </summary>
         /// <param name="text"></param>
         /// <param name="realoffset">"echter" Offset (ohne Multiplikator); da sollte man sich sehr sicher sein</param>
         /// <returns>negativ, wenn die Liste voll ist</returns>
         public override int Insert(string text, int realoffset = -1) {
            if (offsetlist != null &&
                offsetlist.TryGetValue(text, out int offset))
               return offset / OffsetMultiplier;

            int actoffset = realoffset >= 0 ? realoffset : nextoffset;
            if (!setNextOffset(text, actoffset))
               return -1;

            textlist?.Add(actoffset, text);
            offsetlist?.Add(text, actoffset);

            return actoffset / OffsetMultiplier;
         }

         bool setNextOffset(string text, int actoffset) {
            int len = Codec.Encode(text).Length;         // einschließlich abschließender 0
            int tmp_length = actoffset + len;
            int tmp_nextoffset = tmp_length;

            if (tmp_nextoffset % OffsetMultiplier != 0)
               tmp_nextoffset += OffsetMultiplier - tmp_nextoffset % OffsetMultiplier;      // nextoffset auf Vielfache des Multiplikators einstellen

            if (tmp_nextoffset / OffsetMultiplier > 0xFFFFFF)  // mehr als 3 Byte können extern nicht verwendet werden
               return false;
            Length = tmp_length;
            nextoffset = tmp_nextoffset;
            return true;
         }

         /// <summary>
         /// liefert alle Offsets
         /// </summary>
         /// <returns></returns>
         public override int[] Offsets() {
            if (textlist != null) {
               int[] offsets = new int[textlist.Count];
               textlist.Keys.CopyTo(offsets, 0);
               for (int i = 0; i < offsets.Length; i++)
                  offsets[i] /= OffsetMultiplier;
               return offsets;
            }
            return [];
         }
         /// <summary>
         /// liefert den Text zum Offset oder null
         /// </summary>
         /// <param name="offset"></param>
         /// <returns></returns>
         public override string? Text(int offset) => textlist != null &&
                                                     textlist.TryGetValue(offset * OffsetMultiplier, out string? text) ?
                                                            text :
                                                            null;

         /// <summary>
         /// liefert den als Bytefolge encodierten Text
         /// </summary>
         /// <param name="offset"></param>
         /// <returns></returns>
         public override byte[]? EncodedText(int offset) => textlist != null &&
                                                            textlist.TryGetValue(offset * OffsetMultiplier, out string? text) ?
                                                                  Codec.Encode(text) :
                                                                  null;

         /// <summary>
         /// liefert den Offset zum Text oder -1
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public override int Offset(string text) => offsetlist != null &&
                                                    offsetlist.TryGetValue(text, out int offset) ?
                                                                  offset / OffsetMultiplier :
                                                                  -1;


      }


      /// <summary>
      /// Multiplikator für Data-Offsets
      /// </summary>
      uint dataoffsetMultiplier = 1;
      /// <summary>
      /// Multiplikator für POI-Offsets
      /// </summary>
      uint poioffsetMultiplier = 1;

      /// <summary>
      /// interner Codec zum Codieren der Texte
      /// </summary>
      LabelCodec codec;

      #region Lesen und setzen der globalen POI-Flags

      [Flags]
      public enum POIFlags : byte {
         nothing = 0x00,
         street_num = 0x01,
         street = 0x02,
         city = 0x04,
         zip = 0x08,
         phone = 0x10,
         exit = 0x20,
         tide_prediction = 0x40,
         unknown = 0x80,
      }

      public bool POIGlobal_has_street_num {
         get {
            return (POIGlobalMask & POIFlags.street_num) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.street_num;
            else
               POIGlobalMask &= ~POIFlags.street_num;
         }
      }

      public bool POIGlobal_has_street {
         get {
            return (POIGlobalMask & POIFlags.street) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.street;
            else
               POIGlobalMask &= ~POIFlags.street;
         }
      }

      public bool POIGlobal_has_city {
         get {
            return (POIGlobalMask & POIFlags.city) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.city;
            else
               POIGlobalMask &= ~POIFlags.city;
         }
      }

      public bool POIGlobal_has_zip {
         get {
            return (POIGlobalMask & POIFlags.zip) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.zip;
            else
               POIGlobalMask &= ~POIFlags.zip;
         }
      }

      public bool POIGlobal_has_phone {
         get {
            return (POIGlobalMask & POIFlags.phone) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.phone;
            else
               POIGlobalMask &= ~POIFlags.phone;
         }
      }

      public bool POIGlobal_has_exit {
         get {
            return (POIGlobalMask & POIFlags.exit) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.exit;
            else
               POIGlobalMask &= ~POIFlags.exit;
         }
      }

      public bool POIGlobal_has_tide_prediction {
         get {
            return (POIGlobalMask & POIFlags.tide_prediction) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.tide_prediction;
            else
               POIGlobalMask &= ~POIFlags.tide_prediction;
         }
      }

      public bool POIGlobal_has_unknown {
         get {
            return (POIGlobalMask & POIFlags.unknown) != 0;
         }
         set {
            if (value)
               POIGlobalMask |= POIFlags.unknown;
            else
               POIGlobalMask &= ~POIFlags.unknown;
         }
      }

      #endregion

      /// <summary>
      /// Liste aller Label-Texte für ein entsprechendes Offset
      /// </summary>
      public TextBag TextList;
      /// <summary>
      /// Liste aller Länder-Offsets
      /// </summary>
      public List<CountryRecord>? CountryDataList;
      /// <summary>
      /// Liste aller Regionen-Offsets
      /// </summary>
      public List<RegionAndCountryRecord>? RegionAndCountryDataList;
      /// <summary>
      /// Liste aller Städte-Offsets
      /// </summary>
      public List<CityAndRegionOrCountryRecord>? CityAndRegionOrCountryDataList;
      /// <summary>
      /// Liste aller PLZ-Offsets
      /// </summary>
      public List<ZipRecord>? ZipDataList;
      /// <summary>
      /// Liste aller Highway-Offsets
      /// </summary>
      public List<HighwayWithExitRecord>? HighwayWithExitList;
      /// <summary>
      /// Liste aller Highway-Daten
      /// </summary>
      public List<HighwayExitDefRecord>? HighwayExitDefList;
      /// <summary>
      /// Liste aller Exit-Facilities
      /// </summary>
      public List<ExitRecord>? ExitList;

      /// <summary>
      /// Liste aller Pointdaten (<see cref="PointDataRecord"/>) (reine Daten, Geo-Daten in der RGN-Datei!)
      /// </summary>
      public List<PointDataRecord> PointPropertiesList;
      /// <summary>
      /// zum speichern der Offsets der einzelnen <see cref="PointDataRecord"/>
      /// <para>Wenn <see cref="StdFile_RGN.RawPointData.LabelOffsetInLBL"/> eines Punktes nicht auf einen Text sondern auf Zusatzdaten verweist, 
      /// wird hierdurch der Index des <see cref="PointDataRecord"/> aus <see cref="PointPropertiesList"/> ermittelt.</para>
      /// </summary>
      public SortedList<uint, int> PointPropertiesListOffsets;

      // Ev. zur einfacheren Suche bei Punkten aus den Subdiv's?

      /// <summary>
      /// Subdiv/Punktindex-Liste (Verweise auf Punkliste in den Subdiv's)
      /// </summary>
      public List<PointIndexRecord> PointIndexList4RGN;
      /// <summary>
      /// Liste der POI-Typen mit ihrem Startindex in <see cref="PointIndexList4RGN"/>
      /// </summary>
      public List<PointTypeIndexRecord> PointTypeIndexList4RGN;

      /// <summary>
      /// Text, der die Sortierung beschreibt
      /// </summary>
      public string SortDescriptor;

      Encoding cleartext;

      /// <summary>
      /// liefert den PostHeader-Datenbereich
      /// </summary>
      /// <returns></returns>
      public DataBlock? PostHeaderDataBlock { get; private set; }


      public StdFile_LBL(bool readwrite = false)
         : base("LBL") {

         textBagIsReadOnly = !readwrite;

         cleartext = Encoding.GetEncoding(1252);

         Headerlength = 170; // 196
         Headerlength = 196;

         DataOffsetMultiplier = 0;
         POIOffsetMultiplier = 0;
         POIGlobalMask = 0;

         // --------- Headerlänge > 170 Byte

         /* Daten entsprechend der Dateien: mkgmap\resources\sort\cp*.txt
          * z.B.:
               codepage 0
               id1 0
               id2 1
               description "ASCII 7-bit sort" 

               codepage 1250
               id1 12
               id2 1
               description "Central European sort"

               codepage 1252
               id1 7
               id2 2
               description "Western European sort"
         */
         SortDescriptor = "Western European sort";
         ID1 = 0x0007;
         ID2 = 0x8002; // MKGMAP verwendet intern nur 7 Bit für ID2. Warum Bit 31 zusätzlich gesetzt ist, ist unklar.
         EncodingType = 0x09;
         Codepage = 1252;

         // gefunden in "devon2016" von http://pinns.co.uk/devon/mobile/mbdevonmap.html:
         //SortDescriptor = "ASCII 7 - bit sort";
         //ID1 = 0x0000;
         //ID2 = 0x0001; // Bit 31 NICHT gesetzt
         //EncodingType = 0x06;
         //Codepage = 0;

         // --------- Headerlänge > 196 Byte

         dataoffsetMultiplier = (uint)(0x01 << DataOffsetMultiplier);
         poioffsetMultiplier = (uint)(0x01 << POIOffsetMultiplier);
         codec = new LabelCodec(EncodingType, Codepage);
         TextList = getNewTextBag();

         CountryDataList = new List<CountryRecord>();
         RegionAndCountryDataList = new List<RegionAndCountryRecord>();
         CityAndRegionOrCountryDataList = new List<CityAndRegionOrCountryRecord>();
         PointIndexList4RGN = new List<PointIndexRecord>();
         PointTypeIndexList4RGN = new List<PointTypeIndexRecord>();
         ZipDataList = new List<ZipRecord>();
         PointPropertiesList = new List<PointDataRecord>();
         ExitList = new List<ExitRecord>();
         HighwayExitDefList = new List<HighwayExitDefRecord>();
         HighwayWithExitList = new List<HighwayWithExitRecord>();

         PointPropertiesListOffsets = new SortedList<uint, int>();

         SortDescriptorDefBlock = new DataBlock();
         Lbl13Block = new DataBlockWithRecordsize();
         TidePredictionBlock = new DataBlockWithRecordsize();

         UnknownBlock_0xD0 = new DataBlock();
         UnknownBlock_0xDE = new DataBlock();
         UnknownBlock_0xEC = new DataBlock();
         UnknownBlock_0xFA = new DataBlock();
         UnknownBlock_0x108 = new DataBlock();
         UnknownBlock_0x116 = new DataBlock();
         UnknownBlock_0x124 = new DataBlock();
         UnknownBlock_0x132 = new DataBlock();
         UnknownBlock_0x140 = new DataBlock();
         UnknownBlock_0x14E = new DataBlock();
         UnknownBlock_0x15A = new DataBlock();
         UnknownBlock_0x168 = new DataBlock();
         UnknownBlock_0x176 = new DataBlock();
         UnknownBlock_0x184 = new DataBlock();
         UnknownBlock_0x192 = new DataBlock();
         UnknownBlock_0x19A = new DataBlock();
         UnknownBlock_0x1A6 = new DataBlock();
         UnknownBlock_0x1B2 = new DataBlock();
         UnknownBlock_0x1BE = new DataBlock();
         UnknownBlock_0x1CA = new DataBlock();
         UnknownBlock_0x1D8 = new DataBlock();
         UnknownBlock_0x1E6 = new DataBlock();
         UnknownBlock_0x1F2 = new DataBlock();
         UnknownBlock_0x200 = new DataBlock();

      }

      bool textBagIsReadOnly = false;

      TextBag getNewTextBag() {
         if (textBagIsReadOnly)
            return new TextBagRO(codec, (int)dataoffsetMultiplier);
         else
            return new TextBagRW(codec, (int)dataoffsetMultiplier);
      }

      #region Funktionen, die Texte liefern

      /// <summary>
      /// liefert den Text zum Offset oder null (Offset 0 liefert i.A. null)
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Offset ungültig ist</returns>
      public string? GetText(uint offset, bool withctrl) { // = true) {
         if (withctrl)
            return TextList.Text((int)offset);
         else {
            string? txt = TextList.Text((int)offset);
            if (txt != null) {
               byte[] bytes = cleartext.GetBytes(txt);
               for (int i = 0; i < bytes.Length; i++)
                  if (bytes[i] < 0x20)
                     bytes[i] = 0x2E;
               return cleartext.GetString(bytes);
            }
         }
         return null;
      }


      /// <summary>
      /// liefert den Text zum Offset aus der <see cref="PointPropertiesList"/> oder null (Offset 0 liefert i.A. null)
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Offset ungültig ist</returns>
      public string? GetText_FromPointList(uint offset, bool withctrl) => PointPropertiesListOffsets.TryGetValue(offset, out int idx) ?
                                                                              GetText_FromPointList(idx, withctrl) :
                                                                              null;

      /// <summary>
      /// liefert den Text zum Index aus der <see cref="PointPropertiesList"/> oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Index ungültig ist</returns>
      public string? GetText_FromPointList(int idx, bool withctrl) => idx < PointPropertiesList.Count ?
                                                                              PointPropertiesList[idx].GetText(this, withctrl) :
                                                                              null;

      /// <summary>
      /// liefert den Text zum Index aus der <see cref="ZipDataList"/> oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Index ungültig ist</returns>
      public string? GetText_FromZipList(int idx, bool withctrl) => ZipDataList != null &&
                                                                    idx < ZipDataList.Count ?
                                                                              ZipDataList[idx].GetText(this, withctrl) :
                                                                              null;

      /// <summary>
      /// liefert den Text der Stadt oder null (Offset 0 liefert i.A. null) aus der <see cref="CityAndRegionOrCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl"></param>
      /// <returns></returns>
      public string? GetCityText_FromCityAndRegionOrCountryDataList(StdFile_RGN rgn, int idx, bool withctrl) =>
         CityAndRegionOrCountryDataList != null && idx < CityAndRegionOrCountryDataList.Count ?
                  CityAndRegionOrCountryDataList[idx].GetCityText(this, rgn, withctrl) :
                  null;

      /// <summary>
      /// liefert den Text der Region oder null (Offset 0 liefert i.A. null) aus der <see cref="RegionAndCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetRegionText_FromRegionAndCountryList(int idx, bool withctrl) =>
         RegionAndCountryDataList != null && idx < RegionAndCountryDataList.Count ?
                  RegionAndCountryDataList[idx].GetRegionText(this, withctrl) :
                  null;

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="CityAndRegionOrCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetRegionText_FromCityAndRegionOrCountryDataList(int idx, bool withctrl) =>
         CityAndRegionOrCountryDataList != null && idx < CityAndRegionOrCountryDataList.Count ?
                  CityAndRegionOrCountryDataList[idx].GetRegionText(this, withctrl) :
                  null;

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="CountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetCountryText_FromCountryList(int idx, bool withctrl) =>
         CountryDataList != null && idx < CountryDataList.Count ? // Index 1-basiert!
                  CountryDataList[idx].GetText(this, withctrl) :
                  null;

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="RegionAndCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetCountryText_FromRegionAndCountryList(int idx, bool withctrl) =>
         RegionAndCountryDataList != null && idx < RegionAndCountryDataList.Count ?
                  RegionAndCountryDataList[idx].GetCountryText(this, withctrl) :
                  null;

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="CityAndRegionOrCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl"></param>
      /// <returns></returns>
      string? GetCountryText_FromCityAndRegionOrCountryDataList(int idx, bool withctrl) =>
         CityAndRegionOrCountryDataList != null && idx < CityAndRegionOrCountryDataList.Count ?
                  CityAndRegionOrCountryDataList[idx].GetCountryText(this, withctrl) :
                  null;

      #endregion

      /// <summary>
      /// liefert den Offset zum Text (und fügt den Text bei Bedarf ein) oder 0, wenn der Textspeicher voll ist
      /// <para>Wenn der Textspeicher voll ist muss mit einem größeren OffsetMultiplier gearbeitet werden.</para>
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      public uint GetTextOffset(string txt) {
         int offset = TextList.Insert(txt);
         return offset > 0 ? (uint)offset : 0;
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         TextBlock = new DataBlock(br);
         DataOffsetMultiplier = br.ReadByte();
         EncodingType = br.ReadByte();
         CountryBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x29);
         RegionBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x37);
         CityBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x45);
         POIIndexBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x53);
         POIPropertiesBlock = new DataBlock(br);
         POIOffsetMultiplier = br.ReadByte();
         POIGlobalMask = (POIFlags)br.ReadByte();
         br.ReadBytes(Unknown_0x61);
         POITypeIndexBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x6E);
         ZipBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x7C);
         HighwayWithExitBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x8A);
         ExitBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x98);
         HighwayExitBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0xA6);

         // --------- Headerlänge > 170 Byte
         if (Headerlength > 0xaa) {
            ushort tmp = br.Read2AsUShort();
            if (tmp > 0)
               Codepage = tmp;
            ID1 = br.Read2AsUShort();
            ID2 = br.Read2AsUShort();
            SortDescriptorDefBlock = new DataBlock(br);
            Lbl13Block = new DataBlockWithRecordsize(br);
            br.ReadBytes(Unknown_0xC2);

            // --------- Headerlänge > 196 Byte
            if (Headerlength > 0xc4) {
               TidePredictionBlock = new DataBlockWithRecordsize(br);
               if (Headerlength > 0xCE) {
                  br.ReadBytes(Unknown_0xCE);
                  if (Headerlength > 0xD0) {
                     UnknownBlock_0xD0 = new DataBlock(br);
                     if (Headerlength > 0xD8) {
                        br.ReadBytes(Unknown_0xD8);
                        if (Headerlength > 0xDE) {
                           UnknownBlock_0xDE = new DataBlock(br);
                           if (Headerlength > 0xE6) {
                              br.ReadBytes(Unknown_0xE6);
                              if (Headerlength > 0xEC) {
                                 UnknownBlock_0xEC = new DataBlock(br);
                                 if (Headerlength > 0xF4) {
                                    br.ReadBytes(Unknown_0xF4);
                                    if (Headerlength > 0xFA) {
                                       UnknownBlock_0xFA = new DataBlock(br);
                                       if (Headerlength > 0x102) {
                                          br.ReadBytes(Unknown_0x102);
                                          if (Headerlength > 0x108) {
                                             UnknownBlock_0x108 = new DataBlock(br);
                                             if (Headerlength > 0x110) {
                                                br.ReadBytes(Unknown_0x110);
                                                if (Headerlength > 0x116) {
                                                   UnknownBlock_0x116 = new DataBlock(br);
                                                   if (Headerlength > 0x11E) {
                                                      br.ReadBytes(Unknown_0x11E);
                                                      if (Headerlength > 0x124) {
                                                         UnknownBlock_0x124 = new DataBlock(br);
                                                         if (Headerlength > 0x12C) {
                                                            br.ReadBytes(Unknown_0x12C);
                                                            if (Headerlength > 0x132) {
                                                               UnknownBlock_0x132 = new DataBlock(br);
                                                               if (Headerlength > 0x13A) {
                                                                  br.ReadBytes(Unknown_0x13A);
                                                                  if (Headerlength > 0x140) {
                                                                     UnknownBlock_0x140 = new DataBlock(br);
                                                                     if (Headerlength > 0x148) {
                                                                        br.ReadBytes(Unknown_0x148);
                                                                        if (Headerlength > 0x14E) {
                                                                           UnknownBlock_0x14E = new DataBlock(br);
                                                                           if (Headerlength > 0x156) {
                                                                              br.ReadBytes(Unknown_0x156);
                                                                              if (Headerlength > 0x15A) {
                                                                                 UnknownBlock_0x15A = new DataBlock(br);
                                                                                 if (Headerlength > 0x162) {
                                                                                    br.ReadBytes(Unknown_0x162);
                                                                                    if (Headerlength > 0x168) {
                                                                                       UnknownBlock_0x168 = new DataBlock(br);
                                                                                       if (Headerlength > 0x170) {
                                                                                          br.ReadBytes(Unknown_0x170);
                                                                                          if (Headerlength > 0x176) {
                                                                                             UnknownBlock_0x176 = new DataBlock(br);
                                                                                             if (Headerlength > 0x17E) {
                                                                                                br.ReadBytes(Unknown_0x17E);
                                                                                                if (Headerlength > 0x184) {
                                                                                                   UnknownBlock_0x184 = new DataBlock(br);
                                                                                                   if (Headerlength > 0x18C) {
                                                                                                      br.ReadBytes(Unknown_0x18C);
                                                                                                      if (Headerlength > 0x192) {
                                                                                                         UnknownBlock_0x192 = new DataBlock(br);
                                                                                                         if (Headerlength > 0x19A) {
                                                                                                            UnknownBlock_0x19A = new DataBlock(br);
                                                                                                            if (Headerlength > 0x1A2) {
                                                                                                               br.ReadBytes(Unknown_0x1A2);
                                                                                                               if (Headerlength > 0x1A6) {
                                                                                                                  UnknownBlock_0x1A6 = new DataBlock(br);
                                                                                                                  if (Headerlength > 0x1AE) {
                                                                                                                     br.ReadBytes(Unknown_0x1AE);
                                                                                                                     if (Headerlength > 0x1B2) {
                                                                                                                        UnknownBlock_0x1B2 = new DataBlock(br);
                                                                                                                        if (Headerlength > 0x1BA) {
                                                                                                                           br.ReadBytes(Unknown_0x1BA);
                                                                                                                           if (Headerlength > 0x1BE) {
                                                                                                                              UnknownBlock_0x1BE = new DataBlock(br);
                                                                                                                              if (Headerlength > 0x1C6) {
                                                                                                                                 br.ReadBytes(Unknown_0x1C6);
                                                                                                                                 if (Headerlength > 0x1CA) {
                                                                                                                                    UnknownBlock_0x1CA = new DataBlock(br);
                                                                                                                                    if (Headerlength > 0x1D2) {
                                                                                                                                       br.ReadBytes(Unknown_0x1D2);
                                                                                                                                       if (Headerlength > 0x1D8) {
                                                                                                                                          UnknownBlock_0x1D8 = new DataBlock(br);
                                                                                                                                          if (Headerlength > 0x1E0) {
                                                                                                                                             br.ReadBytes(Unknown_0x1E0);
                                                                                                                                             if (Headerlength > 0x1E6) {
                                                                                                                                                UnknownBlock_0x1E6 = new DataBlock(br);
                                                                                                                                                if (Headerlength > 0x1EE) {
                                                                                                                                                   br.ReadBytes(Unknown_0x1EE);
                                                                                                                                                   if (Headerlength > 0x1F2) {
                                                                                                                                                      UnknownBlock_0x1F2 = new DataBlock(br);
                                                                                                                                                      if (Headerlength > 0x1FA) {
                                                                                                                                                         br.ReadBytes(Unknown_0x1FA);
                                                                                                                                                         if (Headerlength > 0x200) {
                                                                                                                                                            UnknownBlock_0x200 = new DataBlock(br);

                                                                                                                                                            if (Headerlength > 0x208) {
                                                                                                                                                               Unknown_0x208 = new byte[Headerlength - 0x208];
                                                                                                                                                               br.ReadBytes(Unknown_0x208);
                                                                                                                                                            }
                                                                                                                                                         }
                                                                                                                                                      }
                                                                                                                                                   }
                                                                                                                                                }
                                                                                                                                             }
                                                                                                                                          }
                                                                                                                                       }
                                                                                                                                    }
                                                                                                                                 }
                                                                                                                              }
                                                                                                                           }
                                                                                                                        }
                                                                                                                     }
                                                                                                                  }
                                                                                                               }
                                                                                                            }
                                                                                                         }
                                                                                                      }
                                                                                                   }
                                                                                                }
                                                                                             }
                                                                                          }
                                                                                       }
                                                                                    }
                                                                                 }
                                                                              }
                                                                           }
                                                                        }
                                                                     }
                                                                  }
                                                               }
                                                            }
                                                         }
                                                      }
                                                   }
                                                }
                                             }
                                          }
                                       }
                                    }
                                 }
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         if (TextBlock != null)
            Filesections.AddSection((int)InternalFileSections.TextBlock, new DataBlock(TextBlock));
         if (CountryBlock != null)
            Filesections.AddSection((int)InternalFileSections.CountryBlock, new DataBlockWithRecordsize(CountryBlock, (ushort)CountryRecord.DataLength));
         if (RegionBlock != null)
            Filesections.AddSection((int)InternalFileSections.RegionBlock, new DataBlockWithRecordsize(RegionBlock, (ushort)RegionAndCountryRecord.DataLength));
         if (CityBlock != null)
            Filesections.AddSection((int)InternalFileSections.CityBlock, new DataBlockWithRecordsize(CityBlock, (ushort)CityAndRegionOrCountryRecord.DataLength));
         if (POIIndexBlock != null)
            Filesections.AddSection((int)InternalFileSections.POIIndexBlock, new DataBlockWithRecordsize(POIIndexBlock, (ushort)PointIndexRecord.DataLength));
         if (POIPropertiesBlock != null)
            Filesections.AddSection((int)InternalFileSections.POIPropertiesBlock, new DataBlock(POIPropertiesBlock));
         if (POITypeIndexBlock != null)
            Filesections.AddSection((int)InternalFileSections.POITypeIndexBlock, new DataBlockWithRecordsize(POITypeIndexBlock, (ushort)PointTypeIndexRecord.DataLength));
         if (ZipBlock != null)
            Filesections.AddSection((int)InternalFileSections.ZipBlock, new DataBlockWithRecordsize(ZipBlock, (ushort)ZipRecord.DataLength));
         if (HighwayWithExitBlock != null)
            Filesections.AddSection((int)InternalFileSections.HighwayWithExitBlock, new DataBlockWithRecordsize(HighwayWithExitBlock, (ushort)HighwayWithExitRecord.DataLength));
         if (ExitBlock != null)
            Filesections.AddSection((int)InternalFileSections.ExitBlock, new DataBlockWithRecordsize(ExitBlock, (ushort)ExitRecord.DataLength));
         if (HighwayExitBlock != null)
            Filesections.AddSection((int)InternalFileSections.HighwayExitBlock, new DataBlockWithRecordsize(HighwayExitBlock, (ushort)HighwayExitBlock.Recordsize)); // ????? variabel?
         Filesections.AddSection((int)InternalFileSections.SortDescriptorDefBlock, new DataBlock(SortDescriptorDefBlock));
         Filesections.AddSection((int)InternalFileSections.Lbl13Block, new DataBlockWithRecordsize(Lbl13Block));
         Filesections.AddSection((int)InternalFileSections.TidePredictionBlock, new DataBlockWithRecordsize(TidePredictionBlock));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0xD0, new DataBlock(UnknownBlock_0xD0));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0xDE, new DataBlock(UnknownBlock_0xDE));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0xEC, new DataBlock(UnknownBlock_0xEC));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0xFA, new DataBlock(UnknownBlock_0xFA));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x108, new DataBlock(UnknownBlock_0x108));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x116, new DataBlock(UnknownBlock_0x116));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x124, new DataBlock(UnknownBlock_0x124));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x132, new DataBlock(UnknownBlock_0x132));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x140, new DataBlock(UnknownBlock_0x140));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x14E, new DataBlock(UnknownBlock_0x14E));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x15A, new DataBlock(UnknownBlock_0x15A));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x168, new DataBlock(UnknownBlock_0x168));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x176, new DataBlock(UnknownBlock_0x176));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x184, new DataBlock(UnknownBlock_0x184));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x192, new DataBlock(UnknownBlock_0x192));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x19A, new DataBlock(UnknownBlock_0x19A));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1A6, new DataBlock(UnknownBlock_0x1A6));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1B2, new DataBlock(UnknownBlock_0x1B2));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1BE, new DataBlock(UnknownBlock_0x1BE));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1CA, new DataBlock(UnknownBlock_0x1CA));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1D8, new DataBlock(UnknownBlock_0x1D8));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1E6, new DataBlock(UnknownBlock_0x1E6));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x1F2, new DataBlock(UnknownBlock_0x1F2));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x200, new DataBlock(UnknownBlock_0x200));

         // GapOffset und DataOffset setzen
         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);

         if (GapOffset > HeaderOffset + Headerlength) { // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            PostHeaderDataBlock = new DataBlock(HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, PostHeaderDataBlock);
         }

         // Datenblöcke einlesen
         Filesections.ReadSections(br);

         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);
      }

      protected override void DecodeSections() {
         TextList = getNewTextBag();
         CountryDataList?.Clear();
         RegionAndCountryDataList?.Clear();
         CityAndRegionOrCountryDataList?.Clear();
         PointIndexList4RGN.Clear();
         PointTypeIndexList4RGN.Clear();
         ZipDataList?.Clear();
         HighwayWithExitList?.Clear();
         HighwayExitDefList?.Clear();
         ExitList?.Clear();
         PointPropertiesList.Clear();

         // Datenblöcke "interpretieren"
         int filesectiontype;

         filesectiontype = (int)InternalFileSections.SortDescriptorDefBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_SortDescriptorDefBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         dataoffsetMultiplier = (uint)(0x01 << DataOffsetMultiplier);
         poioffsetMultiplier = (uint)(0x01 << POIOffsetMultiplier);
         codec = new LabelCodec(EncodingType, Codepage);

         if (Locked != 0) {
            RawRead = true;
            return;
         }

         filesectiontype = (int)InternalFileSections.TextBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_TextBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.CountryBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_CountryBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.RegionBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_RegionBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.CityBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_CityBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.POIIndexBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_POIIndexBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.POITypeIndexBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_POITypeIndexBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.ZipBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_ZipBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.HighwayWithExitBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_HighwayWithExitBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.HighwayExitBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_HighwayExitBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.ExitBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_ExitBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.POIPropertiesBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_POIPropertiesBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.Lbl13Block;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            //Decode_Lbl13Block(Filesections.GetSectionDataReader(filesectiontype), bl);
            //Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.TidePredictionBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            //Decode_TidePredictionBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            //Filesections.RemoveSection(filesectiontype);
         }

      }

      public override void Encode_Sections() {
         SetData2Filesection((int)InternalFileSections.SortDescriptorDefBlock, true);
         SetData2Filesection((int)InternalFileSections.TextBlock, true);
         SetData2Filesection((int)InternalFileSections.CountryBlock, true);
         SetData2Filesection((int)InternalFileSections.RegionBlock, true);
         SetData2Filesection((int)InternalFileSections.CityBlock, true);
         SetData2Filesection((int)InternalFileSections.POIIndexBlock, true);
         SetData2Filesection((int)InternalFileSections.POIPropertiesBlock, true);
         SetData2Filesection((int)InternalFileSections.POITypeIndexBlock, true);
         SetData2Filesection((int)InternalFileSections.ZipBlock, true);
         SetData2Filesection((int)InternalFileSections.HighwayWithExitBlock, true);
         SetData2Filesection((int)InternalFileSections.ExitBlock, true);
         SetData2Filesection((int)InternalFileSections.HighwayExitBlock, true);
         SetData2Filesection((int)InternalFileSections.Lbl13Block, true);
         SetData2Filesection((int)InternalFileSections.TidePredictionBlock, true);
      }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         switch ((InternalFileSections)filesectiontype) {
            case InternalFileSections.TextBlock:
               Encode_TextBlock(bw);
               break;
            case InternalFileSections.CountryBlock:
               Encode_CountryBlock(bw);
               break;
            case InternalFileSections.RegionBlock:
               Encode_RegionBlock(bw);
               break;
            case InternalFileSections.CityBlock:
               Encode_CityBlock(bw);
               break;
            case InternalFileSections.POIIndexBlock:
               Encode_POIIndexBlock(bw);
               break;
            case InternalFileSections.POIPropertiesBlock:
               Encode_POIPropertiesBlock(bw);
               break;
            case InternalFileSections.POITypeIndexBlock:
               Encode_POITypeIndexBlock(bw);
               break;
            case InternalFileSections.ZipBlock:
               Encode_ZipBlock(bw);
               break;
            case InternalFileSections.HighwayWithExitBlock:
               Encode_HighwayWithExitBlock(bw);
               break;
            case InternalFileSections.ExitBlock:
               Encode_ExitBlock(bw);
               break;
            case InternalFileSections.HighwayExitBlock:
               Encode_HighwayExitBlock(bw);
               break;
            case InternalFileSections.SortDescriptorDefBlock:
               Encode_SortDescriptorDefBlock(bw);
               break;
            case InternalFileSections.Lbl13Block:
               Encode_Lbl13Block(bw);
               break;
            case InternalFileSections.TidePredictionBlock:
               Encode_TidePredictionBlock(bw);
               break;
         }
      }

      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.PostHeaderData, pos++);
         Filesections.SetOffset((int)InternalFileSections.SortDescriptorDefBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.TextBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.CountryBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.RegionBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.CityBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.POIIndexBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.POIPropertiesBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.POITypeIndexBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.ZipBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.HighwayWithExitBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.ExitBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.HighwayExitBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.Lbl13Block, pos++);
         Filesections.SetOffset((int)InternalFileSections.TidePredictionBlock, pos++);

         Filesections.AdjustSections(DataOffset);     // lückenlos ausrichten

         TextBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.TextBlock));
         CountryBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.CountryBlock));
         RegionBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.RegionBlock));
         CityBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.CityBlock));
         POIIndexBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.POIIndexBlock));
         POIPropertiesBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.POIPropertiesBlock));
         POITypeIndexBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.POITypeIndexBlock));
         ZipBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.ZipBlock));
         HighwayWithExitBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.HighwayWithExitBlock));
         ExitBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.ExitBlock));
         HighwayExitBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.HighwayExitBlock));
         if (Filesections.ContainsType((int)InternalFileSections.SortDescriptorDefBlock))
            SortDescriptorDefBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.SortDescriptorDefBlock));
         if (Filesections.ContainsType((int)InternalFileSections.Lbl13Block))
            Lbl13Block = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.Lbl13Block));
         if (Filesections.ContainsType((int)InternalFileSections.TidePredictionBlock))
            TidePredictionBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.TidePredictionBlock));
      }

      #region Decodierung der Datenblöcke

      /// <summary>
      /// alle Label-Texte einlesen
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_TextBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);

            TextList = getNewTextBag();
            byte[] buff = br.ReadBytes((int)block.Length); // gesamter codierter Textblock
            int start = TextList.OffsetMultiplier; // eine merkwürdige "Verschwendung" der 1. Bytes
            while (start < block.Length) {
               string txt = codec.Decode(buff, start);
               TextList.Insert(txt, start);
               if (codec.DecodedBytes > 0)
                  start += codec.DecodedBytes;
               else
                  start++;
               // start muss auf einem Vielfachen von dataoffsetMultiplier stehen !
               int r = start % TextList.OffsetMultiplier;
               if (r != 0)
                  start += TextList.OffsetMultiplier - r;
            }
            if (textBagIsReadOnly)
               ((TextBagRO)TextList).Freeze();
         }
      }

      /// <summary>
      /// alle Länderoffsets einlesen
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_CountryBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            CountryDataList = br.ReadArray<CountryRecord>(block);
         }
      }

      /// <summary>
      /// alle Regionsdaten einlesen
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_RegionBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            RegionAndCountryDataList = br.ReadArray<RegionAndCountryRecord>(block);
         }
      }

      /// <summary>
      /// alle Städtedaten einlesen
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_CityBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            CityAndRegionOrCountryDataList = br.ReadArray<CityAndRegionOrCountryRecord>(block);
         }
      }

      void Decode_POIIndexBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            PointIndexList4RGN = br.ReadArray<PointIndexRecord>(block);
         }
      }

      void Decode_POIPropertiesBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            object[] data = new object[1];
            data[0] = (UInt16)((UInt16)POIGlobalMask |
                               ((UInt16)((CityAndRegionOrCountryDataList != null && CityAndRegionOrCountryDataList.Count < 256 ?
                                                   PointDataRecord.SpecPropFlags.ShortCityIndex : 0) |
                                         (ZipDataList != null && ZipDataList.Count < 256 ?
                                                   PointDataRecord.SpecPropFlags.ShortZipIndex : 0) |
                                         (HighwayWithExitList != null && HighwayWithExitList.Count < 256 ?
                                                   PointDataRecord.SpecPropFlags.ShortExitHighwayIndex : 0) |
                                         (ExitList != null && ExitList.Count < 256 ? 
                                                   PointDataRecord.SpecPropFlags.ShortExitIndex : 0)) << 8));
            PointPropertiesList = br.ReadArray<PointDataRecord>(block, data, PointPropertiesListOffsets);
         }
      }

      void Decode_POITypeIndexBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            PointTypeIndexList4RGN = br.ReadArray<PointTypeIndexRecord>(block);
         }
      }

      /// <summary>
      /// alle ZIP-Offsets einlesen
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_ZipBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            ZipDataList = br.ReadArray<ZipRecord>(block);
         }
      }

      void Decode_HighwayWithExitBlock(BinaryReaderWriter? br, DataBlockWithRecordsize? block) {
         if (br != null && block != null && block.Length > 0) {
            HighwayWithExitList = br.ReadArray<HighwayWithExitRecord>(new DataBlock(block));
         }
      }

      void Decode_ExitBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            ExitList = br.ReadArray<ExitRecord>(block);
         }
      }

      /// <summary>
      /// vorher muss <see cref="Decode_HighwayWithExitBlock"/>() erfolgt sein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_HighwayExitBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null &&
             block != null &&
             block.Length > 0 &&
             HighwayWithExitList != null) {
            // Liste der Datensatzlängen für die HighwayDataList erzeugen
            object[] dslen = new object[HighwayWithExitList.Count];
            for (int i = 0; i < HighwayWithExitList.Count; i++)
               dslen[i] = (int)(3 * (HighwayWithExitList[i].FirstExitOffset - 1));      // Startoffset
            for (int i = 0; i < dslen.Length - 1; i++)
               dslen[i] = (int)dslen[i + 1] - (int)dslen[i];
            if (dslen.Length > 0)
               dslen[dslen.Length - 1] = (int)block.Length - (int)dslen[dslen.Length - 1];
            HighwayExitDefList = br.ReadArray<HighwayExitDefRecord>(block, dslen);
         }
      }

      void Decode_SortDescriptorDefBlock(BinaryReaderWriter? br, DataBlock? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);
            SortDescriptor = br.ReadString((int)block.Length);
         }
      }

      void Decode_Lbl13Block(BinaryReaderWriter? br, DataBlockWithRecordsize? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);


            throw new Exception("Decode_Lbl13Block() ist noch nicht implementiert.");


         }
      }

      void Decode_TidePredictionBlock(BinaryReaderWriter? br, DataBlockWithRecordsize? block) {
         if (br != null && block != null && block.Length > 0) {
            br.Seek(block.Offset);


            throw new Exception("Decode_TidePredictionBlock() ist noch nicht implementiert.");


         }
      }

      #endregion

      #region Encodierung der Datenblöcke

      void Encode_TextBlock(BinaryReaderWriter bw) {
         if (bw != null) {
            long startpos = bw.Position;
            int[] offsets = TextList.Offsets();    // liefert alle Offsets sortiert
            for (int i = 0; i < offsets.Length; i++) {
               byte[]? text = TextList.EncodedText(offsets[i]);
               if (text != null)
                  bw.Write(text);
               long nextpos = i < offsets.Length - 1 ?
                                       startpos + offsets[i + 1] * TextList.OffsetMultiplier :
                                       startpos + TextList.Length;
               while (bw.Position < nextpos)
                  bw.Write((byte)0);
            }
         }
      }

      void Encode_CountryBlock(BinaryReaderWriter bw) {
         if (bw != null && CountryDataList != null)
            for (int i = 0; i < CountryDataList.Count; i++)
               CountryDataList[i].Write(bw, null);
      }

      void Encode_RegionBlock(BinaryReaderWriter bw) {
         if (bw != null && RegionAndCountryDataList != null)
            for (int i = 0; i < RegionAndCountryDataList.Count; i++)
               RegionAndCountryDataList[i].Write(bw, null);
      }

      void Encode_CityBlock(BinaryReaderWriter bw) {
         if (bw != null && CityAndRegionOrCountryDataList != null)
            for (int i = 0; i < CityAndRegionOrCountryDataList.Count; i++)
               CityAndRegionOrCountryDataList[i].Write(bw, null);
      }

      void Encode_POIIndexBlock(BinaryReaderWriter bw) {
         if (bw != null)
            for (int i = 0; i < PointIndexList4RGN.Count; i++)
               PointIndexList4RGN[i].Write(bw, null);
      }

      void Encode_POIPropertiesBlock(BinaryReaderWriter bw) {
         if (bw != null)
            for (int i = 0; i < PointPropertiesList.Count; i++)
               PointPropertiesList[i].Write(bw, POIGlobalMask);
      }

      void Encode_POITypeIndexBlock(BinaryReaderWriter bw) {
         if (bw != null)
            for (int i = 0; i < PointTypeIndexList4RGN.Count; i++)
               PointTypeIndexList4RGN[i].Write(bw, null);
      }

      void Encode_ZipBlock(BinaryReaderWriter bw) {
         if (bw != null && ZipDataList != null)
            for (int i = 0; i < ZipDataList.Count; i++)
               ZipDataList[i].Write(bw, null);
      }

      void Encode_HighwayWithExitBlock(BinaryReaderWriter bw) {
         if (bw != null && HighwayWithExitList != null)
            for (int i = 0; i < HighwayWithExitList.Count; i++)
               HighwayWithExitList[i].Write(bw, null);
      }

      void Encode_ExitBlock(BinaryReaderWriter bw) {
         if (bw != null && ExitList != null)
            for (int i = 0; i < ExitList.Count; i++)
               ExitList[i].Write(bw, null);
      }

      void Encode_HighwayExitBlock(BinaryReaderWriter bw) {
         if (bw != null && HighwayExitDefList != null)
            for (int i = 0; i < HighwayExitDefList.Count; i++)
               HighwayExitDefList[i].Write(bw, null);
      }

      void Encode_SortDescriptorDefBlock(BinaryReaderWriter bw) {
         if (bw != null)
            bw.WriteString(SortDescriptor);
      }

      void Encode_Lbl13Block(BinaryReaderWriter bw) {
         if (bw != null) {


            //throw new Exception("Encode_Lbl13Block() ist noch nicht implementiert.");


         }
      }

      void Encode_TidePredictionBlock(BinaryReaderWriter bw) {
         if (bw != null) {


            //throw new Exception("Encode_TidePredictionBlock() ist noch nicht implementiert.");


         }
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            TextBlock?.Write(bw);
            dataoffsetMultiplier = (uint)TextList.OffsetMultiplier;
            DataOffsetMultiplier = (byte)(dataoffsetMultiplier >> 1);
            bw.Write(DataOffsetMultiplier);
            bw.Write(EncodingType);

            CountryBlock?.Write(bw);
            bw.Write(Unknown_0x29);
            RegionBlock?.Write(bw);
            bw.Write(Unknown_0x37);
            CityBlock?.Write(bw);
            bw.Write(Unknown_0x45);
            POIIndexBlock?.Write(bw);
            bw.Write(Unknown_0x53);
            POIPropertiesBlock?.Write(bw);
            POIOffsetMultiplier = (byte)(poioffsetMultiplier >> 1);
            bw.Write(POIOffsetMultiplier);
            bw.Write((byte)POIGlobalMask);
            bw.Write(Unknown_0x61);
            POITypeIndexBlock?.Write(bw);
            bw.Write(Unknown_0x6E);
            ZipBlock?.Write(bw);
            bw.Write(Unknown_0x7C);
            HighwayWithExitBlock?.Write(bw);
            bw.Write(Unknown_0x8A);
            ExitBlock?.Write(bw);
            bw.Write(Unknown_0x98);
            HighwayExitBlock?.Write(bw);
            bw.Write(Unknown_0xA6);

            // --------- Headerlänge > 170 Byte
            if (Headerlength > 0xaa) {
               bw.Write(Codepage);
               bw.Write(ID1);
               bw.Write(ID2);
               SortDescriptorDefBlock.Write(bw);
               Lbl13Block.Write(bw);
               bw.Write(Unknown_0xC2);

               // --------- Headerlänge > 196 Byte
               if (Headerlength > 0xc4) {
                  TidePredictionBlock.Write(bw);

                  if (Headerlength > 0xCE) {
                     bw.Write(Unknown_0xCE);
                     if (Headerlength > 0xD0) {
                        UnknownBlock_0xD0.Write(bw);
                        if (Headerlength > 0xD8) {
                           bw.Write(Unknown_0xD8);
                           if (Headerlength > 0xDE) {
                              UnknownBlock_0xDE.Write(bw);
                              if (Headerlength > 0xE6) {
                                 bw.Write(Unknown_0xE6);
                                 if (Headerlength > 0xEC) {
                                    UnknownBlock_0xEC.Write(bw);
                                    if (Headerlength > 0xF4) {
                                       bw.Write(Unknown_0xF4);
                                       if (Headerlength > 0xFA) {
                                          UnknownBlock_0xFA.Write(bw);
                                          if (Headerlength > 0x102) {
                                             bw.Write(Unknown_0x102);
                                             if (Headerlength > 0x108) {
                                                UnknownBlock_0x108.Write(bw);
                                                if (Headerlength > 0x110) {
                                                   bw.Write(Unknown_0x110);
                                                   if (Headerlength > 0x116) {
                                                      UnknownBlock_0x116.Write(bw);
                                                      if (Headerlength > 0x11E) {
                                                         bw.Write(Unknown_0x11E);
                                                         if (Headerlength > 0x124) {
                                                            UnknownBlock_0x124.Write(bw);
                                                            if (Headerlength > 0x12C) {
                                                               bw.Write(Unknown_0x12C);
                                                               if (Headerlength > 0x132) {
                                                                  UnknownBlock_0x132.Write(bw);
                                                                  if (Headerlength > 0x13A) {
                                                                     bw.Write(Unknown_0x13A);
                                                                     if (Headerlength > 0x140) {
                                                                        UnknownBlock_0x140.Write(bw);
                                                                        if (Headerlength > 0x148) {
                                                                           bw.Write(Unknown_0x148);
                                                                           if (Headerlength > 0x14E) {
                                                                              UnknownBlock_0x14E.Write(bw);
                                                                              if (Headerlength > 0x156) {
                                                                                 bw.Write(Unknown_0x156);
                                                                                 if (Headerlength > 0x15A) {
                                                                                    UnknownBlock_0x15A.Write(bw);
                                                                                    if (Headerlength > 0x162) {
                                                                                       bw.Write(Unknown_0x162);
                                                                                       if (Headerlength > 0x168) {
                                                                                          UnknownBlock_0x168.Write(bw);
                                                                                          if (Headerlength > 0x170) {
                                                                                             bw.Write(Unknown_0x170);
                                                                                             if (Headerlength > 0x176) {
                                                                                                UnknownBlock_0x176.Write(bw);
                                                                                                if (Headerlength > 0x17E) {
                                                                                                   bw.Write(Unknown_0x17E);
                                                                                                   if (Headerlength > 0x184) {
                                                                                                      UnknownBlock_0x184.Write(bw);
                                                                                                      if (Headerlength > 0x18C) {
                                                                                                         bw.Write(Unknown_0x18C);
                                                                                                         if (Headerlength > 0x192) {
                                                                                                            UnknownBlock_0x192.Write(bw);
                                                                                                            if (Headerlength > 0x19A) {
                                                                                                               UnknownBlock_0x19A.Write(bw);
                                                                                                               if (Headerlength > 0x1A2) {
                                                                                                                  bw.Write(Unknown_0x1A2);
                                                                                                                  if (Headerlength > 0x1A6) {
                                                                                                                     UnknownBlock_0x1A6.Write(bw);
                                                                                                                     if (Headerlength > 0x1AE) {
                                                                                                                        bw.Write(Unknown_0x1AE);
                                                                                                                        if (Headerlength > 0x1B2) {
                                                                                                                           UnknownBlock_0x1B2.Write(bw);
                                                                                                                           if (Headerlength > 0x1BA) {
                                                                                                                              bw.Write(Unknown_0x1BA);
                                                                                                                              if (Headerlength > 0x1BE) {
                                                                                                                                 UnknownBlock_0x1BE.Write(bw);
                                                                                                                                 if (Headerlength > 0x1C6) {
                                                                                                                                    bw.Write(Unknown_0x1C6);
                                                                                                                                    if (Headerlength > 0x1CA) {
                                                                                                                                       UnknownBlock_0x1CA.Write(bw);
                                                                                                                                       if (Headerlength > 0x1D2) {
                                                                                                                                          bw.Write(Unknown_0x1D2);
                                                                                                                                          if (Headerlength > 0x1D8) {
                                                                                                                                             UnknownBlock_0x1D8.Write(bw);
                                                                                                                                             if (Headerlength > 0x1E0) {
                                                                                                                                                bw.Write(Unknown_0x1E0);
                                                                                                                                                if (Headerlength > 0x1E6) {
                                                                                                                                                   UnknownBlock_0x1E6.Write(bw);
                                                                                                                                                   if (Headerlength > 0x1EE) {
                                                                                                                                                      bw.Write(Unknown_0x1EE);
                                                                                                                                                      if (Headerlength > 0x1F2) {
                                                                                                                                                         UnknownBlock_0x1F2.Write(bw);
                                                                                                                                                         if (Headerlength > 0x1FA) {
                                                                                                                                                            bw.Write(Unknown_0x1FA);
                                                                                                                                                            if (Headerlength > 0x200) {
                                                                                                                                                               UnknownBlock_0x200.Write(bw);

                                                                                                                                                               if (Headerlength > 0x208) {
                                                                                                                                                                  Unknown_0x208 = new byte[Headerlength - 0x208];
                                                                                                                                                                  bw.Write(Unknown_0x208);
                                                                                                                                                               }
                                                                                                                                                            }
                                                                                                                                                         }
                                                                                                                                                      }
                                                                                                                                                   }
                                                                                                                                                }
                                                                                                                                             }
                                                                                                                                          }
                                                                                                                                       }
                                                                                                                                    }
                                                                                                                                 }
                                                                                                                              }
                                                                                                                           }
                                                                                                                        }
                                                                                                                     }
                                                                                                                  }
                                                                                                               }
                                                                                                            }
                                                                                                         }
                                                                                                      }
                                                                                                   }
                                                                                                }
                                                                                             }
                                                                                          }
                                                                                       }
                                                                                    }
                                                                                 }
                                                                              }
                                                                           }
                                                                        }
                                                                     }
                                                                  }
                                                               }
                                                            }
                                                         }
                                                      }
                                                   }
                                                }
                                             }
                                          }
                                       }
                                    }
                                 }
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
      }

      #endregion

   }
}
