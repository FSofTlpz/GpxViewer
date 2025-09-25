using System;
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
using System.Collections.Generic;

namespace GarminCore.Files {

   public class FileSections {

      /// <summary>
      /// Block von "Rohdaten"
      /// </summary>
      protected class Section : IComparable {

         //protected class Section : Comparer<Section> {

         /// <summary>
         /// Offset und Länge
         /// </summary>
         public DataBlockWithRecordsize Position;

         /// <summary>
         /// Daten
         /// </summary>
         public byte[]? Data;

         /// <summary>
         /// beliebig verwendbare zusätzliche Infodaten für das Objekt
         /// </summary>
         public object? ExtData;


         /// <summary>
         /// Eine neue <see cref="Section"/> ohne Daten wird erzeugt.
         /// </summary>
         public Section() {
            Position = new DataBlockWithRecordsize();
            Data = null;
            ExtData = null;
         }

         /// <summary>
         /// Eine neue <see cref="Section"/> wird erzeugt und ab der akt. Position im Stream werden (max.) die gewünschten Anzahl Bytes 
         /// eingelesen und die vorgegeben Positionsangaben gespeichert.
         /// </summary>
         public Section(BinaryReaderWriter br, uint newoffset, uint length)
            : this() {
            Read(br, newoffset, length);
         }

         /// <summary>
         /// Eine neue <see cref="Section"/> wird erzeugt und ab der akt. Position im Stream werden (max.) die gewünschten Anzahl Bytes 
         /// eingelesen und die vorgegeben Positionsangaben gespeichert.
         /// </summary>
         /// <param name="br"></param>
         /// <param name="block"></param>
         public Section(BinaryReaderWriter br, DataBlock block) :
            this() {
            Read(br, block.Offset, block.Length);
            if (block is DataBlockWithRecordsize)
               Position.Recordsize = ((DataBlockWithRecordsize)block).Recordsize;
         }

         /// <summary>
         /// Ab der akt. Position im Stream werden (max.) die gewünschten Anzahl Bytes eingelesen und die vorgegeben Positionsangaben gespeichert.
         /// </summary>
         /// <param name="br"></param>
         /// <param name="newoffset"></param>
         /// <param name="length"></param>
         /// <returns>Anzahl der tatsächlich gelesenen Bytes</returns>
         public uint Read(BinaryReaderWriter br, uint newoffset, uint length) {
            Position.Offset = newoffset;
            if (length > 0)
               Data = br.ReadBytes((int)length);
            else
               Data = new byte[0];
            Position.Length = (uint)Data.Length;
            return Position.Length;
         }

         /// <summary>
         /// Ab dem vorgegeben Offset des Streams werden (max.) die gewünschten Anzahl Bytes eingelesen und die vorgegeben Positionsangaben gespeichert.
         /// </summary>
         /// <param name="br"></param>
         /// <param name="streamoffset"></param>
         /// <param name="newoffset"></param>
         /// <param name="length"></param>
         /// <returns>Anzahl der tatsächlich gelesenen Bytes</returns>
         public uint Read(BinaryReaderWriter br, uint streamoffset, uint newoffset, uint length) {
            br.Seek(streamoffset);
            return Read(br, newoffset, length);
         }

         /// <summary>
         /// Aus dem Stream werden entsprechend der gespeicherten Position die Daten eingelesen.
         /// </summary>
         /// <param name="br"></param>
         /// <returns>Anzahl der tatsächlich gelesenen Bytes</returns>
         public uint Read(BinaryReaderWriter br) {
            return Read(br, Position.Offset, Position.Offset, Position.Length);
         }

         /// <summary>
         /// In den Stream werden entsprechend der gespeicherten Position die Daten geschrieben.
         /// </summary>
         /// <param name="bw"></param>
         public void Write(BinaryReaderWriter bw) {
            if (Data != null &&
                Data.Length > 0) {
               bw.Seek(Position.Offset);
               bw.Write(Data);
            }
         }

