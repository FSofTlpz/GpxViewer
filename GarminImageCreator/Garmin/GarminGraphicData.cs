using GarminCore;
using GarminCore.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
#if DRAWWITHSKIA
using SkiaSharp;
using System.IO;
using System.Reflection;
#endif

namespace GarminImageCreator.Garmin {

   /// <summary>
   /// grafischen Definitionen aller Objektarten einer Garmin-Karte (aus der TYP-Datei)
   /// </summary>
   public class GarminGraphicData : IDisposable {

      #region Klassen für die Objektarten

      abstract public class ObjectData : IDisposable {
         readonly object bitmap_locker = new object();

         public uint Type { get; protected set; }

         public uint Subtype { get; protected set; }

         public uint Fulltype => GetFulltype(Type, Subtype);

         public static uint GetFulltype(uint type, uint subtype) => (type << 8) + subtype;

         public string? Name { get; protected set; }

         public bool WithBitmap => Bitmap != null;

         protected Bitmap? Bitmap;

         /// <summary>
         /// liefert threadsicher eine Kopie des Bitmaps oder null (bei Multithreading nötig)
         /// </summary>
         public Bitmap? BitmapClone {
            get {
               lock (bitmap_locker) {
                  return Bitmap != null ?
                              Bitmap.Clone() as Bitmap :
                              null;
               }
            }
         }

         /// <summary>
         /// liefert threadsicher einen TextureBrush für das Bitmap oder null (bei Multithreading nötig)
         /// </summary>
         public TextureBrush? BitmapAsTextureBrush {
            get {
               Bitmap? bm = BitmapClone;
               TextureBrush? tb = null;
               if (bm != null) {
                  tb = new TextureBrush(bm) {
                     WrapMode = System.Drawing.Drawing2D.WrapMode.Tile,
                  };
                  bm.Dispose();
               }
               return tb;
            }
         }


         /// <summary>
         /// zusätzlicher transparenter Rand
         /// </summary>
         public readonly int AdditionalEdge = 2;

         /// <summary>
         /// entsprechend der groben Garmin-Angabe
         /// </summary>
         public enum FontSize {
            NoFont = 0,
            Small = 1,
            Default = 2,
            Normal = 3,
            Large = 4,
         }

         public FontSize Fontsize = FontSize.NoFont;

         public Color TextColor { get; protected set; }


         /// <summary>
         /// liefert threadsicher eine Textur aus dem <see cref="Bitmap"/> für Linien (etwas breiter als das normale Bitmap)
         /// </summary>
         public TextureBrush? GetTextureBrush(float factor) {
            lock (bitmap_locker) {
               if (Bitmap != null) {
                  TextureBrush? tb = null;
                  int h = (int)(factor * (Bitmap.Height + 2 * AdditionalEdge)); // etwas breiter, damit beim "Kacheln" kein "Schatten" vom anderen Rand auftaucht
                  using (Bitmap bm = new Bitmap(Bitmap.Width, h)) {
                     using (Graphics g = Graphics.FromImage(bm)) {
                        g.Clear(Color.Transparent);
                        if (factor != 1F) {
                           int edge = (int)Math.Round(factor * AdditionalEdge);
                           g.DrawImage(Bitmap, 0, edge, Bitmap.Width, h - 2 * edge);
                        } else
                           g.DrawImageUnscaled(Bitmap, 0, 2);
                     }

                     tb = new TextureBrush(bm);
                     tb.TranslateTransform(0, bm.Height / 2F);
                  }
                  return tb;
               }
               return null;
            }
         }

         protected void setFontIndex(GarminCore.Files.Typ.GraphicElement.Fontdata fd) {
            switch (fd) {
               case GarminCore.Files.Typ.GraphicElement.Fontdata.Nolabel:
               default:
                  Fontsize = FontSize.NoFont;
                  break;

               case GarminCore.Files.Typ.GraphicElement.Fontdata.Small:
                  Fontsize = FontSize.Small;
                  break;

               case GarminCore.Files.Typ.GraphicElement.Fontdata.Default:
                  Fontsize = FontSize.Default;
                  break;

               case GarminCore.Files.Typ.GraphicElement.Fontdata.Normal:
                  Fontsize = FontSize.Normal;
                  break;

               case GarminCore.Files.Typ.GraphicElement.Fontdata.Large:
                  Fontsize = FontSize.Large;
                  break;
            }
         }

