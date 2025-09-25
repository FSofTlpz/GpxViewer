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

namespace GarminCore.Files {

   /// <summary>
   /// zum Lesen und Schreiben der TRE-Datei (enthält u.a. die Infos über die Maplevel und die Subdivs der RGN-Datei, sowie die verwendeten Objekttypen)
   /// </summary>
   public class StdFile_TRE : StdFile {

      #region Datenobjektdefinitionen

      /// <summary>
      /// ein Item in der <see cref="MapLevel"/>-Tabelle
      /// <para>Das 1. Item steht für den kleinsten Zoom bzw. kleinsten Maßstab, das letzte für den größten Zoom/Maßstab. Der Maßstab ist der Faktor 1 : x. 
      /// Der Nenner x wird im Item gespeichert. Allerdings ist das nur ein symbolischer Wert [0..15] (Bit 0 .. 3) der vom GPS-Gerät je nach Software
      /// interpretiert wird.</para>
      /// </summary>
      public class MapLevel : BinaryReaderWriter.DataStruct {
         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public const uint DataLength = 4;

         byte _SymbolicScaleDenominator;

         /// <summary>
         /// Nenner x des symbolischen Maßstabs 1 : x (0..15, da nur Bits 0 bis 3 verwendbar)
         /// </summary>
         public byte SymbolicScaleDenominator {
            get {
               return (byte)(_SymbolicScaleDenominator & 0x0F);    // Bits 0..3
            }
            set {
               if ((value & 0xF0) > 0)
                  throw new ArgumentException("Der SymbolicScale darf max. 15 sein.");
               _SymbolicScaleDenominator = (byte)((_SymbolicScaleDenominator & 0xF0) | (value & 0x0F));
            }
         }

         /// <summary>
         /// Bit 4 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Bit4 {
            get {
               return Bit.IsSet(_SymbolicScaleDenominator, 4);
            }
            set {
               _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 4, value);
            }
         }