         /// <summary>
         /// In den Stream werden entsprechend des angegeben Offsets die Daten geschrieben.
         /// </summary>
         /// <param name="bw"></param>
         /// <param name="streamoffset"></param>
         public void Write(BinaryReaderWriter bw, uint streamoffset) {
            uint offs = Position.Offset;
            Position.Offset = streamoffset;
            Write(bw);
            Position.Offset = offs;
         }

         public int CompareTo(object? obj) {
            if (obj == null || !(obj is Section))
               return 1;
            Section sec = (Section)obj;
            if (Position.Offset > sec.Position.Offset)
               return 1;
            if (Position.Offset < sec.Position.Offset)
               return -1;
            return 0;
         }

         /// <summary>
         /// Vergleich für die Sortierung nach Offsets
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         //public override int Compare(Section x, Section y) {
         //   if (x.Position.Offset > y.Position.Offset)
         //      return 1;
         //   if (x.Position.Offset < y.Position.Offset)
         //      return -1;
         //   return 0;
         //}

         public override string ToString() {
            return Position.ToString();
         }

      }


      /// <summary>
      /// Liste der Datenblöcke mit jeweils einem eindeutigen Key
      /// </summary>
      SortedList<int, Section> sections;

      /// <summary>
      /// Anzahl der Abschnitte
      /// </summary>
      public int Count {
         get {
            return sections.Count;
         }
      }


      public FileSections() {
         sections = new SortedList<int, Section>();
      }

      /// <summary>
      /// liefert true, wenn der Typ schon existiert
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public bool ContainsType(int type) {
         return sections.ContainsKey(type);
      }

      /// <summary>
      /// fügt einen Abschnitt mit dem angegebenen Typ ein
      /// </summary>
      /// <param name="type"></param>
      /// <returns>false, wenn der Typ schon existiert</returns>
      public bool AddSection(int type) {
         if (!ContainsType(type)) {
            sections.Add(type, new Section());
            return true;
         }
         return false;
      }

      /// <summary>
      /// fügt einen Abschnitt mit dem angegebenen Typ und den Positionsdaten ein
      /// </summary>
      /// <param name="type"></param>
      /// <param name="offset"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public bool AddSection(int type, uint offset, uint length, ushort recordsize = 0) {
         if (AddSection(type)) {
            Section sec = sections[type];
            sec.Position.Offset = offset;
            sec.Position.Length = length;
            sec.Position.Recordsize = recordsize;
            return true;
         }
         return false;
      }

      /// <summary>
      /// fügt einen Abschnitt mit dem angegebenen Typ und den Positionsdaten ein
      /// </summary>
      /// <param name="type"></param>
      /// <param name="block"></param>
      /// <returns></returns>
      public bool AddSection(int type, DataBlock block) {
         return block is DataBlockWithRecordsize ?
                           AddSection(type, block.Offset, block.Length, ((DataBlockWithRecordsize)block).Recordsize) :
                           AddSection(type, block.Offset, block.Length);
      }

      /// <summary>
      /// entfernt den Abschnitt mit dem angegebenen Typ
      /// </summary>
      /// <param name="type"></param>
      /// <returns>false, wenn der Typ nicht existiert</returns>
      public bool RemoveSection(int type) {
         if (ContainsType(type)) {
            sections.Remove(type);
            return true;
         }
         return false;
      }

      /// <summary>
      /// entfernt die Abschnitte mit dem angegebenen Typ
      /// </summary>
      /// <param name="type"></param>
      /// <returns>Anzahl der entfernten Abschnitte</returns>
      public int RemoveSections(int[] type) {
         int count = 0;
         if (type != null)
            foreach (int item in type)
               if (RemoveSection(item))
                  count++;
         return count;
      }

      /// <summary>
      /// entfernt alle Abschnitte außer den angegebenen
      /// </summary>
      /// <param name="type"></param>
      /// <returns>Anzahl der entfernten Abschnitte</returns>
      public int RemoveAllSectionsExcept(int[] type) {
         List<int> alltypes = new List<int>(GetTypes());
         if (type != null)
            foreach (int item in type)
               if (alltypes.Contains(item))
                  alltypes.Remove(item);
         return RemoveSections(alltypes.ToArray());
      }

