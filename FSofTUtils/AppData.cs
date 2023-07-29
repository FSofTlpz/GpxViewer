using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace FSofTUtils {
   /// <summary>
   /// zum einfacheren persistenten Speichern von Application-Daten
   /// </summary>
   abstract public class AppData {

      /// <summary>
      /// für persistente Programmdaten
      /// </summary>
      [Serializable]
      [DataContract]
      protected class PersistentDataXml {
         string filename;
         [DataMember]
         ConcurrentDictionary<string, object> dict = new ConcurrentDictionary<string, object>();


         public PersistentDataXml() { }


         /// <summary>
         /// Entweder in <see cref="Environment.SpecialFolder.LocalApplicationData"/> (%LOCALAPPDATA%, %HOMEPATH%\AppData\Local) oder in <see cref="Environment.SpecialFolder.ApplicationData"/> 
         /// (%APPDATA%, %HOMEPATH%\AppData\Roaming) wird ein Unterverzeichnis entsprechend 'name' verwendet und in diesem die Datei 'filename'.
         /// </summary>
         /// <param name="name"></param>
         /// <param name="local"></param>
         /// <param name="file"></param>
         /// <param name="folder">expl. Verzeichnisvorgabe</param>
         public PersistentDataXml(string name, bool local = false, string file = "persist.xml", string folder = null) {
            filename = Path.Combine(string.IsNullOrEmpty(folder) ?
                                          Environment.GetFolderPath(local ?
                                                              Environment.SpecialFolder.LocalApplicationData :
                                                              Environment.SpecialFolder.ApplicationData) :        // Roaming
                                          folder,
                                     name,
                                     file);
            PersistentDataXml persistentDataXml = Load();
            if (persistentDataXml != null)
               dict = persistentDataXml.dict;
         }

         public void Save() {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
               Directory.CreateDirectory(Path.GetDirectoryName(filename));

            // fkt. mit Dictionary NICHT
            //XmlSerializer writer = new XmlSerializer(typeof(PersistentDataXml));
            //using (FileStream stream = System.IO.File.Create(filename)) {
            //   writer.Serialize(stream, this);
            //}

            string xmlString;
            using (var sw = new StringWriter()) {
               using (var writer = new XmlTextWriter(sw)) {
                  writer.Formatting = Formatting.Indented; // indent the Xml so it’s human readable
                  var serializer = new DataContractSerializer(typeof(PersistentDataXml));
                  serializer.WriteObject(writer, this);
                  writer.Flush();
                  xmlString = sw.ToString();
               }
            }
            File.WriteAllText(filename, xmlString);
         }

         public PersistentDataXml Load() {
            try {
               // fkt. mit Dictionary NICHT
               //XmlSerializer reader = new XmlSerializer(typeof(PersistentDataXml));
               //using (StreamReader stream = new StreamReader(filename)) {
               //   PersistentDataXml t = reader.Deserialize(stream) as PersistentDataXml;
               //   return t;
               //}

               string xmlString = File.ReadAllText(filename);
               PersistentDataXml t = null;
               using (var sr = new StringReader(xmlString)) {
                  using (var r = new XmlTextReader(sr)) {
                     var serializer = new DataContractSerializer(typeof(PersistentDataXml));
                     t = serializer.ReadObject(r) as PersistentDataXml;
                     t.filename = filename;
                  }
               }
               return t;

            } catch (DirectoryNotFoundException) {
            } catch (FileNotFoundException) {
            } catch {
               if (File.Exists(filename))
                  File.Delete(filename);
            }
            return null;
         }

         public void Set(string name, object data) {
            dict[name] = data;
         }

         public T Get<T>(string name, T dummy) {
            object v;
            if (dict.TryGetValue(name, out v) &&
                v.GetType() == typeof(T))
               return (T)v;
            return dummy;
         }

         public void SetList<T>(string name, List<T> lst, string separator = "\n") {
            Set(name, string.Join<T>(separator, lst));
         }

         public List<T> GetList<T>(string name, string separator = "\n") {
            List<T> lst = new List<T>();
            string txt = Get<string>(name, null);
            if (!string.IsNullOrEmpty(txt))
               foreach (string item in txt.Split(new string[] { separator }, StringSplitOptions.None)) {
                  lst.Add((T)Convert.ChangeType(item, typeof(T)));
               }
            return lst;
         }

      }

      protected PersistentDataXml data;

      /*
      public string ExampleString {
         get => data.Get(nameof(ExampleString), "");
         set => data.Set(nameof(ExampleString), value);
      }

      public double ExampleDouble {
         get => data.Get(nameof(ExampleDouble), 14.0);
         set => data.Set(nameof(ExampleDouble), value);
      }

      // Die Strings dürfen KEIN "\n" enthalten!

      public List<string> ExampleList {
         get => data.GetList<string>(nameof(ExampleList));
         set => data.SetList(nameof(ExampleList), value);
      }

      abgeleitet z.B.:

      using FSofTUtilsAssembly = FSofTUtils;
      public class AppData : FSofTUtilsAssembly.AppData { ... }

      */

      /// <summary>
      /// Entweder in <see cref="Environment.SpecialFolder.LocalApplicationData"/> (%LOCALAPPDATA%, %HOMEPATH%\AppData\Local) oder in <see cref="Environment.SpecialFolder.ApplicationData"/> 
      /// (%APPDATA%, %HOMEPATH%\AppData\Roaming) wird ein Unterverzeichnis entsprechend 'name' verwendet und in diesem die Datei 'filename'.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="local"></param>
      /// <param name="file"></param>
      /// <param name="folder">expl. Verzeichnisvorgabe</param>
      public AppData(string name, bool local = false, string file = "persist.xml", string folder = null) {
         data = new PersistentDataXml(name, local, file, folder);
      }

      public void Save() {
         data.Save();
      }

      public void Reload() {
         data = data.Load();
      }

   }
}
