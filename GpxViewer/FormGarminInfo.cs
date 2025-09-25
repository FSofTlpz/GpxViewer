using System.ComponentModel;
using System.Text;
using GarminImageCreator;

namespace GpxViewer {
   public partial class FormGarminInfo : Form {

      public ListBox InfoList => listBox_Info;


      public FormGarminInfo() {
         InitializeComponent();

         KeyPreview = true;
      }

      private void FormGarminInfo_Load(object sender, EventArgs e) {
      }

      protected override void OnClosing(CancelEventArgs e) {
         base.OnClosing(e);
         Owner.RemoveOwnedForm(this);     // Owner ist danach null !
         ClearListBox();
      }

      private void FormGarminInfo_KeyDown(object sender, KeyEventArgs e) {
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

      private void listBox_Info_DrawItem(object sender, DrawItemEventArgs e) {
         e.DrawBackground();
         if (e.Index < 0)
            return;

         object? item = (sender as ListBox).Items[e.Index];

         if (item != null && item is SearchObject) {
            SearchObject so = (SearchObject)item;
            Brush myTextBrush = Brushes.Black;
            switch (so.Objecttype) {
               case SearchObject.ObjectType.Area:
                  myTextBrush = Brushes.DarkGreen;
                  break;

               case SearchObject.ObjectType.Line:
                  myTextBrush = Brushes.Blue;
                  break;

               case SearchObject.ObjectType.Point:
                  myTextBrush = Brushes.Black;
                  break;
            }

            if (so.Bitmap != null) {
               if (so.Bitmap.Height < e.Bounds.Height)
                  e.Graphics.DrawImage(so.Bitmap, e.Bounds.Left, e.Bounds.Top + (e.Bounds.Height - so.Bitmap.Height) / 2);
               else
                  e.Graphics.DrawImage(so.Bitmap, e.Bounds.Left, e.Bounds.Top);
            }

            string txt = so.TypeName;
            if (so.Name != "")
               if (txt != "")
                  txt += ": " + so.Name;
               else
                  txt = so.Name;
            if (e.Font != null)
               e.Graphics.DrawString(txt, e.Font, myTextBrush, 35, e.Bounds.Top + (e.Bounds.Height - e.Font.Height) / 2, StringFormat.GenericDefault);

            e.DrawFocusRectangle();
         }
      }

      private void listBox_Info_MeasureItem(object sender, MeasureItemEventArgs e) {
         SearchObject? so = (sender as ListBox).Items[e.Index] as SearchObject;
         if (so.Bitmap != null &&
            so.Bitmap.Height > e.ItemHeight)
            e.ItemHeight = so.Bitmap.Height;
      }

      /// <summary>
      /// leert die Listbox mit Disposing für die Bitmaps
      /// </summary>
      public void ClearListBox() {
         for (int i = 0; i < listBox_Info.Items.Count; i++) {
            if (listBox_Info.Items[i] is SearchObject)
               (listBox_Info.Items[i] as SearchObject).Bitmap.Dispose();
         }
         listBox_Info.Items.Clear();
      }

      void copyText2Clipboard(int listidx) {
         if (0 <= listidx && listidx < listBox_Info.Items.Count) {
            SearchObject? so = listBox_Info.Items[listidx] as SearchObject;
            Clipboard.SetText(so.TypeName + ": " + so.Name);
         }
      }

      private void ToolStripMenuItem_Copy_Click(object sender, EventArgs e) {
         if (0 <= listBox_Info.SelectedIndex)
            copyText2Clipboard(listBox_Info.SelectedIndex);
      }

      private void ToolStripMenuItem_CopyAll_Click(object sender, EventArgs e) {
         StringBuilder sb = new StringBuilder();
         foreach (SearchObject so in listBox_Info.Items)
            sb.AppendLine(so.TypeName + ": " + so.Name);
         if (sb.Length > 0)
            Clipboard.SetText(sb.ToString());
      }

      private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
         ToolStripMenuItem_Copy.Enabled = 0 <= listBox_Info.SelectedIndex;
      }
   }
}
