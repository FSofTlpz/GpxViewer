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
         this.label1 = new System.Windows.Forms.Label();
         this.numericUpDownHours = new System.Windows.Forms.NumericUpDown();
         this.label2 = new System.Windows.Forms.Label();
         this.numericUpDownMinutes = new System.Windows.Forms.NumericUpDown();
         this.richTextBox1 = new System.Windows.Forms.RichTextBox();
         this.buttonGoOn = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHours)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).BeginInit();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 219);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(56, 16);
         this.label1.TabIndex = 0;
         this.label1.Text = "Stunden";
         // 
         // numericUpDownHours
         // 
         this.numericUpDownHours.Location = new System.Drawing.Point(96, 217);
         this.numericUpDownHours.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
         this.numericUpDownHours.Minimum = new decimal(new int[] {
            23,
            0,
            0,
            -2147483648});
         this.numericUpDownHours.Name = "numericUpDownHours";
         this.numericUpDownHours.Size = new System.Drawing.Size(77, 22);
         this.numericUpDownHours.TabIndex = 1;
         this.numericUpDownHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(266, 217);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(53, 16);
         this.label2.TabIndex = 2;
         this.label2.Text = "Minuten";
         // 
         // numericUpDownMinutes
         // 
         this.numericUpDownMinutes.Location = new System.Drawing.Point(350, 217);
         this.numericUpDownMinutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
         this.numericUpDownMinutes.Name = "numericUpDownMinutes";
         this.numericUpDownMinutes.Size = new System.Drawing.Size(77, 22);
         this.numericUpDownMinutes.TabIndex = 3;
         this.numericUpDownMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
         // 
         // richTextBox1
         // 
         this.richTextBox1.Location = new System.Drawing.Point(12, 12);
         this.richTextBox1.Name = "richTextBox1";
         this.richTextBox1.ReadOnly = true;
         this.richTextBox1.Size = new System.Drawing.Size(415, 168);
         this.richTextBox1.TabIndex = 4;
         this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
         // 
         // buttonGoOn
         // 
         this.buttonGoOn.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonGoOn.Location = new System.Drawing.Point(180, 266);
         this.buttonGoOn.Name = "buttonGoOn";
         this.buttonGoOn.Size = new System.Drawing.Size(98, 27);
         this.buttonGoOn.TabIndex = 5;
         this.buttonGoOn.Text = "weiter";
         this.buttonGoOn.UseVisualStyleBackColor = true;
         this.buttonGoOn.Click += new System.EventHandler(this.button1_Click);
         // 
         // FormOffsetPictureGpx
         // 
         this.AcceptButton = this.buttonGoOn;
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(441, 315);
         this.ControlBox = false;
         this.Controls.Add(this.buttonGoOn);
         this.Controls.Add(this.richTextBox1);
         this.Controls.Add(this.numericUpDownMinutes);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.numericUpDownHours);
         this.Controls.Add(this.label1);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormOffsetPictureGpx";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "Zeitdifferenz";
         this.Load += new System.EventHandler(this.FormOffsetPictureGpx_Load);
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHours)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

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