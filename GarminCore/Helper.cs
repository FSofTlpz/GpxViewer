using System;
using System.IO;
using System.Text;

namespace GarminCore {

   /// <summary>
   /// Hilfsfunktionen
   /// </summary>
   public class Helper {

      /// <summary>
      /// erzeugt einen StringBuilder mit den Hex-Werten des Byte-Arrays
      /// </summary>
      /// <param name="buff">Daten</param>
      /// <param name="start">Index des 1. Bytes</param>
      /// <param name="length">Länge des Bereiches</param>
      /// <param name="bytesperline">wenn größer 0 wird für die entsprechende Anzahl Bytes jeweils eine neue Zeile verwendet</param>
      static public StringBuilder DumpMemory(byte[] buff, int start = 0, int length = -1, int bytesperline = -1) {
         StringBuilder dump = new StringBuilder();
         if (buff != null && buff.Length > 0) {
            start = Math.Max(0, Math.Min(start, buff.Length - 1));
            if (length <= 0)
               length = buff.Length;
            length = Math.Max(0, Math.Min(length, buff.Length - start));
            for (int i = start; i < start + length; i++) {
               if (i == start ||
                  (bytesperline > 0 && (i - start) % bytesperline == 0)) {
                  if (i != start)
                     dump.AppendLine();
                  dump.AppendFormat("{0:x2}", buff[i]);
               } else
                  dump.AppendFormat(" {0:x2}", buff[i]);
            }
         }
         return dump;
      }

      /// <summary>
      /// erzeugt einen StringBuilder mit den Hex-Werten des <see cref="BinaryReaderWriter"/> 
      /// </summary>
      /// <param name="br"></param>
      /// <param name="start">Index des 1. Bytes</param>
      /// <param name="length">Länge des Bereiches</param>
      /// <param name="bytesperline">wenn größer 0 wird für die entsprechende Anzahl Bytes jeweils eine neue Zeile verwendet</param>
      /// <returns></returns>
      static public StringBuilder DumpMemory(BinaryReaderWriter br, int start = -1, int length = -1, int bytesperline = -1) {
         if (start >= 0)
            br.Seek(start);
         byte[] buff = br.ReadBytes(length > 0 ? length : (int)(br.Length - br.Position));
         return DumpMemory(buff, 0, buff.Length, bytesperline);
      }

      /// <summary>
      /// erzeugt einen StringBuilder mit den Hex-Werten des <see cref="BinaryReaderWriter"/> 
      /// </summary>
      /// <param name="br"></param>
      /// <param name="block">Index des 1. Bytes und Länge des Bereichs</param>
      /// <param name="bytesperline">wenn größer 0 wird für die entsprechende Anzahl Bytes jeweils eine neue Zeile verwendet</param>
      /// <returns></returns>
      static public StringBuilder DumpMemory(BinaryReaderWriter br, DataBlock block, int bytesperline = -1) {
         return DumpMemory(br, (int)block.Offset, (int)block.Length, bytesperline);
      }


      /// <summary>
      /// ab der der Position des Start-Bits im Byte-Array wird die benötigte Anzahl Bits (als komplette Bytes) eingelesen
      /// und als Text aufbereitet
      /// </summary>
      /// <param name="buff"></param>
      /// <param name="start">Anzahl der zu ignorierenden Bits ab der akt. Byteposition</param>
      /// <param name="length">wenn größer 0 die Anzahl der Bits</param>
      /// <param name="bitsperline">wenn größer 0 die Anzahl der Bits je Textzeile</param>
      /// <param name="low2high">je Byte beginnend mit Bit 0 oder 7</param>
      /// <returns></returns>
      static public StringBuilder Dumpstream(byte[] buff, int start = 0, int length = -1, int bitsperline = 8, bool low2high = false) {
         StringBuilder dump = new StringBuilder();
         if (buff != null && buff.Length > 0) {
            if (length < 0)
               length = buff.Length * 8 - start;
            if (start < 0)
               start = 0;

            int bitcount = 0;
            for (int byteno = start / 8;
                 byteno < buff.Length;
                 byteno++) {
               byte actbyte = buff[byteno];
               if (!low2high)          // Bitreihenfolge "umdrehen"
                  actbyte = (byte)(((actbyte & 0x01) << 7) |
                                   ((actbyte & 0x02) << 5) |
                                   ((actbyte & 0x04) << 3) |
                                   ((actbyte & 0x08) << 1) |
                                   ((actbyte & 0x10) >> 1) |
                                   ((actbyte & 0x20) >> 3) |
                                   ((actbyte & 0x40) >> 5) |
                                   ((actbyte & 0x80) >> 7));

               for (int bit = start % 8;
                    bit < 8 && bitcount < length;
                    bit++) {
                  byte mask = 0;
                  switch (bit) {
                     case 0: mask = 0x01; break;
                     case 1: mask = 0x02; break;
                     case 2: mask = 0x04; break;
                     case 3: mask = 0x08; break;
                     case 4: mask = 0x10; break;
                     case 5: mask = 0x20; break;
                     case 6: mask = 0x40; break;
                     case 7: mask = 0x80; break;
                  }
                  dump.Append((actbyte & mask) != 0 ? "1" : "0");
                  bitcount++;
                  if (bitsperline > 0 &&
                      bitcount % bitsperline == 0)
                     dump.AppendLine();
               }
            }
         }
         return dump;
      }

