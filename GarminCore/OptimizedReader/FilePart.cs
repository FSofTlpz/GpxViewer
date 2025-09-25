using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GarminCore.Fastreader {
   /// <summary>
   /// hold part of file in memory
   /// </summary>
   public class BinaryReaderWriter {

      /// <summary>
      /// eingelesener Datenbereich
      /// </summary>
      byte[] data = Array.Empty<byte>();

      /// <summary>
      /// Offset des Dateibereiches zum Dateibeginn
      /// </summary>
      public int Offset { get; protected set; }

      /// <summary>
      /// falls die Daten XOR'd sind (NUR ZUM LESEN)
      /// </summary>
      public byte XOR { get; set; }

      readonly byte[] m_buffer = new byte[8];

      char[] m_1char_buffer = new char[1];

      /// <summary>
      /// Position bezüglich des Pufferanfangs
      /// </summary>
      int _relposition = 0;

      /// <summary>
      /// Position bezüglich des Dateianfangs
      /// </summary>
      public int Position {
         get => _relposition + Offset;
         set {
            _relposition = value - Offset;
            BitPosition = 0;
         }
      }

      int _bitPosition = 0;

      /// <summary>
      /// Position innerhalb des Bytes auf <see cref="Position"/> (0..7)
      /// </summary>
      public int BitPosition {
         get => _bitPosition;
         set {
            _bitPosition = value;
            while (_bitPosition >= 8) {
               _bitPosition -= 8;
               Position++;
            }
         }
      }

      /// <summary>
      /// Dateilänge
      /// </summary>
      public int Length { get; protected set; }

      /// <summary>
      /// Länge des Teils
      /// </summary>
      public int PartLength => data.Length;


      /// <summary>
      /// open file, read part and close file
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="offset"></param>
      /// <param name="length"></param>
      public BinaryReaderWriter(string filename, int offset, int length) {
         using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            read(fs, offset, length);
         }
      }

      /// <summary>
      /// read part from open file
      /// </summary>
      /// <param name="fs"></param>
      /// <param name="offset"></param>
      /// <param name="length"></param>
      public BinaryReaderWriter(FileStream fs, int offset, int length) {
         read(fs, offset, length);
      }

      public BinaryReaderWriter(BinaryReaderWriter reader, int offset, int length) {
         Offset = offset;
         data = reader.ReadBytes(new byte[reader.Length]);
         Length = (int)reader.Length;
      }

      void read(FileStream fs, int offset, int length) {
         Offset = offset;
         data = new byte[length];
         int len = fs.Read(data, offset, length);
         if (len != length)
            throw new Exception(nameof(BinaryReaderWriter) + "." + nameof(read) + "(): Nicht genug Daten gelesen.");
         Length = (int)fs.Length;
      }

      public bool IsGetBytesPossible(int count) {
         return Position - Offset + count <= data.Length;
      }

      public bool IsGetBytesPossible(int count, int offset) {
         return Offset <= offset &&
                offset - Offset + count <= data.Length;
      }

      public void Seek(int pos) {
         Position = pos;
      }

      #region basic-reads

      public byte ReadByte() {
         return data[Position++ - Offset];
      }

      public byte ReadByte(int offset) {
         Position = offset;
         return ReadByte();
      }

      public byte[] ReadBytes(int length) {
         byte[] result = new byte[length];
         Array.Copy(data, Position, result, 0, length);
         Position += length;
         return result;
      }

      /// <summary>
      /// Basisfkt zum Füllen des Byte-Puffer
      /// </summary>
      /// <param name="buff"></param>
      /// <returns></returns>
      public byte[] ReadBytes(byte[] buff) {
         Array.Copy(data, Position, buff, 0, buff.Length);
         Position += buff.Length;
         if (XOR != 0)
            for (int i = 0; i < buff.Length; i++)
               buff[i] ^= XOR;
         return buff;
      }

      public byte[] ReadBytes(int offset, int length) {
         Position = offset;
         return ReadBytes(offset, length);
      }

      public int Read1UInt() {
         return ReadByte();
      }

      /// <summary>
      /// liefert 1 Byte als signed Wert
      /// </summary>
      /// <returns></returns>
      public int Read1Int() {
         int v = ReadByte();
         if (v >= 0x80) // Bit 7 = 1 -> negativ
            v -= 0x100;
         return v;
      }

      /// <summary>
      /// liefert 2 Byte als unsigned Wert
      /// </summary>
      /// <returns></returns>
      public int Read2UInt() {
         return ReadByte() + (ReadByte() << 8);
      }

      /// <summary>
      /// liefert 2 Byte als signed Wert
      /// </summary>
      /// <returns></returns>
      public int Read2Int() {
         int v = ReadByte() + (ReadByte() << 8);
         if (v >= 0x8000)
            v -= 0x10000;
         return v;
      }

      /// <summary>
      /// liefert 3 Byte als unsigned Wert
      /// </summary>
      /// <returns></returns>
      public int Read3UInt() {
         return ReadByte() + (ReadByte() << 8) + (ReadByte() << 16);
      }

      /// <summary>
      /// liefert 3 Byte als signed Wert
      /// </summary>
      /// <returns></returns>
      public int Read3Int() {
         int v = ReadByte() + (ReadByte() << 8) + (ReadByte() << 16);
         if (v >= 0x800000)
            v -= 0x1000000;
         return v;
      }

      /// <summary>
      /// liefert 4 Byte als unsigned Wert
      /// </summary>
      /// <returns></returns>
      public uint Read4UInt() {
         return (uint)(ReadByte() + ((long)ReadByte() << 8) + ((long)ReadByte() << 16) + ((long)ReadByte() << 24));
      }

      /// <summary>
      /// liefert 4 Byte als signed Wert
      /// </summary>
      /// <returns></returns>
      public int Read4Int() {
         long v = ReadByte() + (ReadByte() << 8) + (ReadByte() << 16) + (ReadByte() << 24);
         if (v >= 0x80000000)
            v -= 0x100000000;
         return (int)v;
      }

      /// <summary>
      /// konvertiert nur das Ergebnis von <see cref="Read3UInt"/> in uint
      /// </summary>
      /// <returns></returns>
      public uint Read3AsUInt() {
         return (uint)Read3UInt();
      }

      /// <summary>
      /// konvertiert nur das Ergebnis von <see cref="Read2UInt"/> in ushort
      /// </summary>
      /// <returns></returns>
      public ushort Read2AsUShort() {
         return (ushort)Read2UInt();
      }

      /// <summary>
      /// konvertiert nur das Ergebnis von <see cref="Read2Int"/> in short
      /// </summary>
      /// <returns></returns>
      public short Read2AsShort() {
         return (short)Read2Int();
      }

      #endregion

      #region Encoding

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

      #endregion

      #region read char's

      /// <summary>
      /// liest ein einzelnes Zeichen aus dem Stream
      /// </summary>
      /// <param name="encoding"></param>
      /// <returns></returns>
      public char ReadChar(Encoding? encoding = null) {
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
      public char[] ReadChars(int count = 0, Encoding? encoding = null) {
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
            int read_byte = ReadByte();
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

      #endregion

      /// <summary>
      /// liest eine Zeichenkette bis zum 0-Byte oder bis die max. Länge erreicht ist
      /// </summary>
      /// <param name="br"></param>
      /// <param name="maxlen"></param>
      /// <param name="encoder"></param>
      /// <returns></returns>
      public string ReadString(int maxlen = 0, Encoding? encoder = null) {
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
               Position = (int)bl.Offset;
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
         public abstract void Read(BinaryReaderWriter fp, object data);
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
      public List<T> ReadArray<T>(DataBlock bl,
                                  IList<object>? extdata = null,
                                  SortedList<uint, int>? offsets = null) where T : DataStruct, new() {
         List<T> lst = new List<T>();
         if (bl.Length > 0) {
            uint start = bl.Offset;
            uint end = start + bl.Length;
            Position = (int)bl.Offset;
            if (offsets != null)
               offsets.Clear();
            int ds_data = 0;
            int ds_offs = 0;
            object? constdata = extdata != null && extdata.Count > 0 ?
                                                      extdata[0] :
                                                      null;
            while (Position < end) {
               if (offsets != null)
                  offsets.Add((uint)Position - start, ds_offs++); // Offsets speichern

               T t = new T();
               object? dat = extdata != null && ds_data < extdata.Count ? extdata[ds_data++] : constdata;
               if (dat != null) {
                  t.Read(this, dat);
                  lst.Add(t);
               }
            }

            if (extdata != null && ds_data++ < extdata.Count) // ev. noch mit Dummy-Objekten entsprechend der Größe der Datenliste auffüllen
               lst.Add(new T());
         }
         return lst;
      }

      #region read bits

      /// <summary>
      /// liefert das nächste Bit als bool und erhöht <see cref="BitPosition"/> und notfalls auch <see cref="Position"/>
      /// </summary>
      /// <returns></returns>
      public bool Get1() {
         int off = BitPosition % 8;
         BitPosition++;
         byte b = data[Position - Offset];
         return ((b >> off) & 1) == 1;
      }

      /// <summary>
      /// liefert nb Bits als unsigned int und erhöht <see cref="BitPosition"/> und notfalls auch <see cref="Position"/>
      /// </summary>
      /// <param name="nb"></param>
      /// <returns></returns>
      public int Get(int nb) {
         int res = 0;
         int pos = 0;
         while (pos < nb) {
            int off = BitPosition % 8;
            byte b = data[Position - Offset];
            b >>= off;
            int nbits = nb - pos;
            if (nbits > (8 - off)) {
               nbits = 8 - off;
            }

            int mask = (1 << nbits) - 1;
            res |= (b & mask) << pos;
            pos += nbits;
            BitPosition += nbits;
         }

         return res;
      }

      /// <summary>
      /// liefert nb Bits als signed int (das letzte Bit ist das Vorzeichen) und erhöht <see cref="BitPosition"/> und notfalls auch <see cref="Position"/>
      /// </summary>
      /// <param name="nb"></param>
      /// <returns></returns>
      public int SGet(int nb) {
         int res = Get(nb);
         int top = 1 << (nb - 1);
         if ((res & top) != 0) {
            int mask = top - 1;
            res = ~mask | res;
         }
         return res;
      }

      #endregion

   }
}
