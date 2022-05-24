namespace FSofTUtils {
   partial class MyMessageBox {
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
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.textBoxInfo = new System.Windows.Forms.TextBox();
         this.flowLayoutPanelButton = new System.Windows.Forms.FlowLayoutPanel();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // pictureBox1
         // 
         this.pictureBox1.Location = new System.Drawing.Point(12, 12);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(32, 32);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
         this.pictureBox1.TabIndex = 0;
         this.pictureBox1.TabStop = false;
         // 
         // textBoxInfo
         // 
         this.textBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.textBoxInfo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBoxInfo.Location = new System.Drawing.Point(60, 12);
         this.textBoxInfo.Multiline = true;
         this.textBoxInfo.Name = "textBoxInfo";
         this.textBoxInfo.ReadOnly = true;
         this.textBoxInfo.Size = new System.Drawing.Size(399, 93);
         this.textBoxInfo.TabIndex = 1;
         this.textBoxInfo.WordWrap = false;
         this.textBoxInfo.SizeChanged += new System.EventHandler(this.textBoxInfo_SizeChanged);
         // 
         // flowLayoutPanelButton
         // 
         this.flowLayoutPanelButton.BackColor = System.Drawing.SystemColors.ControlLight;
         this.flowLayoutPanelButton.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.flowLayoutPanelButton.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
         this.flowLayoutPanelButton.Location = new System.Drawing.Point(0, 111);
         this.flowLayoutPanelButton.Name = "flowLayoutPanelButton";
         this.flowLayoutPanelButton.Padding = new System.Windows.Forms.Padding(0, 5, 3, 0);
         this.flowLayoutPanelButton.Size = new System.Drawing.Size(466, 46);
         this.flowLayoutPanelButton.TabIndex = 2;
         // 
         // MyMessageBox
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(466, 157);
         this.Controls.Add(this.flowLayoutPanelButton);
         this.Controls.Add(this.textBoxInfo);
         this.Controls.Add(this.pictureBox1);
         this.KeyPreview = true;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.MinimumSize = new System.Drawing.Size(368, 180);
         this.Name = "MyMessageBox";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "MessageBox";
         this.TopMost = true;
         this.Load += new System.EventHandler(this.MessageBox_Load);
         this.Shown += new System.EventHandler(this.MessageBox_Shown);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MyMessageBox_KeyDown);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.TextBox textBoxInfo;
      private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButton;
   }
}