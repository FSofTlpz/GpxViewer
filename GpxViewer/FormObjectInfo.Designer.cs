namespace GpxViewer {
   partial class FormObjectInfo {
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
         components = new System.ComponentModel.Container();
         listBox_Info = new ListBox();
         contextMenuStrip1 = new ContextMenuStrip(components);
         ToolStripMenuItem_Copy = new ToolStripMenuItem();
         ToolStripMenuItem_CopyAll = new ToolStripMenuItem();
         contextMenuStrip1.SuspendLayout();
         SuspendLayout();
         // 
         // listBox_Info
         // 
         listBox_Info.ContextMenuStrip = contextMenuStrip1;
         listBox_Info.Dock = DockStyle.Fill;
         listBox_Info.FormattingEnabled = true;
         listBox_Info.HorizontalScrollbar = true;
         listBox_Info.Location = new Point(0, 0);
         listBox_Info.Margin = new Padding(4);
         listBox_Info.Name = "listBox_Info";
         listBox_Info.Size = new Size(424, 133);
         listBox_Info.TabIndex = 0;
         // 
         // contextMenuStrip1
         // 
         contextMenuStrip1.ImageScalingSize = new Size(20, 20);
         contextMenuStrip1.Items.AddRange(new ToolStripItem[] { ToolStripMenuItem_Copy, ToolStripMenuItem_CopyAll });
         contextMenuStrip1.Name = "contextMenuStrip1";
         contextMenuStrip1.Size = new Size(296, 78);
         contextMenuStrip1.Opening += contextMenuStrip1_Opening;
         // 
         // ToolStripMenuItem_Copy
         // 
         ToolStripMenuItem_Copy.Image = Properties.Resources.copy;
         ToolStripMenuItem_Copy.Name = "ToolStripMenuItem_Copy";
         ToolStripMenuItem_Copy.Size = new Size(295, 26);
         ToolStripMenuItem_Copy.Text = "Text in die Zwischenablage kopieren";
         ToolStripMenuItem_Copy.Click += ToolStripMenuItem_Copy_Click;
         // 
         // ToolStripMenuItem_CopyAll
         // 
         ToolStripMenuItem_CopyAll.Name = "ToolStripMenuItem_CopyAll";
         ToolStripMenuItem_CopyAll.Size = new Size(295, 26);
         ToolStripMenuItem_CopyAll.Text = "alle Texte in die Zwischenablage kopieren";
         ToolStripMenuItem_CopyAll.Click += ToolStripMenuItem_CopyAll_Click;
         // 
         // FormObjectInfo
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(424, 133);
         Controls.Add(listBox_Info);
         FormBorderStyle = FormBorderStyle.SizableToolWindow;
         KeyPreview = true;
         Margin = new Padding(4);
         Name = "FormObjectInfo";
         ShowInTaskbar = false;
         Text = "Objektinfo";
         contextMenuStrip1.ResumeLayout(false);
         ResumeLayout(false);
      }

      #endregion

      private System.Windows.Forms.ListBox listBox_Info;
      private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Copy;
      private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_CopyAll;
   }
}