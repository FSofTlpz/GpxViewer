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
using System.IO;

namespace GarminCore.DskImg {

   /// <summary>
   /// IMG-Datei mit ihrem Dateisystem
   /// </summary>
   public class SimpleFilesystem : IDisposable {

      /// <summary>
      /// Header der IMG-Datei
      /// </summary>
      public Header? ImgHeader;

      /// <summary>
      /// Größe der FAT (einschließlich Root) in Byte
      /// </summary>
      public int FATSize { get; private set; }


      /// <summary>
      /// interne Daten einer Datei
      /// </summary>
      protected class FileProps : IDisposable {

         public FileProps(string name, uint filesize, string? backgroundfile = null) {
            Name = FATBlock.GetValidFullname(name);
            Filesize = filesize;
            PseudoFileBlockList = new SortedDictionary<ushort, int>();
            Backgroundfile = !string.IsNullOrEmpty(backgroundfile) ? backgroundfile : Name;
         }

         string _Name = "";

         /// <summary>
         /// Name der Datei
         /// </summary>
         public string Name {
            get {
               return _Name;
            }
            set {
               _Name = FATBlock.GetValidFullname(value);
            }
         }
         /// <summary>
         /// Dateilänge
         /// </summary>
         public uint Filesize;
         /// <summary>
         /// Hintergrunddatei für geänderte Daten
         /// </summary>
         public string Backgroundfile { get; private set; }
         /// <summary>
         /// liefert die Anzahl der Pseudoblocks
         /// </summary>
         public int PseudoFileBlockCount => PseudoFileBlockList.Count;

         /// <summary>
         /// belegte Pseudo-Datenblöcke [0...] (Position je Blocknummer)
         /// </summary>
         readonly SortedDictionary<UInt16, int> PseudoFileBlockList;

         /// <summary>
         /// löscht die Pseudoblock-Liste
         /// </summary>
         public void PseudoFileBlocksClear() => PseudoFileBlockList.Clear();

         /// <summary>
         /// fügt einen Block and die Pseudoblock-Liste an
         /// </summary>
         /// <param name="blockno"></param>
         public void PseudoFileBlockAdd(UInt16 blockno) => PseudoFileBlockList.Add(blockno, PseudoFileBlockList.Count);

         /// <summary>
         /// liefert den Positionsindex eines Blocks der Pseudoblock-Liste (Reihenfolge)
         /// </summary>
         /// <param name="blockno"></param>
         /// <returns></returns>
         public int IndexOfPseudoFileBlock(UInt16 blockno) {
            if (PseudoFileBlockList.TryGetValue(blockno, out int idx))
               return idx;
            return -1;
         }
         /// <summary>
         /// liefert alle Blocks der Pseudoblock-Liste in der richtigen Reihenfolge
         /// </summary>
         /// <returns></returns>
         public UInt16[] PseudoFileBlocks() {
            UInt16[] tmp = new UInt16[PseudoFileBlockList.Count];
            foreach (var item in PseudoFileBlockList)
               tmp[item.Value] = item.Key;
            return tmp;
         }

         public override string ToString() {
            return string.Format("{0}, {1} Bytes, Backgroundfile: {2}", Name, Filesize, Backgroundfile);
         }

         ~FileProps() {
            Dispose(false);
         }

         #region Implementierung der IDisposable-Schnittstelle

         /// <summary>
         /// true, wenn schon ein Dispose() erfolgte
         /// </summary>
         private bool _isdisposed = false;

         /// <summary>
         /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
         /// </summary>
         public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
         }

         /// <summary>
         /// überschreibt die Standard-Methode
         /// <para></para>
         /// </summary>
         /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
         protected virtual void Dispose(bool notfromfinalizer) {
            if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
               if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)
               if (File.Exists(Backgroundfile))
                  File.Delete(Backgroundfile);

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion

      }

      /// <summary>
      /// Liste aller Dateien
      /// </summary>
      protected List<FileProps> Files = new List<FileProps>();

