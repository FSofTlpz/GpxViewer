namespace GpxViewer {
   partial class FormGarminInfo {
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
         this.listBox_Info = new System.Windows.Forms.ListBox();
         this.SuspendLayout();
         // 
         // listBox_Info
         // 
         this.listBox_Info.Dock = System.Windows.Forms.DockStyle.Fill;
         this.listBox_Info.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
         this.listBox_Info.FormattingEnabled = true;
         this.listBox_Info.IntegralHeight = false;
         this.listBox_Info.Location = new System.Drawing.Point(0, 0);
         this.listBox_Info.Name = "listBox_Info";
         this.listBox_Info.Size = new System.Drawing.Size(325, 125);
         this.listBox_Info.TabIndex = 1;
         this.listBox_Info.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox_Info_DrawItem);
         this.listBox_Info.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.listBox_Info_MeasureItem);
         // 
         // FormGarminInfo
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(325, 125);
         this.Controls.Add(this.listBox_Info);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.Name = "FormGarminInfo";
         this.ShowInTaskbar = false;
         this.Text = "Objektinfo für Garminkarte";
         this.Load += new System.EventHandler(this.FormGarminInfo_Load);
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormGarminInfo_KeyDown);
         this.ResumeLayout(false);

      }

        #endregion
        private System.Windows.Forms.ListBox listBox_Info;
    }
}