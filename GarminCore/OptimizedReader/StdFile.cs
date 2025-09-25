using GarminCore.Files;
using System;

namespace GarminCore.OptimizedReader {

   /// <summary>
   /// Basisklasse für die verschiedenen Garmin-Standarddateien
   /// <para>Diese Dateien haben alle den gleichen allgemeinen Standard-Header. Der Typ der Datei
   /// wird durch eine Zeichenkette angegeben. Die Länge des typspezifischen Headers ergibt sich aus
   /// der Länge des Gesamt-Headers abzüglich der Länge des Standard-Headers.</para>
   /// </summary>
   abstract public class StdFile : IDisposable {

      // prinzipieller Dateiaufbau, mit dem gerechnet werden muss:
      //
      //  PreArea     StdHeader   Header     PostHeaderData    Gap             Section 0   Gap 0  Section 1         Gap n-1 Section n   PostArea
      // |------------HHHHHHHHHHHHhhhhhhhhhhhPPPPPPPPPPPPPPPPPP----------------SSSSSSSSSSSS-------ssssssssssssss....--------SSSSSSSSSSSS-------------------|
      //
      //  HeaderOffset
      // |------------|
      //                   Headerlength
      //              |---------------------|
      //                   GapOffset
      // |----------------------------------------------------|
      //                   DataOffset
      // |---------------------------------------------------------------------|
      //
      // PreArea              normalerweise nicht vorhanden; nur bei GMP
      // StdHeader            für alle Dateitypen identischer Header
      // Header               dateityp-spezifischer Header
      // PostHeaderData       Datenbereich nach dem Header, auf den kein Pointer zeigt
      // Gap                  normalerweise nicht vorhanden; nur bei GMP für weitere Header
      // Gap n                normalerweise nicht vorhanden; nur bei GMP ev. für Daten anderer Dateitypen
      // Section n            Datenblock n
      // PostArea             normalerweise nicht vorhanden; nur bei GMP für weitere Daten anderer Dateitypen


      #region Standard-Header-Daten

      /// <summary>
      /// Länge des gesamten Headers (0x00)
      /// </summary>
      public UInt16 Headerlength { get; protected set; }
      /// <summary>
      /// Typ-Zeichenkette z.B. "GARMIN RGN" (0x02)
      /// </summary>
      protected string GarminTyp { get; private set; } = "";
      /// <summary>
      /// immer 0x01 ? (0x0C)
      /// </summary>
      byte Unknown_0x0C;
      /// <summary>
      /// i.A. 0x00; wenn gesperrt dann 0x80 (0x0D)
      /// </summary>
      public byte Locked { get; protected set; }
      /// <summary>
      /// Datum der Erzeugung (0x0E)
      /// </summary>
      public DateTime CreationDate { get; protected set; }

      #endregion

      /// <summary>
      /// Dateityp (3stellig)
      /// </summary>
      public string Type {
         get => GarminTyp.Length > 7 ? GarminTyp.Substring(7) : "";
         set {
            if (value.Length == 3)
               GarminTyp = "GARMIN " + value.ToUpper();
         }
      }

      /// <summary>
      /// Sammlung aller Dateiabschnitte (als Rohdaten) damit Lesen und Schreiben der Dateien möglich ist, auch ohne den Inhalt der Daten zu dekodieren
      /// <para>Die Ponter beziehen sich immer auf den Anfang des Dateiheaders.</para>
      /// </summary>
      //protected FileSections Filesections;

      /// <summary>
      /// Offset des Headers bezüglich des Dateianfangs; i.A. 0 aber in GMP-Dateien stehen noch andere Daten vor dem Header
      /// </summary>
      public uint HeaderOffset { get; protected set; }

      protected uint _GapOffset;

      /// <summary>
      /// Offset der Lücke bezüglich des Headeranfangs (kann nie kleiner als <see cref="Headerlength"/> sein und ist normalerweise 
      /// auch <see cref="Headerlength"/>)
      /// <para>Dieser Wert muss vor dem Lesen und Schreiben der Daten korrekt gesetzt sein, falls er größer als <see cref="Headerlength"/> sein soll!</para>
      /// </summary>
      public uint GapOffset {
         get => Math.Max(HeaderOffset + Headerlength, _GapOffset);
         protected set => _GapOffset = Math.Max(HeaderOffset + Headerlength, value);
      }

      protected uint _DataOffset;

