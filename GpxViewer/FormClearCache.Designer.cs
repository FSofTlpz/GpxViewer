
namespace GpxViewer {
   partial class FormClearCache {
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
         this.button_actMap = new System.Windows.Forms.Button();
         this.button_allMaps = new System.Windows.Forms.Button();
         this.button_back = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // button_actMap
         // 
         this.button_actMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.button_actMap.Image = global::GpxViewer.Properties.Resources.delete;
         this.button_actMap.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_actMap.Location = new System.Drawing.Point(12, 12);
         this.button_actMap.Name = "button_actMap";
         this.button_actMap.Size = new System.Drawing.Size(246, 30);
         this.button_actMap.TabIndex = 0;
         this.button_actMap.Text = "nur Daten der aktuelle Karte löschen";
         this.button_actMap.UseVisualStyleBackColor = true;
         this.button_actMap.Click += new System.EventHandler(this.button_actMap_Click);
         // 
         // button_allMaps
         // 
         this.button_allMaps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.button_allMaps.Image = global::GpxViewer.Properties.Resources.delete;
         this.button_allMaps.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_allMaps.Location = new System.Drawing.Point(12, 62);
         this.button_allMaps.Name = "button_allMaps";
         this.button_allMaps.Size = new System.Drawing.Size(246, 30);
         this.button_allMaps.TabIndex = 1;
         this.button_allMaps.Text = "Alle Kartendaten löschen!!!";
         this.button_allMaps.UseVisualStyleBackColor = true;
         this.button_allMaps.Click += new System.EventHandler(this.button_allMaps_Click);
         // 
         // button_back
         // 
         this.button_back.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.button_back.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button_back.Image = global::GpxViewer.Properties.Resources.ok;
         this.button_back.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.button_back.Location = new System.Drawing.Point(80, 129);
         this.button_back.Name = "button_back";
         this.button_back.Size = new System.Drawing.Size(123, 30);
         this.button_back.TabIndex = 2;
         this.button_back.Text = "zurück";
         this.button_back.UseVisualStyleBackColor = true;
         // 
         // FormClearCache
         // 
         this.AcceptButton = this.button_back;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.button_back;
         this.ClientSize = new System.Drawing.Size(273, 171);
         this.ControlBox = false;
         this.Controls.Add(this.button_back);
         this.Controls.Add(this.button_allMaps);
         this.Controls.Add(this.button_actMap);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "FormClearCache";
         this.Text = "interne Speicher für Kartenteile leeren";
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Button button_actMap;
      private System.Windows.Forms.Button button_allMaps;
      private System.Windows.Forms.Button button_back;
   }
}