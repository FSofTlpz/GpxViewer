using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GMap.NET.FSofTExtented.MapProviders {
   /// <summary>
   /// nur für <see cref="NET.MapProviders.MultiUseBaseProvider"/> für die mehrere Karten ex. können
   /// </summary>
   public class UniqueIDDelta {

      static SHA256 sha2;        // 256 Bit = 32 Byte

      int lastid = 0;

      Dictionary<string, (int, string)> idlst;

      /// <summary>
      /// ID-Datei (Textdatei mit den ID und Hashes)
      /// </summary>
      string filename;


      static UniqueIDDelta() {
         sha2 = SHA256.Create();
      }

      public UniqueIDDelta(string filename) {
         this.filename = filename;
         idlst = new Dictionary<string, (int, string)>();
         read(filename);
      }

      /// <summary>
      /// liefert das Delta zum (Hash-)String und ergänzt bei Bedarf die ID-Datei
      /// </summary>
      /// <param name="hash"></param>
      /// <param name="comment"></param>
      /// <returns></returns>
      public int GetDelta(string hash, string comment = null) {
         int delta = GetDelta4Hash(hash);
         if (delta >= 0)
            return delta;
         if (string.IsNullOrEmpty(comment))
            comment = "?";
         idlst.Add(hash, (++lastid, comment));
         write(filename);
         return lastid;
      }

      /// <summary>
      /// liefert, wenn vorhanden, das Delta zum Hash
      /// </summary>
      /// <param name="hash"></param>
      /// <returns>negativ, wenn noch nicht vorhanden</returns>
      public int GetDelta4Hash(string hash) => idlst.TryGetValue(hash, out (int, string) id) ? id.Item1 : -1;

      /// <summary>
      /// liefert, wenn vorhanden, den Hash zum Delta
      /// </summary>
      /// <param name="delta"></param>
      /// <returns></returns>
      public string GetHash4Delta(int delta) {
         string hash = string.Empty;
         foreach (var item in idlst) {
            if (item.Value.Item1 == delta) {
               hash = item.Key;
               break;
            }
         }
         return hash;
      }

      /// <summary>
      /// löscht den Eintrag für <paramref name="hash"/>
      /// </summary>
      /// <param name="hash"></param>
      /// <returns>false wenn nicht vorhanden</returns>
      public bool RemoveDelta(string hash) {
         if (hash != string.Empty) {
            idlst.Remove(hash);
            write(filename);
            return true;
         }
         return false;
      }

      /// <summary>
      /// löscht den Eintrag für <paramref name="delta"/>
      /// </summary>
      /// <param name="delta"></param>
      /// <returns>false wenn nicht vorhanden</returns>
      public bool RemoveDelta(int delta) => RemoveDelta(GetHash4Delta(delta));

      /// <summary>
      /// erzeugt den Hash-String aus einer Anzahl von Bytes
      /// </summary>
      /// <param name="bytes"></param>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static string GetHashString(byte[] bytes, int from = 0, int length = -1) => GetHashString(null, bytes, from, length);

      /// <summary>
      /// erzeugt den Hash-String aus einer Anzahl von Bytes und einem Text
      /// </summary>
      /// <param name="exttxt">verwendeter Text (Unicode intern in Bytes umgewandelt)</param>
      /// <param name="bytes">zusätzliche Bytes</param>
      /// <param name="from">Startpunkt für die verwendeten Gesamtbytes</param>
      /// <param name="length">Anzahl der verwendeten Gesamtbytes</param>
      /// <returns></returns>
      public static string GetHashString(string exttxt, byte[] bytes, int from = 0, int length = -1) {
         if (from < 0)
            from = 0;
         if (length < 0)
            length = bytes.Length - from;
         if (bytes.Length < from + length)
            length = bytes.Length - from;
         // length ist jetzt auf den gültigen Bereich aus dem bytes-Array eingegrenzt

         byte[] b = bytes;    // falls kein Text angegeben ist nur dieses Array verwenden

         if (!string.IsNullOrEmpty(exttxt)) {
            byte[] extbytes = Encoding.Unicode.GetBytes(exttxt);
            b = new byte[extbytes.Length + length];
            extbytes.CopyTo(b, 0);
            if (from == 0 && length == bytes.Length) {
               bytes.CopyTo(b, extbytes.Length);
            } else {
               for (int s = from, d = extbytes.Length; s < from + length; s++, d++)
                  b[d] = bytes[s];
            }
            length += extbytes.Length;
            from = 0;
         }
         return hashString4HashBytes(sha2.ComputeHash(b, from, length));
      }

      void read(string filename) {
         if (File.Exists(filename)) {
            string txt = File.ReadAllText(filename);
            foreach (string line in txt.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)) {
               string[] f = line.Split('\t');
               if (f.Length == 3) {    // Delta, Name, Hash
                  int v = Convert.ToInt32(f[0]);
                  idlst.Add(f[2], (v, f[1]));
                  lastid = Math.Max(lastid, v);
               }
            }
         }
      }

      void write(string filename) {
         StringBuilder sb = new StringBuilder();
         foreach (var item in idlst)
            sb.AppendLine(item.Value.Item1 + "\t" + item.Value.Item2 + "\t" + item.Key);  // Delta, Name, Hash

         string dir = Path.GetDirectoryName(filename);
         if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
         File.WriteAllText(filename, sb.ToString());
      }

      static string hashString4HashBytes(byte[] bytes) {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < bytes.Length; i++)
            sb.Append(bytes[i].ToString("x2"));
         return sb.ToString();
      }

      public Dictionary<string, (int, string)> AllData() => idlst;

      public override string ToString() {
         return string.Format("registriert: {0} (lastid={1})", idlst.Count, lastid);
      }

   }
}
