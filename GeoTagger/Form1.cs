using ExifLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GeoTagger {
   public partial class Form1 : Form {

      class PictureData {

         public readonly string FileName;
         public readonly Image Thumbnail;
         public readonly DateTime DateTimeFile;

         public readonly DateTime DateTime;
         public readonly DateTime DateTimeOriginal;
         public readonly double Latitude;
         public readonly double Longitude;
         public readonly double Direction;

         public PictureData(string filename, Size thumbnailsize) {
            FileName = filename;
            ExifGeo exifGeo = new ExifGeo(FileName);
            DateTime = exifGeo.GetDateTime();
            DateTimeOriginal = exifGeo.GetDateTimeOriginal();
            Direction = exifGeo.GetDirection();
            exifGeo.GetLatLon(out Latitude, out Longitude);

            Stream stream = new FileStream(filename, FileMode.Open);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();         // kein Zugriff mehr auf die Datei

            Image img = Image.FromStream(new MemoryStream(buffer));
            Thumbnail = new Bitmap(thumbnailsize.Width, thumbnailsize.Height);
            using (Graphics g = Graphics.FromImage(Thumbnail)) {
               g.Clear(Color.Transparent);
               float x = 0, y = 0, w = thumbnailsize.Width, h = thumbnailsize.Height;
               if (img.Width * Thumbnail.Height < img.Height * Thumbnail.Width) {  // Original ist zu schmal
                  w = (float)(img.Width * Thumbnail.Height) / img.Height;
                  x = (thumbnailsize.Width - w) / 2;
               } else {
                  h = (float)(img.Height * Thumbnail.Width) / img.Width;
                  y = (thumbnailsize.Height - h) / 2;
               }
               //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
               //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
               //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
               g.DrawImage(img, x, y, w, h);
            }
            img.Dispose();
         }

         public override string ToString() {
            return string.Format("{0}, {1}, lat={2}°, lon={3}°, direction={4}°",
                                 FileName,
                                 DateTime.ToString("G"),
                                 Latitude,
                                 Longitude,
                                 Direction);
         }
      }



      public Form1() {
         InitializeComponent();
      }

      private void Form1_Load(object sender, EventArgs e) {
         // x();
         y();

      }

      void x() {
         ExifGeo exifGeo = new ExifGeo(@"p:\Programmierung\!Privat\aktuell\GpxViewer\Sonstiges\Gpx\2020_02 Stralsund\20200215_152813.jpg");
         exifGeo.GetLatLon(out double lat, out double lon);
         double dir = exifGeo.GetDirection();
         DateTime dt = exifGeo.GetDateTime();
         DateTime dt2 = exifGeo.GetDateTimeOriginal();

      }

      void y() {

         ListView lv = listView1;

         lv.Items.Clear();
         
         lv.LargeImageList = new ImageList();
         lv.LargeImageList.ImageSize = new Size(100, 100);

         lv.SmallImageList = new ImageList();
         lv.SmallImageList.ImageSize = new Size(20, 20);

         lv.Columns.Clear();
         lv.Columns.Add("C1");
         lv.Columns.Add("C2");


         PictureData pd1 = new PictureData(@"p:\Programmierung\!Privat\aktuell\GpxViewer\Sonstiges\Gpx\2020_02 Stralsund\20200215_125438.jpg", lv.LargeImageList.ImageSize);
         PictureData pd2 = new PictureData(@"p:\Programmierung\!Privat\aktuell\GpxViewer\Sonstiges\Gpx\2020_02 Stralsund\20200215_152813.jpg", lv.LargeImageList.ImageSize);
         PictureData pd3 = new PictureData(@"p:\Programmierung\!Privat\aktuell\GpxViewer\Sonstiges\Gpx\2020_02 Stralsund\20200215_160948.jpg", lv.LargeImageList.ImageSize);

         ListViewItem lvi;
         string text;
         
         lv.LargeImageList.Images.Add(pd1.Thumbnail);
         text = Path.GetFileName(pd1.FileName);
         lvi = new ListViewItem(new string[] { text, "c", "d" }, lv.LargeImageList.Images.Count - 1) {
            Tag = pd1,
            ToolTipText = text,
         };
         lv.Items.Add(lvi);

         lv.LargeImageList.Images.Add(pd2.Thumbnail);
         text = Path.GetFileName(pd2.FileName);
         lvi = new ListViewItem(text, lv.LargeImageList.Images.Count - 1) {
            Tag = pd2,
            ToolTipText = text,
         };
         lv.Items.Add(lvi);

         lv.LargeImageList.Images.Add(pd3.Thumbnail);
         text = Path.GetFileName(pd3.FileName);
         lvi = new ListViewItem(text, lv.LargeImageList.Images.Count - 1) {
            Tag = pd3,
            ToolTipText = text,
         };
         lvi.SubItems.Add("d");
         lvi.SubItems.Add("e");
         lvi.SubItems.Add("f");
         lv.Items.Add(lvi);




      }





      /// <summary>
      /// liefert den Itemindex oder -1 an dieser Pos.
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      public int GetItemIdx4Point(Point pt) {
         ListViewHitTestInfo hit = listView1.HitTest(pt);
         if (hit != null && hit.Item != null)
            return hit.Item.Index;
         return -1;
      }

      private void button1_Click(object sender, EventArgs e) {
         listView1.View = listView1.View == View.LargeIcon ?
                              View.Details :
                              View.LargeIcon;
      }
   }



}