         protected void setFontColor(GarminCore.Files.Typ.GraphicElement.FontColours fc, Color col1, Color col2) {
            switch (fc) {
               case GarminCore.Files.Typ.GraphicElement.FontColours.No:             // nicht vorgegeben
                  TextColor = Color.Black;
                  break;

               case GarminCore.Files.Typ.GraphicElement.FontColours.Day:
                  TextColor = col1;
                  break;

               case GarminCore.Files.Typ.GraphicElement.FontColours.DayAndNight:
                  TextColor = col1;
                  break;

               case GarminCore.Files.Typ.GraphicElement.FontColours.Night:
                  TextColor = col2;
                  break;

            }
         }

         ~ObjectData() {
            Dispose(false);
         }

         #region Implementierung der IDisposable-Schnittstelle

         /// <summary>
         /// true, wenn schon ein Dispose() erfolgte
         /// </summary>
         private bool _isdisposed = false;

         /// <summary>
         /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
         /// </summary>
         public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
         }

         /// <summary>
         /// überschreibt die Standard-Methode
         /// <para></para>
         /// </summary>
         /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
         protected virtual void Dispose(bool notfromfinalizer) {
            if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
               if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
                  Bitmap?.Dispose();
               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion
      }

      public class AreaData : ObjectData {

         public readonly Color InnerColor;

         public AreaData(GarminCore.Files.Typ.Polygone area) :
            base() {
            Type = area.Type;
            Subtype = area.Subtype;
            Name = area.Text.Get(GarminCore.Files.Typ.Text.LanguageCode.german);
            if (string.IsNullOrEmpty(Name))
               Name = area.Text.Get(0).Txt;
            if (area.WithDayBitmap) {
               Bitmap = area.AsBitmap(true, false);
            } else {
               switch (area.Colortype) {
                  case GarminCore.Files.Typ.Polygone.ColorType.Day1:
                  case GarminCore.Files.Typ.Polygone.ColorType.Day1_Night1:
                     InnerColor = area.DayColor1;
                     break;
               }
            }

            setFontIndex(area.FontType);
            setFontColor(area.FontColType, area.FontColor1, area.FontColor2);
         }

         public Bitmap GetAsBitmap(int length) {
            Bitmap bm = new Bitmap(length, Bitmap != null ? Bitmap.Height : length);
            using (Graphics g = Graphics.FromImage(bm)) {
               g.Clear(Color.Transparent);
               if (WithBitmap) {
                  if (Bitmap != null)
                     for (int x = 0; x < length; x += Bitmap.Width)
                        g.DrawImageUnscaled(Bitmap, x, 0);
               } else {

                  g.Clear(InnerColor);

               }
            }
            return bm;
         }

         /// <summary>
         /// erzeugt einen Brush
         /// </summary>
         /// <returns></returns>
         public Brush? GetBrush() => !WithBitmap ? new SolidBrush(InnerColor) : BitmapAsTextureBrush;

         public override string ToString() {
            return string.Format("[Fulltype={0:x4}, Name={1}, Bitmap={2}x{3}]",
                                 Fulltype,
                                 Name,
                                 Bitmap != null ? Bitmap.Width : 0,
                                 Bitmap != null ? Bitmap.Height : 0);
         }

      }

      public class LineData : ObjectData {

         public readonly Color InnerColor;
         public readonly Color EdgeColor;
         public readonly uint InnerWidth;
         public readonly uint Width;
         public readonly bool WithBorder;

         /// <summary>
         /// Test für "Verbreiterungsfaktor" (1 ist Originalbreite)
         /// </summary>
         readonly float widthFactor;

         public readonly bool IsTransparent;


