namespace GpxViewer {
   public partial class FormTextInput : Form {

      public string Usertext {
         get => textBox_Name.Text;
         set => textBox_Name.Text = value;
      }

      public string Usercaption {
         get => Text;
         set => Text = value;
      }


      public FormTextInput() {
         InitializeComponent();
      }

      private void FormTextInput_Load(object sender, EventArgs e) {

      }
   }
}
