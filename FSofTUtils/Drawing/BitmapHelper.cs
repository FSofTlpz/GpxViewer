#if GMAP4SKIA
using SkiaSharp;
using System;
using System.Drawing;
#else
using System;
using System.Drawing;
using System.IO;
#endif


namespace FSofTUtils.Drawing {
   public class BitmapHelper {

      /*
https://en.wikipedia.org/wiki/BMP_file_format#Example_1
      

https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmap

typedef struct tagBITMAP {
  LONG   bmType;
  LONG   bmWidth;
  LONG   bmHeight;
  LONG   bmWidthBytes;
  WORD   bmPlanes;
  WORD   bmBitsPixel;
  LPVOID bmBits;
} BITMAP, *PBITMAP, *NPBITMAP, *LPBITMAP;
      

typedef struct tagBITMAPFILEHEADER {
  WORD  bfType;                        // 42h, 4Dh ("BM")
  DWORD bfSize;                        // The size, in bytes, of the bitmap file.
  WORD  bfReserved1;                   // Reserved; must be zero.
  WORD  bfReserved2;                   // Reserved; must be zero.
  DWORD bfOffBits;                     // The offset, in bytes, from the beginning of the BITMAPFILEHEADER structure to the bitmap bits.
} BITMAPFILEHEADER, *LPBITMAPFILEHEADER, *PBITMAPFILEHEADER;


https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapcoreheader

typedef struct tagBITMAPCOREHEADER {
  DWORD bcSize;
  WORD  bcWidth;
  WORD  bcHeight;
  WORD  bcPlanes;
  WORD  bcBitCount;
} BITMAPCOREHEADER, *LPBITMAPCOREHEADER, *PBITMAPCOREHEADER;


typedef struct tagBITMAPCOREINFO {
  BITMAPCOREHEADER bmciHeader;
  RGBTRIPLE        bmciColors[1];
} BITMAPCOREINFO, *LPBITMAPCOREINFO, *PBITMAPCOREINFO;
      
typedef struct tagBITMAPINFO {
  BITMAPINFOHEADER bmiHeader;
  RGBQUAD          bmiColors[1];
} BITMAPINFO, *LPBITMAPINFO, *PBITMAPINFO;


https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapv4header

typedef struct {                                             typedef struct {
  DWORD        bV4Size;                                        DWORD        bV5Size;
  LONG         bV4Width;                                       LONG         bV5Width;
  LONG         bV4Height;                                      LONG         bV5Height;
  WORD         bV4Planes;                                      WORD         bV5Planes;
  WORD         bV4BitCount;                                    WORD         bV5BitCount;
  DWORD        bV4V4Compression;                               DWORD        bV5Compression;
  DWORD        bV4SizeImage;                                   DWORD        bV5SizeImage;
  LONG         bV4XPelsPerMeter;                               LONG         bV5XPelsPerMeter;
  LONG         bV4YPelsPerMeter;                               LONG         bV5YPelsPerMeter;
  DWORD        bV4ClrUsed;                                     DWORD        bV5ClrUsed;
  DWORD        bV4ClrImportant;                                DWORD        bV5ClrImportant;
  DWORD        bV4RedMask;                                     DWORD        bV5RedMask;
  DWORD        bV4GreenMask;                                   DWORD        bV5GreenMask;
  DWORD        bV4BlueMask;                                    DWORD        bV5BlueMask;
  DWORD        bV4AlphaMask;                                   DWORD        bV5AlphaMask;
  DWORD        bV4CSType;                                      DWORD        bV5CSType;
  CIEXYZTRIPLE bV4Endpoints;                                   CIEXYZTRIPLE bV5Endpoints;
  DWORD        bV4GammaRed;                                    DWORD        bV5GammaRed;
  DWORD        bV4GammaGreen;                                  DWORD        bV5GammaGreen;
  DWORD        bV4GammaBlue;                                   DWORD        bV5GammaBlue;
} BITMAPV4HEADER, *LPBITMAPV4HEADER, *PBITMAPV4HEADER;       
                                                               DWORD        bV5Intent;
                                                               DWORD        bV5ProfileData;
                                                               DWORD        bV5ProfileSize;
                                                               DWORD        bV5Reserved;
                                                             } BITMAPV5HEADER, *LPBITMAPV5HEADER, *PBITMAPV5HEADER;

- Zeile 0 ist UNTEN!
- bei 32-Bit-Pixeln keine Padding-Bytes je Zeile nötig
- Bytefolge je Pixel: BGRA
       */

      /// <summary>
      /// Bitmap aus einem Color-Array erzeugen
      /// </summary>
      /// <param name="width"></param>
      /// <param name="heigth"></param>
      /// <param name="pixel"></param>
      /// <returns></returns>
      //public static Bitmap CreateBitmap32(int width, int heigth, Color[] pixel) {
      //   if (width * heigth != pixel.Length)
      //      throw new Exception("CreateBitmap()");