      /// <summary>
      /// alle Abschnitte werden entfernt
      /// </summary>
      public void ClearSections() {
         sections.Clear();
      }

      /// <summary>
      /// liest den Abschnitt mit dem Typ ab der angegebenen (!) Position im <see cref="BinaryReaderWriter"/> ein und speichert die angegebenen Positionsdaten
      /// </summary>
      /// <param name="type"></param>
      /// <param name="br"></param>
      /// <param name="streamoffset"></param>
      /// <param name="newoffset"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public uint Read(int type, BinaryReaderWriter br, uint streamoffset, uint newoffset, uint length) {
         if (ContainsType(type))
            return sections[type].Read(br, streamoffset, newoffset, length);
         return 0;
      }

      /// <summary>
      /// liest den Abschnitt mit dem Typ ab der akt. (!) Position im <see cref="BinaryReaderWriter"/> ein und speichert die angegebenen Positionsdaten
      /// </summary>
      /// <param name="type"></param>
      /// <param name="br"></param>
      /// <param name="newoffset"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public uint Read(int type, BinaryReaderWriter br, uint newoffset, uint length) {
         if (ContainsType(type))
            return sections[type].Read(br, newoffset, length);
         return 0;
      }

      /// <summary>
      /// liest den Abschnitt mit dem Typ entsprechend der registrierten Position aus dem <see cref="BinaryReaderWriter"/> ein
      /// </summary>
      /// <param name="type"></param>
      /// <param name="br"></param>
      /// <returns></returns>
      public uint Read(int type, BinaryReaderWriter br) {
         if (ContainsType(type) && br != null)
            return sections[type].Read(br);
         return 0;
      }

      /// <summary>
      /// liest alle Abschnitte entsprechend der registrierten Positionen aus dem <see cref="BinaryReaderWriter"/> ein
      /// </summary>
      /// <param name="br"></param>
      public void ReadSections(BinaryReaderWriter br) {
         foreach (Section sec in SortedList4Offset())
            if (sec.Position.Length > 0)
               sec.Read(br);
      }

      /// <summary>
      /// schreibt den Abschnitt mit dem Typ am angegebenen Offset in den <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="type"></param>
      /// <param name="bw"></param>
      /// <param name="streamoffset"></param>
      /// <returns>false, wenn der Typ nicht existiert</returns>
      public bool Write(int type, BinaryReaderWriter bw, uint streamoffset) {
         if (ContainsType(type)) {
            writeSection(sections[type], streamoffset, bw);
            return true;
         }
         return false;
      }

      /// <summary>
      /// schreibt den Abschnitt mit dem Typ an der registrierten Stelle in den <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="type"></param>
      /// <param name="bw"></param>
      /// <returns>false, wenn der Typ nicht existiert</returns>
      public bool Write(int type, BinaryReaderWriter bw) {
         if (ContainsType(type)) {
            writeSection(sections[type], bw);
            return true;
         }
         return false;
      }

      /// <summary>
      /// schreibt alle Abschnitte entsprechend ihres Offsets in den <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="datablocks"></param>
      public void WriteSections(BinaryReaderWriter bw) {
         if (sections.Count > 0)
            foreach (Section sec in SortedList4Offset())
               writeSection(sec, bw);
      }

      /// <summary>
      /// schreibt eine <see cref="Section"/> entsprechend ihrer Position in den <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="sec"></param>
      /// <param name="bw"></param>
      void writeSection(Section sec, BinaryReaderWriter bw) {
         if (sec != null &&
             (sec.Data != null ? sec.Data.Length : 0) > 0)
            sec.Write(bw);
      }

      /// <summary>
      /// schreibt eine <see cref="Section"/> entsprechend des angegebenen Offsets in den <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="sec"></param>
      /// <param name="streamoffset"></param>
      /// <param name="bw"></param>
      void writeSection(Section sec, uint streamoffset, BinaryReaderWriter bw) {
         if (sec != null &&
             (sec.Data != null ? sec.Data.Length : 0) > 0)
            sec.Write(bw, streamoffset);
      }

      /// <summary>
      /// liefert einen <see cref="BinaryReaderWriter"/> um die Abschnittsdaten zu lesen
      /// </summary>
      /// <param name="type"></param>
      /// <returns>null, wenn keine Daten vorhanden sind</returns>
      public BinaryReaderWriter? GetSectionDataReader(int type) {
         if (!ContainsType(type))
            return null;
         Section sec = sections[type];
         if (sec.Data == null)
            return null;
         return new BinaryReaderWriter(sec.Data, 0, sec.Data.Length);
      }

      /// <summary>
      /// setzt die Abschnittsdaten mit den Daten des <see cref="BinaryReaderWriter"/> ab der aktuellen (!) Position des <see cref="BinaryReaderWriter"/>
      /// </summary>
      /// <param name="type"></param>
      /// <param name="bw"></param>
      /// <returns></returns>
      public bool SetSectionDataFromWriter(int type, BinaryReaderWriter bw) {
         if (!ContainsType(type) || bw == null)
            return false;
         Section sec = sections[type];
         sec.Data = bw.ToArray();
         sec.Position.Length = (uint)sec.Data.Length;
         return true;
      }

      /// <summary>
      /// setzt die Abschnittsdaten mit den Array-Daten
      /// </summary>
      /// <param name="type"></param>
      /// <param name="data"></param>
      /// <returns></returns>
      public bool SetSectionDataFromByteArray(int type, byte[] data) {
         if (!ContainsType(type) || data == null)
            return false;
         Section sec = sections[type];
         sec.Data = (byte[])data.Clone();
         sec.Position.Length = (uint)sec.Data.Length;
         return true;
      }

      /// <summary>
      /// setzt leere Abschnittsdaten
      /// </summary>
      /// <param name="type"></param>
      /// <param name="datalen"></param>
      /// <returns></returns>
      public bool SetEmptyData(int type, uint datalen) {
         if (!ContainsType(type) || datalen == 0)
            return false;
         Section sec = sections[type];
         sec.Data = new byte[datalen];
         sec.Position.Length = (uint)sec.Data.Length;
         return true;
      }

      /// <summary>
      /// setzt leere Abschnittsdaten
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public bool SetEmptyData(int type) {
         if (!ContainsType(type))
            return false;
         Section sec = sections[type];
         sec.Data = new byte[sec.Position.Length];
         return true;
      }

      /// <summary>
      /// setzt den akt. registrierte Offset des Abschnitts mit dem Typ
      /// </summary>
      /// <param name="type"></param>
      /// <returns>false, wenn der Typ nicht existiert</returns>
      public bool SetOffset(int type, uint newoffset) {
         if (ContainsType(type)) {
            sections[type].Position.Offset = newoffset;
            return true;
         }
         return false;
      }

      /// <summary>
      /// setzt den akt. registrierte Offset und die Länge des Abschnitts mit dem Typ
      /// <para>ACHTUNG. Die Länge muss nicht mit der realen Datenlänge übereinstimmen.</para>
      /// </summary>
      /// <param name="type"></param>
      /// <param name="newoffset"></param>
      /// <param name="length"></param>
      /// <returns>false, wenn der Typ nicht existiert</returns>
      public bool SetPosition(int type, uint newoffset, uint length, ushort recordsize = 0) {
         if (ContainsType(type)) {
            sections[type].Position.Offset = newoffset;
            sections[type].Position.Length = length;
            sections[type].Position.Recordsize = recordsize;
            return true;
         }
         return false;
      }

      /// <summary>
      /// setzt den akt. registrierte Offset und die Länge des Abschnitts mit dem Typ
      /// <para>ACHTUNG. Die Länge muss nicht mit der realen Datenlänge übereinstimmen.</para>
      /// </summary>
      /// <param name="type"></param>
      /// <param name="block"></param>
      /// <returns>false, wenn der Typ nicht existiert</returns>
      public bool SetPosition(int type, DataBlock block) {
         return block is DataBlockWithRecordsize ?
            SetPosition(type, block.Offset, block.Length, ((DataBlockWithRecordsize)block).Recordsize) :
            SetPosition(type, block.Offset, block.Length);
      }

      /// <summary>
      /// liefert den akt. registrierte Offset und die Länge des Abschnitts mit dem Typ
      /// <para>ACHTUNG. Die Länge muss nicht mit der realen Datenlänge übereinstimmen.</para>
      /// </summary>
      /// <param name="type"></param>
      /// <returns>null, wenn der Typ nicht existiert</returns>
      public DataBlockWithRecordsize? GetPosition(int type) {
         if (ContainsType(type))
            return sections[type].Position;
         return null;
      }

      /// <summary>
      /// liefert den akt. registrierte Offset des Abschnitts mit dem Typ
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public uint GetOffset(int type) {
         if (ContainsType(type))
            return sections[type].Position.Offset;
         return 0;
      }

      /// <summary>
      /// liefert die akt. registrierte Länge des Abschnitts mit dem Typ
      /// <para>ACHTUNG. Die Länge muss nicht mit der realen Datenlänge übereinstimmen.</para>
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public uint GetLength(int type) {
         if (ContainsType(type))
            return sections[type].Position.Length;
         return 0;
      }

      /// <summary>
      /// liefert die akt. reale Länge des Abschnitts mit dem Typ
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public uint GetDataLength(int type) {
         if (ContainsType(type)) {
            Section sec = sections[type];
            return sec != null && sec.Data != null ? (uint)sec.Data.Length : 0;
         }
         return 0;
      }

      /// <summary>
      /// liefert alle akt. vorhandenen Typen
      /// </summary>
      /// <returns></returns>
      public int[] GetTypes() {
         int[] type = new int[sections.Count];
         if (sections.Count > 0)
            sections.Keys.CopyTo(type, 0);
         return type;
      }

      /// <summary>
      /// liefert die sortierten Offsets der akt. Liste und den jeweiligen Typ 
      /// </summary>
      /// <param name="type">Array der zugehörigen Typen</param>
      /// <param name="onlyused">wenn true, werden nur Abschnitte mit einer Länge größer 0 geliefert</param>
      /// <returns></returns>
      public uint[] GetSortedOffsets(out int[] type, bool onlyused = false) {
         int count = sections.Count;
         if (onlyused) {
            foreach (var dat in sections)
               if (dat.Value.Position.Length == 0)
                  count--;
         }

         type = new int[count];
         uint[] offs = new uint[count];

         if (sections.Count > 0) {
            int pos = 0;
            foreach (Section sec in SortedList4Offset())
               if (sec.Position.Length > 0 || !onlyused) {
                  offs[pos] = sec.Position.Offset;
                  if (sec.ExtData != null)
                     type[pos++] = (int)sec.ExtData;
               }
         }

         return offs;
      }

      /// <summary>
      /// liefert die sortierten Offsets der akt. Liste
      /// </summary>
      /// <returns></returns>
      public uint[] GetSortedOffsets() {
         int[] type;
         return GetSortedOffsets(out type);
      }

      /// <summary>
      /// liefert den kleinsten Offset (oder 0) der akt. Liste
      /// </summary>
      /// <returns></returns>
      public uint GetStartOffset() {
         return sections.Count > 0 ? SortedList4Offset()[0].Position.Offset : 0;
      }

      /// <summary>
      /// Offset auf die 1. Position hinter dem letzten Abschnitt der akt. Liste
      /// </summary>
      /// <returns></returns>
      public uint GetOffsetBehind() {
         Section? lastsec = sections.Count > 0 ? SortedList4Offset()[sections.Count - 1] : null;
         return lastsec != null ? lastsec.Position.Offset + lastsec.Position.Length : 0;
      }

      /// <summary>
      /// liefert eine nach Offsets sortierte Liste (der gleiche Offset kann mehrfach auftreten); ExtData ist mit dem aktuellen Typ gesetzt
      /// </summary>
      /// <returns></returns>
      List<Section> SortedList4Offset() {
         foreach (var item in sections)         // damit der Key danach noch bekannt ist
            item.Value.ExtData = item.Key;

         List<Section> lst = new List<Section>(sections.Values);
         lst.Sort();
         return lst;
      }

      /// <summary>
      /// setzt die Länge der Blöcke entsprechend der Datenlänge und schiebt die Offsets so zusammen, dass keine Lücke bleibt
      /// <para>Die Sortierung der Abschnitte bleibt entsprechend des ursprünglichen Offsets erhalten.</para>
      /// </summary>
      /// <param name="startoffset">Wert des 1. Offsets</param>
      public void AdjustSections(uint startoffset) {
         if (sections.Count > 0) {
            List<Section> lst = SortedList4Offset(); // nach den akt. Offsets sortiert
            lst[0].Position.Offset = startoffset;
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
            lst[0].Position.Length = (uint)(lst[0].Data != null ? lst[0].Data.Length : 0);
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
            for (int i = 1; i < lst.Count; i++) {
               lst[i].Position.Offset = lst[i - 1].Position.Offset + lst[i - 1].Position.Length;
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
               lst[i].Position.Length = (uint)(lst[i].Data != null ? lst[i].Data.Length : 0);
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
            }

            int[] keys = new int[sections.Count];
            sections.Keys.CopyTo(keys, 0); // sortierte Liste der Keys
            for (int i = 1; i < keys.Length; i++) {
               if (sections[keys[i]].Position.Length == 0)
                  sections[keys[i]].Position.Offset = sections[keys[i - 1]].Position.Offset;
            }
         }
      }

      /// <summary>
      /// setzt die Länge der Blöcke entsprechend der Datenlänge und schiebt die Offsets so zusammen, dass keine Lücke bleibt
      /// <para>Ausnahme: <see cref="gapoffset"/> ist kleiner als <see cref="dataoffset"/> und die 1. <see cref="Section"/> hat den Typ <see cref="postheadertyp"/></para>
      /// <para>In diesem Fall werden alle Abschnitte erst ab <see cref="dataoffset"/> kalkuliert.</para>
      /// <para>Sonst steht der 1. Abschnitt ab <see cref="gapoffset"/> und die restlichen ab <see cref="dataoffset"/>.</para>
      /// </summary>
      /// <param name="gapoffset"></param>
      /// <param name="dataoffset"></param>
      /// <param name="postheadertyp"></param>
      public void AdjustSections(uint gapoffset, uint dataoffset, int postheadertyp) {
         if (sections.Count > 0) {
            uint gap = dataoffset - gapoffset;
            int[] type;
            GetSortedOffsets(out type, true);

            if (type.Length > 0) {
               Section sec = sections[type[0]];
               uint offs = gapoffset;
               uint length = (uint)(sec.Data != null ? sec.Data.Length : 0);

               if (type[0] != postheadertyp) {
                  offs = dataoffset;
                  gap = 0;
               }

               sec.Position.Offset = offs;
               sec.Position.Length = length;
               offs += length + gap;

               for (int i = 1; i < type.Length; i++) {
                  sec = sections[type[i]];
                  length = (uint)(sec.Data != null ? sec.Data.Length : 0);
                  sec.Position.Offset = offs;
                  sec.Position.Length = length;
                  offs += length;
               }
            }

            for (int i = 1; i < type.Length; i++) {
               if (sections[type[i]].Position.Length == 0)
                  sections[type[i]].Position.Offset = sections[type[i - 1]].Position.Offset;
            }
         }
      }

      public override string ToString() {
         return string.Format("Count={0}, StartOffset=0x{1:x}, OffsetBehind=0x{2:x}",
                              Count,
                              GetStartOffset(),
                              GetOffsetBehind());
      }

   }
}
