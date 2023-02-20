using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class PictureManager : UserControl {

      class PictureData {

         const int SUBIDX_PICTURE = 0;
         const int SUBIDX_FILENAME = 1;
         const int SUBIDX_FILEDATE = 2;
         const int SUBIDX_GEODATE = 3;
         const int SUBIDX_GEOCOORD = 4;
         const int SUBIDX_GEODIRECT = 5;

         public string FileName { get; protected set; }
         public Image Thumbnail { get; protected set; }
         public DateTime DateTimeFile { get; protected set; }

         public DateTime DateTime { get; protected set; }
         public DateTime DateTimeOriginal { get; protected set; }
         public double Latitude { get; protected set; }
         public double Longitude { get; protected set; }
         public double Direction { get; protected set; }

         public bool Changed { get; protected set; }

         public bool IsInit { get; protected set; }

         public bool HasGeoData {
            get => !(double.IsNaN(Longitude) || double.IsNaN(Latitude));
         }

         public ListViewItem ListViewItem;

         /// <summary>
         /// unter diesem Namen (noch) gespeichert
         /// </summary>
         string orgFilename = "";


         public PictureData(string filename) {
            IsInit = false;
            Changed = false;
            orgFilename = FileName = filename;

            DateTimeFile = File.GetLastWriteTime(orgFilename);
            getActualGeoData();

            string text = Path.GetFileName(orgFilename);
            ListViewItem = new ListViewItem() {
               Tag = this,
               ToolTipText = text,
               //Text = text,
               ImageIndex = 0,
            };
            setSubItemsText();
         }

         public PictureData(string filename, Size thumbnailsize) : this(filename) {
            InitData(thumbnailsize);
         }

         public void InitData(Size thumbnailsize) {
            //DateTimeFile = File.GetLastWriteTime(FileName);
            //getActualGeoData();
            using (Image img = ReadImage(orgFilename)) {
               if (img != null) {
                  Thumbnail = createPicture(img, thumbnailsize.Width, thumbnailsize.Height);
                  IsInit = true;
               }
            }
         }

         public void ChangeLonLat(double longitude, double latitude) {
            Changed = true;
            Longitude = longitude;
            Latitude = latitude;
            setSubItemsText();
         }

         public void ChangeFilename(string poorfilename) {
            Changed = true;
            FileName = Path.Combine(Path.GetDirectoryName(orgFilename), poorfilename);
            setSubItemsText();
         }

         public bool Save(bool preserveDateTimeFile = false) {
            if (Changed) {
               DateTime orgDateTime = File.GetLastWriteTime(orgFilename);
               ExifGeo exifGeo = new ExifGeo(orgFilename);

               if (exifGeo.SetLatLon(Latitude, Longitude, true)) {
                  exifGeo.SaveImage(FileName);
                  getActualGeoData();
                  if (preserveDateTimeFile)
                     File.SetLastWriteTime(FileName, orgDateTime);
                  else
                     DateTimeFile = File.GetLastWriteTime(FileName);
                  Changed = false;
                  orgFilename = FileName;
                  return true;
               }
            }
            return false;
         }

         public Image ReadImage() {
            return ReadImage(orgFilename);
         }

         void getActualGeoData() {
            ExifGeo exifGeo = new ExifGeo(orgFilename);
            DateTime = exifGeo.GetDateTime();
            DateTimeOriginal = exifGeo.GetDateTimeOriginal();
            Direction = exifGeo.GetDirection();
            Latitude = exifGeo.GetLat();
            Longitude = exifGeo.GetLon();
         }

         /// <summary>
         /// setzt die Texte der Subitems entsprechend den akt. Daten
         /// </summary>
         void setSubItemsText() {
            const string textprefix = "  ";
            const string textpostfix = "   ";    // need for AutoResizeColumns()

            while (ListViewItem.SubItems.Count < 6)
               ListViewItem.SubItems.Add("");
            ListViewItem.SubItems[SUBIDX_PICTURE].Text = "";
            ListViewItem.SubItems[SUBIDX_FILENAME].Text = textprefix + Path.GetFileName(FileName) + textpostfix;
            ListViewItem.SubItems[SUBIDX_FILEDATE].Text = textprefix + DateTimeFile.ToString("G") + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEODATE].Text = textprefix + DateTimeOriginal.ToString("G") + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEOCOORD].Text = double.IsNaN(Longitude) || double.IsNaN(Latitude) ? textprefix : string.Format("{0:N6}° {1:N6}°", Longitude, Latitude) + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEODIRECT].Text = double.IsNaN(Direction) ? "" : textprefix + string.Format("{0:N0}°", Direction) + textpostfix;
         }

         static Bitmap createPicture(Image img, int width, int height) {
            Bitmap bm = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bm)) {
               g.Clear(Color.Transparent);
               float x = 0, y = 0, w = width, h = height;
               if (img.Width * height < img.Height * width) {  // Original ist zu schmal
                  w = (float)(img.Width * height) / img.Height;
                  x = (width - w) / 2;
               } else {
                  h = (float)(img.Height * width) / img.Width;
                  y = (height - h) / 2;
               }
               //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
               //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
               //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
               g.DrawImage(img, x, y, w, h);
            }
            return bm;
         }

         public static Image ReadImage(string filename) {
            byte[] buffer = null;
            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) { // nur zum lesen öffnen mit read-sharing für andere
               buffer = new byte[stream.Length];
               stream.Read(buffer, 0, buffer.Length);
               stream.Close();         // kein Zugriff mehr auf die Datei
            }
            if (buffer != null) {
               using (MemoryStream memstream = new MemoryStream(buffer)) {
                  return Image.FromStream(memstream);
               }
            }
            return null;
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

      enum PictureFilter {
         None,
         WithGeoData,
         WithoutGeoData,
      }

      enum PictureSort {
         None,
         FilenameAscending,
         FilenameDescending,
         FiledateAscending,
         FiledateDescending,
         GeodateAscending,
         GeodateDescending,
      }

      #region events

      public class PictureDataListEventArgs : EventArgs {

         public class PictureData {

            public readonly string Filename;

            public readonly double Latitude;

            public readonly double Longitude;

            public PictureData(string filename, double lon, double lat) {
               Filename = filename;
               Longitude = lon;
               Latitude = lat;
            }

            public override string ToString() {
               return string.Format("{0:N6}° {1:N6}° {2}", Longitude, Latitude, Filename);
            }
         }

         public readonly PictureData[] PictureDatas;


         public PictureDataListEventArgs(IList<PictureData> pd) {
            PictureDatas = new PictureData[pd.Count];
            pd.CopyTo(PictureDatas, 0);
         }

      }

      public class PictureDataEventArgs : EventArgs {

         public readonly string Filename;

         public readonly double Latitude;

         public readonly double Longitude;


         public PictureDataEventArgs(string filename, double lon, double lat) {
            Filename = filename;
            Longitude = lon;
            Latitude = lat;
         }

         public override string ToString() {
            return string.Format("{0:N6}° {1:N6}° {2}", Longitude, Latitude, Filename);
         }

      }

      /// <summary>
      /// (Bild-)Dateien wurden ausgewählt.
      /// </summary>
      public event EventHandler<PictureDataListEventArgs> OnSelectPictures;

      /// <summary>
      /// Die Auswahl von (Bild-)Dateien wurde beendet.
      /// </summary>
      public event EventHandler<PictureDataListEventArgs> OnDeselectPictures;

      /// <summary>
      /// Eine (Bild-)Datei sollte extern angezeigt werden.
      /// </summary>
      public event EventHandler<PictureDataEventArgs> OnShowExtern;

      /// <summary>
      /// Für die (Bild-)Datei werden neue Geodaten gewünscht.
      /// </summary>
      public event EventHandler<PictureDataEventArgs> OnNeedNewData;

      /// <summary>
      /// Der Filter wurde geändert.
      /// </summary>
      public event EventHandler<EventArgs> OnNewFiltered;

      /// <summary>
      /// Die Sortierung wurde geändert.
      /// </summary>
      public event EventHandler<EventArgs> OnNewSorted;

      #endregion

      #region Comparer for ListView

      private class ListViewBaseComparer : System.Collections.IComparer {

         public Size ImgSize = Size.Empty;

         public ListViewBaseComparer(Size imgsize) {
            ImgSize = imgsize;
         }

         public int Compare(object x, object y) {
            PictureData p1 = (x != null && (x as ListViewItem).Tag != null) ? (x as ListViewItem).Tag as PictureData : null;
            PictureData p2 = (y != null && (y as ListViewItem).Tag != null) ? (y as ListViewItem).Tag as PictureData : null;
            if (p1 == null && p2 == null)
               return 0;
            if (p2 == null)
               return 1;
            if (p1 == null)
               return -1;
            //if (!p1.IsInit && ImgSize != Size.Empty)
            //   p1.InitData(ImgSize);
            //if (!p2.IsInit && ImgSize != Size.Empty)
            //   p2.InitData(ImgSize);

            return CompareSpec(p1, p2);
         }

         protected virtual int CompareSpec(PictureData p1, PictureData p2) {
            return 0;
         }

         protected int compareDateTime(DateTime dt1, DateTime dt2) {
            if (dt1 == DateTime.MinValue && dt2 == DateTime.MinValue)
               return 0;
            if (dt2 == DateTime.MinValue)
               return 1;
            if (dt1 == DateTime.MinValue)
               return -1;

            return dt1.Ticks > dt2.Ticks ? 1 :
                   dt1.Ticks == dt2.Ticks ? 0 :
                     -1;
         }
      }

      private class ListViewFilenameAscendComparer : ListViewBaseComparer {

         public ListViewFilenameAscendComparer() : base(Size.Empty) { }

         protected override int CompareSpec(PictureData p1, PictureData p2) => string.Compare(p1.FileName, p2.FileName);
      }

      private class ListViewFilenameDescendComparer : ListViewBaseComparer {

         public ListViewFilenameDescendComparer() : base(Size.Empty) { }

         protected override int CompareSpec(PictureData p1, PictureData p2) => -string.Compare(p1.FileName, p2.FileName);
      }

      private class ListViewFiledateAscendComparer : ListViewBaseComparer {

         public ListViewFiledateAscendComparer(Size imgsize) : base(imgsize) { }

         protected override int CompareSpec(PictureData p1, PictureData p2) => compareDateTime(p1.DateTimeFile, p2.DateTimeFile);
      }

      private class ListViewFiledateDescendComparer : ListViewBaseComparer {

         public ListViewFiledateDescendComparer(Size imgsize) : base(imgsize) { }

         protected override int CompareSpec(PictureData p1, PictureData p2) => -compareDateTime(p1.DateTimeFile, p2.DateTimeFile);
      }

      private class ListViewGeodateAscendComparer : ListViewBaseComparer {

         public ListViewGeodateAscendComparer(Size imgsize) : base(imgsize) { }

         protected override int CompareSpec(PictureData p1, PictureData p2) => compareDateTime(p1.DateTime, p2.DateTime);
      }

      private class ListViewGeodateDescendComparer : ListViewBaseComparer {

         public ListViewGeodateDescendComparer(Size imgsize) : base(imgsize) { }

         protected override int CompareSpec(PictureData p1, PictureData p2) => -compareDateTime(p1.DateTime, p2.DateTime);
      }

      #endregion

      /// <summary>
      /// Anzahl noch nicht gespeicherter Bilder
      /// </summary>
      public int UnsavedPictures {
         get {
            int count = 0;
            foreach (PictureData pd in dataCacheAll)
               if (pd.Changed)
                  count++;
            return count;
         }
      }

      /// <summary>
      /// Anzahl der Bilder in der Ansicht
      /// </summary>
      public int ShownPictures {
         get => dataCache4Showing.Count;
      }


      List<PictureData> dataCacheAll = new List<PictureData>();

      List<PictureData> dataCache4Showing = new List<PictureData>();

      readonly Size ThumbnailSize = new Size(120, 80);

      PictureFilter _pictureFilter = PictureFilter.None;

      PictureFilter pictureFilter {
         get => _pictureFilter;
         set {
            if (value != _pictureFilter) {
               Cursor cursor = Cursor;
               Cursor = Cursors.WaitCursor;
               _pictureFilter = value;
               fillListView(listView1, value, pictureSort);
               Cursor = cursor;
               OnNewFiltered?.Invoke(this, EventArgs.Empty);
            }

            ToolStripMenuItem_ViewAll.Checked =
            ToolStripMenuItem_ViewWithGeo.Checked =
            ToolStripMenuItem_ViewWithoutGeo.Checked = false;
            switch (_pictureFilter) {
               case PictureFilter.None:
                  ToolStripMenuItem_ViewAll.Checked = true;
                  break;

               case PictureFilter.WithGeoData:
                  ToolStripMenuItem_ViewWithGeo.Checked = true;
                  break;

               case PictureFilter.WithoutGeoData:
                  ToolStripMenuItem_ViewWithoutGeo.Checked = true;
                  break;
            }
         }
      }

      PictureSort _pictureSort = PictureSort.None;

      PictureSort pictureSort {
         get => _pictureSort;
         set {
            if (value != _pictureSort) {
               Cursor cursor = Cursor;
               Cursor = Cursors.WaitCursor;
               _pictureSort = value;
               switch (_pictureSort) {
                  case PictureSort.FilenameAscending:
                     listView1.ListViewItemSorter = new ListViewFilenameAscendComparer();
                     break;

                  case PictureSort.FilenameDescending:
                     listView1.ListViewItemSorter = new ListViewFilenameDescendComparer();
                     break;

                  case PictureSort.FiledateAscending:
                     listView1.ListViewItemSorter = new ListViewFiledateAscendComparer(ThumbnailSize);
                     break;

                  case PictureSort.FiledateDescending:
                     listView1.ListViewItemSorter = new ListViewFiledateDescendComparer(ThumbnailSize);
                     break;

                  case PictureSort.GeodateAscending:
                     listView1.ListViewItemSorter = new ListViewGeodateAscendComparer(ThumbnailSize);
                     break;

                  case PictureSort.GeodateDescending:
                     listView1.ListViewItemSorter = new ListViewGeodateDescendComparer(ThumbnailSize);
                     break;
               }
               Cursor = cursor;
               OnNewSorted?.Invoke(this, EventArgs.Empty);
            }

            ToolStripMenuItem_FilenameAsc.Checked =
            ToolStripMenuItem_FilenameDesc.Checked =
            ToolStripMenuItem_FiledateAsc.Checked =
            ToolStripMenuItem_FiledateDesc.Checked =
            ToolStripMenuItem_GeodateAsc.Checked =
            ToolStripMenuItem_GeodateDesc.Checked = false;
            switch (_pictureSort) {
               case PictureSort.FilenameAscending:
                  ToolStripMenuItem_FilenameAsc.Checked = true;
                  break;

               case PictureSort.FilenameDescending:
                  ToolStripMenuItem_FilenameDesc.Checked = true;
                  break;

               case PictureSort.FiledateAscending:
                  ToolStripMenuItem_FiledateAsc.Checked = true;
                  break;

               case PictureSort.FiledateDescending:
                  ToolStripMenuItem_FiledateDesc.Checked = true;
                  break;

               case PictureSort.GeodateAscending:
                  ToolStripMenuItem_GeodateAsc.Checked = true;
                  break;

               case PictureSort.GeodateDescending:
                  ToolStripMenuItem_GeodateDesc.Checked = true;
                  break;
            }
         }
      }

      string actualPicturePath = "";



      public PictureManager() {
         InitializeComponent();
      }

      private void PictureManager_Load(object sender, EventArgs e) {
         listView1.OwnerDraw = true;
         listView1.DrawItem += ListView1_DrawItem;
         listView1.DrawSubItem += ListView1_DrawSubItem;
         listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
         listView1.View = View.Tile;
         listView1.TileSize = new Size(TextRenderer.MeasureText("=-999,999999° -99,999999° [999°]=", listView1.Font).Width,   // längster möglicherText
                                       2 +
                                       2 * listView1.Font.Height +
                                       2 +
                                       ThumbnailSize.Height + 6 +
                                       2 +
                                       2 * listView1.Font.Height +
                                       2);

         pictureFilter = PictureFilter.None;
         pictureSort = PictureSort.FilenameAscending;

         toolStripButton_SaveGpx.Enabled = false;
         toolStripButton_SaveAll.Enabled = false;

         actualPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

         //newLoad(@"..\..\..\..\Gpx\2020_02 Stralsund", false);
      }

      void newLoad(string path, bool recursesubdirectories) {
         actualPicturePath = Path.GetFullPath(path);
         if (!string.IsNullOrEmpty(actualPicturePath)) {
            fillDataCache(actualPicturePath, recursesubdirectories);
            fillListView(listView1, pictureFilter, pictureSort);
            toolStripButton_SaveAll.Enabled = false;
         }
      }

      /// <summary>
      /// fill <see cref="dataCacheAll"/> with filedata from path
      /// </summary>
      /// <param name="path"></param>
      /// <param name="recursesubdirectories"></param>
      void fillDataCache(string path, bool recursesubdirectories) {
         Cursor cursor = Cursor;
         Cursor = Cursors.WaitCursor;

         List<string> pictfiles = new List<string>();

         pictfiles.AddRange(Directory.GetFiles(path, "*?.png", recursesubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
         pictfiles.AddRange(Directory.GetFiles(path, "*?.jpg", recursesubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
         pictfiles.AddRange(Directory.GetFiles(path, "*?.jpeg", recursesubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
         pictfiles.Sort((string name1, string name2) => { return string.Compare(name1, name2, true); });
         // "*.jpg" liefert auch "*.jpg_orig", deshalb:
         for (int i = pictfiles.Count - 1; i >= 0; i--) {
            string ext = Path.GetExtension(pictfiles[i]).ToLower();
            if (!(ext == ".png" ||
                  ext == ".jpg" ||
                  ext == ".jpeg"))
               pictfiles.RemoveAt(i);
         }
         fillDataCache(pictfiles);
         Cursor = cursor;
      }

      /// <summary>
      /// fill <see cref="dataCacheAll"/> with filedata for files
      /// </summary>
      /// <param name="files"></param>
      void fillDataCache(IList<string> files) {
         Cursor orgCursor = Cursor;
         Cursor = Cursors.WaitCursor;

         dataCacheAll.Clear();
         foreach (string file in files) {
            PictureData pd = new PictureData(file);
            dataCacheAll.Add(pd);
         }

         Cursor = orgCursor;
      }

      /// <summary>
      /// fill <see cref="listView1"/> for given <see cref="PictureFilter"/> and <see cref="PictureSort"/>
      /// </summary>
      /// <param name="lv"></param>
      /// <param name="pictureFilter"></param>
      /// <param name="pictureSorting"></param>
      void fillListView(ListView lv, PictureFilter pictureFilter, PictureSort pictureSorting) {
         switch (pictureFilter) {
            case PictureFilter.None:
               dataCache4Showing = dataCacheAll;
               break;

            case PictureFilter.WithGeoData:
               dataCache4Showing = new List<PictureData>();
               foreach (PictureData pd in dataCacheAll)
                  if (pd.HasGeoData)
                     dataCache4Showing.Add(pd);
               break;

            case PictureFilter.WithoutGeoData:
               dataCache4Showing = new List<PictureData>();
               foreach (PictureData pd in dataCacheAll)
                  if (!pd.HasGeoData)
                     dataCache4Showing.Add(pd);
               break;
         }
         pictureSort = pictureSorting;

         lv.SuspendLayout();
         lv.BeginUpdate();
         lv.Items.Clear();

         foreach (PictureData pd in dataCache4Showing)
            lv.Items.Add(pd.ListViewItem);

         lv.ResumeLayout();
         lv.EndUpdate();

         if (lv.Items.Count > 0)
            lv.Items[0].Selected = true;

         toolStripStatusLabel_Path.Text = actualPicturePath;
         showPictureCount();
      }

      #region ownerdraw listview

      private void ListView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
         using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.LightGray, Color.DarkGray, LinearGradientMode.Vertical)) {
            Rectangle r = e.Bounds;
            r.Width -= 2;
            r.Height -= 2;
            e.Graphics.FillRectangle(brush, r);
         }
         e.DrawText(TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }

      private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e) {
         ListView lv = sender as ListView;
         PictureData pd = e.Item.Tag != null ?
                              e.Item.Tag as PictureData :
                              null;
         if (pd != null &&
             !pd.IsInit) {
            try {
               pd.InitData(ThumbnailSize);
               pd.ListViewItem.ToolTipText = Path.GetFileName(pd.FileName);
            } catch (Exception ex) {
               MessageBox.Show("Fehler beim Lesen der Bilddatei '" + pd.FileName + "': " + ex.Message, "FEHLER", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }

         if (lv.View == View.Tile) {
            // background
            if (e.Item.Selected) {
               using (SolidBrush brush = new SolidBrush(System.Drawing.SystemColors.Highlight))
                  e.Graphics.FillRectangle(brush, e.Bounds);
               if (e.Item.Focused)
                  e.DrawFocusRectangle();
            } else {
               if (pd == null || !pd.Changed)
                  using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.LightGray, Color.DarkGray, LinearGradientMode.Vertical)) {
                     e.Graphics.FillRectangle(brush, e.Bounds);
                  }
               else
                  using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.LightSalmon, Color.Red, LinearGradientMode.Vertical)) {
                     e.Graphics.FillRectangle(brush, e.Bounds);
                  }
            }

            // content
            drawTileContent(e.Graphics,
                            pd,
                            e.Bounds,
                            e.Item.Font);

         } else {
            //if (e.Item.Focused)
            //   e.DrawFocusRectangle();
         }
      }

      /// <summary>
      /// nur bei <see cref="View.Tile"/>
      /// </summary>
      /// <param name="g"></param>
      /// <param name="pd"></param>
      /// <param name="bounds"></param>
      /// <param name="font"></param>
      void drawTileContent(Graphics g, PictureData pd, Rectangle bounds, Font font) {
         if (pd != null) {
            int y = bounds.Location.Y;
            int xCenter = bounds.Location.X + bounds.Width / 2;

            StringFormat sfCenter = new StringFormat(StringFormatFlags.NoWrap) {
               Alignment = StringAlignment.Center,
               LineAlignment = StringAlignment.Near,
               Trimming = StringTrimming.EllipsisCharacter,
            };

            g.Clip = new Region(bounds);

            y += 2;

            using (Font font1 = new Font(font, FontStyle.Bold)) {
               g.DrawString(Path.GetFileName(pd.FileName),
                            font1,
                            Brushes.Black,
                            new RectangleF(bounds.Left, y, bounds.Width, bounds.Height),
                            sfCenter);
            }
            y += font.Height;

            g.DrawString(pd.DateTimeFile.ToString("G"), font, Brushes.Blue, xCenter, y, sfCenter);
            y += font.Height;

            if (pd.Thumbnail != null) {
               y += 3;

               int left = xCenter - pd.Thumbnail.Width / 2;
               int top = y;
               int height = pd.Thumbnail.Height;
               int width = pd.Thumbnail.Width;

               Brush content = SystemBrushes.ControlLight;        // Brushes.White;
               Brush ligthedge = SystemBrushes.ControlLightLight; // Brushes.LightGray;
               Brush darkedge = SystemBrushes.ControlDark;        // Brushes.DarkGray;

               g.FillRectangle(darkedge, left - 3, top - 3, width + 6, height + 6);
               g.FillRectangle(ligthedge, left, top, width + 3, height + 3);
               g.FillPolygon(ligthedge,
                             new Point[] {
                                new Point(left - 3, top + height + 3),
                                new Point(left, top + height + 3),
                                new Point(left, top + height),
                             });
               g.FillPolygon(ligthedge,
                             new Point[] {
                                new Point(left + width, top),
                                new Point(left + width + 3, top),
                                new Point(left + width + 3, top - 3),
                             });
               g.FillRectangle(content, left, top, width, height);

               g.DrawImageUnscaled(pd.Thumbnail, left, top);

               y += height + 6;
            }

            if (pd.DateTimeOriginal != DateTime.MinValue) {
               g.DrawString(pd.DateTimeOriginal.ToString("G"), font, Brushes.DarkGreen, xCenter, y, sfCenter);
               y += font.Height;
            }

            if (!(double.IsNaN(pd.Longitude) || double.IsNaN(pd.Latitude))) {
               string txt = string.Format("{0:N6}° {1:N6}°", pd.Longitude, pd.Latitude);
               if (!double.IsNaN(pd.Direction))
                  txt += string.Format(" [{0:N0}°]", pd.Direction);
               g.DrawString(txt, font, Brushes.DarkRed, xCenter, y, sfCenter);
               y += font.Height;
            }
         }
      }

      /// <summary>
      /// nur bei View.Details
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
         Rectangle rectSubItem = e.Bounds;
         rectSubItem.Width -= 2;
         rectSubItem.Height -= 2;

         // draw background
         if (e.Item.Selected) {
            using (SolidBrush brush = new SolidBrush(System.Drawing.SystemColors.Highlight)) {
               using (Pen pen = new Pen(Color.Black)) {
                  e.Graphics.FillRectangle(brush, rectSubItem);
                  e.Graphics.DrawRectangle(pen, rectSubItem);
               }
            }
         } else {
            //e.DrawBackground();
            using (SolidBrush brush = new SolidBrush(System.Drawing.SystemColors.InactiveCaption)) {
               e.Graphics.FillRectangle(brush, rectSubItem);
            }
         }

         // draw content
         if (e.ColumnIndex == 0) {
            PictureData pd = e.Item.Tag as PictureData;
            if (pd != null) {
               Image img = pd.Thumbnail;
               if (img != null) {
                  Rectangle r = rectSubItem;
                  float h = r.Height;
                  float w = h * img.Width / img.Height;
                  e.Graphics.DrawImage(img, r.Left, r.Top, w, h);
               }
            }
         } else {
            using (Brush brush = new SolidBrush(e.SubItem.ForeColor))
               e.Graphics.DrawString(e.SubItem.Text,
                                     e.SubItem.Font,
                                     brush,
                                     rectSubItem,
                                     new StringFormat() {
                                        Alignment = StringAlignment.Near,
                                        LineAlignment = StringAlignment.Center,
                                        FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces,
                                     });
         }
      }

      #endregion

      void showPictureCount() {
         toolStripStatusLabel_Count.Text = (ShownPictures != 1 ? ShownPictures + " Bilder" : "1 Bild") + " (" + UnsavedPictures + " nicht gespeichert)";
      }

      PictureDataListEventArgs.PictureData[] lastSelectedPictureData = new PictureDataListEventArgs.PictureData[0];

      public void SetExtern(string filename, double longitude, double latitude) {
         ListView lv = listView1;
         PictureData pd = null;
         for (int i = 0; i < listView1.Items.Count; i++) {
            if (lv.Items[i] != null && lv.Items[i].Tag != null) {
               if ((lv.Items[i].Tag as PictureData).FileName == filename) {
                  pd = lv.Items[i].Tag as PictureData;
                  break;
               }
            }
         }

         if (pd != null) {
            pd.ChangeLonLat(longitude, latitude);
            lv.Refresh();
         }
         toolStripButton_SaveAll.Enabled = UnsavedPictures > 0;
         showPictureCount();
         OnShowExtern?.Invoke(this, new PictureDataEventArgs(filename, longitude, latitude));
      }

      private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
         OnDeselectPictures?.Invoke(this, new PictureDataListEventArgs(lastSelectedPictureData));

         toolStripStatusLabel_Filename.Text = "";
         PictureData pd = getFirstSelectedPictureData(sender as ListView);
         if (pd != null && !string.IsNullOrEmpty(pd.FileName)) {
            pictureBox1.Image = (sender as ListView).SelectedIndices.Count == 1 ?
                                    pd.ReadImage() :
                                    null;
            toolStripStatusLabel_Filename.Text = Path.GetFileName(pd.FileName);
         } else
            pictureBox1.Image = null;

         PictureDataListEventArgs.PictureData[] sp = getSelectedPictureDataArray(sender as ListView);
         lastSelectedPictureData = sp;
         OnSelectPictures?.Invoke(this, new PictureDataListEventArgs(sp));

         toolStripButton_SaveGpx.Enabled = sp.Length > 1;
      }

      int itemidx4contextmenu = -1;

      private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         e.Cancel = true;
         itemidx4contextmenu = getItemIdx4Point(listView1, listView1.PointToClient(MousePosition));
         if (itemidx4contextmenu >= 0 &&
             itemidx4contextmenu < listView1.Items.Count) {
            PictureData pd = listView1.Items[itemidx4contextmenu].Tag as PictureData;
            if (pd != null) {
               e.Cancel = false;
               ToolStripMenuItemSave.Enabled = pd.Changed;
            }
         }
      }

      void swapView(ListView lv) {
         lv.SuspendLayout();
         lv.BeginUpdate();
         if (lv.View == View.Tile) {
            lv.View = View.Details;
            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
         } else {
            lv.View = View.Tile;
         }
         lv.ResumeLayout();
         lv.EndUpdate();
      }

      PictureDataListEventArgs.PictureData[] getSelectedPictureDataArray(ListView lv) {
         PictureData[] pd = getSelectedPictureDatas(lv);
         if (pd != null && pd.Length > 0) {
            PictureDataListEventArgs.PictureData[] dat = new PictureDataListEventArgs.PictureData[pd.Length];
            for (int i = 0; i < pd.Length; i++)
               dat[i] = new PictureDataListEventArgs.PictureData(pd[i].FileName, pd[i].Longitude, pd[i].Latitude);
            return dat;
         }
         return new PictureDataListEventArgs.PictureData[0];
      }

      /// <summary>
      /// liefert den Itemindex oder -1 an dieser Pos.
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      int getItemIdx4Point(ListView lv, Point pt) {
         ListViewHitTestInfo hit = lv.HitTest(pt);
         if (hit != null && hit.Item != null)
            return hit.Item.Index;
         return -1;
      }

      /// <summary>
      /// liefert die Daten des 1. ausgewählten Bildes oder null
      /// </summary>
      /// <param name="lv"></param>
      /// <returns></returns>
      PictureData getFirstSelectedPictureData(ListView lv) {
         if (lv.SelectedItems.Count > 0 &&
             lv.SelectedItems[0].Tag != null)
            return lv.SelectedItems[0].Tag as PictureData;
         return null;
      }

      /// <summary>
      /// liefert die Daten aller ausgewählten Bilder oder null
      /// </summary>
      /// <param name="lv"></param>
      /// <returns></returns>
      PictureData[] getSelectedPictureDatas(ListView lv) {
         if (lv.SelectedItems.Count > 0 &&
             lv.SelectedItems[0].Tag != null) {
            int count = 0;
            foreach (ListViewItem item in lv.SelectedItems)
               if (item.Tag != null)
                  count++;
            PictureData[] dat = new PictureData[count];
            count = 0;
            foreach (ListViewItem item in lv.SelectedItems)
               if (item.Tag != null)
                  dat[count++] = item.Tag as PictureData;
            return dat;
         }
         return null;
      }

      PictureData getPictureData4Idx(int idx) {
         if (idx >= 0 &&
             idx < listView1.Items.Count)
            return listView1.Items[idx].Tag as PictureData;
         return null;
      }

      #region Contextmenu

      private void ToolStripMenuItemSave_Click(object sender, EventArgs e) {
         PictureData pd = getPictureData4Idx(itemidx4contextmenu);
         if (pd != null) {
            try {
               pd.Save();
            } catch (Exception ex) {
               MessageBox.Show("Fehler beim Speichern: " + ex.Message, "FEHLER", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }
         showPictureCount();
         toolStripButton_SaveAll.Enabled = UnsavedPictures > 0;
      }

      private void ToolStripMenuItemShow_Click(object sender, EventArgs e) {
         PictureData pd = getPictureData4Idx(itemidx4contextmenu);
         if (pd != null)
            OnShowExtern?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.Longitude, pd.Latitude));
      }

      private void ToolStripMenuItemSet_Click(object sender, EventArgs e) {
         PictureData pd = getPictureData4Idx(itemidx4contextmenu);
         if (pd != null)
            OnNeedNewData?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.Longitude, pd.Latitude));
      }

      private void ToolStripMenuItemEditFilename_Click(object sender, EventArgs e) {
         PictureData pd = getPictureData4Idx(itemidx4contextmenu);
         if (pd != null) {

            Point pt = listView1.PointToScreen(pd.ListViewItem.Position);
            pt.Offset(5, 5);

            FormEditPictureFilename form = new FormEditPictureFilename() {
               Location = pt,
               EditText = Path.GetFileName(pd.FileName),
            };
            if (form.ShowDialog() == DialogResult.OK) {
               if (filenameIsValid(form.EditText))
                  pd.ChangeFilename(form.EditText);
               else
                  MessageBox.Show("Der Dateiname '" + form.EditText + "' ist nicht gültig.", "Achtung", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
         }
      }

      #endregion

      #region Toolbar

      private void toolStripButton_SwapView_Click(object sender, EventArgs e) {
         swapView(listView1);
      }

      private void ToolStripMenuItem_ViewAll_Click(object sender, EventArgs e) {
         pictureFilter = PictureFilter.None;
      }

      private void ToolStripMenuItem_ViewWithGeo_Click(object sender, EventArgs e) {
         pictureFilter = PictureFilter.WithGeoData;
      }

      private void ToolStripMenuItem_ViewWithoutGeo_Click(object sender, EventArgs e) {
         pictureFilter = PictureFilter.WithoutGeoData;
      }

      private void ToolStripMenuItem_FilenameAsc_Click(object sender, EventArgs e) {
         pictureSort = PictureSort.FilenameAscending;
      }

      private void ToolStripMenuItem_FilenameDesc_Click(object sender, EventArgs e) {
         pictureSort = PictureSort.FilenameDescending;
      }

      private void ToolStripMenuItem_FiledateAsc_Click(object sender, EventArgs e) {
         pictureSort = PictureSort.FiledateAscending;
      }

      private void ToolStripMenuItem_FiledateDesc_Click(object sender, EventArgs e) {
         pictureSort = PictureSort.FiledateDescending;
      }

      private void ToolStripMenuItem_GeodateAsc_Click(object sender, EventArgs e) {
         pictureSort = PictureSort.GeodateAscending;
      }

      private void ToolStripMenuItem_GeodateDesc_Click(object sender, EventArgs e) {
         pictureSort = PictureSort.GeodateDescending;
      }

      private void toolStripButton_OpenPath_Click(object sender, EventArgs e) {
         // SendKeys.Send("{TAB}{TAB}{RIGHT}");

         folderBrowserDialog1.SelectedPath = actualPicturePath;
         if (!CancelNewLoad() &&
             folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            newLoad(folderBrowserDialog1.SelectedPath, toolStripButton_WithSubDirs.Checked);
      }

      bool internToolstripButtonSet = false;

      private void toolStripButton_WithSubDirs_Click(object sender, EventArgs e) {
         if (!internToolstripButtonSet) {
            if (!CancelNewLoad())
               newLoad(actualPicturePath, (sender as ToolStripButton).Checked);
            else {
               internToolstripButtonSet = true;
               (sender as ToolStripButton).Checked = !(sender as ToolStripButton).Checked;
               internToolstripButtonSet = false;
            }
         }
      }

      public bool CancelNewLoad() {
         int count = UnsavedPictures;
         if (count > 0)
            return MessageBox.Show("Es gibt noch " + count + "ungespeicherte Änderung/en." + Environment.NewLine + Environment.NewLine +
                                   "Deren Anzeige ist z.Z. eventuell durch Filter unterdrückt." + Environment.NewLine + Environment.NewLine +
                                   "Sollen die Änderungen verworfen werden?",
                                   "Achtung",
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Warning,
                                   MessageBoxDefaultButton.Button2) == DialogResult.No;
         return false;
      }

      private void toolStripButton_SaveAll_Click(object sender, EventArgs e) {
         foreach (PictureData pd in dataCacheAll)
            if (pd.Changed)
               try {
                  pd.Save(false);
                  //pd.ListViewItem.
               } catch (Exception ex) {
                  MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Stop);
               }

         showPictureCount();
         toolStripButton_SaveAll.Enabled = UnsavedPictures > 0;
         listView1.Refresh();
      }

      private void toolStripButton_SaveGpx_Click(object sender, EventArgs e) {
         List<PictureData> dataList = new List<PictureData>();
         foreach (ListViewItem item in listView1.SelectedItems) {
            PictureData pd = item.Tag as PictureData;
            if (pd.HasGeoData)
               dataList.Add(pd);
         }

         if (dataList.Count > 0) {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
               /*
               <?xml version="1.0" encoding="utf-8" standalone="no"?>
               <gpx xmlns="http://www.topografix.com/GPX/1/1" creator="Gpx2Picture" version="1.1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
                <wpt lat="50.9828238001315" lon="13.5817009602195">
                 <name>file:///%USERPROFILE%\Pictures\2022_07_23 Freiberg\20220723_094622.jpg</name>
                 <cmt>20220723_094622.jpg</cmt>
                </wpt>
                <wpt lat="50.9827918888889" lon="13.5816526388889">
                 <name>file:///%USERPROFILE%\Pictures\2022_07_23 Freiberg\20220723_094636.jpg</name>
                 <cmt>20220723_094636.jpg</cmt>
                </wpt>
               </gpx>
                */

               // use known Environment-Paths
               string evar = "";
               string content = "";
               System.Collections.IDictionary ev = Environment.GetEnvironmentVariables();
               foreach (string key in ev.Keys) {
                  if (dataList[0].FileName.StartsWith(ev[key] as string, StringComparison.CurrentCultureIgnoreCase)) {
                     if ((ev[key] as string).Length > content.Length) {
                        content = ev[key] as string;
                        evar = key;
                     }
                  }
               }

               using (StreamWriter file = new StreamWriter(saveFileDialog1.FileName)) {
                  file.WriteLine("<?xml version=\"1.0\" encoding=\"utf - 8\" standalone=\"no\"?>");
                  file.WriteLine("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" creator=\"GpxViewer\" version=\"1.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">");
                  foreach (PictureData pd in dataList) {
                     string filename = pd.FileName;
                     if (evar.Length > 0)
                        filename = "%" + evar + "%" + filename.Substring(content.Length);

                     file.WriteLine(" <wpt lat=\"" + pd.Latitude.ToString(CultureInfo.InvariantCulture) + "\" lon=\"" + pd.Longitude.ToString(CultureInfo.InvariantCulture) + "\">");
                     file.WriteLine("  <name>file:///" + filename + "</name>");
                     file.WriteLine("  <cmt>" + Path.GetFileName(filename) + "</cmt>");
                     file.WriteLine(" </wpt>");
                  }
                  file.WriteLine("</gpx>");
               }
            }
         }
      }

      private void toolStripButton_Reload_Click(object sender, EventArgs e) {
         if (!CancelNewLoad() &&
             !string.IsNullOrEmpty(actualPicturePath))
            newLoad(actualPicturePath, toolStripButton_WithSubDirs.Checked);
      }

      #endregion

      bool filenameIsValid(string fileName) {
         FileInfo fi = null;
         try {
            fi = new FileInfo(fileName);
         } catch (ArgumentException) { } catch (PathTooLongException) { } catch (NotSupportedException) { }
         if (!ReferenceEquals(fi, null) &&
             !File.Exists(fileName))
            return true;
         return false;
      }

      //private void button4_Click(object sender, EventArgs e) {

      //   // TEST
      //   string filename = "";
      //   if (listView1.SelectedItems.Count > 0) {
      //      ListViewItem lvi = listView1.SelectedItems[0];
      //      if (lvi != null && lvi.Tag != null) {
      //         PictureData pd = lvi.Tag as PictureData;
      //         filename = pd.FileName;
      //      }
      //   }

      //   if (!string.IsNullOrEmpty(filename))
      //      SetExtern(filename, 12.345678, 15.678901);

      //}

   }
}
