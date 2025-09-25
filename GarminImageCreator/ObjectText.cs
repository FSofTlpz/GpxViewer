using FSofTUtils.Geometry;
using System;
using System.Drawing;

namespace GarminImageCreator {
   /// <summary>
   /// zum Anzeigen des Textes eines Objekts
   /// </summary>
   class ObjectText : IComparable, IDisposable {

      public enum ObjectType {
         Area,
         Line,
         Point
      }

      public ObjectType ObjType { get; protected set; }
      public int GarminType { get; protected set; }
      public string Text { get; protected set; }
      public Font Font { get; protected set; }
      public Color Color { get; protected set; }
      /// <summary>
      /// Referenzpunkt für den Text
      /// </summary>
      public PointF ReferencePoint { get; protected set; }
      /// <summary>
      /// nur bei Lines verwendet
      /// </summary>
      public PointF ReferencePoint2 { get; protected set; }
      /// <summary>
      /// Winkel (bzgl. x-Achse) in Grad
      /// </summary>
      public float Angle { get; protected set; }

      /// <summary>
      /// merkt sich ein einmal berechnetes Rechteck (für einen waagerechten Text)
      /// </summary>
      RectangleF simpleTextArea = RectangleF.Empty;
      /// <summary>
      /// "schräge" Rechtecke (bei <see cref="Angle"/> != 0) können nur als Region behandelt werden
      /// </summary>
      RectangleCommon? commonTextArea = null;

      bool textAreaIsCalculated = false;

      /// <summary>
      /// kurz für <see cref="Font.GetHeight()"/> (zur Opt.)
      /// </summary>
      float fontGetHeight = 0;


      /// <summary>
      /// i.A. für POI's und Flächen (Text waagerecht)
      /// </summary>
      /// <param name="text"></param>
      /// <param name="type"></param>
      /// <param name="garmintype"></param>
      /// <param name="font"></param>
      /// <param name="col"></param>
      /// <param name="pt"></param>
      /// <param name="fontheight">kurz für <see cref="Font.GetHeight()"/> (zur Opt.)</param>
      public ObjectText(string text, ObjectType type, int garmintype, Font font, Color col, PointF pt, float fontheight) {
         ObjType = type;
         GarminType = garmintype;
         Text = text[0] >= 0x20 ?
                     text :
                     text.Substring(1);   // Codes kleiner 0x20 haben eine Sonderbedeutung: "Schild" vor dem Namen
         if (Text.Length > 0 &&
             !hasLowerChars())
            simpleGarminTextConvert();
         Font = font;
         fontGetHeight = fontheight <= 0 ? Font.GetHeight() : fontheight;
         Color = col;
         ReferencePoint = pt;
         Angle = 0;
      }

      /// <summary>
      /// i.A. für Linien (Text an Linie angepasst)
      /// </summary>
      /// <param name="text"></param>
      /// <param name="type"></param>
      /// <param name="garmintype"></param>
      /// <param name="font"></param>
      /// <param name="col"></param>
      /// <param name="pt"></param>
      /// <param name="pt2"></param>
      /// <param name="fontheight">kurz für <see cref="Font.GetHeight()"/> (zur Opt.)</param>
      public ObjectText(string text, ObjectType type, int garmintype, Font font, Color col, PointF pt, PointF pt2, float fontheight) :
         this(text, type, garmintype, font, col, pt, fontheight) {
         ReferencePoint2 = pt2;
      }