         public LineData(GarminCore.Files.Typ.Polyline line, double linefactor) : base() {
            Type = line.Type;
            Subtype = line.Subtype;
            widthFactor = (float)linefactor;
            Name = line.Text.Get(GarminCore.Files.Typ.Text.LanguageCode.german);
            if (string.IsNullOrEmpty(Name))
               Name = line.Text.Get(0).Txt;
            InnerWidth = 0;
            Width = 0;
            WithBorder = false;
            IsTransparent = false;

            if (line.WithDayBitmap) {
               Bitmap = line.AsBitmap(true, false);

               //if (Type == 0x115) {// && Subtype == 0x01) {
               //   Bitmap = new Bitmap(32, 12);
               //   Graphics g = Graphics.FromImage(Bitmap);
               //   g.Clear(Color.LightGreen);

               //   Pen pen1 = new Pen(Color.Red, 1);
               //   Pen pen2 = new Pen(Color.Blue, 1);

               //   g.DrawLine(pen1, 0, 0, Bitmap.Height / 2F, Bitmap.Height / 2F);               //  \
               //   g.DrawLine(pen2, 0, Bitmap.Height, Bitmap.Height / 2F, Bitmap.Height / 2F);   //  /
               //   g.FillEllipse(new SolidBrush(Color.Blue), Bitmap.Height / 2F - 3, Bitmap.Height / 2F - 3, 6, 6);

               //   g.DrawLine(pen1, 16, 0, 16 + Bitmap.Height / 2F, Bitmap.Height / 2F);
               //   g.DrawLine(pen2, 16, Bitmap.Height, 16 + Bitmap.Height / 2F, Bitmap.Height / 2F);
               //   g.FillEllipse(new SolidBrush(Color.Blue), 16 + Bitmap.Height / 2F - 3, Bitmap.Height / 2F - 3, 6, 6);

               //   g.Flush();
               //}

               if (Bitmap != null) {
                  Width = InnerWidth = (uint)Bitmap.Height;

                  IsTransparent = true;
                  for (int x = 0; x < Bitmap.Width; x++)
                     for (int y = 0; y < Bitmap.Height; y++)
                        if (Bitmap.GetPixel(x, y).A != 0) {     // wenigstens 1 nicht volltransparentes Pixel
                           IsTransparent = false;
                           break;
                        }
               }

            } else {
               switch (line.Polylinetype) {
                  case GarminCore.Files.Typ.Polyline.PolylineType.Day1_Night2:
                     InnerColor = line.DayColor1;
                     if (InnerColor.A == 0)
                        IsTransparent = true;
                     break;
                  case GarminCore.Files.Typ.Polyline.PolylineType.Day2:
                  case GarminCore.Files.Typ.Polyline.PolylineType.Day2_Night2:
                     InnerColor = line.DayColor1;
                     EdgeColor = line.DayColor2;
                     if (InnerColor.A == 0 &&
                         EdgeColor.A == 0)
                        IsTransparent = true;
                     WithBorder = true;
                     break;
                  case GarminCore.Files.Typ.Polyline.PolylineType.NoBorder_Day1:
                  case GarminCore.Files.Typ.Polyline.PolylineType.NoBorder_Day1_Night1:
                     InnerColor = line.DayColor1;
                     if (InnerColor.A == 0)
                        IsTransparent = true;
                     break;
                  case GarminCore.Files.Typ.Polyline.PolylineType.NoBorder_Day2_Night1:
                     InnerColor = line.DayColor1;
                     EdgeColor = line.DayColor2;
                     if (InnerColor.A == 0 &&
                         EdgeColor.A == 0)
                        IsTransparent = true;
                     WithBorder = true;
                     break;
               }
               InnerWidth = line.InnerWidth;
               Width = InnerWidth + line.BorderWidth * 2;

            }

            setFontIndex(line.FontType);
            setFontColor(line.FontColType, line.FontColor1, line.FontColor2);
         }