         /// <summary>
         /// Bit 5 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Bit5 {
            get {
               return Bit.IsSet(_SymbolicScaleDenominator, 5);
            }
            set {
               _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 5, value);
            }
         }

         /// <summary>
         /// Bit 6 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Bit6 {
            get {
               return Bit.IsSet(_SymbolicScaleDenominator, 6);
            }
            set {
               _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 6, value);
            }
         }

         /// <summary>
         /// Bit 7 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Inherited {
            get {
               return Bit.IsSet(_SymbolicScaleDenominator, 7);
            }
            set {
               _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 7, value);
            }
         }

         /// <summary>
         /// Bitanzahl je Zahlenwert für Koordinaten
         /// <para>Genauigkeit der Koordinaten; je höher der <see cref="SymbolicScaleDenominator"/>, je geringer ist die Bitanzahl</para>
         /// <para>i.A. im Bereich 10 .. 24; höhere Werte vermutlich nicht möglich</para>
         /// </summary>
         public byte CoordBits;

         /// <summary>
         /// Anzahl der zum Level zugehörigen Subdivinfos
         /// </summary>
         public UInt16 SubdivInfos;

         /// <summary>
         /// 1-basierter Index der 1. Subdivinfo in der <see cref="SubdivInfoList"/>-Liste (diese Angabe stammt NICHT aus der <see cref="MapLevel"/>-Liste)
         /// </summary>
         public UInt16 FirstSubdivInfoNumber;


         public MapLevel() {
            _SymbolicScaleDenominator = 0;
            CoordBits = 0;
            SubdivInfos = 0;
            FirstSubdivInfoNumber = 0;
         }

         public MapLevel(MapLevel ml) {
            _SymbolicScaleDenominator = ml._SymbolicScaleDenominator;
            CoordBits = ml.CoordBits;
            SubdivInfos = ml.SubdivInfos;
            FirstSubdivInfoNumber = ml.FirstSubdivInfoNumber;
         }

         public override void Read(BinaryReaderWriter br, object? extdata = null) {
            _SymbolicScaleDenominator = br.ReadByte();
            CoordBits = br.ReadByte();
            SubdivInfos = br.Read2AsUShort();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) {
            bw.Write(_SymbolicScaleDenominator);
            bw.Write(CoordBits);
            bw.Write(SubdivInfos);
         }

         public override string ToString() {
            return string.Format("SymbolicScale {0}, Inherited {1}, CoordBits {2}, FirstSubdivInfoNumber {3}, Subdivs {4}",
               SymbolicScaleDenominator,
               Inherited,
               CoordBits,
               FirstSubdivInfoNumber,
               SubdivInfos);
         }
      }

      /* Beispiel einer Maplevel-Liste:
       * 
         MaplevelList	Count = 0x00000009
         [0x00000000]	{SymbolicScale 8, Inherited true, coordbits 16, Subdivs 1}
         [0x00000001]	{SymbolicScale 7, Inherited false, coordbits 17, Subdivs 10}
         [0x00000002]	{SymbolicScale 6, Inherited false, coordbits 18, Subdivs 18}
         [0x00000003]	{SymbolicScale 5, Inherited false, coordbits 19, Subdivs 44}
         [0x00000004]	{SymbolicScale 4, Inherited false, coordbits 20, Subdivs 177}
         [0x00000005]	{SymbolicScale 3, Inherited false, coordbits 21, Subdivs 218}
         [0x00000006]	{SymbolicScale 2, Inherited false, coordbits 22, Subdivs 380}
         [0x00000007]	{SymbolicScale 1, Inherited false, coordbits 23, Subdivs 425}
         [0x00000008]	{SymbolicScale 0, Inherited false, coordbits 24, Subdivs 487}

       * Im Maplevel 0 (höchster SymbolicScaleDenominator) ist die gesamte Karte in nur wenige Teilbereiche aufgeteilt. Jeder Teilbereich wird im nächsten SymbolicScaleDenominator
       * (nächsthöherer Maplevel) in mehrerer kleinere Teilbereich aufgeteilt. Dadurch entsteht eine Hierarchie der Teilbereiche.
       * Die Teilbereiche sind durchnummeriert. Sie enthalten als Verweis auf den 1. untergeordneten Teilbereich dessen Nummer. Die Teilbereiche des niedrigsten Maplevel
       * enthalten diesen Verweis nicht mehr (unnötig). Jeder Teilbereich ist durch die Koordinaten des Mittelpunktes und die Breite und Höhe des Bereiches definiert.
       * Er enthält außerdem einen Offset auf die eigentlichen Daten und eine Kennung über die Art der Daten.
       * Da ein übergeordneter Teilbereich nur auf _einen_ untergeordneten Teilbereich verweisen kann, ist das immer der erste untergeordnete Teilbereich. Alle folgenden
       * Teilbereiche sind dann ebenfalls untergeordnet, bis ein Teilbereich mit dem Terminating-Flag erreicht wird (Bit 15 der Breite).
       * 
       * Die SymbolicScaleDenominator müssen nicht von 0 bis n durchnummeriert sein! Vermutlich hat das Einfluß darauf, welchen Maplevel die (PC- oder Device-)Software zur Darstellung für einen
       * bestimmten Zoom auswählt.
       */

      public class SymbolicScaleDenominatorAndBits {

         List<MapLevel> ml;


         public SymbolicScaleDenominatorAndBits() {
            ml = new List<StdFile_TRE.MapLevel>();
         }

         public SymbolicScaleDenominatorAndBits(IList<MapLevel> mllst) : this() {
            Clear();
            for (int i = 0; i < mllst.Count; i++) {
               MapLevel mli = new MapLevel(mllst[i]) {
                  // da im Original nicht gespeichert, wird sie hier sicherheitshalber berechnet
                  FirstSubdivInfoNumber = (ushort)(ml.Count == 0 ?
                                                1 :
                                                ml[ml.Count - 1].FirstSubdivInfoNumber + ml[ml.Count - 1].SubdivInfos)
               };
               ml.Add(mli);
            }
         }

         public SymbolicScaleDenominatorAndBits(SymbolicScaleDenominatorAndBits ssab) : this() {
            ml.AddRange(ssab.ml);
         }

         public void Clear() {
            ml.Clear();
         }

         /// <summary>
         /// fügt eine neue Ebene an (<see cref="scale"/> absteigend, <see cref="bits"/> aufsteigend)
         /// </summary>
         /// <param name="scale"></param>
         /// <param name="bits"></param>
         /// <param name="subdivs"></param>
         public void AddLevel(int scale, int bits, int subdivs = 0) {
            if (scale < 0 || scale > 15)
               throw new ArgumentException("Der symbolische Maßstab muss im Bereich 0 .. 15 sein.");
            if (bits < 2 || bits > 24)
               throw new ArgumentException("Die Bitanzahl muss im Bereich 2 .. 24 sein.");

            StdFile_TRE.MapLevel mli = new StdFile_TRE.MapLevel {
               SymbolicScaleDenominator = (byte)scale,
               CoordBits = (byte)bits,
               FirstSubdivInfoNumber = (ushort)(ml.Count == 0 ?
                                                         1 :
                                                         ml[ml.Count - 1].FirstSubdivInfoNumber + ml[ml.Count - 1].SubdivInfos),
               SubdivInfos = (ushort)subdivs
            };
            if (ml.Count == 0)
               mli.Inherited = true;
            else {
               if (scale >= ml[ml.Count - 1].SymbolicScaleDenominator)
                  throw new ArgumentException("Der neue symbolische Maßstab muss kleiner " + ml[ml.Count - 1].SymbolicScaleDenominator.ToString() + " sein.");
               if (bits <= ml[ml.Count - 1].CoordBits)
                  throw new ArgumentException("Die neue Bitanzahl muss größer " + ml[ml.Count - 1].CoordBits.ToString() + " sein.");
            }
            ml.Add(mli);
         }

         /// <summary>
         /// erzeugt die Maplevel-Liste aus den internen Daten
         /// </summary>
         /// <returns></returns>
         public List<MapLevel> GetMaplevelList() {
            List<StdFile_TRE.MapLevel> lst = new List<StdFile_TRE.MapLevel>();
            for (int i = 0; i < ml.Count; i++)
               lst.Add(new StdFile_TRE.MapLevel(ml[i]));
            return lst;
         }

         /// <summary>
         /// Anzahl der Ebenen
         /// </summary>
         public int Count {
            get {
               return ml.Count;
            }
         }

         /// <summary>
         /// liefert den Nenner x des symbolischen Maßstabs 1 : x (0..15) der Ebene
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public int SymbolicScaleDenominator(int level) {
            LevelCheck(level);
            return ml[level].SymbolicScaleDenominator;
         }

         /// <summary>
         /// Vererbungsflag (?) der Ebene
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public bool Inherited(int level) {
            LevelCheck(level);
            return ml[level].Inherited;
         }

         /// <summary>
         /// liefert die Bitanzahl je Koordinate der Ebene
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public int Bits(int level) {
            LevelCheck(level);
            return ml[level].CoordBits;
         }

         /// <summary>
         /// liefert den 1-basierten Index des 1. untergeordneten Subdivs der Ebene
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public int FirstSubdivChildIdx(int level) {
            LevelCheck(level);
            return ml[level].FirstSubdivInfoNumber;
         }

         /// <summary>
         /// liefert die Anzahl der zur Ebene gehörenden Subdivs
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public int Subdivs(int level) {
            LevelCheck(level);
            return ml[level].SubdivInfos;
         }

         void LevelCheck(int level) {
            if (level < 0 || level >= Count)
               throw new ArgumentException("Die Ebene liegt außerhalb des gültigen Bereichs 0 .. " + (Count - 1).ToString());
         }

         /// <summary>
         /// liefert die Ebene, zu der der 1-basierte Subdiv-Index gehört
         /// </summary>
         /// <param name="idx"></param>
         /// <returns></returns>
         public int Level4SubdivIdx1(int idx) {
            for (int i = Count - 1; i >= 0; i--)
               if (idx >= FirstSubdivChildIdx(i))
                  return i;
            return Count - 1;
         }

         /// <summary>
         /// liefert die Bitanzahl, zu der der 1-basierte Subdiv-Index gehört
         /// </summary>
         /// <param name="idx"></param>
         /// <returns></returns>
         public int Bits4SubdivIdx1(int idx) {
            return Bits(Level4SubdivIdx1(idx));
         }

         public override string ToString() {
            return string.Format("{0} Level", Count);
         }
      }


      /*
            public class Subdiv {

               public SubdivInfoBasic Info { get; }

               public StdFile_RGN.SubdivData Data { get; }


               /// <summary>
               /// übernimmt <see cref="sdi"/> und <see cref="sdc"/> nur als Referenz (!)
               /// </summary>
               /// <param name="sdi"></param>
               /// <param name="sdc"></param>
               public Subdiv(SubdivInfoBasic sdi = null, StdFile_RGN.SubdivData sdc = null) {
                  Info = sdi == null ? new SubdivInfo() : sdi;
                  Content = sdc == null ? new StdFile_RGN.SubdivData() : sdc;
               }

               /// <summary>
               /// liefert tru, wenn <see cref="Info"/> gesetzt ist und ein <see cref="SubdivInfoBasic"/> ist
               /// </summary>
               public bool IsSubdivInfoBasic {
                  get {
                     return Info != null && Info is SubdivInfoBasic;
                  }
               }

               /// <summary>
               /// liefert tru, wenn <see cref="Info"/> gesetzt ist und ein <see cref="SubdivInfo"/> ist
               /// </summary>
               public bool IsSubdivInfo {
                  get {
                     return Info != null && Info is SubdivInfo;
                  }
               }

               /// <summary>
               /// 1-basierter Index des Subdiv's des nächsten untergeordneten Level (mit mehr Details); im niedrigsten/letzten Level nicht mehr vorhanden!
               /// </summary>
               public UInt16 FirstChildSubdivIdx1 {
                  get {
                     return IsSubdivInfo ? (Info as SubdivInfo).FirstChildSubdivIdx1 : (ushort)0;
                  }
                  set {
                     CheckSetable(IsSubdivInfo ? Info : null);
                     (Info as SubdivInfo).FirstChildSubdivIdx1 = value;
                  }
               }

               /// <summary>
               /// Anzahl der zum Subdiv zugehörigen untergeordneten Subdivinfos
               /// <para>Damit ergibt sich der 1-basierte Index aller untergeordneten Subdivinfos als Folge <see cref="FirstChildSubdivIdx1"/> ... (<see cref="FirstChildSubdivIdx1"/>+<see cref="ChildSubdivInfos"/>-1).</para>
               /// </summary>
               public UInt16 ChildSubdivInfos {
                  get {
                     return IsSubdivInfo ? (Info as SubdivInfo).ChildSubdivInfos : (ushort)0;
                  }
                  set {
                     CheckSetable(IsSubdivInfo ? Info : null);
                     (Info as SubdivInfo).ChildSubdivInfos = value;
                  }
               }

               /// <summary>
               /// Datenbereich
               /// </summary>
               public DataBlock Datablock {
                  get {
                     return Info != null ? Info.Data : null;
                  }
                  set {
                     CheckSetable(Info);
                     Info.Data = value;
                  }
               }

               /// <summary>
               /// Sammlung aller Inhalte in dieser Subdiv
               /// </summary>
               public SubdivInfoBasic.SubdivContent Content {
                  get {
                     return Info != null ? Info.Content : SubdivInfoBasic.SubdivContent.nothing;
                  }
                  set {
                     CheckSetable(Info);
                     Info.Content = value;
                  }
               }

               /// <summary>
               /// geografische Länge des Mittelpunktes in Mapunits
               /// </summary>
               public int LongitudeCenter {
                  get {
                     return Info != null ? Info.LongitudeCenter : 0;
                  }
                  set {
                     CheckSetable(Info);
                     Info.LongitudeCenter = value;
                  }
               }

               /// <summary>
               /// geografische Breite des Mittelpunktes in Mapunits
               /// </summary>
               public int LatitudeCenter {
                  get {
                     return Info != null ? Info.LatitudeCenter : 0;
                  }
                  set {
                     CheckSetable(Info);
                     Info.LatitudeCenter = value;
                  }
               }

               /// <summary>
               /// geografische Länge des Mittelpunktes in Grad
               /// </summary>
               public double LongitudeCenterInDegree {
                  get {
                     return Info != null ? GarminCoordinate.MapUnits2Degree(Info.LongitudeCenter) : 0;
                  }
                  set {
                     CheckSetable(Info);
                     Info.LongitudeCenter = GarminCoordinate.Degree2MapUnits(value);
                  }
               }

               /// <summary>
               /// geografische Breite des Mittelpunktes in Grad
               /// </summary>
               public double LatitudeCenterInDegree {
                  get {
                     return Info != null ? GarminCoordinate.MapUnits2Degree(Info.LatitudeCenter) : 0;
                  }
                  set {
                     CheckSetable(Info);
                     Info.LatitudeCenter = GarminCoordinate.Degree2MapUnits(value);
                  }
               }

               /// <summary>
               /// halbe Breite (in Rawunits)
               /// </summary>
               public UInt16 HalfWidth {
                  get {
                     return Info != null ? Info.HalfWidth : (ushort)0;
                  }
                  set {
                     CheckSetable(Info);
                     Info.HalfWidth = value;
                  }
               }

               /// <summary>
               /// halbe Höhe (in Rawunits)
               /// </summary>
               public UInt16 HalfHeight {
                  get {
                     return Info != null ? Info.HalfHeight : (ushort)0;
                  }
                  set {
                     CheckSetable(Info);
                     Info.HalfHeight = value;
                  }
               }

               /// <summary>
               /// Letztes Subdiv?
               /// </summary>
               public bool LastSubdiv {
                  get {
                     return Info != null ? Info.LastSubdiv : false;
                  }
                  set {
                     CheckSetable(Info);
                     Info.LastSubdiv = value;
                  }
               }

               /// <summary>
               /// Liste der Punkte
               /// </summary>
               public List<StdFile_RGN.PointData> PointList {
                  get {
                     return Data != null ? Data.PointList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.PointList = value;
                  }
               }

               /// <summary>
               /// Liste der IndexPunkte (wahrscheinlich nur für "Stadt"-Typen, kleiner 0x12)
               /// </summary>
               public List<StdFile_RGN.PointData> IdxPointList {
                  get {
                     return Data != null ? Data.IdxPointList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.IdxPointList = value;
                  }
               }

               /// <summary>
               /// Liste der Polylines
               /// </summary>
               public List<StdFile_RGN.PolyData> LineList {
                  get {
                     return Data != null ? Data.LineList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.LineList = value;
                  }
               }

               /// <summary>
               /// Liste der Polygone
               /// </summary>
               public List<StdFile_RGN.PolyData> AreaList {
                  get {
                     return Data != null ? Data.AreaList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.AreaList = value;
                  }
               }

               // Die Listen für die erweiterten Daten werden nur extern (!) verwaltet, da diese Daten nicht im Subdiv-Bereich selbst gespeichert werden.

               /// <summary>
               /// Listen der erweiterten Linien
               /// </summary>
               public List<StdFile_RGN.ExtPolyData> ExtLineList {
                  get {
                     return Data != null ? Data.ExtLineList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.ExtLineList = value;
                  }
               }

               /// <summary>
               /// Listen der erweiterten Flächen
               /// </summary>
               public List<StdFile_RGN.ExtPolyData> ExtAreaList {
                  get {
                     return Data != null ? Data.ExtAreaList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.ExtAreaList = value;
                  }
               }

               /// <summary>
               /// Listen der erweiterten Punkte
               /// </summary>
               public List<StdFile_RGN.ExtPointData> ExtPointList {
                  get {
                     return Data != null ? Data.ExtPointList : null;
                  }
                  set {
                     CheckSetable(Data);
                     Data.ExtPointList = value;
                  }
               }

               void CheckSetable(object obj) {
                  if (obj == null)
                     throw new ArgumentException("Kann für dieses Element nicht gesetzt werden.");
               }

            }

            public class SubdivList {

               List<Subdiv> lst;


               public SubdivList(SubdivList sdlst) {
                  lst = new List<Subdiv>();
                  if (sdlst != null && sdlst.Count > 0)
                     lst.AddRange(sdlst.lst);
               }

               public SubdivList(IList<SubdivInfoBasic> sdi = null, IList<StdFile_RGN.SubdivData> sdc = null) {
                  lst = new List<Subdiv>();
                  if (sdi != null && sdc != null) {
                     if (sdi.Count != sdc.Count)
                        throw new ArgumentException("Die beiden Listen haben eine unterschiedliche Länge.");
                     for (int i = 0; i < sdi.Count; i++)
                        lst.Add(new Subdiv(sdi[i], sdc[i]));
                  } else if (sdi != null) {
                     for (int i = 0; i < sdi.Count; i++)
                        lst.Add(new Subdiv(sdi[i], null));
                  } else if (sdc != null) {
                     for (int i = 0; i < sdc.Count; i++)
                        lst.Add(new Subdiv(null, sdc[i]));
                  }
               }

               /// <summary>
               /// Anzahl der Subdivs
               /// </summary>
               public int Count {
                  get {
                     return lst.Count;
                  }
               }

               public void Add(SubdivInfoBasic sdi, StdFile_RGN.SubdivData sdc) {
                  lst.Add(new Subdiv(sdi, sdc));
               }





               public override string ToString() {
                  return string.Format("{0} Subdivs", Count);
               }
            }
      */

      /// <summary>
      /// Subdiv für die höchste Zoomstufe == niedrigster Maplevel
      /// </summary>
      public class SubdivInfoBasic : BinaryReaderWriter.DataStruct {

         public SubdivInfoBasic() {
            Data = new DataBlock();
            Content = SubdivContent.nothing;
            Center = new MapUnitPoint();
            HalfWidth = HalfHeight = 0;
            LastSubdiv = false;
         }

         public SubdivInfoBasic(SubdivInfoBasic? sdi = null) : this() {
            if (sdi != null) {
               Data.Offset = sdi.Data.Offset;
               Data.Length = sdi.Data.Length;
               Content = sdi.Content;
               Center = sdi.Center;
               HalfHeight = sdi.HalfHeight;
               HalfWidth = sdi.HalfWidth;
               LastSubdiv = sdi.LastSubdiv;
            }
         }

         [Flags]
         public enum SubdivContent : byte {
            nothing = 0x00,
            poi = 0x10,
            idxpoi = 0x20,
            line = 0x40,
            area = 0x80
         }

         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public const uint DataLength = 14;

         /// <summary>
         /// Datenbereich
         /// </summary>
         public DataBlock Data;

         /// <summary>
         /// Sammlung aller Inhalte in dieser Subdiv
         /// </summary>
         public SubdivContent Content;

         /// <summary>
         /// geografische Länge und Breite des Mittelpunktes
         /// </summary>
         public MapUnitPoint Center;


         /// <summary>
         /// halbe Breite (in Rawunits); Bit 0..14; Bit 15 <see cref="LastSubdiv"/>
         /// </summary>
         UInt16 _HalfWidth;

         /// <summary>
         /// halbe Breite (in Rawunits)
         /// </summary>
         public UInt16 HalfWidth {
            get {
               return (UInt16)(_HalfWidth & 0x7FFF);
            }
            set {
               if (value >= 0x7FFF)
                  throw new Exception("Die (halbe) Breite für die Subdivision ist zu groß.");
               _HalfWidth |= (UInt16)(value & 0x7FFF);
            }
         }

         /// <summary>
         /// halbe Höhe (in Rawunits)
         /// </summary>
         public UInt16 HalfHeight { get; set; }

         /// <summary>
         /// Letztes Subdiv?
         /// </summary>
         public bool LastSubdiv {
            get {
               return Bit.IsSet(_HalfWidth, 15);
            }
            set {
               _HalfWidth = (UInt16)Bit.Set(_HalfWidth, 15, value);
            }
         }

         public override void Read(BinaryReaderWriter br, object? extdata = null) {
            Data.Offset = br.Read3AsUInt();
            Content = (SubdivContent)br.ReadByte();
            Center.Longitude = br.Read3Int();
            Center.Latitude = br.Read3Int();
            _HalfWidth = br.Read2AsUShort();
            HalfHeight = br.Read2AsUShort();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) {
            bw.Write3(Data.Offset);
            bw.Write((byte)Content);
            bw.Write3(Center.Longitude);
            bw.Write3(Center.Latitude);
            bw.Write(_HalfWidth);
            bw.Write(HalfHeight);
         }

         /// <summary>
         /// liefert die Umgrenzung der Subdiv
         /// </summary>
         /// <param name="coordbits"></param>
         /// <returns></returns>
         public Bound GetBound(int coordbits) {
            double halfWidthDegree = Coord.RawUnits2Degree(HalfWidth, coordbits);
            double halfHeightDegree = Coord.RawUnits2Degree(HalfHeight, coordbits);
            return new Bound(Center.LongitudeDegree - halfWidthDegree,
                             Center.LongitudeDegree + halfWidthDegree,
                             Center.LatitudeDegree - halfHeightDegree,
                             Center.LatitudeDegree + halfHeightDegree);
         }

         public int GetHalfWidthMapUnits(int coordbits) {
            return Coord.RawUnits2MapUnits(HalfWidth, coordbits);
         }

         public void SetHalfWidthMapUnits(int mapunits, int coordbits) {
            HalfWidth = (ushort)Coord.MapUnits2RawUnits(mapunits, coordbits);
         }

         public double GetHalfWidthDegree(int coordbits) {
            return Coord.RawUnits2Degree(HalfWidth, coordbits);
         }

         public void SetHalfWidthDegree(double degree, int coordbits) {
            HalfWidth = (ushort)Coord.Degree2RawUnits(degree, coordbits);
         }

         public int GetHalfHeightMapUnits(int coordbits) {
            return Coord.RawUnits2MapUnits(HalfHeight, coordbits);
         }

         public void SetHalfHeightMapUnits(int mapunits, int coordbits) {
            HalfHeight = (ushort)Coord.MapUnits2RawUnits(mapunits, coordbits);
         }

         public double GetHalfHeightDegree(int coordbits) {
            return Coord.RawUnits2Degree(HalfHeight, coordbits);
         }

         public void SetHalfHeightDegree(double degree, int coordbits) {
            HalfHeight = (ushort)Coord.Degree2RawUnits(degree, coordbits);
         }

         public override string ToString() {
            return string.Format("Offset {0}, Content {1}, LastSubdiv {2}, Center {3}° {4}°",
                                 Data,
                                 Content,
                                 LastSubdiv,
                                 Center.LongitudeDegree,
                                 Center.LatitudeDegree);
         }
      }

      /// <summary>
      /// Subdiv das selbst Child-Subdivs hat (also NICHT die höchste Zoomstufe bzw. NICHT niedrigster Maplevel)
      /// </summary>
      public class SubdivInfo : SubdivInfoBasic {

         public SubdivInfo() : base() {
            FirstChildSubdivIdx1 = 0;
            ChildSubdivInfos = 0;
         }

         public SubdivInfo(SubdivInfo? sdi = null) : base(sdi) {
            FirstChildSubdivIdx1 = sdi == null ? (ushort)0 : sdi.FirstChildSubdivIdx1;
            ChildSubdivInfos = sdi == null ? (ushort)0 : sdi.ChildSubdivInfos;
         }


         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public new const uint DataLength = 16;

         /// <summary>
         /// 1-basierter Index des Subdiv's des nächsten untergeordneten Level (mit mehr Details); im niedrigsten/letzten Level nicht mehr vorhanden!
         /// </summary>
         public UInt16 FirstChildSubdivIdx1;

         /// <summary>
         /// Anzahl der zum Subdiv zugehörigen untergeordneten Subdivinfos
         /// <para>Damit ergibt sich der 1-basierte Index aller untergeordneten Subdivinfos als Folge <see cref="FirstChildSubdivIdx1"/> ... (<see cref="FirstChildSubdivIdx1"/>+<see cref="ChildSubdivInfos"/>-1).</para>
         /// </summary>
         public UInt16 ChildSubdivInfos;

         public override void Read(BinaryReaderWriter br, object? extdata) {
            base.Read(br, extdata);
            FirstChildSubdivIdx1 = br.Read2AsUShort();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata) {
            base.Write(bw, extdata);
            if (FirstChildSubdivIdx1 != 0)
               bw.Write(FirstChildSubdivIdx1);
         }

         public override string ToString() {
            return string.Format("Offset {0}, Content {1}, FirstChildSubdiv {2}, SubdivInfos {3}, LastSubdiv {4}, Center {5}° {6}°",
                                 Data,
                                 Content,
                                 FirstChildSubdivIdx1,
                                 ChildSubdivInfos,
                                 LastSubdiv,
                                 Center.LongitudeDegree,
                                 Center.LatitudeDegree);
         }
      }

      /// <summary>
      /// 2-Byte-Overview-Info
      /// </summary>
      public class OverviewObject2Byte : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public const uint DataLength = 2;

         /// <summary>
         /// Objektart
         /// </summary>
         public byte Type;

         /// <summary>
         /// größer Level für Anzeige
         /// </summary>
         public byte MaxLevel;


         public OverviewObject2Byte() : this(0, 0) { }

         public OverviewObject2Byte(byte type, byte maxlevel) {
            Type = type;
            MaxLevel = maxlevel;
         }

         public override void Read(BinaryReaderWriter br, object? extdata = null) {
            Type = br.ReadByte();
            MaxLevel = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) {
            bw.Write(Type);
            bw.Write(MaxLevel);
         }

         public override string ToString() {
            return string.Format("MaxLevel {0}, Type = 0x{1:x2}", MaxLevel, Type);
         }
      }

      /// <summary>
      /// 3-Byte-Overview-Info
      /// </summary>
      public class OverviewObject3Byte : OverviewObject2Byte {

         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public new const uint DataLength = 3;

         /// <summary>
         /// Subtyp Objektart
         /// </summary>
         public byte SubType;


         public OverviewObject3Byte() : this(0, 0, 0) { }

         public OverviewObject3Byte(byte type, byte subtype, byte maxlevel)
            : base(type, maxlevel) {
            SubType = subtype;
         }

         public override void Read(BinaryReaderWriter br, object? extdata = null) {
            base.Read(br, extdata);
            SubType = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) {
            base.Write(bw, extdata);
            bw.Write(SubType);
         }

         public override string ToString() {
            return base.ToString() + ", SubType 0x" + SubType.ToString("x2");
         }
      }

      /// <summary>
      /// 4-Byte-Overview-Info
      /// </summary>
      public class OverviewObject4Byte : OverviewObject3Byte {

         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public new const uint DataLength = 4;

         /// <summary>
         /// unbekannt, i.A. 0
         /// </summary>
         public byte Unknown;


         public OverviewObject4Byte() : this(0, 0, 0) { }

         public OverviewObject4Byte(byte type, byte subtype, byte maxlevel, byte unknown = 0)
            : base(type, subtype, maxlevel) {
            Unknown = unknown;
         }

         public override void Read(BinaryReaderWriter br, object? extdata = null) {
            base.Read(br, extdata);
            Unknown = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) {
            base.Write(bw, extdata);
            bw.Write(Unknown);
         }

         public override string ToString() {
            return base.ToString() + ", Unknown " + Unknown.ToString("x2");
         }
      }

      /// <summary>
      /// Offset für erweiterte Typen
      /// </summary>
      public class ExtendedTypeOffsets : BinaryReaderWriter.DataStruct {

         /// <summary>
         /// Größe des Speicherbereiches in der TRE-Datei
         /// </summary>
         public uint DataLength = 13;

         public UInt32 AreasOffset, LinesOffset, PointsOffset;

         /// <summary>
         /// Anzahl der enthaltenen Bereiche (mit Länge ungleich 0)
         /// </summary>
         public byte Kinds;


         public ExtendedTypeOffsets() {
            AreasOffset =
            LinesOffset =
            PointsOffset = 0;
            Kinds = 0;
         }

         public override void Read(BinaryReaderWriter br, object? extdata = null) {
            AreasOffset = br.Read4UInt();
            LinesOffset = br.Read4UInt();
            PointsOffset = br.Read4UInt();
            if (extdata != null)
               DataLength = (UInt16)extdata;
            if (DataLength > 12)
               Kinds = br.ReadByte();
         }

         public override void Write(BinaryReaderWriter bw, object? extdata = null) {
            bw.Write(AreasOffset);
            bw.Write(LinesOffset);
            bw.Write(PointsOffset);
            if (DataLength > 12)
               bw.Write(Kinds);
         }

         public override string ToString() {
            return string.Format("AreasOffset 0x{0:x}, LinesOffset = 0x{1:x}, PointsOffset 0x{2:x}, Kinds {3}",
                                 AreasOffset,
                                 LinesOffset,
                                 PointsOffset,
                                 Kinds);
         }

      }

      #endregion

      #region Header-Daten

      /// <summary>
      /// nördliche Kartengrenze in 360/(2^24) Grad (0x15)
      /// </summary>
      public Latitude North;
      /// <summary>
      /// östliche Kartengrenze in 360/(2^24) Grad (0x18)
      /// </summary>
      public Longitude East;
      /// <summary>
      /// südliche Kartengrenze in 360/(2^24) Grad (0x1B)
      /// </summary>
      public Latitude South;
      /// <summary>
      /// westliche Kartengrenze in 360/(2^24) Grad (0x1E)
      /// </summary>
      public Longitude West;
      /// <summary>
      /// Datenbereich für die Tabelle der Maplevel (0x21)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlock MaplevelBlock;
      /// <summary>
      /// Datenbereich für die Tabelle der Subdivisions (0x29)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlock SubdivisionBlock;
      /// <summary>
      /// Datenbereich für die Tabelle der Copyright-Texte (0x31)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlockWithRecordsize CopyrightBlock;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x3B = { 0, 0, 0, 0 };
      /// <summary>
      /// Optionen für die POI-Anzeige (0x3F)
      /// </summary>
      public byte POIDisplayFlags { get; private set; }
      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_TransparentMap {
         get {
            return Bit.IsSet(POIDisplayFlags, 1);
         }
         set {
            POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 1, value);
         }
      }
      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_ShowStreetBeforeNumber {
         get {
            return Bit.IsSet(POIDisplayFlags, 2);
         }
         set {
            POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 2, value);
         }
      }
      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_ShowZipBeforeCity {
         get {
            return Bit.IsSet(POIDisplayFlags, 3);
         }
         set {
            POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 3, value);
         }
      }
      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_DriveLeft {
         get {
            return Bit.IsSet(POIDisplayFlags, 5);
         }
         set {
            POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 5, value);
         }
      }
      /// <summary>
      /// Kartenlayer ? (0x40)
      /// </summary>
      public int DisplayPriority;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x43 = { 0x01, 0x03, 0x11, 0x00, 0x01, 0x00, 0x00 };
      // public byte[] Unknown_x43 = { 0x01, 0x04, 0x17, 0x00, 0x01, 0x00, 0x00 };      // Topo D V3
      /// <summary>
      /// Datenbereich für die Tabelle der Polylines (0x4A)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlockWithRecordsize LineOverviewBlock;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x54 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Tabelle der Polygone (0x58)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlockWithRecordsize AreaOverviewBlock;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x62 = { 0, 0, 0, 0 };
      /// <summary>
      /// Datenbereich für die Tabelle der Punkte (0x66)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlockWithRecordsize PointOverviewBlock;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x70 = { 0, 0, 0, 0 };

      // --------- Headerlänge > 116 Byte (zusätzlich Map-ID)

      /// <summary>
      /// Karten-ID (0x74)
      /// </summary>
      public UInt32 MapID;

      // --------- Headerlänge > 120 Byte (zusätzlich erweiterte Typen)

