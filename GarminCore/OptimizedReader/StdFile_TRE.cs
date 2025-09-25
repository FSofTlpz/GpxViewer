using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GarminCore.OptimizedReader {

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
            get => (byte)(_SymbolicScaleDenominator & 0x0F);    // Bits 0..3
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
            get => Bit.IsSet(_SymbolicScaleDenominator, 4);
            protected set => _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 4, value);
         }

         /// <summary>
         /// Bit 5 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Bit5 {
            get => Bit.IsSet(_SymbolicScaleDenominator, 5);
            protected set => _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 5, value);
         }

         /// <summary>
         /// Bit 6 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Bit6 {
            get => Bit.IsSet(_SymbolicScaleDenominator, 6);
            protected set => _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 6, value);
         }

         /// <summary>
         /// Bit 7 des <see cref="_SymbolicScaleDenominator"/>
         /// </summary>
         public bool Inherited {
            get => Bit.IsSet(_SymbolicScaleDenominator, 7);
            set => _SymbolicScaleDenominator = (byte)Bit.Set(_SymbolicScaleDenominator, 7, value);
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

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

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

         public void Clear() {
            ml.Clear();
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
         /// liefert die Bitanzahl je Koordinate der Ebene
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         public int Bits(int level) {
            LevelCheck(level);
            return ml[level].CoordBits;
         }

         void LevelCheck(int level) {
            if (level < 0 || level >= Count)
               throw new ArgumentException("Die Ebene liegt außerhalb des gültigen Bereichs 0 .. " + (Count - 1).ToString());
         }

         /// <summary>
         /// liefert den 1-basierten Index des 1. untergeordneten Subdivs der Ebene
         /// </summary>
         /// <param name="level"></param>
         /// <returns></returns>
         int FirstSubdivChildIdx(int level) {
            LevelCheck(level);
            return ml[level].FirstSubdivInfoNumber;
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
         public int Bits4SubdivIdx1(int idx) => Bits(Level4SubdivIdx1(idx));

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
            get => (UInt16)(_HalfWidth & 0x7FFF);
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

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

         public Bound GetBound(int coordbits) {
            int w2 = Coord.RawUnits2MapUnits(HalfWidth, coordbits);
            int h2 = Coord.RawUnits2MapUnits(HalfHeight, coordbits);
            return new Bound(new MapUnitPoint(Center.X - w2, Center.Y - h2),
                             new MapUnitPoint(Center.X + w2, Center.Y + h2));
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

         public override void Write(BinaryReaderWriter bw, object? extdata) { }

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

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

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

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

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

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

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

         public override void Write(BinaryReaderWriter bw, object? extdata = null) { }

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
      public Latitude North { get; protected set; }

      /// <summary>
      /// östliche Kartengrenze in 360/(2^24) Grad (0x18)
      /// </summary>
      public Longitude East { get; protected set; }

      /// <summary>
      /// südliche Kartengrenze in 360/(2^24) Grad (0x1B)
      /// </summary>
      public Latitude South { get; protected set; }

      /// <summary>
      /// westliche Kartengrenze in 360/(2^24) Grad (0x1E)
      /// </summary>
      public Longitude West { get; protected set; }

      /// <summary>
      /// Datenbereich für die Tabelle der Maplevel (0x21)
      /// </summary>
      DataBlock MaplevelBlock;

      /// <summary>
      /// Datenbereich für die Tabelle der Subdivisions (0x29)
      /// </summary>
      DataBlock SubdivisionBlock;

      /// <summary>
      /// Datenbereich für die Tabelle der Copyright-Texte (0x31)
      /// </summary>
      DataBlockWithRecordsize CopyrightBlock;

      byte[] Unknown_x3B = { 0, 0, 0, 0 };

      /// <summary>
      /// Optionen für die POI-Anzeige (0x3F)
      /// </summary>
      public byte POIDisplayFlags { get; private set; }

      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_TransparentMap {
         get => Bit.IsSet(POIDisplayFlags, 1);
         protected set => POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 1, value);
      }

      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_ShowStreetBeforeNumber {
         get => Bit.IsSet(POIDisplayFlags, 2);
         protected set => POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 2, value);
      }

      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_ShowZipBeforeCity {
         get => Bit.IsSet(POIDisplayFlags, 3);
         protected set => POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 3, value);
      }

      /// <summary>
      /// liefert oder setzt den Wert aus <see cref="POIDisplayFlags"/> entsprechend
      /// </summary>
      public bool POIDisplay_DriveLeft {
         get => Bit.IsSet(POIDisplayFlags, 5);
         protected set => POIDisplayFlags = (byte)Bit.Set(POIDisplayFlags, 5, value);
      }

      /// <summary>
      /// Kartenlayer ? (0x40)
      /// </summary>
      public int DisplayPriority { get; protected set; }

      byte[] Unknown_x43 = { 0x01, 0x03, 0x11, 0x00, 0x01, 0x00, 0x00 };
      // public byte[] Unknown_x43 = { 0x01, 0x04, 0x17, 0x00, 0x01, 0x00, 0x00 };      // Topo D V3

      /// <summary>
      /// Datenbereich für die Tabelle der Polylines (0x4A)
      /// </summary>
      DataBlockWithRecordsize LineOverviewBlock;

      byte[] Unknown_x54 = { 0, 0, 0, 0 };

      /// <summary>
      /// Datenbereich für die Tabelle der Polygone (0x58)
      /// </summary>
      DataBlockWithRecordsize AreaOverviewBlock;

      byte[] Unknown_x62 = { 0, 0, 0, 0 };

      /// <summary>
      /// Datenbereich für die Tabelle der Punkte (0x66)
      /// </summary>
      DataBlockWithRecordsize PointOverviewBlock;

      byte[] Unknown_x70 = { 0, 0, 0, 0 };

      // --------- Headerlänge > 116 Byte (zusätzlich Map-ID)

      /// <summary>
      /// Karten-ID (0x74)
      /// </summary>
      public UInt32 MapID { get; protected set; }

      // --------- Headerlänge > 120 Byte (zusätzlich erweiterte Typen)

      byte[] Unknown_x78 = { 0, 0, 0, 0 };

      /// <summary>
      /// Offsets auf die Datenbereiche der erweiterten Typen in der RGN Datei
      /// </summary>
      DataBlockWithRecordsize ExtTypeOffsetsBlock;

      byte[] Unknown_x86 = { 0x07, 0x06, 0x00, 0x00 };

      /// <summary>
      /// Verweis auf die Tabelle der erweiterten Typen (0x8A)
      /// </summary>
      DataBlockWithRecordsize ExtTypeOverviewsBlock;

      /// <summary>
      /// Anzahl der erweiterten Polylinientypen (0x94)
      /// </summary>
      UInt16 ExtLineCount;

      /// <summary>
      /// Anzahl der erweiterten Polygontypen (0x96)
      /// </summary>
      UInt16 ExtAreaCount;

      /// <summary>
      /// Anzahl der erweiterten Punkttypen (0x98)
      /// </summary>
      UInt16 ExtPointCount;

      // --------- Headerlänge > 154 Byte ("Verschlüsselung" der Maplevel-Tabelle)

      /// <summary>
      /// (0x9A)
      /// </summary>
      UInt32[] MapValues;

      /// <summary>
      /// Verschlüsselungs-Key für die Mapleveltabelle
      /// </summary>
      UInt32 MaplevelScrambleKey;

      DataBlock? UnknownBlock_xAE;

      byte[] Unknown_xB6 = new byte[0x6];

      // --------- Headerlänge > 188 Byte

      DataBlock? UnknownBlock_xBC;

      byte[] Unknown_xC4 = new byte[0x1F];

      DataBlock? UnknownBlock_xE3;

      byte[]? Unknown_xEB = null;

      #endregion


      /// <summary>
      /// zur Verwaltung der MapLevel
      /// </summary>
      public SymbolicScaleDenominatorAndBits? SymbolicScaleDenominatorAndBitsLevel { get; protected set; }

      /// <summary>
      /// liefert den Datenbereich für die Kartenbeschreibung
      /// </summary>
      /// <returns></returns>
      DataBlock? MapDescriptionBlock;

      /// <summary>
      /// Liste der Texte für die Kartenbeschreibung
      /// </summary>
      public List<string> MapDescriptionList { get; protected set; }

      /// <summary>
      /// Liste der Copyright-Offsets
      /// </summary>
      public List<uint> CopyrightOffsetsList { get; protected set; }

      /// <summary>
      /// Liste der Maplevel (Index 0 mit den geringsten Details, d.h. kleinster Maßstab)
      /// </summary>
      public List<MapLevel> MaplevelList { get; protected set; }

      /// <summary>
      /// Liste ALLER Subdivinfos (wird sequentiell in der TRE-Datei gespeichert; Verweise sind 1-basiert; max. ushort-Elemente)
      /// </summary>
      public List<SubdivInfoBasic> SubdivInfoList { get; protected set; }


      /// <summary>
      /// Offsets in der RGN-Datei für die Linien für jede Subdiv (Subdivnummer und Datenblock)
      /// </summary>
      public SortedDictionary<int, DataBlock> ExtLineBlock4Subdiv { get; protected set; }

      /// <summary>
      /// Offsets in der RGN-Datei für die Flächen für jede Subdiv (Subdivnummer und Datenblock)
      /// </summary>
      public SortedDictionary<int, DataBlock> ExtAreaBlock4Subdiv { get; protected set; }

      /// <summary>
      /// Offsets in der RGN-Datei für die Punkte für jede Subdiv (Subdivnummer und Datenblock)
      /// </summary>
      public SortedDictionary<int, DataBlock> ExtPointBlock4Subdiv { get; protected set; }


      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene Polylinientypen (.. 0xff)
      /// </summary>
      public List<OverviewObject2Byte> LineOverviewList { get; protected set; }

      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene Polygontypen (.. 0xff)
      /// </summary>
      public List<OverviewObject2Byte> AreaOverviewList { get; protected set; }

      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene Punkttypen (.. 0xff)
      /// </summary>
      public List<OverviewObject3Byte> PointOverviewList { get; protected set; }


      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene erweiterten Polylinientypen (0x100 .. mit Subtyp)
      /// </summary>
      public List<OverviewObject4Byte> ExtLineOverviewList { get; protected set; }

      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene erweiterten Polygontypen (0x100 .. mit Subtyp)
      /// </summary>
      public List<OverviewObject4Byte> ExtAreaOverviewList { get; protected set; }

      /// <summary>
      /// Tabelle der in den zugehörigen Daten enthaltene erweiterten Punkttypen (0x100 .. mit Subtyp)
      /// </summary>
      public List<OverviewObject4Byte> ExtPointOverviewList { get; protected set; }


      public StdFile_TRE()
         : base("TRE") {
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

      public override void ReadHeader(BinaryReaderWriter reader) {
         readCommonHeader(reader, Type);

         North = reader.Read3Int();
         East = reader.Read3Int();
         South = reader.Read3Int();
         West = reader.Read3Int();

         MaplevelBlock = new DataBlock(reader);
         SubdivisionBlock = new DataBlock(reader);
         CopyrightBlock = new DataBlockWithRecordsize(reader);

         reader.ReadBytes(Unknown_x3B);
         POIDisplayFlags = reader.ReadByte();
         DisplayPriority = reader.Read3Int();
         reader.ReadBytes(Unknown_x43);

         LineOverviewBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_x54);
         AreaOverviewBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_x62);
         PointOverviewBlock = new DataBlockWithRecordsize(reader);
         reader.ReadBytes(Unknown_x70);

         if (Headerlength > 0x74) {          // > 116
            MapID = reader.Read4UInt();

            if (Headerlength > 0x78) {       // > 120
               reader.ReadBytes(Unknown_x78);
               ExtTypeOffsetsBlock = new DataBlockWithRecordsize(reader);
               reader.ReadBytes(Unknown_x86);
               ExtTypeOverviewsBlock = new DataBlockWithRecordsize(reader);
               ExtLineCount = reader.Read2AsUShort();
               ExtAreaCount = reader.Read2AsUShort();
               ExtPointCount = reader.Read2AsUShort();

               if (Headerlength > 0x9a) {    // > 154

                  for (int i = 0; i < MapValues.Length; i++)
                     MapValues[i] = reader.Read4UInt();
                  MaplevelScrambleKey = reader.Read4UInt();

                  UnknownBlock_xAE = new DataBlock(reader);
                  reader.ReadBytes(Unknown_xB6);

                  if (Headerlength > 0xbc) { // > 188
                     UnknownBlock_xBC = new DataBlock(reader);
                     reader.ReadBytes(Unknown_xC4);
                     if (Headerlength > 0xfc) {
                        UnknownBlock_xE3 = new DataBlock(reader);
                        Unknown_xEB = new byte[Headerlength - 0xEB];
                        reader.ReadBytes(Unknown_xEB);
                     }
                  }
               }
            }

         }
      }

      public override void ReadMinimalSections(BinaryReaderWriter reader) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         // Wenn jetzt eine Lücke zum 1. Datenbereich oder zum nächsten Dateiheader (bei GMP) besteht, sollte eine Beschreibung folgen, die aus mehreren
         // 0-terminierten Zeichenketten besteht. Es existiert KEINE Endekennung dieses Bereiches.

         // Beschreibung einlesen
         if (GapOffset > HeaderOffset + Headerlength) { // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            MapDescriptionBlock = new DataBlock(HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));
            Decode_DescriptionBlock(reader, MapDescriptionBlock);
         }

         // Copyright-Offsets einlesen
         Decode_CopyrightBlock(reader, CopyrightBlock);

         // alle Maplevel einlesen
         if (!Decode_MapLevelBlock(reader, new DataBlockWithRecordsize(MaplevelBlock, 0)))
            return;

         // alle Subdivs einlesen
         Decode_SubdivisionBlock(reader, SubdivisionBlock, MaplevelList);

         // Overview-Objekte einlesen
         Decode_LineOverviewBlock(reader, LineOverviewBlock);
         Decode_AreaOverviewBlock(reader, AreaOverviewBlock);
         Decode_PointOverviewBlock(reader, PointOverviewBlock);

         // erweiterte Typen einlesen
         Decode_ExtTypeOverviewsBlock(reader, ExtTypeOverviewsBlock, ExtLineCount, ExtAreaCount, ExtPointCount);

         // Offsets für die erweiterten Typen einlesen und umwandeln
         SplitExtTypeOffsetList(Decode_ExtTypeOffsetsBlock(reader, ExtTypeOffsetsBlock));
      }

      /// <summary>
      /// liefert eine Indexliste der betroffenen Subdivs
      /// </summary>
      /// <param name="firstidx"></param>
      /// <param name="count"></param>
      /// <param name="bound"></param>
      /// <param name="coordbits"></param>
      /// <returns></returns>
      public List<int> GetSubdivIdxList(int firstidx, int count, Bound bound, int coordbits) {
         List<int> idxlst = new List<int>();
         for (int i = firstidx; i < firstidx + count; i++) {
            if (SubdivInfoList[i].GetBound(coordbits).IsOverlapped(bound))
               idxlst.Add(i);
         }
         return idxlst;
      }

      /// <summary>
      /// liefert das Subdiv-Array zur Indexliste
      /// </summary>
      /// <param name="idxlst"></param>
      /// <returns></returns>
      public SubdivInfoBasic[] GetSubdivs(IList<int> idxlst) {
         SubdivInfoBasic[] sd = new SubdivInfoBasic[idxlst.Count];
         for (int i = 0; i < idxlst.Count; i++)
            sd[i] = SubdivInfoList[idxlst[i]];
         return sd;
      }

      #region Decodierung der Datenblöcke

      /// <summary>
      /// liest die Kartenbeschreibung aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_DescriptionBlock(BinaryReaderWriter br, DataBlock block) {
         MapDescriptionList.Clear();
         if (br != null &&
             block != null &&
             block.Length > 0) {
            br.Seek(block.Offset);
            while (br.Position < block.Offset + block.Length)
               MapDescriptionList.Add(br.ReadString());
         }
      }

      /// <summary>
      /// liest die Copyright-Offset-Daten aus dem <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block"></param>
      void Decode_CopyrightBlock(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         CopyrightOffsetsList.Clear();
         if (br != null &&
             block != null &&
             block.Length > 0)
            CopyrightOffsetsList = br.ReadUintArray(block);
      }

      /// <summary>
      /// liest die MapLevel-Daten aus dem <see cref="BinaryReaderWriter"/> und versucht, sie bei Bedarf zu entschlüsseln
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block">Datenbereich aus dem gelesen wird</param>
      /// <returns>false, wenn nicht vorhanden oder nicht entschlüsselbar</returns>
      bool Decode_MapLevelBlock(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         if (br != null &&
             block != null &&
             block.Length > 0) {
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
      bool Decode_SubdivisionBlock(BinaryReaderWriter br, DataBlock block, List<MapLevel> maplevelList) {
         SubdivInfoList.Clear();
         if (br != null &&
             block != null &&
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
      void Decode_PointOverviewBlock(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         PointOverviewList.Clear();
         if (br != null &&
             block != null &&
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
      void Decode_AreaOverviewBlock(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         AreaOverviewList.Clear();
         if (br != null &&
             block != null &&
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
      void Decode_LineOverviewBlock(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         LineOverviewList.Clear();
         if (br != null &&
             block != null &&
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
      void Decode_ExtTypeOverviewsBlock(BinaryReaderWriter br, DataBlockWithRecordsize block, ushort extPolylineCount, ushort extPolygoneCount, ushort extPointCount) {
         ExtLineOverviewList.Clear();
         ExtAreaOverviewList.Clear();
         ExtPointOverviewList.Clear();

         if (br != null &&
             block != null &&
             block.Length > 0) {
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
      List<ExtendedTypeOffsets>? Decode_ExtTypeOffsetsBlock(BinaryReaderWriter br, DataBlockWithRecordsize block) {
         if (br != null &&
             block != null &&
             block.Length > 0)
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
            SubdivInfo? sdi2 = sdi[lastidx] as SubdivInfo;
            if (sdi2 != null &&
                !sdi2.LastSubdiv)
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


      public override string ToString() {
         return string.Format("{0}; {1}°..{2}°, {3}°..{4}°", base.ToString(), West.ValueDegree, East.ValueDegree, South.ValueDegree, North.ValueDegree);
      }

   }

}
