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
using System.Text.RegularExpressions;

namespace GarminCore.Files {
   /**
    * Labels are used for names of roads, points of interest etc.
    *
    * There are different storage formats.
    *
    * 1. A 6 bit compact uppercase ascii format, that has escape codes for some
    * special characters.
    *
    * 2. An 8 bit format.  This seems to be a fairly straightforward latin-1 like
    * encoding with no tricks to reduce the amount of space required.
    *
    * 3. A multi-byte format. For unicode, cp932 etc.
    *
    * @author Steve Ratcliffe
    */
   class Label {

      public Label(string text) {
         Text = text;
         encText = null;
      }

      public Label(char[] text) {
         Text = null;
         encText = text;
      }

      public static Label NULL_LABEL = new Label("");
      public static Label NULL_OUT_LABEL = new Label(new char[0]);

      public string? Text { get; private set; }

      public int Offset { get; set; }

      public int Length {
         get {
            return Text != null ? Text.Length :
                   encText != null ? encText.Length : 0;
         }
      }

      public static string? stripGarminCodes(String s) {
         if (s == null)
            return null;
         s = SHIELDS.Replace(s, "");         // remove
         s = SEPARATORS.Replace(s, " ");     // replace with a space
         s = SQUASH_SPACES.Replace(s, " ");  // replace with a space
         // a leading separator would have turned into a space so trim it
         return s.Trim();
      }

      public static string? squashSpaces(String s) {
         if (string.IsNullOrEmpty(s))
            return null;
         return SQUASH_SPACES.Replace(s, " "); // replace with single space
      }

      /// <summary>
      /// Unicode-Text
      /// </summary>
      char[]? encText;

      // highway shields and "thin" separators
      static Regex SHIELDS = new Regex("[\u0001-\u0006\u001b-\u001c]");

      // "fat" separators
      static Regex SEPARATORS = new Regex("[\u001d-\u001f]");

      // two or more whitespace characters
      static Regex SQUASH_SPACES = new Regex("\\s\\s+");

   }
}
