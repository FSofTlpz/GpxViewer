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
         this.textBoxInfo = new System.Windows.Forms.TextBox();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.button_End = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // textBoxInfo
         // 
         this.textBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxInfo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBoxInfo.Location = new System.Drawing.Point(60, 12);
         this.textBoxInfo.Multiline = true;
         this.textBoxInfo.Name = "textBoxInfo";
         this.textBoxInfo.ReadOnly = true;
         this.textBoxInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBoxInfo.Size = new System.Drawing.Size(332, 392);
         this.textBoxInfo.TabIndex = 3;
         this.textBoxInfo.WordWrap = false;
         // 
         // pictureBox1
         // 
         this.pictureBox1.Location = new System.Drawing.Point(12, 12);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(32, 32);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
         this.pictureBox1.TabIndex = 2;
         this.pictureBox1.TabStop = false;
         // 
         // button_End
         // 
         this.button_End.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
         this.button_End.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.button_End.Location = new System.Drawing.Point(171, 419);
         this.button_End.Name = "button_End";
         this.button_End.Size = new System.Drawing.Size(75, 23);
         this.button_End.TabIndex = 4;
         this.button_End.Text = "&schließen";
         this.button_End.UseVisualStyleBackColor = true;
         this.button_End.Click += new System.EventHandler(this.button_End_Click);
         // 
         // FormInfo
         // 
         this.AcceptButton = this.button_End;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_End;
         this.ClientSize = new System.Drawing.Size(415, 454);
         this.ControlBox = false;
         this.Controls.Add(this.button_End);
         this.Controls.Add(this.textBoxInfo);
         this.Controls.Add(this.pictureBox1);
         this.Name = "FormInfo";
         this.Text = "Info";
         this.Load += new System.EventHandler(this.FormInfo_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormInfo_KeyDown);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox textBoxInfo;
      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.Button button_End;
   }
}