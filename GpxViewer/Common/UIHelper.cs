using System;
using System.Text;
using System.IO;
using System.Diagnostics;
#if Android
using Xamarin.Forms;
using System.Threading.Tasks;
#else
using System.Drawing;
using System.Windows.Forms;
#endif

#if Android
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif

   internal static class UIHelper {

      /// <summary>
      /// kann gesetzt werden, wenn der Parameter 'logfile' nicht verwendet werden soll (logfile == null)
      /// </summary>
      static public string ExceptionLogfile = null;

#if Android

      /// <summary>
      /// kann gesetzt werden, wenn der Parameter 'page' nicht verwendet werden soll (page == null)
      /// </summary>
      static public Page ParentPage = null;

      /// <summary>
      /// zeigt einen Info-Text an
      /// </summary>
      /// <param name="page"></param>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      static public async Task ShowInfoMessage(Page page, string txt, string caption = "Info") {
         if (page == null)
            page = ParentPage;
         if (page != null) {
            try {
               await FSofTUtils.Xamarin.Helper.MessageBox(page,
                                                          caption,
                                                          txt,
                                                          "weiter");
            } catch (Exception ex) {
               Debug.WriteLine(nameof(ShowInfoMessage) + "(): " + caption + ", " + txt + ": " + ex.Message);
            }
         }
      }

      /// <summary>
      /// zeigt einen Fehlertext an (z.B. aus einer Exception)
      /// </summary>
      /// <param name="page"></param>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      static public async Task ShowErrorMessage(Page page, string txt, string caption = "Fehler") =>
         await ShowInfoMessage(page, txt, caption);

      /// <summary>
      /// Exception anzeigen
      /// </summary>
      /// <param name="ex"></param>
      /// <param name="exit">wenn true, dann Prog sofort abbrechen</param>
      /// <returns></returns>
      static public async Task ShowExceptionMessage(Page page, Exception ex, string logfile, bool exit) =>
         await ShowExceptionMessage(page, null, "Fehler", ex, logfile, exit);

      static public async Task ShowExceptionMessage(Page page, string caption, Exception ex, string logfile, bool exit) {
         await ShowExceptionMessage(page, null, caption, ex, logfile, exit);
      }

      static public async Task ShowExceptionMessage(Page page, string message, string caption, Exception ex, string logfile, bool exit) {
         if (message == null)
            message = "";
         message += getExceptionMessage(ex);

         if (string.IsNullOrEmpty(logfile))
            logfile = ExceptionLogfile;

         if (!string.IsNullOrEmpty(logfile))
            try {
               File.AppendAllText(logfile, DateTime.Now.ToString("G") + " " + caption + ": " + message);
            } catch { }

         await ShowErrorMessage(page, caption, message);
         if (exit) {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
         }
      }

      static public async Task<bool> ShowYesNoQuestion_StdIsYes(Page page, string txt, string caption) =>
         await FSofTUtils.Xamarin.Helper.MessageBox(page, caption, txt, "ja", "nein");

      static public async Task<bool> ShowYesNoQuestion_StdIsNo(Page page, string txt, string caption) =>
         await FSofTUtils.Xamarin.Helper.MessageBox(page, caption, txt, "nein", "ja");

#else

      /// <summary>
      /// zeigt einen Info-Text an
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      static public void ShowInfoMessage(string txt, string caption = "Info") =>
         FSofTUtils.MyMessageBox.Show(txt,
                                      caption,
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information,
                                      MessageBoxDefaultButton.Button1,
                                      null,
                                      false,
                                      true,
                                      false);

      /// <summary>
      /// zeigt einen Fehlertext an (z.B. aus einer Exception)
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      static public void ShowErrorMessage(string txt, string caption = "Fehler") =>
         FSofTUtils.MyMessageBox.Show(txt,
                                      caption,
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error,
                                      MessageBoxDefaultButton.Button1,
                                      Color.FromArgb(255, 220, 220));

      static public void ShowExceptionMessage(Form mainform, Exception ex, string logfile, bool exit) =>
         ShowExceptionMessage(mainform, null, "Fehler", ex, logfile, exit);

      static public void ShowExceptionMessage(Form mainform, string caption, Exception ex, string logfile, bool exit) =>
         ShowExceptionMessage(mainform, null, caption, ex, logfile, exit);

      static public void ShowExceptionMessage(Form mainform, string message, string caption, Exception ex, string logfile, bool exit) {
         if (message == null)
            message = "";
         message += getExceptionMessage(ex);

         if (string.IsNullOrEmpty(logfile))
            logfile = ExceptionLogfile;

         if (!string.IsNullOrEmpty(logfile))
            try {
               File.AppendAllText(logfile, DateTime.Now.ToString("G") + " " + caption + ": " + message);
            } catch { }

         ShowErrorMessage(message, caption);
         if (mainform != null && exit) {
            mainform.Close();
         }
      }

      /// <summary>
      /// nach Möglichkeit "ausführliche" Anzeige einer Exception
      /// </summary>
      /// <param name="ex"></param>
      static public void ShowExceptionError(Exception ex, string logfile = null) =>
         ShowExceptionMessage(null, ex, logfile, false);

      static public bool ShowYesNoQuestion_IsYes(string txt, string caption) =>
         FSofTUtils.MyMessageBox.Show(txt,
                                      caption,
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question,
                                      MessageBoxDefaultButton.Button2) == DialogResult.Yes;

#endif

      static string getExceptionMessage(Exception ex) {
         StringBuilder sb = new StringBuilder();

         do {

            sb.AppendLine(ex.Message);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(ex.StackTrace)) {
               sb.AppendLine();
               sb.AppendLine("StackTrace:");
               sb.AppendLine(ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(ex.Source)) {
               sb.AppendLine();
               sb.AppendLine("Source:");
               sb.AppendLine(ex.Source);
            }

            ex = ex.InnerException;
         } while (ex != null);

         return sb.ToString();
      }

   }
}
