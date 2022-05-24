
namespace GpxViewer {
   partial class MoveControl {
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
         this.SuspendLayout();
         // 
         // MoveControl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Name = "MoveControl";
         this.Load += new System.EventHandler(this.MoveControl_Load);
         this.BackColorChanged += new System.EventHandler(this.MoveControl_BackColorChanged);
         this.Paint += new System.Windows.Forms.PaintEventHandler(this.MoveControl_Paint);
         this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MoveControl_MouseClick);
         this.ResumeLayout(false);

      }

      #endregion
   }
}