      /// <summary>
      /// Dateiindex für jede Blocknummer
      /// </summary>
      Dictionary<UInt16, int>? file4block;

      /// <summary>
      /// Anzahl der Blöcke vor den Datei-Datenblöcken in der IMG-Datei (für das Lesen der Daten)
      /// </summary>
      protected int preblocks4read;

      /// <summary>
      /// BinaryReaderWriter für die zu lesende IMG-Datei
      /// </summary>
      protected BinaryReaderWriter? binreader;

      /// <summary>
      /// Pfad zu den temp. Hintergrunddateien
      /// </summary>
      readonly string? backgroundpath;

      /// <summary>
      /// Pfad zu den temp. Hintergrunddateien selbst erzeugt ?
      /// </summary>
      readonly bool backgroundpathcreated;


      /// <summary>
      /// IMG-Datei-Verwaltung mit temp. Hintergrunddateien
      /// </summary>
      /// <param name="backgroundpath">wenn null, dann Path.GetTempFileName()</param>
      /// <param name="withbackgroundfiles">wenn false, dann ohne Hintergrunddateien</param>
      public SimpleFilesystem(string? backgroundpath = null, bool withbackgroundfiles = true) {
         init();

         if (withbackgroundfiles) {
            if (string.IsNullOrEmpty(backgroundpath)) {
               backgroundpath = Path.GetTempFileName();
               File.Delete(backgroundpath);
            }
            this.backgroundpath = backgroundpath;
            backgroundpathcreated = false;

            if (this.backgroundpath != "." &&
                !Directory.Exists(this.backgroundpath)) {
               Directory.CreateDirectory(this.backgroundpath);
               backgroundpathcreated = true;
            }
         }
      }

      protected void init() {
         ImgHeader = new Header {
            FileBlockLength = 0x200,
            FATBlockLength = 0x200,
            HeadSectors = 2
         };

         file4block = new Dictionary<ushort, int>();
         preblocks4read = -1;
         binreader = null;
      }

      /// <summary>
      /// der Stream meldet eine neue Dateigröße
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="newsize"></param>
      /// <param name="extradata"></param>
      void stream_NewSize(object sender, uint newsize, object? extradata) {
         if (extradata != null)
            ((FileProps)extradata).Filesize = newsize;
      }

      //int GetFileIdx(string filename) {
      //   for (int i = 0; i < Files.Count; i++)
      //      if (Files[i].Name == filename)
      //         return i;
      //   return -1;
      //}

      /// <summary>
      /// Name der Backgrounddatei zur Datei
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      string getBackFilename(string filename) =>
         backgroundpath != null ? Path.Combine(backgroundpath, filename) : filename;

      /// <summary>
      /// liefert die nötige Anzahl Blöcke für die Dateigröße; abh. von der <see cref="Header.FileBlockLength"/>
      /// </summary>
      /// <param name="filesize"></param>
      /// <returns></returns>
      int blocks4File(int filesize)
         => ImgHeader != null ? filesize / ImgHeader.FileBlockLength + (filesize % ImgHeader.FileBlockLength != 0 ? 1 : 0) : 0;

      /// <summary>
      /// liefert die nötige Anzahl FAT-Blöcke für die Dateigröße; abh. von der <see cref="Header.FileBlockLength"/> und der <see cref="Header.FATBlockLength"/>
      /// </summary>
      /// <param name="filesize"></param>
      /// <returns></returns>
      int FATBlocks4File(int filesize) {
         if (ImgHeader != null) {
            int blocks = blocks4File(filesize);
            int maxblocks = FATBlock.MaxBlocks(ImgHeader.FATBlockLength);
            return blocks / maxblocks + (blocks % maxblocks != 0 ? 1 : 0);
         }
         return 0;
      }

