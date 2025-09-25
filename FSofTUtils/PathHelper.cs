using System;
using System.Collections;
using System.IO;

// Stand 6.9.2020

namespace FSofTUtils {
   public class PathHelper {

      /// <summary>
      /// ersetzt ev. vorhandene Umgebungsvariablen im Text
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      static public string ReplaceEnvironmentVars(string path) {
         if (path.Contains("%"))
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
               if (de.Value != null)
                  path = path.Replace("%" + de.Key + "%", de.Value.ToString());
         return path;
      }

      /// <summary>
      /// ersetzt den Anfang des Textes nach Möglichkeit durch (irgendeine) eine Umgebungsvariablen
      /// <para>Es wird die Umgebungsvar verwendet, die den längsten Text ersetzt.</para>
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      static public string UseEnvironmentVars4Path(string path) {
         // u.a.:
         //    ALLUSERSPROFILE = C:\ProgramData
         //    APPDATA = C:\Users\puf\AppData\Roaming
         //    HOMEPATH=\Users\puf
         //    LOCALAPPDATA=C:\Users\puf\AppData\Local
         //    USERPROFILE=C:\Users\puf
         bool unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
         string varname = string.Empty;
         int contentlength = 0;
         foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
            string? content = de.Value?.ToString();
            if (content != null &&
                pathStartWithText(path, content) &&
                contentlength < content.Length &&
                ((path.Length == content.Length) ||
                 (path[content.Length] == Path.DirectorySeparatorChar))) {
#pragma warning disable CS8600 // Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
               varname = Convert.ToString(de.Key);
#pragma warning restore CS8600 // Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
               contentlength = content.Length;
            }
         }
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
         if (varname.Length > 0)
            path = "%" + varname + "%" + path.Substring(contentlength);
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
         return path;
      }

      /// <summary>
      /// ersetzt nach Möglichkeit den Anfang des Textes durch die Umgebungsvariable
      /// </summary>
      /// <param name="path"></param>
      /// <param name="varname"></param>
      /// <returns></returns>
      static public string UseEnvironmentVar4Path(string path, string varname) {
         string? content = Environment.GetEnvironmentVariable(varname);
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
         if (!string.IsNullOrEmpty(content) &&
             content.Length <= path.Length &&
             pathStartWithText(path, content) &&
             (path.Length == content.Length) ||
             (path[content.Length] == Path.DirectorySeparatorChar)) {
            bool unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            if ((unix && path.Substring(0, content.Length) == content) ||
                (!unix && path.Substring(0, content.Length).ToLower() == content.ToLower()))
               path = "%" + varname + "%" + path.Substring(content.Length);
         }
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
         return path;
      }

      /// <summary>
      /// Beginnt der Pfad-Text mit dem Text (case-insensitiv unter Windows)?
      /// </summary>
      /// <param name="path"></param>
      /// <param name="text"></param>
      /// <returns></returns>
      static bool pathStartWithText(string path, string text) {
         bool unix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
         if ((unix && path.StartsWith(text)) ||
             (!unix && path.ToLower().StartsWith(text.ToLower())))
            return true;
         return false;
      }


      /// <summary>
      /// liefert den abs. Pfad bezüglich des akt. Arbeitsverzeichnisses
      /// <para>Handelt es sich schon um einen abs. Pfad, wird er unverändert zurückgeliefert.</para>
      /// </summary>
      /// <param name="relpath"></param>
      /// <returns></returns>
      static public string GetFullPathAppliedCurrentDirectory(string relpath) {
         return GetFullPathAppliedDirectory(relpath, Directory.GetCurrentDirectory());
      }

      /// <summary>
      /// liefert den abs. Pfad bezüglich des Verzeichnisses
      /// <para>Handelt es sich beim "Verzeichnis" um eine ex. Datei, wird deren Verzeichnis verwendet</para>
      /// <para>Handelt es sich schon um einen abs. Pfad oder ist das "Verzeichnis" kein abs. Pfad, wird der gelieferte Pfad unverändert zurückgeliefert.</para>
      /// </summary>
      /// <param name="relpath"></param>
      /// <param name="absdirectoryorfile"></param>
      /// <returns></returns>
      static public string GetFullPathAppliedDirectory(string relpath, string absdirectoryorfile) {
         string? path = File.Exists(absdirectoryorfile) ?
                              Path.GetDirectoryName(absdirectoryorfile) :
                              absdirectoryorfile;
         return Path.IsPathRooted(relpath) ||
                !Path.IsPathRooted(absdirectoryorfile) ?
                     relpath :
                     path != null ? Path.GetFullPath(Path.Combine(path, relpath)) :
                     relpath;
      }

      /// <summary>
      /// der absolute oder relative Filename 'sAbsOrRelFile' wird (wenn möglich) bezüglich 'sAbsOrRelPath' relativ gemacht;
      /// ist 'sAbsOrRelFile' oder 'sAbsOrRelPath' relativ, wird es zunächst bezüglich das akt. Arbeitsverzeichnisses absolut "gemacht"
      /// </summary>
      /// <param name="sAbsOrRelFile"></param>
      /// <param name="sAbsOrRelPath"></param>
      /// <returns></returns>
      static public string GetRelativPath(string sAbsOrRelFile, string sAbsOrRelPath) {
         bool bCaseSensitive = System.Environment.OSVersion.Platform == PlatformID.Unix;
         string sPath = Path.IsPathRooted(sAbsOrRelPath) ? sAbsOrRelPath : Path.GetFullPath(sAbsOrRelPath);
         string sFile = Path.IsPathRooted(sAbsOrRelFile) ? sAbsOrRelFile : Path.GetFullPath(sAbsOrRelFile);
         string? sDestRoot = Path.GetPathRoot(sPath);
         if (sDestRoot != null &&
             string.Compare(Path.GetPathRoot(sFile), sDestRoot, !bCaseSensitive) == 0) {        // Ist die Root gleich?
            string[] sDestElements = sPath.Substring(sDestRoot.Length).Split(Path.DirectorySeparatorChar);
            string[] sFileElements = sFile.Substring(sDestRoot.Length).Split(Path.DirectorySeparatorChar);
            // Beginnend mit der obersten Verzeichnisebene werden alle Ebenen auf Gleichheit verglichen.
            int j = 0;
            for (; j < sDestElements.Length && j < sFileElements.Length - 1; j++)
               if (string.Compare(sDestElements[j], sFileElements[j], !bCaseSensitive) != 0)
                  break;
            // j ist der 1. ungleiche Ebenenindex.
            sFile = "";
            for (int k = j; k < sDestElements.Length; k++)
               sFile += ".." + Path.DirectorySeparatorChar;
            return sFile + string.Join(Path.DirectorySeparatorChar.ToString(), sFileElements, j, sFileElements.Length - j);
         }
         return "";
      }


   }
}
