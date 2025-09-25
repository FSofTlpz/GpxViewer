namespace GpxViewer {
   partial class FormOffsetPictureGpx {
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOffsetPictureGpx));
         label1 = new Label();
         numericUpDownHours = new NumericUpDown();
         label2 = new Label();
         numericUpDownMinutes = new NumericUpDown();
         richTextBox1 = new RichTextBox();
         buttonGoOn = new Button();
         ((System.ComponentModel.ISupportInitialize)numericUpDownHours).BeginInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownMinutes).BeginInit();
         SuspendLayout();
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(10, 205);
         label1.Name = "label1";
         label1.Size = new Size(51, 15);
         label1.TabIndex = 0;
         label1.Text = "Stunden";
         // 
         // numericUpDownHours
         // 
         numericUpDownHours.Location = new Point(84, 203);
         numericUpDownHours.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
         numericUpDownHours.Minimum = new decimal(new int[] { 23, 0, 0, int.MinValue });
         numericUpDownHours.Name = "numericUpDownHours";
         numericUpDownHours.Size = new Size(67, 23);
         numericUpDownHours.TabIndex = 1;
         numericUpDownHours.TextAlign = HorizontalAlignment.Right;
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point(233, 203);
         label2.Name = "label2";
         label2.Size = new Size(52, 15);
         label2.TabIndex = 2;
         label2.Text = "Minuten";
         // 
         // numericUpDownMinutes
         // 
         numericUpDownMinutes.Location = new Point(306, 203);
         numericUpDownMinutes.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
         numericUpDownMinutes.Name = "numericUpDownMinutes";
         numericUpDownMinutes.Size = new Size(67, 23);
         numericUpDownMinutes.TabIndex = 3;
         numericUpDownMinutes.TextAlign = HorizontalAlignment.Right;
         // 
         // richTextBox1
         // 
         richTextBox1.Location = new Point(10, 11);
         richTextBox1.Name = "richTextBox1";
         richTextBox1.ReadOnly = true;
         richTextBox1.Size = new Size(364, 158);
         richTextBox1.TabIndex = 4;
         richTextBox1.Text = resources.GetString("richTextBox1.Text");
         // 
         // buttonGoOn
         // 
         buttonGoOn.DialogResult = DialogResult.OK;
         buttonGoOn.Location = new Point(158, 249);
         buttonGoOn.Name = "buttonGoOn";
         buttonGoOn.Size = new Size(86, 25);
         buttonGoOn.TabIndex = 5;
         buttonGoOn.Text = "weiter";
         buttonGoOn.UseVisualStyleBackColor = true;
         buttonGoOn.Click += button1_Click;
         // 
         // FormOffsetPictureGpx
         // 
         AcceptButton = buttonGoOn;
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(386, 290);
         ControlBox = false;
         Controls.Add(buttonGoOn);
         Controls.Add(richTextBox1);
         Controls.Add(numericUpDownMinutes);
         Controls.Add(label2);
         Controls.Add(numericUpDownHours);
         Controls.Add(label1);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormOffsetPictureGpx";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Zeitdifferenz";
         Load += FormOffsetPictureGpx_Load;
         ((System.ComponentModel.ISupportInitialize)numericUpDownHours).EndInit();
         ((System.ComponentModel.ISupportInitialize)numericUpDownMinutes).EndInit();
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.NumericUpDown numericUpDownHours;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.NumericUpDown numericUpDownMinutes;
      private System.Windows.Forms.RichTextBox richTextBox1;
      private System.Windows.Forms.Button buttonGoOn;
   }
}