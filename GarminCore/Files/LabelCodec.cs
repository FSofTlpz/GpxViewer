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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GarminCore.Files {

   public class LabelCodec {

      /// <summary>
      /// Art des Codecs
      /// </summary>
      public byte Type { get; private set; }
      /// <summary>
      /// Codepage (z.B. 1252, Western European; 1250 Mitteleuropäisch mit Umlauten; 1251 Kyrillisch)
      /// </summary>
      public uint Codepage { get; private set; }
      /// <summary>
      /// Name der Codierung
      /// </summary>
      public string Charsetname { get; private set; }
      /// <summary>
      /// Anzahl der zuletzt dekodierten Bytes
      /// </summary>
      public int DecodedBytes { get; private set; }

      /// <summary>
      /// Typen der Spezial"buchstaben"
      /// </summary>
      public enum SpecialCodes {
         /// <summary>
         /// Delimiter between a formal name and its abbreviation
         /// </summary>
         Delimiter = 0x1d,
         /// <summary>
         /// Hide the preceding characters and insert a space
         /// </summary>
         HidePrecedingAndInsertSpace = 0x1e,
         /// <summary>
         /// Hide the following characters and insert a space
         /// </summary>
         HideFollowingAndInsertSpace = 0x1f,
         SymbolInterstate = 0x1,
         SymbolHighway = 0x2,
         SymbolStateHighway = 0x3,
         SymbolCanadianHighwayBlueRed = 0x4,
         SymbolCanadianHighwayBlackWhite = 0x5,
         SymbolHighwaySmallWhite = 0x6
      }

      /// <summary>
      /// Spezial"buchstaben", die im Text verwendet werden können
      /// </summary>
      public SortedDictionary<SpecialCodes, char> SpecialChars = new SortedDictionary<SpecialCodes, char>() {
         { SpecialCodes.Delimiter, '\u001d' },
         { SpecialCodes.HidePrecedingAndInsertSpace, '\u001e' },
         { SpecialCodes.HideFollowingAndInsertSpace, '\u001f' },

         { SpecialCodes.SymbolInterstate, '\u0001' },
         { SpecialCodes.SymbolHighway, '\u0002' },
         { SpecialCodes.SymbolStateHighway, '\u0003' },
         { SpecialCodes.SymbolCanadianHighwayBlueRed, '\u0004' },
         { SpecialCodes.SymbolCanadianHighwayBlackWhite, '\u0005' },
         { SpecialCodes.SymbolHighwaySmallWhite, '\u0006' },
      };

      /// <summary>
      /// liefert den Spezialcode für die Spezial"buchstaben"
      /// </summary>
      SortedDictionary<char, SpecialCodes> SpecialCode4Chars;

      Encoder? enc;
      Decoder? dec;



      /// <summary>
      /// 
      /// </summary>
      /// <param name="type"></param>
      /// <param name="codepage"></param>
      /// <param name="charsetname"></param>
      public LabelCodec(byte type = 0x06, uint codepage = 0, string charsetname = "") {
         Type = type;
         Codepage = codepage;
         Charsetname = charsetname;
         DecodedBytes = 0;
         enc = null;
         dec = null;

         if (Type != 0x06) {
            Encoding? encoding = null;
            if (codepage > 0)
               encoding = Encoding.GetEncoding((int)Codepage);
            else
               encoding = Encoding.GetEncoding(charsetname);
            enc = encoding.GetEncoder();
            dec = encoding.GetDecoder();
         }

         SpecialCode4Chars = new SortedDictionary<char, SpecialCodes>();
         foreach (var item in SpecialChars) {
            SpecialCode4Chars.Add(item.Value, item.Key);
         }

      }

      /// <summary>
      /// liefert den Text entsprechend der Kodierung als Bytefolge
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      public byte[] Encode(string txt) {
         byte[] buff;
         int charsUsed;
         int bytesUsed = 0;
         bool completed;

         switch (Type) {
            case 0x06:
               return Encode6(txt);

            case 0x09:
               buff = new byte[2 * txt.Length + 2];
               if (enc != null)
                  enc.Convert(txt.ToCharArray(), 0, txt.Length, buff, 0, buff.Length, true, out charsUsed, out bytesUsed, out completed);
               buff[bytesUsed] = 0;
               return buff.Take(bytesUsed + 1).ToArray();

            default:
               throw new Exception("Unbekannte Codierung.");
         }
      }

      /// <summary>
      /// liefert die Bytefolge entsprechend der Codierung als Text
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="start">Startindex für die Dekodierung</param>
      /// <returns></returns>
      public string Decode(byte[] txt, int start = 0) {
         string? text = null;

         int charsUsed;
         int bytesUsed;
         bool completed;

         switch (Type) {
            case 0x06:
               text = Decode6(txt, start, out bytesUsed);
               DecodedBytes = bytesUsed;
               break;

            case 0x09:
               List<char> chars = new List<char>();
               char[] outbuff = new char[1];
               byte[] inbuff = new byte[1];
               int inbuffidx = start;
               DecodedBytes = 0;
               completed = false;
               if (dec != null)
                  while (inbuffidx < txt.Length) {
                     inbuff[0] = txt[inbuffidx++];
                     dec.Convert(inbuff, 0, 1, outbuff, 0, 1, false, out bytesUsed, out charsUsed, out completed);
                     DecodedBytes += bytesUsed;
                     if (charsUsed > 0)
                        if (inbuff[0] != 0x00)
                           chars.Add(outbuff[0]);
                        else
                           break;
                  }
               return new string(chars.ToArray());

            default:
               throw new Exception(string.Format("Unbekannte Codierungsart: 0x{0:x}", Type));
         }
         return text;
      }


      #region 6-Bit-Codierung

      /* Es werden nur 6 Bit je Zeichen verwendet. I.A. werden vermutlich nur Großbuchstaben verwendet. Bei Kleinbuchstaben muss vor jedem (!) Buchstaben
       * der Code NextIsLower stehen. Analog muss vor jedem (!) Symbol NextIsSymbol stehen.
       * Codes >= 0x2A stehen für die Anzeige spezieller "Straßensymbole".
       * Außerdem stehen 3 spezielle Trennzeichen zur Verfügung.
       * Die Umcodierung erfolgt entsprechend der untenstehenden Tabellen. 
       * Beginnt ein Zeichen mit 2 1-Bits, wäre der Code >= 0x30. Er wird dann aber gar nicht mehr ermittelt, sondern die 2 Bit stehen als Endekennung des Textes.
       * die ev. noch zur Verfügung stehenden restlichen Bits des aktuellen Bytes werden nicht benutzt.
       */


      /// <summary>
      /// gültige Zeichen für die 6-Bit-Codierung
      /// </summary>
      static Regex Valid6BitChars = new Regex("[^\u0001-\u0006\u001d-\u001f A-Za-z0-9@!\"#$%&'()*+,-./:;<=>?[\\]^_]");

      char[] CodeTable6Bit ={
         ' ', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',        // 0x00 .. 0x0F
         'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',                                 // 0x10 .. 0x1A
         '\u0007', '\u0008', '\u0009', '\u000a', '\u000b',                                      // 0x1B .. 0x1F
         '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',                                      // 0x20 .. 0x29
         '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006'                             // 0x2A .. 0x2F
      };
      char[] LowerTable6Bit ={
         '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',        // 0x00 .. 0x0F
         'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', ' ', ' ', ' ', ' ', ' ',        // 0x10 .. 0x1F
         ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',        // 0x20 .. 0x2F
      };
      char[] SymbolTable6Bit ={
         '@','!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',        // 0x00 .. 0x0F
         ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ':', ';', '<', '=', '>', '?',        // 0x10 .. 0x1F
         ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '[', '\\', ']', '^', '_'        // 0x20 .. 0x2F
      };

      enum SpecialCodes6Bit {
         nothing = 0,
         /// <summary>
         /// the following character will be in lower case
         /// </summary>
         NextIsLower = 0x1b,
         /// <summary>
         /// the following character will be a symbol
         /// </summary>
         NextIsSymbol = 0x1c,
         /// <summary>
         /// Delimiter between a formal name and its abbreviation
         /// </summary>
         Delimiter = 0x1d,
         /// <summary>
         /// Hide the preceding characters and insert a space
         /// </summary>
         HidePrecedingAndInsertSpace = 0x1e,
         /// <summary>
         /// Hide the following characters and insert a space
         /// </summary>
         HideFollowingAndInsertSpace = 0x1f,
         SymbolInterstate = 0x2a,
         SymbolHighway = 0x2b,
         SymbolStateHighway = 0x2c,
         SymbolCanadianHighwayBlueRed = 0x2d,
         SymbolCanadianHighwayBlackWhite = 0x2e,
         SymbolHighwaySmallWhite = 0x2f
      }

      /// <summary>
      /// codiert den Text, der ev. <see cref="SpecialChars"/> enthält
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      byte[] Encode6(string txt) {
         txt = Valid6BitChars.Replace(txt, "#").Trim();     // Text enthält nur noch gültige Zeichen für 6-Bit; ungültige Zeichen werden zu '#'
         List<bool> bit = new List<bool>();
         for (int k = 0; k < txt.Length; k++) {
            char c = txt[k];
            bool lastchar = k == txt.Length - 1;
            int code6 = -1;

            SpecialCodes6Bit spec = SpecialCodes6Bit.nothing;
            if (SpecialCode4Chars.ContainsKey(c)) {
               switch (SpecialCode4Chars[c]) {
                  case SpecialCodes.SymbolCanadianHighwayBlackWhite:
                  case SpecialCodes.SymbolCanadianHighwayBlueRed:
                  case SpecialCodes.SymbolHighway:
                  case SpecialCodes.SymbolHighwaySmallWhite:
                  case SpecialCodes.SymbolInterstate:
                  case SpecialCodes.SymbolStateHighway:
                  case SpecialCodes.Delimiter:
                  case SpecialCodes.HideFollowingAndInsertSpace:
                  case SpecialCodes.HidePrecedingAndInsertSpace:
                     code6 = (int)SpecialCode4Chars[c];
                     break;
               }
            } else {
               for (int i = 0; i < CodeTable6Bit.Length; i++)
                  if (CodeTable6Bit[i] == c) {
                     spec = SpecialCodes6Bit.nothing;
                     code6 = i;
                     break;
                  }

               if (code6 < 0)
                  for (int i = 0; i < SymbolTable6Bit.Length; i++)
                     if (SymbolTable6Bit[i] == c) {
                        spec = SpecialCodes6Bit.NextIsSymbol;
                        code6 = i;
                        break;
                     }

               if (code6 < 0)
                  for (int i = 0; i < LowerTable6Bit.Length; i++)
                     if (LowerTable6Bit[i] == c) {
                        spec = SpecialCodes6Bit.NextIsLower;
                        code6 = i;
                        break;
                     }
            }

            if (lastchar) {
               if (code6 < 0)
                  code6 = 0;
               code6 |= 0x30;             // Endekennung gesetzt (Bit 4 und 5)
            }

            if (code6 >= 0) {
               if (spec != SpecialCodes6Bit.nothing) {
                  int code = (int)spec;
                  // 6 Bits, beginnend mit Bit 0, in den Puffer schieben
                  for (int i = 0; i < 6; i++) {
                     bit.Add((code & 0x01) != 0);
                     code >>= 1;
                  }
               }

               // 6 Bits, beginnend mit Bit 0, in den Puffer schieben
               for (int i = 0; i < 6; i++) {
                  bit.Add((code6 & 0x01) != 0);
                  code6 >>= 1;
               }
            }
         }

         // Bit-Liste in Byte-Liste umrechnen
         List<byte> lst = new List<byte>();
         byte b = 0x00;
         for (int i = 0; i < bit.Count; i++) {
            if (i % 8 == 0) {             // neues Byte
               b = 0x00;
            } else
               b <<= 1;

            if (bit[i])
               b |= 0x81;                 // das niederwertigste Bit setzen

            if (i % 8 == 7 ||             // Byte ist voll
                i == bit.Count - 1) {     // alle Bits verarbeitet
               if (i % 8 != 7)
                  b <<= 7 - i % 8;        // nach "oben" schieben
               lst.Add(b);
            }
         }

         return lst.ToArray();
      }

      /// <summary>
      /// liefert den Text, der ev. <see cref="SpecialChars"/> enthält
      /// </summary>
      /// <param name="data"></param>
      /// <param name="start">Startindex der Decodierung</param>
      /// <param name="bytesconsumed">Anzahl der verwendeten Bytes</param>
      /// <returns></returns>
      string Decode6(byte[] data, int start, out int bytesconsumed) {
         bytesconsumed = 0;
         string text = "";
         Queue<bool> bits = new Queue<bool>();
         SpecialCodes6Bit spec = SpecialCodes6Bit.nothing;
         bool end = false;

         do {
            byte code6 = 0x00;
            if (bits.Count < 2) {
               Byte2BitQueue(data[start++], bits);
               bytesconsumed++;
            }
            code6 = PushBit(code6, bits.Dequeue());
            code6 = PushBit(code6, bits.Dequeue());
            if (code6 != 0x03) { // keine Endekennung 11xxxx
               if (bits.Count < 4) {
                  Byte2BitQueue(data[start++], bits);
                  bytesconsumed++;
               }
               for (int j = 0; j < 4; j++)
                  code6 = PushBit(code6, bits.Dequeue());

               switch (spec) {
                  case SpecialCodes6Bit.NextIsSymbol:
                  case SpecialCodes6Bit.NextIsLower:
                     text += spec == SpecialCodes6Bit.NextIsSymbol ?
                                       SymbolTable6Bit[code6] :
                                       LowerTable6Bit[code6];
                     spec = SpecialCodes6Bit.nothing;
                     break;

                  default:
                     switch (code6) {
                        case (int)SpecialCodes6Bit.NextIsSymbol:
                        case (int)SpecialCodes6Bit.NextIsLower:
                           spec = (SpecialCodes6Bit)code6;
                           break;

                        case (int)SpecialCodes6Bit.Delimiter:
                        case (int)SpecialCodes6Bit.HideFollowingAndInsertSpace:
                        case (int)SpecialCodes6Bit.HidePrecedingAndInsertSpace:
                        case (int)SpecialCodes6Bit.SymbolCanadianHighwayBlackWhite:
                        case (int)SpecialCodes6Bit.SymbolCanadianHighwayBlueRed:
                        case (int)SpecialCodes6Bit.SymbolHighway:
                        case (int)SpecialCodes6Bit.SymbolHighwaySmallWhite:
                        case (int)SpecialCodes6Bit.SymbolInterstate:
                        case (int)SpecialCodes6Bit.SymbolStateHighway:
                           text += SpecialChars[(SpecialCodes)code6];
                           break;

                        default:
                           text += CodeTable6Bit[code6 & 0x3f];
                           break;
                     }
                     break;
               }

            } else
               end = true;
         } while (!end);
         return text;
      }

      /// <summary>
      /// alle 8 Bits des Bytes in die Bit-Liste schieben
      /// </summary>
      /// <param name="b"></param>
      /// <param name="bits"></param>
      void Byte2BitQueue(byte b, Queue<bool> bits) {
         for (int j = 0; j < 8; j++) {
            bits.Enqueue((b & 0x80) != 0);
            b <<= 1;
         }
      }
      /// <summary>
      /// 1 Bit wird an die "niederwertigste" Stelle reingeschoben, alle "alten" Bits rutschen eine Stelle nach oben
      /// </summary>
      /// <param name="b"></param>
      /// <param name="bit"></param>
      /// <returns></returns>
      byte PushBit(byte b, bool bit) {
         b <<= 1;
         if (bit)
            b |= 0x01;
         return b;
      }




      /// <summary>
      /// liefert den Text, der ev. <see cref="SpecialChars"/> enthält
      /// </summary>
      /// <param name="data"></param>
      /// <param name="start">Startindex der Decodierung</param>
      /// <param name="bytesconsumed">Anzahl der verwendeten Bytes</param>
      /// <returns></returns>
      string Decode6x(byte[] data, int start, out int bytesconsumed) {
         string text = "";
         bytesconsumed = 0;
         SpecialCodes6Bit spec = SpecialCodes6Bit.nothing;
         List<bool> bit = new List<bool>();
         bool end = false;

         for (int i = start; i < data.Length && !end; i++) {
            // alle 8 Bits jedes Bytes in die Bit-Liste schieben
            byte b = data[i];
            bytesconsumed++;
            for (int j = 0; j < 8; j++) {
               bit.Add((b & 0x80) != 0);
               b <<= 1;
            }
            // wenn die akt. Bitliste min. 6 Bit lang ist, kann wieder dekodiert werden
            while (bit.Count >= 6) {      // nächstes Zeichen dekodieren
               int code6 = 0x00;
               for (int j = 0; j < 6; j++) { // 6 Bits aus der Bitliste holen
                  if (bit[0])
                     code6 |= 0x01;
                  if (j < 5)
                     code6 <<= 1;
                  bit.RemoveAt(0); // Bit aus der Bitliste entfernen
               }

               if (code6 > 0x2f) { // Ende
                  // Skip until the next byte boundary.  Note that may mean that we skip more or *less* than 6 bits. 
                  bit.Clear();
                  end = true;
                  break;
               }

               switch (spec) {
                  case SpecialCodes6Bit.NextIsSymbol:
                     text += SymbolTable6Bit[code6 & 0x3f];
                     spec = SpecialCodes6Bit.nothing;
                     break;

                  case SpecialCodes6Bit.NextIsLower:
                     text += LowerTable6Bit[code6 & 0x3f];
                     spec = SpecialCodes6Bit.nothing;
                     break;

                  default:
                     switch (code6 & 0x3f) {
                        case (int)SpecialCodes6Bit.NextIsSymbol:
                           spec = SpecialCodes6Bit.NextIsSymbol;
                           break;

                        case (int)SpecialCodes6Bit.NextIsLower:
                           spec = SpecialCodes6Bit.NextIsLower;
                           break;

                        case (int)SpecialCodes6Bit.Delimiter:
                           text += SpecialChars[SpecialCodes.Delimiter];
                           break;

                        case (int)SpecialCodes6Bit.HideFollowingAndInsertSpace:
                           text += SpecialChars[SpecialCodes.HideFollowingAndInsertSpace];
                           break;

                        case (int)SpecialCodes6Bit.HidePrecedingAndInsertSpace:
                           text += SpecialChars[SpecialCodes.HidePrecedingAndInsertSpace];
                           break;

                        case (int)SpecialCodes6Bit.SymbolCanadianHighwayBlackWhite:
                           text += SpecialChars[SpecialCodes.SymbolCanadianHighwayBlackWhite];
                           break;

                        case (int)SpecialCodes6Bit.SymbolCanadianHighwayBlueRed:
                           text += SpecialChars[SpecialCodes.SymbolCanadianHighwayBlueRed];
                           break;

                        case (int)SpecialCodes6Bit.SymbolHighway:
                           text += SpecialChars[SpecialCodes.SymbolHighway];
                           break;

                        case (int)SpecialCodes6Bit.SymbolHighwaySmallWhite:
                           text += SpecialChars[SpecialCodes.SymbolHighwaySmallWhite];
                           break;

                        case (int)SpecialCodes6Bit.SymbolInterstate:
                           text += SpecialChars[SpecialCodes.SymbolInterstate];
                           break;

                        case (int)SpecialCodes6Bit.SymbolStateHighway:
                           text += SpecialChars[SpecialCodes.SymbolStateHighway];
                           break;

                        default:
                           text += CodeTable6Bit[code6 & 0x3f];
                           break;
                     }
                     break;
               }

               //if (code6 > 0x2f) {
               //   i = txt.Length;
               //   break;
               //}
            }

         }
         return text;
      }






      #endregion


      public override string ToString() {
         return string.Format("Type 0x{0:x}, Codepage {1} {2}, DecodedBytes {3}",
            Type,
            Codepage,
            Charsetname,
            DecodedBytes);
      }

   }
}
