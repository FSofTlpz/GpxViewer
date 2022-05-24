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

namespace GarminCore {

   /// <summary>
   /// Basisklasse zum Lesen und Schreiben der Daten
   /// <para>Die Klasse ähnelt den Klassen BinaryReader und BinaryWriter und stellt einfache Lese- und Schreibfunktionen für rudimentäre
   /// Datenelemente bereit.</para>
   /// </summary>
   public class BinaryReaderWriter : IDisposable {

      readonly byte[] m_buffer = new byte[8];
      char[] m_1char_buffer = new char[1];

      /// <summary>
      /// falls die Daten XOR'd sind (NUR ZUM LESEN)
      /// </summary>
      public byte XOR { get; set; }

      /// <summary>
      /// Basiert der <see cref="BinaryReaderWriter"/> auf einem <see cref="MemoryStream"/> fester Länge?
      /// </summary>
      public bool IsFixedLengthMemoryStream {
         get {
            return InMemoryData != null;
         }
      }

      /// <summary>
      /// Daten für einen für <see cref="MemoryStream"/> fester Länge
      /// </summary>
      public byte[] InMemoryData { get; protected set; }


      /// <summary>
      /// erzeugt ein Objekt, das auf dem Stream basiert
      /// </summary>
      /// <param name="stream"></param>
      /// <param name="encoding"></param>
      public BinaryReaderWriter(Stream stream, Encoding encoding = null) {
         XOR = 0;
         BaseStream = stream;
         if (encoding != null)
            StandardEncoding = encoding;
      }