      /// <summary>
      /// liefert die aktuellen Dateidaten (über den <see cref="BinaryReaderWriter"/> des Dateisystems oder aus der Backup-Datei)
      /// <para>Wegen der Funktion Stream.Read() darf die internen Datei nicht größer als 0x3FFFFFFF (2 GB) sein.</para>
      /// <para>Wenn der IMG-BinaryReaderWriter geliefert wird, werden die Datei-Daten in einen neuen Speicherbereich kopiert und dieser geliefert.</para>
      /// </summary>
      /// <param name="file"></param>
      /// <param name="br"></param>
      /// <returns></returns>
      protected byte[]? getFiledata(FileProps file, BinaryReaderWriter? br) {
         if (ImgHeader != null) {
            if (br == null ||
                preblocks4read < 0 ||
                File.Exists(file.Backgroundfile)) {
               if (File.Exists(file.Backgroundfile))
                  return File.ReadAllBytes(file.Backgroundfile);
               // dann ex. noch keine Daten
               return new byte[0];
            } else {
               if (br != null &&
                   preblocks4read > 0) {                          // aus dem Originalstream Daten einlesen
                  byte[] data = new byte[file.Filesize];          // neuer Speicherbereich
                  UInt16[] blocks = file.PseudoFileBlocks();

                  for (int i = 0; i < blocks.Length; i++) {
                     int offset = ImgHeader.FileBlockLength * i;
                     br.Seek(ImgHeader.FileBlockLength * (long)(preblocks4read + blocks[i]));
                     br.Read(data,
                             offset,
                             file.Filesize - offset >= ImgHeader.FileBlockLength ? ImgHeader.FileBlockLength : (int)file.Filesize - offset);
                  }

                  return data;
               }
            }
         }
         return null;
      }

      /// <summary>
      /// erzeugt so viele Blöcke wie nötig, um die Blöcke für die Datei zu adressieren
      /// </summary>
      /// <param name="filename">Dateiname ("." für Root; "" oder null für einen ungenutzten Block)</param>
      /// <param name="filesize">Dateigröße</param>
      /// <param name="startblockno">erste Blocknummer für den Dateiinhalt</param>
      /// <returns></returns>
      List<FATBlock> buildFATEntry(string? filename, uint filesize, ref ushort startblockno) {
         List<FATBlock> lst = new List<FATBlock>();

         if (ImgHeader != null) {
            FATBlock bl = new FATBlock((uint)ImgHeader.FATBlockLength);

            if (!string.IsNullOrEmpty(filename)) {
               bl.Used = true;
               bl.Flag = (byte)(filename == "." ? 0x03 : 0x00);
               bl.FullName = filename;
               bl.Filesize = filesize;
               bl.Part = 0;

               int blocks4file = blocks4File((int)filesize);
               do {
                  bl.ClearBlockNumbers();
                  while (blocks4file > 0 &&
                         !bl.BlockTableIsFull) {
                     bl.AppendBlockNumber(startblockno++);
                     blocks4file--;
                  }
                  lst.Add(bl);

                  if (blocks4file > 0) {
                     bl = new FATBlock(bl);
                     bl.Part++;
                     bl.Filesize = 0;
                  }
               }
               while (blocks4file > 0);
            } else {
               bl.Used = false;

               lst.Add(bl);
            }
         }

         return lst;
      }

      /// <summary>
      /// testet, ob der Dateiname schon als interne Datei existiert
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public bool FilenameExist(string filename) {
         return FilenameIdx(filename) >= 0;
      }

      /// <summary>
      /// liefert den Index einer internen Datei (oder eine negative Zahl)
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public int FilenameIdx(string filename) {
         for (int i = 0; i < Files.Count; i++)
            if (Files[i].Name == filename)
               return i;
         return -1;
      }

      /// <summary>
      /// liefert die Dateianzahl der internen Dateien
      /// </summary>
      public int FileCount {
         get {
            return Files.Count;
         }
      }

      /// <summary>
      /// liefert den Dateinamen der internen Datei
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string Filename(int idx) {
         return Files[idx].Name;
      }

