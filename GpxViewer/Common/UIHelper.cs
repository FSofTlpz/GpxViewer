using System.Text;
using System.Diagnostics;

#if ANDROID
namespace TrackEddi.Common {
#else
namespace GpxViewer.Common {
#endif

   public static class UIHelper {

      public class BusyEventArgs {

         /// <summary>
         /// kann null sein
         /// </summary>
#if ANDROID
         public readonly Page? Page;
#else
         public readonly Form? Page;
#endif

         public readonly bool Busy;

         public BusyEventArgs(
#if ANDROID
                  Page? page,
#else
                  Form? page,
#endif
                  bool busy) {
            Page = page;
            Busy = busy;
         }

      }

      /// <summary>
      /// der Busy-Status soll gesetzt werden
      /// </summary>
      public static event EventHandler<BusyEventArgs>? SetBusyStatusEvent;

      /// <summary>
      /// kann gesetzt werden, wenn der Parameter 'logfile' nicht verwendet werden soll (logfile == null)
      /// </summary>
      static public string? ExceptionLogfile = null;

#if ANDROID

      /// <summary>
      /// kann gesetzt werden, wenn der Parameter 'page' nicht verwendet werden soll (page == null)
      /// </summary>
      static public Page? ParentPage = null;

      /// <summary>
      /// zeigt einen Info-Text an
      /// </summary>
      /// <param name="page"></param>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      static public async Task ShowInfoMessage(Page? page, string txt, string caption = "Info") {
         if (page == null)
            page = ParentPage;
         if (page != null) {
            try {
               await FSofTUtils.OSInterface.Helper.MessageBox(page,
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
      static public async Task ShowExceptionMessage(Page page, Exception ex, string? logfile, bool exit) =>
         await ShowExceptionMessage(page, null, "Fehler", ex, logfile, exit);

      static public async Task ShowExceptionMessage(Page page, string caption, Exception ex, string? logfile, bool exit) =>
         await ShowExceptionMessage(page, null, caption, ex, logfile, exit);

      static public async Task ShowExceptionMessage(Page page, string? message, string caption, Exception ex, string? logfile, bool exit) {
         if (message == null)
            message = "";
         message += GetExceptionMessage(ex);
         Message2Logfile(caption, message, logfile);
         await ShowErrorMessage(page, message, caption);
         if (exit) {
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
         }
      }

      /// <summary>
      /// Ja/Nein-Frage; NUR bei expliziter Auswahl von "Ja" wird true geliefert
      /// </summary>
      /// <param name="page"></param>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      /// <returns></returns>
      static public async Task<bool> ShowYesNoQuestion_RealYes(Page page, string txt, string caption) =>
         await FSofTUtils.OSInterface.Helper.MessageBox(page, caption, txt, "ja", "nein");

      /// <summary>
      /// Ja/Nein-Frage; NUR bei expliziter Auswahl von "Nein" wird true geliefert
      /// </summary>
      /// <param name="page"></param>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      /// <returns></returns>
      static public async Task<bool> ShowYesNoQuestion_RealNo(Page page, string txt, string caption) =>
         await FSofTUtils.OSInterface.Helper.MessageBox(page, caption, txt, "nein", "ja");

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

      static public void ShowExceptionMessage(Form? mainform, Exception ex, string? logfile, bool exit) =>
         ShowExceptionMessage(mainform, null, "Fehler", ex, logfile, exit);

      static public void ShowExceptionMessage(Form? mainform, string caption, Exception ex, string? logfile, bool exit) =>
         ShowExceptionMessage(mainform, null, caption, ex, logfile, exit);

      static public void ShowExceptionMessage(Form? mainform, string? message, string caption, Exception ex, string? logfile, bool exit) {
         if (message == null)
            message = "";
         message += GetExceptionMessage(ex);

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
      static public void ShowExceptionError(Exception ex, string? logfile = null) =>
         ShowExceptionMessage(null, ex, logfile, false);

      /// <summary>
      /// Ja/Nein-Frage; NUR bei expliziter Auswahl von "Ja" wird true geliefert ("Nein" ist voreingestellt)
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      /// <returns></returns>
      static public bool ShowYesNoQuestion_RealYes(string txt, string caption) =>
         FSofTUtils.MyMessageBox.Show(txt,
                                      caption,
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question,
                                      MessageBoxDefaultButton.Button2) == DialogResult.Yes;

      /// <summary>
      /// Ja/Nein-Frage; NUR bei expliziter Auswahl von "Nein" wird true geliefert ("Ja" ist voreingestellt)
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      /// <returns></returns>
      static public bool ShowYesNoQuestion_RealNo(string txt, string caption) =>
         FSofTUtils.MyMessageBox.Show(txt,
                                      caption,
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question,
                                      MessageBoxDefaultButton.Button1) == DialogResult.No;

#endif

      static public void Message2Logfile(string? theme, string? message, string? logfile) {
         if (string.IsNullOrEmpty(logfile))
            logfile = ExceptionLogfile;

         if (!string.IsNullOrEmpty(logfile)) {
            if (theme == null)
               theme = string.Empty;
            else
               theme = " " + theme;
            if (message == null)
               message = string.Empty;
            try {
               File.AppendAllText(logfile, DateTime.Now.ToString("O") + " " + theme + ": " + message);
            } catch { }
         }
      }


      static public string GetExceptionMessage(Exception? ex) {
         StringBuilder sb = new StringBuilder();

         if (ex != null)
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

      /// <summary>
      /// löst das <see cref="SetBusyStatusEvent"/> aus
      /// </summary>
      /// <param name="page"></param>
      /// <param name="busy"></param>
      static public void SetBusyStatus(
#if ANDROID
                           Page? page,
#else
                           Form? page,
#endif
                           bool busy = true) =>
         SetBusyStatusEvent?.Invoke(null, new BusyEventArgs(page, busy));
   }
}
