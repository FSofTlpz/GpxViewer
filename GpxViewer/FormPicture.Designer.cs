namespace GpxViewer {
   partial class FormPicture {
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
         components = new System.ComponentModel.Container();
         pictureBox1 = new PictureBox();
         textBoxInfo = new TextBox();
         button_Copy = new Button();
         toolTip1 = new ToolTip(components);
         ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
         SuspendLayout();
         // 
         // pictureBox1
         // 
         pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         pictureBox1.Location = new Point(0, 0);
         pictureBox1.Margin = new Padding(4, 3, 4, 3);
         pictureBox1.Name = "pictureBox1";
         pictureBox1.Size = new Size(619, 457);
         pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
         pictureBox1.TabIndex = 0;
         pictureBox1.TabStop = false;
         // 
         // textBoxInfo
         // 
         textBoxInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         textBoxInfo.Location = new Point(40, 464);
         textBoxInfo.Margin = new Padding(4, 3, 4, 3);
         textBoxInfo.Name = "textBoxInfo";
         textBoxInfo.ReadOnly = true;
         textBoxInfo.Size = new Size(578, 23);
         textBoxInfo.TabIndex = 1;
         textBoxInfo.WordWrap = false;
         // 
         // button_Copy
         // 
         button_Copy.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
         button_Copy.Image = Properties.Resources.copy;
         button_Copy.Location = new Point(0, 457);
         button_Copy.Margin = new Padding(4, 3, 4, 3);
         button_Copy.Name = "button_Copy";
         button_Copy.Size = new Size(35, 32);
         button_Copy.TabIndex = 0;
         toolTip1.SetToolTip(button_Copy, "Bild in die Zwischenablage kopieren");
         button_Copy.UseVisualStyleBackColor = true;
         button_Copy.Click += button_Copy_Click;
         // 
         // FormPicture
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(620, 489);
         Controls.Add(button_Copy);
         Controls.Add(textBoxInfo);
         Controls.Add(pictureBox1);
         FormBorderStyle = FormBorderStyle.SizableToolWindow;
         KeyPreview = true;
         Margin = new Padding(4, 3, 4, 3);
         Name = "FormPicture";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "FormPicture";
         Load += FormPicture_Load;
         KeyDown += FormPicture_KeyDown;
         ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBoxInfo;
      private System.Windows.Forms.Button button_Copy;
      private System.Windows.Forms.ToolTip toolTip1;
   }
}