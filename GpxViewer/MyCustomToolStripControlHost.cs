using System.Windows.Forms;

namespace GpxViewer {
   public class MyCustomToolStripControlHost : ToolStripControlHost {
      public MyCustomToolStripControlHost()
          : base(new Control()) {
      }
      public MyCustomToolStripControlHost(Control c)
          : base(c) {
      }
   }
}
