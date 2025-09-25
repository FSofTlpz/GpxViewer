namespace GpxViewer {
   partial class FormInfo {
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
         textBoxInfo = new TextBox();
         pictureBox1 = new PictureBox();
         button_End = new Button();
         ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
         SuspendLayout();
         // 
         // textBoxInfo
         // 
         textBoxInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         textBoxInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
         textBoxInfo.Location = new Point(70, 14);
         textBoxInfo.Margin = new Padding(4, 3, 4, 3);
         textBoxInfo.Multiline = true;
         textBoxInfo.Name = "textBoxInfo";
         textBoxInfo.ReadOnly = true;
         textBoxInfo.ScrollBars = ScrollBars.Both;
         textBoxInfo.Size = new Size(406, 452);
         textBoxInfo.TabIndex = 3;
         textBoxInfo.WordWrap = false;
         // 
         // pictureBox1
         // 
         pictureBox1.Location = new Point(14, 14);
         pictureBox1.Margin = new Padding(4, 3, 4, 3);
         pictureBox1.Name = "pictureBox1";
         pictureBox1.Size = new Size(37, 37);
         pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
         pictureBox1.TabIndex = 2;
         pictureBox1.TabStop = false;
         // 
         // button_End
         // 
         button_End.Anchor = AnchorStyles.Bottom;
         button_End.DialogResult = DialogResult.Cancel;
         button_End.Location = new Point(209, 483);
         button_End.Margin = new Padding(4, 3, 4, 3);
         button_End.Name = "button_End";
         button_End.Size = new Size(88, 27);
         button_End.TabIndex = 4;
         button_End.Text = "&schließen";
         button_End.UseVisualStyleBackColor = true;
         button_End.Click += button_End_Click;
         // 
         // FormInfo
         // 
         AcceptButton = button_End;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = button_End;
         ClientSize = new Size(503, 524);
         ControlBox = false;
         Controls.Add(button_End);
         Controls.Add(textBoxInfo);
         Controls.Add(pictureBox1);
         Margin = new Padding(4, 3, 4, 3);
         Name = "FormInfo";
         Text = "Info";
         Load += FormInfo_Load;
         KeyDown += FormInfo_KeyDown;
         ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private System.Windows.Forms.TextBox textBoxInfo;
      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.Button button_End;
   }
}