using System;
using System.Collections.Generic;

#pragma warning disable IDE1006

namespace GarminCore.OptimizedReader {

   /// <summary>
   /// Infos über routable-Straßen
   /// </summary>
   public class StdFile_NET : StdFile {

      #region Header-Daten

      /// <summary>
      /// Road definitions (0x15)
      /// </summary>
      public DataBlock? RoadDefinitionsBlock { get; private set; }

      /// <summary>
      /// Road definitions offset multiplier (power of 2) (0x1D)
      /// </summary>
      byte RoadDefinitionsOffsetMultiplier;

      /// <summary>
      /// Segmented roads (0x1E)
      /// </summary>
      public DataBlock? SegmentedRoadsBlock { get; private set; }

      /// <summary>
      /// Segmented roads offset multiplier (power of 2) (0x26)
      /// </summary>
      byte SegmentedRoadsOffsetMultiplier;

      /// <summary>
      /// Sorted roads (0x27)
      /// </summary>
      public DataBlockWithRecordsize? SortedRoadsBlock { get; private set; }

      byte[] Unknown_0x31 = new byte[4];
      byte Unknown_0x35;
      byte Unknown_0x36;
      byte[] Unknown_0x37 = new byte[4];
      byte[] Unknown_0x3B = new byte[8];
      DataBlock UnknownBlock_0x43;
      byte Unknown_0x4B;
      DataBlock UnknownBlock_0x4C;
      byte[] Unknown_0x54 = new byte[2];
      DataBlock UnknownBlock_0x56;
      byte[] Unknown_0x5E = new byte[6];

      #endregion


      /// <summary>
      /// Daten einer Straße
      /// </summary>
      internal class RoadData : BinaryReaderWriter.DataStruct {

         [Flags]
         public enum NodInfo : byte {
            unknown = 0x00,
            /// <summary>
            /// NOD-Offset ist 2-Byte-Pointer
            /// </summary>
            two_byte_pointer = 0x01,
            /// <summary>
            /// NOD-Offset ist 3-Byte-Pointer
            /// </summary>
            three_byte_pointer = 0x02,

            // weitere Bits unbekannt
         }

         [Flags]
         public enum RoadDataInfo : byte {
            unknown = 0x00,
            unknown0 = 0x01,
            oneway = 0x02,
            lock2road_shownextroad = 0x04,
            unknown3 = 0x08,
            has_street_address_info = 0x10,
            addr_start_right = 0x20,
            has_nod_info = 0x40,
            major_highway = 0x80
         }

         public RoadData() {
            LabelInfo = new List<uint>();
            SegmentedRoadOffsets = new List<uint>();
            Roaddatainfos = RoadDataInfo.unknown;
            RoadLength = 0;
            RgnIndexOverview = new List<byte>();
            Indexdata = new IndexData[0];
            NODFlag = NodInfo.unknown;
            NOD_Offset = 0;
         }

         /// <summary>
         /// Verweis in die Straßenliste einer Subdiv
         /// </summary>
         internal class IndexData {
            public readonly byte RoadIndex;
            public readonly UInt16 Subdivisionnumber;

            public IndexData(byte roadIndex, UInt16 subdivisionnumber) {
               RoadIndex = roadIndex;
               Subdivisionnumber = subdivisionnumber;
            }

            public override string ToString() {
               return string.Format("Subdivisionnumber {0}, RoadIndex {1}", Subdivisionnumber, RoadIndex);
            }
         }

         public enum Side {
            Left = 0, Right = 1
         }

         internal class Housenumbers {

            internal class Housenumbers4Node {

               public readonly int Idx;
               public readonly NumberStyle LeftStyle;
               public readonly int LeftFrom;
               public readonly int LeftTo;
               public readonly NumberStyle RightStyle;
               public readonly int RightFrom;
               public readonly int RightTo;

               /// <summary>
               /// tatsächlich gelesene Daten
               /// </summary>
               public RawData4Node? rawdata;

               public Housenumbers4Node() {
                  Idx = -1;
                  LeftStyle = RightStyle = NumberStyle.None;
                  LeftFrom = LeftTo = 0;
                  RightFrom = RightTo = 0;
               }

               public Housenumbers4Node(int idx,
                                        NumberStyle leftstyle, int leftfrom, int leftto,
                                        NumberStyle rightstyle, int rightfrom, int rightto) {
                  Idx = idx;
                  LeftStyle = leftstyle;
                  LeftFrom = leftfrom;
                  LeftTo = leftto;
                  RightStyle = rightstyle;
                  RightFrom = rightfrom;
                  RightTo = rightto;
               }

               public override string ToString() {
                  string txt = "Idx " + Idx.ToString() + ", Left " + LeftStyle.ToString();
                  if (LeftStyle != NumberStyle.None) {
                     txt += string.Format(" {0}-{1}", LeftFrom, LeftTo);
                  }
                  txt += ", Right ";
                  txt += RightStyle.ToString();
                  if (RightStyle != NumberStyle.None) {
                     txt += string.Format(" {0}-{1}", RightFrom, RightTo);
                  }
                  return txt;
               }

            }