         /// <summary>
         /// erzeugt einen neuen Pen (auch als Rand bei Linien mit Rand)
         /// </summary>
         /// <returns></returns>
         public Pen? GetPen() {
            if (WithBitmap) {
               Pen? pen = null;

               using (TextureBrush? tb = GetTextureBrush(widthFactor)) {
                  if (tb != null)
                     pen = new Pen(tb,
                                   widthFactor * (tb.Image.Height - AdditionalEdge)) {    // in Win nötig!!!
                        StartCap = System.Drawing.Drawing2D.LineCap.Round,
                        EndCap = System.Drawing.Drawing2D.LineCap.Round,
                        Alignment = System.Drawing.Drawing2D.PenAlignment.Outset,
                        /*
                              Center   0 	Gibt an, dass das Pen-Objekt auf der theoretischen Linie zentriert ist.
                              Inset    1 	Gibt an, dass sich das Pen-Objekt auf der Innenseite der theoretischen Linie befindet.
                              Left     3 	Gibt an, dass das Pen-Objekt links von der theoretischen Linie positioniert ist.
                              Outset   2 	Gibt an, dass das Pen-Objekt außerhalb der theoretischen Linie positioniert ist.
                              Right    4 	Gibt an, dass das Pen-Objekt rechts von der theoretischen Linie positioniert ist.
                         */
                     };
               }


               //using (TextureBrush tb = GetTextureBrush(widthFactor)) {
               //   pen = new Pen(tb) {
               //      StartCap = System.Drawing.Drawing2D.LineCap.Round,
               //      EndCap = System.Drawing.Drawing2D.LineCap.Round,
               //      //Alignment = System.Drawing.Drawing2D.PenAlignment.Outset,
               //      /*
               //            Center   0 	Gibt an, dass das Pen-Objekt auf der theoretischen Linie zentriert ist.
               //            Inset    1 	Gibt an, dass sich das Pen-Objekt auf der Innenseite der theoretischen Linie befindet.
               //            Left     3 	Gibt an, dass das Pen-Objekt links von der theoretischen Linie positioniert ist.
               //            Outset   2 	Gibt an, dass das Pen-Objekt außerhalb der theoretischen Linie positioniert ist.
               //            Right    4 	Gibt an, dass das Pen-Objekt rechts von der theoretischen Linie positioniert ist.
               //       */
               //   };
               //}
               return pen;
            } else {
               //Pen pen = new Pen(WithBorder ? EdgeColor : InnerColor,
               //                  Factor * (WithBorder ? Width : InnerWidth)) {
               //   LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
               //   EndCap = System.Drawing.Drawing2D.LineCap.Round,
               //   StartCap = System.Drawing.Drawing2D.LineCap.Round
               //};

               return WithBorder ?
                           getPen(EdgeColor, widthFactor * Width) :
                           getPen(InnerColor, widthFactor * InnerWidth);  // liefert den Pen mit diesen Daten wenn kein Rand vorhanden ist
            }
         }

         /// <summary>
         /// erzeugt einen neuen Pen für das Innere wenn die Linie einen Rand hat
         /// </summary>
         /// <returns></returns>
         public Pen GetInnerPen() => getPen(InnerColor, widthFactor * InnerWidth);

         /// <summary>
         /// erzeugt ein Bitmap für die Linie
         /// </summary>
         /// <param name="length"></param>
         /// <returns></returns>
         public Bitmap GetAsBitmap(int length) {
            Bitmap bm = new Bitmap(length,
                                   WithBitmap ?
                                       Bitmap != null ? Bitmap.Height : 1 :
                                       AdditionalEdge + (int)Width);
            Graphics g = Graphics.FromImage(bm);
            g.Clear(Color.Transparent);

            if (WithBitmap) {
               if (Bitmap != null)
                  g.DrawImage(Bitmap, 0, (bm.Height - Bitmap.Height) / 2);
            } else {
               Pen? pen = GetPen();
               if (pen != null)
                  g.DrawLine(pen, 0, bm.Height / 2, length, bm.Height / 2);
               pen?.Dispose();
            }

            if (WithBorder) {
               Pen pen = GetInnerPen();
               g.DrawLine(pen, 0, bm.Height / 2, length, bm.Height / 2);
               pen.Dispose();
            }

            g.Flush();
            return bm;
         }

         Pen getPen(Color col, float width) => new Pen(col, width) {
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
         };

         public override string ToString() {
            return string.Format("[Fulltype={0:x4}, Name={1}, Width={2}, InnerWidth={3}, Bitmap={4}x{5}]",
                                 Fulltype,
                                 Name,
                                 Width,
                                 InnerWidth,
                                 Bitmap != null ? Bitmap.Width : 0,
                                 Bitmap != null ? Bitmap.Height : 0);
         }

      }

      public class PointData : ObjectData {

