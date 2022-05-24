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

#pragma warning disable 0661,0660

namespace GarminCore.Files {

   /// <summary>
   /// MAR-Datei (nur eine Hülle)
   /// </summary>
   public class StdFile_MAR : StdFile {

      #region Header-Daten


      #endregion

      enum InternalFileSections {
         PostHeaderData = 0,
         RawData,
      }



      public StdFile_MAR()
         : base("MAR") {

      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

      }

      protected override void ReadSections(BinaryReaderWriter br) {
         // --------- Dateiabschnitte für die Rohdaten bilden ---------
         // der gesamte Rest der Datei sind MAR-Daten
         Filesections.AddSection((int)InternalFileSections.RawData, new DataBlock(DataOffset, (uint)br.Length - DataOffset));
         if (GapOffset > HeaderOffset + Headerlength) // nur möglich, wenn extern z.B. auf den nächsten Header gesetzt
            Filesections.AddSection((int)InternalFileSections.PostHeaderData, HeaderOffset + Headerlength, GapOffset - (HeaderOffset + Headerlength));

         // Datenblöcke einlesen
         Filesections.ReadSections(br);

         SetSpecialOffsetsFromSections((int)InternalFileSections.PostHeaderData);
      }

      protected override void DecodeSections() {

         RawRead = true; // besser geht es noch nicht


         if (RawRead || Locked != 0) {
            RawRead = true;
            return;
         }


      }

      public override void Encode_Sections() { }
      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) { }
      public override void SetSectionsAlign() {
         // durch Pseudo-Offsets die Reihenfolge der Abschnitte festlegen
         uint pos = 0;
         Filesections.SetOffset((int)InternalFileSections.PostHeaderData, pos++);
         Filesections.SetOffset((int)InternalFileSections.RawData, pos++);

         // endgültige Offsets der Datenabschnitte setzen
         Filesections.AdjustSections(GapOffset, DataOffset, (int)InternalFileSections.PostHeaderData);     // lückenlos ausrichten

         // Offsets für den Header setzen

         // Das wird bei einer Veränderung sicher nicht fkt., da vermutlich auch Pointer im Header ex. die angepasst werden müssten!!

      }

      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

         }
      }

   }

}
