using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GarminImageCreator;

namespace GpxViewer {
   public partial class FormGarminInfo : Form {

      public ListBox InfoList {
         get {
            return listBox_Info;
         }
      }


      public FormGarminInfo() {
         InitializeComponent();
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
               //case Keys.Enter:
               Close();
               break;
         }
      }

      private void listBox_Info_DrawItem(object sender, DrawItemEventArgs e) {
         e.DrawBackground();
         if (e.Index < 0)
            return;

         SearchObject so = (sender as ListBox).Items[e.Index] as SearchObject;

         if (so != null) {
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
            e.Graphics.DrawString(txt, e.Font, myTextBrush, 35, e.Bounds.Top + (e.Bounds.Height - e.Font.Height) / 2, StringFormat.GenericDefault);

            e.DrawFocusRectangle();
         }

      }

      private void listBox_Info_MeasureItem(object sender, MeasureItemEventArgs e) {
         SearchObject so = (sender as ListBox).Items[e.Index] as SearchObject;
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

   }
}
