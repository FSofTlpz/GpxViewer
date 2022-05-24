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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapControl));
         this.smc = new SmallMapControl.SmallMapCtrl();
         this.trackBarZoom = new System.Windows.Forms.TrackBar();
         this.moveControl1 = new GpxViewer.MoveControl();
         ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
         this.SuspendLayout();
         // 
         // smc
         // 
         this.smc.Bearing = 0F;
         this.smc.CanDragMap = true;
         this.smc.Dock = System.Windows.Forms.DockStyle.Left;
         this.smc.EmptyTileColor = System.Drawing.Color.Navy;
         this.smc.GrayScaleMode = false;
         this.smc.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
         this.smc.LevelsKeepInMemory = 5;
         this.smc.Location = new System.Drawing.Point(0, 0);
         this.smc.MapRenderZoom2RealDevice = 1F;
         this.smc.MarkersEnabled = true;
         this.smc.MaxZoom = 24;
         this.smc.SMC_Cursor = System.Windows.Forms.Cursors.Default;
         this.smc.SMC_DragButton = System.Windows.Forms.MouseButtons.Right;
         this.smc.MinZoom = 2;
         this.smc.MouseWheelZoomEnabled = true;
         this.smc.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         this.smc.Name = "smc";
         this.smc.NegativeMode = false;
         this.smc.PolygonsEnabled = true;
         this.smc.RetryLoadTile = 0;
         this.smc.RoutesEnabled = true;
         this.smc.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional;
         this.smc.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
         this.smc.ShowTileGridLines = false;
         this.smc.Size = new System.Drawing.Size(189, 213);
         this.smc.SMC_EmptyMapBackground = System.Drawing.Color.LightGray;
         this.smc.SMC_EmptyTileColor = System.Drawing.Color.Navy;
         this.smc.SMC_EmptyTileText = "keine Daten";
         this.smc.SMC_MapRenderZoom2RealDevice = 1F;
         this.smc.SMC_MaxZoom = 24;
         this.smc.SMC_MinZoom = 2;
         this.smc.SMC_Position = ((GMap.NET.PointLatLng)(resources.GetObject("smc.SMC_Position")));
         this.smc.SMC_ShowCenter = false;
         this.smc.SMC_ShowTileGridLines = false;
         this.smc.Width = 189;
         this.smc.SMC_Zoom = 2D;
         this.smc.TabIndex = 0;
         this.smc.Zoom = 2D;
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
         this.moveControl1.Location = new System.Drawing.Point(203, 168);
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
         this.Size = new System.Drawing.Size(248, 213);
         this.Load += new System.EventHandler(this.MapControl_Load);
         this.SizeChanged += new System.EventHandler(this.MapControl_SizeChanged);
         ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private SmallMapControl.SmallMapCtrl smc;
      private System.Windows.Forms.TrackBar trackBarZoom;
      private MoveControl moveControl1;
   }
}
