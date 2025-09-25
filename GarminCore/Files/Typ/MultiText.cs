/*
Copyright (C) 2011 Frank Stinner

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
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// mehrere verbundene Texte (i.A. verschiedene Sprachen)
   /// </summary>
   public class MultiText {
      /* Die Längenkennung ist etwas tricky:
       * Wenn Bit 0 == 1, dann bilden Bit 1 bis Bit 7 die Länge des nachfolgenden Textes (einschlielich aller
       * Länderkennungen und 0-Bytes).
       * Wenn Bit 0 == 0 und Bit == 1, dann gehört das folgende Byte auch zur Längenkennung und Bit 2 bis Bit 15
       * bilden die die Länge des nachfolgenden Textes.
       * Prinzipiell läßt sich dieses Spielchen natürlich fortsetzen: Bit 0 bis 2 == 100, dann gehört
       * vielleicht das 3 Byte auch zur Längenkennung und die Länge wird durch Bit 3 bis Bit 23 gebildet (?).
       * usw. usf.
       */

      SortedList<Text.LanguageCode, string> txt;

      /// <summary>
      /// max. Länge eines Multitextes in Byte (einschließlich jeweils 1 Byte Ländercode und 1 0-Byte)
      /// </summary>
      public const int MaxRealLength = (0xffff - 2) << 2;       // da max. 2 Byte für die Speicherung

      public MultiText() {
         txt = new SortedList<Text.LanguageCode, string>();
      }

      public MultiText(Text txt)
         : this() {
         Set(txt);
      }

      public MultiText(Text.LanguageCode code, string txt)
         : this() {
         Set(code, txt);
      }

      public MultiText(MultiText mt)
         : this() {
         foreach (Text.LanguageCode lcode in mt.txt.Keys) {
            txt.Add(lcode, mt.txt[lcode]);
         }
      }

      public MultiText(BinaryReaderWriter br) :
         this() {
         int len = br.ReadByte();
         if ((len & 0x1) == 0x1)       // ungerade --> nur 1 Byte für Längekennung
            len >>= 1;
         else {                        // gerade --> 2 Byte für Längekennung
            len += br.ReadByte() << 8;
            len >>= 2;
         }
         while (len > 0) {
            Text t = new Text(br);
            Set(t);
            len -= t.Txt.Length + 2;   // einschließlich jeweils 1 Byte Ländercode und 1 0-Byte
         }
      }

      /// <summary>
      /// löscht alle Texteinträge
      /// </summary>
      public void Clear() {
         txt.Clear();
      }

      /// <summary>
      /// fügt dem Multitext einen Text hinzu bzw. ersetzt ihn
      /// </summary>
      /// <param name="txt"></param>
      public void Set(Text txt) {
         Set(txt.Language, txt.Txt);
      }

      /// <summary>
      /// fügt dem Multitext einen Text hinzu bzw. ersetzt ihn
      /// </summary>
      /// <param name="code"></param>
      /// <param name="txt"></param>
      public void Set(Text.LanguageCode code, string txt) {
         if (txt.Length > 0)
            if (this.txt.ContainsKey(code)) {
               if (txt.Length > FreeTxtLength())
                  throw new Exception(string.Format("Der Text darf höchtens noch {0} Zeichen lang sein.", FreeTxtLength()));
               this.txt[code] = txt;
            } else {
               if (txt.Length > FreeNewTxtLength())
                  throw new Exception(string.Format("Der Text darf höchtens noch {0} Zeichen lang sein.", FreeNewTxtLength()));
               this.txt.Add(code, txt);
            }
      }

      /// <summary>
      /// max. noch mögliche Textlänge (Anzahl der Zeichen)
      /// </summary>
      /// <returns></returns>
      public int FreeTxtLength() {
         return MaxRealLength - GetRealLength();
      }

      /// <summary>
      /// max. noch mögliche Textlänge (Anzahl der Zeichen) für einen zusätzlichen (!) Text
      /// </summary>
      /// <returns></returns>
      public int FreeNewTxtLength() {
         return FreeTxtLength() - 2;
      }

      /// <summary>
      /// liefert den Text für die entsprechende Sprache (oder falls nicht vorhanden einen leeren Text)
      /// </summary>
      /// <param name="code"></param>
      /// <returns></returns>
      public string Get(Text.LanguageCode code) {
         if (this.txt.ContainsKey(code))
            return this.txt[code];
         return "";
      }

      /// <summary>
      /// liefert alle enthaltenen <see cref="Text.LanguageCode"/>
      /// </summary>
      /// <returns></returns>
      public Text.LanguageCode[] GetLanguageCodes() {
         Text.LanguageCode[] code = new Text.LanguageCode[txt.Count];
         txt.Keys.CopyTo(code, 0);
         return code;
      }

      /// <summary>
      /// liefert den Text mit dem Index
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public Text Get(int idx) {
         if (0 <= idx && idx < Count)
            return new Text(txt[txt.Keys[idx]], txt.Keys[idx]);
         return new Text("", Text.LanguageCode.unspecified);
      }

      /// <summary>
      /// Anzahl der Texte im Multitext
      /// </summary>
      public int Count { get { return txt.Count; } }

      /// <summary>
      /// schreibt den Multitext in den Stream
      /// </summary>
      /// <param name="bw"></param>
      /// <param name="iCodepage"></param>
      public void Write(BinaryReaderWriter bw, int iCodepage) {
         int len = GetIdentificationLength();
         if (len <= 0xff)
            bw.Write((byte)(0xff & len));
         else
            bw.Write((UInt16)(0xffff & len));
         foreach (Text.LanguageCode code in txt.Keys)
            new Text(txt[code], code).Write(bw, iCodepage);
      }

      /// <summary>
      /// liefert die real benötigte Länge in Byte für den Textbereich (ohne Längenkennung)
      /// </summary>
      /// <returns></returns>
      public int GetRealLength() {
         int len = 0;
         foreach (Text.LanguageCode code in txt.Keys)
            len += txt[code].Length + 2;     // (Text + 1 Byte Ländercode + 1 0-Byte)
         return len;
      }

      /// <summary>
      /// liefert die Längenkennung für die Speicherung in der Datei
      /// </summary>
      /// <returns></returns>
      public int GetIdentificationLength() {
         int len = GetRealLength();
         len = 2 * len + 1;      // immer ungerade
         if (len > 0xff)
            len *= 2;            // immer gerade
         return len;
      }

      public string GetAsSimpleString() {
         StringBuilder sb = new StringBuilder();
         foreach (Text.LanguageCode code in GetLanguageCodes()) {
            sb.Append("[");
            sb.Append(code.ToString());
            sb.Append("]\"");
            sb.Append(Get(code));
            sb.Append("\"");
         }
         return sb.ToString();
      }

      public override string ToString() {
         StringBuilder sb = new StringBuilder();
         sb.Append("MultiText=[Längekennung=");
         sb.Append("0x" + GetIdentificationLength().ToString("x") + ",");
         for (int i = 0; i < Count; i++)
            sb.Append(" " + Get(i).ToString());
         sb.Append(" ]");
         return sb.ToString(); ;
      }

   }

}
