using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormMapLocationName : Form {
      public FormMapLocationName() {
         InitializeComponent();
      }

      public string PositionName {
         get {
            return textBox_Name.Text;
         }
         set {
            textBox_Name.Text = value;
         }
      }

      private void FormMapLocationName_Load(object sender, EventArgs e) {

      }
   }
}