#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x78 = { 0, 0, 0, 0 };
      /// <summary>
      /// Offsets auf die Datenbereiche der erweiterten Typen in der RGN Datei
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlockWithRecordsize ExtTypeOffsetsBlock;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_x86 = { 0x07, 0x06, 0x00, 0x00 };
      /// <summary>
      /// Verweis auf die Tabelle der erweiterten Typen (0x8A)
      /// </summary>
#if IS4EXPLORING
      public
#endif
      DataBlockWithRecordsize ExtTypeOverviewsBlock;
      /// <summary>
      /// Anzahl der erweiterten Polylinientypen (0x94)
      /// </summary>
      public UInt16 ExtLineCount { get; private set; }
      /// <summary>
      /// Anzahl der erweiterten Polygontypen (0x96)
      /// </summary>
      public UInt16 ExtAreaCount { get; private set; }
      /// <summary>
      /// Anzahl der erweiterten Punkttypen (0x98)
      /// </summary>
      public UInt16 ExtPointCount { get; private set; }

      // --------- Headerlänge > 154 Byte ("Verschlüsselung" der Maplevel-Tabelle)

      /// <summary>
      /// (0x9A)
      /// </summary>
      public UInt32[] MapValues;
      /// <summary>
      /// Verschlüsselungs-Key für die Mapleveltabelle
      /// </summary>