            internal class RawData4Node {
               /// <summary>
               /// Basiswert für die Berechnung der 1. (oder einzigen) Seite
               /// </summary>
               public int base1;
               /// <summary>
               /// Basiswert für die Berechnung der 2. Seite
               /// </summary>
               public int base2;
               /// <summary>
               /// Wird <see cref="diffstart2"/> auf <see cref="diffstart1"/> gesetzt?
               /// </summary>
               public bool diffstartisequal;
               /// <summary>
               /// Bei <see cref="diffstartisequal"/>==true wird angegeben ob <see cref="base2"/>=<see cref="base1"/> gesetzt wird oder andersherum.
               /// </summary>
               public bool commonbase1;
               /// <summary>
               /// Kann ein Wert für <see cref="diffend2"/> überhaupt angegeben werden?
               /// </summary>
               public bool diffend2ispossible;
               public int diffstart1, diffend1, diffstart2, diffend2;

               public RawData4Node() {
                  diffstartisequal = commonbase1 = diffend2ispossible = false;
                  base1 = base2 = diffstart1 = diffend1 = diffstart2 = diffend2 = 0;
               }

               public override string ToString() {
                  return string.Format("Flags: diffstartisequal={0}, commonbase1={1}, diffend2ispossible={2} / Numbers: base1={3}, diffstart1={4}, diffend1={5}, base2={6}, diffstart2={7}, diffend2={8}",
                                       diffstartisequal,
                                       commonbase1,
                                       diffend2ispossible,
                                       base1,
                                       diffstart1,
                                       diffend1,
                                       base2,
                                       diffstart2,
                                       diffend2);
               }
            }

            public enum NumberStyle {
               None,
               Even,
               Odd,
               Both
            }

            /// <summary>
            /// spez. Bitreader zum Einlesen der Rohdaten
            /// </summary>
            class SpecBitReader : BitReader {

               // The minimum values of the start and end bit widths.
               const int START_WIDTH_MIN = 5;
               const int END_WIDTH_MIN = 2;

               public int startminWidth;           // min. Bitanzahl (außer Vorzeichen)
               public int startaddWidth;           // ist 0 bzw. der Zusatz (!) gegenüber startminWidth
               public bool startnegative;
               public bool startsigned;

               public int endminWidth;           // min. Bitanzahl (außer Vorzeichen)
               public int endaddWidth;           // ist 0 bzw. der Zusatz (!) gegenüber endminWidth
               public bool endnegative;
               public bool endsigned;


               public SpecBitReader(byte[] bits,
                                    bool startnegative,
                                    bool startsigned,
                                    int startaddWidth,
                                    bool endnegative,
                                    bool endsigned,
                                    int endaddWidth) : base(bits) {
                  this.startnegative = startnegative;
                  this.startsigned = startsigned;
                  startminWidth = START_WIDTH_MIN;
                  this.startaddWidth = startaddWidth;
                  this.endnegative = startnegative;
                  this.endsigned = startsigned;
                  endminWidth = END_WIDTH_MIN;
                  this.endaddWidth = endaddWidth;
               }

               /// <summary>
               /// liest nb Bits als Zahl
               /// </summary>
               /// <param name="nb">Bitanzahl</param>
               /// <param name="start">Start- oder Endwert</param>
               /// <returns>int.MinValue wenn nicht genug Bits im Bitstreams</returns>
               public int GetN(int nb, bool start) {
                  if (RestBits() < nb)
                     return int.MinValue;
                  int v;
                  if (start) {
                     if (startsigned) {
                        v = SGet(nb);
                     } else {
                        v = Get(nb);
                        if (startnegative)
                           v = -v;
                     }
                  } else {
                     if (endsigned) {
                        v = SGet(nb);
                     } else {
                        v = Get(nb);
                        if (endnegative)
                           v = -v;
                     }
                  }
                  return v;
               }

               /// <summary>
               /// liest eine Zahl entsprechend dem akt. Format ein
               /// </summary>
               /// <param name="start">Start- oder Endwert</param>
               /// <returns>int.MinValue wenn nicht genug Bits im Bitstreams</returns>
               public int Get(bool start) {
                  if (start)
                     return GetN(startminWidth + startaddWidth + (startsigned ? 1 : 0), start);
                  else
                     return GetN(endminWidth + endaddWidth + (endsigned ? 1 : 0), start);
               }

               /// <summary>
               /// liest den Initialisierungwert ein
               /// </summary>
               /// <returns>int.MinValue wenn nicht genug Bits im Bitstreams</returns>
               public int ReadInitialValue() {
                  if (RestBits() < 6)
                     return int.MinValue;
                  bool width5 = Get1();
                  if (width5) {
                     return Get(5);
                  } else {
                     int w = Get(4);
                     if (RestBits() < w)
                        return int.MinValue;
                     return Get(w);
                  }
               }

               /// <summary>
               /// liest 2 Bit als <see cref="NumberStyle"/>
               /// </summary>
               /// <returns></returns>
               NumberStyle ReadNumberStyle() {
                  switch (Get(2)) {
                     case 0:
                        return NumberStyle.None;
                     case 1:
                        return NumberStyle.Even;
                     case 2:
                        return NumberStyle.Odd;
                     case 3:
                        return NumberStyle.Both;
                  }
                  return NumberStyle.None;
               }

