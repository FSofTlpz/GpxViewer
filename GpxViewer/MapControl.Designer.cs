namespace GpxViewer {
   partial class MapControl {
      /// <summary> 
      /// Erforderliche Designervariable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary> 
      /// Verwendete Ressourcen bereinigen.
      /// </summary>
      /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Vom Komponenten-Designer generierter Code

      /// <summary> 
      /// Erforderliche Methode für die Designerunterstützung. 
      /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
      /// </summary>
      private void InitializeComponent() {
         this.smc = new SpecialMapCtrl.SpecialMapCtrl();
         this.trackBarZoom = new System.Windows.Forms.TrackBar();
         this.moveControl1 = new GpxViewer.MoveControl();
         ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
         this.SuspendLayout();
         // 
         // smc
         // 
         this.smc.Dock = System.Windows.Forms.DockStyle.Left;
         this.smc.Location = new System.Drawing.Point(0, 0);
         this.smc.Map_CanDragMap = true;
         this.smc.Map_ClickTolerance4Tracks = 1F;
         this.smc.Map_CopyrightFont = new System.Drawing.Font("Microsoft Sans Serif", 7F);
         this.smc.Map_EmptyMapBackgroundColor = System.Drawing.Color.LightGray;
         this.smc.Map_EmptyTileColor = System.Drawing.Color.DarkGray;
         this.smc.Map_EmptyTileText = "keine Daten";
         this.smc.Map_FillEmptyTiles = false;
         this.smc.Map_IsDragging = false;
         this.smc.Map_LevelsKeepInMemory = 5;
         this.smc.Map_MarkersEnabled = true;
         this.smc.Map_MaxZoom = 24;
         this.smc.Map_MinZoom = 0;
         this.smc.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         this.smc.Map_PolygonsEnabled = true;
         this.smc.Map_DeviceZoom = 1F;
         this.smc.Map_RetryLoadTile = 0;
         this.smc.Map_ScaleMode = GMap.NET.WindowsForms.GMapControl.ScaleModes.Fractional;
         this.smc.Map_SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
         this.smc.Map_ShowTileGridLines = true;
         this.smc.Map_TracksEnabled = true;
         this.smc.Map_Zoom = 20D;
         this.smc.Map_ZoomLinear = 1048576D;
         this.smc.Name = "smc";
         this.smc.Size = new System.Drawing.Size(189, 218);
         this.smc.SpecMapCursor = System.Windows.Forms.Cursors.Default;
         this.smc.SpecMapMaxZoom = 24;
         this.smc.SpecMapMinZoom = 0;
         this.smc.SpecMapShowCenter = false;
         this.smc.SpecMapZoom = 20D;
         this.smc.TabIndex = 0;
         this.smc.Load += new System.EventHandler(this.Smc_Load);
         // 
         // trackBarZoom
         // 
         this.trackBarZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.trackBarZoom.LargeChange = 10;
         this.trackBarZoom.Location = new System.Drawing.Point(203, 0);
         this.trackBarZoom.Maximum = 240;
         this.trackBarZoom.Minimum = 120;
         this.trackBarZoom.Name = "trackBarZoom";
         this.trackBarZoom.Orientation = System.Windows.Forms.Orientation.Vertical;
         this.trackBarZoom.Size = new System.Drawing.Size(45, 162);
         this.trackBarZoom.TabIndex = 1;
         this.trackBarZoom.TickFrequency = 10;
         this.trackBarZoom.TickStyle = System.Windows.Forms.TickStyle.Both;
         this.trackBarZoom.Value = 120;
         this.trackBarZoom.ValueChanged += new System.EventHandler(this.trackBarZoom_ValueChanged);
         // 
         // moveControl1
         // 
         this.moveControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.moveControl1.Location = new System.Drawing.Point(203, 173);
         this.moveControl1.Name = "moveControl1";
         this.moveControl1.Size = new System.Drawing.Size(45, 45);
         this.moveControl1.TabIndex = 2;
         // 
         // MapControl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.moveControl1);
         this.Controls.Add(this.trackBarZoom);
         this.Controls.Add(this.smc);
         this.Name = "MapControl";
         this.Size = new System.Drawing.Size(248, 218);
         this.Load += new System.EventHandler(this.MapControl_Load);
         this.SizeChanged += new System.EventHandler(this.MapControl_SizeChanged);
         ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private SpecialMapCtrl.SpecialMapCtrl smc;
      private System.Windows.Forms.TrackBar trackBarZoom;
      private MoveControl moveControl1;
   }
}
