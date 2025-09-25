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

#pragma warning disable IDE1006

namespace GarminCore.Files {

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
      public byte RoadDefinitionsOffsetMultiplier;

      /// <summary>
      /// Segmented roads (0x1E)
      /// </summary>
      public DataBlock? SegmentedRoadsBlock { get; private set; }

      /// <summary>
      /// Segmented roads offset multiplier (power of 2) (0x26)
      /// </summary>
      public byte SegmentedRoadsOffsetMultiplier;

      /// <summary>
      /// Sorted roads (0x27)
      /// </summary>
      public DataBlockWithRecordsize? SortedRoadsBlock { get; private set; }

      public byte[] Unknown_0x31 = new byte[4];
      public byte Unknown_0x35;
      public byte Unknown_0x36;
      public byte[] Unknown_0x37 = new byte[4];
      public byte[] Unknown_0x3B = new byte[8];
      public DataBlock? UnknownBlock_0x43 { get; private set; }
      public byte Unknown_0x4B;
      public DataBlock? UnknownBlock_0x4C { get; private set; }
      public byte[] Unknown_0x54 = new byte[2];
      public DataBlock? UnknownBlock_0x56 { get; private set; }
      public byte[] Unknown_0x5E = new byte[6];

      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,
         RoadDefinitionsBlock,
         SegmentedRoadsBlock,
         SortedRoadsBlock,
         UnknownBlock_0x43,
         UnknownBlock_0x4C,
         UnknownBlock_0x56,
      }


      /// <summary>
      /// Datene einer Straße
      /// </summary>
      public class RoadData : BinaryReaderWriter.DataStruct {

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
            Indexdata = new List<IndexData>();
            NODFlag = NodInfo.unknown;
            NOD_Offset = 0;
         }

         /// <summary>
         /// Verweis in die Straßenliste einer Subdiv
         /// </summary>
         public class IndexData {
            public byte RoadIndex;
            public UInt16 Subdivisionnumber;

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

         public class Housenumbers {

            public class Housenumbers4Node {

               public int Idx;
               public NumberStyle LeftStyle;
               public int LeftFrom;
               public int LeftTo;
               public NumberStyle RightStyle;
               public int RightFrom;
               public int RightTo;

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

            public class RawData4Node {
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
         public List<uint> LabelInfo;

         public List<uint> SegmentedRoadOffsets;

         /// <summary>
         /// Art der vorhandenen Straßendaten
         /// </summary>
         public RoadDataInfo Roaddatainfos;

         /// <summary>
         /// Ist die spez. Info vorhanden?
         /// </summary>
         /// <param name="info"></param>
         /// <returns></returns>
         public bool HasRoadDataInfo(RoadDataInfo info) {
            return (Roaddatainfos & info) != 0;
         }

         /// <summary>
         /// (halbe) Länge der Straße in Meter
         /// </summary>
         public uint RoadLength;

         /// <summary>
         /// Anzahl der <see cref="Indexdata"/>-Items je Level
         /// </summary>
         public List<byte> RgnIndexOverview;

         /// <summary>
         /// Liste aller Verweise in Straßenlisten von Subdivs
         /// </summary>
         public List<IndexData> Indexdata;

         /// <summary>
         /// Anzahl Nodes und Flags für Zips, Citys usw. (<see cref="NodeCount"/>, <see cref="ZipFlag"/>, <see cref="CityFlag"/>)
         /// </summary>
         public UInt16 NodeCountAndFlags;

         /// <summary>
         /// Index-Liste für die Zip-Tabelle links rechts mit dem Node-Index
         /// </summary>
         public List<Dictionary<int, int>>? ZipIndex4Node;

         /// <summary>
         /// Index-Liste für die City-Tabelle links rechts mit dem Node-Index
         /// </summary>
         public List<Dictionary<int, int>>? CityIndex4Node;

         /// <summary>
         /// Stream, der die Hausnummernbereiche enthält
         /// </summary>
         public byte[]? NumberStream;

         public NodInfo NODFlag;

         public uint NOD_Offset;

         /// <summary>
         /// Anzahl der Nodes (Bit 0..9)
         /// </summary>
         public int NodeCount {
            get {
               return NodeCountAndFlags & 0x3FF;
            }
         }

         /// <summary>
         /// Zip-Bits (10 u. 11 für Zip); 0 und 1 stehen für 1 bzw. 2 Byte je Index, 3 für "kein Index"
         /// </summary>
         public int ZipFlag {
            get {
               return (NodeCountAndFlags >> 10) & 0x03;
            }
         }

         /// <summary>
         /// City-Bits (12 u. 13 für City); 0 und 1 stehen für 1 bzw. 2 Byte je Index, 3 für "kein Index"
         /// </summary>
         public int CityFlag {
            get {
               return (NodeCountAndFlags >> 12) & 0x03;
            }
         }

         /// <summary>
         /// Hausnummern-Bits (14 u. 15 für Hausnummern);  0, 1 und 2 stehen für 1, 2 bzw. 3 Byte je Nummer, 3 für "keine Nummer"
         /// </summary>
         public int NumberFlag {
            get {
               return (NodeCountAndFlags >> 14) & 0x03;
            }
         }

         /// <summary>
         /// Anzahl der Bytes, die eingelesen wurden
         /// </summary>
         public int RawBytes;


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

            for (int i = 0; i < RgnIndexOverview.Count; i++)
               for (int j = 0; j < RgnIndexOverview[i]; j++)
                  Indexdata.Add(new IndexData(br.ReadByte(), (ushort)br.Read2UInt())); // road_index (in subdivision) und subdivision_number

            if ((Roaddatainfos & RoadDataInfo.has_street_address_info) == RoadDataInfo.has_street_address_info) {
               NodeCountAndFlags = (ushort)br.Read2UInt();

               if (lbl.ZipDataList != null)
                  ZipIndex4Node = GetIndex4CityOrZip(br, ZipFlag, lbl.ZipDataList.Count < 256 ? 1 : 2);
               if (lbl.CityAndRegionOrCountryDataList != null)
                  CityIndex4Node = GetIndex4CityOrZip(br, CityFlag, lbl.CityAndRegionOrCountryDataList.Count < 256 ? 1 : 2);
               NumberStream = GetNumberStream(br, NumberFlag);
            } else {
               ZipIndex4Node = new List<Dictionary<int, int>>() { new Dictionary<int, int>(), new Dictionary<int, int>(), };
               CityIndex4Node = new List<Dictionary<int, int>>() { new Dictionary<int, int>(), new Dictionary<int, int>(), };
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

         public override void Write(BinaryReaderWriter bw, object? data) {

            throw new Exception("RoadData.Write() is not implemented");

            /*
                        StdFile_LBL lbl = data as StdFile_LBL;
                        uint tmpu = 0;
                        for (int i = 0; i < LabelInfo.Count; i++) {
                           tmpu = LabelInfo[i];
                           if (i == LabelInfo.Count - 1) {
                              if (SegmentedRoadOffsets.Count > 0)
                                 Bit.Set(tmpu, 14);
                              else
                                 Bit.Set(tmpu, 15);
                           }
                        }
                        for (int i = 0; i < SegmentedRoadOffsets.Count; i++) {
                           tmpu = SegmentedRoadOffsets[i];
                           if (i == SegmentedRoadOffsets.Count - 1)
                              Bit.Set(tmpu, 15);
                        }
                        bw.Write3(tmpu);

                        bw.Write((byte)Roaddata);

                        bw.Write3(RoadLength);

                        for (int i = 0; i < RgnIndexOverview.Count; i++)
                           if (i < RgnIndexOverview.Count - 1)
                              bw.Write(RgnIndexOverview[i]);
                           else
                              bw.Write((byte)(RgnIndexOverview[i] | 0x80));

                        for (int i = 0; i < Indexdata.Count; i++) {
                           bw.Write(Indexdata[i].RoadIndex);
                           bw.Write(Indexdata[i].Subdivisionnumber);
                        }

                        if ((Roaddata & RoadData2.has_street_address_info) != RoadData2.has_street_address_info) {
                           bw.Write(CountFlags);

                           if (ZipIndex.Count > 0)
                              if (lbl.ZipDataList.Count < 256)
                                 bw.Write((byte)ZipIndex);
                              else
                                 bw.Write((UInt16)ZipIndex);

                           if (CityIndex.Count > 0)
                              if (lbl.CityAndRegionOrCountryDataList.Count < 256)
                                 bw.Write((byte)CityIndex);
                              else
                                 bw.Write((UInt16)CityIndex);

                           if (NumberStream != null && 
                               NumberStream.Length > 0) {
                              if (NumberStream.Length < 256)
                                 bw.Write((byte)NumberStream.Length);
                              else
                                 bw.Write((UInt16)NumberStream.Length);
                              bw.Write(NumberStream);
                           }
                        }

                        if ((Roaddata & RoadData2.has_nod_info) != RoadData2.unknown) {
                           bw.Write((byte)NODFlag);
                           if ((NODFlag & NodInfo.two_byte_pointer) != NodInfo.unknown)
                              bw.Write((UInt16)NOD_Offset);
                           else
                              bw.Write((byte)NOD_Offset);
                        }
            */
         }

         /// <summary>
         /// liefert den Node/Index-Liste für die Zip- oder City-Tabelle
         /// </summary>
         /// <param name="br"></param>
         /// <param name="flag">0, 1, 2 oder 3</param>
         /// <param name="idxsize">Anzahl der Bytes für den Index (oder die Speicherlänge bei flag=2)</param>
         /// <returns>Liste der eingelesenen Indexe links und rechts</returns>
         List<Dictionary<int, int>>? GetIndex4CityOrZip(BinaryReaderWriter br, int flag, int idxsize) {
            int n;
            List<Dictionary<int, int>>? indexes = null;
            switch (flag) {
               case 0x0:
                  indexes = new List<Dictionary<int, int>>() {
                     new Dictionary<int, int>(),
                     new Dictionary<int, int>(),
                  };
                  parseList(br, br.Read1UInt(), idxsize, indexes);    // 1 Byte für Datenbereichslänge für die Liste
                  break;

               case 0x1:
                  indexes = new List<Dictionary<int, int>>() {
                     new Dictionary<int, int>(),
                     new Dictionary<int, int>(),
                  };
                  parseList(br, br.Read2UInt(), idxsize, indexes);    // 2 Byte für Datenbereichslänge für die Liste
                  break;

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
                     indexes = new List<Dictionary<int, int>>() {
                        new Dictionary<int, int>(),
                        new Dictionary<int, int>(),
                     };
                     indexes[0].Add(-1, n - 1);
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
         /// <param name="indexes">Liste der Indexe links und rechts und node</param>
         void parseList(BinaryReaderWriter br, int datalen, int size, List<Dictionary<int, int>> indexes) {
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
                  indexes[0].Add(node, left - 1);
               }
               if (right > 0 && left != right)
                  indexes[1].Add(node, right - 1);
            }
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
         /// liefert einen einzelnen Labeltext oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="idxlabel"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetText(StdFile_LBL lbl, int idxlabel, bool withctrl) {
            if (idxlabel < LabelInfo.Count)
               return lbl.GetText(LabelInfo[idxlabel], withctrl);
            return null;
         }

         /// <summary>
         /// liefert den Text aller Label verbunden mit "; " oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetText(StdFile_LBL lbl, bool withctrl) {
            string txt = "";
            for (int i = 0; i < LabelInfo.Count; i++) {
               if (i > 0)
                  txt += "; ";
               txt += GetText(lbl, i, withctrl);
            }
            return LabelInfo.Count > 0 ? txt : null;
         }


         /// <summary>
         /// liefert einen einzelnen Zip-Text oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="side"></param>
         /// <param name="idx"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetZipText(StdFile_LBL lbl, Side side, int idx, bool withctrl) {
            if (ZipIndex4Node != null) {
               Dictionary<int, int> dict = ZipIndex4Node[side == Side.Left ? 0 : 1];
               if (idx < dict.Count) {
                  int[] tabidx = new int[dict.Count];
                  dict.Values.CopyTo(tabidx, 0);
                  return lbl.GetText_FromZipList(tabidx[idx], withctrl);
               }
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
               Dictionary<int, int> dict = ZipIndex4Node[side == Side.Left ? 0 : 1];
               int[] tabidx = new int[dict.Count];
               dict.Values.CopyTo(tabidx, 0);
               string txt = "";
               for (int i = 0; i < tabidx.Length; i++) {
                  if (i > 0)
                     txt += "; ";
                  txt += lbl.GetText_FromZipList(tabidx[i], withctrl);
               }
               return dict.Count > 0 ? txt : null;
            }
            return null;
         }

         /// <summary>
         /// liefert einen einzelnen City-Text oder null
         /// </summary>
         /// <param name="lbl"></param>
         /// <param name="rgn"></param>
         /// <param name="side"></param>
         /// <param name="idx"></param>
         /// <param name="withctrl">wenn false, dann alle Steuerzeichen als '.'</param>
         /// <returns></returns>
         public string? GetCityText(StdFile_LBL lbl, StdFile_RGN rgn, Side side, int idx, bool withctrl) {
            if (CityIndex4Node != null) {
               Dictionary<int, int> dict = CityIndex4Node[side == Side.Left ? 0 : 1];
               if (idx < dict.Count) {
                  int[] tabidx = new int[dict.Count];
                  dict.Values.CopyTo(tabidx, 0);
                  return lbl.GetCityText_FromCityAndRegionOrCountryDataList(rgn, tabidx[idx], withctrl);
               }
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
               Dictionary<int, int> dict = CityIndex4Node[side == Side.Left ? 0 : 1];
               int[] tabidx = new int[dict.Count];
               dict.Values.CopyTo(tabidx, 0);
               string txt = "";
               for (int i = 0; i < tabidx.Length; i++) {
                  if (i > 0)
                     txt += "; ";
                  txt += lbl.GetCityText_FromCityAndRegionOrCountryDataList(rgn, tabidx[i], withctrl);
               }
               return dict.Count > 0 ? txt : null;
            }
            return null;
         }


         public override string ToString() {
            return string.Format("Roaddata {0}, RoadLength {1}, LabelInfo {2}, SegmentedRoadOffsets{3}, CityIndex {4}, ZipIndex {5}, RgnIndexOverview {6}, Indexdata {7}",
                                 Roaddatainfos,
                                 RoadLength,
                                 LabelInfo != null ? LabelInfo.Count : 0,
                                 SegmentedRoadOffsets != null ? SegmentedRoadOffsets.Count : 0,
                                 CityIndex4Node != null ? CityIndex4Node.Count : 0,
                                 ZipIndex4Node != null ? ZipIndex4Node.Count : 0,
                                 RgnIndexOverview != null ? RgnIndexOverview.Count : 0,
                                 Indexdata != null ? Indexdata.Count : 0
                                 );
         }

      }


      /// <summary>
      /// Liste der Straßendaten
      /// </summary>
      public List<RoadData> Roaddata;

      /// <summary>
      /// liefert den Index in <see cref="Roaddata"/> zum Offset aus <see cref="SortedOffsets"/>
      /// </summary>
      public SortedDictionary<uint, int> Idx4Offset;

      /// <summary>
      /// Offsets der Daten der alphabetisch sortierten Straßennamen
      /// </summary>
      public List<uint> SortedOffsets;

      /// <summary>
      /// muss vor dem Lesen/Schreiben gesetzt sein, wenn interpretierte Daten verwendet werden sollen
      /// <para>Andernfalls werden nur die Rohdaten der Datenblöcke verwendet.</para>
      /// </summary>
      public StdFile_LBL? Lbl;

      /// <summary>
      /// liefert den PostHeader-Datenbereich
      /// </summary>
      /// <returns></returns>
      public DataBlock? PostHeaderDataBlock { get; private set; }


      /// <summary>
      /// liefert die <see cref="RoadData"/> mit dem Index aus der Tabelle oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public RoadData? GetRoadData(int idx) {
         return idx < Roaddata.Count ?
            Roaddata[idx] :
            null;
      }

      /// <summary>
      /// liefert die <see cref="RoadData"/> mit dem Offset aus der Tabelle oder null
      /// </summary>
      /// <param name="offset"></param>
      /// <returns></returns>
      public RoadData? GetRoadData(uint offset) {
         if (Idx4Offset.TryGetValue(offset, out int idx))
            return GetRoadData(idx);
         return null;
      }



      public StdFile_NET() : base("NET") {
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

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         RoadDefinitionsBlock = new DataBlock(br);
         RoadDefinitionsOffsetMultiplier = br.ReadByte();
         SegmentedRoadsBlock = new DataBlock(br);
         SegmentedRoadsOffsetMultiplier = br.ReadByte();
         SortedRoadsBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_0x31);
         Unknown_0x35 = br.ReadByte();
         Unknown_0x36 = br.ReadByte();

         // --------- Headerlänge > 55 Byte

         if (Headerlength >= 0x37) {
            br.ReadBytes(Unknown_0x37);

            if (Headerlength >= 0x3B) {
               br.ReadBytes(Unknown_0x3B);

               if (Headerlength >= 0x43) {
                  UnknownBlock_0x43 = new DataBlock(br);

                  if (Headerlength >= 0x4B) {
                     Unknown_0x4B = br.ReadByte();

                     if (Headerlength >= 0x4C) {
                        UnknownBlock_0x4C = new DataBlock(br);

                        if (Headerlength >= 0x54) {
                           br.ReadBytes(Unknown_0x54);

                           if (Headerlength >= 0x56) {
                              UnknownBlock_0x56 = new DataBlock(br);

                              if (Headerlength >= 0x5E) {
                                 br.ReadBytes(Unknown_0x5E);
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
         Filesections.AddSection((int)InternalFileSections.RoadDefinitionsBlock, new DataBlock(RoadDefinitionsBlock));
         Filesections.AddSection((int)InternalFileSections.SegmentedRoadsBlock, new DataBlock(SegmentedRoadsBlock));
         Filesections.AddSection((int)InternalFileSections.SortedRoadsBlock, new DataBlockWithRecordsize(SortedRoadsBlock));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x43, new DataBlock(UnknownBlock_0x43));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x4C, new DataBlock(UnknownBlock_0x4C));
         Filesections.AddSection((int)InternalFileSections.UnknownBlock_0x56, new DataBlock(UnknownBlock_0x56));

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
         Roaddata.Clear();
         Idx4Offset.Clear();
         SortedOffsets.Clear();

         if (Locked != 0 || Lbl == null || Lbl.RawRead) {
            RawRead = true;
            return;
         }

         // Datenblöcke "interpretieren"
         int filesectiontype;

         filesectiontype = (int)InternalFileSections.RoadDefinitionsBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_RoadDefinitionsBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)), Lbl);
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.SegmentedRoadsBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            //Decode_SegmentedRoadsBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            //Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.SortedRoadsBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_SortedRoadsBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }
      }

      public override void Encode_Sections() {
         SetData2Filesection((int)InternalFileSections.RoadDefinitionsBlock, true);
         SetData2Filesection((int)InternalFileSections.SegmentedRoadsBlock, true);
         SetData2Filesection((int)InternalFileSections.SortedRoadsBlock, true);
      }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         switch ((InternalFileSections)filesectiontype) {
            case InternalFileSections.RoadDefinitionsBlock:
               if (Lbl == null)
                  return;
               Encode_RoadDefinitionsBlock(bw);
               break;

            case InternalFileSections.SegmentedRoadsBlock:
               Encode_SegmentedRoadsBlock(bw);
               break;

            case InternalFileSections.SortedRoadsBlock:
               Encode_SortedRoadsBlock(bw, Filesections.GetPosition(filesectiontype));
               break;

         }
      }

      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.PostHeaderData, pos++);
         Filesections.SetOffset((int)InternalFileSections.RoadDefinitionsBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.SegmentedRoadsBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.SortedRoadsBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.UnknownBlock_0x43, pos++);
         Filesections.SetOffset((int)InternalFileSections.UnknownBlock_0x4C, pos++);
         Filesections.SetOffset((int)InternalFileSections.UnknownBlock_0x56, pos++);

         Filesections.AdjustSections(DataOffset);     // lückenlos ausrichten

         RoadDefinitionsBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.RoadDefinitionsBlock));
         SegmentedRoadsBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.SegmentedRoadsBlock));
         SortedRoadsBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.SortedRoadsBlock));
         UnknownBlock_0x43 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.UnknownBlock_0x43));
         UnknownBlock_0x4C = new DataBlock(Filesections.GetPosition((int)InternalFileSections.UnknownBlock_0x4C));
         UnknownBlock_0x56 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.UnknownBlock_0x56));
      }

      #region Encodierung der Datenblöcke

      void Encode_RoadDefinitionsBlock(BinaryReaderWriter bw) {
         if (bw != null) {
            for (int i = 0; i < Roaddata.Count; i++) {
               Roaddata[i].Write(bw, Lbl);
            }
         }
      }

      void Encode_SegmentedRoadsBlock(BinaryReaderWriter bw) {


         throw new Exception("Encode_SegmentedRoadsBlock() ist noch nicht implementiert.");


      }

      void Encode_SortedRoadsBlock(BinaryReaderWriter bw, DataBlockWithRecordsize? src) {
         if (bw != null && src != null) {
            foreach (uint offs in SortedOffsets)
               switch (src.Recordsize) {
                  case 2: bw.Write((UInt16)offs); break;
                  case 3: bw.Write3(offs); break;
                  case 4: bw.Write(offs); break;
               }
         }
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            RoadDefinitionsBlock?.Write(bw);
            bw.Write(RoadDefinitionsOffsetMultiplier);
            SegmentedRoadsBlock?.Write(bw);
            bw.Write(SegmentedRoadsOffsetMultiplier);
            RoadDefinitionsBlock?.Write(bw);
            bw.Write(Unknown_0x31);
            bw.Write(Unknown_0x35);
            bw.Write(Unknown_0x36);

            if (Headerlength >= 0x37) {
               bw.Write(Unknown_0x37);

               if (Headerlength >= 0x3B)
                  bw.Write(Unknown_0x3B);

               if (Headerlength >= 0x43) {
                  UnknownBlock_0x43?.Write(bw);

                  if (Headerlength >= 0x4B) {
                     bw.Write(Unknown_0x4B);

                     if (Headerlength >= 0x4C) {
                        UnknownBlock_0x4C?.Write(bw);

                        if (Headerlength >= 0x54) {
                           bw.Write(Unknown_0x54);

                           if (Headerlength >= 0x56) {
                              UnknownBlock_0x56?.Write(bw);

                              if (Headerlength >= 0x5E) {
                                 bw.Write(Unknown_0x5E);
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

      #region Decodierung der Datenblöcke

      void Decode_RoadDefinitionsBlock(BinaryReaderWriter? br, DataBlock src, StdFile_LBL lbl) {
         uint end = src.Offset + src.Length;
         int idx = 0;
         if (br != null)
            while (br.Position < end) {
               Idx4Offset.Add((uint)br.Position - src.Offset, idx++);
               RoadData rd = new RoadData();
               rd.Read(br, lbl);
               Roaddata.Add(rd);
            }
      }

      void Decode_SegmentedRoadsBlock(BinaryReaderWriter? br, DataBlock src) {


         throw new Exception("Decode_SegmentedRoadsBlock() ist noch nicht implementiert.");


      }

      void Decode_SortedRoadsBlock(BinaryReaderWriter? br, DataBlockWithRecordsize src) {
         if (br != null)
            // eigentlich nur Bit 0..21, d.h. & 0x3FFFFF nötig
            // Bit 22-23: label_number (0-3)
            SortedOffsets = br.ReadUintArray(src);
         // roaddata[idx4offset[sortedoffsets[...]]]
      }

      #endregion

   }
}
