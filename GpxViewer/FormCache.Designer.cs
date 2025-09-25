namespace GpxViewer {
   partial class FormCache {
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
         listBox1 = new ListBox();
         labelSum = new Label();
         labelActualMap = new Label();
         label1 = new Label();
         label2 = new Label();
         buttonClearActual = new Button();
         buttonClearAll = new Button();
         label3 = new Label();
         labelCachelocation = new Label();
         buttonRefresh = new Button();
         SuspendLayout();
         // 
         // listBox1
         // 
         listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
         listBox1.FormattingEnabled = true;
         listBox1.HorizontalScrollbar = true;
         listBox1.IntegralHeight = false;
         listBox1.Location = new Point(12, 176);
         listBox1.Name = "listBox1";
         listBox1.Size = new Size(704, 315);
         listBox1.TabIndex = 0;
         listBox1.MouseDoubleClick += listBox1_MouseDoubleClick;
         // 
         // labelSum
         // 
         labelSum.AutoSize = true;
         labelSum.Location = new Point(303, 115);
         labelSum.Name = "labelSum";
         labelSum.Size = new Size(56, 15);
         labelSum.TabIndex = 2;
         labelSum.Text = "labelSum";
         // 
         // labelActualMap
         // 
         labelActualMap.AutoSize = true;
         labelActualMap.Location = new Point(303, 84);
         labelActualMap.Name = "labelActualMap";
         labelActualMap.Size = new Size(38, 15);
         labelActualMap.TabIndex = 3;
         labelActualMap.Text = "label1";
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(186, 84);
         label1.Name = "label1";
         label1.Size = new Size(81, 15);
         label1.TabIndex = 4;
         label1.Text = "aktuelle Karte:";
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point(186, 115);
         label2.Name = "label2";
         label2.Size = new Size(100, 15);
         label2.TabIndex = 5;
         label2.Text = "Cache insgesamt:";
         // 
         // buttonClearActual
         // 
         buttonClearActual.BackColor = Color.MistyRose;
         buttonClearActual.Location = new Point(12, 80);
         buttonClearActual.Name = "buttonClearActual";
         buttonClearActual.Size = new Size(158, 25);
         buttonClearActual.TabIndex = 6;
         buttonClearActual.Text = "Cache für Karte löschen";
         buttonClearActual.UseVisualStyleBackColor = false;
         buttonClearActual.Click += buttonClearActual_Click;
         // 
         // buttonClearAll
         // 
         buttonClearAll.BackColor = Color.OrangeRed;
         buttonClearAll.Location = new Point(12, 111);
         buttonClearAll.Name = "buttonClearAll";
         buttonClearAll.Size = new Size(158, 25);
         buttonClearAll.TabIndex = 7;
         buttonClearAll.Text = "gesamten Cache löschen";
         buttonClearAll.UseVisualStyleBackColor = false;
         buttonClearAll.Click += buttonClearAll_Click;
         // 
         // label3
         // 
         label3.AutoSize = true;
         label3.Location = new Point(12, 158);
         label3.Name = "label3";
         label3.Size = new Size(91, 15);
         label3.TabIndex = 8;
         label3.Text = "aktueller Cache:";
         // 
         // labelCachelocation
         // 
         labelCachelocation.AutoSize = true;
         labelCachelocation.Location = new Point(12, 50);
         labelCachelocation.Name = "labelCachelocation";
         labelCachelocation.Size = new Size(38, 15);
         labelCachelocation.TabIndex = 11;
         labelCachelocation.Text = "label5";
         // 
         // buttonRefresh
         // 
         buttonRefresh.Image = Properties.Resources.arrow_refresh;
         buttonRefresh.ImageAlign = ContentAlignment.MiddleLeft;
         buttonRefresh.Location = new Point(12, 12);
         buttonRefresh.Name = "buttonRefresh";
         buttonRefresh.Size = new Size(143, 25);
         buttonRefresh.TabIndex = 12;
         buttonRefresh.Text = "Daten aktualisieren";
         buttonRefresh.TextAlign = ContentAlignment.MiddleRight;
         buttonRefresh.UseVisualStyleBackColor = true;
         buttonRefresh.Click += buttonRefresh_Click;
         // 
         // FormCache
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(731, 503);
         Controls.Add(listBox1);
         Controls.Add(label3);
         Controls.Add(buttonRefresh);
         Controls.Add(labelCachelocation);
         Controls.Add(buttonClearAll);
         Controls.Add(buttonClearActual);
         Controls.Add(label2);
         Controls.Add(label1);
         Controls.Add(labelActualMap);
         Controls.Add(labelSum);
         MaximizeBox = false;
         MinimizeBox = false;
         MinimumSize = new Size(747, 542);
         Name = "FormCache";
         ShowIcon = false;
         ShowInTaskbar = false;
         Text = "Cacheinfo und löschen";
         Load += FormCache_Load;
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private ListBox listBox1;
      private Label labelSum;
      private Label labelActualMap;
      private Label label1;
      private Label label2;
      private Button buttonClearActual;
      private Button buttonClearAll;
      private Label label3;
      private Label labelCachelocation;
      private Button buttonRefresh;
   }
}