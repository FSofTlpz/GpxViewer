using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GMap.NET.CoreExt.MapProviders {
   /// <summary>
   /// nur für <see cref="GMapProvider"/> für die mehrere Karten ex. können
   /// </summary>
   internal class UniqueIDDelta {

      static SHA256 sha2;        // 256 Bit = 32 Byte

      int lastid = 0;

      Dictionary<string, int> idlst;

      string filename;


      static UniqueIDDelta() {
         sha2 = new SHA256CryptoServiceProvider();
      }

      public UniqueIDDelta(string filename) {
         this.filename = filename;
         idlst = new Dictionary<string, int>();
         read(filename);
      }

      /// <summary>
      /// liefert das Delta zum (Hash-)String
      /// </summary>
      /// <param name="hash"></param>
      /// <returns></returns>
      public int GetDelta(string hash) {
         if (idlst.TryGetValue(hash, out int id))
            return id;
         idlst.Add(hash, ++lastid);
         write(filename);
         return lastid;
      }

      /// <summary>
      /// erzeugt den Hash-String aus einer Anzahl von Bytes
      /// </summary>
      /// <param name="bytes"></param>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static string GetHashString(byte[] bytes, int from = 0, int length = -1) {
         return GetHashString(null, bytes, from, length);
      }

      /// <summary>
      /// erzeugt den Hash-String aus einer Anzahl von Bytes und einem Text
      /// </summary>
      /// <param name="exttxt"></param>
      /// <param name="bytes"></param>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static string GetHashString(string exttxt, byte[] bytes, int from = 0, int length = -1) {
         if (length < 0)
            length = bytes.Length - from;
         if (bytes.Length < from + length)
            length = bytes.Length - from;

         byte[] b = bytes;

         if (!string.IsNullOrEmpty(exttxt)) {
            byte[] extbytes = Encoding.Unicode.GetBytes(exttxt);
            b = new byte[extbytes.Length + length];
            extbytes.CopyTo(b, 0);
            if (from == 0 && length == bytes.Length)
               bytes.CopyTo(b, extbytes.Length);
            else {
               for (int s = from, d = extbytes.Length; s < bytes.Length + length; s++, d++) {
                  b[d] = bytes[s];
               }
            }
         }
         return hashString4HashBytes(sha2.ComputeHash(b, from, length));
      }

      void read(string filename) {
         if (File.Exists(filename)) {
            string txt = File.ReadAllText(filename);
            foreach (string line in txt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
               string[] f = line.Split('\t');
               if (f.Length == 2) {
                  int v = Convert.ToInt32(f[0]);
                  idlst.Add(f[1], v);
                  lastid = Math.Max(lastid, v);
               }
            }
         }
      }

      void write(string filename) {
         StringBuilder sb = new StringBuilder();
         foreach (var item in idlst)
            sb.AppendLine(item.Value + "\t" + item.Key);
         File.WriteAllText(filename, sb.ToString());
      }

      static string hashString4HashBytes(byte[] bytes) {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < bytes.Length; i++)
            sb.Append(bytes[i].ToString("x2"));
         return sb.ToString();
      }

      public override string ToString() {
         return string.Format("registriert: {0} (lastid={1})", idlst.Count, lastid);
      }

   }
}
