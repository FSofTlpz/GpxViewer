using System;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class FormGoToPos : Form {

      public double Latitude = 0;

      public double Longitude = 0;

      string maskedTextBox_Latitude_Text = "";
      string maskedTextBox_Longitude_Text = "";

      string maskedTextBox_LatitudeDec_Text = "";
      string maskedTextBox_LongitudeDec_Text = "";


      public FormGoToPos() {
         InitializeComponent();
      }

      private void FormGoToPos_Load(object sender, EventArgs e) {
         maskedTextBox_Latitude.Text = maskedTextBox_Latitude_Text = getDmsString(Latitude, true);
         maskedTextBox_Longitude.Text = maskedTextBox_Longitude_Text = getDmsString(Longitude, false);

         maskedTextBox_LatitudeDec.Text = maskedTextBox_LatitudeDec_Text = getDString(Latitude, true);
         maskedTextBox_LongitudeDec.Text = maskedTextBox_LongitudeDec_Text = getDString(Longitude, false);

         maskedTextBox_Latitude.TextChanged += MaskedTextBox_LatLon_TextChanged;
         maskedTextBox_Longitude.TextChanged += MaskedTextBox_LatLon_TextChanged;

         maskedTextBox_LatitudeDec.TextChanged += MaskedTextBox_LatLonDec_TextChanged;
         maskedTextBox_LongitudeDec.TextChanged += MaskedTextBox_LatLonDec_TextChanged;
      }

      string getDmsString(double v, bool forlat) {
         d2dms(v, out int d, out int m, out int s, out double ss, out bool negativ);
         int ssi = (int)Math.Round(1000 * ss);
         return string.Format("{0,3}°{1,2}'{2,2},{3:D3}\" {4}",
                              d,
                              m,
                              s,
                              ssi,
                              forlat ? (negativ ? "S" : "N") : (negativ ? "W" : "O"));
      }

      string getDString(double v, bool forlat) {
         bool negativ = v < 0;
         if (negativ)
            v = -v;
         return string.Format("{0,10:F6}° {1}",
                              v,
                              forlat ? (negativ ? "S" : "N") : (negativ ? "W" : "O"));
      }

      /// <summary>
      /// wandelt einen Dezimalgradwert in die Bestandteile Grad-Minute-Sekunde um
      /// </summary>
      /// <param name="v"></param>
      /// <param name="d"></param>
      /// <param name="m"></param>
      /// <param name="s"></param>
      /// <param name="ss">Sekundenbruchteile (dez.)</param>
      /// <param name="negativ"></param>
      void d2dms(double v, out int d, out int m, out int s, out double ss, out bool negativ) {
         negativ = v < 0;
         if (negativ)
            v = -v;
         v *= 3600;     // in s
         ss = v - (int)v;

         d = (int)v / 3600;
         v -= d * 3600;

         m = (int)v / 60;
         v -= m * 60;

         s = (int)v;
      }

      /// <summary>
      /// wandelt einen Wert in Grad-Minute-Sekunde in Dezimalgrad um
      /// </summary>
      /// <param name="d"></param>
      /// <param name="m"></param>
      /// <param name="s"></param>
      /// <param name="ss">Sekundenbruchteile (dez.)</param>
      /// <param name="negativ"></param>
      /// <returns></returns>
      double dms2d(int d, int m, int s, double ss, bool negativ) {
         return (negativ ? -1 : 1) * (d + m / 60.0 + (s + ss) / 3600.0);
      }

      int interpretDmsString(string txt, bool is4lat, out int d, out int m, out int s, out int ss, out bool negativ) {
         d = getInt(txt.Substring(0, 3));
         m = getInt(txt.Substring(4, 2));
         s = getInt(txt.Substring(7, 2));
         ss = getInt(txt.Substring(10, 3));
         char direction = txt.Substring(15, 1).ToUpper()[0];
         negativ = direction == 'S' || direction == 'W';

         //if (d > (is4lat ? 180 : 90) ||
         //    (d == (is4lat ? 180 : 90) && (m > 0 || s > 0 || ss > 0)) ||
         //    m >= 60 ||
         //    s >= 60 ||
         //    (is4lat && !(direction == 'N' || direction == 'S')) ||
         //    (!is4lat && !(direction == 'O' || direction == 'W')))
         //   return false;

         if (d > (is4lat ? 180 : 90) ||
             (d == (is4lat ? 180 : 90) && (m > 0 || s > 0 || ss > 0)))
            return 0;

         if (m >= 60)
            return 4;

         if (s >= 60)
            return 7;

         if ((is4lat && !(direction == 'N' || direction == 'S')) ||
             (!is4lat && !(direction == 'O' || direction == 'W')))
            return 15;

         return -1;
      }

      int interpretDString(string txt, bool is4lat, out double d, out bool negativ) {
         d = getDouble(txt.Substring(0, 10));
         char direction = txt.Substring(12, 1).ToUpper()[0];
         negativ = direction == 'S' || direction == 'W';

         if (d > (is4lat ? 180 : 90))
            return 0;

         if ((is4lat && !(direction == 'N' || direction == 'S')) ||
             (!is4lat && !(direction == 'O' || direction == 'W')))
            return 12;

         return -1;
      }

      int getInt(string txt) {
         int idx;
         while ((idx = txt.IndexOf('_')) >= 0) {   // entfernt auch "innere" '_'
            txt = txt.Remove(idx);
         }
         return txt.Length == 0 ? 0 : Convert.ToInt32(txt);
      }

      double getDouble(string txt) {
         int idx;
         while ((idx = txt.IndexOf('_')) >= 0) {   // entfernt auch "innere" '_'
            txt = txt.Remove(idx);
         }
         return txt.Length == 0 ? 0 : Convert.ToDouble(txt);
      }

      bool setMbtIntern = false;

      private void MaskedTextBox_LatLon_TextChanged(object sender, EventArgs e) {
         MaskedTextBox mtb = (sender as MaskedTextBox);
         bool is4lat = mtb.Equals(maskedTextBox_Latitude);
         string txt = mtb.Text;     // Hier ex. mit Sicherheit ein der Maske entsprechender Text.
         int pos = interpretDmsString(txt, is4lat, out int d, out int m, out int s, out int ss, out bool negativ);
         if (pos >= 0) {   // Fehler
            mtb.Text = is4lat ? maskedTextBox_Latitude_Text : maskedTextBox_Longitude_Text;
            mtb.Select(pos, 0);
         } else {
            double v = dms2d(d, m, s, ss / 1000.0, negativ);
            if (is4lat) {
               maskedTextBox_Latitude_Text = txt;
               if (!setMbtIntern) {
                  Latitude = v;
                  setMbtIntern = true;
                  maskedTextBox_LatitudeDec.Text = maskedTextBox_LatitudeDec_Text = getDString(Latitude, true);
               }
            } else {
               maskedTextBox_Longitude_Text = txt;
               if (!setMbtIntern) {
                  Longitude = v;
                  setMbtIntern = true;
                  maskedTextBox_LongitudeDec.Text = maskedTextBox_LongitudeDec_Text = getDString(Longitude, false);
               }
            }
         }
         setOkStatus();
         setMbtIntern = false;
      }

      private void MaskedTextBox_LatLonDec_TextChanged(object sender, EventArgs e) {
         MaskedTextBox mtb = (sender as MaskedTextBox);
         bool is4lat = mtb.Equals(maskedTextBox_LatitudeDec);
         string txt = mtb.Text;     // Hier ex. mit Sicherheit ein der Maske entsprechender Text.

         int pos = interpretDString(txt, is4lat, out double v, out bool negativ);
         if (pos >= 0) {
            mtb.Text = is4lat ? maskedTextBox_LatitudeDec_Text : maskedTextBox_LongitudeDec_Text;
            mtb.Select(pos, 0);
         } else {
            if (negativ)
               v = -v;
            if (is4lat) {
               maskedTextBox_LatitudeDec_Text = txt;
               if (!setMbtIntern) {
                  Latitude = v;
                  setMbtIntern = true;
                  maskedTextBox_Latitude.Text = maskedTextBox_Latitude_Text = getDmsString(Latitude, true);
               }
            } else {
               maskedTextBox_LongitudeDec_Text = txt;
               if (!setMbtIntern) {
                  Longitude = v;
                  setMbtIntern = true;
                  maskedTextBox_Longitude.Text = maskedTextBox_Longitude_Text = getDmsString(Longitude, false);
               }
            }
         }
         setOkStatus();
         setMbtIntern = false;
      }

      void setOkStatus() {

         //button_OK.Enabled = false;
      }

   }
}