               /// <summary>
               /// liefert den NumberStyle links und rechts, falls zunächst 2 0-Bits folgen
               /// </summary>
               /// <returns>null wenn kein NumberStyle oder nicht genug Bits im Bitstreams</returns>
               public NumberStyle[]? ReadNumberStyles() {
                  if (RestBits() < 2)
                     return null;
                  if (Get(2) == 0) {
                     if (RestBits() < 4)
                        return null;
                     NumberStyle[] st = new NumberStyle[2];
                     st[0] = ReadNumberStyle();
                     st[1] = ReadNumberStyle();
                     return st;
                  } else
                     Seek(-2, SeekOrigin.Current);
                  return null;
               }

               /// <summary>
               /// liefert einen Skip-Wert, falls zunächst 110 folgt
               /// </summary>
               /// <returns>kleiner 0 wenn nicht genug Bits im Bitstreams</returns>
               public int ReadSkip() {
                  if (RestBits() < 3)
                     return -1;

                  if (Get(3) == 6) { // 0b011
                     if (RestBits() < 1)
                        return -1;
                     bool width10 = Get1();
                     if (width10) {
                        if (RestBits() < 10)
                           return -1;
                        return Get(10);
                     } else {
                        if (RestBits() < 5)
                           return -1;
                        return Get(5);
                     }
                  } else
                     Seek(-3, SeekOrigin.Current);

                  return 0;
               }

               /// <summary>
               /// falls zunächst 4 Bits mit dem Wert 2 oder 10 folgen wird ein Zahlenformat eingelesen
               /// </summary>
               /// <param name="initial">bei true entfällt der Test der ersten 4 Bit</param>
               /// <param name="forstart"></param>
               /// <param name="negative"></param>
               /// <param name="signed"></param>
               /// <param name="add"></param>
               /// <returns>false wenn nicht genug Bits im Bitstreams</returns>
               public bool ReadFormat(bool initial, out bool forstart, out bool negative, out bool signed, out int add) {
                  forstart = negative = signed = false;
                  add = 0;
                  if (!initial) {
                     if (RestBits() < 10)
                        return false;
                     switch (Get(4)) {
                        case 2:
                           forstart = true;
                           break;
                        case 10:
                           forstart = false;
                           break;
                        default:
                           Seek(-4, SeekOrigin.Current);
                           return false;
                     }
                  } else {
                     if (RestBits() < 6)
                        return false;
                  }

                  negative = Get1();
                  signed = Get1();
                  add = Get(4);
                  return true;
               }

               /// <summary>
               /// liefert die Rohdaten für die Nummern
               /// </summary>
               /// <param name="isSingleSide"></param>
               /// <returns>null wenn nicht genug Bits im Bitstreams</returns>
               public RawData4Node? ReadRawNumbers(bool isSingleSide) {
                  if (RestBits() < 6 ||
                      !Get1())      // 1-Bit nötig
                     return null;

                  RawData4Node? rawdata = new RawData4Node();

                  bool diffstartexist = false;
                  bool diffendexist = false;

                  if (!isSingleSide) { // dann folgen 2 oder Flags
                     rawdata.diffstartisequal = Get1();
                     if (rawdata.diffstartisequal)
                        rawdata.commonbase1 = Get1();
                     rawdata.diffend2ispossible = !Get1();
                  }

                  diffstartexist = !Get1();  // Ist der Startwert der 1. Seite angegeben?
                  diffendexist = !Get1();    // Ist der Endwert der 1. Seite angegeben?
                  if (diffstartexist)
                     rawdata.diffstart1 = Get(true);
                  if (rawdata.diffstart1 != int.MinValue) {
                     if (diffendexist)
                        rawdata.diffend1 = Get(false);
                     if (rawdata.diffend1 != int.MinValue) {

                        if (!isSingleSide) {
                           diffstartexist = false;
                           diffendexist = false;

                           if (!rawdata.diffstartisequal) {
                              if (RestBits() < 1)
                                 return null;
                              diffstartexist = !Get1();     // Ist der Startwert der 2. Seite angegeben?
                           } else {
                              if (RestBits() < 1)
                                 return null;
                              rawdata.diffstart2 = rawdata.diffstart1;
                           }
                           if (rawdata.diffend2ispossible) {
                              if (RestBits() < 1)
                                 return null;
                              diffendexist = !Get1();       // Ist der Endwert der 2. Seite angegeben?
                           } else
                              rawdata.diffend2 = rawdata.diffend1;

                           if (diffstartexist)
                              rawdata.diffstart2 = Get(true);
                           if (diffendexist)
                              rawdata.diffend2 = Get(false);
                        }
                     }
                  }

                  if (rawdata.diffstart1 == int.MinValue ||
                      rawdata.diffend1 == int.MinValue ||
                      rawdata.diffstart2 == int.MinValue ||
                      rawdata.diffend2 == int.MinValue)
                     rawdata = null;

                  return rawdata;
               }


               int RestBits() {
                  return Length - Position - 1;
               }

            }

            public List<Housenumbers4Node> Numbers;


            public Housenumbers() {
               Numbers = new List<Housenumbers4Node>();
            }