      /// <summary>
      /// setzt das interne einfache Textrechteck (für waagerechten Text), bei Bedarf auch das schräge Textrechteck und liefert es
      /// </summary>
      /// <param name="canvas"></param>
      /// <param name="sf"></param>
      /// <param name="tilesize"></param>
      /// <returns></returns>
      public RectangleF GetTextArea(Graphics canvas, StringFormat sf, float tilesize) {
         if (!textAreaIsCalculated) {
            textAreaIsCalculated = true;

            if (ObjType == ObjectType.Line) // dann Winkel und Referenzpunkt ermitteln
               ReferencePoint = getAngleAndPoint4LineText(ReferencePoint, ReferencePoint2);

            if (0 <= ReferencePoint.X && ReferencePoint.X < tilesize &&    // der Referenzpunkt muss innerhalb des Tiles liegen
                0 <= ReferencePoint.Y && ReferencePoint.Y < tilesize) {
               SizeF stringSize = canvas.MeasureString(Text, Font, ReferencePoint, sf);

               // Referenzpunkt entsprechend der Ausrichtung setzen
               if (stringSize.Width <= tilesize) {
                  float x = ReferencePoint.X;
                  switch (sf.Alignment) {
                     case StringAlignment.Near:
                        break;

                     case StringAlignment.Center:
                        x -= stringSize.Width / 2;
                        break;

                     case StringAlignment.Far:
                        x -= stringSize.Width;
                        break;
                  }

                  float y = ReferencePoint.Y;
                  switch (sf.LineAlignment) {
                     case StringAlignment.Near:
                        break;

                     case StringAlignment.Center:
                        y -= stringSize.Height / 2;
                        break;

                     case StringAlignment.Far:
                        y -= stringSize.Height;
                        break;
                  }

                  simpleTextArea = new RectangleF(x, y, stringSize.Width, stringSize.Height);

                  makeRectangleValid(tilesize, tilesize);
               }
            }
         }
         return simpleTextArea;
      }

      //public void MoveRectangle(float dx, float dy, float tilesize) {
      //   if (simpleTextArea != RectangleF.Empty) {
      //      simpleTextArea.X += dx;
      //      simpleTextArea.Y += dy;

      //      makeRectangleValid(tilesize, tilesize);
      //   }
      //}

      /// <summary>
      /// Pos. auf jeden Fall gültig machen, d.h. bei Bedarf <see cref="ReferencePoint"/> und <see cref="simpleTextArea"/> anpassen
      /// </summary>
      /// <param name="pictwidth"></param>
      /// <param name="pictheight"></param>
      void makeRectangleValid(float pictwidth, float pictheight) {
         if (Angle == 0) {

            if (getFitting2Area(simpleTextArea, pictwidth, pictheight, out SizeF translation)) {
               if (translation != SizeF.Empty) {
                  simpleTextArea.X += translation.Width;
                  simpleTextArea.Y += translation.Height;
                  ReferencePoint = PointF.Add(ReferencePoint, translation);
               }
            } else // Fitting ist nicht möglich
               simpleTextArea = RectangleF.Empty;

         } else { // "schräges" Rechteck als Region

            commonTextArea = new RectangleCommon(simpleTextArea.Location, simpleTextArea.Width, simpleTextArea.Height, Angle, RectangleCommon.RotationPoint.Center);
            if (getFitting2Area(commonTextArea, pictwidth, pictheight, out SizeF translation)) {
               if (translation != SizeF.Empty) {
                  commonTextArea.Move(translation.Width, translation.Height);
                  ReferencePoint = PointF.Add(ReferencePoint, translation);
               }
            } else { // Fitting ist nicht möglich
               simpleTextArea = RectangleF.Empty;
               commonTextArea = null;
            }

         }
      }

      /// <summary>
      /// liefert die nötige Translation, um ein RectangleF in ein Gebiet einzupassen
      /// </summary>
      /// <param name="rect"></param>
      /// <param name="areawidth"></param>
      /// <param name="areaheight"></param>
      /// <param name="translation"></param>
      /// <returns>false, wenn ein Einpassen nicht möglich ist</returns>
      bool getFitting2Area(RectangleF rect, float areawidth, float areaheight, out SizeF translation) {
         translation = SizeF.Empty;

         if (rect.Width > areawidth ||
             rect.Height > areaheight)    // Einpassen unmöglich
            return false;

         if (rect.X < 0)
            translation.Width = -rect.X;
         if (areawidth < rect.X + rect.Width)
            translation.Width = (areawidth - rect.Width) - rect.X;

         if (rect.Y < 0)
            translation.Height = -rect.Y;
         if (areaheight < rect.Y + rect.Height)
            translation.Height = (areaheight - rect.Height) - rect.Y;

         return true;
      }