      /// <summary>
      /// liefert die Dateigröße der internen Datei
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public uint Filesize(int idx) {
         return Files[idx].Filesize;
      }

      /// <summary>
      /// setzt den internen Dateinamen neu
      /// </summary>
      /// <param name="oldfilename"></param>
      /// <param name="filename"></param>
      /// <returns></returns>
      public bool FileRename(string oldfilename, string filename) {
         int idx = FilenameIdx(oldfilename);
         if (idx >= 0 &&
             !FilenameExist(filename)) {
            Files[idx].Name = filename;
            return true;
         }
         return false;
      }

      /// <summary>
      /// fügt eine interne Datei hinzu
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="filesize"></param>
      /// <param name="pos">Pos.im Dateisystem</param>
      /// <returns></returns>
      public bool FileAdd(string filename, uint filesize, int pos = int.MaxValue) {
         if (!FilenameExist(filename)) {
            pos = Math.Max(0, Math.Min(pos, Files.Count));
            if (pos == Files.Count)
               Files.Add(new FileProps(filename, filesize, getBackFilename(filename)));
            else
               Files.Insert(pos, new FileProps(filename, filesize, getBackFilename(filename)));
            return true;
         }
         return false;
      }

      /// <summary>
      /// löscht eine interne Datei
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public bool FileDelete(string filename) {
         int idx = FilenameIdx(filename);
         if (idx >= 0) {
            if (File.Exists(Files[idx].Backgroundfile))
               File.Delete(Files[idx].Backgroundfile);
            Files.RemoveAt(idx);
            return true;
         }
         return false;
      }

      /// <summary>
      /// liefert die Basisdateinamen (aller internen TRE-Dateien, also i.A. 1)
      /// </summary>
      /// <returns></returns>
      public List<string> AllBasenames() {
         List<string> names = new List<string>();
         for (int i = 0; i < Files.Count; i++) {
            string extension = Files[i].Name.Substring(8);
            if (extension == ".TRE" ||
                extension == ".GMP")
               names.Add(Files[i].Name.Substring(0, 8));
         }
         return names;
      }

      /// <summary>
      /// liefert alle Dateinamen zu einem Basisdateinamen
      /// </summary>
      /// <param name="basename"></param>
      /// <returns></returns>
      public List<string> AllFilenames4Basename(string basename) {
         List<string> names = new List<string>();
         for (int i = 0; i < Files.Count; i++)
            if (Files[i].Name.Substring(0, 8) == basename)
               names.Add(Files[i].Name);
         return names;
      }

      /// <summary>
      /// liefert einen eigenen BinaryReaderWriter für die interne Datei oder null
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public BinaryReaderWriter? GetBinaryReaderWriter4File(string filename) {
         int idx = FilenameIdx(filename);
         if (idx >= 0 && ImgHeader != null) {
            MyStream stream = new MyStream(Files[idx].Backgroundfile, getFiledata(Files[idx], binreader), Files[idx], false);
            stream.NewSize += stream_NewSize;
            BinaryReaderWriter br = new BinaryReaderWriter(stream) {
               XOR = ImgHeader.XOR
            };
            return br;
         }
         return null;
      }