            public Housenumbers(byte[] codedbytes) : this() {
               SpecBitReader sr = new SpecBitReader(codedbytes, false, false, 0, false, false, 0);

               bool forstart;
               bool negative;
               bool signed;
               int add;
               if (sr.ReadFormat(true, out forstart, out negative, out signed, out add)) {
                  sr.startnegative = negative;
                  sr.startsigned = signed;
                  sr.startaddWidth = add;
                  if (sr.ReadFormat(true, out forstart, out negative, out signed, out add)) {
                     sr.endnegative = negative;
                     sr.endsigned = signed;
                     sr.endaddWidth = add;

                     int init = sr.ReadInitialValue();
                     if (init != int.MinValue) {
                        int idx = -1;
                        NumberStyle[] nr = new NumberStyle[] { NumberStyle.Odd, NumberStyle.Even };

                        do {  // jede Node lesen
                           // ev. Skip Nodes
                           int skip = sr.ReadSkip();
                           if (skip >= 0) {
                              idx += skip + 1;
                              // ev. neuer NumberStyle
                              NumberStyle[]? nrtmp = sr.ReadNumberStyles();
                              if (nrtmp != null) {
                                 nr[0] = nrtmp[0];
                                 nr[1] = nrtmp[1];
                              }

                              // ev. 1 oder 2 neue Zahlenformate
                              for (int i = 0; i < 2; i++) {
                                 if (sr.ReadFormat(false, out forstart, out negative, out signed, out add)) {
                                    if (forstart) {
                                       sr.startnegative = negative;
                                       sr.startsigned = signed;
                                       sr.startaddWidth = add;
                                    } else {
                                       sr.endnegative = negative;
                                       sr.endsigned = signed;
                                       sr.endaddWidth = add;
                                    }
                                 } else
                                    break;
                              }

                              // jetzt müssen Zahlen folgen
                              bool isSingleSide = !(nr[0] != NumberStyle.None && nr[1] != NumberStyle.None);
                              RawData4Node? rawdata = sr.ReadRawNumbers(isSingleSide);
                              if (rawdata != null) { // es folgen Nummern
                                 if (Numbers.Count == 0) {
                                    rawdata.base1 = init;
                                    rawdata.base2 = init;
                                 } else {
                                    Housenumbers4Node lastnums = Numbers[Numbers.Count - 1];
                                    bool lastIsSingleSide = lastnums.LeftStyle == NumberStyle.None || lastnums.RightStyle == NumberStyle.None;

                                    // letzte Basiswerte übernehmen ...
                                    // letzte Basiswerte übernehmen ...
                                    if (lastnums.rawdata != null) {
                                       rawdata.base1 = lastnums.rawdata.base1;
                                       rawdata.base2 = lastnums.rawdata.base2;

                                       // ... und ev. anpassen
                                       if (!lastIsSingleSide) {
                                          rawdata.base1 = lastnums.LeftTo + 1;
                                          if (lastnums.LeftFrom > lastnums.LeftTo)
                                             rawdata.base1 -= 2;

                                          rawdata.base2 = lastnums.RightTo + 1;
                                          if (lastnums.RightFrom > lastnums.RightTo)
                                             rawdata.base2 -= 2;
                                       } else {

                                          if (lastnums.LeftStyle != NumberStyle.None) {
                                             rawdata.base1 = lastnums.LeftTo + 1;
                                             if (lastnums.LeftFrom > lastnums.LeftTo)
                                                rawdata.base1 -= 2;
                                          } else if (lastnums.RightStyle != NumberStyle.None) {
                                             rawdata.base1 = lastnums.RightTo + 1;
                                             if (lastnums.RightFrom > lastnums.RightTo)
                                                rawdata.base1 -= 2;
                                          }

                                       }

                                       if (rawdata.diffstartisequal)
                                          if (rawdata.commonbase1)
                                             rawdata.base2 = rawdata.base1;
                                          else
                                             rawdata.base1 = rawdata.base2;

                                    }

                                 }

                                 /*
                                 Hat nur eine Seite Nummern, ergibt sich diese Seite aus den beiden NumberStyles.

                                 Haben beide Seiten Nummern wird erst die linke, danach die rechte Seite angegeben.

                                 Berechnung aus den Differenzwerten:
                                 - diffstart und diffend sind zunächst 0
                                 - falls diffend gesetzt wird, folgt 
                                       bei pos. diffend: diffend--
                                       bei neg. diffend: diffend++
                                 - Start = init + diffstart
                                 - passt Start nicht zum NumberStyle der Seite, folgt 
                                       bei pos. diffend: Start++
                                       bei neg. diffend: Start--
                                 - End   = init + diffstart + diffend
                                 - passt End   nicht zum NumberStyle der Seite, folgt 
                                       bei pos. diffend: End--
                                       bei neg. diffend: End++
                                  */

                                 int diffstart1 = rawdata.diffstart1,
                                     diffend1 = rawdata.diffend1,
                                     diffstart2 = rawdata.diffstart2,
                                     diffend2 = rawdata.diffend2;

                                 int from1 = fitfrom(nr[0], rawdata.base1 + diffstart1, diffend1);
                                 int to1 = fitto(nr[0], rawdata.base1 + diffstart1 + diffend1 + (diffend1 > 0 ? -1 : diffend1 < 0 ? 1 : 0), diffend1);

                                 if (isSingleSide) {

                                    if (nr[0] != NumberStyle.None)   // nur linke Seite
                                       Numbers.Add(new Housenumbers4Node(idx, nr[0], from1, to1, NumberStyle.None, 0, 0) { rawdata = rawdata });
                                    else                             // nur rechte Seite
                                       Numbers.Add(new Housenumbers4Node(idx, NumberStyle.None, 0, 0, nr[0], from1, to1) { rawdata = rawdata });

                                 } else { // beide Seiten

                                    int from2 = fitfrom(nr[1], rawdata.base2 + diffstart2, diffend2);
                                    int to2 = fitto(nr[1], rawdata.base2 + diffstart2 + diffend2 + (diffend2 > 0 ? -1 : diffend2 < 0 ? 1 : 0), diffend2);

                                    Numbers.Add(new Housenumbers4Node(idx, nr[0], from1, to1, nr[1], from2, to2) { rawdata = rawdata });

                                 }
                              } else
                                 break;
                           } else
                              break;
                        } while (true);
                     }
                  }
               }
            }

