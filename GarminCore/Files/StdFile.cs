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

namespace GarminCore.Files {

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
      public byte Unknown_0x0C { get; protected set; }
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
         get {
            return GarminTyp.Length > 7 ? GarminTyp.Substring(7) : "";
         }
         set {
            if (value.Length == 3)
               GarminTyp = "GARMIN " + value.ToUpper();
         }
      }

      /// <summary>
      /// Sammlung aller Dateiabschnitte (als Rohdaten) damit Lesen und Schreiben der Dateien möglich ist, auch ohne den Inhalt der Daten zu dekodieren
      /// <para>Die Ponter beziehen sich immer auf den Anfang des Dateiheaders.</para>
      /// </summary>
      protected FileSections Filesections;

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
         get {
            return Math.Max(HeaderOffset + Headerlength, _GapOffset);
         }
         protected set {
            _GapOffset = Math.Max(HeaderOffset + Headerlength, value);
         }
      }

      protected uint _DataOffset;

      /// <summary>
      /// Offset des Datenbereiches bezüglich des Headeranfangs (kann nie kleiner als <see cref="GapOffset"/> sein und ist normalerweise 
      /// auch <see cref="Headerlength"/>)
      /// <para>Dieser Wert muss vor dem Schreiben der Daten korrekt gesetzt sein, falls er größer als <see cref="GapOffset"/> sein soll!</para>
      /// </summary>
      public virtual uint DataOffset {
         get {
            return Math.Max(GapOffset, _DataOffset);
         }
         protected set {
            _DataOffset = Math.Max(GapOffset, value);
         }
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
         Filesections = new FileSections();
      }

      /// <summary>
      /// liest den allgemeinen Header ab <see cref="HeaderOffset"/> ein
      /// <para>Ist der eingelesene Typ nicht der erwartete Typ, wird eine Exception ausgelöst.</para>
      /// </summary>
      /// <param name="br"></param>
      /// <param name="expectedtyp">Extension des erwarteten Typs z.B. 'LBL', sonst null</param>
      protected void ReadCommonHeader(BinaryReaderWriter br, string? expectedtyp = null) {
         br.Seek(HeaderOffset);

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
      /// schreibt den allgemeinen Header
      /// </summary>
      /// <param name="wr"></param>
      /// <param name="typ">nur angeben wenn der akt. <see cref="GarminTyp"/> überschrieben werden soll</param>
      protected void WriteCommonHeader(BinaryReaderWriter wr, string? typ = null) {
         wr.Seek(HeaderOffset);

         wr.Write(Headerlength);
         if (!string.IsNullOrEmpty(typ))
            GarminTyp = "GARMIN " + typ;
         wr.WriteString(GarminTyp, null, false);
         wr.Write(Unknown_0x0C);
         wr.Write(Locked);
         wr.Write((Int16)(CreationDate.Year));
         wr.Write((byte)(CreationDate.Month));
         wr.Write((byte)(CreationDate.Day));
         wr.Write((byte)(CreationDate.Hour));
         wr.Write((byte)(CreationDate.Minute));
         wr.Write((byte)(CreationDate.Second));
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
      abstract protected void ReadSections(BinaryReaderWriter br);

      /// <summary>
      /// decodiert die eingelesenen Datenabschnitte (wenn nicht <see cref="RawRead"/>)
      /// </summary>
      abstract protected void DecodeSections();

      public virtual void Read(BinaryReaderWriter br, bool raw = false, uint headeroffset = 0, uint gapoffset = 0) {
         RawRead = raw;
         HeaderOffset = headeroffset;
         GapOffset = gapoffset;

         ReadHeader(br);

         Filesections.ClearSections();
         ReadSections(br);

         if (!RawRead)
            DecodeSections();
      }

      /// <summary>
      /// alle Datenabschnitte neu encodieren
      /// </summary>
      abstract public void Encode_Sections();

      /// <summary>
      /// Offset der Datenabschnitte festlegen
      /// </summary>
      abstract public void SetSectionsAlign();

      /// <summary>
      /// alle Daten schreiben
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="gapoffset">neuer Offset für den Header-Anfang</param>
      /// <param name="headerlength">i.A. 0xbc; Die Funktion der Daten für einen größeren Header sind z.Z. sowieso unbekannt.</param>
      /// <param name="gapoffset">neuer Offset für Gap</param>
      /// <param name="dataoffset">neuer Offset für Data</param>
      /// <param name="encodesections">wenn true, werden die Datenabschnitte mit neu encodierten Daten gefüllt</param>
      public virtual void Write(BinaryReaderWriter bw, uint headeroffset = 0, UInt16 headerlength = 0, uint gapoffset = 0, uint dataoffset = 0, bool encodesections = true) {
         HeaderOffset = headeroffset;
         if (headerlength > 0)
            Headerlength = headerlength;
         CreationDate = DateTime.Now;

         GapOffset = gapoffset;
         DataOffset = dataoffset;

         if (encodesections)
            Encode_Sections();
         SetSectionsAlign();

         Encode_Header(bw); // Header mit den akt. Offsets neu erzeugen
         Filesections.WriteSections(bw);
      }

      /// <summary>
      /// muss i.A. überschrieben werden, um den spezifischen Header auch zu speichern
      /// </summary>
      /// <param name="bw"></param>
      protected virtual void Encode_Header(BinaryReaderWriter bw) {
         if (bw != null)
            WriteCommonHeader(bw, Type);

      }

      /// <summary>
      /// einen bestimmten Datenabschnitt encodieren
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="filesectiontype"></param>
      abstract protected void Encode_Filesection(BinaryReaderWriter bw, int filesectiontype);

      /// <summary>
      /// Die Daten werden encodiert und in den entsprechenden Abschnitt geschrieben. Ex. der Abschnitt schon, erfolgt KEIN Schreiben!
      /// </summary>
      /// <param name="filesectiontype"></param>
      /// <param name="overwrite">wenn true, werden ev. schon vorhandene Daten überschrieben</param>
      /// <returns>false, wenn der Abschnitt nicht überschrieben werden kann oder keine Daten ex.</returns>
      protected bool SetData2Filesection(int filesectiontype, bool overwrite) {
         if (!overwrite &&
             Filesections.ContainsType(filesectiontype))
            return false;

         BinaryReaderWriter bw = new BinaryReaderWriter();
         Encode_Filesection(bw, filesectiontype); // Daten nur in den temp. Writer schreiben

         if (bw.Length > 0) {
            if (!Filesections.ContainsType(filesectiontype))
               Filesections.AddSection(filesectiontype);
            bw.Seek(0);
            Filesections.SetSectionDataFromWriter(filesectiontype, bw);
         } else {
            if (Filesections.ContainsType(filesectiontype))
               Filesections.RemoveSection(filesectiontype);
            return false;
         }
         return true;
      }

      /// <summary>
      /// liefert den Offset auf den Bereich nach den Datenabschnitten
      /// </summary>
      public uint GetOffsetBehind() {
         return Filesections != null && Filesections.Count > 0 ? Filesections.GetOffsetBehind() : 0;
      }

      /// <summary>
      /// Länge des 1. Datenabschnitts (falls vorhanden)
      /// </summary>
      /// <returns></returns>
      public uint GetPostHeaderLength() {
         return Filesections.GetLength(0);   // per Def. Abschnitt 0!
      }

      /// <summary>
      /// ermittelt und setzt den <see cref="GapOffset"/> und den <see cref="DataOffset"/> aus den aktuellen <see cref="Filesections"/>
      /// </summary>
      /// <param name="postheadertype">der Typ des Postheaders (oder ein negativer Wert)</param>
      /// <returns>false, wenn keine Abschnitte ex.</returns>
      protected bool SetSpecialOffsetsFromSections(int postheadertype = -1) {
         uint[] sortedoffsets = Filesections.GetSortedOffsets(out int[] sectiontype, true);
         if (sortedoffsets.Length > 0) {
            DataOffset = GapOffset = sortedoffsets[0];
            if (postheadertype == sectiontype[0]) {
               if (sortedoffsets.Length > 1)
                  DataOffset = sortedoffsets[1]; // es könnte eine Lücke zwischen 1. und 2. Abschnitt bleiben
               else
                  DataOffset += Filesections.GetLength(postheadertype); // 2. (noch unbekannter) Abschnitt folgt direkt nach dem 1.
            }
            return true;
         }
         return false;
      }

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
