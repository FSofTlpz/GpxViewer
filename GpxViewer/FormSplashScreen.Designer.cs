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
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.panel1 = new System.Windows.Forms.Panel();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.panel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // textBox1
         // 
         this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox1.Location = new System.Drawing.Point(161, 12);
         this.textBox1.Multiline = true;
         this.textBox1.Name = "textBox1";
         this.textBox1.ReadOnly = true;
         this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBox1.Size = new System.Drawing.Size(433, 258);
         this.textBox1.TabIndex = 0;
         this.textBox1.WordWrap = false;
         // 
         // pictureBox1
         // 
         this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
         this.pictureBox1.InitialImage = null;
         this.pictureBox1.Location = new System.Drawing.Point(3, 67);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(152, 143);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.pictureBox1.TabIndex = 1;
         this.pictureBox1.TabStop = false;
         // 
         // panel1
         // 
         this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.panel1.Controls.Add(this.pictureBox1);
         this.panel1.Controls.Add(this.textBox1);
         this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panel1.Location = new System.Drawing.Point(0, 0);
         this.panel1.Name = "panel1";
         this.panel1.Size = new System.Drawing.Size(607, 284);
         this.panel1.TabIndex = 2;
         // 
         // FormSplashScreen
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(607, 284);
         this.ControlBox = false;
         this.Controls.Add(this.panel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
         this.Name = "FormSplashScreen";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "FormStartInfo";
         this.Shown += new System.EventHandler(this.FormStartInfo_Shown);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.panel1.ResumeLayout(false);
         this.panel1.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.Panel panel1;
   }
}