            int fitfrom(NumberStyle style, int from, int diffend) {
               if ((style == NumberStyle.Even && from % 2 == 1) ||
                   (style == NumberStyle.Odd && from % 2 == 0)) {
                  if (diffend > 0) from++;
                  else if (diffend < 0) from--;
               }
               return from;
            }

            int fitto(NumberStyle style, int to, int diffend) {
               if ((style == NumberStyle.Even && to % 2 == 1) ||
                   (style == NumberStyle.Odd && to % 2 == 0)) {
                  if (diffend > 0) to--;
                  else if (diffend < 0) to++;
               }
               return to;
            }


            public override string ToString() {
               return string.Format("Numbers: {0}", Numbers.Count);
            }

         }



         /// <summary>
         /// bis zu 4 Labels je Straße möglich
         /// </summary>
         public List<uint> LabelInfo { get; protected set; }

         List<uint> SegmentedRoadOffsets;

         /// <summary>
         /// Art der vorhandenen Straßendaten
         /// </summary>
         RoadDataInfo Roaddatainfos;

         /// <summary>
         /// (halbe) Länge der Straße in Meter
         /// </summary>
         public uint RoadLength { get; protected set; }

         /// <summary>
         /// Anzahl der <see cref="Indexdata"/>-Items je Level
         /// </summary>
         List<byte> RgnIndexOverview;

         /// <summary>
         /// Liste aller Verweise in Straßenlisten von Subdivs
         /// </summary>
         //List<IndexData> Indexdata;
         IndexData[] Indexdata;

         /// <summary>
         /// Anzahl Nodes und Flags für Zips, Citys usw. (<see cref="NodeCount"/>, <see cref="ZipFlag"/>, <see cref="CityFlag"/>)
         /// </summary>
         UInt16 NodeCountAndFlags;

         /// <summary>
         /// Index-Liste für die Zip-Tabelle links rechts mit dem Node-Index
         /// </summary>
         public Dictionary<int, int>?[]? ZipIndex4Node { get; protected set; }

         /// <summary>
         /// Index-Liste für die City-Tabelle links rechts mit dem Node-Index
         /// </summary>
         public Dictionary<int, int>?[]? CityIndex4Node { get; protected set; }

         /// <summary>
         /// Stream, der die Hausnummernbereiche enthält
         /// </summary>
         byte[]? NumberStream;

         NodInfo NODFlag;

         uint NOD_Offset;

         /// <summary>
         /// Anzahl der Nodes (Bit 0..9)
         /// </summary>
         int NodeCount => NodeCountAndFlags & 0x3FF;

         /// <summary>
         /// Zip-Bits (10 u. 11 für Zip); 0 und 1 stehen für 1 bzw. 2 Byte je Index, 3 für "kein Index"
         /// </summary>
         int ZipFlag => (NodeCountAndFlags >> 10) & 0x03;

         /// <summary>
         /// City-Bits (12 u. 13 für City); 0 und 1 stehen für 1 bzw. 2 Byte je Index, 3 für "kein Index"
         /// </summary>
         int CityFlag => (NodeCountAndFlags >> 12) & 0x03;

         /// <summary>
         /// Hausnummern-Bits (14 u. 15 für Hausnummern);  0, 1 und 2 stehen für 1, 2 bzw. 3 Byte je Nummer, 3 für "keine Nummer"
         /// </summary>
         int NumberFlag => (NodeCountAndFlags >> 14) & 0x03;

         /// <summary>
         /// Anzahl der Bytes, die eingelesen wurden
         /// </summary>
         int RawBytes;