      /// <summary>
      /// erzeugt ein Objekt, das auf einem FileStream basiert
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="read"></param>
      /// <param name="write"></param>
      /// <param name="encoding"></param>
      public BinaryReaderWriter(string filename, bool read, bool write = false, bool create = false, Encoding encoding = null) {
         XOR = 0;
         if (read) {
            if (write)
               BaseStream = File.Open(filename, create ? FileMode.Create : FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            else
               BaseStream = File.Open(filename, create ? FileMode.Create : FileMode.Open, FileAccess.Read, FileShare.Read);
         } else
            BaseStream = File.Open(filename, create ? FileMode.Create : FileMode.Open, FileAccess.Write, FileShare.None);
         if (encoding != null)
            StandardEncoding = encoding;
      }

      /// <summary>
      /// erzeugt ein Objekt, das auf einem MemoryStream variabler Länge basiert
      /// </summary>
      /// <param name="encoding"></param>
      public BinaryReaderWriter(Encoding encoding = null) {
         XOR = 0;
         BaseStream = new MemoryStream();
         if (encoding != null)
            StandardEncoding = encoding;
      }

      /// <summary>
      /// erzeugt ein Objekt, das auf einem MemoryStream für das vorgegebene Byte-Array basiert
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="startindex"></param>
      /// <param name="count"></param>
      /// <param name="encoding"></param>
      public BinaryReaderWriter(byte[] buffer, int startindex, int count, Encoding encoding = null, bool writable = true) {
         XOR = 0;
         InMemoryData = buffer;
         BaseStream = new MemoryStream(InMemoryData, startindex, count, writable);
         if (encoding != null)
            StandardEncoding = encoding;
      }

      /// <summary>
      /// erzeugt ein Objekt, das auf einem MemoryStream für die vollständig eingelesene Datei basiert
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="encoding"></param>
      public BinaryReaderWriter(string filename, Encoding encoding = null) {
         XOR = 0;
         FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
         InMemoryData = new byte[fs.Length];
         fs.Read(InMemoryData, 0, InMemoryData.Length);
         fs.Close();

         BaseStream = new MemoryStream(InMemoryData, 0, InMemoryData.Length, false);

         if (encoding != null)
            StandardEncoding = encoding;
      }

      ~BinaryReaderWriter() {
         Dispose(false);
      }

      /// <summary>
      /// liefert den zu Grunde legenden Basisstream
      /// </summary>
      public Stream BaseStream { get; private set; }

      /// <summary>
      /// liefert oder setzt das Standard-Encoding
      /// </summary>
      public Encoding StandardEncoding { get; set; } = Encoding.GetEncoding(1252);

      /// <summary>
      /// setzt eine neue Standard-Codierung
      /// </summary>
      /// <param name="codePage"></param>
      public void SetEncoding(int codePage) {
         StandardEncoding = Encoding.GetEncoding(codePage);
      }

      /// <summary>
      /// setzt eine neue Stabdard-Codierung
      /// </summary>
      /// <param name="codePage"></param>
      public void SetEncoding(string codePage) {
         StandardEncoding = Encoding.GetEncoding(codePage);
      }

      /// <summary>
      /// akt. Länge des Streams
      /// </summary>
      public long Length {
         get {
            return BaseStream.Length;
         }
         set {
            BaseStream.SetLength(value);
         }
      }

      /// <summary>
      /// akt. Position im Stream
      /// </summary>
      public long Position {
         get {
            return BaseStream.Position;
         }
         set {
            BaseStream.Position = value;
         }
      }

      /// <summary>
      /// Anzahl Bytes von der akt. Position bis zum Ende
      /// </summary>
      public long LeftBytes {
         get {
            return BaseStream.Length - BaseStream.Position;
         }
      }

      /// <summary>
      /// Seek bzgl. des Streamanfangs
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public long Seek(long pos, SeekOrigin where = SeekOrigin.Begin) {
         return BaseStream.Seek(pos, where);
      }

      public void Flush() {
         BaseStream.Flush();
      }

      /// <summary>
      /// schreibt einen int als 3-Byte-Wert
      /// </summary>
      /// <param name="wr"></param>
      /// <param name="v"></param>
      public void Write3(Int32 v) {
         Write3((UInt32)(v >= 0 ? v : 0x1000000 + v));
      }
      /// <summary>
      /// schreibt einen uint als 3-Byte-Wert
      /// </summary>
      /// <param name="wr"></param>
      /// <param name="v"></param>
      public void Write3(UInt32 v) {
         Write((UInt16)(v & 0xffff));
         Write((byte)((v >> 16) & 0xff));
      }

      #region spez. Member-Lesefkt. für den bequemeren Zugriff auf Daten (greifen auf die Kernfkt. zurück)

      /// <summary>
      /// liefert 1 Byte (unsigned)
      /// </summary>
      /// <returns></returns>
      public byte ReadByte() {
         return ReadByte(this);
      }

      /// <summary>
      /// erzeugt und füllt den Byte-Puffer
      /// </summary>
      /// <param name="count"></param>
      /// <returns></returns>
      public byte[] ReadBytes(int count) {
         return ReadBytes(new byte[count]);
      }

      /// <summary>
      /// füllt den Byte-Puffer
      /// </summary>
      /// <param name="buff"></param>
      /// <returns></returns>
      public byte[] ReadBytes(byte[] buff) {
         return ReadBytes(this, buff);
      }

      public int Read1UInt() {
         return Read1UInt(this);
      }

      public int Read1Int() {
         return Read1Int(this);
      }

      public int Read2UInt() {
         return Read2UInt(this);
      }

      public int Read2Int() {
         return Read2Int(this);
      }

      public int Read3UInt() {
         return Read3UInt(this);
      }

      public int Read3Int() {
         return Read3Int(this);
      }

      public int Read4Int() {
         return Read4Int(this);
      }

      public uint Read4UInt() {
         return Read4UInt(this);
      }

      /// <summary>
      /// konvertiert nur das Ergebnis von <see cref="Read3UInt"/> in uint
      /// </summary>
      /// <returns></returns>
      public uint Read3AsUInt() {
         return (uint)Read3UInt(this);
      }

      /// <summary>
      /// konvertiert nur das Ergebnis von <see cref="Read2UInt"/> in ushort
      /// </summary>
      /// <returns></returns>
      public ushort Read2AsUShort() {
         return (ushort)Read2UInt(this);
      }

      /// <summary>
      /// konvertiert nur das Ergebnis von <see cref="Read2Int"/> in short
      /// </summary>
      /// <returns></returns>
      public short Read2AsShort() {
         return (short)Read2Int(this);
      }

      //public long ReadInt64() {
      //   basestream.Read(m_buffer, 0, 8);
      //   if (XOR != 0)
      //      for (int i = 0; i < 8; i++)
      //         m_buffer[i] ^= XOR;
      //   uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 |
      //                    m_buffer[2] << 16 | m_buffer[3] << 24);
      //   uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 |
      //                    m_buffer[6] << 16 | m_buffer[7] << 24);
      //   return (long)((ulong)hi) << 32 | lo;
      //}

      //public ulong ReadUInt64() {
      //   basestream.Read(m_buffer, 0, 8);
      //   if (XOR != 0)
      //      for (int i = 0; i < 8; i++)
      //         m_buffer[i] ^= XOR;
      //   uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 |
      //                    m_buffer[2] << 16 | m_buffer[3] << 24);
      //   uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 |
      //                    m_buffer[6] << 16 | m_buffer[7] << 24);
      //   return ((ulong)hi) << 32 | lo;
      //}

      #endregion

      #region spez. static-Lesefkt. für den bequemeren Zugriff auf Daten (greifen auf die Kernfkt. zurück)

      /// <summary>
      /// liefert 1 Byte als unsigned Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read1UInt(BinaryReaderWriter br) {
         return ReadByte(br);
      }

