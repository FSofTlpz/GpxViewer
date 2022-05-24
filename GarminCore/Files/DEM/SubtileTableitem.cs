using System.IO;

namespace GarminCore.Files.DEM {

   /// <summary>
   /// Tabelleneintrag für die Subtile-Tabelle
   /// </summary>
   public class SubtileTableitem {
      /// <summary>
      /// Offset auf die Daten (bezogen auf den Anfang des Höhendatenbereichs)
      /// </summary>
      public uint Offset { get; set; }
      /// <summary>
      /// Bezugshöhe
      /// </summary>
      public short Baseheight { get; set; }
      /// <summary>
      /// max. Höhendiff.
      /// </summary>
      public ushort Diff { get; set; }
      /// <summary>
      /// Codiertyp
      /// </summary>
      public byte Type { get; set; }

      public SubtileTableitem() {
         Offset = 0;
         Baseheight = 0;
         Diff = 0;
         Type = 0;
      }

      /// <summary>
      /// liest einen Tabelleneintrag ein
      /// </summary>
      /// <param name="br"></param>
      /// <param name="offset_len">Länge des Speicherbereichs für den Offset in Byte</param>
      /// <param name="baseheight_len">Länge des Speicherbereichs für die Basishöhe in Byte</param>
      /// <param name="diff_len">Länge des Speicherbereichs in Byte</param>
      /// <param name="extraBytes">wenn größer 0, dann 1 zusätzliches Byte</param>
      public void Read(BinaryReaderWriter br, int offset_len = 3, int baseheight_len = 2, int diff_len = 2, int type_len = 1) {

         switch (offset_len) {
            case 1:
               Offset = br.ReadByte();
               break;

            case 2:
               Offset = br.Read2AsUShort();
               break;

            case 3:
               Offset = br.Read3AsUInt();
               break;

            case 4:
               Offset = br.Read4UInt();
               break;
         }

         switch (baseheight_len) {
            case 1:
               Baseheight = br.ReadByte();
               break;

            case 2:
               Baseheight = br.Read2AsShort();
               break;
         }

         switch (diff_len) {
            case 1:
               Diff = br.ReadByte();
               break;

            case 2:
               Diff = br.Read2AsUShort();
               break;
         }

         if (type_len > 0)
            Type = br.ReadByte();
      }

      /// <summary>
      /// schreibt den Tabelleneintrag
      /// </summary>
      /// <param name="w"></param>
      /// <param name="offset_len">Byteanzahl für Offset</param>
      /// <param name="baseheight_len">Byteanzahl für Bezugshöhe</param>
      /// <param name="diff_len">Byteanzahl für Höhendiff</param>
      /// <param name="type_len">Byteanzahl für Codiertyp (hier auch 0 möglich)</param>
      public void Write(BinaryReaderWriter w, int offset_len = 3, int baseheight_len = 2, int diff_len = 2, int type_len = 1) {
         // Offset
         switch (offset_len) {
            case 1:
               w.Write((byte)(Offset & 0xFF));
               break;
            case 2:
               w.Write((byte)(Offset & 0xFF));
               w.Write((byte)((Offset & 0xFF00) >> 8));
               break;
            case 3:
               w.Write((byte)(Offset & 0xFF));
               w.Write((byte)((Offset & 0xFF00) >> 8));
               w.Write((byte)((Offset & 0xFF0000) >> 16));
               break;
            case 4:
               w.Write((byte)(Offset & 0xFF));
               w.Write((byte)((Offset & 0xFF00) >> 8));
               w.Write((byte)((Offset & 0xFF0000) >> 16));
               w.Write((byte)((Offset & 0xFF000000) >> 24));
               break;
            default:
               throw new System.Exception("Die Offsetlänge im Tabelleneintrag darf größer als 4 sein.");
         }

         // Basishöhe
         switch (baseheight_len) {
            case 1:
               w.Write((byte)(Baseheight & 0xFF));
               break;
            case 2:
               w.Write((byte)(Baseheight & 0xFF));
               w.Write((byte)((Baseheight & 0xFF00) >> 8));
               break;
            default:
               throw new System.Exception("Die Basishöhenlänge im Tabelleneintrag darf größer als 2 sein.");
         }

         // Diff.
         switch (diff_len) {
            case 1:
               w.Write((byte)(Diff & 0xFF));
               break;
            case 2:
               w.Write((byte)(Diff & 0xFF));
               w.Write((byte)((Diff & 0xFF00) >> 8));
               break;
            default:
               throw new System.Exception("Die Differenzhöhenlänge im Tabelleneintrag darf größer als 2 sein.");
         }

         // Typ
         if (type_len > 0) {
            w.Write(Type);
         }

      }

      public override string ToString() {
         return string.Format("Offset 0x{0:X}, Baseheight 0x{1:X}, Diff 0x{2:X}, Type 0x{3:X}", Offset, Baseheight, Diff, Type);
      }

   }
}