         public override void Read(BinaryReaderWriter br, object? data) {
            if (data == null)
               throw new Exception("RoadData.Read(): Die Daten können ohne gültige LBL-Datei nicht gelesen werden.");

            long startpos = br.Position;

            StdFile_LBL lbl = (StdFile_LBL)data;
            uint tmpu = 0;
            bool label = true;
            do {     // 1..4 Labels
               tmpu = (uint)br.Read3UInt();
               if (label)
                  LabelInfo.Add(tmpu & 0x3FFFFF);
               else
                  SegmentedRoadOffsets.Add(tmpu & 0x3FFFFF);
               if ((tmpu & 0x400000) != 0)        // Bit 22: die nächstens Offsets sind SegmentedRoadOffsets
                  label = false;
            } while ((tmpu & 0x800000) == 0);     // Bit 23: Ende-Bit

            Roaddatainfos = (RoadDataInfo)br.ReadByte();   // writer.put((byte) netFlags);

            RoadLength = (uint)br.Read3UInt();    // writer.put3(roadLength);

            byte tmpb = 0;
            do {
               tmpb = br.ReadByte();
               RgnIndexOverview.Add((byte)(tmpb & 0x7F));
            } while ((tmpb & 0x80) == 0);    // Bit 7: Ende-Bit

            int size = 0;
            for (int i = 0; i < RgnIndexOverview.Count; i++)
               size += RgnIndexOverview[i];
            Indexdata = new IndexData[size];
            size = 0;
            for (int i = 0; i < RgnIndexOverview.Count; i++)
               for (int j = 0; j < RgnIndexOverview[i]; j++)
                  Indexdata[size++] = new IndexData(br.ReadByte(), (ushort)br.Read2UInt()); // road_index (in subdivision) und subdivision_number

            if ((Roaddatainfos & RoadDataInfo.has_street_address_info) == RoadDataInfo.has_street_address_info) {
               NodeCountAndFlags = (ushort)br.Read2UInt();

               ZipIndex4Node = GetIndex4CityOrZip(br, ZipFlag, lbl.ZipDataList.Count < 256 ? 1 : 2);
               CityIndex4Node = GetIndex4CityOrZip(br, CityFlag, lbl.CityAndRegionOrCountryDataList.Count < 256 ? 1 : 2);
               NumberStream = GetNumberStream(br, NumberFlag);
            } else {
               ZipIndex4Node = null;
               CityIndex4Node = null;
               NumberStream = new byte[0];
            }

            if ((Roaddatainfos & RoadDataInfo.has_nod_info) != RoadDataInfo.unknown) {
               NODFlag = (NodInfo)br.Read1UInt();

               if ((NODFlag & NodInfo.two_byte_pointer) != NodInfo.unknown)
                  NOD_Offset = (uint)br.Read2UInt();
               else
                  NOD_Offset = (uint)br.Read3UInt();
            }

            RawBytes = (int)(br.Position - startpos); // Anzahl der Bytes, die gelesen wurden
         }

         public override void Write(BinaryReaderWriter bw, object? data) { }

         /// <summary>
         /// liefert den Node/Index-Dictionarys links [0] und rechts [1] für die Zip- oder City-Tabelle
         /// </summary>
         /// <param name="br"></param>
         /// <param name="flag">0, 1, 2 oder 3</param>
         /// <param name="idxsize">Anzahl der Bytes für den Index (oder die Speicherlänge bei flag=2)</param>
         /// <returns>Liste der eingelesenen Indexe links und rechts</returns>
         Dictionary<int, int>?[]? GetIndex4CityOrZip(BinaryReaderWriter br, int flag, int idxsize) {
            int n;
            Dictionary<int, int>?[]? indexes = null;
            switch (flag) {
               case 0x0:
                  return parseList(br, br.Read1UInt(), idxsize);    // 1 Byte für Datenbereichslänge für die Liste

               case 0x1:
                  return parseList(br, br.Read2UInt(), idxsize);    // 2 Byte für Datenbereichslänge für die Liste

               case 0x2:
                  switch (idxsize) {
                     case 1:
                        n = getCityOrZip(br, idxsize);            // 1 Byte für Einzel-Index
                        break;
                     case 2:
                        n = getCityOrZip(br, idxsize);            // 2 Byte für Einzel-Index
                        break;
                     default:
                        throw new Exception("GetCityZip(): Dekodierung nicht bekannt.");
                  }
                  if (n != 0) {
                     indexes = [
                        new Dictionary<int, int>(),
                        null,
                     ];
                     indexes[0]?.Add(-1, n - 1);
                  }
                  break;

               case 0x3:              // keine Daten vorhanden
                  break;
            }
            return indexes;
         }

         /// <summary>
         /// aus MKGMAP
         /// </summary>
         /// <param name="br"></param>
         /// <param name="datalen">Länge des Datenbereiches</param>
         /// <param name="size">1 oder 2 Byte für Daten</param>
         /// <returns>Liste der Dictionarys (node, index) für links und rechts</returns>
         Dictionary<int, int>?[]? parseList(BinaryReaderWriter br, int datalen, int size) {
            Dictionary<int, int>? indexesleft = null;
            Dictionary<int, int>? indexesright = null;
            long endPos = br.Position + datalen;
            int node = 0; // not yet used
            while (br.Position < endPos) {
               int initFlag = br.ReadByte() & 0xff;
               int skip = (initFlag & 0x1f);             // niedrigste 5 Bit
               initFlag >>= 5;                           // oberste 3 Bit ...
               if (initFlag == 7) {                      // ... == 0b111
                  // Need to read another byte
                  initFlag = br.ReadByte() & 0xff;
                  skip |= ((initFlag & 0x1f) << 5);      // skip += (niedrigste 5 Bit << 5)
                  initFlag >>= 5;                        // oberste 3 Bit
               }
               node += skip + 1;
               int right = 0, left = 0;
               if (initFlag == 0) {                      // 0b000
                  right = left = getCityOrZip(br, size);
               } else if ((initFlag & 0x4) != 0) {       // 0b1**
                  if ((initFlag & 1) == 0)               // 0b1*1
                     right = 0;
                  if ((initFlag & 2) == 0)               // 0b11*
                     left = 0;
               } else {                                  // 0b0**
                  if ((initFlag & 1) != 0)               // 0b0*1
                     left = getCityOrZip(br, size);
                  if ((initFlag & 2) != 0)               // 0b01*
                     right = getCityOrZip(br, size);
               }
               if (left > 0) {
                  if (indexesleft == null)
                     indexesleft = new Dictionary<int, int>();
                  indexesleft.Add(node, left - 1);
               }
               if (right > 0 && left != right) {
                  if (indexesright == null)
                     indexesright = new Dictionary<int, int>();
                  indexesright.Add(node, right - 1);
               }
            }
            return [indexesleft, indexesright];
         }

