using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GpxViewer {
   public partial class PictureManager : UserControl {

      class PictureData {

         /// <summary>
         /// Spaltenindex im <see cref="ListViewItem"/>
         /// </summary>
         public const int SUBIDX_PICTURE = 0;
         public const int SUBIDX_FILENAME = 1;
         public const int SUBIDX_FILEDATE = 2;
         public const int SUBIDX_ORIGINALDATE = 3;
         public const int SUBIDX_GEOCOORD = 4;
         public const int SUBIDX_GEODIRECT = 5;
         public const int SUBIDX_USERCOMMENT = 6;
         public const int SUBIDX_LAST = SUBIDX_USERCOMMENT;

         /// <summary>
         /// Dateiname
         /// </summary>
         public string FileName { get; protected set; }

         /// <summary>
         /// (kleines) Bild für die Anzeige im <see cref="ListViewItem"/>
         /// </summary>
         public Image Thumbnail { get; protected set; }

         /// <summary>
         /// Dateidatum
         /// </summary>
         public DateTime DateTimeFile { get; protected set; }

         /// <summary>
         /// Bilddatum aus den Baseline-Tags (i.A. identisch zu <see cref="OriginalDateTime"/>)
         /// </summary>
         public DateTime DateTime { get; protected set; }

         /// <summary>
         /// Originaldatum des Bildes (EXIF) (The date and time when the original image data was generated. For a digital still camera, this is the date and time the picture was taken or recorded.)
         /// </summary>
         public DateTime OriginalDateTime { get; protected set; }

         /// <summary>
         /// geografische Breite des Bilds (EXIF)
         /// </summary>
         public double Latitude { get; protected set; }

         /// <summary>
         /// geografische Länge des Bilds (EXIF)
         /// </summary>
         public double Longitude { get; protected set; }

         /// <summary>
         /// Richtung des Bildes (0..360°) (EXIF)
         /// </summary>
         public double Direction { get; protected set; }

         /// <summary>
         /// Bildkommentar (EXIF)
         /// </summary>
         public string UserComment { get; protected set; }

         /// <summary>
         /// Daten wurden geändert
         /// </summary>
         public bool Changed { get; protected set; } = false;

         /// <summary>
         /// Wurden die EXIF-Daten schon eingelesen?
         /// </summary>
         public bool IsExifRead { get; protected set; } = false;

         /// <summary>
         /// Wurde der <see cref="Thumbnail"/> schon erzeugt?
         /// </summary>
         public bool ExistThumbnail =>
            Thumbnail != null;

         /// <summary>
         /// Sind geografische Länge und Breite vorhanden?
         /// </summary>
         public bool HasGeoData =>
            !(double.IsNaN(Longitude) || double.IsNaN(Latitude));

         /// <summary>
         /// zugehöriges <see cref="ListViewItem"/>
         /// </summary>
         public ListViewItem ListViewItem;

         /// <summary>
         /// unter diesem Namen (noch) gespeichert
         /// </summary>
         string orgFilename = "";


         /// <summary>
         /// ermittelt alle notwendigen Daten, liest aber noch nicht das Bild (erzeugt also noch kein <see cref="Thumbnail"/>)
         /// </summary>
         /// <param name="filename"></param>
         public PictureData(string filename) {
            orgFilename = FileName = filename;

            DateTimeFile = File.GetLastWriteTime(orgFilename);
            getActualExifData();

            string text = Path.GetFileName(orgFilename);
            ListViewItem = new ListViewItem() {
               Tag = this,
               ToolTipText = text,
               //Text = text,
               ImageIndex = 0,
            };
            setSubItemsText();
         }

         /// <summary>
         /// ermittelt alle notwendigen Daten und erzeugt auch den <see cref="Thumbnail"/>
         /// </summary>
         /// <param name="filename"></param>
         /// <param name="thumbnailsize"></param>
         public PictureData(string filename, Size thumbnailsize) : this(filename) {
            CreateThumbnail(thumbnailsize);
         }

         /// <summary>
         /// erzeugt den <see cref="Thumbnail"/> aus den Dateidaten
         /// </summary>
         /// <param name="thumbnailsize"></param>
         public void CreateThumbnail(Size thumbnailsize) {
            if (Thumbnail == null)
               using (Image img = ReadImage(orgFilename)) {
                  if (img != null)
                     Thumbnail = createThumbnail(img, thumbnailsize.Width, thumbnailsize.Height);
               }
         }

         /// <summary>
         /// geografische Länge und Breite werden geändert
         /// </summary>
         /// <param name="longitude"></param>
         /// <param name="latitude"></param>
         public void ChangeLonLat(double longitude, double latitude) {
            Changed = true;
            Longitude = longitude;
            Latitude = latitude;
            setSubItemsText();
         }

         /// <summary>
         /// der Dateiname wir geändert
         /// </summary>
         /// <param name="poorfilename"></param>
         public void ChangeFilename(string poorfilename) {
            Changed = true;
            FileName = Path.Combine(Path.GetDirectoryName(orgFilename), poorfilename);
            setSubItemsText();
         }

         /// <summary>
         /// der Dateikommentar wird geändert
         /// </summary>
         /// <param name="comment"></param>
         public void ChangeUserComment(string comment) {
            Changed = true;
            UserComment = comment;
            setSubItemsText();
         }

         /// <summary>
         /// wenn Daten geändert wurden (<see cref="Changed"/>==true) wird die Datei neu gespeichert
         /// </summary>
         /// <param name="preserveDateTimeFile"></param>
         /// <returns></returns>
         public bool Save(bool preserveDateTimeFile = false) {
            if (Changed) {
               DateTime orgDateTime = File.GetLastWriteTime(orgFilename);
               ExifGeo exifGeo = new ExifGeo(orgFilename);

               bool set1 = exifGeo.SetLatLon(Latitude, Longitude, true);
               bool set2 = exifGeo.SetUserComment(UserComment);
               if (set1 ||
                   set2) {
                  exifGeo.SaveImage(FileName);
                  getActualExifData();
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

         /// <summary>
         /// das Bild wird aus der Datei eingelesen
         /// </summary>
         /// <returns></returns>
         public Image ReadImage() =>
            ReadImage(orgFilename);

         /// <summary>
         /// das Bild wird aus dieser Datei eingelesen
         /// </summary>
         /// <param name="filename"></param>
         /// <returns></returns>
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

         /// <summary>
         /// alle EXIF-Daten werden eingelesen
         /// </summary>
         void getActualExifData() {
            ExifGeo exifGeo = new ExifGeo(orgFilename);

            OriginalDateTime = exifGeo.GetDateTimeOriginal();
            DateTime = exifGeo.GetDateTime();
            if (DateTime == DateTime.MinValue &&
               OriginalDateTime != DateTime.MinValue)
               DateTime = OriginalDateTime;

            Direction = exifGeo.GetDirection();
            Latitude = exifGeo.GetLat();
            Longitude = exifGeo.GetLon();
            UserComment = exifGeo.GetUserComment();
            IsExifRead = true;
         }

         /// <summary>
         /// setzt die Texte der Subitems entsprechend den akt. Daten
         /// </summary>
         void setSubItemsText() {
            const string textprefix = "  ";
            const string textpostfix = "   ";    // need for AutoResizeColumns()

            while (ListViewItem.SubItems.Count < SUBIDX_LAST + 1)
               ListViewItem.SubItems.Add("");
            ListViewItem.SubItems[SUBIDX_PICTURE].Text = "";
            ListViewItem.SubItems[SUBIDX_FILENAME].Text = textprefix + Path.GetFileName(FileName) + textpostfix;
            ListViewItem.SubItems[SUBIDX_FILEDATE].Text = textprefix + DateTimeFile.ToString("G") + textpostfix;
            ListViewItem.SubItems[SUBIDX_ORIGINALDATE].Text = textprefix + OriginalDateTime.ToString("G") + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEOCOORD].Text = double.IsNaN(Longitude) || double.IsNaN(Latitude) ? textprefix : string.Format("{0:N6}° {1:N6}°", Longitude, Latitude) + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEODIRECT].Text = double.IsNaN(Direction) ? "" : textprefix + string.Format("{0:N0}°", Direction) + textpostfix;
            ListViewItem.SubItems[SUBIDX_USERCOMMENT].Text = textprefix + UserComment + textpostfix;
         }

         /// <summary>
         /// erzeugt aus dem Bild den <see cref="Thumbnail"/>
         /// </summary>
         /// <param name="img"></param>
         /// <param name="width"></param>
         /// <param name="height"></param>
         /// <returns></returns>
         static Bitmap createThumbnail(Image img, int width, int height) {
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

         public override string ToString() {
            return string.Format("{0}, {1}, lat={2}°, lon={3}°, direction={4}°",
                                 FileName,
                                 DateTime.ToString("G"),
                                 Latitude,
                                 Longitude,
                                 Direction);
         }
      }

      /// <summary>
      /// Filter für die Anzeige der Bilder
      /// </summary>
      enum PictureFilter {
         None,
         /// <summary>
         /// nur wenn Geodaten vorhanden sind
         /// </summary>
         WithGeoData,
         /// <summary>
         /// nur wenn keine Geodaten vorhanden sind
         /// </summary>
         WithoutGeoData,
      }

      /// <summary>
      /// Sortierung der Bilder
      /// </summary>
      enum PictureSort {
         None,
         /// <summary>
         /// nach Dateiname aufsteigen
         /// </summary>
         FilenameAscending,
         /// <summary>
         /// nach Dateiname absteigen
         /// </summary>
         FilenameDescending,
         /// <summary>
         /// nach Dateidatum aufsteigen
         /// </summary>
         FiledateAscending,
         /// <summary>
         /// nach Dateidatum absteigen
         /// </summary>
         FiledateDescending,
         /// <summary>
         /// nach Aufnahmedatum aufsteigen
         /// </summary>
         OriginaldateAscending,
         /// <summary>
         /// nach Aufnahmedatum aufsteigen
         /// </summary>
         OriginaldateDescending,
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

         public readonly DateTime Timestamp;


         public PictureDataEventArgs(string filename, double lon, double lat) {
            Filename = filename;
            Longitude = lon;
            Latitude = lat;
         }

         public PictureDataEventArgs(string filename, DateTime timestamp) {
            Filename = filename;
            Timestamp = timestamp;
            Longitude = Latitude = double.MinValue;
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

                  case PictureSort.OriginaldateAscending:
                     listView1.ListViewItemSorter = new ListViewGeodateAscendComparer(ThumbnailSize);
                     break;

                  case PictureSort.OriginaldateDescending:
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

               case PictureSort.OriginaldateAscending:
                  ToolStripMenuItem_GeodateAsc.Checked = true;
                  break;

               case PictureSort.OriginaldateDescending:
                  ToolStripMenuItem_GeodateDesc.Checked = true;
                  break;
            }
         }
      }

      public string ActualPicturePath { get; set; } = "";

      const int ITEMMARGIN = 2;
      const int ITEMMARGINTHUMBNAIL = 3;

      List<string> proposalComment = new List<string>();

      List<string> proposalFilename = new List<string>();


      /*
       * Bei einem Verzeichniswechsel werden die EXIF-Daten aller Bilder eingelesen. 
       *    Das könnte man vermeiden und diese Daten immer erst bei Bedarf einlesen. Bei der Bildsortierung und -filterung müssten dann aber z.T. alle Daten auf
       *    einmal ermittelt werden.
       * Das jeweilige Thumbnail wird erst erzeugt, wenn es das Listview benötigt.
       * Das Bild für die Picturebox wird immer neu eingelesen, wenn es benötigt wird.
       * 
       */


      public PictureManager() {
         InitializeComponent();

         ActualPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
      }

      private void PictureManager_Load(object sender, EventArgs e) {
         listView1.DrawItem += ListView1_DrawItem;
         listView1.DrawSubItem += ListView1_DrawSubItem;
         listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
         // ACHTUNG
         // Es scheint ein interner Fehler zu ex.. Wenn im Designer die View-Art auf Detail gesetzt ist, wird bei der Zeichnenfunktion eine falscher Bounds-Wert geliefert,
         // obwohl TileSize korrekt ist. Deshalb schon im Designer auf "View.Tile" setzen!!!
         listView1.TileSize = computeTileSize();
         listView1.OwnerDraw = true;
         if (listView1.View != View.Tile)
            swapView(listView1);
         listView1.MultiSelect = false;


         /*
                 this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                     this.columnPicture,
                     this.columnFile,
                     this.columnFileDate,
                     this.columnPictureDate,
                     this.columnCoordinates,
                     this.columnDirection,
                     this.columnComment});
          */

         listView1.Columns[PictureData.SUBIDX_ORIGINALDATE].DisplayIndex = 1;
         listView1.Columns[PictureData.SUBIDX_USERCOMMENT].DisplayIndex = 2;


         pictureFilter = PictureFilter.None;
         pictureSort = PictureSort.FilenameAscending;

         toolStripButton_SaveGpx.Enabled = false;
         toolStripButton_SaveAll.Enabled = false;
      }

      #region public-functions

      /// <summary>
      /// neuen Dateipfad setzen
      /// </summary>
      /// <param name="path"></param>
      /// <param name="recursesubdirectories"></param>
      public async void SetPicturePath(string path, bool recursesubdirectories) {
         string newpath = Path.GetFullPath(path);
         if (!string.IsNullOrEmpty(newpath) &&
             Directory.Exists(newpath)) {
            ActualPicturePath = newpath;
            listView1.Items.Clear();   // als Zeichen, dass etwas passiert
            Cursor orgcursor = Cursor;
            Cursor = Cursors.WaitCursor;

            //fillDataCache(ActualPicturePath, recursesubdirectories);
            await fillDataCacheAsync(ActualPicturePath, recursesubdirectories);
            fillListView(listView1, pictureFilter, pictureSort);

            setStatusSaveButtons();

            Cursor = orgcursor;

            if (Parent is Form) {
               Parent.Text = "Bilder: " + ActualPicturePath;
            }
         }
      }

      /// <summary>
      /// Kommentar eines Bildes ändern
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void EditPictureComment(int idx = -1) =>
         editPictureText(idx, true);

      /// <summary>
      /// Dateiname eines Bildes ändern
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void EditPictureFilename(int idx = -1) {
         editPictureText(idx, false);
      }

      /// <summary>
      /// Bild speichern
      /// </summary>
      /// <param name="all">alle ungespeicherten Items oder nur 1</param>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void SavePicture(bool all, int idx = -1) {
         if (all)
            toolStripButton_SaveAll_Click(null, null);
         else {
            PictureData pd = getPictureData(idx);
            if (pd != null &&
                pd.Changed) {
               try {
                  pd.Save();
               } catch (Exception ex) {
                  MessageBox.Show("Fehler beim Speichern: " + ex.Message, "FEHLER", MessageBoxButtons.OK, MessageBoxIcon.Error);
               }
            }
            showPictureCount();
            setStatusSaveButtons();
         }
      }

      /// <summary>
      /// Bildposition auf Karte anzeigen
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void GoToPicturePosition(int idx = -1) {
         PictureData pd = getPictureData(idx);
         if (pd != null &&
             !double.IsNaN(pd.Longitude) &&
             !double.IsNaN(pd.Latitude))
            OnShowExtern?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.Longitude, pd.Latitude));
      }

      /// <summary>
      /// Bildposition auf Karte setzen
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void SetPicturePosition(int idx = -1) {
         PictureData pd = getPictureData(idx);
         if (pd != null)
            OnNeedNewData?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.Longitude, pd.Latitude));
      }

      public void SetPicturePosition2(int idx = -1) {
         PictureData pd = getPictureData(idx);
         if (pd != null)
            OnNeedNewData?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.DateTime));
      }

      public void SetPositionExtern(string filename, double longitude, double latitude) {
         if (!string.IsNullOrEmpty(filename) &&
             longitude != double.MinValue &&
             latitude != double.MinValue) {
            ListView lv = listView1;
            PictureData pd = null;
            // Item suchen
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
            showPictureCount();
            setStatusSaveButtons();
            OnShowExtern?.Invoke(this, new PictureDataEventArgs(filename, longitude, latitude));
         }
      }

      public void OpenPath() {
         toolStripButton_OpenPath_Click(null, null);
      }

      public void SwapView() {
         toolStripButton_SwapView_Click(null, null);
      }

      public void Reload() {
         toolStripButton_Reload_Click(null, null);
      }

      #endregion

      /// <summary>
      /// Text für Kommentar oder Dateiname ändern
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="comment"></param>
      void editPictureText(int idx, bool comment) {
         PictureData pd = getPictureData(idx);
         if (pd != null) {
            Point pt = listView1.PointToScreen(pd.ListViewItem.Bounds.Location);
            if (pd.ListViewItem.ListView.View == View.Tile) {
               pt.Offset(5, 5);
            } else if (pd.ListViewItem.ListView.View == View.Details) {
               pt = listView1.PointToScreen(pd.ListViewItem.SubItems[comment ?
                                                                        PictureData.SUBIDX_USERCOMMENT :
                                                                        PictureData.SUBIDX_FILENAME].Bounds.Location);
            }

            FormEditPictureFilename form = new FormEditPictureFilename() {
               Location = pt,
               EditText = comment ?
                              pd.UserComment :
                              Path.GetFileName(pd.FileName),
               ProposalText = comment ?
                                 proposalComment :
                                 proposalFilename,
            };
            string oldtext = form.EditText;
            if (form.ShowDialog() == DialogResult.OK &&
                oldtext != form.EditText) {
               if (comment) {
                  pd.ChangeUserComment(form.EditText);
                  if (form.EditText != "") {
                     proposalComment.Remove(form.EditText);
                     proposalComment.Insert(0, form.EditText);
                  }
               } else {
                  if (filenameIsValid(form.EditText)) {
                     pd.ChangeFilename(form.EditText);
                     if (form.EditText != "") {
                        proposalFilename.Remove(form.EditText);
                        proposalFilename.Insert(0, form.EditText);
                     }
                  } else
                     MessageBox.Show("Der Dateiname '" + form.EditText + "' ist nicht gültig.", "Achtung", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
               }
               if (pd.Changed) {
                  showPictureCount();
                  setStatusSaveButtons();
               }
            }
         }
      }

      void setStatusSaveButtons() {
         toolStripButton_SaveGpx.Enabled = getSelectedPictureDataArray(listView1).Length > 0; // ShownPictures > 0
         toolStripButton_SaveAll.Enabled = UnsavedPictures > 0;
      }

      async Task fillDataCacheAsync(string path, bool recursesubdirectories) {
         await Task.Run(() => {
            fillDataCache(path, recursesubdirectories);
         });
      }

      /// <summary>
      /// fill <see cref="dataCacheAll"/> with filedata from path
      /// </summary>
      /// <param name="path"></param>
      /// <param name="recursesubdirectories"></param>
      void fillDataCache(string path, bool recursesubdirectories) {
         List<string> pictfiles = new List<string>();

         if (Directory.Exists(path)) {
            pictfiles.AddRange(Directory.GetFiles(path, "*?.png", recursesubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            pictfiles.AddRange(Directory.GetFiles(path, "*?.jpg", recursesubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            pictfiles.AddRange(Directory.GetFiles(path, "*?.jpeg", recursesubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            pictfiles.Sort((string name1, string name2) => { return string.Compare(name1, name2, true); });
         }
         // "*.jpg" liefert auch "*.jpg_orig", deshalb:
         for (int i = pictfiles.Count - 1; i >= 0; i--) {
            string ext = Path.GetExtension(pictfiles[i]).ToLower();
            if (!(ext == ".png" ||
                  ext == ".jpg" ||
                  ext == ".jpeg"))
               pictfiles.RemoveAt(i);
         }
         fillDataCache(pictfiles);
      }

      /// <summary>
      /// fill <see cref="dataCacheAll"/> with filedata for files
      /// </summary>
      /// <param name="files"></param>
      void fillDataCache(IList<string> files) {
         dataCacheAll.Clear();
         foreach (string file in files) {
            PictureData pd = new PictureData(file);
            dataCacheAll.Add(pd);
         }
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

         toolStripStatusLabel_Path.Text = ActualPicturePath;
         showPictureCount();
      }

      #region ownerdraw listview

      private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e) {
         ListView lv = sender as ListView;
         PictureData pd = e.Item.Tag != null ?
                              e.Item.Tag as PictureData :
                              null;
         if (pd != null) {
            pd.ListViewItem.ToolTipText = Path.GetFileName(pd.FileName);
            if (!pd.ExistThumbnail)
               try {
                  pd.CreateThumbnail(ThumbnailSize);
               } catch (Exception ex) {
                  MessageBox.Show("Fehler beim Lesen der Bilddatei '" + pd.FileName + "': " + ex.Message, "FEHLER", MessageBoxButtons.OK, MessageBoxIcon.Error);
               }
         }

         if (lv.View == View.Tile) {
            drawTileBackground(e.Graphics, pd, e.Bounds, e.Item.Selected);

            //if (e.Item.Selected &&
            //    e.Item.Focused)
            //   e.DrawFocusRectangle();           // unnötig wegen Backgroundfarbe

            drawTileContent(e.Graphics,
                            pd,
                            e.Bounds,
                            e.Item.ListView.Font);
         } else {

            // e.Item.Bounds.Width == e.Item.SubItems[0].Bounds.Width
            int w = e.Item.SubItems[0].Bounds.Width;
            for (int i = 1; i < e.Item.SubItems.Count; i++)
               w -= e.Item.SubItems[i].Bounds.Width;

            Rectangle rectSubItem = new Rectangle(e.Item.Position.X - (lv.Margin.Left + 1),
                                                  e.Item.Bounds.Y,
                                                  w,
                                                  e.Item.Bounds.Height);

            drawDetailBackground(e.Graphics, e.Item, rectSubItem);
            drawDetailColPicture(e.Graphics, e.Item, rectSubItem);

            //if (e.Item.Focused)
            //   e.DrawFocusRectangle();
         }
      }

      void drawTileBackground(Graphics g, PictureData pd, Rectangle bounds, bool selected) {
         if (selected) {
            using (SolidBrush brush = new SolidBrush(SystemColors.Highlight))
               g.FillRectangle(brush, bounds);
         } else {
            if (pd == null || !pd.Changed)
               using (LinearGradientBrush brush = new LinearGradientBrush(bounds, Color.LightGray, Color.DarkGray, LinearGradientMode.Vertical))
                  g.FillRectangle(brush, bounds);
            else
               using (LinearGradientBrush brush = new LinearGradientBrush(bounds, Color.LightSalmon, Color.Red, LinearGradientMode.Vertical))
                  g.FillRectangle(brush, bounds);
         }
      }

      /// <summary>
      /// nur bei <see cref="View.Tile"/>
      /// </summary>
      /// <param name="g"></param>
      /// <param name="pd"></param>
      /// <param name="bounds"></param>
      /// <param name="font">i.A. Font des <see cref="ListView"/></param>
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

            y += ITEMMARGIN;

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

            if (pd.Thumbnail != null) {      // Bild ausgeben
               y += ITEMMARGINTHUMBNAIL;

               int left = xCenter - pd.Thumbnail.Width / 2;
               int top = y;
               int height = pd.Thumbnail.Height;
               int width = pd.Thumbnail.Width;

               Brush content = SystemBrushes.ControlLight;        // Brushes.White;
               Brush ligthedge = SystemBrushes.ControlLightLight; // Brushes.LightGray;
               Brush darkedge = SystemBrushes.ControlDark;        // Brushes.DarkGray;

               g.FillRectangle(darkedge, left - ITEMMARGINTHUMBNAIL, top - ITEMMARGINTHUMBNAIL, width + 2 * ITEMMARGINTHUMBNAIL, height + 2 * ITEMMARGINTHUMBNAIL);
               g.FillRectangle(ligthedge, left, top, width + ITEMMARGINTHUMBNAIL, height + ITEMMARGINTHUMBNAIL);
               g.FillPolygon(ligthedge,
                             new Point[] {
                                new Point(left - ITEMMARGINTHUMBNAIL, top + height + ITEMMARGINTHUMBNAIL),
                                new Point(left, top + height + ITEMMARGINTHUMBNAIL),
                                new Point(left, top + height),
                             });
               g.FillPolygon(ligthedge,
                             new Point[] {
                                new Point(left + width, top),
                                new Point(left + width + ITEMMARGINTHUMBNAIL, top),
                                new Point(left + width + ITEMMARGINTHUMBNAIL, top - ITEMMARGINTHUMBNAIL),
                             });
               g.FillRectangle(content, left, top, width, height);

               g.DrawImageUnscaled(pd.Thumbnail, left, top);

               y += height + 2 * ITEMMARGINTHUMBNAIL;
            }

            if (pd.OriginalDateTime != DateTime.MinValue) {
               g.DrawString(pd.OriginalDateTime.ToString("G"), font, Brushes.DarkGreen, xCenter, y, sfCenter);
               y += font.Height;
            }

            if (!(double.IsNaN(pd.Longitude) || double.IsNaN(pd.Latitude))) {
               string txt = string.Format("{0:N6}° {1:N6}°", pd.Longitude, pd.Latitude);
               if (!double.IsNaN(pd.Direction))
                  txt += string.Format(" [{0:N0}°]", pd.Direction);
               g.DrawString(txt, font, Brushes.DarkRed, xCenter, y, sfCenter);
               y += font.Height;
            }
            if (!string.IsNullOrEmpty(pd.UserComment)) {
               g.DrawString(pd.UserComment, font, Brushes.Black, xCenter, y, sfCenter);
               y += font.Height;
            }
         }
      }

      private void ListView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
         using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.LightGray, Color.DarkGray, LinearGradientMode.Vertical))
            e.Graphics.FillRectangle(brush, e.Bounds);
         using (Pen pen = new Pen(Color.Black))
            e.Graphics.DrawRectangle(pen, e.Bounds);
         e.DrawText(TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }

      void drawDetailBackground(Graphics g, ListViewItem lvi, Rectangle bounds) {
         //e.DrawBackground();
         if (lvi.Selected) {
            using (SolidBrush brush = new SolidBrush(System.Drawing.SystemColors.Highlight))
               g.FillRectangle(brush, bounds);
            using (Pen pen = new Pen(Color.Black))
               g.DrawRectangle(pen, bounds);
         } else {
            PictureData pd = lvi.Tag != null ?
                                 lvi.Tag as PictureData :
                                 null;
            if (pd != null) {
               using (SolidBrush brush = new SolidBrush(pd.Changed ? Color.FromArgb(255, 128, 128) : Color.LightGray))
                  g.FillRectangle(brush, bounds);
               using (Pen pen = new Pen(Color.Black, 1))
                  g.DrawRectangle(pen, bounds);
            }
         }
      }

      void drawDetailColPicture(Graphics g, ListViewItem lvi, Rectangle bounds) {
         using (SolidBrush brush = new SolidBrush(lvi.BackColor))
            g.FillRectangle(brush, bounds);
         using (Pen pen = new Pen(Color.Black, 1))
            g.DrawRectangle(pen, bounds);

         PictureData pd = lvi.Tag as PictureData;
         if (pd != null) {
            Image img = pd.Thumbnail;
            if (img != null) {
               g.DrawImage(img,
                           bounds.Left + 1,
                           bounds.Top + 1,
                           (bounds.Height - 1) * img.Width / img.Height,
                           bounds.Height - 1);
            }
         }
      }

      void drawDetailColText(Graphics g, ListViewItem.ListViewSubItem lvsi, Rectangle bounds) {
         using (Brush brush = new SolidBrush(lvsi.ForeColor))
            g.DrawString(lvsi.Text,
                         lvsi.Font,
                         brush,
                         bounds,
                         new StringFormat() {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces,
                         });
      }

      /// <summary>
      /// nur bei View.Details
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
         // draw content
         if (e.ColumnIndex > 0) {
            drawDetailBackground(e.Graphics, e.Item, e.Bounds);
            drawDetailColText(e.Graphics, e.SubItem, e.Bounds);
         }
      }

      #endregion

      void showPictureCount() {
         toolStripStatusLabel_Count.Text = (ShownPictures != 1 ? ShownPictures + " Bilder" : "1 Bild") + " (" + UnsavedPictures + " nicht gespeichert)";
      }

      PictureDataListEventArgs.PictureData[] lastSelectedPictureData = new PictureDataListEventArgs.PictureData[0];

      private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
         OnDeselectPictures?.Invoke(this, new PictureDataListEventArgs(lastSelectedPictureData));

         toolStripStatusLabel_Filename.Text = "";
         PictureData pd = getFirstSelectedPictureData(sender as ListView);
         if (pd != null &&
             !string.IsNullOrEmpty(pd.FileName)) {
            pictureBox1.Image = (sender as ListView).SelectedIndices.Count == 1 ?   // genau 1 Bild markiert
                                    pd.ReadImage() :                                // da das Originalbild nicht gespeichert wird, wird es immer akt. eingelesen
                                    null;
            toolStripStatusLabel_Filename.Text = Path.GetFileName(pd.FileName);
         } else
            pictureBox1.Image = null;

         PictureDataListEventArgs.PictureData[] sp = getSelectedPictureDataArray(sender as ListView);
         lastSelectedPictureData = sp;
         OnSelectPictures?.Invoke(this, new PictureDataListEventArgs(sp));

         toolStripButton_SaveGpx.Enabled = sp.Length > 0;
      }

      /// <summary>
      /// liefert den Index des ListView-Items oder -1
      /// </summary>
      /// <param name="desctoppt"></param>
      /// <returns></returns>
      int listviewItemIdx4Point(Point desctoppt) {
         int idx = getItemIdx4Point(listView1, listView1.PointToClient(desctoppt));
         return 0 <= idx && idx < listView1.Items.Count ? idx : -1;
      }

      int itemidx4contextmenu = -1;

      private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         e.Cancel = true;
         itemidx4contextmenu = listviewItemIdx4Point(MousePosition);
         if (0 <= itemidx4contextmenu) {
            PictureData pd = listView1.Items[itemidx4contextmenu].Tag as PictureData;
            if (pd != null) {
               e.Cancel = false;
               ToolStripMenuItemSave.Enabled = pd.Changed;
            }
         }
      }

      /// <summary>
      /// berechnet die notwendige Größe für ein Tile
      /// </summary>
      /// <returns></returns>
      Size computeTileSize() =>
         new Size(TextRenderer.MeasureText("=-999,999999° -99,999999° [999°]=", listView1.Font).Width,   // längster möglicherText
                  ITEMMARGIN +
                  2 * listView1.Font.Height +                                             // 2 Textzeilen
                  ITEMMARGINTHUMBNAIL + ThumbnailSize.Height + 2 * ITEMMARGINTHUMBNAIL +  // Bild
                  3 * listView1.Font.Height +                                             // 3 Textzeilen
                  ITEMMARGIN);

      async void swapViewAsync(ListView lv) {
         Cursor orgcursor = Cursor;
         Cursor = Cursors.WaitCursor;

         await Task.Run(() => {
            swapView(lv);
         });

         Cursor = orgcursor;
      }


      /// <summary>
      /// !!! relativ langsam !!!
      /// </summary>
      /// <param name="lv"></param>
      void swapView(ListView lv) {
         Cursor orgcursor = Cursor;
         Cursor = Cursors.WaitCursor;

         lv.SuspendLayout();
         lv.BeginUpdate();
         if (lv.View == View.Tile) {
            lv.View = View.Details;
            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            // zusätzlich die Breite für die Spaltenheader testen:
            ListView.ColumnHeaderCollection cc = lv.Columns;
            for (int i = 0; i < cc.Count; i++) {
               int colWidth = TextRenderer.MeasureText(cc[i].Text, lv.Font).Width + 20;
               if (colWidth > cc[i].Width)   // falls zu schmal
                  cc[i].Width = colWidth;
            }
         } else {
            lv.TileSize = computeTileSize();
            lv.View = View.Tile;
         }
         lv.ResumeLayout();
         lv.EndUpdate();

         Cursor = orgcursor;
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

      /// <summary>
      /// liefert die <see cref="PictureData"/> zum Item mit dem Index oder zum 1. ausgewählten Item oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      PictureData getPictureData(int idx) {
         PictureData pd = 0 <= idx &&
                          idx < listView1.Items.Count ?
                                    listView1.Items[idx].Tag as PictureData :
                                    null;
         if (pd == null)
            pd = listView1.SelectedItems.Count == 1 ?
                     listView1.SelectedItems[0].Tag as PictureData :
                     null;
         return pd;
      }

      #region Contextmenu

      private void ToolStripMenuItemSave_Click(object sender, EventArgs e) {
         SavePicture(false, itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      private void ToolStripMenuItemShow_Click(object sender, EventArgs e) {
         GoToPicturePosition(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      private void ToolStripMenuItemSet_Click(object sender, EventArgs e) {
         SetPicturePosition(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      private void ToolStripMenuItemSet2_Click(object sender, EventArgs e) {
         SetPicturePosition2(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      private void ToolStripMenuItemEditComment_Click(object sender, EventArgs e) {
         EditPictureComment(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      private void ToolStripMenuItemEditFilename_Click(object sender, EventArgs e) {
         EditPictureFilename(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }


      #endregion

      #region Toolbar

      private void toolStripButton_SwapView_Click(object sender, EventArgs e) =>
         swapView(listView1);

      private void ToolStripMenuItem_ViewAll_Click(object sender, EventArgs e) =>
         pictureFilter = PictureFilter.None;

      private void ToolStripMenuItem_ViewWithGeo_Click(object sender, EventArgs e) =>
         pictureFilter = PictureFilter.WithGeoData;

      private void ToolStripMenuItem_ViewWithoutGeo_Click(object sender, EventArgs e) =>
         pictureFilter = PictureFilter.WithoutGeoData;

      private void ToolStripMenuItem_FilenameAsc_Click(object sender, EventArgs e) =>
         pictureSort = PictureSort.FilenameAscending;

      private void ToolStripMenuItem_FilenameDesc_Click(object sender, EventArgs e) =>
         pictureSort = PictureSort.FilenameDescending;

      private void ToolStripMenuItem_FiledateAsc_Click(object sender, EventArgs e) =>
         pictureSort = PictureSort.FiledateAscending;

      private void ToolStripMenuItem_FiledateDesc_Click(object sender, EventArgs e) =>
         pictureSort = PictureSort.FiledateDescending;

      private void ToolStripMenuItem_GeodateAsc_Click(object sender, EventArgs e) =>
         pictureSort = PictureSort.OriginaldateAscending;

      private void ToolStripMenuItem_GeodateDesc_Click(object sender, EventArgs e) =>
         pictureSort = PictureSort.OriginaldateDescending;

      private void toolStripButton_OpenPath_Click(object sender, EventArgs e) {
         // SendKeys.Send("{TAB}{TAB}{RIGHT}");

         folderBrowserDialog1.SelectedPath = ActualPicturePath;
         if (!CancelNewLoad() &&
             folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            SetPicturePath(folderBrowserDialog1.SelectedPath, toolStripButton_WithSubDirs.Checked);
      }

      bool internToolstripButtonSet = false;

      private void toolStripButton_WithSubDirs_Click(object sender, EventArgs e) {
         if (!internToolstripButtonSet) {
            if (!CancelNewLoad())
               SetPicturePath(ActualPicturePath, (sender as ToolStripButton).Checked);
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
         Cursor orgcursor = Cursor;
         Cursor = Cursors.WaitCursor;

         foreach (PictureData pd in dataCacheAll)
            if (pd.Changed)
               try {
                  pd.Save(false);
                  //pd.ListViewItem.
               } catch (Exception ex) {
                  MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Stop);
               }

         showPictureCount();
         setStatusSaveButtons();
         listView1.Refresh();

         Cursor = orgcursor;
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
             !string.IsNullOrEmpty(ActualPicturePath))
            SetPicturePath(ActualPicturePath, toolStripButton_WithSubDirs.Checked);
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
   }
}