      /// <summary>
      /// liefert 1 Byte als signed Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read1Int(BinaryReaderWriter br) {
         int v = br.ReadByte();
         if (v >= 0x80) // Bit 7 = 1 -> negativ
            v -= 0x100;
         return v;
      }

      /// <summary>
      /// liefert 2 Byte als unsigned Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read2UInt(BinaryReaderWriter br) {
         return ReadByte(br) + (ReadByte(br) << 8);
      }

      /// <summary>
      /// liefert 2 Byte als signed Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read2Int(BinaryReaderWriter br) {
         int v = ReadByte(br) + (ReadByte(br) << 8);
         if (v >= 0x8000)
            v -= 0x10000;
         return v;
      }

      /// <summary>
      /// liefert 3 Byte als unsigned Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read3UInt(BinaryReaderWriter br) {
         return ReadByte(br) + (ReadByte(br) << 8) + (ReadByte(br) << 16);
      }

      /// <summary>
      /// liefert 3 Byte als signed Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read3Int(BinaryReaderWriter br) {
         int v = ReadByte(br) + (ReadByte(br) << 8) + (ReadByte(br) << 16);
         if (v >= 0x800000)
            v -= 0x1000000;
         return v;
      }

      /// <summary>
      /// liefert 4 Byte als unsigned Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public uint Read4UInt(BinaryReaderWriter br) {
         return (uint)(ReadByte(br) + ((long)ReadByte(br) << 8) + ((long)ReadByte(br) << 16) + ((long)ReadByte(br) << 24));
      }

      /// <summary>
      /// liefert 4 Byte als signed Wert
      /// </summary>
      /// <param name="br"></param>
      /// <returns></returns>
      static public int Read4Int(BinaryReaderWriter br) {
         long v = ReadByte(br) + (ReadByte(br) << 8) + (ReadByte(br) << 16) + (ReadByte(br) << 24);
         if (v >= 0x80000000)
            v -= 0x100000000;
         return (int)v;
      }

      #endregion

      #region static-Kernfkt. zum Lesen von Daten

      /// <summary>
      /// Basisfkt zum Füllen des Byte-Puffer
      /// </summary>
      /// <param name="buff"></param>
      /// <returns></returns>
      static public byte[] ReadBytes(BinaryReaderWriter br, byte[] buff) {
         br.BaseStream.Read(buff, 0, buff.Length);
         if (br.XOR != 0)
            for (int i = 0; i < buff.Length; i++)
               buff[i] ^= br.XOR;
         return buff;
      }

      /// <summary>
      /// Basisfkt. zum Lesen eines Bytes (ev. auch XORt; IOException am Streamende)
      /// </summary>
      /// <returns></returns>
      static public byte ReadByte(BinaryReaderWriter br) {
         int b = br.BaseStream.ReadByte() ^ br.XOR;
         if (b == -1)
            throw new IOException("End of file");
         return (byte)b;
      }

      #endregion


      //public unsafe double ReadFloat() {
      //   basestream.Read(m_buffer, 0, 8);
      //   uint tmp = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
      //   return *((float*)&tmp);
      //}

      //public unsafe double ReadDouble() {
      //   basestream.Read(m_buffer, 0, 8);
      //   uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
      //   uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 | m_buffer[6] << 16 | m_buffer[7] << 24);

      //   ulong tmpBuffer = ((ulong)hi) << 32 | lo;
      //   return *((double*)&tmpBuffer);
      //}

      /// <summary>
      /// liest ein einzelnes Zeichen aus dem Stream
      /// </summary>
      /// <param name="encoding"></param>
      /// <returns></returns>
      public char ReadChar(Encoding encoding = null) {
         if (ReadChar(encoding ?? StandardEncoding, ref m_1char_buffer))
            return m_1char_buffer[0];
         throw new EndOfStreamException();
      }

      /// <summary>
      /// liest (max.) die Anzahl Zeichen ein; bei 0 wird bis '\0' oder zum Streamende eingelesen 
      /// </summary>
      /// <param name="count"></param>
      /// <param name="encoding"></param>
      /// <returns></returns>
      public char[] ReadChars(int count = 0, Encoding encoding = null) {
         if (count > 0) {
            char[] full = new char[count];
            int chars = ReadCharBytes(encoding ?? StandardEncoding, full, count);

            if (chars == 0)
               return new char[0];

            if (chars != full.Length) {         // kleineres Array erzeugen
               char[] ret = new char[chars];
               System.Array.Copy(full, 0, ret, 0, chars);
               return ret;
            } else
               return full;
         } else {
            List<char> lst = new List<char>();
            while (ReadChar(encoding ?? StandardEncoding, ref m_1char_buffer))
               if (m_1char_buffer[0] == '\0')
                  break;
               else
                  lst.Add(m_1char_buffer[0]);
            return lst.ToArray();
         }
      }

      /// <summary>
      /// liest max. die gewünschte Anzahl Zeichen aus dem Stream in den Puffer ein
      /// </summary>
      /// <param name="encoding"></param>
      /// <param name="buffer">Zeichenpuffer</param>
      /// <param name="count">max. Anzahl der Zeichen (bei zu kleinem Puffer durch die Pufferlänge begrenzt); bei 0 wird bis '\0' eingelesen</param>
      /// <returns>liefert die Anzahl der gelesenen Zeichen</returns>
      int ReadCharBytes(Encoding encoding, char[] buffer, int count = 0) {
         int chars_read = 0;
         while ((chars_read < count || count <= 0) &&
                chars_read < buffer.Length) {
            if (ReadChar(encoding ?? StandardEncoding, ref m_1char_buffer))
               buffer[chars_read] = m_1char_buffer[0];
            else
               break;
            if (buffer[chars_read] == '\0')
               break;
            chars_read++;
         }
         return chars_read;
      }

      /// <summary>
      /// liest ein einzelnes Zeichen entsprechend der Codierung
      /// </summary>
      /// <param name="encoding"></param>
      /// <param name="ch"></param>
      /// <returns>true, wenn ein Zeichen gelesen wurde</returns>
      bool ReadChar(Encoding encoding, ref char[] ch) {
         int pos = 0;
         while (true) {    // ein einzelnes Zeichen ermitteln
            // Der Puffer muss nur für 1 Zeichen ausreichen. Dafür sollten 8 Byte mehr als genug sein.
            //CheckBuffer(pos + 1);
            int read_byte = BaseStream.ReadByte();
            if (read_byte == -1)    /* EOF */
               return false;
            m_buffer[pos++] = (byte)(((byte)read_byte) ^ XOR);
            // liefert: Die tatsächliche Anzahl der Zeichen, die in chars geschrieben werden.
            int n = encoding.GetChars(m_buffer,             // Das Bytearray, das die zu decodierende Bytefolge enthält. 
                                      0,                    // Der Index des ersten zu decodierenden Bytes. 
                                      pos,                  // Die Anzahl der zu decodierenden Bytes. 
                                      ch,                   // Das Zeichenarray, das die sich ergebenden Zeichen enthalten soll. 
                                      0);                   // Der Index, an dem mit dem Schreiben der sich ergebenden Zeichen begonnen werden soll. 
            if (n > 0)
               return true;
         }
      }

      /// <summary>
      /// Ensures that m_buffer is at least length bytes long, growing it if necessary
      /// </summary>
      /// <param name="length"></param>
      //void CheckBuffer(int length) {
      //   if (m_buffer.Length <= length) {
      //      byte[] new_buffer = new byte[length];
      //      Array.Copy(m_buffer, 0, new_buffer, 0, m_buffer.Length);
      //      m_buffer = new_buffer;
      //   }
      //}

      /// <summary>
      /// liest eine Zeichenkette bis zum 0-Byte oder bis die max. Länge erreicht ist
      /// </summary>
      /// <param name="br"></param>
      /// <param name="maxlen"></param>
      /// <param name="encoder"></param>
      /// <returns></returns>
      public string ReadString(int maxlen = 0, Encoding encoder = null) {
         List<byte> dat = new List<byte>();
         byte b;
         int len = maxlen > 0 ? maxlen : int.MaxValue;
         do {
            b = ReadByte();
            if (b != 0)
               dat.Add(b);
            len--;
         } while (b != 0 && len > 0);
         return encoder == null ? StandardEncoding.GetString(dat.ToArray()) : encoder.GetString(dat.ToArray());
      }

      /// <summary>
      /// liest die Datentabelle als Liste von UInt ein
      /// </summary>
      /// <param name="bl"></param>
      /// <returns></returns>
      public List<UInt32> ReadUintArray(DataBlockWithRecordsize bl) {
         List<UInt32> lst = new List<uint>();
         if (bl.Length > 0) {
            if (bl.Recordsize == 0)
               throw new Exception("Datensatzlänge 0 bei Blocklänge > 0 ist nicht erlaubt.");
            uint count = bl.Length / bl.Recordsize;
            if (count > 0) {
               Seek(bl.Offset);
               for (uint i = 0; i < count; i++) {
                  switch (bl.Recordsize) {
                     case 1:
                        lst.Add(ReadByte());
                        break;

                     case 2:
                        lst.Add(Read2AsUShort());
                        break;

                     case 3:
                        lst.Add(Read3AsUInt());
                        break;

                     case 4:
                        lst.Add(Read4UInt());
                        break;

                     default:
                        throw new Exception("Unbekanntes Integerformat.");
                  }
               }
            }
         }
         return lst;
      }

      /// <summary>
      /// abstrakte Basisklasse für Datenstrukturen
      /// </summary>
      abstract public class DataStruct {
         public abstract void Read(BinaryReaderWriter br, object data);
         public abstract void Write(BinaryReaderWriter bw, object data);
      }

      /// <summary>
      /// liest die Datentabelle als Liste von Datenstrukturen ein
      /// <para>Die Länge der Datenstrukturen muss vom neuen Objekt selbst bestimmt werden.</para>
      /// <para>Über <see cref="data"/> können beliebige Zusatzdaten mitgeliefert werden. Ist die Liste nicht vorhanden, wird null
      /// geliefert. Ist sie zu kurz wird das erste Listenelement geliefert. Eine Liste mit einem einzigen Element führt also
      /// dazu, dass immer die gleichen Zusatzdaten geliefert werden.</para>
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="bl">Datenbereich</param>
      /// <param name="extdata">Liste der Zusatzdaten oder null; enthält die Liste nur 1 Objekt, so wird dieses Objekt für ALLE eingelesenen Daten verwendet</param>
      /// <param name="offsets">Liste für die Speicherung der Offsets und des Datensatzindex bzgl. des Blocks</param>
      /// <returns></returns>
      public List<T> ReadArray<T>(DataBlock bl, IList<object> extdata = null, SortedList<uint, int> offsets = null) where T : DataStruct, new() {
         List<T> lst = new List<T>();
         if (bl.Length > 0) {
            uint start = bl.Offset;
            uint end = start + bl.Length;
            Seek(bl.Offset);
            if (offsets != null)
               offsets.Clear();
            int ds_data = 0;
            int ds_offs = 0;
            object constdata = extdata != null && extdata.Count > 0 ?
                                                      extdata[0] :
                                                      null;
            while (Position < end) {
               if (offsets != null)
                  offsets.Add((uint)Position - start, ds_offs++); // Offsets speichern

               T t = new T();
               t.Read(this, extdata != null && ds_data < extdata.Count ? extdata[ds_data++] : constdata);

               //try {
               //   t.Read(this, data != null && ds_data < data.Count ? data[ds_data++] : constdata);

               //} catch (Exception ex) {

               //   throw;
               //}

#if DEBUG
               if (Position > end)
                  Debug.WriteLine("Vermutlich Fehler bei SUB_File.ReadArray(). {0} Bytes zu viel gelesen.", Position - end);
#endif
               //Debug.WriteLineIf(br.BaseStream.Position > end, "Vermutlich Fehler bei SUB_File.ReadArray().");
               lst.Add(t);
            }

            if (extdata != null && ds_data++ < extdata.Count) // ev. noch mit Dummy-Objekten entsprechend der Größe der Datenliste auffüllen
               lst.Add(new T());
         }
         return lst;
      }


      public void Write(byte value) {
         BaseStream.WriteByte(value);
      }

      public void Write(byte[] value) {
         BaseStream.Write(value, 0, value.Length);
      }

      public void Write(byte[] value, int offset, int length) {
         BaseStream.Write(value, offset, length);
      }

      public void Write(bool value) {
         m_buffer[0] = (byte)(value ? 1 : 0);
         BaseStream.Write(m_buffer, 0, 1);
      }

      public void Write(short value) {
         m_buffer[0] = (byte)value;
         m_buffer[1] = (byte)(value >> 8);
         BaseStream.Write(m_buffer, 0, 2);
      }

      public void Write(ushort value) {
         m_buffer[0] = (byte)value;
         m_buffer[1] = (byte)(value >> 8);
         BaseStream.Write(m_buffer, 0, 2);
      }

      public void Write(int value) {
         m_buffer[0] = (byte)value;
         m_buffer[1] = (byte)(value >> 8);
         m_buffer[2] = (byte)(value >> 16);
         m_buffer[3] = (byte)(value >> 24);
         BaseStream.Write(m_buffer, 0, 4);
      }

      public void Write(uint value) {
         m_buffer[0] = (byte)value;
         m_buffer[1] = (byte)(value >> 8);
         m_buffer[2] = (byte)(value >> 16);
         m_buffer[3] = (byte)(value >> 24);
         BaseStream.Write(m_buffer, 0, 4);
      }

      public void Write(long value) {
         m_buffer[0] = (byte)value;
         m_buffer[1] = (byte)(value >> 8);
         m_buffer[2] = (byte)(value >> 16);
         m_buffer[3] = (byte)(value >> 24);
         m_buffer[4] = (byte)(value >> 32);
         m_buffer[5] = (byte)(value >> 40);
         m_buffer[6] = (byte)(value >> 48);
         m_buffer[7] = (byte)(value >> 56);
         BaseStream.Write(m_buffer, 0, 8);
      }

      public void Write(ulong value) {
         m_buffer[0] = (byte)value;
         m_buffer[1] = (byte)(value >> 8);
         m_buffer[2] = (byte)(value >> 16);
         m_buffer[3] = (byte)(value >> 24);
         m_buffer[4] = (byte)(value >> 32);
         m_buffer[5] = (byte)(value >> 40);
         m_buffer[6] = (byte)(value >> 48);
         m_buffer[7] = (byte)(value >> 56);
         BaseStream.Write(m_buffer, 0, 8);
      }

      //public unsafe void Write(float value) {
      //   ulong TmpValue = *(uint*)&value;
      //   m_buffer[0] = (byte)TmpValue;
      //   m_buffer[1] = (byte)(TmpValue >> 8);
      //   m_buffer[2] = (byte)(TmpValue >> 16);
      //   m_buffer[3] = (byte)(TmpValue >> 24);
      //   basestream.Write(m_buffer, 0, 4);
      //}

      //public unsafe void Write(double value) {
      //   ulong TmpValue = *(ulong*)&value;
      //   m_buffer[0] = (byte)TmpValue;
      //   m_buffer[1] = (byte)(TmpValue >> 8);
      //   m_buffer[2] = (byte)(TmpValue >> 16);
      //   m_buffer[3] = (byte)(TmpValue >> 24);
      //   m_buffer[4] = (byte)(TmpValue >> 32);
      //   m_buffer[5] = (byte)(TmpValue >> 40);
      //   m_buffer[6] = (byte)(TmpValue >> 48);
      //   m_buffer[7] = (byte)(TmpValue >> 56);
      //   basestream.Write(m_buffer, 0, 8);
      //}

      /// <summary>
      /// schreibt ein Zeichen in den Stream
      /// </summary>
      /// <param name="value"></param>
      /// <param name="encoding"></param>
      public void Write(char value, Encoding encoding = null) {
         if (encoding == null)
            encoding = StandardEncoding;
         m_1char_buffer[0] = value;
         Write(encoding.GetBytes(m_1char_buffer));
      }

      /// <summary>
      /// schreibt ein Zeichen-Array in den Stream
      /// </summary>
      /// <param name="value"></param>
      /// <param name="encoding"></param>
      public void Write(char[] value, Encoding encoding = null) {
         if (encoding == null)
            encoding = StandardEncoding;
         Write(encoding.GetBytes(value));
      }

      /// <summary>
      /// schreibt eine Zeichenkette (normalerweise mit abschließendem 0-Byte)
      /// </summary>
      /// <param name="wr"></param>
      /// <param name="text"></param>
      /// <param name="encoder"></param>
      /// <param name="bEnding0"></param>
      public void WriteString(string text, Encoding encoding = null, bool bEnding0 = true) {
         if (encoding == null)
            encoding = StandardEncoding;
         Write(encoding.GetBytes(text));
         if (bEnding0)
            Write((byte)0);
      }

      /// <summary>
      /// kopiert die restlichen Bytes des Streams
      /// </summary>
      /// <param name="brw"></param>
      public void CopyTo(BinaryReaderWriter brw) {
         CopyTo(brw.BaseStream);
      }

      /// <summary>
      /// kopiert die restlichen Bytes des Streams
      /// </summary>
      /// <param name="stream"></param>
      public void CopyTo(Stream stream) {
         BaseStream.Seek(0, SeekOrigin.Begin);
         BaseStream.CopyTo(stream);
      }

      /// <summary>
      /// kopiert die restlichen Bytes des Streams
      /// </summary>
      /// <returns></returns>
      public byte[] ToArray() {
         MemoryStream ms = new MemoryStream();
         BaseStream.CopyTo(ms);
         return ms.ToArray();
      }

      public override string ToString() {
         return string.Format("Position=0x{0:x}, Length=0x{1:x}", Position, Length);
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
            if (BaseStream != null &&
                BaseStream.CanSeek) {        // sonst schon wahrscheinlich schon geschlossen
               BaseStream.Flush();
               BaseStream.Close();
               BaseStream.Dispose();
               BaseStream = null;
            }
            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