         public PointData(GarminCore.Files.Typ.POI point, double symbolfactor) :
            base() {
            Type = point.Type;
            Subtype = point.Subtype;
            Name = point.Text.Get(GarminCore.Files.Typ.Text.LanguageCode.german);
            if (string.IsNullOrEmpty(Name))
               Name = point.Text.Get(0).Txt;
            Bitmap? bmorg = point.AsBitmap(true);
            if (symbolfactor == 1.0)
               Bitmap = bmorg;
            else if (symbolfactor > 0) {
               if (bmorg != null) {
                  Bitmap = new Bitmap((int)Math.Round(symbolfactor * bmorg.Width),
                                      (int)Math.Round(symbolfactor * bmorg.Height));
                  if (Bitmap != null)
                     using (Graphics graphics = Graphics.FromImage(Bitmap))
                        graphics.DrawImage(bmorg, 0, 0, Bitmap.Width, Bitmap.Height);
               }
            }

            setFontIndex(point.FontType);
            setFontColor(point.FontColType, point.FontColor1, point.FontColor2);
         }

         public override string ToString() {
            return string.Format("[Fulltype={0:x4}, Name={1}, Bitmap={2}x{3}]",
                                 Fulltype,
                                 Name,
                                 Bitmap != null ? Bitmap.Width : 0,
                                 Bitmap != null ? Bitmap.Height : 0);
         }

      }

      #endregion

      /// <summary>
      /// verwaltet alle Fonts für einen bestimmten Größenfaktor
      /// </summary>
      public class ObjectFonts : IDisposable {

         public readonly double Factor;

         readonly Font?[] textFont;


         public ObjectFonts(double fontfactor, string fontname) {
            Factor = fontfactor;

            textFont = new Font[1 + (int)ObjectData.FontSize.Large];    // VORSICHT: nur korrekt wenn FontSize.Large die höchste Indexnummer hat
            textFont[(int)ObjectData.FontSize.NoFont] = null;
#if DRAWWITHSKIA
            dataStdTypeface = Array.Empty<byte>();
            loadExternFontdata(fontname);

            textFont[(int)ObjectData.FontSize.Small] = new Font(getStdSKTypeface(), 5 * (float)fontfactor);
            textFont[(int)ObjectData.FontSize.Default] = new Font(getStdSKTypeface(), 10 * (float)fontfactor);
            textFont[(int)ObjectData.FontSize.Normal] = new Font(getStdSKTypeface(), 10 * (float)fontfactor);
            textFont[(int)ObjectData.FontSize.Large] = new Font(getStdSKTypeface(), 12 * (float)fontfactor);
#else
            textFont[(int)ObjectData.FontSize.Small] = new Font(fontname, 5 * (float)fontfactor);
            textFont[(int)ObjectData.FontSize.Default] = new Font(fontname, 10 * (float)fontfactor);
            textFont[(int)ObjectData.FontSize.Normal] = new Font(fontname, 10 * (float)fontfactor);
            textFont[(int)ObjectData.FontSize.Large] = new Font(fontname, 12 * (float)fontfactor);
#endif
         }

         public Font? TextFont(ObjectData.FontSize size) => textFont[(int)size];

#if DRAWWITHSKIA
         byte[] dataStdTypeface;

         void loadExternFontdata(string fontname) {
            var assembly = Assembly.GetExecutingAssembly();
            try {
               using (Stream? stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + fontname + ".ttf")) {
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
                  dataStdTypeface = new byte[stream.Length];
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
                  int len = stream.Read(dataStdTypeface, 0, dataStdTypeface.Length);
                  if (len != dataStdTypeface.Length)
                     throw new Exception(nameof(GarminGraphicData) + "." + nameof(loadExternFontdata) + "(): Nicht genug Daten gelesen.");
               }
            } catch (Exception) {
               throw new Exception("Der Font '" + fontname + "' konnte nicht aus den Ressourcen geladen werden.");
            }
         }

         SKTypeface getStdSKTypeface() {
            MemoryStream tmp = new MemoryStream(dataStdTypeface);
            SKTypeface sKTypeface = SKTypeface.FromStream(tmp);
            tmp.Dispose();
            return sKTypeface;
         }

#endif

         ~ObjectFonts() {
            Dispose(false);
         }

         #region Implementierung der IDisposable-Schnittstelle

         /// <summary>
         /// true, wenn schon ein Dispose() erfolgte
         /// </summary>
         private bool _isdisposed = false;

