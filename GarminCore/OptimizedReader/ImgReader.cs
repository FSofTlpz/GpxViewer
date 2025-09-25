using GarminCore.DskImg;
using System;
using System.Collections.Generic;

namespace GarminCore.OptimizedReader {

   /// <summary>
   /// IMG-Datei mit ihrem Dateisystem
   /// </summary>
   public class ImgReader : IDisposable {

      /// <summary>
      /// Header der IMG-Datei
      /// </summary>
      Header? ImgHeader;

      /// <summary>
      /// Größe der FAT (einschließlich Root) in Byte
      /// </summary>
      public int FATSize { get; private set; }


      /// <summary>
      /// interne Daten einer Datei
      /// </summary>
      protected class FileProps : IDisposable {

         string _Name = "";

         /// <summary>
         /// Name der Datei
         /// </summary>
         public string Name {
            get => _Name;
            protected set => _Name = FATBlock.GetValidFullname(value);
         }

         /// <summary>
         /// Dateilänge
         /// </summary>
         public readonly uint Filesize;

         /// <summary>
         /// liefert die Anzahl der Pseudoblocks
         /// </summary>
         public int PseudoFileBlockCount {
            get {
               return PseudoFileBlockList.Count;
            }
         }

         /// <summary>
         /// belegte Pseudo-Datenblöcke [0...] (Position je Blocknummer)
         /// </summary>
         readonly SortedDictionary<ushort, int> PseudoFileBlockList;


         public FileProps(string name, uint filesize) {
            Name = FATBlock.GetValidFullname(name);
            Filesize = filesize;
            PseudoFileBlockList = new SortedDictionary<ushort, int>();
         }

         /// <summary>
         /// fügt einen Block and die Pseudoblock-Liste an
         /// </summary>
         /// <param name="blockno"></param>
         public void PseudoFileBlockAdd(UInt16 blockno) {
            PseudoFileBlockList.Add(blockno, PseudoFileBlockList.Count);
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

         public ushort FirstPseudoBlockNo {
            get {
               ushort[] blocks = PseudoFileBlocks();
               return blocks[0];
            }
         }

         public override string ToString() {
            return string.Format("{0}, {1} Bytes", Name, Filesize);
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
      Dictionary<UInt16, int> file4block = new Dictionary<ushort, int>();

      /// <summary>
      /// Anzahl der Blöcke vor den Datei-Datenblöcken in der IMG-Datei (für das Lesen der Daten)
      /// </summary>
      protected int preblocks4read;

      /// <summary>
      /// BinaryReaderWriter für die zu lesende IMG-Datei
      /// </summary>
      protected BinaryReaderWriter? binreader;

      /// <summary>
      /// liefert die Dateianzahl der internen Dateien
      /// </summary>
      public int FileCount => Files.Count;



      /// <summary>
      /// IMG-Datei-Verwaltung
      /// </summary>
      /// <param name="filename"></param>
      public ImgReader(string filename) {
         init();
         read(new BinaryReaderWriter(filename, true, false, false)); // nur zum Lesen und Share.Read
      }

      public ImgReader(BinaryReaderWriter reader) {
         init();
         read(reader);
      }

      protected void init() {
         ImgHeader = new Header {
            FileBlockLength = 0x200,
            FATBlockLength = 0x200,
            HeadSectors = 2
         };

         Files = new List<FileProps>();
         file4block = new Dictionary<ushort, int>();
         preblocks4read = -1;
         binreader = null;
      }

      /// <summary>
      /// liest das gesamte Dateisystem (aber ohne die Daten der interner Dateien) ein und "behält" den <see cref="BinaryReaderWriter"/> intern
      /// <para>Der Dateiinhalt wird immer nur bei Bedarf eingelesen.</para>
      /// </summary>
      /// <param name="br"></param>
      void read(BinaryReaderWriter br) {
         binreader = br;

         if (ImgHeader != null) {
            // Header einlesen
            binreader.Seek(0);
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
            file4block.Clear();
            Files.Clear();
            preblocks4read = (UInt16)(root[0].Filesize / ImgHeader.FileBlockLength);     // Anzahl der Datenblöcke bis zum Start des echten Dateiinhaltbereiches
            if (root[0].Filesize % ImgHeader.FileBlockLength != 0)
               preblocks4read++;
            FATSize = (int)root[0].Filesize - ImgHeader.HeaderLength;

            for (int block = 0; block < fat.Count; block++) {
               FATBlock bl = fat[block];
               if (bl.Used) {
                  FileProps fileprops;
                  if (bl.Part == 0) {
                     string name = bl.FullName;
                     if (name != ".") {
                        fileprops = new FileProps(name, bl.Filesize);
                        Files.Add(fileprops);
                     }
                  }
                  int fileidx = Files.Count - 1;
                  fileprops = Files[fileidx];
                  for (int j = 0; j < bl.BlockNumberCount; j++) {             // alle Blocknummern registrieren
                     UInt16 blockno = (UInt16)(bl.GetBlockNumber(j) - preblocks4read);
                     fileprops.PseudoFileBlockAdd(blockno);                   // 0-basierte Blocknummern speichern
                     file4block.Add(blockno, fileidx);
                  }
               }
            }
         }
      }

      /// <summary>
      /// liefert den Dateinamen der internen Datei
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public string Filename(int idx) => Files[idx].Name;

      /// <summary>
      /// liefert die Dateigröße der internen Datei
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public uint Filesize(int idx) => Files[idx].Filesize;

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
      /// testet, ob der Dateiname schon als interne Datei existiert
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public bool FilenameExist(string filename) => FilenameIdx(filename) >= 0;

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
      /// liefert einen eigenen BinaryReaderWriter für den Datenbereich der Datei
      /// <para>Dieser basiert auf dem zugrunde liegenden <see cref="BinaryReaderWriter"/>und arbeitet deshalb entweder
      /// direkt auf der Datei oder dem Datenpuffer.</para>
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public BinaryReaderWriter? GetBinaryReaderWriter4File(string filename) {
         int idx = FilenameIdx(filename);
         if (idx >= 0 &&
             binreader != null &&
             ImgHeader != null) {
            if (binreader.InMemoryData != null) {
               return new BinaryReaderWriter(binreader.InMemoryData,
                                             (int)ImgHeader.FileBlockLength * (preblocks4read + Files[idx].FirstPseudoBlockNo),
                                             (int)Files[idx].Filesize,
                                             null,
                                             false);
            } else {
               BinaryReaderWriter reader = new BinaryReaderWriter(binreader);
               reader.Offset4Part = ImgHeader.FileBlockLength * (preblocks4read + Files[idx].FirstPseudoBlockNo);
               reader.Position = 0;
               reader.Length4Part = Files[idx].Filesize;
               return reader;
            }
         }
         return null;
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

      public override string ToString() {
         return string.Format("{0}, Dateien: {1}", ImgHeader, FileCount);
      }


      ~ImgReader() {
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
               foreach (FileProps fileprops in Files)
                  fileprops.Dispose();

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
