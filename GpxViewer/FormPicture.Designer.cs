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
         this.components = new System.ComponentModel.Container();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.textBoxInfo = new System.Windows.Forms.TextBox();
         this.button_Copy = new System.Windows.Forms.Button();
         this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // pictureBox1
         // 
         this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.pictureBox1.Location = new System.Drawing.Point(0, 0);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(538, 396);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.pictureBox1.TabIndex = 0;
         this.pictureBox1.TabStop = false;
         // 
         // textBoxInfo
         // 
         this.textBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxInfo.Location = new System.Drawing.Point(34, 402);
         this.textBoxInfo.Name = "textBoxInfo";
         this.textBoxInfo.ReadOnly = true;
         this.textBoxInfo.Size = new System.Drawing.Size(504, 20);
         this.textBoxInfo.TabIndex = 1;
         this.textBoxInfo.WordWrap = false;
         // 
         // button_Copy
         // 
         this.button_Copy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button_Copy.Image = global::GpxViewer.Properties.Resources.copy;
         this.button_Copy.Location = new System.Drawing.Point(0, 396);
         this.button_Copy.Name = "button_Copy";
         this.button_Copy.Size = new System.Drawing.Size(30, 28);
         this.button_Copy.TabIndex = 0;
         this.toolTip1.SetToolTip(this.button_Copy, "Bild in die Zwischenablage kopieren");
         this.button_Copy.UseVisualStyleBackColor = true;
         this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
         // 
         // FormPicture
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(539, 424);
         this.Controls.Add(this.button_Copy);
         this.Controls.Add(this.textBoxInfo);
         this.Controls.Add(this.pictureBox1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.KeyPreview = true;
         this.Name = "FormPicture";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "FormPicture";
         this.Load += new System.EventHandler(this.FormPicture_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormPicture_KeyDown);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBoxInfo;
      private System.Windows.Forms.Button button_Copy;
      private System.Windows.Forms.ToolTip toolTip1;
   }
}