         /// <summary>
         /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
         /// </summary>
         public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
         }

         /// <summary>
         /// überschreibt die Standard-Methode
         /// <para></para>
         /// </summary>
         /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
         protected virtual void Dispose(bool notfromfinalizer) {
            if (!_isdisposed) {            // bisher noch kein Dispose erfolgt
               if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
                  for (int i = 0; i < textFont.Length; i++)
                     textFont[i]?.Dispose();
               }
               // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

               _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            }
         }

         #endregion
      }


      /// <summary>
      /// Name der Garmin-TYP-Datei
      /// </summary>
      public readonly string Filename;

      /// <summary>
      /// Def. für jeden Gebietstyp
      /// </summary>
      public readonly Dictionary<long, AreaData> Areas;
      /// <summary>
      /// Def. für jeden Linientyp
      /// </summary>
      public readonly Dictionary<long, LineData> Lines;
      /// <summary>
      /// Def. für jeden Punkttyp
      /// </summary>
      public readonly Dictionary<long, PointData> Points;
      /// <summary>
      /// Reihenfolge für das Zeichnen von Gebietstypen; Value: upper 8 Bit Type, lower 8 Bit Subtype)
      /// </summary>
      public readonly SortedDictionary<uint, uint> AreaDrawOrder;

      public readonly ObjectFonts Fonts;

      public double FontFactor => Fonts.Factor;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="typfile"></param>
      /// <param name="fontname"></param>
      /// <param name="fontfactor">zum Ändern der Originalgröße der Texte</param>
      /// <param name="linefactor">zum Ändern der Originalbreite der Linien</param>
      /// <param name="symbolfactor">zum Ändern der Originalgröße der Symbole</param>
      public GarminGraphicData(string typfile,
                               string fontname,
                               double fontfactor = 1.0,
                               double linefactor = 1.0,
                               double symbolfactor = 1.0) {
         Filename = typfile;

         Lines = new Dictionary<long, LineData>();
         Points = new Dictionary<long, PointData>();
         Areas = new Dictionary<long, AreaData>();
         AreaDrawOrder = new SortedDictionary<uint, uint>();
         read(linefactor, symbolfactor);

         Fonts = new ObjectFonts(fontfactor, fontname);
      }


      void read(double linefactor, double symbolfactor) {
         try {
            using (BinaryReaderWriter br = new BinaryReaderWriter(Filename, true)) {
               StdFile_TYP typ = new StdFile_TYP();
               typ.Read(br);

               for (int i = 0; i < typ.PolylineCount; i++) {
                  GarminCore.Files.Typ.Polyline? polylinetyp = typ.GetPolyline(i);
                  if (polylinetyp != null)
                     Lines.Add(LineData.GetFulltype(polylinetyp.Type, polylinetyp.Subtype), new LineData(polylinetyp, linefactor));
               }

               for (int i = 0; i < typ.PoiCount; i++) {
                  GarminCore.Files.Typ.POI? point = typ.GetPoi(i);
                  if (point != null)
                     Points.Add(PointData.GetFulltype(point.Type, point.Subtype), new PointData(point, symbolfactor));
               }

               uint draworderdelta = 0;
               for (int i = 0; i < typ.PolygonCount; i++) {
                  GarminCore.Files.Typ.Polygone? area = typ.GetPolygone(i);
                  if (area != null) {
                     while (AreaDrawOrder.ContainsKey(area.Draworder + draworderdelta))
                        draworderdelta++;
                     AreaDrawOrder.Add(area.Draworder + draworderdelta, (area.Type << 8) | area.Subtype);
                     Areas.Add(AreaData.GetFulltype(area.Type, area.Subtype), new AreaData(area));
                  }
               }
               typ.Dispose();
            }
         } catch (Exception ex) {
            throw new Exception("error reading TYP '" + Filename + "': " + ex.Message);
         }
      }

      ~GarminGraphicData() {
         Dispose(false);
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               Fonts.Dispose();

               for (int i = 0; i < Lines.Count; i++)
                  Lines[i].Dispose();
               Lines.Clear();

               for (int i = 0; i < Points.Count; i++)
                  Points[i].Dispose();
               Points.Clear();

               for (int i = 0; i < Areas.Count; i++)
                  Areas[i].Dispose();
               Areas.Clear();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
