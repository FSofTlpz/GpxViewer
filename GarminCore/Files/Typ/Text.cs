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
using System.Text;

namespace GarminCore.Files.Typ {

   /// <summary>
   /// einzelner Text mit Sprachkennung
   /// </summary>
   public class Text {

      /// <summary>
      /// Sprachcodes
      /// </summary>
      public enum LanguageCode {
         unspecified = 0x00,
         french = 0x01,
         german = 0x02,
         dutch = 0x03,
         english = 0x04,
         italian = 0x05,
         finnish = 0x06,
         swedish = 0x07,
         spanish = 0x08,
         basque = 0x09,
         catalan = 0x0a,
         galician = 0x0b,
         welsh = 0x0c,
         gaelic = 0x0d,
         danish = 0x0e,
         norwegian = 0x0f,
         portuguese = 0x10,
         slovak = 0x11,
         czech = 0x12,
         croatian = 0x13,
         hungarian = 0x14,
         polish = 0x15,
         turkish = 0x16,
         greek = 0x17,
         slovenian = 0x18,
         russian = 0x19,
         estonian = 0x1a,
         latvian = 0x1b,
         romanian = 0x1c,
         albanian = 0x1d,
         bosnian = 0x1e,
         lithuanian = 0x1f,
         serbian = 0x20,
         macedonian = 0x21,
         bulgarian = 0x22
      }

      public string Txt { get; private set; }
      public LanguageCode Language { get; private set; }

      public Text() {
         Txt = "";
         Language = LanguageCode.unspecified;
      }

      public Text(BinaryReaderWriter br)
         : this() {
         Language = (LanguageCode)br.ReadByte();
         Txt = br.ReadString(0); // Encoding sollte im BinaryReaderWriter gesetzt sein; Encoding.Default);

         //List<char> chars = new List<char>();
         //char c;
         //do {
         //   c = br.ReadChar();
         //   if (c != 0x0) 
         //      chars.Add(c);
         //} while (c != 0x0);
         //Txt = new string(chars.ToArray());
      }

      public Text(string txt)
         : this() {
         Txt = txt;
         if (Length4Write > 253)
            throw new Exception("Ein Text darf max. 253 Zeichen lang sein");
      }

      public Text(string txt, LanguageCode lang)
         : this() {
         Txt = txt;
         Language = lang;
      }

      /// <summary>
      /// Länge im Typfile (Text + 1 Byte Ländercode + 1 0-Byte)
      /// </summary>
      public int Length4Write { get { return Txt.Length + 2; } }

      public void Write(BinaryReaderWriter bw, int codepage) {
         bw.Write((byte)Language);
         byte[] txt = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(codepage), Encoding.Unicode.GetBytes(Txt));
         bw.Write(txt);
         bw.Write((byte)0);
      }

      public override string ToString() {
         return string.Format("[{0}] {1}", Language, Txt);
      }

   }

}
