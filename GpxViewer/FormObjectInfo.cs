using System.ComponentModel;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormObjectInfo : Form {

      public ListBox InfoList {
         get {
            return listBox_Info;
         }
      }


      public FormObjectInfo() {
         InitializeComponent();
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
               //case Keys.Enter:
               Close();
               break;
         }
      }

      /// <summary>
      /// leert die Listbox
      /// </summary>
      public void ClearListBox() {
         listBox_Info.Items.Clear();
      }

   }
}
