namespace GarminCore {
   public class BitReader {

      byte[] buf;

      /// <summary>
      /// Bitanzahl
      /// </summary>
      public int Length { get; protected set; }

      /// <summary>
      /// akt. Pos.
      /// </summary>
      public int Position { get; protected set; }


      public enum SeekOrigin {
         Begin, Current, End
      }


      public BitReader(byte[] bits, int length = -1) {
         buf = new byte[bits.Length];
         bits.CopyTo(buf, 0);
         Position = 0;
         if (0 <= length && length <= bits.Length * 8)
            Length = length;
         else
            Length = bits.Length * 8;
      }

      /// <summary>
      /// liefert das nächste Bit als bool
      /// </summary>
      /// <returns></returns>
      public bool Get1() {
         int off = Position % 8;
         byte b = buf[Position / 8];
         Position++;
         return ((b >> off) & 1) == 1;
      }

      /// <summary>
      /// liefert nb Bits als unsigned int
      /// </summary>
      /// <param name="nb"></param>
      /// <returns></returns>
      public int Get(int nb) {
         int res = 0;
         int pos = 0;
         while (pos < nb) {
            int index = Position / 8;
            int off = Position % 8;

            byte b = buf[index];
            b >>= off;
            int nbits = nb - pos;
            if (nbits > (8 - off)) {
               nbits = 8 - off;
            }

            int mask = ((1 << nbits) - 1);
            res |= ((b & mask) << pos);
            pos += nbits;
            Position += nbits;
         }

         return res;
      }

      /// <summary>
      /// liefert nb Bits als signed int (das letzte Bit ist das Vorzeichen)
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

      /// <summary>
      /// setzt die Lesepos. neu
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="origin"></param>
      /// <returns></returns>
      public int Seek(int offset, SeekOrigin origin) {
         int newpos = 0;
         switch (origin) {
            case SeekOrigin.Begin:
               newpos = offset;
               break;
            case SeekOrigin.Current:
               newpos = Position + offset;
               break;
            case SeekOrigin.End:
               newpos = Length - 1 - offset;
               break;
         }
         if (0 <= newpos && newpos < Length)
            Position = newpos;
         return Position;
      }

      public override string ToString() {
         return string.Format("Position={0}, Length={1}", Position, Length);
      }

   }
}