         /// <summary>
         /// liest 1, 2 oder 3 Byte unsigned ein
         /// </summary>
         /// <param name="br"></param>
         /// <param name="idxsize"></param>
         /// <returns></returns>
         int getCityOrZip(BinaryReaderWriter br, int idxsize) {
            switch (idxsize) {
               case 1:
                  return br.Read1UInt();
               case 2:
                  return br.Read2UInt();
               default:
                  return br.Read3UInt();
            }
         }

         /// <summary>
         /// liefert den Stream, der die Hausnummernbereiche codiert (oder null)
         /// </summary>
         /// <param name="br"></param>
         /// <param name="size4memlen">0 (1 Byte), 1 (2 Byte), 2 (3 Byte) oder 3 (nichts)</param>
         /// <returns></returns>
         byte[]? GetNumberStream(BinaryReaderWriter br, int size4memlen) {
            int n;
            switch (size4memlen) {
               case 0x0:
                  n = br.Read1UInt();
                  return br.ReadBytes(n);

               case 0x1:
                  n = br.Read2UInt();
                  return br.ReadBytes(n);

               case 0x2:
                  n = br.Read3UInt();
                  return br.ReadBytes(n);
               //throw new Exception("unknown number-flag on RoadData");

               case 0x03:              // keine Daten vorhanden
                  return new byte[0];
            }
            return null;
         }

         /// <summary>
         /// liefert alle Zip-Texte der Seite verbunden mit "; " oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="side"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetZipText(StdFile_LBL lbl, Side side, bool withctrl) {
            if (ZipIndex4Node != null) {
               Dictionary<int, int>? dict = ZipIndex4Node[side == Side.Left ? 0 : 1];
               if (dict != null) {
                  string txt = "";
                  foreach (var item in dict) {
                     if (txt.Length > 0)
                        txt += "; ";
                     txt += lbl.GetText_FromZipList(item.Value, withctrl);
                  }
                  if (txt.Length > 0)
                     return txt;
               }
               //int[] tabidx = new int[dict.Count];
               //dict.Values.CopyTo(tabidx, 0);
               //string txt = "";
               //for (int i = 0; i < tabidx.Length; i++) {
               //   if (i > 0)
               //      txt += "; ";
               //   txt += lbl.GetText_FromZipList(tabidx[i], withctrl);
               //}
               //return dict.Count > 0 ? txt : null;
            }
            return null;
         }

         /// <summary>
         /// liefert alle City-Texte der Seite verbunden mit "; " oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="rgn"></param>
         /// <param name="side"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetCityText(StdFile_LBL lbl, StdFile_RGN rgn, Side side, bool withctrl) {
            if (CityIndex4Node != null) {
               Dictionary<int, int>? dict = CityIndex4Node[side == Side.Left ? 0 : 1];
               if (dict != null) {
                  string txt = "";
                  foreach (var item in dict) {
                     if (txt.Length > 0)
                        txt += "; ";
                     txt += lbl.GetCityText_FromCityAndRegionOrCountryDataList(rgn, item.Value, withctrl);
                  }
                  if (txt.Length > 0)
                     return txt;
               }
               //int[] tabidx = new int[dict.Count];
               //dict.Values.CopyTo(tabidx, 0);
               //string txt = "";
               //for (int i = 0; i < tabidx.Length; i++) {
               //   if (i > 0)
               //      txt += "; ";
               //   txt += lbl.GetCityText_FromCityAndRegionOrCountryDataList(rgn, tabidx[i], withctrl);
               //}
               //return dict.Count > 0 ? txt : null;
            }
            return null;
         }


