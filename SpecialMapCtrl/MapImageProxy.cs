using System;
using System.Diagnostics;
using System.IO;
using GMap.NET;
using GMap.NET.FSofTExtented;


#if !GMAP4SKIA
using System.Drawing;
using System.Drawing.Imaging;
#else
using SkiaSharp;
#endif

namespace SpecialMapCtrl {

   /// <summary>
   ///     image abstraction proxy
   /// </summary>
   public class MapImageProxy : PureImageProxy {

      public static PureImageProxy TileImageProxy {
         get => PublicCore.TileImageProxy;
         set => PublicCore.TileImageProxy = value;
      }

      MapImageProxy() { }

      public static void Enable() => TileImageProxy = Instance;


      public static readonly MapImageProxy Instance = new MapImageProxy();

#if !GMAP4SKIA
      internal ColorMatrix? ColorMatrix;
#endif

      static readonly bool Win7OrLater = PublicCore.IsRunningOnWin7OrLater;

      public override PureImage? FromStream(Stream stream) {
         try {

#if !GMAP4SKIA
            var m = Image.FromStream(stream, true, !Win7OrLater);
            if (m != null)
               return new MapImage {
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

            return new MapImage {
               Img = bm
            };
#endif

         } catch (Exception ex) {
            Debug.WriteLine("FromStream: " + ex);
         }
         return null;
      }

      public override bool Save(Stream stream, PureImage image) {
         MapImage? ret = image as MapImage;
         bool ok = true;

         if (ret != null && ret.Img != null) {
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