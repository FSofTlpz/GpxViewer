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
using System.Reflection;

namespace GarminCore {

   public static class Garmin {

      /// <summary>
      /// liefert den Standard-Text zum Garmin-Typ (Prefix P, G oder L)
      /// </summary>
      /// <param name="typprefix"></param>
      /// <param name="typ"></param>
      /// <param name="subtyp"></param>
      /// <returns></returns>
      public static string? GetGarminTypname(string typprefix, uint typ, uint subtyp) {
         string txt = "";
         if (typprefix == "P")
            txt = string.Format("{0}0x{1:x2}{2:x2}", typprefix, typ, subtyp);
         else
            txt = string.Format("{0}0x{1:x2}", typprefix, typ);
         return Properties.Resources.ResourceManager.GetString(txt);
      }

      /// <summary>
      /// liefert den Titel der DLL
      /// </summary>
      /// <param name="a"></param>
      /// <returns></returns>
      public static string DllTitle() {
         Assembly a = Assembly.GetExecutingAssembly();

         string sTitle = "";
         object[] attributes = a.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
         if (attributes.Length > 0) {
            AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
            if (titleAttribute.Title != "")
               sTitle = titleAttribute.Title;
         }
         if (sTitle.Length == 0) {          // Notlösung
            string? tmp = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            if (tmp != null)
               sTitle = tmp;
         }
         //string sVersion = a.GetName().Version.ToString();

         string sCopyright = "";
         attributes = a.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
         if (attributes.Length > 0)
            sCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

         string sInfoVersion = "";
         attributes = a.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
         if (attributes.Length > 0)
            sInfoVersion = ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
         return sTitle + ", Version vom " + sInfoVersion + ", " + sCopyright;
      }

   }
}
