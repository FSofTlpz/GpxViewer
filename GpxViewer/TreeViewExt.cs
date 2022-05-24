using System.Drawing;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class TreeViewExt : TreeView {
      public TreeViewExt() { }

      /// <summary>
      /// Workaround um einen Doppelklick auf die Checkbox zu verhindern (wird im Originalcontrol nicht korrekt behandelt!)
      /// </summary>
      /// <param name="m"></param>
      protected override void WndProc(ref Message m) {
         if (m.Msg == 0x0203 && 
             CheckBoxes) {
            Point localPos = PointToClient(Cursor.Position);
            TreeViewHitTestInfo hitTestInfo = HitTest(localPos);
            if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage) {
               m.Msg = 0x0201;
            }
         }
         base.WndProc(ref m);
      }

   }
}
