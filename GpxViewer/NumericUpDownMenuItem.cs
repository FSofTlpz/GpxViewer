using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

// https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/how-to-wrap-a-windows-forms-control-with-toolstripcontrolhost?redirectedfrom=MSDN

namespace GpxViewer {

   [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
   public class NumericUpDownMenuItem : ToolStripControlHost {

      /// <summary>
      /// direkter Zugriff auf das <see cref="NumericUpDown"/>
      /// </summary>
      [Browsable(true)]
      public NumericUpDown NumUpDown { get; protected set; }

      /// <summary>
      /// akt. Wert des <see cref="NumericUpDown"/>
      /// </summary>
      [Browsable(true)]
      public decimal Value {
         get {
            return NumUpDown.Value;
         }
         set {
            NumUpDown.Value = value;
         }
      }

      /// <summary>
      /// Veränderung des <see cref="NumericUpDown"/>
      /// </summary>
      [Browsable(true)]
      public decimal Increment {
         get {
            return NumUpDown.Increment;
         }
         set {
            NumUpDown.Increment = value;
         }
      }

      /// <summary>
      /// Minimum des <see cref="NumericUpDown"/>
      /// </summary>
      [Browsable(true)]
      public decimal Minimum {
         get {
            return NumUpDown.Minimum;
         }
         set {
            NumUpDown.Minimum = value;
         }
      }

      /// <summary>
      /// Maximum des <see cref="NumericUpDown"/>
      /// </summary>
      [Browsable(true)]
      public decimal Maximum {
         get {
            return NumUpDown.Maximum;
         }
         set {
            NumUpDown.Maximum = value;
         }
      }


      private readonly FlowLayoutPanel flowLayoutPanel1;

      private readonly Label label1;





      public NumericUpDownMenuItem() : base(new FlowLayoutPanel()) {

         //AutoSize = true;
         //Available = true;
         //Padding = new Padding(5);

         label1 = new Label {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Location = new System.Drawing.Point(3, 6),
            Name = "label1",
            Size = new System.Drawing.Size(15, 13),
            TabIndex = 0,
            Text = "la"
         };

         NumUpDown = new NumericUpDown {
            DecimalPlaces = 1,
            Location = new System.Drawing.Point(24, 3),
            Name = "numericUpDown2",
            Size = new System.Drawing.Size(56, 20),
            TabIndex = 1,
            TextAlign = HorizontalAlignment.Right,
            Margin = new Padding(5),
         };

         flowLayoutPanel1 = Control as FlowLayoutPanel;
         flowLayoutPanel1.AutoSize = true;
         flowLayoutPanel1.Controls.Add(label1);
         flowLayoutPanel1.Controls.Add(NumUpDown);
         flowLayoutPanel1.Location = new System.Drawing.Point(26, 82);
         flowLayoutPanel1.Margin = new Padding(5);
         flowLayoutPanel1.Name = "flowLayoutPanel1";
         flowLayoutPanel1.Size = new System.Drawing.Size(85, 26);
         flowLayoutPanel1.BackColor = base.BackColor; // System.Drawing.SystemColors.Control;
         flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
         flowLayoutPanel1.WrapContents = false;

         TextChanged += NumericUpDownMenuItem_TextChanged;
         //NumUpDown.ValueChanged += NumUpDown_ValueChanged;



      }

      //private void NumUpDown_ValueChanged(object sender, EventArgs e) {

      //}

      private void NumericUpDownMenuItem_TextChanged(object sender, EventArgs e) {
         label1.Text = Text;
      }

   }
}