      //   Bitmap bm;
      //   MemoryStream ms = new MemoryStream();
      //   using (BinaryWriter bw = new BinaryWriter(ms)) {
      //      writeHeaderV4(bw, width, heigth);

      //      for (int y = heigth - 1; y >= 0; y--)
      //         for (int x = 0; x < width; x++) {
      //            Color col = pixel[y * width + x];
      //            bw.Write(col.B);
      //            bw.Write(col.G);
      //            bw.Write(col.R);
      //            bw.Write(col.A);
      //         }

      //      ms.Position = 0;
      //      Bitmap tmp = new Bitmap(ms);
      //      bm = new Bitmap(tmp);
      //      tmp.Dispose();
      //      tmp.Dispose();
      //   }
      //   return bm;
      //}

      /// <summary>
      /// Bitmap aus einem UInt32-Array mit ARGB-Werten (siehe <see cref="GetUInt4Color"/>()) erzeugen
      /// </summary>
      /// <param name="width"></param>
      /// <param name="heigth"></param>
      /// <param name="pixel"></param>
      /// <returns></returns>
      public static Bitmap CreateBitmap32(int width, int heigth, UInt32[] pixel) {
         if (width * heigth != pixel.Length)
            throw new Exception("CreateBitmap()");

         Bitmap bm;

#if GMAP4SKIA
         // Der Umweg über den Stream fkt. mit SKBitmap unter Android NICHT für BMP (aber z.B. für PNG und JPEG).

         bm = new Bitmap(width, heigth);
         SKColor[] p = new SKColor[pixel.Length];
         for (int i = 0; i < pixel.Length; i++)
            p[i] = pixel[i];
         bm.Pixels = p;
#else
         MemoryStream ms = new MemoryStream();
         using (BinaryWriter bw = new BinaryWriter(ms)) {
            writeHeaderV4(bw, width, heigth);

            for (int y = heigth - 1; y >= 0; y--)
               for (int x = 0; x < width; x++)
                  bw.Write(pixel[y * width + x]);

            ms.Position = 0;
            Bitmap tmp = new Bitmap(ms);
            bm = new Bitmap(tmp);
            tmp.Dispose();
            tmp.Dispose();
         }
#endif
         return bm;
      }