#if IS4EXPLORING
      public
#endif
      UInt32 MaplevelScrambleKey;
#if IS4EXPLORING
      public
#endif
      DataBlock? UnknownBlock_xAE;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_xB6 = new byte[0x6];

      // --------- Headerlänge > 188 Byte

#if IS4EXPLORING
      public
#endif
      DataBlock? UnknownBlock_xBC;
#if IS4EXPLORING
      public 
#endif
      byte[] Unknown_xC4 = new byte[0x1F];
#if IS4EXPLORING
      public
#endif
      DataBlock? UnknownBlock_xE3;
#if IS4EXPLORING
      public 
#endif
      byte[]? Unknown_xEB = null;

      #endregion

      enum InternalFileSections {
         DescriptionBlock = 0,
         MaplevelBlock,
         SubdivisionBlock,
         CopyrightBlock,
         LineOverviewBlock,
         AreaOverviewBlock,
         PointOverviewBlock,
         ExtTypeOffsetsBlock,
         ExtTypeOverviewsBlock,
         UnknownBlock_xAE,
         UnknownBlock_xBC,
         UnknownBlock_xE3,
      }

      /// <summary>
      /// zur Verwaltung der MapLevel
      /// </summary>
      public SymbolicScaleDenominatorAndBits? SymbolicScaleDenominatorAndBitsLevel;

      /// <summary>
      /// liefert den Datenbereich für die Kartenbeschreibung
      /// </summary>
      /// <returns></returns>
      public DataBlock? MapDescriptionBlock { get; private set; }

      /// <summary>
      /// Liste der Texte für die Kartenbeschreibung
      /// </summary>
      public List<string> MapDescriptionList;
      /// <summary>
      /// Liste der Copyright-Offsets
      /// </summary>
      public List<uint> CopyrightOffsetsList;
      /// <summary>
      /// Liste der Maplevel (Index 0 mit den geringsten Details, d.h. kleinster Maßstab)
      /// </summary>
      public List<MapLevel> MaplevelList { get; private set; }
      /// <summary>
      /// Liste ALLER Subdivinfos (wird sequentiell in der TRE-Datei gespeichert; Verweise sind 1-basiert; max. ushort-Elemente)
      /// </summary>
      public List<SubdivInfoBasic> SubdivInfoList;

      /// <summary>
      /// Offsets in der RGN-Datei für die Linien für jede Subdiv (Subdivnummer und Datenblock)
      /// </summary>
      public SortedDictionary<int, DataBlock> ExtLineBlock4Subdiv;
      /// <summary>
      /// Offsets in der RGN-Datei für die Flächen für jede Subdiv (Subdivnummer und Datenblock)
      /// </summary>
      public SortedDictionary<int, DataBlock> ExtAreaBlock4Subdiv;
      /// <summary>
      /// Offsets in der RGN-Datei für die Punkte für jede Subdiv (Subdivnummer und Datenblock)
      /// </summary>
      public SortedDictionary<int, DataBlock> ExtPointBlock4Subdiv;

      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene Polylinientypen (.. 0xff)
      /// </summary>
      public List<OverviewObject2Byte> LineOverviewList { get; private set; }
      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene Polygontypen (.. 0xff)
      /// </summary>
      public List<OverviewObject2Byte> AreaOverviewList { get; private set; }
      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene Punkttypen (.. 0xff)
      /// </summary>
      public List<OverviewObject3Byte> PointOverviewList { get; private set; }

      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene erweiterten Polylinientypen (0x100 .. mit Subtyp)
      /// </summary>
      public List<OverviewObject4Byte> ExtLineOverviewList { get; private set; }
      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene erweiterten Polygontypen (0x100 .. mit Subtyp)
      /// </summary>
      public List<OverviewObject4Byte> ExtAreaOverviewList { get; private set; }
      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene erweiterten Punkttypen (0x100 .. mit Subtyp)
      /// </summary>
      public List<OverviewObject4Byte> ExtPointOverviewList { get; private set; }


      public StdFile_TRE() : base("TRE") {
         Headerlength = 0x78;
         Headerlength = 0xbc;

         North =
         South = 0;
         East =
         West = 0;

         POIDisplayFlags = 0;
         DisplayPriority = 31;
         MapID = 0;
         ExtLineCount =
         ExtAreaCount =
         ExtPointCount = 0;
         MapValues = new UInt32[4];

         MapDescriptionList = new List<string>();
         CopyrightOffsetsList = new List<uint>();
         MaplevelList = new List<MapLevel>();
         SubdivInfoList = new List<SubdivInfoBasic>();
         LineOverviewList = new List<OverviewObject2Byte>();
         AreaOverviewList = new List<OverviewObject2Byte>();
         PointOverviewList = new List<OverviewObject3Byte>();
         ExtLineOverviewList = new List<OverviewObject4Byte>();
         ExtAreaOverviewList = new List<OverviewObject4Byte>();
         ExtPointOverviewList = new List<OverviewObject4Byte>();

         ExtLineBlock4Subdiv = new SortedDictionary<int, DataBlock>();
         ExtAreaBlock4Subdiv = new SortedDictionary<int, DataBlock>();
         ExtPointBlock4Subdiv = new SortedDictionary<int, DataBlock>();

         MaplevelBlock = new DataBlock();
         SubdivisionBlock = new DataBlock();
         CopyrightBlock = new DataBlockWithRecordsize(new DataBlock(), 3);
         LineOverviewBlock = new DataBlockWithRecordsize(new DataBlock(), 2);
         AreaOverviewBlock = new DataBlockWithRecordsize(new DataBlock(), 2);
         PointOverviewBlock = new DataBlockWithRecordsize(new DataBlock(), 3);
         ExtTypeOffsetsBlock = new DataBlockWithRecordsize(new DataBlock(), 13);
         ExtTypeOverviewsBlock = new DataBlockWithRecordsize(new DataBlock(), 4);
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         North = br.Read3Int();
         East = br.Read3Int();
         South = br.Read3Int();
         West = br.Read3Int();

         MaplevelBlock = new DataBlock(br);
         SubdivisionBlock = new DataBlock(br);
         CopyrightBlock = new DataBlockWithRecordsize(br);

         br.ReadBytes(Unknown_x3B);
         POIDisplayFlags = br.ReadByte();
         DisplayPriority = br.Read3Int();
         br.ReadBytes(Unknown_x43);

         LineOverviewBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x54);
         AreaOverviewBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x62);
         PointOverviewBlock = new DataBlockWithRecordsize(br);
         br.ReadBytes(Unknown_x70);

         if (Headerlength > 0x74) {          // > 116
            MapID = br.Read4UInt();

            if (Headerlength > 0x78) {       // > 120
               br.ReadBytes(Unknown_x78);
               ExtTypeOffsetsBlock = new DataBlockWithRecordsize(br);
               br.ReadBytes(Unknown_x86);
               ExtTypeOverviewsBlock = new DataBlockWithRecordsize(br);
               ExtLineCount = br.Read2AsUShort();
               ExtAreaCount = br.Read2AsUShort();
               ExtPointCount = br.Read2AsUShort();

               if (Headerlength > 0x9a) {    // > 154

                  for (int i = 0; i < MapValues.Length; i++)
                     MapValues[i] = br.Read4UInt();
                  MaplevelScrambleKey = br.Read4UInt();

                  UnknownBlock_xAE = new DataBlock(br);
                  br.ReadBytes(Unknown_xB6);

                  if (Headerlength > 0xbc) { // > 188
                     UnknownBlock_xBC = new DataBlock(br);
                     br.ReadBytes(Unknown_xC4);
                     if (Headerlength > 0xfc) {
                        UnknownBlock_xE3 = new DataBlock(br);
                        Unknown_xEB = new byte[Headerlength - 0xEB];
                        br.ReadBytes(Unknown_xEB);
                     }
                  }
               }
            }

         }
      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         // Wenn jetzt eine Lücke zum 1. Datenbereich oder zum nächsten Dateiheader (bei GMP) besteht, sollte eine Beschreibung folgen, die aus mehreren
         // 0-terminierten Zeichenketten besteht. Es existiert KEINE Endekennung dieses Bereiches.

         Filesections.AddSection((int)InternalFileSections.MaplevelBlock, new DataBlock(MaplevelBlock));
         Filesections.AddSection((int)InternalFileSections.SubdivisionBlock, new DataBlock(SubdivisionBlock));
         Filesections.AddSection((int)InternalFileSections.CopyrightBlock, new DataBlockWithRecordsize(CopyrightBlock));
         Filesections.AddSection((int)InternalFileSections.LineOverviewBlock, new DataBlockWithRecordsize(LineOverviewBlock));
         Filesections.AddSection((int)InternalFileSections.AreaOverviewBlock, new DataBlockWithRecordsize(AreaOverviewBlock));
         Filesections.AddSection((int)InternalFileSections.PointOverviewBlock, new DataBlockWithRecordsize(PointOverviewBlock));
         if (ExtTypeOffsetsBlock != null)
            Filesections.AddSection((int)InternalFileSections.ExtTypeOffsetsBlock, new DataBlockWithRecordsize(ExtTypeOffsetsBlock));
         if (ExtTypeOverviewsBlock != null)
            Filesections.AddSection((int)InternalFileSections.ExtTypeOverviewsBlock, new DataBlockWithRecordsize(ExtTypeOverviewsBlock));
         if (UnknownBlock_xAE != null)
            Filesections.AddSection((int)InternalFileSections.UnknownBlock_xAE, new DataBlock(UnknownBlock_xAE));
         if (UnknownBlock_xBC != null)
            Filesections.AddSection((int)InternalFileSections.UnknownBlock_xBC, new DataBlock(UnknownBlock_xBC));
         if (UnknownBlock_xE3 != null)
            Filesections.AddSection((int)InternalFileSections.UnknownBlock_xE3, new DataBlock(UnknownBlock_xE3));

         // GapOffset und DataOffset setzen
         SetSpecialOffsetsFromSections((int)InternalFileSections.DescriptionBlock);

         if (GapOffset > HeaderOffset + Headerlength) { // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            MapDescriptionBlock = new DataBlock(HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));
            Filesections.AddSection((int)InternalFileSections.DescriptionBlock, MapDescriptionBlock);
         }

         // Datenblöcke einlesen
         Filesections.ReadSections(br);
      }

      protected override void DecodeSections() {
         MapDescriptionList.Clear();
         CopyrightOffsetsList.Clear();
         MaplevelList.Clear();
         SubdivInfoList.Clear();
         LineOverviewList.Clear();
         AreaOverviewList.Clear();
         PointOverviewList.Clear();
         ExtLineOverviewList.Clear();
         ExtAreaOverviewList.Clear();
         ExtPointOverviewList.Clear();

         if (Locked != 0) {
            RawRead = true;
            Decode_DescriptionBlock(Filesections.GetSectionDataReader((int)InternalFileSections.DescriptionBlock), new DataBlock(0, Filesections.GetLength((int)InternalFileSections.DescriptionBlock)));
            return;
         }

         // Datenblöcke "interpretieren"
         int filesectiontype;

         // Beschreibung einlesen
         filesectiontype = (int)InternalFileSections.DescriptionBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_DescriptionBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)));
            Filesections.RemoveSection(filesectiontype);
         }

         // Copyright-Offsets einlesen
         filesectiontype = (int)InternalFileSections.CopyrightBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_CopyrightBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         // alle Maplevel einlesen
         filesectiontype = (int)InternalFileSections.MaplevelBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            if (!Decode_MapLevelBlock(Filesections.GetSectionDataReader(filesectiontype), bl))
               return;
            Filesections.RemoveSection(filesectiontype);
         }

         // alle Subdivs einlesen
         filesectiontype = (int)InternalFileSections.SubdivisionBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            Decode_SubdivisionBlock(Filesections.GetSectionDataReader(filesectiontype), new DataBlock(0, Filesections.GetLength(filesectiontype)), MaplevelList);
            Filesections.RemoveSection(filesectiontype);
         }

         // Overview-Objekte einlesen
         filesectiontype = (int)InternalFileSections.LineOverviewBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_LineOverviewBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.AreaOverviewBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_AreaOverviewBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         filesectiontype = (int)InternalFileSections.PointOverviewBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_PointOverviewBlock(Filesections.GetSectionDataReader(filesectiontype), bl);
            Filesections.RemoveSection(filesectiontype);
         }

         // erweiterte Typen einlesen
         filesectiontype = (int)InternalFileSections.ExtTypeOverviewsBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            Decode_ExtTypeOverviewsBlock(Filesections.GetSectionDataReader(filesectiontype), bl, ExtLineCount, ExtAreaCount, ExtPointCount);
            Filesections.RemoveSection(filesectiontype);
         }

         // Offsets für die erweiterten Typen einlesen und umwandeln
         filesectiontype = (int)InternalFileSections.ExtTypeOffsetsBlock;
         if (Filesections.GetLength(filesectiontype) > 0) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(Filesections.GetPosition(filesectiontype)) {
               Offset = 0
            };
            SplitExtTypeOffsetList(Decode_ExtTypeOffsetsBlock(Filesections.GetSectionDataReader(filesectiontype), bl));
            Filesections.RemoveSection(filesectiontype);
         }
      }

      #region Decodierung der Datenblöcke

      /// <summary>
      /// liest die Kartenbeschreibung aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_DescriptionBlock(BinaryReaderWriter? br, DataBlock block) {
         MapDescriptionList.Clear();
         if (br != null) {
            if (br != null) {
               br.Seek(block.Offset);
               while (br.Position < block.Offset + block.Length)
                  MapDescriptionList.Add(br.ReadString());
            }
         }
      }

      /// <summary>
      /// liest die Copyright-Offset-Daten aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_CopyrightBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block) {
         CopyrightOffsetsList.Clear();
         if (br != null)
            CopyrightOffsetsList = br.ReadUintArray(block);
      }

      /// <summary>
      /// liest die MapLevel-Daten aus dem <see cref="BinaryReaderWriter"/> und versucht, sie bei Bedarf zu entschlüsseln
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block">Datenbereich aus dem gelesen wird</param>
      /// <returns>false, wenn nicht vorhanden oder nicht entschlüsselbar</returns>
      bool Decode_MapLevelBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block) {
         if (br != null) {
            SymbolicScaleDenominatorAndBitsLevel = new SymbolicScaleDenominatorAndBits();
            if (!UnlockMapLevel(br, block))
               return false;
            SymbolicScaleDenominatorAndBitsLevel = new SymbolicScaleDenominatorAndBits(MaplevelList);
         } else
            return false;
         return true;
      }

      /// <summary>
      /// liest die Subdivision-Infodaten aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block">Datenbereich aus dem gelesen wird</param>
      /// <param name="maplevelList">Maplevel-Definitionen</param>
      /// <returns>false, wenn nicht plausibel</returns>
      bool Decode_SubdivisionBlock(BinaryReaderWriter? br, DataBlock block, List<MapLevel> maplevelList) {
         SubdivInfoList.Clear();
         if (br != null &&
             block.Length > 0) {
            DataBlock tmpbl = new DataBlock(block.Offset, 0);

            for (int i = 0; i < maplevelList.Count; i++) {
               tmpbl.Offset += tmpbl.Length;                   // die Blöcke je Maplevel liegen direkt hintereinander
               if (tmpbl.Offset >= block.Offset + block.Length)
                  throw new Exception("Die Maplevel-Definitionen passen nicht zum Datenblock der SubdivInfos.");

               maplevelList[i].FirstSubdivInfoNumber = (UInt16)(SubdivInfoList.Count + 1);
               if (i < maplevelList.Count - 1) {               // NICHT der niedrigste Maplevel, also NICHT die höchste Zoomstufe, also liegen SubdivInfo-Einträge vor
                  tmpbl.Length = (uint)(maplevelList[i].SubdivInfos * SubdivInfo.DataLength);
                  List<SubdivInfo> tmp = br.ReadArray<SubdivInfo>(tmpbl);
                  for (int j = 0; j < tmp.Count; j++)
                     SubdivInfoList.Add(tmp[j]);
               } else {                                        // niedrigster Maplevel == höchste Zoomstufe, also liegen SubdivInfoBasic-Einträge vor
                  tmpbl.Length = (uint)(maplevelList[i].SubdivInfos * SubdivInfoBasic.DataLength);
                  List<SubdivInfoBasic> tmp = br.ReadArray<SubdivInfoBasic>(tmpbl);
                  for (int j = 0; j < tmp.Count; j++)
                     SubdivInfoList.Add(tmp[j]);
               }
            }

            for (int i = 0; i < SubdivInfoList.Count; i++)        // "Verkettung" setzen, d.h. die Anzahl der untergeordneten SubdivInfos
               if (SubdivInfoList[i] is SubdivInfo) {
                  SubdivInfo sdi = (SubdivInfo)SubdivInfoList[i];
                  for (int j = sdi.FirstChildSubdivIdx1; j > 0; j++)
                     if (SubdivInfoList[j - 1].LastSubdiv) {      // 1-basierter Index
                        sdi.ChildSubdivInfos = (UInt16)(j - sdi.FirstChildSubdivIdx1 + 1);
                        break;
                     }
               }

            UInt32 endpos_rgn = br.Read4UInt(); // Endposition des Datenbereichs in RGN-Datei lesen (treFile.setLastRgnPos(rgnFile.position() - RGNHeader.HEADER_LEN);)
            // Jetzt kann die Länge der Datenblöcke jeder Subdiv gesetzt werden.
            for (int i = 0; i < SubdivInfoList.Count - 1; i++)
               SubdivInfoList[i].Data.Length = SubdivInfoList[i + 1].Data.Offset - SubdivInfoList[i].Data.Offset;
            if (SubdivInfoList.Count > 0)
               SubdivInfoList[SubdivInfoList.Count - 1].Data.Length = endpos_rgn - SubdivInfoList[SubdivInfoList.Count - 1].Data.Offset;
         }
         return SubdivPlausibility(MaplevelList, SubdivInfoList);
      }

      /// <summary>
      /// liest die PointOverview-Daten aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_PointOverviewBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block) {
         PointOverviewList.Clear();
         if (br != null &&
             block.Length > 0) {
            switch (block.Recordsize) {
               case 2:
                  List<OverviewObject2Byte> tmp2 = br.ReadArray<OverviewObject2Byte>(block);
                  for (int i = 0; i < tmp2.Count; i++)
                     PointOverviewList.Add(new OverviewObject3Byte(tmp2[i].Type, 0, tmp2[i].MaxLevel));
                  break;

               case 3:
                  PointOverviewList = br.ReadArray<OverviewObject3Byte>(block);
                  break;

               case 4:
                  List<OverviewObject4Byte> tmp4 = br.ReadArray<OverviewObject4Byte>(block);
                  for (int i = 0; i < tmp4.Count; i++)
                     PointOverviewList.Add(tmp4[i]);
                  break;

               default:
                  throw new Exception("Die Datensatzlänge " + block.Recordsize.ToString() + " für die Overview-Objekt-Tabelle der Punkte ist unbekannt (2..4).");
            }
         }
      }

      /// <summary>
      /// liest die PolygoneOverview-Daten aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block">Datenbereich aus dem gelesen wird</param>
      void Decode_AreaOverviewBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block) {
         AreaOverviewList.Clear();
         if (br != null &&
             block.Length > 0) {
            switch (block.Recordsize) {
               case 2:
                  AreaOverviewList = br.ReadArray<OverviewObject2Byte>(block);
                  break;

               case 3:
                  List<OverviewObject3Byte> tmp3 = br.ReadArray<OverviewObject3Byte>(block);
                  for (int i = 0; i < tmp3.Count; i++)
                     AreaOverviewList.Add(tmp3[i]);
                  break;

               case 4:
                  List<OverviewObject4Byte> tmp4 = br.ReadArray<OverviewObject4Byte>(block);
                  for (int i = 0; i < tmp4.Count; i++)
                     AreaOverviewList.Add(tmp4[i]);
                  break;

               default:
                  throw new Exception("Die Datensatzlänge " + block.Recordsize.ToString() + " für die Overview-Objekt-Tabelle der Polygone ist unbekannt (2..4).");
            }
         }
      }

      /// <summary>
      /// liest die PolylineOverview-Daten aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block">Datenbereich aus dem gelesen wird</param>
      void Decode_LineOverviewBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block) {
         LineOverviewList.Clear();
         if (br != null &&
             block.Length > 0) {
            switch (block.Recordsize) {
               case 2:
                  LineOverviewList = br.ReadArray<OverviewObject2Byte>(block);
                  break;

               case 3:
                  List<OverviewObject3Byte> tmp3 = br.ReadArray<OverviewObject3Byte>(block);
                  for (int i = 0; i < tmp3.Count; i++)
                     LineOverviewList.Add(tmp3[i]);
                  break;

               case 4:
                  List<OverviewObject4Byte> tmp4 = br.ReadArray<OverviewObject4Byte>(block);
                  for (int i = 0; i < tmp4.Count; i++)
                     LineOverviewList.Add(tmp4[i]);
                  break;

               default:
                  throw new Exception("Die Datensatzlänge " + block.Recordsize.ToString() + " für die Overview-Objekt-Tabelle der Polylinien ist unbekannt (2..4).");
            }
         }
      }

      /// <summary>
      /// liest die erweiterten Typen aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      /// <param name="extPolylineCount">Anzahl der Linientypen</param>
      /// <param name="extPolygoneCount">Anzahl der Flächentypen</param>
      /// <param name="extPointCount">Anzahl der Punkttypen</param>
      void Decode_ExtTypeOverviewsBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block, ushort extPolylineCount, ushort extPolygoneCount, ushort extPointCount) {
         ExtLineOverviewList.Clear();
         ExtAreaOverviewList.Clear();
         ExtPointOverviewList.Clear();

         if (br != null) {
            DataBlockWithRecordsize bl = new DataBlockWithRecordsize(block);

            bl.Length = (uint)(extPolylineCount * bl.Recordsize);
            switch (bl.Recordsize) {
               case 3:
                  List<OverviewObject3Byte> tmp3 = br.ReadArray<OverviewObject3Byte>(bl);
                  for (int i = 0; i < tmp3.Count; i++)
                     ExtLineOverviewList.Add(new OverviewObject4Byte(tmp3[i].Type, tmp3[i].SubType, tmp3[i].MaxLevel, 0));
                  break;
               case 4:
                  ExtLineOverviewList = br.ReadArray<OverviewObject4Byte>(bl);
                  break;

               default:
                  throw new Exception("Die Datensatzlänge " + bl.Recordsize.ToString() + " für die Overview-Objekt-Tabelle der erweiterten Linien ist zu groß (max. 4).");
            }

            bl.Offset += bl.Length;
            bl.Length = (uint)(extPolygoneCount * bl.Recordsize);
            switch (bl.Recordsize) {
               case 3:
                  List<OverviewObject3Byte> tmp3 = br.ReadArray<OverviewObject3Byte>(bl);
                  for (int i = 0; i < tmp3.Count; i++)
                     ExtAreaOverviewList.Add(new OverviewObject4Byte(tmp3[i].Type, tmp3[i].SubType, tmp3[i].MaxLevel, 0));
                  break;
               case 4:
                  ExtAreaOverviewList = br.ReadArray<OverviewObject4Byte>(bl);
                  break;

               default:
                  throw new Exception("Die Datensatzlänge " + bl.Recordsize.ToString() + " für die Overview-Objekt-Tabelle der erweiterten Flächen ist zu groß (max. 4).");
            }

            bl.Offset += bl.Length;
            bl.Length = (uint)(extPointCount * bl.Recordsize);
            switch (bl.Recordsize) {
               case 3:
                  List<OverviewObject3Byte> tmp3 = br.ReadArray<OverviewObject3Byte>(bl);
                  for (int i = 0; i < tmp3.Count; i++)
                     ExtPointOverviewList.Add(new OverviewObject4Byte(tmp3[i].Type, tmp3[i].SubType, tmp3[i].MaxLevel, 0));
                  break;
               case 4:
                  ExtPointOverviewList = br.ReadArray<OverviewObject4Byte>(bl);
                  break;

               default:
                  throw new Exception("Die Datensatzlänge " + bl.Recordsize.ToString() + " für die Overview-Objekt-Tabelle der erweiterten Punkte ist zu groß (max. 4).");
            }
         }
      }

      /// <summary>
      /// liest die Offsets für die erweiterten Typen aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      /// <returns></returns>
      List<ExtendedTypeOffsets>? Decode_ExtTypeOffsetsBlock(BinaryReaderWriter? br, DataBlockWithRecordsize block) {
         if (br != null)
            return br.ReadArray<ExtendedTypeOffsets>(block, new object[] { block.Recordsize });
         return null;
      }

      /// <summary>
      /// einfacher Test, ob die <see cref="MapLevel"/>-Liste plausibel ist
      /// </summary>
      /// <param name="ml"></param>
      /// <param name="lax">bei false wird ein etwas "härterer" Test durchgeführt</param>
      /// <returns></returns>
      static public bool MaplevelsPlausibility(IList<MapLevel> ml, bool lax = false) {
         if (ml != null && ml.Count > 0) {
            // absteigender SymbolicScale, z.B. 15..0
            // Bit4, Bit5, Bit6 nicht gesetzt
            // aufsteigende Bitanzahl für Koordinaten, z.B. 12..24
            // aufsteigende (nicht kleiner werdende) Anzahl Subdivs, etwa 1..

            if (!ml[0].Inherited)
               return false;

            for (int i = 1; i < ml.Count; i++) {
               if (ml[i].SymbolicScaleDenominator >= ml[i - 1].SymbolicScaleDenominator)
                  return false;
               if (ml[i].CoordBits <= ml[i - 1].CoordBits)
                  return false;
               if (ml[i].SubdivInfos <= ml[i - 1].SubdivInfos)
                  return false;
               if (ml[i].Inherited ||
                   ml[i].Bit4 ||
                   ml[i].Bit4 ||
                   ml[i].Bit6)
                  return false;
            }

            if (lax)
               return true;

            // Die folgenden Bedingungen sind normalerweise auch (aber nicht notwendigerweise) erfüllt.
            if (ml.Count < 3)
               return false;

            if (ml[ml.Count - 1].SymbolicScaleDenominator != 24)
               return false;

            if (ml[0].SubdivInfos != 1)
               return false;

            return true;
         }

         return false;
      }

      /// <summary>
      /// Test, ob die <see cref="SubdivInfoBasic"/>-Liste plausibel ist
      /// </summary>
      /// <param name="ml"></param>
      /// <param name="sdi"></param>
      /// <returns></returns>
      static public bool SubdivPlausibility(IList<MapLevel> ml, IList<SubdivInfoBasic> sdi) {
         // leere Subdivs sollten die Länge 0 haben, nichtleere eine >0
         // die letzte Subdiv eines Maplevels muss true sein
         for (int m = 0; m < ml.Count; m++) {
            int lastidx = ml[m].FirstSubdivInfoNumber - 1 + ml[m].SubdivInfos - 1;
            if (lastidx >= sdi.Count)
               return false;
            if ((sdi[lastidx] is SubdivInfo) &&
                !((SubdivInfo)sdi[lastidx]).LastSubdiv)
               return false;
         }
         for (int i = 0; i < sdi.Count - 1; i++) {
            if (sdi[i].Content == SubdivInfoBasic.SubdivContent.nothing &&
                sdi[i].Data.Offset != sdi[i + 1].Data.Offset)
               return false;

            if (sdi[i].Content != SubdivInfoBasic.SubdivContent.nothing &&
                sdi[i].Data.Offset == sdi[i + 1].Data.Offset)
               return false;
         }
         return true;
      }

      /// <summary>
      /// teilt die gespeicherte Liste in 3 Offset-Listen auf und löscht den Originalinhalt
      /// </summary>
      /// <param name="ExtTypeOffsetList"></param>
      void SplitExtTypeOffsetList(List<ExtendedTypeOffsets>? ExtTypeOffsetList) {
         ExtLineBlock4Subdiv.Clear();
         ExtAreaBlock4Subdiv.Clear();
         ExtPointBlock4Subdiv.Clear();
         if (ExtTypeOffsetList != null)
            for (int i = 0; i < ExtTypeOffsetList.Count - 1; i++) {
               if (ExtTypeOffsetList[i].LinesOffset != ExtTypeOffsetList[i + 1].LinesOffset)       // Bereich ex.
                  ExtLineBlock4Subdiv.Add(i, new DataBlock(ExtTypeOffsetList[i].LinesOffset, ExtTypeOffsetList[i + 1].LinesOffset - ExtTypeOffsetList[i].LinesOffset));
               if (ExtTypeOffsetList[i].AreasOffset != ExtTypeOffsetList[i + 1].AreasOffset)       // Bereich ex.
                  ExtAreaBlock4Subdiv.Add(i, new DataBlock(ExtTypeOffsetList[i].AreasOffset, ExtTypeOffsetList[i + 1].AreasOffset - ExtTypeOffsetList[i].AreasOffset));
               if (ExtTypeOffsetList[i].PointsOffset != ExtTypeOffsetList[i + 1].PointsOffset)     // Bereich ex.
                  ExtPointBlock4Subdiv.Add(i, new DataBlock(ExtTypeOffsetList[i].PointsOffset, ExtTypeOffsetList[i + 1].PointsOffset - ExtTypeOffsetList[i].PointsOffset));
            }
      }

      #endregion

      /// <summary>
      /// alle Datenabschnitte neu encodieren
      /// </summary>
      public override void Encode_Sections() {
         SetData2Filesection((int)InternalFileSections.DescriptionBlock, true);
         SetData2Filesection((int)InternalFileSections.CopyrightBlock, true);
         SetData2Filesection((int)InternalFileSections.MaplevelBlock, true);
         SetData2Filesection((int)InternalFileSections.PointOverviewBlock, true);
         SetData2Filesection((int)InternalFileSections.AreaOverviewBlock, true);
         SetData2Filesection((int)InternalFileSections.LineOverviewBlock, true);
         SetData2Filesection((int)InternalFileSections.ExtTypeOffsetsBlock, true);
         SetData2Filesection((int)InternalFileSections.ExtTypeOverviewsBlock, true);
         SetData2Filesection((int)InternalFileSections.SubdivisionBlock, true);
      }

      /// <summary>
      /// einen bestimmten Datenabschnitt encodieren
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="filesectiontype"></param>
      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) {
         switch ((InternalFileSections)filesectiontype) {
            case InternalFileSections.CopyrightBlock:
               Encode_CopyrightBlock(bw);
               break;

            case InternalFileSections.DescriptionBlock:
               Encode_DescriptionBlock(bw);
               break;

            case InternalFileSections.MaplevelBlock:
               Encode_MaplevelBlock(bw);
               break;

            case InternalFileSections.PointOverviewBlock:
               Encode_PointOverviewBlock(bw);
               break;

            case InternalFileSections.AreaOverviewBlock:
               Encode_AreaOverviewBlock(bw);
               break;

            case InternalFileSections.LineOverviewBlock:
               Encode_LineOverviewBlock(bw);
               break;

            case InternalFileSections.ExtTypeOverviewsBlock:
               Encode_ExtTypeOverviewsBlock(bw);
               break;

            case InternalFileSections.ExtTypeOffsetsBlock:
               Encode_ExtTypeOffsetsBlock(bw);
               break;

            case InternalFileSections.SubdivisionBlock:
               Encode_SubdivisionBlock(bw);
               break;
         }
      }

      #region Encodierung der Datenblöcke und des Headers

      void Encode_DescriptionBlock(BinaryReaderWriter bw) {
         if (bw != null)
            foreach (string txt in MapDescriptionList)
               bw.WriteString(txt);
      }

      void Encode_CopyrightBlock(BinaryReaderWriter bw) {
         if (bw != null)
            foreach (uint offs in CopyrightOffsetsList)
               bw.Write3(offs);
      }

      void Encode_MaplevelBlock(BinaryReaderWriter bw) {
         if (bw != null && SymbolicScaleDenominatorAndBitsLevel != null) {
            MaplevelList = SymbolicScaleDenominatorAndBitsLevel.GetMaplevelList();
            foreach (MapLevel item in MaplevelList)
               item.Write(bw);
         }
      }

      void Encode_PointOverviewBlock(BinaryReaderWriter bw) {
         if (bw != null) {
            foreach (var item in PointOverviewList)
               if (item is OverviewObject3Byte)
                  item.Write(bw);
               else
                  throw new Exception("Falscher Objekttyp in der PointOverviewList.");
         }
      }

      void Encode_AreaOverviewBlock(BinaryReaderWriter bw) {
         if (bw != null)
            foreach (var item in AreaOverviewList)
               if (item is OverviewObject2Byte)
                  item.Write(bw);
               else
                  throw new Exception("Falscher Objekttyp in der PolygoneOverviewList.");
      }

      void Encode_LineOverviewBlock(BinaryReaderWriter bw) {
         if (bw != null)
            foreach (var item in LineOverviewList)
               if (item is OverviewObject2Byte)
                  item.Write(bw);
               else
                  throw new Exception("Falscher Objekttyp in der PolylineOverviewList.");
      }

      void Encode_ExtTypeOverviewsBlock(BinaryReaderWriter bw) {
         if (bw != null) {
            ExtLineCount = 0;
            ExtAreaCount = 0;
            ExtPointCount = 0;
            foreach (var item in ExtLineOverviewList)
               if (item is OverviewObject4Byte) {
                  item.Write(bw);
                  ExtLineCount++;
               } else
                  throw new Exception("Falscher Objekttyp in der ExtPolylineOverviewList.");

            foreach (var item in ExtAreaOverviewList)
               if (item is OverviewObject4Byte) {
                  item.Write(bw);
                  ExtAreaCount++;
               } else
                  throw new Exception("Falscher Objekttyp in der ExtPolygoneOverviewList.");

            foreach (var item in ExtPointOverviewList)
               if (item is OverviewObject4Byte) {
                  item.Write(bw);
                  ExtPointCount++;
               } else
                  throw new Exception("Falscher Objekttyp in der ExtPointOverviewList.");
         }
      }

      void Encode_ExtTypeOffsetsBlock(BinaryReaderWriter bw) {
         if (bw != null)
            if (ExtLineOverviewList.Count > 0 ||
                ExtAreaOverviewList.Count > 0 ||
                ExtPointOverviewList.Count > 0)
               foreach (var item in BuildExtTypeOffsetList())
                  item.Write(bw);
      }

      void Encode_SubdivisionBlock(BinaryReaderWriter bw) {
         if (SubdivInfoList.Count >= ushort.MaxValue)
            throw new Exception(string.Format("Zu viele Subdiv's: {0}, aber nur {1} erlaubt.", SubdivInfoList.Count, ushort.MaxValue - 1));

         if (bw != null) {
            foreach (var sd in SubdivInfoList)
               sd.Write(bw);
            if (SubdivInfoList.Count > 0) // Endposition in RGN-Datei schreiben (treFile.setLastRgnPos(rgnFile.position() - RGNHeader.HEADER_LEN);)
               bw.Write((Int32)(SubdivInfoList[SubdivInfoList.Count - 1].Data.Offset + SubdivInfoList[SubdivInfoList.Count - 1].Data.Length - SubdivInfoList[0].Data.Offset));
            else
               bw.Write((Int32)0);
         }
      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            // Header-Daten schreiben
            bw.Write3(North);
            bw.Write3(East);
            bw.Write3(South);
            bw.Write3(West);
            MaplevelBlock.Write(bw);
            SubdivisionBlock.Write(bw);
            CopyrightBlock.Write(bw);
            bw.Write(Unknown_x3B);
            bw.Write(POIDisplayFlags);
            bw.Write3(DisplayPriority);
            bw.Write(Unknown_x43);
            LineOverviewBlock.Write(bw);
            bw.Write(Unknown_x54);
            AreaOverviewBlock.Write(bw);
            bw.Write(Unknown_x62);
            PointOverviewBlock.Write(bw);
            bw.Write(Unknown_x70);

            if (Headerlength > 0x74) {          // > 116
               bw.Write(MapID);

               if (Headerlength > 0x78) {       // > 120
                  bw.Write(Unknown_x78);
                  ExtTypeOffsetsBlock.Write(bw);
                  bw.Write(Unknown_x86);
                  ExtTypeOverviewsBlock.Write(bw);

                  bw.Write(ExtLineCount);
                  bw.Write(ExtAreaCount);
                  bw.Write(ExtPointCount);

                  if (Headerlength > 0x9a) {    // > 154

                     ReCalculateMapValues();

                     for (int i = 0; i < MapValues.Length; i++)
                        bw.Write(MapValues[i]);
                     bw.Write(MaplevelScrambleKey);      // i.A. 0, da der Verschlüsselungsalgorithmus nicht implementiert ist
                     bw.Write(Unknown_xB6);

                     if (Headerlength > 0xbc) { // > 188

                        bw.Write(Unknown_xC4);

                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// erzeugt die zu speichernde Liste aus den 3 Offset-Listen für die erweiterten Typen
      /// </summary>
      /// <returns></returns>
      List<ExtendedTypeOffsets> BuildExtTypeOffsetList() {
         /*
	private void writeExtTypeOffsetsRecords() {
		header.setExtTypeOffsetsPos(position());
		Subdivision sd = null;
		for (int i = 15; i >= 0; i--) {
			Zoom z = mapLevels[i];
			if (z == null)
				continue;

			Iterator<Subdivision> it = z.subdivIterator();
			while (it.hasNext()) {
				sd = it.next();
				sd.writeExtTypeOffsetsRecord(getWriter());
				header.incExtTypeOffsetsSize();
			}
		}
		if(sd != null) {
			sd.writeLastExtTypeOffsetsRecord(getWriter());
			header.incExtTypeOffsetsSize();
		}
	} 
         */
         List<ExtendedTypeOffsets> ExtTypeOffsetList = new List<ExtendedTypeOffsets>();
         ExtendedTypeOffsets nexttypeoffset = new ExtendedTypeOffsets();
         for (int i = 0; i < SubdivInfoList.Count; i++) {
            DataBlock? bl = new DataBlock();
            ExtendedTypeOffsets typeoffset = new ExtendedTypeOffsets {
               // Daten übernehmen
               LinesOffset = nexttypeoffset.LinesOffset,
               AreasOffset = nexttypeoffset.AreasOffset,
               PointsOffset = nexttypeoffset.PointsOffset
            };

            if (ExtLineBlock4Subdiv.TryGetValue(i, out bl)) {
               typeoffset.Kinds++;
               typeoffset.LinesOffset = bl.Offset;
               nexttypeoffset.LinesOffset = bl.Offset + bl.Length;
            }
            if (ExtAreaBlock4Subdiv.TryGetValue(i, out bl)) {
               typeoffset.Kinds++;
               typeoffset.AreasOffset = bl.Offset;
               nexttypeoffset.AreasOffset = bl.Offset + bl.Length;
            }
            if (ExtPointBlock4Subdiv.TryGetValue(i, out bl)) {
               typeoffset.Kinds++;
               typeoffset.PointsOffset = bl.Offset;
               nexttypeoffset.PointsOffset = bl.Offset + bl.Length;
            }
            ExtTypeOffsetList.Add(typeoffset);
         }
         ExtTypeOffsetList.Add(nexttypeoffset);
         return ExtTypeOffsetList;
      }

      #endregion

      /// <summary>
      /// Offset der Datenabschnitte festlegen und für Header speichern
      /// </summary>
      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.DescriptionBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.MaplevelBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.SubdivisionBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.CopyrightBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.LineOverviewBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.AreaOverviewBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.PointOverviewBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.ExtTypeOverviewsBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.ExtTypeOffsetsBlock, pos++);
         Filesections.SetOffset((int)InternalFileSections.UnknownBlock_xAE, pos++);
         Filesections.SetOffset((int)InternalFileSections.UnknownBlock_xBC, pos++);
         Filesections.SetOffset((int)InternalFileSections.UnknownBlock_xE3, pos++);

         // endgültige Offsets der Datenabschnitte setzen
         Filesections.AdjustSections(GapOffset, DataOffset, (int)InternalFileSections.DescriptionBlock);     // lückenlos ausrichten

         // Offsets für den Header setzen
         MaplevelBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.MaplevelBlock));
         SubdivisionBlock = new DataBlock(Filesections.GetPosition((int)InternalFileSections.SubdivisionBlock));
         CopyrightBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.CopyrightBlock), CopyrightBlock.Recordsize);
         LineOverviewBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.LineOverviewBlock), LineOverviewBlock.Recordsize);
         AreaOverviewBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.AreaOverviewBlock), AreaOverviewBlock.Recordsize);
         PointOverviewBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.PointOverviewBlock), PointOverviewBlock.Recordsize);
         ExtTypeOffsetsBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.ExtTypeOffsetsBlock), ExtTypeOffsetsBlock.Recordsize);
         ExtTypeOverviewsBlock = new DataBlockWithRecordsize(Filesections.GetPosition((int)InternalFileSections.ExtTypeOverviewsBlock), ExtTypeOverviewsBlock.Recordsize);
         UnknownBlock_xAE = new DataBlock(Filesections.GetPosition((int)InternalFileSections.UnknownBlock_xAE));
         UnknownBlock_xBC = new DataBlock(Filesections.GetPosition((int)InternalFileSections.UnknownBlock_xBC));
         UnknownBlock_xE3 = new DataBlock(Filesections.GetPosition((int)InternalFileSections.UnknownBlock_xE3));
      }

      #region Unlock

      /// <summary>
      /// Algorithmus der Maplevel-Entschlüsselung nach Wu Yongzheng
      /// </summary>
      /// <param name="key"></param>
      /// <param name="buff"></param>
      void UnscrambleMaplevel(UInt32 key, byte[] buff) {
         byte[] shuf = {
                     0xb, 0xc, 0xa, 0x0,
                     0x8, 0xf, 0x2, 0x1,
                     0x6, 0x4, 0x9, 0x3,
                     0xd, 0x5, 0x7, 0xe };
         int key_sum = shuf[((key >> 24) + (key >> 16) + (key >> 8) + key) & 0x0F];
         for (int i = 0, ringctr = 16; i < buff.Length; i++) {
            uint upper = (uint)(buff[i] >> 4);
            uint lower = buff[i];

            upper -= (uint)key_sum;
            upper -= key >> ringctr;
            upper -= shuf[(key >> ringctr) & 0x0F];
            ringctr = ringctr > 0 ? ringctr - 4 : 16;

            lower -= (uint)key_sum;
            lower -= key >> ringctr;
            lower -= shuf[(key >> ringctr) & 0x0F];
            ringctr = ringctr > 0 ? ringctr - 4 : 16;

            buff[i] = (byte)((upper << 4) & 0xF0 | (lower & 0x0F));
         }
      }

      /// <summary>
      /// Unlock nach Wu Yongzheng (gimgunlock) für die <see cref="MapLevel"/>-Liste
      /// <para>Wenn das Unlock nicht funktioniert, bleiben die Daten unverändert.</para>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      /// <returns></returns>
      bool UnlockMapLevel(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         block.Offset = 0;
         MaplevelList = br.ReadArray<MapLevel>(block);

         if (Locked != 0 &&
             MaplevelScrambleKey != 0) {

            br.Seek(0);
            byte[] buff = br.ToArray();
            UnscrambleMaplevel(MaplevelScrambleKey, buff);

            BinaryReaderWriter tmp = new BinaryReaderWriter(buff, 0, buff.Length);
            List<MapLevel> test = tmp.ReadArray<MapLevel>(block);

            if (MaplevelsPlausibility(test, true)) {
               MaplevelList = new List<MapLevel>(test);
               return true;
            }

            return false;
         }
         return true;
      }

      /// <summary>
      /// Unlock nach Wu Yongzheng (gimgunlock) bzw. MKGMAP
      /// </summary>
      public void Unlock() {
         if (Locked != 0 &&
             MaplevelScrambleKey != 0) {
            MaplevelScrambleKey = 0;
            Locked = 0;

            // gimgunlock-Algorithmus (XORt das 3. Byte, aber ev. wäre das 2. richtig?)
            MapValues[0] = (MapValues[0] & 0xFF000000) | ((MapValues[0] & 0xFF0000) ^ 0x800000) | (MapValues[0] & 0xFFFF);
            MapValues[2] = MapValues[3];

            // oder besser (?):

            // MKGMAP-Algorithmus
            ReCalculateMapValues();
         }
      }

      #endregion

      #region Berechnung der 4 MapValue-Werte

      /// <summary>
      /// berechnet die 4 <see cref="MapValues"/> neu auf Basis der akt. <see cref="MapID"/> und <see cref="Headerlength"/>
      /// <para>MKGMAP-Algorithmus</para>
      /// </summary>
      public void ReCalculateMapValues() {
         MapValuesCalculator mvc = new MapValuesCalculator(MapID, Headerlength);
         for (int i = 0; i < 4; i++)
            MapValues[i] = mvc.Value(i);
      }

      /// <summary>
      /// aus der Map-ID und der Headerlänge werden die 4 MapValues berechnet
      /// <para>Vgl. MKGMAP: mkgmap/src/uk/me/parabola/imgfmt/app/trergn/MapValues.java</para>
      /// <para>(leicht modifiziert)</para>
      /// </summary>
      class MapValuesCalculator {
         readonly uint mapid;
         readonly uint headerlength;

         List<byte[]> nibble;
         readonly uint[] value;

         /* Die Karte "TOPO Deutschland v3.gmap" enthält im "gelockten" Zustand folgende Werte:
          *    0x493a1ab2
          *    0x886a9aad
          *    0xee62d855
          *    0x88ab9ab1
          * Der Original MKGMAP-Algo liefert für eine "entlockte" Karte:
          *    0x493a9ab2
          *    0x886a9aad
          *    0x88ab9ab1
          *    0x88ab9ab1
          * Das deutet darauf hin, dass die Berechnung des 3. Wertes eigentlich für den 4. Wert bestimmt ist.
          * Der völlig andere 3. Wert dürfte eine andere Bedeutung haben.
          * 
          * Deshalb wurde die Berechnung hier etwas geändert.
          */


         public MapValuesCalculator(uint mapid, uint headerlength) {
            nibble = new List<byte[]>();
            for (int i = 0; i < 4; i++)
               nibble.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            value = new uint[4];
            this.mapid = mapid;
            this.headerlength = headerlength;
            Calculate();
         }

         /// <summary>
         /// liefert einen 32-Bit-Wert
         /// </summary>
         /// <param name="i">0..3</param>
         /// <returns></returns>
         public uint Value(int i) {
            return value[i];
         }

         /// <summary>
         /// berechnet die 4 32-Bit-Werte
         /// </summary>
         void Calculate() {
            calcFourth();
            calcThird();
            calcSecond();
            calcFirst();

            addOffset();

            for (int i = 0; i < 4; i++)
               value[i] = Nibble2UInt(nibble[i]);
         }

         /// <summary>
         /// wandelt die 8 Nibble in eine Zahl um
         /// </summary>
         /// <param name="nibble"></param>
         /// <returns></returns>
         uint Nibble2UInt(byte[] nibble) {
            uint v = 0;
            for (int i = 0; i < nibble.Length; i++) {
               v <<= 4;
               v += (uint)(nibble[i] & 0xF);
            }
            return v;
         }

         readonly byte[] offsettransform = { 0x6, 0x7, 0x5, 0xB, 0x3, 0xA, 0xD, 0xC, 0x1, 0xF, 0x4, 0xE, 0x8, 0x0, 0x2, 0x9 };

         /// <summary>
         /// festes Offset zu jedem Nibble addieren
         /// </summary>
         void addOffset() {
            uint n = mapIdNibble(1) + mapIdNibble(3) + mapIdNibble(5) + mapIdNibble(7);
            byte offset = offsettransform[n & 0xF];

            for (int i = 0; i < nibble.Count; i++)
               for (int j = 0; j < nibble[i].Length; j++)
                  nibble[i][j] += offset;
         }

         void calcFirst() {
            byte[] v = nibble[0];
            byte[] v4 = nibble[3];

            for (int i = 0; i < 4; i++)
               v[i] = (byte)(mapIdNibble(i + 4) + v4[i]);

            for (int i = 4; i < 8; i++)
               v[i] = v4[i];

            v[7] += 1;
         }

         void calcSecond() {
            byte[] v = nibble[1];
            byte[] v4 = nibble[3];

            v[0] = v4[0];
            v[1] = v4[1];

            uint h1 = headerlength >> 4;
            uint h2 = headerlength;

            v[2] = (byte)((v4[2] + h1) & 0xF);
            v[3] = (byte)((v4[3] + h2) & 0xF);

            for (int i = 4; i < 8; i++)
               v[i] = (byte)(mapIdNibble(i - 4) + v4[i]);
         }

         readonly byte[] mapidtransform = { 0x0, 0x1, 0xF, 0x5, 0xD, 0x4, 0x7, 0x6, 0xB, 0x9, 0xE, 0x8, 0x2, 0xA, 0xC, 0x3 };

         void calcThird() {
            for (int i = 0; i < nibble[3].Length; i++)
               nibble[2][i] = nibble[3][i];
         }

         void calcFourth() {
            byte[] v = nibble[3];
            for (int i = 0; i < v.Length; i++)
               v[i ^ 1] = mapidtransform[mapIdNibble(i)];
         }

         /// <summary>
         /// liefert das i-te Nibble (4 Bit); 0 liefert das höchstwertige Nibble, 7 das niederwertigste
         /// </summary>
         /// <param name="i"></param>
         /// <returns></returns>
         uint mapIdNibble(int i) {
            return (mapid >> (4 * (7 - i))) & 0xF;
         }

      }

      #endregion

      /// <summary>
      /// erzeugt aus einem 'Gesamttyp' den Typ und den Subtyp
      /// </summary>
      /// <param name="type"></param>
      /// <param name="subtype"></param>
      /// <returns></returns>
      byte SplitType(int type, out byte subtype) {
         subtype = (byte)(type & 0x1F);
         return (byte)((type >> 8) & 0xFF); ;
      }
      /// <summary>
      /// erzeugt aus Typ und Subtyp den 'Gesamttyp'
      /// </summary>
      /// <param name="type"></param>
      /// <param name="subtype"></param>
      /// <returns></returns>
      int MergeType(byte type, byte subtype) {
         return (type << 8) | subtype;
      }

      #region Verwaltung der Overview-Listen

      /// <summary>
      /// löscht alle internen Overview-Tabellen
      /// </summary>
      public void OverviewsClear() {
         LineOverviewList.Clear();
         AreaOverviewList.Clear();
         PointOverviewList.Clear();
         ExtLineOverviewList.Clear();
         ExtAreaOverviewList.Clear();
         ExtPointOverviewList.Clear();
      }

      /// <summary>
      /// liefert alle Typen als sortierte Listen aus den Overviewdaten
      /// </summary>
      /// <param name="lines"></param>
      /// <param name="polygones"></param>
      /// <param name="points"></param>
      public void GetAllOverviewTypes(out SortedSet<int> lines, out SortedSet<int> polygones, out SortedSet<int> points) {
         lines = new SortedSet<int>();
         foreach (var item in LineOverviewList)
            lines.Add(MergeType(item.Type, 0));
         foreach (var item in ExtLineOverviewList)
            lines.Add(MergeType(item.Type, item.SubType));

         polygones = new SortedSet<int>();
         foreach (var item in AreaOverviewList)
            polygones.Add(MergeType(item.Type, 0));
         foreach (var item in ExtAreaOverviewList)
            polygones.Add(MergeType(item.Type, item.SubType));

         points = new SortedSet<int>();
         foreach (var item in PointOverviewList)
            points.Add(MergeType(item.Type, item.SubType));
         foreach (var item in ExtPointOverviewList)
            points.Add(MergeType(item.Type, item.SubType));
      }

      /// <summary>
      /// Art der Overviewdaten
      /// </summary>
      public enum Overview {
         Point = 0,
         Line,
         Area
      }

      /// <summary>
      /// Anzahl der Einträge in die entsprechende Overviewliste
      /// </summary>
      /// <param name="ovtype"></param>
      /// <param name="exttype">erweiterter Typ oder nicht</param>
      /// <returns></returns>
      public int OverviewCount(Overview ovtype, bool exttype) {
         switch (ovtype) {
            case Overview.Point: return exttype ? ExtPointOverviewList.Count : PointOverviewList.Count;
            case Overview.Line: return exttype ? ExtLineOverviewList.Count : LineOverviewList.Count;
            case Overview.Area: return exttype ? ExtAreaOverviewList.Count : AreaOverviewList.Count;
         }
         return 0;
      }

      /// <summary>
      /// Existiert dieser Typ in der Overviewliste?
      /// </summary>
      /// <param name="ovtype"></param>
      /// <param name="type"></param>
      /// <returns></returns>
      public bool OverviewTypeExist(Overview ovtype, int type) {
         byte typ = SplitType(type, out byte subtype);
         if (type >= 0x10000) {
            switch (ovtype) {
               case Overview.Point:
                  foreach (var item in ExtPointOverviewList)
                     if (item.Type == type && item.SubType == subtype)
                        return true;
                  break;

               case Overview.Line:
                  foreach (var item in ExtLineOverviewList)
                     if (item.Type == type && item.SubType == subtype)
                        return true;
                  break;

               case Overview.Area:
                  foreach (var item in ExtAreaOverviewList)
                     if (item.Type == type && item.SubType == subtype)
                        return true;
                  break;
            }
         } else {
            switch (ovtype) {
               case Overview.Point:
                  foreach (var item in PointOverviewList)
                     if (item.Type == type && item.SubType == subtype)
                        return true;
                  break;

               case Overview.Line:
                  foreach (var item in LineOverviewList)
                     if (item.Type == type)
                        return true;
                  break;

               case Overview.Area:
                  foreach (var item in AreaOverviewList)
                     if (item.Type == type)
                        return true;
                  break;
            }
         }
         return false;
      }

      /// <summary>
      /// fügt einen Overview-Typ hinzu (sortierte Reihenfolge beachten(?))
      /// <para>Ein "erweitertes" Objekt hat den Typ 0x1ttss. Für den Typ stehen 8 Bit zur Verfügung, für den Subtyp 5 Bit (Subtyp 0x00..0x1F).</para>
      /// <para>Für die eigentliche Typangabe stehen bei "normalen" Punkten 7 Bit zur Verfügung, für den Subtyp 5 Bit.
      /// Der erlaubte Wertebereich ist deshalb auf 0x0000..0x7F1F eingeschränkt (Subtyp 0x00..0x1F).</para>
      /// <para>Für die eigentliche Typangabe stehen bei "normalen" Linien nur 6 Bit zur Verfügung und es gibt keinen Subtyp. 
      /// Der erlaubte Wertebereich ist deshalb auf 0x0000..0x3F00 eingeschränkt.</para>
      /// <para>Für die eigentliche Typangabe stehen bei "normalen" Gebieten nur 7 Bit zur Verfügung und es gibt keinen Subtyp. 
      /// Der erlaubte Wertebereich ist deshalb auf 0x0000..0x7F00 eingeschränkt.</para>
      /// </summary>
      /// <param name="ovtype"></param>
      /// <param name="type">Typ (höherwertige 8 Bit) und Subtyp (niederwertige 8 Bit); erweiterter Typ mit größer 0xFFFF</param>
      /// <param name="maxlevel"></param>
      public void OverviewAdd(Overview ovtype, int type, byte maxlevel) {
         if (!OverviewTypeExist(ovtype, type)) {
            byte typ = SplitType(type, out byte subtype);
            switch (ovtype) {
               case Overview.Point:
                  if (((type & 0xFF) & ~0x1F) != 0)
                     throw new Exception("Der Subtyp 0x" + (type & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00..0x1F).");

                  if (type >= 0x10000) // erweiterter Typ
                     ExtPointOverviewList.Add(new OverviewObject4Byte((byte)((type >> 8) & 0xFF), (byte)(type & 0xFF), maxlevel));
                  else {
                     if ((type & 0x8000) != 0)
                        throw new Exception("Der Typ 0x" + ((type >> 8) & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00..0x7F).");
                     PointOverviewList.Add(new OverviewObject3Byte(typ, subtype, maxlevel)); // 3-Byte-Typ
                  }
                  break;

               case Overview.Line:
                  if (type >= 0x10000) {
                     if (((type & 0xFF) & ~0x1F) != 0)
                        throw new Exception("Der Subtyp 0x" + (type & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00..0x1F).");
                     ExtLineOverviewList.Add(new OverviewObject4Byte(typ, subtype, maxlevel, 0));
                  } else {
                     if ((type & 0xC000) != 0)
                        throw new Exception("Der Typ 0x" + ((type >> 8) & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00..0x3F).");
                     if ((type & 0xFF) != 0)
                        throw new Exception("Ein Subtyp 0x" + (type & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00).");
                     LineOverviewList.Add(new OverviewObject2Byte(typ, maxlevel)); // 2-Byte-Typ
                  }
                  break;

               case Overview.Area:
                  if (type >= 0x10000) {
                     if (((type & 0xFF) & ~0x1F) != 0)
                        throw new Exception("Der Subtyp 0x" + (type & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00..0x1F).");
                     ExtAreaOverviewList.Add(new OverviewObject4Byte(typ, subtype, maxlevel, 0));
                  } else {
                     if ((type & 0x8000) != 0)
                        throw new Exception("Der Typ 0x" + ((type >> 8) & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00..0x7F).");
                     if ((type & 0xFF) != 0)
                        throw new Exception("Ein Subtyp 0x" + (type & 0xFF).ToString("X") + " ist nicht erlaubt (nur 0x00).");
                     AreaOverviewList.Add(new OverviewObject2Byte(typ, maxlevel)); // 2-Byte-Typ
                  }
                  break;
            }
         }
      }

      /// <summary>
      /// liefert die Daten eines Overvieweintrages
      /// </summary>
      /// <param name="ovtype"></param>
      /// <param name="exttype">erweiterter Typ oder nicht</param>
      /// <param name="idx">Index (kleiner als <see cref="OverviewCount"/>)</param>
      /// <param name="type">Typ; bei Fehler kleiner 0</param>
      /// <param name="subtype">Subtyp; bei Fehler oder Nichtexistenz kleiner 0</param>
      /// <param name="unknown">unbekanntes Byte; bei Fehler oder Nichtexistenz kleiner 0</param>
      /// <returns>max. Level; bei Fehler kleiner 0</returns>
      public int GetOverviewData(Overview ovtype, bool exttype, int idx, out int type, out int subtype, out int unknown) {
         type = -1;
         subtype = -1;
         unknown = -1;
         if (idx >= 0)
            switch (ovtype) {
               case Overview.Point:
                  if (exttype) {
                     if (idx < ExtPointOverviewList.Count) {
                        type = ExtPointOverviewList[idx].Type;
                        subtype = ExtPointOverviewList[idx].SubType;
                        unknown = ExtPointOverviewList[idx].Unknown;
                        return ExtPointOverviewList[idx].MaxLevel;
                     }
                  } else {
                     if (idx < PointOverviewList.Count) {
                        type = PointOverviewList[idx].Type;
                        subtype = PointOverviewList[idx].SubType;
                        return PointOverviewList[idx].MaxLevel;
                     }
                  }
                  break;

               case Overview.Line:
                  if (exttype) {
                     if (idx < ExtLineOverviewList.Count) {
                        type = ExtLineOverviewList[idx].Type;
                        subtype = ExtLineOverviewList[idx].SubType;
                        unknown = ExtLineOverviewList[idx].Unknown;
                        return ExtLineOverviewList[idx].MaxLevel;
                     }
                  } else {
                     if (idx < LineOverviewList.Count) {
                        type = LineOverviewList[idx].Type;
                        return LineOverviewList[idx].MaxLevel;
                     }
                  }
                  break;

               case Overview.Area:
                  if (exttype) {
                     if (idx < ExtAreaOverviewList.Count) {
                        type = ExtAreaOverviewList[idx].Type;
                        subtype = ExtAreaOverviewList[idx].SubType;
                        unknown = ExtAreaOverviewList[idx].Unknown;
                        return ExtAreaOverviewList[idx].MaxLevel;
                     }
                  } else {
                     if (idx < AreaOverviewList.Count) {
                        type = AreaOverviewList[idx].Type;
                        return AreaOverviewList[idx].MaxLevel;
                     }
                  }
                  break;
            }

         return -1;
      }

      #endregion


      public override string ToString() {
         return string.Format("{0}; {1}°..{2}°, {3}°..{4}°", base.ToString(), West.ValueDegree, East.ValueDegree, South.ValueDegree, North.ValueDegree);
      }

   }

}
