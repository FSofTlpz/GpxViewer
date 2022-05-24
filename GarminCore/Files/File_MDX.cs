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
   /// liest oder erzeugt eine MDX-Datei;
   /// das ist einfach nur eine Auflistung aller zugehörigen Teilkarten
   /// </summary>
   public class File_MDX {

      public class MapEntry {
         /// <summary>
         /// ID der Kachel (identisch zur TRE-Datei) und i.A. auch der (hexadezimale) Basisname der Dateien bzw. der (dezimale) Name der IMG-Kachel-Datei
         /// (Hexadezimalzahl notfalls mit führendem 'I' und Vornullen für einen Namen mit 8 Zeichen)
         /// <para>Eine Hexadezimalzahl größer oder gleich 0x00989680 liefert eine Dezimalzahl mit 8 Stellen. Sie sollte aber auch nicht größer als 0x05F5E0FF sein</para>
         /// </summary>
         public UInt32 MapID;

         public UInt16 ProductID;

         public UInt16 FamilyID;

         /// <summary>
         /// Nummer (Name) des Verzeichnisses oder der IMG-Kachel, dass die RGN- usw. Daten enthält
         /// <para>Das Verzeichnis oder die IMG-Datei hat die Dezimalzahl als Namen.</para>
         /// <para>Wenn <see cref="MapID"/> aber auf eine IMG-Datei verweist, sollte <see cref="MapNumber"/> identisch sein (?).</para>
         /// <para>Bei der 'TOPO Deutschland v3'  gibt es aber z.B. auch die Datei 'TOPO Deutschland v3.gmap\Product1\TPDEUEC3\BASEMAP.TRE' mit der 
         /// <see cref="MapID"/> 0xFCC19B und der Nummer 0. Zwischen Nummer und ID besteht scheinbar kein Zusammenhang.</para>
         /// </summary>
         public UInt32 MapNumber;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="mapid">Nummer (Name) der RGN- usw. Dateien</param>
         /// <param name="productid"></param>
         /// <param name="familyid"></param>
         /// <param name="mapumber">Nummer (Name) des Verzeichnisses, dass die RGN- usw. Daten enthält bzw. Nummer (Name) der IMG-Datei</param>
         public MapEntry(UInt32 mapid = 0, UInt16 productid = 0, UInt16 familyid = 0, UInt32 mapumber = 0) {
            MapID = mapid;
            ProductID = productid;
            FamilyID = familyid;
            MapNumber = mapumber;
         }

         public void Read(BinaryReaderWriter br) {
            MapID = br.Read4UInt();
            ProductID = br.Read2AsUShort();
            FamilyID = br.Read2AsUShort();
            MapNumber = br.Read4UInt();
         }

         public void Write(BinaryReaderWriter wr) {
            wr.Write(MapID);
            wr.Write(ProductID);
            wr.Write(FamilyID);
            wr.Write(MapNumber);
         }

         public override string ToString() {
            return string.Format("ProductID {0}, FamilyID {1} / 0x{1:x}, MapID {2} / 0x{2:x}, DirNumber {3} / 0x{3:x}",
               ProductID,
               FamilyID,
               MapID,
               MapNumber);
         }

      }


      public UInt16 Unknown1;

      public UInt16 Unknown2;

      /// <summary>
      /// sollte mit der Anzahl in <see cref="Maps"/> übereinstimmen
      /// </summary>
      public UInt32 Count;

      /// <summary>
      /// Liste der Teilkarten
      /// </summary>
      public List<MapEntry> Maps;


      public File_MDX() {
         Maps = new List<MapEntry>();
         Unknown1 = 0xc;
         Unknown2 = 0;
      }

      /// <summary>
      /// lese die Daten aus einer MDX-Datei ein
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         br.Seek(0);
         byte[] id = br.ReadBytes(6);

         if (id[0] != 'M' ||
             id[1] != 'i' ||
             id[2] != 'd' ||
             id[3] != 'x' ||
             id[4] != 'd' ||
             id[5] != 0)
            throw new Exception("Keine MDX-Datei.");

         Unknown1 = br.Read2AsUShort();
         Unknown2 = br.Read2AsUShort();

         Count = br.Read4UInt();
         Maps.Clear();
         for (int i = 0; i < Count; i++) {
            MapEntry entry = new MapEntry();
            entry.Read(br);
            Maps.Add(entry);
         }
      }

      /// <summary>
      /// schreibe die aktuellen Daten als MDX-Datei
      /// </summary>
      /// <param name="wr"></param>
      public void Write(BinaryReaderWriter wr) {
         wr.Write(new byte[] { (byte)'M', (byte)'i', (byte)'d', (byte)'x', (byte)'d', 0 });
         wr.Write(Unknown1);
         wr.Write(Unknown2);
         wr.Write((UInt32)Maps.Count);
         for (int i = 0; i < Maps.Count; i++)
            Maps[i].Write(wr);
      }

      public override string ToString() {
         return Maps.Count.ToString() + " Teilkarten";
      }

   }
}
