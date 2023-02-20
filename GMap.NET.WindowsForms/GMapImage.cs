using System;
using System.Diagnostics;
using System.IO;

#if !GMAP4SKIA
using System.Drawing;
using System.Drawing.Imaging;

namespace GMap.NET.WindowsForms {
#else
using SkiaSharp;

namespace GMap.NET.Skia {
#endif

   /// <summary>
   /// image abstraction
   /// </summary>
   public class GMapImage : PureImage {
#if !GMAP4SKIA
      public Image Img;
#else
      public SKBitmap Img;
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

   /// <summary>
   ///     image abstraction proxy
   /// </summary>
   public class GMapImageProxy : PureImageProxy {

      public static PureImageProxy TileImageProxy {
         get => PublicCore.TileImageProxy;
         set => PublicCore.TileImageProxy = value;
      }

      GMapImageProxy() { }

      public static void Enable() {
         TileImageProxy = Instance;
      }

      public static readonly GMapImageProxy Instance = new GMapImageProxy();

#if !GMAP4SKIA
      internal ColorMatrix ColorMatrix;
#endif

      static readonly bool Win7OrLater = PublicCore.IsRunningOnWin7OrLater;

      public override PureImage FromStream(Stream stream) {
         try {

#if !GMAP4SKIA
            var m = Image.FromStream(stream, true, !Win7OrLater);
            if (m != null)
               return new GMapImage {
                  Img = ColorMatrix != null ?
                                          ApplyColorMatrix(m, ColorMatrix) :
                                          m
               };
#else
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            SKBitmap bm = SKBitmap.Decode(memoryStream);
            memoryStream.Dispose();

            return new GMapImage {
               Img = bm
            };
#endif

         } catch (Exception ex) {
            Debug.WriteLine("FromStream: " + ex);
         }
         return null;
      }

      public override bool Save(Stream stream, PureImage image) {
         GMapImage ret = image as GMapImage;
         bool ok = true;

         if (ret.Img != null) {
            // try png
            try {
#if !GMAP4SKIA
               ret.Img.Save(stream, ImageFormat.Png);
#else
               ret.Img.Encode(stream, SKEncodedImageFormat.Png, 100);
#endif
            } catch {
               // try jpeg
               try {
                  stream.Seek(0, SeekOrigin.Begin);
#if !GMAP4SKIA
                  ret.Img.Save(stream, ImageFormat.Jpeg);
#else
                  ret.Img.Encode(stream, SKEncodedImageFormat.Jpeg, 90);
#endif
               } catch {
                  ok = false;
               }
            }
         } else {
            ok = false;
         }

         return ok;
      }

#if !GMAP4SKIA
      Bitmap ApplyColorMatrix(Image original, ColorMatrix matrix) {
         // create a blank bitmap the same size as original
         var newBitmap = new Bitmap(original.Width, original.Height);

         using (original) { // destroy original
                            // get a graphics object from the new image
            using (var g = Graphics.FromImage(newBitmap)) {
               // set the color matrix attribute
               using (var attributes = new ImageAttributes()) {
                  attributes.SetColorMatrix(matrix);
                  g.DrawImage(original,
                              new Rectangle(0, 0, original.Width, original.Height),
                              0,
                              0,
                              original.Width,
                              original.Height,
                              GraphicsUnit.Pixel,
                              attributes);
               }
            }
         }

         return newBitmap;
      }
#endif
   }
}