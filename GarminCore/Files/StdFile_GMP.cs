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
   /// Sammeldatei für die "geografischen" Daten
   /// <para>
   /// ACHTUNG: "Echte" NT-Karten kodieren u.a. die LBL-Daten in einem neuen unbekannten Format.
   /// </para>
   /// </summary>
   public class StdFile_GMP : StdFile, IDisposable {

      /* Am Anfang der Datei steht wie üblich der Standardheader. Danach folgt im spezifischen Teil eine Tabelle fester Länge mit den Verweisen auf die Subdateiheader.
       * Wenn diese 0 sind, ex. die Subdatei nicht.
       * 
       * ACHTUNG
       * Zwischen Subdateiheadern können zusätzliche Daten enthalten!
       * 
       * Die Subdateiheader verweisen wie üblich auf die eigentlichen Daten, die aber in einem gemeinsamen Datenbereich am Ende der GMP-Datei stehen. Theoretisch können
       * die Datenbereiche der Subdateien also auch vermischt sein.
       */


      #region Header-Daten

      public byte[] Unknown_0x15;

      /// <summary>
      /// Offset der Header der Sub-Dateien
      /// </summary>
      UInt32[] SubHeaderOffsets;

      public byte[] Unknown_0x35;

      public List<string> Copyright;

      #endregion


      /// <summary>
      /// enthaltene Dateien
      /// </summary>
      StdFile[] stdfiles;

      /// <summary>
      /// Headerlänge der Sub-Dateien
      /// </summary>
      readonly uint[] SubHeaderLength;

      /// <summary>
      /// Abstand von einem Sub-Datei-Headeranfang zum nächsten (0, wenn kein Header mehr folgt)
      /// </summary>
      readonly uint[] SubHeaderDistance;

      /// <summary>
      /// Offsets der Gap's
      /// </summary>
      readonly uint[] SubGapOffset;


      /// <summary>
      /// Dateitypen, die enthalten sein können (die Nummer entspricht der internen Reihenfolge; die Größe der SubHeaderOffsets-Tabelle ist durch diese Typanzahl bestimmt!)
      /// </summary>
      public enum Filetype {
         TRE = 0,
         RGN,
         LBL,
         NET,
         NOD,
         DEM,
         MAR,
      }

      /// <summary>
      /// Offset des Datenbereiches bezüglich des Headeranfangs (kann nie kleiner als <see cref="Headerlength"/> sein und ist normalerweise 
      /// auch <see cref="Headerlength"/>)
      /// </summary>
      public override uint DataOffset {
         get {
            uint ext = 0;
            if (Copyright != null)
               foreach (string txt in Copyright)
                  ext += (uint)(txt.Length + 1);
            return Math.Max(Headerlength + ext, _DataOffset);
         }
         protected set {
            uint ext = 0;
            if (Copyright != null)
               foreach (string txt in Copyright)
                  ext += (uint)(txt.Length + 1);
            _DataOffset = Math.Max(Headerlength + ext, value);
         }
      }


      public StdFile_GMP()
         : base("GMP") {
         Unknown_0x15 = new byte[4];
         SubHeaderOffsets = new uint[Enum.GetNames(typeof(Filetype)).Length];

         Unknown_0x35 = new byte[0];

         Copyright = new List<string>();

         SubGapOffset = new uint[SubHeaderOffsets.Length];
         SubHeaderLength = new uint[SubHeaderOffsets.Length];
         SubHeaderDistance = new uint[SubHeaderOffsets.Length];
         stdfiles = new StdFile[SubHeaderOffsets.Length];
      }

      public override void ReadHeader(BinaryReaderWriter br) {
         base.ReadCommonHeader(br, Type);

         br.ReadBytes(Unknown_0x15);

         // die Header- also Sub-Dateianfänge einlesen
         for (int i = 0; i < SubHeaderOffsets.Length; i++)
            SubHeaderOffsets[i] = br.Read4UInt();

         if (Headerlength > 0x35)
            Unknown_0x35 = br.ReadBytes(Headerlength - 0x35);

         // echte Sub-Dateiheaderlängen aus dem jeweiligen Sub-Datei-Header einlesen
         for (int i = 0; i < SubHeaderOffsets.Length; i++)
            if (SubHeaderOffsets[i] != 0) {
               br.Seek(SubHeaderOffsets[i]);
               SubHeaderLength[i] = br.Read2AsUShort();
            } else
               SubHeaderLength[i] = 0;

         // Subdateiheaderdistanzen berechnen
         // - bei SubHeaderOffsets==0 ist auch SubHeaderDistance==0 weil der Header ungültig ist
         // - beim letzten SubHeader ist SubHeaderDistance==uint.MaxValue, da es nicht anders geht
         for (int i = 0; i < SubHeaderOffsets.Length; i++) {
            if (SubHeaderOffsets[i] > 0 &&
                i < SubHeaderOffsets.Length - 1) {
               // nächsten gültigen Subheader suchen
               int j = SubHeaderOffsets.Length;
               for (j = i + 1; j < SubHeaderOffsets.Length; j++)
                  if (SubHeaderOffsets[j] >= SubHeaderOffsets[i] + SubHeaderLength[i])
                     break;
               if (j < SubHeaderOffsets.Length)
                  SubHeaderDistance[i] = SubHeaderOffsets[j] - SubHeaderOffsets[i];
            } else
               SubHeaderDistance[i] = uint.MaxValue;
         }

      }

      protected override void ReadSections(BinaryReaderWriter br) { }

      protected override void DecodeSections() { }

      public override void Read(BinaryReaderWriter br, bool raw = false, uint headeroffset = 0, uint gapoffset = 0) {
         base.Read(br, raw, headeroffset, gapoffset);

         if (Locked != 0)
            throw new Exception("Eine gesperrte GMP-Datei kann nicht gelesen werden.");

         SortedSet<int> sortedSubHeaderOffsets = new SortedSet<int>();
         foreach (int offs in SubHeaderOffsets)
            if (offs > 0)
               sortedSubHeaderOffsets.Add(offs);

         if (sortedSubHeaderOffsets.Count > 0) { // sonst hat die GMP-Datei keinen sinnvollen Inhalt
            int minoffs = sortedSubHeaderOffsets.Min;

            if (HeaderOffset + Headerlength < minoffs) // freier Bereich zum 1. Subheader -> Copyright-Abschnitt
               // Copyright dekodieren
               Decode_Copyright(br, new DataBlock(HeaderOffset + Headerlength, (ushort)minoffs - (HeaderOffset + Headerlength)));

            for (int i = 0; i < SubHeaderOffsets.Length; i++)
               if (SubHeaderOffsets[i] > 0) { // sonst ex. der Header nicht
                  switch (i) {
                     case 0:
                        stdfiles[i] = new StdFile_TRE();
                        break;

                     case 1:
                        stdfiles[i] = new StdFile_RGN((StdFile_TRE)stdfiles[0]);
                        break;

                     case 2:
                        stdfiles[i] = new StdFile_LBL();
                        break;

                     case 3:
                        stdfiles[i] = new StdFile_NET();
                        break;

                     case 4:
                        stdfiles[i] = new StdFile_NOD();
                        break;

                     case 5:
                        stdfiles[i] = new StdFile_DEM();
                        break;

                     case 6:
                        stdfiles[i] = new StdFile_MAR();
                        break;
                  }
                  if (SubHeaderDistance[i] < uint.MaxValue)
                     SubGapOffset[i] = SubHeaderOffsets[i] + SubHeaderDistance[i];
                  stdfiles[i].Read(br, raw, SubHeaderOffsets[i], SubGapOffset[i]);
               }
         }
      }

      public override void Write(BinaryReaderWriter bw, uint headeroffset = 0, UInt16 headerlength = 0x35, uint gapoffset = 0, uint dataoffset = 0, bool setsectiondata = true) {
         base.Write(bw, headeroffset, Headerlength, gapoffset, dataoffset, false);

         Encode_Copyright(bw);

         uint startoffs = (uint)bw.Length;

         // Offsets für die Subheader bestimmen
         for (int i = 0; i < SubHeaderOffsets.Length; i++)
            if (stdfiles[i] != null)
               SubHeaderOffsets[i] = startoffs;
            else
               SubHeaderOffsets[i] = 0;
         for (int i = 0; i < SubHeaderOffsets.Length; i++)
            for (int j = i + 1; j < SubHeaderOffsets.Length; j++)
               if (SubHeaderOffsets[j] > 0)
                  SubHeaderOffsets[j] += stdfiles[i].Headerlength;

         // Postheaderdaten berücksichtigen
         uint[] SubGapOffset = new uint[stdfiles.Length];
         for (int i = 0; i < stdfiles.Length; i++)
            if (stdfiles[i] != null) {
               if (!stdfiles[i].RawRead)
                  stdfiles[i].Encode_Sections(); // damit steht die Länge der Datenabschnitte fest
               SubGapOffset[i] = SubHeaderOffsets[i] + stdfiles[i].Headerlength;
               uint postheaderlen = stdfiles[i].GetPostHeaderLength();
               if (postheaderlen > 0) {
                  SubGapOffset[i] += postheaderlen;
                  for (int j = i + 1; j < stdfiles.Length; j++)
                     SubHeaderOffsets[j] += postheaderlen;
               }
               startoffs = Math.Max(startoffs, SubGapOffset[i]);
            }

         // alle Subdateien speichern
         for (int i = 0; i < stdfiles.Length; i++)
            if (stdfiles[i] != null) {
               stdfiles[i].Write(bw, SubHeaderOffsets[i], stdfiles[i].Headerlength, SubGapOffset[i], startoffs, false);
               startoffs = (uint)bw.Length;
            }
      }

      void Decode_Copyright(BinaryReaderWriter br, DataBlock block) {
         Copyright.Clear();
         if (br != null) {
            br.Seek(block.Offset);
            while (br.Position < block.Offset + block.Length)
               Copyright.Add(br.ReadString());
         }
      }


      public override void Encode_Sections() { }

      protected override void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype) { }

      public override void SetSectionsAlign() { }


      protected override void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null) {
            base.Encode_Header(bw);

            bw.Write(Unknown_0x15);

            for (int i = 0; i < SubHeaderOffsets.Length; i++)
               bw.Write(SubHeaderOffsets[i]);

            if (Headerlength > 0x35)
               bw.Write(Unknown_0x35);
         }
      }

      void Encode_Copyright(BinaryReaderWriter bw) {
         if (bw != null)
            foreach (string txt in Copyright)
               bw.WriteString(txt);
      }


      public StdFile GetFile(Filetype typ) {
         return stdfiles[(int)typ];
      }

      public StdFile_TRE TRE {
         get {
            return (StdFile_TRE)stdfiles[(int)Filetype.TRE];
         }
         set {
            stdfiles[(int)Filetype.TRE] = value;
         }
      }

      public StdFile_RGN RGN {
         get {
            return (StdFile_RGN)stdfiles[(int)Filetype.RGN];
         }
         set {
            stdfiles[(int)Filetype.RGN] = value;
         }
      }

      public StdFile_LBL LBL {
         get {
            return (StdFile_LBL)stdfiles[(int)Filetype.LBL];
         }
         set {
            stdfiles[(int)Filetype.LBL] = value;
         }
      }

      public StdFile_NET NET {
         get {
            return (StdFile_NET)stdfiles[(int)Filetype.NET];
         }
         set {
            stdfiles[(int)Filetype.NET] = value;
         }
      }

      public StdFile_NOD NOD {
         get {
            return (StdFile_NOD)stdfiles[(int)Filetype.NOD];
         }
         set {
            stdfiles[(int)Filetype.NOD] = value;
         }
      }

      public StdFile_DEM DEM {
         get {
            return (StdFile_DEM)stdfiles[(int)Filetype.DEM];
         }
         set {
            stdfiles[(int)Filetype.DEM] = value;
         }
      }

      public StdFile_MAR MAR {
         get {
            return (StdFile_MAR)stdfiles[(int)Filetype.MAR];
         }
         set {
            stdfiles[(int)Filetype.MAR] = value;
         }
      }

      /// <summary>
      /// liefert die Dateierweiterung zum Typ
      /// </summary>
      /// <param name="typ"></param>
      /// <returns></returns>
      public string GetFileExtension(Filetype typ) {
         switch (typ) {
            case Filetype.TRE: return ".TRE";
            case Filetype.RGN: return ".RGN";
            case Filetype.LBL: return ".LBL";
            case Filetype.NET: return ".NET";
            case Filetype.NOD: return ".NOD";
            case Filetype.DEM: return ".DEM";
            case Filetype.MAR: return ".MAR";
         }
         return "";
      }

   }
}
