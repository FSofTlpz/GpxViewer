using GarminCore.Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace GarminCore.OptimizedReader {

   /// <summary>
   /// zum Lesen und Schreiben der LBL-Datei (enthält i.W. alle Texte, z.T. Tabellen auf deren Inhalt per 1-basiertem Index zugegriffen wird)
   /// </summary>
   public class StdFile_LBL : StdFile {

      #region Header-Daten

      /// <summary>
      /// Datenbereich für die Texte / Labels (0x15)
      /// </summary>
      DataBlock? TextBlock;
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
      DataBlockWithRecordsize? CountryBlock;
      byte[] Unknown_0x29 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Regionentabelle (0x2D)
      /// </summary>
      DataBlockWithRecordsize? RegionBlock;
      byte[] Unknown_0x37 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Städtetabelle (0x3B)
      /// </summary>
      DataBlockWithRecordsize? CityBlock;
      byte[] Unknown_0x45 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für POI ... ? (0x49)
      /// </summary>
      DataBlockWithRecordsize? POIIndexBlock;
      byte[] Unknown_0x53 = { 0, 0, 0, 0 };
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
      byte[] Unknown_0x61 = { 0, 0, 0 };
      /// <summary>
      /// Datenbereich für POI ... ? (0x64)
      /// </summary>
      DataBlockWithRecordsize? POITypeIndexBlock;
      byte[] Unknown_0x6E = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Postleitzahltabelle (0x72)
      /// </summary>
      DataBlockWithRecordsize? ZipBlock;
      byte[] Unknown_0x7C = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Highwaytabelle (für Routing?) (0x80)
      /// </summary>
      DataBlockWithRecordsize? HighwayWithExitBlock;
      byte[] Unknown_0x8A = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Ein- und Ausfahrtentabelle (für Routing?) (0x8E)
      /// </summary>
      DataBlockWithRecordsize? ExitBlock;
      byte[] Unknown_0x98 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die zusätzliche Highwaytabelle (für Routing?) (0x9C)
      /// </summary>
      DataBlockWithRecordsize? HighwayExitBlock;
      byte[] Unknown_0xA6 = { 0, 0, 0, 0 };

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
      DataBlock SortDescriptorDefBlock;
      /// <summary>
      /// ? (0xB8)
      /// </summary>
      DataBlockWithRecordsize Lbl13Block;
      byte[] Unknown_0xC2 = { 0, 0 };

      /// <summary>
      /// ? (0xC4)
      /// </summary>
      DataBlockWithRecordsize TidePredictionBlock;
      byte[] Unknown_0xCE = { 0, 0 };

      DataBlock UnknownBlock_0xD0;
      byte[] Unknown_0xD8 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0xDE;
      byte[] Unknown_0xE6 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0xEC;
      byte[] Unknown_0xF4 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0xFA;
      byte[] Unknown_0x102 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x108;
      byte[] Unknown_0x110 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x116;
      byte[] Unknown_0x11E = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x124;
      byte[] Unknown_0x12C = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x132;
      byte[] Unknown_0x13A = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x140;
      byte[] Unknown_0x148 = { 0, 0, 0, 0, 0, 0 };

      DataBlock UnknownBlock_0x14E;
      byte[] Unknown_0x156 = { 0, 0, 0, 0 };

      DataBlock UnknownBlock_0x15A;
      byte[] Unknown_0x162 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x168;
      byte[] Unknown_0x170 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x176;
      byte[] Unknown_0x17E = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x184;
      byte[] Unknown_0x18C = { 0, 0, 0, 0, 0, 0 };

      DataBlock UnknownBlock_0x192;

      DataBlock UnknownBlock_0x19A;
      byte[] Unknown_0x1A2 = { 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x1A6;
      byte[] Unknown_0x1AE = { 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x1B2;
      byte[] Unknown_0x1BA = { 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x1BE;
      byte[] Unknown_0x1C6 = { 0, 0, 0, 0 };

      DataBlock UnknownBlock_0x1CA;
      byte[] Unknown_0x1D2 = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x1D8;
      byte[] Unknown_0x1E0 = { 0, 0, 0, 0, 0, 0 };

      DataBlock UnknownBlock_0x1E6;
      byte[] Unknown_0x1EE = { 0, 0, 0, 0 };

      DataBlock UnknownBlock_0x1F2;
      byte[] Unknown_0x1FA = { 0, 0, 0, 0, 0, 0 };
      DataBlock UnknownBlock_0x200;
      byte[] Unknown_0x208 = { 0, 0, 0, 0, 0, 0, 0, 0 };

      #endregion

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
         byte Unknown1;

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

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            TextOffset = reader.Read3AsUInt();
            FirstExitOffset = reader.Read2AsUShort();
            Unknown1 = reader.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            Unknown1 = reader.ReadByte();
            RegionIndex = reader.Read2AsUShort();
            ExitList = new List<ExitPoint>();
            if (extdata != null) {
               int len = (int)extdata;
               len -= 3;
               while (len >= 3) {
                  ExitPoint ep = new ExitPoint {
                     PointIndexInRGN = reader.ReadByte(),
                     SubdivisionNumberInRGN = reader.Read2AsUShort()
                  };
                  ExitList.Add(ep);
                  len -= 3;
               }
            }
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            PointType = reader.ReadByte();
            StartIdx = reader.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            PointIndexInRGN = reader.ReadByte();
            SubdivisionNumberInRGN = reader.Read2AsUShort();
            SubType = reader.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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
         public UInt32 TextOffset => _data & 0x7FFFFF;         // Bit 0..22

         /// <summary>
         /// ein Hausnummernoffset wird gesetzt oder geliefert (<see cref="StreetNumber"/> ist dann ungültig)
         /// </summary>
         public UInt32 StreetNumberOffset {
            get => StreetNumberIsSet && !StreetNumberIsCoded ? _streetnumberoffset : UInt32.MaxValue;
            protected set {
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
               if (StreetNumberIsCoded) {
                  return DecodeString11(_streetnumber_encoded);
               }
               return null;
            }
            protected set {
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
            get => StreetIsSet ? _streetoffset : UInt32.MaxValue;
            protected set {
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
         public UInt16 CityIndex => CityIsSet ?
                                          (UseShortCityIndex ? (UInt16)(_cityindex & 0xFF) : _cityindex) :
                                          UInt16.MaxValue;

         /// <summary>
         /// ein Index für den Städtenamen wird gesetzt oder geliefert (1basiert ?)
         /// </summary>
         public UInt16 ZipIndex => ZipIsSet ?
                                          (UseShortZipIndex ? (UInt16)(_zipindex & 0xFF) : _zipindex) :
                                          UInt16.MaxValue;

         /// <summary>
         /// ein Telefonnummernoffset wird gesetz oder geliefert (<see cref="PhoneNumber"/> ist dann ungültig)
         /// </summary>
         public UInt32 PhoneNumberOffset {
            get => PhoneIsSet && !PhoneNumberIsCoded ? _phonenumberoffset : UInt32.MaxValue;
            protected set {
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
               if (PhoneNumberIsCoded)
                  return DecodeString11(_phonenumber_encoded);
               return null;
            }
            protected set {
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
         public UInt16 ExitHighwayIndex => ExitIsSet ?
                                             (UseShortExitHighwayIndex ? (UInt16)(_ExitHighwayIndex & 0xFF) : _ExitHighwayIndex) :
                                             UInt16.MaxValue;

         /// <summary>
         /// Ist die Hausnummer gesetzt?
         /// </summary>
         public bool StreetNumberIsSet {
            get => (_internalPropMask & POIFlags.street_num) != 0;
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
            get => (_internalPropMask & POIFlags.street) != 0;
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
            get => (_internalPropMask & POIFlags.city) != 0;
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
            get => (_internalPropMask & POIFlags.zip) != 0;
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
            get => (_internalPropMask & POIFlags.phone) != 0;
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
         public bool ExitIsSet => (_internalPropMask & POIFlags.exit) != 0;

         /// <summary>
         /// Ist der Abfahrtindex gesetzt?
         /// </summary>
         public bool ExitIndexIsSet => ExitIsSet && (_ExitOffset & 0x800000) != 0;

         /// <summary>
         /// Ist ... gesetzt?
         /// </summary>
         public bool TidePredictionIsSet => (_internalPropMask & POIFlags.tide_prediction) != 0;

         public bool StreetNumberIsCoded {
            get => StreetNumberIsSet && (_SpecPropFlags & SpecPropFlags.StreetNumberEncoded) != 0;
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
            get => PhoneIsSet && (_SpecPropFlags & SpecPropFlags.PhoneNumberEncoded) != 0;
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
         public bool UseShortCityIndex => (_SpecPropFlags & SpecPropFlags.ShortCityIndex) != 0;

         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für Postleitzahlen verwendet wird
         /// </summary>
         public bool UseShortZipIndex => (_SpecPropFlags & SpecPropFlags.ShortZipIndex) != 0;

         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für die Abfahrt-Straße verwendet wird
         /// </summary>
         public bool UseShortExitHighwayIndex => (_SpecPropFlags & SpecPropFlags.ShortExitHighwayIndex) != 0;

         /// <summary>
         /// setzt oder liefert, ob der kurze Index (1-Byte-Zahl) für die Abfahrt verwendet wird
         /// </summary>
         public bool UseShortExitIndex => (_SpecPropFlags & SpecPropFlags.ShortExitIndex) != 0;

         /// <summary>
         /// Ex. eine eigene Flag-Maske? (Bit 23 von <see cref="TextOffset"/>)
         /// </summary>
         public bool HasLocalProperties => (_data & 0x800000) != 0;     // Bit 23


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
         public string? GetText(StdFile_LBL lbl, bool withctrl) => lbl.GetText(TextOffset, withctrl);


         public PointDataRecord() {
            _data = 0;
            _internalPropMask = 0;
            _SpecPropFlags = 0;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="reader"></param>
         /// <param name="extdata">Bit 0..7 globale POIFlags, Bit 8..15 SpecPropFlags</param>
         public override void Read(BinaryReaderWriter reader, object? extdata) {
            if (extdata == null)
               return;
            UInt16 dat = (UInt16)extdata;
            POIFlags POIGlobalFlags = (POIFlags)(0xFF & dat);
            _SpecPropFlags = (SpecPropFlags)(0xFF & (dat >> 8));

            _data = reader.Read3AsUInt();

            if (HasLocalProperties)
               _internalPropMask = DecompressedPOIFlags(POIGlobalFlags, (POIFlags)reader.ReadByte());
            else
               _internalPropMask = POIGlobalFlags;

            if (StreetNumberIsSet) {
               byte v = reader.ReadByte();
               if ((v & 0x80) != 0) {        // vom 1. Byte mit gesetztem 7. Bit bis zum nächsten Byte mit gesetztem 7. Bit
                  List<byte> lst = new List<byte> {
                     v
                  };
                  do {
                     v = reader.ReadByte();
                     lst.Add(v);
                  } while ((v & 0x80) == 0);
                  StreetNumber = DecodeString11(lst);
               } else {
                  reader.Position--;
                  StreetNumberOffset = reader.Read3AsUInt();
               }
            }

            if (StreetIsSet)
               StreetOffset = reader.Read3AsUInt();

            if (CityIsSet)
               if (UseShortCityIndex)
                  _cityindex = reader.ReadByte();
               else
                  _cityindex = reader.Read2AsUShort();

            if (ZipIsSet)
               if (UseShortZipIndex)
                  _zipindex = reader.ReadByte();
               else
                  _zipindex = reader.Read2AsUShort();

            if (PhoneIsSet) {
               byte v = reader.ReadByte();
               if ((v & 0x80) == 0x80) {
                  List<byte> lst = new List<byte> {
                     v
                  };
                  do {
                     v = reader.ReadByte();
                     lst.Add(v);
                  } while ((v & 0x80) == 0);
                  PhoneNumber = DecodeString11(lst);
               } else {
                  reader.Position--;
                  PhoneNumberOffset = reader.Read3AsUInt();
               }
            }

            if (ExitIsSet) {
               _ExitOffset = reader.Read3AsUInt();

               /*    3 Byte
                *    Bit 0 .. 21 für Index
                *    Bit 22      OvernightParking
                *    Bit 23      facilities defined
                *    n Bytes     highwayIndex (n abh. von der max. Anzahl der Highways)
                *    n Bytes     exitFacilityIndex (n abh. von der max. Anzahl der ExitFacility)
                */

               if (UseShortExitHighwayIndex)
                  _ExitHighwayIndex = reader.ReadByte();
               else
                  _ExitHighwayIndex = reader.Read2AsUShort();

               if (ExitIndexIsSet) {
                  if (UseShortExitIndex)
                     _ExitIndex = reader.ReadByte();
                  else
                     _ExitIndex = reader.Read2AsUShort();
               }
            }

            if ((_internalPropMask & POIFlags.tide_prediction) != 0)
               throw new Exception("Die Behandlung von Bit 6 in der POI-Maske ist noch nicht implementiert.");

            if ((_internalPropMask & POIFlags.unknown) != 0)
               throw new Exception("Die Behandlung von Bit 7 in der POI-Maske ist noch nicht implementiert.");

         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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
         string DecodeString11(IList<byte>? array) {
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
         public UInt32 TextOffsetInLBL => _data & 0x3fffff;      // Bit 0..21

         /// <summary>
         /// last facility for this exit
         /// </summary>
         public bool LastFacilitie => (_data & 0x800000) != 0;     // Bit 23

         /// <summary>
         /// nur 4 Bit
         /// </summary>
         public byte Type => (byte)((_data >> 24) & 0xF);    // Bit 24..27

         /// <summary>
         /// nur 3 Bit
         /// </summary>
         public byte Direction => (byte)((_data >> 29) & 0x7);    // Bit 29..31

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 5;


         public ExitRecord() {
            _data = 0;
            Facilities = 0;
         }

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            _data = reader.Read4UInt();
            Facilities = reader.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            TextOffsetInLBL = reader.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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
         public UInt32 TextOffsetInLBL => IsPointInRGN ? 0 : _Data;

         /// <summary>
         /// nur gültig wenn <see cref="IsPointInRGN"/> true ist (Bit 0..7)
         /// </summary>
         public byte PointIndexInRGN => (byte)(IsPointInRGN ? _Data & 0xff : 0);

         /// <summary>
         /// nur gültig wenn <see cref="IsPointInRGN"/> true ist (Bit 8..23)
         /// </summary>
         public UInt16 SubdivisionNumberInRGN => (UInt16)(IsPointInRGN ? (_Data >> 8) & 0xffff : 0);

         /// <summary>
         /// Bit 0..13 für den Region/Country-Index, Bit 14 als Flag für Region/Country, Bit 15 als Flag für POI oder Text-Offset
         /// </summary>
         UInt16 _Info;

         /// <summary>
         /// setzt oder liefert ob entweder <see cref="PointIndexInRGN"/> und <see cref="SubdivisionNumberInRGN"/> oder <see cref="TextOffsetInLBL"/> gültig ist
         /// </summary>
         public bool IsPointInRGN => Bit.IsSet(_Info, 15);

         /// <summary>
         /// setzt oder liefert ob <see cref="RegionOrCountryIndex"/> einen Region- oder Country-Index liefert (Bit 14)
         /// </summary>
         public bool IsCountry => Bit.IsSet(_Info, 14);

         /// <summary>
         /// liefert oder setzt Werte von 0 .. 0x3FFF (Bit 0 .. 13)
         /// </summary>
         public UInt16 RegionOrCountryIndex => (UInt16)(_Info & 0x3fff);

         /// <summary>
         /// Größe des Speicherbereiches in der LBL-Datei
         /// </summary>
         public const uint DataLength = 5;

         public CityAndRegionOrCountryRecord() {
            _Data = 0;
            _Info = 0;
         }

         public override void Read(BinaryReaderWriter reader, object? extdata) {
            _Data = reader.Read3AsUInt();
            _Info = reader.Read2AsUShort();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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

         public override void Read(BinaryReaderWriter fp, object? extdata) {
            CountryIndex = fp.Read2AsUShort();
            TextOffset = fp.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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

         public override void Read(BinaryReaderWriter fp, object? extdata) {
            TextOffsetInLBL = fp.Read3AsUInt();
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

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
            frozendata = new Data[0];
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
            int idx = frozendata != null ?
                           Array.BinarySearch(frozendata, new Data(offset * OffsetMultiplier, null)) :
                           -1;
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
               foreach (var item in frozendata)
                  if (item.Text == text)
                     return item.Offset;
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

      #region Lesen der globalen POI-Flags

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

      public bool POIGlobal_has_street_num => (POIGlobalMask & POIFlags.street_num) != 0;

      public bool POIGlobal_has_street => (POIGlobalMask & POIFlags.street) != 0;

      public bool POIGlobal_has_city => (POIGlobalMask & POIFlags.city) != 0;

      public bool POIGlobal_has_zip => (POIGlobalMask & POIFlags.zip) != 0;

      public bool POIGlobal_has_phone => (POIGlobalMask & POIFlags.phone) != 0;

      public bool POIGlobal_has_exit => (POIGlobalMask & POIFlags.exit) != 0;

      public bool POIGlobal_has_tide_prediction => (POIGlobalMask & POIFlags.tide_prediction) != 0;

      public bool POIGlobal_has_unknown => (POIGlobalMask & POIFlags.unknown) != 0;

      #endregion

      /// <summary>
      /// Liste aller Label-Texte für ein entsprechendes Offset
      /// </summary>
      public TextBag TextList { get; protected set; }

      /// <summary>
      /// Liste aller Länder-Offsets
      /// </summary>
      public List<CountryRecord> CountryDataList { get; protected set; }

      /// <summary>
      /// Liste aller Regionen-Offsets
      /// </summary>
      public List<RegionAndCountryRecord> RegionAndCountryDataList { get; protected set; }

      /// <summary>
      /// Liste aller Städte-Offsets
      /// </summary>
      public List<CityAndRegionOrCountryRecord> CityAndRegionOrCountryDataList { get; protected set; }

      /// <summary>
      /// Liste aller PLZ-Offsets
      /// </summary>
      public List<ZipRecord> ZipDataList { get; protected set; }

      /// <summary>
      /// Liste aller Highway-Offsets
      /// </summary>
      public List<HighwayWithExitRecord> HighwayWithExitList { get; protected set; }

      /// <summary>
      /// Liste aller Highway-Daten
      /// </summary>
      public List<HighwayExitDefRecord> HighwayExitDefList { get; protected set; }

      /// <summary>
      /// Liste aller Exit-Facilities
      /// </summary>
      public List<ExitRecord> ExitList { get; protected set; }

      /// <summary>
      /// Liste aller Pointdaten (<see cref="PointDataRecord"/>) (reine Daten, Geo-Daten in der RGN-Datei!)
      /// </summary>
      public List<PointDataRecord> PointPropertiesList { get; protected set; }

      /// <summary>
      /// zum speichern der Offsets der einzelnen <see cref="PointDataRecord"/>
      /// <para>Wenn <see cref="StdFile_RGN.RawPointData.LabelOffset"/> eines Punktes nicht auf einen Text sondern auf Zusatzdaten verweist, 
      /// wird hierdurch der Index des <see cref="PointDataRecord"/> aus <see cref="PointPropertiesList"/> ermittelt.</para>
      /// </summary>
      public SortedList<uint, int> PointPropertiesListOffsets { get; protected set; }

      // Ev. zur einfacheren Suche bei Punkten aus den Subdiv's?

      /// <summary>
      /// Subdiv/Punktindex-Liste (Verweise auf Punkliste in den Subdiv's)
      /// </summary>
      public List<PointIndexRecord> PointIndexList4RGN { get; protected set; }

      /// <summary>
      /// Liste der POI-Typen mit ihrem Startindex in <see cref="PointIndexList4RGN"/>
      /// </summary>
      public List<PointTypeIndexRecord> PointTypeIndexList4RGN { get; protected set; }

      /// <summary>
      /// Text, der die Sortierung beschreibt
      /// </summary>
      public string SortDescriptor { get; protected set; }

      Encoding cleartext;

      /// <summary>
      /// liefert den PostHeader-Datenbereich
      /// </summary>
      /// <returns></returns>
      public DataBlock? PostHeaderDataBlock { get; private set; }


      BinaryReaderWriter? livereader;



      public StdFile_LBL()
         : base("LBL") {

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

      TextBag getNewTextBag() {
         return new TextBagRO(codec, (int)dataoffsetMultiplier);
      }

      #region Funktionen, die Texte liefern

      /// <summary>
      /// liefert den Text zum Offset oder null (Offset 0 liefert i.A. null)
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Offset ungültig ist</returns>
      //public string GetText(uint offset, bool withctrl) { // = true) {
      //   if (withctrl)
      //      return TextList.Text((int)offset);
      //   else {
      //      string txt = TextList.Text((int)offset);
      //      if (txt != null) {
      //         byte[] bytes = cleartext.GetBytes(txt);
      //         for (int i = 0; i < bytes.Length; i++)
      //            if (bytes[i] < 0x20)
      //               bytes[i] = 0x2E;
      //         return cleartext.GetString(bytes);
      //      }
      //   }
      //   return null;
      //}


      /// <summary>
      /// liefert den Text zum Offset aus der <see cref="PointPropertiesList"/> oder null (Offset 0 liefert i.A. null)
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Offset ungültig ist</returns>
      public string? GetText_FromPointList(uint offset, bool withctrl) {
         if (PointPropertiesListOffsets.TryGetValue(offset, out int idx))
            return GetText_FromPointList((uint)idx, withctrl);
         return null;
      }

      /// <summary>
      /// liefert den Text zum Index aus der <see cref="PointPropertiesList"/> oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Index ungültig ist</returns>
      public string? GetText_FromPointList(int idx, bool withctrl) {
         if (idx < PointPropertiesList.Count)
            return PointPropertiesList[idx].GetText(this, withctrl);
         return null;
      }


      /// <summary>
      /// liefert den Text zum Index aus der <see cref="ZipDataList"/> oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns>null, wenn der Index ungültig ist</returns>
      public string? GetText_FromZipList(int idx, bool withctrl) {
         if (idx < ZipDataList.Count)
            return ZipDataList[idx].GetText(this, withctrl);
         return null;
      }


      /// <summary>
      /// liefert den Text der Stadt oder null (Offset 0 liefert i.A. null) aus der <see cref="CityAndRegionOrCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl"></param>
      /// <returns></returns>
      public string? GetCityText_FromCityAndRegionOrCountryDataList(StdFile_RGN rgn, int idx, bool withctrl) {
         if (idx < CityAndRegionOrCountryDataList.Count)
            return CityAndRegionOrCountryDataList[idx].GetCityText(this, rgn, withctrl);
         return null;
      }


      /// <summary>
      /// liefert den Text der Region oder null (Offset 0 liefert i.A. null) aus der <see cref="RegionAndCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetRegionText_FromRegionAndCountryList(int idx, bool withctrl) {
         if (idx < RegionAndCountryDataList.Count)
            return RegionAndCountryDataList[idx].GetRegionText(this, withctrl);
         return null;
      }

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="CityAndRegionOrCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetRegionText_FromCityAndRegionOrCountryDataList(int idx, bool withctrl) {
         if (idx < CityAndRegionOrCountryDataList.Count)
            return CityAndRegionOrCountryDataList[idx].GetRegionText(this, withctrl);
         return null;
      }


      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="CountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetCountryText_FromCountryList(int idx, bool withctrl) {
         if (idx < CountryDataList.Count) // Index 1-basiert!
            return CountryDataList[idx].GetText(this, withctrl);
         return null;
      }

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="RegionAndCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
      /// <returns></returns>
      string? GetCountryText_FromRegionAndCountryList(int idx, bool withctrl) {
         if (idx < RegionAndCountryDataList.Count)
            return RegionAndCountryDataList[idx].GetCountryText(this, withctrl);
         return null;
      }

      /// <summary>
      /// liefert den Text des Landes oder null (Offset 0 liefert i.A. null) aus der <see cref="CityAndRegionOrCountryDataList"/>
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="withctrl"></param>
      /// <returns></returns>
      string? GetCountryText_FromCityAndRegionOrCountryDataList(int idx, bool withctrl) {
         if (idx < CityAndRegionOrCountryDataList.Count)
            return CityAndRegionOrCountryDataList[idx].GetCountryText(this, withctrl);
         return null;
      }

      #endregion

      public override void ReadHeader(BinaryReaderWriter reader) {
         livereader = reader;

         readCommonHeader(reader, Type);

         TextBlock = new DataBlock(reader);
         DataOffsetMultiplier = reader.ReadByte();
         EncodingType = reader.ReadByte();
         CountryBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x29);
         RegionBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x37);
         CityBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x45);
         POIIndexBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x53);
         POIPropertiesBlock = new DataBlock(reader);
         POIOffsetMultiplier = reader.ReadByte();
         POIGlobalMask = (POIFlags)reader.ReadByte();
         reader.ReadBytes(Unknown_0x61);
         POITypeIndexBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x6E);
         ZipBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x7C);
         HighwayWithExitBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x8A);
         ExitBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x98);
         HighwayExitBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0xA6);

         // --------- Headerlänge > 170 Byte
         if (Headerlength > 0xaa) {
            ushort tmp = reader.Read2AsUShort();
            if (tmp > 0)
               Codepage = tmp;
            ID1 = reader.Read2AsUShort();
            ID2 = reader.Read2AsUShort();
            SortDescriptorDefBlock = new DataBlock(reader);
            Lbl13Block = new DataBlockWithRecordsize(reader);
            reader.ReadBytes(Unknown_0xC2);

            // --------- Headerlänge > 196 Byte
            if (Headerlength > 0xc4) {
               TidePredictionBlock = new DataBlockWithRecordsize(reader);
               if (Headerlength > 0xCE) {
                  reader.ReadBytes(Unknown_0xCE);
                  if (Headerlength > 0xD0) {
                     UnknownBlock_0xD0 = new DataBlock(reader);
                     if (Headerlength > 0xD8) {
                        reader.ReadBytes(Unknown_0xD8);
                        if (Headerlength > 0xDE) {
                           UnknownBlock_0xDE = new DataBlock(reader);
                           if (Headerlength > 0xE6) {
                              reader.ReadBytes(Unknown_0xE6);
                              if (Headerlength > 0xEC) {
                                 UnknownBlock_0xEC = new DataBlock(reader);
                                 if (Headerlength > 0xF4) {
                                    reader.ReadBytes(Unknown_0xF4);
                                    if (Headerlength > 0xFA) {
                                       UnknownBlock_0xFA = new DataBlock(reader);
                                       if (Headerlength > 0x102) {
                                          reader.ReadBytes(Unknown_0x102);
                                          if (Headerlength > 0x108) {
                                             UnknownBlock_0x108 = new DataBlock(reader);
                                             if (Headerlength > 0x110) {
                                                reader.ReadBytes(Unknown_0x110);
                                                if (Headerlength > 0x116) {
                                                   UnknownBlock_0x116 = new DataBlock(reader);
                                                   if (Headerlength > 0x11E) {
                                                      reader.ReadBytes(Unknown_0x11E);
                                                      if (Headerlength > 0x124) {
                                                         UnknownBlock_0x124 = new DataBlock(reader);
                                                         if (Headerlength > 0x12C) {
                                                            reader.ReadBytes(Unknown_0x12C);
                                                            if (Headerlength > 0x132) {
                                                               UnknownBlock_0x132 = new DataBlock(reader);
                                                               if (Headerlength > 0x13A) {
                                                                  reader.ReadBytes(Unknown_0x13A);
                                                                  if (Headerlength > 0x140) {
                                                                     UnknownBlock_0x140 = new DataBlock(reader);
                                                                     if (Headerlength > 0x148) {
                                                                        reader.ReadBytes(Unknown_0x148);
                                                                        if (Headerlength > 0x14E) {
                                                                           UnknownBlock_0x14E = new DataBlock(reader);
                                                                           if (Headerlength > 0x156) {
                                                                              reader.ReadBytes(Unknown_0x156);
                                                                              if (Headerlength > 0x15A) {
                                                                                 UnknownBlock_0x15A = new DataBlock(reader);
                                                                                 if (Headerlength > 0x162) {
                                                                                    reader.ReadBytes(Unknown_0x162);
                                                                                    if (Headerlength > 0x168) {
                                                                                       UnknownBlock_0x168 = new DataBlock(reader);
                                                                                       if (Headerlength > 0x170) {
                                                                                          reader.ReadBytes(Unknown_0x170);
                                                                                          if (Headerlength > 0x176) {
                                                                                             UnknownBlock_0x176 = new DataBlock(reader);
                                                                                             if (Headerlength > 0x17E) {
                                                                                                reader.ReadBytes(Unknown_0x17E);
                                                                                                if (Headerlength > 0x184) {
                                                                                                   UnknownBlock_0x184 = new DataBlock(reader);
                                                                                                   if (Headerlength > 0x18C) {
                                                                                                      reader.ReadBytes(Unknown_0x18C);
                                                                                                      if (Headerlength > 0x192) {
                                                                                                         UnknownBlock_0x192 = new DataBlock(reader);
                                                                                                         if (Headerlength > 0x19A) {
                                                                                                            UnknownBlock_0x19A = new DataBlock(reader);
                                                                                                            if (Headerlength > 0x1A2) {
                                                                                                               reader.ReadBytes(Unknown_0x1A2);
                                                                                                               if (Headerlength > 0x1A6) {
                                                                                                                  UnknownBlock_0x1A6 = new DataBlock(reader);
                                                                                                                  if (Headerlength > 0x1AE) {
                                                                                                                     reader.ReadBytes(Unknown_0x1AE);
                                                                                                                     if (Headerlength > 0x1B2) {
                                                                                                                        UnknownBlock_0x1B2 = new DataBlock(reader);
                                                                                                                        if (Headerlength > 0x1BA) {
                                                                                                                           reader.ReadBytes(Unknown_0x1BA);
                                                                                                                           if (Headerlength > 0x1BE) {
                                                                                                                              UnknownBlock_0x1BE = new DataBlock(reader);
                                                                                                                              if (Headerlength > 0x1C6) {
                                                                                                                                 reader.ReadBytes(Unknown_0x1C6);
                                                                                                                                 if (Headerlength > 0x1CA) {
                                                                                                                                    UnknownBlock_0x1CA = new DataBlock(reader);
                                                                                                                                    if (Headerlength > 0x1D2) {
                                                                                                                                       reader.ReadBytes(Unknown_0x1D2);
                                                                                                                                       if (Headerlength > 0x1D8) {
                                                                                                                                          UnknownBlock_0x1D8 = new DataBlock(reader);
                                                                                                                                          if (Headerlength > 0x1E0) {
                                                                                                                                             reader.ReadBytes(Unknown_0x1E0);
                                                                                                                                             if (Headerlength > 0x1E6) {
                                                                                                                                                UnknownBlock_0x1E6 = new DataBlock(reader);
                                                                                                                                                if (Headerlength > 0x1EE) {
                                                                                                                                                   reader.ReadBytes(Unknown_0x1EE);
                                                                                                                                                   if (Headerlength > 0x1F2) {
                                                                                                                                                      UnknownBlock_0x1F2 = new DataBlock(reader);
                                                                                                                                                      if (Headerlength > 0x1FA) {
                                                                                                                                                         reader.ReadBytes(Unknown_0x1FA);
                                                                                                                                                         if (Headerlength > 0x200) {
                                                                                                                                                            UnknownBlock_0x200 = new DataBlock(reader);

                                                                                                                                                            if (Headerlength > 0x208) {
                                                                                                                                                               Unknown_0x208 = new byte[Headerlength - 0x208];
                                                                                                                                                               reader.ReadBytes(Unknown_0x208);
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

      public override void ReadMinimalSections(BinaryReaderWriter reader) {
         livereader = reader;

         //DataBlockWithRecordsize dataBlockWithRecordsize;

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(POIIndexBlock, 0);
         //if (dataBlockWithRecordsize.Length > 0)
         Decode_SortDescriptorDefBlock(reader, POIIndexBlock);

         dataoffsetMultiplier = (uint)(0x01 << DataOffsetMultiplier);
         poioffsetMultiplier = (uint)(0x01 << POIOffsetMultiplier);
         codec = new LabelCodec(EncodingType, Codepage);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(CountryBlock, (ushort)CountryRecord.DataLength);
         Decode_CountryBlock(reader, CountryBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(RegionBlock, (ushort)RegionAndCountryRecord.DataLength);
         Decode_RegionBlock(reader, RegionBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(CityBlock, (ushort)CityAndRegionOrCountryRecord.DataLength);
         Decode_CityBlock(reader, CityBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(POIIndexBlock, (ushort)PointIndexRecord.DataLength);
         Decode_POIIndexBlock(reader, POIIndexBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(POIPropertiesBlock, 0);
         Decode_POIPropertiesBlock(reader, new DataBlockWithRecordsize(POIPropertiesBlock, 0));

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(POITypeIndexBlock, (ushort)PointTypeIndexRecord.DataLength);
         Decode_POITypeIndexBlock(reader, POITypeIndexBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(ZipBlock, (ushort)ZipRecord.DataLength);
         Decode_ZipBlock(reader, ZipBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(HighwayWithExitBlock, (ushort)HighwayWithExitRecord.DataLength);
         Decode_HighwayWithExitBlock(reader, HighwayWithExitBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(HighwayExitBlock, (ushort)HighwayExitBlock.Recordsize);
         Decode_HighwayExitBlock(reader, HighwayExitBlock);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(ExitBlock, (ushort)ExitRecord.DataLength);
         Decode_ExitBlock(reader, ExitBlock);


         //dataBlockWithRecordsize = new DataBlockWithRecordsize(POIIndexBlock, 0);
         //Decode_Lbl13Block(new FilePart(reader, (int)dataBlockWithRecordsize.Offset, (int)dataBlockWithRecordsize.Length), dataBlockWithRecordsize);

         //dataBlockWithRecordsize = new DataBlockWithRecordsize(POIIndexBlock, 0);
         //Decode_TidePredictionBlock(new FilePart(reader, (int)dataBlockWithRecordsize.Offset, (int)dataBlockWithRecordsize.Length), dataBlockWithRecordsize);
      }

      /// <summary>
      /// live-decoding
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="adr"></param>
      /// <returns></returns>
      string decode_Text(int adr) {
         if (livereader != null) {
            livereader.Seek(adr);
            byte[] buff = livereader.ReadBytes(256);
            return codec.Decode(buff, 0);
         }
         return string.Empty;
      }

      public string? GetText(uint offset, bool withctrl) {
         string txt = decode_Text((int)(dataoffsetMultiplier * offset + (TextBlock != null ? TextBlock.Offset : 0)));
         if (withctrl)
            return txt;
         else {
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

      #region Decodierung der Datenblöcke

      /// <summary>
      /// alle Label-Texte einlesen
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="block"></param>
      void Decode_TextBlock(BinaryReaderWriter reader, DataBlock block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            reader.Seek(block.Offset);

            TextList = getNewTextBag();
            byte[] buff = reader.ReadBytes((int)block.Length); // gesamter codierter Textblock

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
            ((TextBagRO)TextList).Freeze();
         }
      }

      /// <summary>
      /// alle Länderoffsets einlesen
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="block"></param>
      void Decode_CountryBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            CountryDataList = reader.ReadArray<CountryRecord>(block);
         }
      }

      /// <summary>
      /// alle Regionsdaten einlesen
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="block"></param>
      void Decode_RegionBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            RegionAndCountryDataList = reader.ReadArray<RegionAndCountryRecord>(block);
         }
      }

      /// <summary>
      /// alle Städtedaten einlesen
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="block"></param>
      void Decode_CityBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            CityAndRegionOrCountryDataList = reader.ReadArray<CityAndRegionOrCountryRecord>(block);
         }
      }

      void Decode_POIIndexBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            PointIndexList4RGN = reader.ReadArray<PointIndexRecord>(block);
         }
      }

      void Decode_POIPropertiesBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            object[] data = new object[1];
            data[0] = (UInt16)((UInt16)POIGlobalMask |
                               ((UInt16)((CityAndRegionOrCountryDataList.Count < 256 ? PointDataRecord.SpecPropFlags.ShortCityIndex : 0) |
                                         (ZipDataList.Count < 256 ? PointDataRecord.SpecPropFlags.ShortZipIndex : 0) |
                                         (HighwayWithExitList.Count < 256 ? PointDataRecord.SpecPropFlags.ShortExitHighwayIndex : 0) |
                                         (ExitList.Count < 256 ? PointDataRecord.SpecPropFlags.ShortExitIndex : 0)) << 8));
            PointPropertiesList = reader.ReadArray<PointDataRecord>(block, data, PointPropertiesListOffsets);
         }
      }

      void Decode_POITypeIndexBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            PointTypeIndexList4RGN = reader.ReadArray<PointTypeIndexRecord>(block);
         }
      }

      /// <summary>
      /// alle ZIP-Offsets einlesen
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="block"></param>
      void Decode_ZipBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            ZipDataList = reader.ReadArray<ZipRecord>(block);
         }
      }

      void Decode_HighwayWithExitBlock(BinaryReaderWriter reader, DataBlockWithRecordsize? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            HighwayWithExitList = reader.ReadArray<HighwayWithExitRecord>(new DataBlock(block));
         }
      }

      void Decode_ExitBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            ExitList = reader.ReadArray<ExitRecord>(block);
         }
      }

      /// <summary>
      /// vorher muss <see cref="Decode_HighwayWithExitBlock"/>() erfolgt sein
      /// </summary>
      /// <param name="reader"></param>
      /// <param name="block"></param>
      void Decode_HighwayExitBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            // Liste der Datensatzlängen für die HighwayDataList erzeugen
            object[] dslen = new object[HighwayWithExitList.Count];
            for (int i = 0; i < HighwayWithExitList.Count; i++)
               dslen[i] = (int)(3 * (HighwayWithExitList[i].FirstExitOffset - 1));      // Startoffset
            for (int i = 0; i < dslen.Length - 1; i++)
               dslen[i] = (int)dslen[i + 1] - (int)dslen[i];
            if (dslen.Length > 0)
               dslen[dslen.Length - 1] = (int)block.Length - (int)dslen[dslen.Length - 1];
            HighwayExitDefList = reader.ReadArray<HighwayExitDefRecord>(block, dslen);
         }
      }

      void Decode_SortDescriptorDefBlock(BinaryReaderWriter reader, DataBlock? block) {
         if (reader != null &&
             block != null &&
             block.Length > 0) {
            reader.Seek((int)block.Offset);
            SortDescriptor = reader.ReadString((int)block.Length);
         }
      }

      #endregion

   }
}
