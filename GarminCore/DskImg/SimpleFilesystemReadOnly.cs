using System;

namespace GarminCore.DskImg {

   /// <summary>
   /// Nur für den lesenden Zugriff: Die IMG-Datei wird einmal komplette in den Hauptspeicher eingelesen. Die <see cref="BinaryReaderWriter"/> für die die
   /// einzelnen internen Datei basieren auf auf MemoryStreams aud diesen Hauptspeicherbereich.
   /// <para>Das sollte für reine Leseoperationen auf Tilemap-IMG's effektiver sein.</para>
   /// </summary>
   public class SimpleFilesystemReadOnly : SimpleFilesystem {

      /// <summary>
      /// liest die vollständige IMG-Datei in den Hauptspeicher ein
      /// </summary>
      /// <param name="imgfile"></param>
      public SimpleFilesystemReadOnly(string imgfile) :
         base(null, false) {
         Read(new BinaryReaderWriter(imgfile));
      }

      public new BinaryReaderWriter? GetBinaryReaderWriter4File(string filename) {
         int idx = FilenameIdx(filename);
         if (idx >= 0 &&
             binreader != null &&
             ImgHeader != null) {
            if (binreader.IsFixedLengthMemoryStream) {
               BinaryReaderWriter? br = null;
               // Test, ob die Datenblöcke alle der Reihe nach nacheinander folgen:
               bool safe = true;
               UInt16[] blocks = Files[idx].PseudoFileBlocks(); // Bockliste dieser Datei
               for (int i = 1; i < blocks.Length; i++) {
                  if (blocks[i] != blocks[i - 1] + 1) {
                     safe = false;                          // Blöcke NICHT direkt hintereinander
                     break;
                  }
               }

               if (safe) { // KEINE Kopie der Daten nötig -> direkt aus dem Speicher lesen
                  if (binreader.InMemoryData != null)
                     br = new BinaryReaderWriter(binreader.InMemoryData,
                                                 ImgHeader.FileBlockLength * (preblocks4read + blocks[0]),
                                                 (int)Files[idx].Filesize,
                                                 null,
                                                 false);
               } else { // Kopie der Dateidaten erzeugen
                  byte[]? buff = getFiledata(Files[idx], binreader);
                  if (buff != null)
                     br = new BinaryReaderWriter(buff,
                                                 0,
                                                 (int)Files[idx].Filesize,
                                                 null,
                                                 false);
               }
               if (br != null)
                  br.XOR = ImgHeader.XOR;
               return br;
            }
         }
         return null;
      }

      #region "blockierte" Originalfkt.

      public new bool FileRename(string oldfilename, string filename) { return false; }

      public new bool FileAdd(string filename, uint filesize, int pos = int.MaxValue) { return false; }

      public new bool FileDelete(string filename) { return false; }

      public new void Write(BinaryReaderWriter wr) { }

      #endregion

      ~SimpleFilesystemReadOnly() {
         Dispose(false);
      }

      #region Implementierung der IDisposable-Schnittstelle

      bool _isdisposed = false;

      protected override void Dispose(bool notfromfinalizer) {
         if (!_isdisposed) {
            if (notfromfinalizer) { // nur dann alle managed Ressourcen freigeben

            }

            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;
            base.Dispose(notfromfinalizer);
         }
      }

      #endregion

   }
}