      /// <summary>
      /// ab der der Position des Start-Bits im <see cref="BinaryReaderWriter"/> wird die benötigte Anzahl Bits (als komplette Bytes) eingelesen
      /// und als Text aufbereitet
      /// </summary>
      /// <param name="br"></param>
      /// <param name="start">Anzahl der zu ignorierenden Bits ab der akt. Byteposition</param>
      /// <param name="length">wenn größer 0 die Anzahl der Bits</param>
      /// <param name="bitsperline">wenn größer 0 die Anzahl der Bits je Textzeile</param>
      /// <param name="low2high">je Byte beginnend mit Bit 0 oder 7</param>
      /// <returns></returns>
      static public StringBuilder Dumpstream(BinaryReaderWriter br, int start = -1, int length = -1, int bitsperline = 8, bool low2high = false) {
         if (start >= 0)
            br.Seek(start);
         byte[] buff = br.ReadBytes(length > 0 ? 
                                          1 + length / 8 : 
                                          (int)(br.Length - br.Position));
         return Dumpstream(br, 0, length, bitsperline, low2high);
      }


      protected static Encoding stringenc = Encoding.GetEncoding(1252); //new ASCIIEncoding();

      /// <summary>
      /// Klasse zum 1 oder mehrmaligen patchen einer Datei
      /// </summary>
      public class Patcher : IDisposable {

         FileStream fs;
         BinaryWriter bw;

         public Patcher(string filename) {
            fs = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.None);
            bw = new BinaryWriter(fs);
         }

         public void Patch(int addr, byte v) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(v);
         }

         public void Patch(int addr, UInt16 v) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(v);
         }

         public void Patch(int addr, Int16 v) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(v);
         }

         public void Patch(int addr, UInt32 v) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(v);
         }

         public void Patch(int addr, Int32 v) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(v);
         }

         public void Patch(int addr, byte[] v) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(v);
         }

         public void Patch(int addr, string v, Encoding? encoder = null, bool bEnding0 = true) {
            bw.Seek(addr, SeekOrigin.Begin);
            bw.Write(encoder == null ? stringenc.GetBytes(v) : encoder.GetBytes(v));
            if (bEnding0)
               bw.Write((byte)0);
         }

         #region statische Funktion (für das einmalige patchen einer Datei)

         static public void Patch(string filename, int addr, byte v) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v);
            p.Dispose();
         }

         static public void Patch(string filename, int addr, UInt32 v) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v);
            p.Dispose();
         }

         static public void Patch(string filename, int addr, Int32 v) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v);
            p.Dispose();
         }

         static public void Patch(string filename, int addr, UInt16 v) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v);
            p.Dispose();
         }

         static public void Patch(string filename, int addr, Int16 v) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v);
            p.Dispose();
         }

         static public void Patch(string filename, int addr, byte[] v) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v);
            p.Dispose();
         }

         static public void Patch(string filename, int addr, string v, Encoding? encoder = null, bool bEnding0 = true) {
            Patcher p = new Patcher(filename);
            p.Patch(addr, v, encoder, bEnding0);
            p.Dispose();
         }

         #endregion

         public void Close() {
            if (bw != null) {
               bw.Close();
               if (fs != null) {
                  fs.Close();
                  fs.Dispose();
               }
               bw.Dispose();
            }
         }

         ~Patcher() {
            Dispose(false);
         }

         #region Implemetierung der IDisposable-Schnittstelle

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
               Close();
               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion
      }






   }
}