         public override string ToString() {
            return string.Format("Roaddata {0}, RoadLength {1}, LabelInfo {2}, SegmentedRoadOffsets{3}, CityIndex {4}, ZipIndex {5}, RgnIndexOverview {6}, Indexdata {7}",
                                 Roaddatainfos,
                                 RoadLength,
                                 LabelInfo != null ? LabelInfo.Count : 0,
                                 SegmentedRoadOffsets != null ? SegmentedRoadOffsets.Count : 0,
                                 CityIndex4Node != null ? CityIndex4Node.Length : 0,
                                 ZipIndex4Node != null ? ZipIndex4Node.Length : 0,
                                 RgnIndexOverview != null ? RgnIndexOverview.Count : 0,
                                 Indexdata != null ? Indexdata.Length : 0
                                 );
         }

      }


      /// <summary>
      /// Liste der Straßendaten
      /// </summary>
      internal List<RoadData> Roaddata { get; private set; }

      /// <summary>
      /// liefert den Index in <see cref="Roaddata"/> zum Offset aus <see cref="SortedOffsets"/>
      /// </summary>
      internal SortedDictionary<uint, int> Idx4Offset { get; private set; }

      /// <summary>
      /// Offsets der Daten der alphabetisch sortierten Straßennamen
      /// </summary>
      List<uint> SortedOffsets;

      /// <summary>
      /// muss vor dem Lesen/Schreiben gesetzt sein, wenn interpretierte Daten verwendet werden sollen
      /// <para>Andernfalls werden nur die Rohdaten der Datenblöcke verwendet.</para>
      /// </summary>
      public StdFile_LBL? Lbl;

      BinaryReaderWriter? livereader;


      public StdFile_NET()
         : base("NET") {
         Unknown_0x35 = 0x01;
         RoadDefinitionsOffsetMultiplier = 0;
         SegmentedRoadsOffsetMultiplier = 0;

         Roaddata = new List<RoadData>();
         Idx4Offset = new SortedDictionary<uint, int>();
         SortedOffsets = new List<uint>();

         UnknownBlock_0x43 = new DataBlock();
         UnknownBlock_0x4C = new DataBlock();
         UnknownBlock_0x56 = new DataBlock();
      }

      public override void ReadHeader(BinaryReaderWriter reader) {
         livereader = reader;

         readCommonHeader(reader, Type);

         RoadDefinitionsBlock = new DataBlock(reader);
         RoadDefinitionsOffsetMultiplier = reader.ReadByte();
         SegmentedRoadsBlock = new DataBlock(reader);
         SegmentedRoadsOffsetMultiplier = reader.ReadByte();
         SortedRoadsBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_0x31);
         Unknown_0x35 = reader.ReadByte();
         Unknown_0x36 = reader.ReadByte();

         // --------- Headerlänge > 55 Byte

         if (Headerlength >= 0x37) {
            reader.ReadBytes(Unknown_0x37);

            if (Headerlength >= 0x3B) {
               reader.ReadBytes(Unknown_0x3B);

               if (Headerlength >= 0x43) {
                  UnknownBlock_0x43 = new DataBlock(reader);

                  if (Headerlength >= 0x4B) {
                     Unknown_0x4B = reader.ReadByte();

                     if (Headerlength >= 0x4C) {
                        UnknownBlock_0x4C = new DataBlock(reader);

                        if (Headerlength >= 0x54) {
                           reader.ReadBytes(Unknown_0x54);

                           if (Headerlength >= 0x56) {
                              UnknownBlock_0x56 = new DataBlock(reader);

                              if (Headerlength >= 0x5E) {
                                 reader.ReadBytes(Unknown_0x5E);
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
      }

      public override void ReadMinimalSections(BinaryReaderWriter br) {
         Roaddata.Clear();
         Idx4Offset.Clear();
         SortedOffsets.Clear();

         if (Locked != 0 || Lbl == null || Lbl.RawRead) {
            RawRead = true;
            return;
         }

         // Datenblöcke "interpretieren"
         //Decode_RoadDefinitionsBlock(br, RoadDefinitionsBlock, Lbl);

         //Decode_SegmentedRoadsBlock(br, SegmentedRoadsBlock);
         //Decode_SortedRoadsBlock(br, SortedRoadsBlock);
      }

      #region Decodierung der Datenblöcke

      void Decode_RoadDefinitionsBlock(BinaryReaderWriter br, DataBlock src, StdFile_LBL lbl) {
         if (br != null &&
             src != null &&
             src.Length > 0) {
            br.Position = src.Offset;
            uint end = src.Offset + src.Length;
            int idx = 0;
            while (br.Position < end) {
               Idx4Offset.Add((uint)br.Position - src.Offset, idx++);
               RoadData rd = new RoadData();
               rd.Read(br, lbl);
               Roaddata.Add(rd);
            }
         }
      }

      internal RoadData Decode_RoadDefinition(uint offset, StdFile_LBL lbl) {
         RoadData rd = new RoadData();
         if (livereader != null && RoadDefinitionsBlock != null) {
            livereader.Seek(RoadDefinitionsBlock.Offset + offset);
            rd.Read(livereader, lbl);
         }
         return rd;
      }

      //void Decode_SegmentedRoadsBlock(BinaryReaderWriter br, DataBlock src) {
      //   if (br != null &&
      //       src != null &&
      //       src.Length > 0) {

      //      //throw new Exception("Decode_SegmentedRoadsBlock() ist noch nicht implementiert.");

      //   }
      //}

      void Decode_SortedRoadsBlock(BinaryReaderWriter br, DataBlockWithRecordsize src) {
         if (br != null &&
             src != null &&
             src.Length > 0) {

            // eigentlich nur Bit 0..21, d.h. & 0x3FFFFF nötig
            // Bit 22-23: label_number (0-3)
            SortedOffsets = br.ReadUintArray(src);
            // roaddata[idx4offset[sortedoffsets[...]]]


         }
      }

      #endregion

   }
}