      /// <summary>
      /// liest das gesamte Dateisystem (aber ohne die Daten interner Dateien) ein und "behält" den <see cref="BinaryReaderWriter"/>
      /// <para>Der Dateiinhalt wird immer nur bei Bedarf eingelesen.</para>
      /// </summary>
      /// <param name="br"></param>
      public void Read(BinaryReaderWriter br) {
         binreader = br;

         // Header einlesen
         binreader.Seek(0);
         if (ImgHeader != null) {
            ImgHeader.Read(binreader);

            List<FATBlock> root = new List<FATBlock>();
            List<FATBlock> fat = new List<FATBlock>();

            // gesamte FAT einlesen
            int sumfatblocks = -1;
            while (sumfatblocks != 0) {
               FATBlock bl = new FATBlock((uint)ImgHeader.FATBlockLength);
               bl.Read(binreader);
               if (sumfatblocks < 0)
                  sumfatblocks = ((int)bl.Filesize - ImgHeader.HeaderLength) / ImgHeader.FATBlockLength;     // Anzahl der FAT-Blocks aus dem 1. Block ("Dateigröße") ermitteln

               if (bl.FullName == ".")
                  root.Add(bl);
               else
                  fat.Add(bl);
               sumfatblocks--;
            }

            // Dateiliste erzeugen
            file4block?.Clear();
            Files.Clear();
            preblocks4read = (UInt16)(root[0].Filesize / ImgHeader.FileBlockLength);     // Anzahl der Datenblöcke bis zum Start des echten Dateiinhaltbereiches
            if (root[0].Filesize % ImgHeader.FileBlockLength != 0)
               preblocks4read++;
            FATSize = (int)root[0].Filesize - ImgHeader.HeaderLength;

            for (int block = 0; block < fat.Count; block++) {
               FATBlock bl = fat[block];
               if (bl.Used) {
                  FileProps file;
                  if (bl.Part == 0) {
                     string name = bl.FullName;
                     if (name != ".") {
                        file = new FileProps(name, bl.Filesize, string.IsNullOrEmpty(backgroundpath) ? null : getBackFilename(name));
                        Files.Add(file);
                     }
                  }
                  int fileidx = Files.Count - 1;
                  file = Files[fileidx];
                  for (int j = 0; j < bl.BlockNumberCount; j++) {             // alle Blocknummern registrieren
                     UInt16 blockno = (UInt16)(bl.GetBlockNumber(j) - preblocks4read);
                     file.PseudoFileBlockAdd(blockno);                        // 0-basierte Blocknummern speichern
                     file4block?.Add(blockno, fileidx);
                  }
               }
            }
         }
      }

      /*
       * Der Header besteht aus x Sectoren der festen Länge SECTOR_BLOCKSIZE.
       * I.A. bildet 1 Sektor einen Block, so dass der Header aus y (=x) Blöcken besteht.
       * 
       * Der IMG-Dateibereich vom Dateianfang bis zum Ende der FAT wird als Pseudodatei aufgefasst die aus z Blöcken mit FileBlockLength besteht. 
       * Diese z Blöcke (0 .. z-1) müssen in der Root registriert sein. Sollte der Root-Größe dafür nicht ausreichen, muss sie entsprechend 
       * vergrößert werden. Damit vergrößert sich natürlich die Pseudodatei, d.h. die Root muss entsprechend mehr Einträge aufnehmen können.
       * Deshalb ist es zweckmäßig, die Root-Größe iterativ zu bestimmen.
       * 
       * Die FileBlockLength ist eine 2er Potenz von FATBlockLength (ev. also auch gleich).
       * 
       * 
       * 
       */

