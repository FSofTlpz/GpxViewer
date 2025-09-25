using System.ComponentModel;
using System.Text;

namespace GpxViewer {
   public partial class FormObjectInfo : Form {

      public ListBox InfoList => listBox_Info;


      public FormObjectInfo() {
         InitializeComponent();

         KeyPreview = true;
      }

      protected override void OnClosing(CancelEventArgs e) {
         base.OnClosing(e);
         Owner.RemoveOwnedForm(this);     // Owner ist danach null !
         ClearListBox();
      }

      protected override void OnKeyDown(KeyEventArgs e) {
         base.OnKeyDown(e);

         switch (e.KeyData) {
            case Keys.Escape:
               Close();
               break;

            case Keys.Control | Keys.C:
               if (0 <= listBox_Info.SelectedIndex)
                  copyText2Clipboard(listBox_Info.SelectedIndex);
               break;
         }
      }

      /// <summary>
      /// leert die Listbox
      /// </summary>
      public void ClearListBox() {
         listBox_Info.Items.Clear();
      }

      void copyText2Clipboard(int listidx) {
         if (0 <= listidx && listidx < listBox_Info.Items.Count) {
            string? txt = listBox_Info.Items[listidx].ToString();
            if (txt != null)
               Clipboard.SetText(txt);
         }
      }

      private void ToolStripMenuItem_Copy_Click(object sender, System.EventArgs e) {
         if (0 <= listBox_Info.SelectedIndex)
            copyText2Clipboard(listBox_Info.SelectedIndex);
      }

      private void ToolStripMenuItem_CopyAll_Click(object sender, System.EventArgs e) {
         StringBuilder sb = new StringBuilder();
         foreach (var item in listBox_Info.Items)
            sb.AppendLine(item.ToString());
         if (sb.Length > 0)
            Clipboard.SetText(sb.ToString());

      }

      private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
         ToolStripMenuItem_Copy.Enabled = 0 <= listBox_Info.SelectedIndex;
      }
   }
}
