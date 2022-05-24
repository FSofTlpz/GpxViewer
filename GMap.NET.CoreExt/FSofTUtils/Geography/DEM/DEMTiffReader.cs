using System;
using BitMiracle.LibTiff.Classic;

// https://bitmiracle.com/libtiff/

/*
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

    <Reference Include="PresentationCore" />
    <Reference Include="WindowsBase" />
 */

namespace FSofTUtils.Geography.DEM {
   class DEMTiffReader : DEM1x1 {

      /// <summary>
      /// "nodata" value in tif-files
      /// </summary>
      const short TIF_NOVALUE = -32768;

      string tiffile;


      public DEMTiffReader(int left, int bottom, string tiffile) :
         base(left, bottom) {
         this.tiffile = tiffile;
      }

      public override void SetDataArray() {
         /*
         Stream stream = File.Open(tiffile, FileMode.Open, FileAccess.Read, FileShare.Read);
         TiffBitmapDecoder tiffDecoder = new TiffBitmapDecoder(
                                                 stream,
                                                 BitmapCreateOptions.PreservePixelFormat,
                                                 BitmapCacheOption.Default); //.None);
         BitmapFrame firstFrame = tiffDecoder.Frames[0];
         FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(firstFrame, PixelFormats.Gray16, null, 0);
         //FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(firstFrame, firstFrame.Format, null, 0);

         Columns = convertedBitmap.PixelWidth;
         Rows = convertedBitmap.PixelHeight;

         data = new short[Columns * Rows];
         convertedBitmap.CopyPixels(data, 
                                    Columns * 2,   // Zeilenlänge in Byte
                                    0);
         stream.Dispose();
         */

         using (Tiff tiff = Tiff.Open(tiffile, "r")) {
            //int frames = tiff.GetField(TiffTag.FRAMECOUNT)[0].ToInt();
            //int bits = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            //double dpiX = tiff.GetField(TiffTag.XRESOLUTION)[0].ToDouble();
            //double dpiY = tiff.GetField(TiffTag.YRESOLUTION)[0].ToDouble();
            Columns = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            Rows = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            data = new short[Columns * Rows];

            byte[] scanline = new byte[tiff.ScanlineSize()];
            short[] temp = new short[Columns];
            for (int r = 0; r < Rows; r++) {
               tiff.ReadScanline(scanline, r);
               // pack all bytes to ushorts
               Buffer.BlockCopy(scanline, 0, temp, 0, scanline.Length);
               Array.Copy(temp, 0, data, r * Columns, temp.Length);
            }

            //// Read the image into the memory buffer
            //int[] raster = new int[Columns * Rows];
            //tiff.ReadRGBAImage(Columns, Rows, raster);
            //for (int r = 0; r < Rows; r++) {
            //   for (int c = 0; c < Columns; c++) {
            //      int offset = (Rows - r - 1) * Columns + c;
            //      Color col = Color.FromArgb(Tiff.GetR(raster[offset]),
            //                                 Tiff.GetG(raster[offset]),
            //                                 Tiff.GetB(raster[offset]));

            //   }
            //}
         }

         Maximum = short.MinValue;
         Minimum = short.MaxValue;
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            if (data[i] != TIF_NOVALUE) {
               if (Maximum < data[i])
                  Maximum = data[i];
               if (Minimum > data[i])
                  Minimum = data[i];
            } else {
               NotValid++;
               data[i] = DEMNOVALUE;
            }
         }
         if (NotValid == data.Length) {
            Maximum =
            Minimum = DEMNOVALUE;
         }

      }

   }
}
