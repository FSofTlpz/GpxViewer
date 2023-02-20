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
   /// das ist i.W. eine Auflistung aller zugehörigen Teilkarten
   /// </summary>
   public class File_MPS {

      public class MapEntry {

         public char Typ;

         public UInt16 ProductID;

         public UInt16 FamilyID;

         /// <summary>
         /// Nummer (Name) der RGN- usw. Dateien
         /// <para>Aus z.B. 0xFCC19B wird der Name I0FCC19B.RGN usw.</para>
         /// <para>Wenn <see cref="Unknown1"/> aber auf eine IMG-Datei verweist, sollte <see cref="MapNumber"/> identisch sein.</para>
         /// </summary>
         public UInt32 MapNumber;

         /// <summary>
         /// I.A. SeriesName, MapDescription, AreaName
         /// </summary>
         public List<string> Name;

         /// <summary>
         /// HexNumber
         /// </summary>
         public UInt32 Unknown0;

         public UInt32 Unknown1;

         public UInt16 Unknown2;

         public UInt16 Unknown3;

         public UInt16 Unknown4;

         public byte Unknown5;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="dirnumber">Nummer (Name) des Verzeichnisses, dass die RGN- usw. Daten enthält bzw. Nummer (Name) der IMG-Datei</param>
         /// <param name="productid"></param>
         /// <param name="familyid"></param>
         /// <param name="mapnumber">Nummer (Name) der RGN- usw. Dateien</param>
         public MapEntry() {
            ProductID = 0;
            FamilyID = 0;
            MapNumber = 0;
            Unknown0 = Unknown1 = 0;
            Unknown2 = Unknown3 = Unknown4 = 0;
            Name = new List<string>();
            Typ = ' ';
         }

         public void Read(BinaryReaderWriter br) {
            Typ = (char)br.ReadByte();
            UInt16 len = br.Read2AsUShort();       // Anzahl der noch folgenden Bytes
            long end = br.Position + len;

            switch (Typ) {
               case 'L': // MapBlock
                  ProductID = br.Read2AsUShort();
                  FamilyID = br.Read2AsUShort();
                  MapNumber = br.Read4UInt();
                  while (br.Position < end - 9)     // seriesName, mapDescription, areaName
                     Name.Add(br.ReadString());
                  Unknown0 = br.Read4UInt();
                  Unknown1 = br.Read4UInt();
                  break;

               case 'P': // ProductBlock
                  ProductID = br.Read2AsUShort();
                  FamilyID = br.Read2AsUShort();
                  Unknown2 = br.Read2AsUShort();
                  Unknown3 = br.Read2AsUShort();
                  Unknown4 = br.Read2AsUShort();
                  break;

               case 'F': // vereinfachter MapBlock ?
                  ProductID = br.Read2AsUShort();
                  FamilyID = br.Read2AsUShort();
                  while (br.Position < end)         // description (nur 1x?)
                     Name.Add(br.ReadString());
                  break;

               case 'V':
                  while (br.Position < end - 1)
                     Name.Add(br.ReadString());
                  Unknown5 = br.ReadByte();
                  break;

               default:

                  break;
            }
         }

         public void Write(BinaryReaderWriter wr) {
            wr.Write((byte)Typ);
            UInt16 len = 0;

            if (Typ == 'L') {

               len = (UInt16)(12 + Name.Count);
               foreach (var item in Name)
                  len += (UInt16)item.Length;
               wr.Write(len);
               wr.Write(ProductID);
               wr.Write(FamilyID);
               wr.Write(MapNumber);
               foreach (var item in Name)
                  wr.WriteString(item);
               wr.Write(Unknown0);
               wr.Write(Unknown1);

            } else if (Typ == 'F') {

               len = (UInt16)(8 + Name.Count);
               foreach (var item in Name)
                  len += (UInt16)item.Length;
               wr.Write(len);
               wr.Write(ProductID);
               wr.Write(FamilyID);
               foreach (var item in Name)
                  wr.WriteString(item);

            } else if (Typ == 'P') {

               len = (UInt16)14;
               wr.Write(len);
               wr.Write(ProductID);
               wr.Write(FamilyID);
               wr.Write(Unknown2);
               wr.Write(Unknown3);
               wr.Write(Unknown4);

            } else if (Typ == 'V') {

               len = (UInt16)Name.Count;
               foreach (var item in Name)
                  len += (UInt16)item.Length;
               wr.Write(len);
               foreach (var item in Name)
                  wr.WriteString(item);

            }
         }

         public override string ToString() {
            return string.Format("Typ {0}, ProductID {1}, FamilyID {2} / 0x{2:x}, MapNumber 0x{3:x}, Unknown1 0x{4:x}, {5}",
                                 Typ,
                                 ProductID,
                                 FamilyID,
                                 MapNumber,
                                 Unknown1,
                                 string.Join("; ", Name));
         }

      }


      /// <summary>
      /// Liste der Teilkarten
      /// </summary>
      public List<MapEntry> Maps;


      public File_MPS() {
         Maps = new List<MapEntry>();
      }

      /// <summary>
      /// lese die Daten aus einer MDX-Datei ein
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         br.Seek(0);
         while (br.Position < br.Length) {
            MapEntry me = new MapEntry();
            me.Read(br);
            Maps.Add(me);
         }
      }

      /// <summary>
      /// schreibe die aktuellen Daten als MDX-Datei
      /// </summary>
      /// <param name="wr"></param>
      public void Write(BinaryReaderWriter wr) {
         for (int i = 0; i < Maps.Count; i++)
            Maps[i].Write(wr);
      }

      public override string ToString() {
         return Maps.Count.ToString() + " Teilkarten";
      }

   }
}
