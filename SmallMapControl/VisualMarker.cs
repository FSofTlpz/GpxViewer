using System.Collections.Generic;
using System.Drawing;
using FSofTUtils.Geography.Garmin;
#if GMAP4SKIA
using GMap.NET.Skia.Markers;
#else
using GMap.NET.CoreExt.WindowsForms.ToolTips;
using GMap.NET.WindowsForms.Markers;
#endif

namespace SmallMapControl {

   /// <summary>
   /// Erweiterung der <see cref="GMap.NET.WindowsForms.GMapRoute"/> um Gpx-Daten und grafische Daten
   /// </summary>
//   public class VisualMarker : GMap.NET.WindowsForms.Markers.GMarkerGoogle {
   public class VisualMarker : GMarkerGoogle {

      /// <summary>
      /// Zuordnung von Bitmap und Bitmap-Offset für die geograf. Pos. für einen Marker
      /// </summary>
      public class MarkerPicture {
         private int v1;
         private int v2;
         private byte[] foto;

         public Bitmap Picture { get; private set; }

         public Point Offset { get; private set; }

         public MarkerPicture(int offsetx, int offsety, Bitmap bm) {
            Picture = bm;
            Offset = new Point(offsetx, offsety);
         }

         public MarkerPicture(int v1, int v2, byte[] foto) {
            this.v1 = v1;
            this.v2 = v2;
            this.foto = foto;
         }
      }


#if GMAP4SKIA
      public readonly static MarkerPicture FotoMarker = new MarkerPicture(-12, -12, Skia.Properties.Resources.Foto);
      public readonly static MarkerPicture FlagBlueMarker = new MarkerPicture(-7, -22, Skia.Properties.Resources.FlagBlue);
      public readonly static MarkerPicture FlagGreenMarker = new MarkerPicture(-7, -22, Skia.Properties.Resources.FlagGreen);
      public readonly static MarkerPicture PointMarker = new MarkerPicture(-2, -2, Skia.Properties.Resources.Point1);
#else
      public readonly static MarkerPicture FotoMarker = new MarkerPicture(-12, -12, Properties.Resources.Foto);
      public readonly static MarkerPicture FlagBlueMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagBlue);
      //readonly static MarkerPicture FlagRedMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagRed);
      public readonly static MarkerPicture FlagGreenMarker = new MarkerPicture(-7, -22, Properties.Resources.FlagGreen);
      //readonly static MarkerPicture PinBlueMarker = new MarkerPicture(0, -24, Properties.Resources.PinBlue);
      //readonly static MarkerPicture PinRedMarker = new MarkerPicture(0, -24, Properties.Resources.PinRed);
      //readonly static MarkerPicture PinGreenMarker = new MarkerPicture(0, -24, Properties.Resources.PinGreen);
      public readonly static MarkerPicture PointMarker = new MarkerPicture(-2, -2, Properties.Resources.Point1);
      public readonly static MarkerPicture GeoTaggingMarker = new MarkerPicture(-12, -12, Properties.Resources.GeoTagging);
#endif



      public enum VisualStyle {
         /// <summary>
         /// noch nicht festgelegt
         /// </summary>
         notdefined,
         /// <summary>
         /// für Foto
         /// </summary>
         FotoMarker,
         /// <summary>
         /// Standard
         /// </summary>
         StandardMarker,
         /// <summary>
         /// Standard, editierbar
         /// </summary>
         StandardEditableMarker,
         /// <summary>
         /// Punkt
         /// </summary>
         PointMarker,
         /// <summary>
         /// für GeoTagging
         /// </summary>
         GeoTagging,
      }


      static Dictionary<string, MarkerPicture> ExternSymbols = null;

      /// <summary>
      /// Gpx-Daten
      /// </summary>
      public Marker RealMarker { get; private set; } = null;

      /// <summary>
      /// akt. Darstellung
      /// </summary>
      public VisualStyle Visualstyle { get; private set; } = VisualStyle.notdefined;


      static MarkerPicture markerPicture4Style(VisualStyle style, string symbolname = null) {
         MarkerPicture mp;
         switch (style) {
            case VisualStyle.StandardMarker:
               if (!string.IsNullOrEmpty(symbolname) &&
                   ExternSymbols != null &&
                   ExternSymbols.TryGetValue(symbolname, out mp))
                  return mp;
               return FlagBlueMarker;

            case VisualStyle.StandardEditableMarker:
               if (!string.IsNullOrEmpty(symbolname) &&
                   ExternSymbols != null &&
                   ExternSymbols.TryGetValue(symbolname, out mp))
                  return mp;
               return FlagGreenMarker;

            case VisualStyle.PointMarker: return PointMarker;
            case VisualStyle.FotoMarker: return FotoMarker;
            case VisualStyle.GeoTagging: return GeoTaggingMarker;
         }
         return null; ;
      }

      /// <summary>
      /// liefert das Bitmap für diesen Stil (i.A. nur für symbolname sinnvoll)
      /// </summary>
      /// <param name="style"></param>
      /// <param name="symbolname"></param>
      /// <returns></returns>
      public static Bitmap Bitmap4Style(VisualStyle style, string symbolname = null) {
         MarkerPicture mp = markerPicture4Style(style, symbolname);
         if (mp != null)
            return mp.Picture;
         return null; ;
      }



      /// <summary>
      /// erzeugt den <see cref="VisualTrack"/> zum <see cref="Track"/> 
      /// </summary>
      /// <param name="exttrack"></param>
      /// <param name="name"></param>
      /// <param name="style"></param>
      /// <param name="mapControl"></param>
      public VisualMarker(Marker marker,
                          string name,
                          VisualStyle style,
                          string symbolname) :
      base(new GMap.NET.PointLatLng(marker.Waypoint.Lat, marker.Waypoint.Lon),
           markerPicture4Style(style, symbolname).Picture) {
         ToolTipText = name;
         RealMarker = marker;
         IsHitTestVisible = true;
         IsVisible = true;
         Offset = markerPicture4Style(style, symbolname).Offset;
         Visualstyle = style;
#if !GMAP4SKIA
         if (style != VisualStyle.FotoMarker) {
            ToolTip = new GMapMarkerToolTip(this);
            ToolTipText = RealMarker.Text;
            //ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.OnMouseOver;
            ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.Always;
         }
#endif
      }

      /// <summary>
      /// (Pseudo-)Refresh der Anzeige
      /// </summary>
      public void Refresh() {
         if (IsVisible) {
            IsVisible = false;
            IsVisible = true;
         }
      }

      public static void RegisterExternSymbols(IList<GarminSymbol> symbols) {
         ExternSymbols = new Dictionary<string, MarkerPicture>();
         foreach (var item in symbols) {
            ExternSymbols.Add(item.Name, new MarkerPicture(-item.Bitmap.Width / 2, -item.Bitmap.Height / 2, item.Bitmap));
         }
      }

      public override string ToString() {
         return string.Format("Visualstyle={0}, GpxMarkerData=[{2}]", Visualstyle, RealMarker);
      }

   }
}