      /// <summary>
      /// liefert einen Farbwert als UInt32
      /// </summary>
      /// <param name="alpha"></param>
      /// <param name="r"></param>
      /// <param name="g"></param>
      /// <param name="b"></param>
      /// <returns></returns>
      public static UInt32 GetUInt4Color(int alpha, int r, int g, int b) {
         return (uint)((alpha << 24) | (r << 16) | (g << 8) | b);
      }

#if !GMAP4SKIA
      static void writeHeaderV4(BinaryWriter bw, int width, int heigth) {
         // BMP Header 
         // 0h    2  42 4D    "BM"  ID field(42h, 4Dh)
         // 2h    4  9A 00 00 00    154 bytes(122 + 32)   Size of the BMP file
         // 6h    2  00 00    Unused Application specific
         // 8h    2  00 00    Unused Application specific
         // Ah    4  7A 00 00 00    122 bytes(14 + 108)   Offset where the pixel array(bitmap data) can be found
         bw.Write('B');
         bw.Write('M');
         bw.Write(0x7A + width * heigth * 4); // in Byte
         bw.Write(0);
         bw.Write(0x7A);

         // DIB Header

         // Eh    4  6C 00 00 00    108 bytes Number of bytes in the DIB header(from this point)
         // 12h   4  04 00 00 00    4 pixels(left to right order)   Width of the bitmap in pixels
         // 16h   4  02 00 00 00    2 pixels(bottom to top order)   Height of the bitmap in pixels
         // 1Ah   2  01 00    1 plane Number of color planes being used
         // 1Ch   2  20 00    32 bits Number of bits per pixel
         bw.Write(0x6C);       // DWORD bV4Size;    
         bw.Write(width);      // LONG  bV4Width;   
         bw.Write(heigth);     // LONG  bV4Height;  
         bw.Write((short)1);   // WORD  bV4Planes;  
         bw.Write((short)32);  // WORD  bV4BitCount;

         // 1Eh   4  03 00 00 00    3  BI_BITFIELDS, no pixel array compression used
         // 22h   4  20 00 00 00    32 bytes Size of the raw bitmap data(including padding)
         // 26h   4  13 0B 00 00    2835 pixels / metre horizontal Print resolution of the image,   72 DPI × 39.3701 inches per metre yields 2834.6472
         // 2Ah   4  13 0B 00 00    2835 pixels / metre vertical                                    72 DPI × 39.3701 inches per metre yields 2834.6472
         // 2Eh   4  00 00 00 00    0 colors Number of colors in the palette
         // 32h   4  00 00 00 00    0 important colors   0 means all colors are important
         bw.Write(3);                  // DWORD bV4V4Compression; 
         /*
            BI_RGB = 0x0000,           The bitmap is in uncompressed red green blue (RGB) format that is not compressed and does not use color masks
            BI_RLE8 = 0x0001,
            BI_RLE4 = 0x0002,
            BI_BITFIELDS = 0x0003,     Specifies that the bitmap is not compressed. The members bV4RedMask, bV4GreenMask, and bV4BlueMask specify the red, green, 
                                       and blue components for each pixel. This is valid when used with 16- and 32-bpp bitmaps.
            BI_JPEG = 0x0004,
            BI_PNG = 0x0005,
            BI_CMYK = 0x000B,
            BI_CMYKRLE8 = 0x000C,
            BI_CMYKRLE4 = 0x000D
          */
         bw.Write(width * heigth * 4); // DWORD bV4SizeImage;     in Byte
         bw.Write(2835);               // LONG  bV4XPelsPerMeter; 
         bw.Write(2835);               // LONG  bV4YPelsPerMeter; 
         bw.Write(0);                  // DWORD bV4ClrUsed;       
         bw.Write(0);                  // DWORD bV4ClrImportant;  

         // 36h   4  00 00 FF 00    00FF0000 in big - endian  Red channel bit mask(valid because BI_BITFIELDS is specified)
         // 3Ah   4  00 FF 00 00    0000FF00 in big - endian  Green channel bit mask(valid because BI_BITFIELDS is specified)
         // 3Eh   4  FF 00 00 00    000000FF in big - endian  Blue channel bit mask(valid because BI_BITFIELDS is specified)
         // 42h   4  00 00 00 FF FF000000 in big - endian  Alpha channel bit mask
         // 46h   4  20 6E 69 57    little - endian "Win "    LCS_WINDOWS_COLOR_SPACE
         // 4Ah   24h   24h * 00...00   CIEXYZTRIPLE Color Space endpoints  Unused for LCS "Win " or "sRGB"
         // 6Eh   4  00 00 00 00    0 Red Gamma    Unused for LCS "Win " or "sRGB"
         // 72h   4  00 00 00 00    0 Green Gamma  Unused for LCS "Win " or "sRGB"
         // 76h   4  00 00 00 00    0 Blue Gamma   Unused for LCS "Win " or "sRGB"

         bw.Write(0x00FF0000);   // DWORD bV4RedMask;  
         bw.Write(0x0000FF00);   // DWORD bV4GreenMask;
         bw.Write(0x000000FF);   // DWORD bV4BlueMask; 
         bw.Write(0xFF000000);   // DWORD bV4AlphaMask;
         bw.Write(0x57696E20);   // DWORD bV4CSType;
         /*
            typedef  enum
             {
               LCS_CALIBRATED_RGB = 0x00000000,
               LCS_sRGB = 0x73524742,
               LCS_WINDOWS_COLOR_SPACE = 0x57696E20
             } LogicalColorSpace;
         */
         /* CIEXYZTRIPLE bV4Endpoints;                              
            auch:
               LONG RedX;          X coordinate of red endpoint
               LONG RedY;          Y coordinate of red endpoint
               LONG RedZ;          Z coordinate of red endpoint
               LONG GreenX;        X coordinate of green endpoint
               LONG GreenY;        Y coordinate of green endpoint
               LONG GreenZ;        Z coordinate of green endpoint
               LONG BlueX;         X coordinate of blue endpoint
               LONG BlueY;         Y coordinate of blue endpoint
               LONG BlueZ;         Z coordinate of blue endpoint
         */
         for (int i = 0; i < 9; i++)
            bw.Write(0);

         bw.Write(0);   // DWORD bV4GammaRed;  
         bw.Write(0);   // DWORD bV4GammaGreen;
         bw.Write(0);   // DWORD bV4GammaBlue; 
      }
#endif

      //public static Bitmap Testbild(int width, int heigth, int alpha) {
      //   Color[] pixel = new Color[width * heigth];
      //   Color col;
      //   for (int y = 0; y < heigth; y++) {
      //      if (y < heigth / 3)
      //         col = Color.FromArgb(alpha, 255, 0, 0);
      //      else if (y < (2 * heigth) / 3)
      //         col = Color.FromArgb(alpha, 0, 255, 0);
      //      else
      //         col = Color.FromArgb(alpha, 0, 0, 255);

      //      for (int x = 0; x < width; x++)
      //         pixel[x + y * width] = col;
      //   }
      //   return CreateBitmap32(width, heigth, pixel);
      //}

      //public static Bitmap Testbild2(int width, int heigth, int alpha) {
      //   uint[] pixel = new uint[width * heigth];
      //   uint col;
      //   for (int y = 0; y < heigth; y++) {
      //      if (y < heigth / 3)
      //         col = GetUInt4Color(alpha, 255, 0, 0);
      //      else if (y < (2 * heigth) / 3)
      //         col = GetUInt4Color(alpha, 0, 255, 0);
      //      else
      //         col = GetUInt4Color(alpha, 0, 0, 255);

      //      for (int x = 0; x < width; x++)
      //         pixel[x + y * width] = col;
      //   }
      //   return CreateBitmap32(width, heigth, pixel);
      //}

   }
}
