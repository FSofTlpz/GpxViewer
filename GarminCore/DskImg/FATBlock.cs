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

namespace GarminCore.DskImg {

   /// <summary>
   /// ein einzelner Speicherblock der FAT
   /// </summary>
   class FATBlock {

      public FATBlock(uint blocklength) {
         BlockSize = blocklength;
         Name = "";
         Typ = "";
         Filesize = 0;
         Used = false;
         Flag = 0;
         Part = 0;
         Unknown = new byte[14];
         for (int i = 0; i < Unknown.Length; i++)
            Unknown[i] = 0x00;
         blocks = new List<UInt16>(MaxBlockNumberCount);
      }

      public FATBlock(FATBlock bl) :
         this(bl.BlockSize) {
         BlockSize = bl.BlockSize;
         Name = bl.Name;
         Typ = bl.Typ;
         Filesize = bl.Filesize;
         Used = bl.Used;
         Flag = bl.Flag;
         Part = bl.Part;
         Unknown = new byte[bl.Unknown.Length];
         bl.Unknown.CopyTo(Unknown, 0);
      }


      /// <summary>
      /// interne Blocktabelle
      /// </summary>
      List<UInt16> blocks;

      /// <summary>
      /// Größe des FAT-Blocks
      /// </summary>
      public uint BlockSize { get; private set; }
      /// <summary>
      /// Block (Datei) wird genutzt
      /// </summary>
      public bool Used;

      /// <summary>
      /// Datei-/Verzeichnisname (gesamter)
      /// </summary>
      public string FullName {
         get {
            return _Name.Trim() + "." + _Typ.Trim();
         }
         set {
            GetValidFullname(value, out string name, out string typ);
            Name = name;
            Typ = typ;
         }
      }

      /// <summary>
      /// erzeugt einen gültigen Namen
      /// </summary>
      /// <param name="fullname"></param>
      /// <returns></returns>
      public static string GetValidFullname(string fullname) {
         GetValidFullname(fullname, out string name, out string typ);
         return fullname;
      }

      static void GetValidFullname(string fullname, out string name, out string typ) {
         fullname = fullname.ToUpper();
         int p = fullname.IndexOf('.');
         if (p >= 0) {
            name = fullname.Substring(0, p);
            typ = fullname.Substring(p + 1);
         } else {
            name = fullname;
            typ = "";
         }
         fullname = name.Trim() + "." + typ.Trim();
      }

      string _Name = "";

      /// <summary>
      /// Datei-/Verzeichnisname (org., ohne Typ)
      /// </summary>
      public string Name {
         get {
            return _Name;
         }
         set {
            _Name = value;
            if (_Name.Length > 8)
               _Name = _Name.Substring(0, 8);
            else
               if (_Name.Length < 8)
               _Name += new string(' ', 8 - _Name.Length);
         }
      }

      string _Typ = "";

      /// <summary>
      /// Typ (org.)
      /// </summary>
      public string Typ {
         get {
            return _Typ;
         }
         set {
            _Typ = value;
            if (_Typ.Length > 3)
               _Typ = _Typ.Substring(0, 3);
            else
               if (_Typ.Length < 3)
               _Typ += new string(' ', 3 - _Typ.Length);
         }
      }
      /// <summary>
      /// Dateigröße in Byte
      /// </summary>
      public uint Filesize;

      /// <summary>
      /// ev. wie FAT: Bit 0: Schreibgeschützt; Bit 1: Versteckt; Bit 2: Systemdatei; Bit 3: Volume-Label; Bit 4: Unterverzeichnis; Bit 5: Archiv
      /// </summary>
      public byte Flag;
      /// <summary>
      /// i.A. 0, aber wenn ein <see cref="FATBlock"/> nicht für eine Datei ausreicht (Dateilänge zu groß), werden mehrere <see cref="FATBlock"/> verwendet und durchnummeriert
      /// </summary>
      public byte Part;

      public byte[] Unknown;

      /// <summary>
      /// Ist die interne Blocktabelle voll?
      /// </summary>
      public bool BlockTableIsFull { get { return MaxBlockNumberCount == blocks.Count; } }

      /// <summary>
      /// Anzahl der Blöcke
      /// </summary>
      public int BlockNumberCount { get { return blocks.Count; } }

      /// <summary>
      /// max. speicherbare Anzahl von Blöcken
      /// </summary>
      public int MaxBlockNumberCount { get { return MaxBlocks((int)BlockSize); } }

      /// <summary>
      /// max. speicherbare Anzahl von Blöcken
      /// </summary>
      /// <param name="blocksize"></param>
      /// <returns></returns>
      public static int MaxBlocks(int blocksize) {
         return (int)(blocksize - 0x20) / 2;
      }

      /// <summary>
      /// Nummer eines Blocks aus der Tabelle
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public UInt16 GetBlockNumber(int idx) {
         return blocks[idx];
      }
      /// <summary>
      /// setzt die NUmmer eines Blocks
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="number"></param>
      public void SetBlockNumber(int idx, UInt16 number) {
         blocks[idx] = number;
      }
      /// <summary>
      /// fügt eine Blocknummer hinzu
      /// </summary>
      /// <param name="number"></param>
      public void AppendBlockNumber(UInt16 number) {
         blocks.Add(number);
      }
      /// <summary>
      /// löscht alle Blocknummern
      /// </summary>
      public void ClearBlockNumbers() {
         blocks.Clear();
      }

      public void Read(BinaryReaderWriter br) {
         blocks.Clear();

         // 0x0
         Used = br.ReadByte() == 0x01;

         // 0x01
         Name = br.ReadString(8);

         // 0x09
         Typ = br.ReadString(3);

         // 0x0c
         Filesize = br.Read4UInt();

         // 0x10
         Flag = br.ReadByte();

         // 0x11
         Part = br.ReadByte();

         // 0x12
         Unknown = br.ReadBytes(14);

         // 0x20
         for (int i = 0; i < MaxBlockNumberCount; i++) {
            UInt16 no = br.Read2AsUShort();
            if (no != 0xffff)
               blocks.Add(no);
         }

      }

      public void Write(BinaryReaderWriter wr) {
         wr.Write(Used);
         if (Used) {
            wr.WriteString(Name, null, false);
            wr.WriteString(Typ, null, false);
            wr.Write(Filesize);
            wr.Write(Flag);
            wr.Write(Part);
            wr.Write(Unknown);
            if (blocks.Count > 0)
               for (int i = 0; i < (BlockSize - 32) / 2; i++) {
                  if (i < blocks.Count)
                     wr.Write((UInt16)(blocks[i] < 0xffff ? blocks[i] : 0xffff));
                  else
                     wr.Write((UInt16)0xffff);
               }
         } else
            for (int i = 1; i < BlockSize; i++)
               wr.Write((byte)0x00);
      }

      public override string ToString() {
         return string.Format("Used {0}, {1}, Teil {2}, {3} Bytes", Used, FullName, Part, Filesize);
      }

   }
}
