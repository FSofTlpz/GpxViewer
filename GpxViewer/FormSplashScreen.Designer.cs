namespace GpxViewer {
   partial class FormSplashScreen {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent() {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSplashScreen));
         textBox1 = new TextBox();
         pictureBox1 = new PictureBox();
         panel1 = new Panel();
         ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
         panel1.SuspendLayout();
         SuspendLayout();
         // 
         // textBox1
         // 
         textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         textBox1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
         textBox1.Location = new Point(188, 14);
         textBox1.Margin = new Padding(4, 3, 4, 3);
         textBox1.Multiline = true;
         textBox1.Name = "textBox1";
         textBox1.ReadOnly = true;
         textBox1.ScrollBars = ScrollBars.Both;
         textBox1.Size = new Size(504, 308);
         textBox1.TabIndex = 0;
         textBox1.WordWrap = false;
         // 
         // pictureBox1
         // 
         pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
         pictureBox1.InitialImage = null;
         pictureBox1.Location = new Point(4, 77);
         pictureBox1.Margin = new Padding(4, 3, 4, 3);
         pictureBox1.Name = "pictureBox1";
         pictureBox1.Size = new Size(177, 165);
         pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
         pictureBox1.TabIndex = 1;
         pictureBox1.TabStop = false;
         // 
         // panel1
         // 
         panel1.BorderStyle = BorderStyle.FixedSingle;
         panel1.Controls.Add(pictureBox1);
         panel1.Controls.Add(textBox1);
         panel1.Dock = DockStyle.Fill;
         panel1.Location = new Point(0, 0);
         panel1.Margin = new Padding(4, 3, 4, 3);
         panel1.Name = "panel1";
         panel1.Size = new Size(708, 338);
         panel1.TabIndex = 2;
         // 
         // FormSplashScreen
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(708, 338);
         ControlBox = false;
         Controls.Add(panel1);
         FormBorderStyle = FormBorderStyle.None;
         Margin = new Padding(4, 3, 4, 3);
         Name = "FormSplashScreen";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "FormStartInfo";
         Shown += FormStartInfo_Shown;
         ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
         panel1.ResumeLayout(false);
         panel1.PerformLayout();
         ResumeLayout(false);
      }

      #endregion

      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.Panel panel1;
   }
}