      /// <summary>
      /// Offset des Datenbereiches bezüglich des Headeranfangs (kann nie kleiner als <see cref="GapOffset"/> sein und ist normalerweise 
      /// auch <see cref="Headerlength"/>)
      /// <para>Dieser Wert muss vor dem Schreiben der Daten korrekt gesetzt sein, falls er größer als <see cref="GapOffset"/> sein soll!</para>
      /// </summary>
      public virtual uint DataOffset {
         get => Math.Max(GapOffset, _DataOffset);
         protected set => _DataOffset = Math.Max(GapOffset, value);
      }

      /// <summary>
      /// Wurden die Daten nur "roh" eingelesen oder auch noch interpretiert
      /// </summary>
      public bool RawRead { get; protected set; }



      /// <summary>
      /// Erzeugt ein Dateiobjekt
      /// </summary>
      /// <param name="typ">Dateityp ("LBL" oder ähnlich); wenn null wird intern nur "xxx" gesetzt</param>
      public StdFile(string? typ = null) {
         Headerlength = 0;
         Type = typ != null && typ.Length == 3 ? typ : "xxx";
         Unknown_0x0C = 0x01;
         Locked = 0x00;
         CreationDate = DateTime.MinValue;

         HeaderOffset = 0;
         _DataOffset = 0;
         //Filesections = new FileSections();
      }

      /// <summary>
      /// liest den allgemeinen Header ab <see cref="HeaderOffset"/> ein
      /// <para>Ist der eingelesene Typ nicht der erwartete Typ, wird eine Exception ausgelöst.</para>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="expectedtyp">Extension des erwarteten Typs z.B. 'LBL', sonst null</param>
      protected void readCommonHeader(BinaryReaderWriter br, string? expectedtyp = null) {
         br.Position = HeaderOffset;

         Headerlength = br.Read2AsUShort();

         GarminTyp = br.ReadString(10);       // z.B. "GARMIN RGN"
         if (GarminTyp.Length != 10 ||
             GarminTyp.Substring(0, 7) != "GARMIN ")
            throw new Exception("Das ist kein Garmin-SUB-File.");

         if (!string.IsNullOrEmpty(expectedtyp) &&
             GarminTyp.Substring(7) != expectedtyp)
            throw new Exception("Das ist nicht der erwartete Dateityp (" + expectedtyp + ").");

         Unknown_0x0C = br.ReadByte();

         Locked = br.ReadByte();

         try {
            CreationDate = new DateTime(br.Read2AsShort(),
                                        br.ReadByte(), // "echter" Monat
                                        br.ReadByte(),
                                        br.ReadByte(),
                                        br.ReadByte(),
                                        br.ReadByte());
         } catch { // Datum/Uhrzeit nicht erkannt
         }
      }


      /// <summary>
      /// liest den gesamten Header ein
      /// </summary>
      /// <param name="br"></param>
      abstract public void ReadHeader(BinaryReaderWriter br);

      /// <summary>
      /// liest die definierten Datenabschnitte ein
      /// </summary>
      /// <param name="wr"></param>
      abstract public void ReadMinimalSections(BinaryReaderWriter br);


      public void ReadMinimalData(BinaryReaderWriter reader) {
         RawRead = false;
         HeaderOffset = 0;
         GapOffset = 0;

         ReadHeader(reader);
         ReadMinimalSections(reader);
      }

      /// <summary>
      /// ermittelt und setzt den <see cref="GapOffset"/> und den <see cref="DataOffset"/> aus den aktuellen <see cref="Filesections"/>
      /// </summary>
      /// <param name="postheadertype">der Typ des Postheaders (oder ein negativer Wert)</param>
      /// <returns>false, wenn keine Abschnitte ex.</returns>
      //protected bool SetSpecialOffsetsFromSections(int postheadertype = -1) {
      //   uint[] sortedoffsets = Filesections.GetSortedOffsets(out int[] sectiontype, true);
      //   if (sortedoffsets.Length > 0) {
      //      DataOffset = GapOffset = sortedoffsets[0];
      //      if (postheadertype == sectiontype[0]) {
      //         if (sortedoffsets.Length > 1)
      //            DataOffset = sortedoffsets[1]; // es könnte eine Lücke zwischen 1. und 2. Abschnitt bleiben
      //         else
      //            DataOffset += Filesections.GetLength(postheadertype); // 2. (noch unbekannter) Abschnitt folgt direkt nach dem 1.
      //      }
      //      return true;
      //   }
      //   return false;
      //}

      public override string ToString() {
         return string.Format("Typ {0}, Locked 0x{1:x}, CreationDate {2}, Headerlength 0x{3:x}, GapOffset 0x{4:x}, DataOffset 0x{5:x}, RawRead {6}",
            GarminTyp, Locked, CreationDate, Headerlength, GapOffset, DataOffset, RawRead);
      }

      ~StdFile() {
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
         if (!_isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }

}
