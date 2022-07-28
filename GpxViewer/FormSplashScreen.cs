using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormSplashScreen : Form {


      public FormSplashScreen() {
         InitializeComponent();

      }

      public void End() {
         if (!Disposing &&
             !IsDisposed) {
            while (!IsHandleCreated) ;
            Invoke(new Action(() => Close()));
         }
      }

      public void AppendText(string txt) {
         if (!Disposing &&
             !IsDisposed) {
            while (!IsHandleCreated) ;
            Invoke(new Action(() => textBox1.AppendText(txt)));
         }
      }

      public void AppendTextLine(string txt) {
         AppendText(txt + Environment.NewLine);
      }

      private void FormStartInfo_Shown(object sender, EventArgs e) {

      }
   }
}
