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
using System.IO;

namespace GarminCore.DskImg {

   /// <summary>
   /// Bei einer Änderung der Daten werden die Daten beim Schließen des Streams als Datei gespeichert.
   /// </summary>
   class MyStream : Stream {

      public delegate void NewSizeEventHandler(object sender, uint newsize, object? extradata);

      public event NewSizeEventHandler? NewSize;

      bool _bReadonly;
      string backgroundfile;
      object? eventdata;
      MemoryStream memstream;
      bool bChanged;

      /// <summary>
      /// Die Datei wird als Speicher für veränderte Daten verwendet.
      /// </summary>
      /// <param name="backgroundfile">Hintergrunddatei, die beim Ändern der Daten erzeugt wird</param>
      /// <param name="data">Streamdaten</param>
      /// <param name="eventdata">Daten, die beim Event übergeben werden</param>
      /// <param name="bReadonly"></param>
      public MyStream(string backgroundfile, byte[]? data = null, object? eventdata = null, bool bReadonly = false) {
         _bReadonly = bReadonly;
         bChanged = false;
         this.backgroundfile = backgroundfile;
         this.eventdata = eventdata;

         if (!bReadonly &&
             string.IsNullOrEmpty(backgroundfile))
            throw new Exception("Eine Datei muss angegeben sein.");

         memstream = new MemoryStream();
         if (data != null && data.Length > 0)
            memstream.Write(data, 0, data.Length);
         memstream.Position = 0;

      }

      public override bool CanRead => true;

      public override bool CanWrite => !_bReadonly;

      public override bool CanSeek => true;

      public override long Position {
         get {
            return memstream.Position;
         }
         set {
            memstream.Position = value;
         }
      }

      public override long Length => memstream.Length;

      public override void SetLength(long value) {
         memstream.SetLength(value);
         bChanged = true;
      }

      public override long Seek(long offset, SeekOrigin origin) => memstream.Seek(offset, origin);

      public override void Flush() {
         memstream.Flush();
         if (bChanged) {
            File.WriteAllBytes(backgroundfile, memstream.ToArray());
            NewSize?.Invoke(this, (uint)memstream.Length, eventdata);
            bChanged = false;
         }
      }

      public override int Read(byte[] buffer, int offset, int count) => memstream.Read(buffer, offset, count);

      public override void Write(byte[] buffer, int offset, int count) {
         memstream.Write(buffer, offset, count);
         bChanged = true;
      }

      public override void Close() {
         base.Close();
         if (bChanged) {
            File.WriteAllBytes(backgroundfile, memstream.ToArray());
            if (NewSize != null)
               NewSize(this, (uint)memstream.Length, eventdata);
         }
         memstream.Close();
      }

      protected override void Dispose(bool disposing) {
         if (memstream != null)
            memstream.Dispose();
         base.Dispose(disposing);
      }

   }
}
