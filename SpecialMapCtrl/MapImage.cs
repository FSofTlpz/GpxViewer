using System;
using GMap.NET;
using GMap.NET.FSofTExtented;


#if !GMAP4SKIA
using System.Drawing;
#else
using SkiaSharp;
#endif

namespace SpecialMapCtrl {

   /// <summary>
   /// image abstraction
   /// </summary>
   public class MapImage : PureImage {
#if !GMAP4SKIA
      public Image? Img;
#else
      public SKBitmap? Img;
#endif

      public bool IsParent => PublicCore.GetImageIsParent(this);

      public Int64 Xoff => PublicCore.GetImageXoff(this);

      public Int64 Yoff => PublicCore.GetImageYoff(this);

      public Int64 Ix => PublicCore.GetImageIx(this);


      public override void Dispose() {
         if (Img != null) {
            Img.Dispose();
            Img = null;
         }

         if (Data != null) {
            Data.Dispose();
            Data = null;
         }
      }
   }

}