      /// <summary>
      /// schreibt die gesamte IMG-Datei
      /// </summary>
      /// <param name="wr"></param>
      public void Write(BinaryReaderWriter wr) {
         if (ImgHeader != null) {
            int prefileblocks = 0; // alle Blocks vor den fileblocks (Blocklänge: FileBlockLength)
            int fileblocks = 0; // alle Blocks für die Dateiinhalte (Blocklänge: FileBlockLength)

            int rootblocks = 1; // Blocks für die Root (Blocklänge: FATBlockLength)
            int fatblocks = 0; // Blocks für die FAT (Blocklänge: FATBlockLength)
            do {
               // Anzahl der Blöcke für die Dateidaten und die FAT bestimmen (abh. von den akt. Blockgrößen)
               fileblocks = 0;
               fatblocks = 0;
               foreach (FileProps file in Files) {
                  fileblocks += blocks4File((int)file.Filesize);
                  fatblocks += FATBlocks4File((int)file.Filesize);
               }
               // Anzahl der Rootblocks und Preblocks bestimmen
               int newrootblocks = 0;
               rootblocks = 1;
               do {
                  prefileblocks = blocks4File(ImgHeader.HeaderLength + (rootblocks + fatblocks) * ImgHeader.FATBlockLength);
                  newrootblocks = FATBlocks4File(prefileblocks * ImgHeader.FileBlockLength);
                  if (newrootblocks != rootblocks) // Platz in der aktuellen Root ist noch nicht ausreichend
                     rootblocks = newrootblocks;
                  else
                     break;
               } while (true);

               // ev. noch Blocklänge vergrößern
               if (prefileblocks + fileblocks > 0xffff) // mehr Blöcke können im Header nicht angegeben werden (UInt16) -> Vergrößerung der Blockgröße nötig
                  ImgHeader.FileBlockLength *= 2;
               /* Blockgröße    max. Dateigröße
                *   512 Byte ->   33553920 Byte, etwa 32 MB
                *  1024 Byte ->   67107840 Byte, etwa 64 MB
                *  2048 Byte ->  134215680 Byte, etwa 127 MB
                *  4096 Byte ->  268431360 Byte, etwa 256 MB
                *  8192 Byte ->  536862720 Byte, etwa 512 MB
                * 16384 Byte -> 1073725440 Byte, etwa 1024 MB, 1GB
                * 32768 Byte -> 2147450880 Byte, etwa 2048 MB, 2GB
                * 65536 Byte -> 4294901760 Byte, etwa 4096 MB, 4GB
                */
               else {
                  ImgHeader.Blocks4Img = (UInt16)(prefileblocks + fileblocks);
                  break;
               }
            } while (true);

            // Header schreiben
            wr.Seek(0);
            ImgHeader.Write(wr);

            UInt16 block = 0;
            // Root schreiben (Datei ".")
            List<FATBlock> fatbl = buildFATEntry(".", (uint)(prefileblocks * ImgHeader.FileBlockLength), ref block);
            foreach (FATBlock item in fatbl)
               item.Write(wr);

            // die eigentliche FAT schreiben
            block = (UInt16)prefileblocks; // 1. Blocknummer des Dateibereiches
            foreach (FileProps file in Files) {
               fatbl = buildFATEntry(file.Name, file.Filesize, ref block);
               foreach (FATBlock item in fatbl)
                  item.Write(wr);
            }

            // ev. leere, ungenutzte Blöcke schreiben
            long filestart = prefileblocks * ImgHeader.FileBlockLength;
            while (wr.Position < filestart) {
               fatbl = buildFATEntry(null, 0, ref block);
               foreach (FATBlock item in fatbl)
                  item.Write(wr);
            }

            FATSize = (int)wr.Position - ImgHeader.HeaderLength;

            // Daten schreiben
            foreach (FileProps file in Files) {
               byte[]? data = getFiledata(file, binreader);
               int offset = 0;
               if (data != null)
                  do {
                     if (offset + ImgHeader.FileBlockLength <= data.Length)   // vollständigen Block schreiben
                        wr.Write(data, offset, ImgHeader.FileBlockLength);
                     else {
                        wr.Write(data, offset, data.Length - offset);         // restliche Bytes schreiben
                        int i = data.Length % ImgHeader.FileBlockLength;
                        for (; i < ImgHeader.FileBlockLength; i++)            // ungenutzte Bytes mit 0x00 füllen
                           wr.Write((byte)0x00);
                     }
                     offset += ImgHeader.FileBlockLength;
                  } while (offset < data.Length);
            }
         }
      }

      public override string ToString() {
         return string.Format("{0}, Dateien: {1}, Backgroundpath: {2}", ImgHeader, FileCount, backgroundpath);
      }


      ~SimpleFilesystem() {
         Dispose(false);
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)
            foreach (FileProps file in Files)
               file.Dispose();
            if (backgroundpathcreated)
               try {
                  if (backgroundpath != null)
                     Directory.Delete(backgroundpath, true);
               } finally { // Fehler ignorieren

               }

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