      /// <summary>
      /// liefert die nötige Translation, um ein <see cref="RectangleCommon"/> in ein Gebiet einzupassen
      /// </summary>
      /// <param name="rect"></param>
      /// <param name="areawidth"></param>
      /// <param name="areaheight"></param>
      /// <param name="translation"></param>
      /// <returns>false, wenn ein Einpassen nicht möglich ist</returns>
      bool getFitting2Area(RectangleCommon rect, float areawidth, float areaheight, out SizeF translation) {
         RectangleF bounds = rect.GetBounds();
         if (getFitting2Area(bounds, areawidth, areaheight, out translation)) {
            if (translation != SizeF.Empty) {
               bounds.X += translation.Width;
               if (rect.BottomRight.X != rect.BottomLeft.X) {
                  float exty = (rect.BottomRight.Y - rect.BottomLeft.Y) / (rect.BottomRight.X - rect.BottomLeft.X) * translation.Width; // Zusatzbetrag entsprechend dem Anstieg
                  bounds.Y -= exty;

                  if (0 <= bounds.Top && bounds.Bottom <= areaheight)
                     translation.Height -= exty;
               }
            }
            return true;
         }
         return false;

         // ACHTUNG: Besser wäre wahrscheinlich eine Verschiebung auf der Gerade durch die 2 Referenzpunkte.
         //          Aber eigentlich müßte nach der Verschiebung wieder eine neue Anpassung an die Linie erfolgen.

         //return getFitting2Area(rect.GetBounds(), areawidth, areaheight, out translation);
      }

      /// <summary>
      /// Durch die 2 Punkte (der Linie) ist eine Referenzstrecke vorgegeben, die die Neigung und den Mittelpunkt des Textes angiebt.
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns>Mittelpunkt des Textes</returns>
      PointF getAngleAndPoint4LineText(PointF pt1, PointF pt2) {
         /*
            Drehwinkel zählt bzgl. der x-Achse!
            Da alle Texte "von rechts" und/oder "von unten" lesbar sein sollen, ist der Drehwinkel auf -90°..90° beschränkt.
          */

         float dx = pt2.X - pt1.X;
         float dy = pt2.Y - pt1.Y;

         if (dx == 0)
            Angle = -90;
         else {
            Angle = (float)(Math.Atan(dy / dx) / Math.PI * 180);

            while (Angle < -90)
               Angle += 180;
            while (Angle > 90)
               Angle -= 180;
         }

         PointF pt = new PointF(pt1.X + dx / 2, pt1.Y + dy / 2);  // Mittelpunkt der Strecke

         /*
         Damit der Text ÜBER der Mittellinie der Linie steht, muss er noch um die Texthöhe verschoben werden.
         
         Das Dreieck aus dx, dy und der Strecke P1P2 ist math. ähnlich dem Dreieck aus der Texthöhe und den Korrekturwerten für x und y 
         für den Referenzpunkt.
         Deshalb wird der Faktor aus der Länge der Strecke P1P2 und der Texthöhe gebildet. Mit diesem Faktor können aus dx und dy die
         Korrektwerte einfach bestimmt werden.
          */
         //float lineheight = Font.GetHeight();
         float f = fontGetHeight / (float)Math.Sqrt(dx * dx + dy * dy);
         f = f * 3 / 4;

         if (Angle < 0) {  // nach oben geneigt (SW <-> NO), 
            pt.Y -= (float)Math.Abs(dx) * f;
            pt.X -= (float)Math.Abs(dy) * f;
         } else {            // nach unten geneigt (NW <-> SO), 
            pt.Y -= (float)Math.Abs(dx) * f;
            pt.X += (float)Math.Abs(dy) * f;
         }

         return pt;
      }

      bool textAreaIsSimple {
         get {
            return commonTextArea == null;
         }
      }

      bool intersectsWith(RectangleF rect, RectangleCommon? region2) {
         if (rect == Rectangle.Empty)
            return false;

         RectangleCommon rectcom = new RectangleCommon(rect.Location, rect.Width, rect.Height, 0, RectangleCommon.RotationPoint.TopLeft);
         return region2 != null ? rectcom.IsIntersect(region2) : true;
      }

