namespace GpxViewer.PictureEdit {
   partial class FormEditPictureDateTime {
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
         label1 = new Label();
         panel1 = new Panel();
         dateTimePicker1 = new DateTimePicker();
         panel1.SuspendLayout();
         SuspendLayout();
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(4, 32);
         label1.Margin = new Padding(4, 0, 4, 0);
         label1.Name = "label1";
         label1.Size = new Size(212, 15);
         label1.TabIndex = 1;
         label1.Text = "Abbruch mit ESC / Beenden mit ENTER";
         // 
         // panel1
         // 
         panel1.BorderStyle = BorderStyle.Fixed3D;
         panel1.Controls.Add(dateTimePicker1);
         panel1.Controls.Add(label1);
         panel1.Dock = DockStyle.Fill;
         panel1.Location = new Point(0, 0);
         panel1.Margin = new Padding(4, 3, 4, 3);
         panel1.Name = "panel1";
         panel1.Size = new Size(230, 64);
         panel1.TabIndex = 2;
         // 
         // dateTimePicker1
         // 
         dateTimePicker1.CustomFormat = "dd.MM.yyyy HH:mm:ss";
         dateTimePicker1.Format = DateTimePickerFormat.Custom;
         dateTimePicker1.Location = new Point(7, 3);
         dateTimePicker1.Margin = new Padding(4, 3, 4, 3);
         dateTimePicker1.Name = "dateTimePicker1";
         dateTimePicker1.ShowCheckBox = true;
         dateTimePicker1.Size = new Size(204, 23);
         dateTimePicker1.TabIndex = 2;
         // 
         // FormEditPictureDateTime
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(230, 64);
         Controls.Add(panel1);
         FormBorderStyle = FormBorderStyle.None;
         KeyPreview = true;
         Margin = new Padding(4, 3, 4, 3);
         Name = "FormEditPictureDateTime";
         ShowInTaskbar = false;
         StartPosition = FormStartPosition.Manual;
         Text = "FormEditPictureFilename";
         FormClosed += FormEditPictureFilename_FormClosed;
         Shown += FormEditPictureFilename_Shown;
         KeyDown += FormEditPictureFilename_KeyDown;
         panel1.ResumeLayout(false);
         panel1.PerformLayout();
         ResumeLayout(false);
      }

      #endregion
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Panel panel1;
      private System.Windows.Forms.DateTimePicker dateTimePicker1;
   }
}