      /// <summary>
      /// Vergleich auf Überlappung der Textbereiche
      /// </summary>
      /// <param name="ot"></param>
      /// <returns></returns>
      public bool IntersectsWith(ObjectText ot) {
         if (textAreaIsSimple && ot.textAreaIsSimple)
            return simpleTextArea.IntersectsWith(ot.simpleTextArea);
         else {
            if (textAreaIsSimple)
               return intersectsWith(simpleTextArea, ot.commonTextArea);
            else {
               if (ot.textAreaIsSimple)
                  return ot.intersectsWith(ot.simpleTextArea, commonTextArea);
               else 
                  return commonTextArea != null &&
                         ot.commonTextArea != null ? ot.commonTextArea.IsIntersect(commonTextArea) : false;
            }
         }
      }

      /// <summary>
      /// Vergleich für die sortierte Ausgabe
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public int CompareTo(object? obj) {
         // Liste spez. sortieren für die Reihenfolge der Ausgabe
         //    Punkte 0x01 .. 0x0b           Namen für Orte
         //    Linien 0x01 .. 0x0a           Straßen
         //    Flächen nach GarminType
         //    restliche Linien nach GarminType
         //    restliche Punkte nach GarminType
         int s1 = getSortValue(this);
         int s2 = getSortValue((ObjectText?)obj);
         if (s1 < s2)
            return -1;
         if (s1 == s2)
            return 0;
         return 1;
      }

      /// <summary>
      /// für den einfachen Vergleich mit CompareTo() eine spez. "Bewertung" erzeugen
      /// </summary>
      /// <param name="ot"></param>
      /// <returns></returns>
      int getSortValue(ObjectText? ot) {
         if (ot == null)
            return GarminType + (5 << 18);

         int group;
         if (ot.ObjType == ObjectType.Point &&
             ot.GarminType <= 0x0b00) {
            group = 1;
         } else {
            if (ot.ObjType == ObjectType.Line &&
                ot.GarminType <= 0x0a00) {
               group = 2;
            } else {
               if (ot.ObjType == ObjectType.Area) {
                  group = 3;
               } else {
                  if (ot.ObjType == ObjectType.Line)
                     group = 4;
                  else
                     group = 5;
               }
            }
         }
         return GarminType + (group << 18);
      }

      /// <summary>
      /// Ist im Text min. 1 Kleinbuchstabe enthalten?
      /// </summary>
      /// <returns></returns>
      bool hasLowerChars() {
         for (int i = 0; i < Text.Length; i++) {
            if (char.IsLower(Text[i]))
               return true;
         }
         return false;
      }

      /// <summary>
      /// ähnlich der Primitivkonvertierung von Garmin: alles in Kleinbuchstaben; der jeweils 1. Buchstabe eines Wortes groß
      /// </summary>
      void simpleGarminTextConvert() {
         Text = SimpleGarminTextConvert(Text);
      }

      /// <summary>
      /// ähnlich der Primitivkonvertierung von Garmin: alles in Kleinbuchstaben; der jeweils 1. Buchstabe eines Wortes groß
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string SimpleGarminTextConvert(string? text) {
         if (!string.IsNullOrEmpty(text)) {
            string txt = text.ToLower();
            if (txt != text) {
               // alle Anfangsbuchstaben von Wörtern wieder groß
               foreach (char sep in new char[] { ' ', '-', '"', '(' }) {
                  string[] words = txt.Split(new char[] { sep }, StringSplitOptions.RemoveEmptyEntries);
                  for (int i = 0; i < words.Length; i++)
                     if (i > 0 ||
                         char.IsLower(words[i][0]))
                        words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
                  txt = string.Join(sep.ToString(), words);
               }
            }
            return txt;
         }
         return "";
      }

      public override string ToString() {
         return ObjType == ObjectType.Line ?
                     string.Format("{0}, {1} / {2}, Angle {3}, GarminType 0x{4}: {5}", ObjType, ReferencePoint, ReferencePoint2, Angle, GarminType.ToString("X"), Text) :
                     string.Format("{0}, {1}, GarminType 0x{2}: {3}", ObjType, ReferencePoint, GarminType.ToString("X"), Text);
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
               //textArea?.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
