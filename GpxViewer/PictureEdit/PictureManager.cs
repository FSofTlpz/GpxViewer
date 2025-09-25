#define DEBUG_EXT

using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace GpxViewer.PictureEdit {
   public partial class PictureManager : UserControl {

      /// <summary>
      /// arbeitet intern mit dem <see cref="ThreadPool"/> und begrenzt die Anzahl der gleichzeitig laufenden Threads
      /// <para>Achtung: <see cref="Queue(object)"/> "hängt" wenn die Maximalanzahl Threads gerade verwendet wird bis wieder ein Thread frei wird</para>
      /// </summary>
      class ThreadQueue {

         SemaphoreSlim semaphoreSlim;
         Action<object>? worker1;
         Action<object, object>? worker2;
         Action<object, object, object>? worker3;
         Action<object, object, object, object>? worker4;
         Action<object, object, object, object, object>? worker5;
         Action<int> readychanged;

         FSofTUtils.Threading.ThreadSafeIntVariable _ready = new FSofTUtils.Threading.ThreadSafeIntVariable();

         public int Ready => _ready.Value;


         ThreadQueue(int maxpar, Action<int> readychanged) {
            semaphoreSlim = new SemaphoreSlim(maxpar, maxpar);
            this.readychanged = readychanged;
         }

         public ThreadQueue(Action<object> workeraction, int maxpar, Action<int> readychanged) : this(maxpar, readychanged) {
            worker1 = workeraction;
         }

         public ThreadQueue(Action<object, object> workeraction, int maxpar, Action<int> readychanged) : this(maxpar, readychanged) {
            worker2 = workeraction;
         }

         public ThreadQueue(Action<object, object, object> workeraction, int maxpar, Action<int> readychanged) : this(maxpar, readychanged) {
            worker3 = workeraction;
         }

         public ThreadQueue(Action<object, object, object, object> workeraction, int maxpar, Action<int> readychanged) : this(maxpar, readychanged) {
            worker4 = workeraction;
         }

         public ThreadQueue(Action<object, object, object, object, object> workeraction, int maxpar, Action<int> readychanged) : this(maxpar, readychanged) {
            worker5 = workeraction;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="datarray">i.A. Array; sonst Einzelparameter</param>
         /// <returns></returns>
         public bool Queue(object datarray) {
            if (datarray != null &&
                datarray is object[]) {
               semaphoreSlim.Wait();
               ThreadPool.QueueUserWorkItem(wrapper,
                                            datarray is object[]?
                                                datarray :
                                                new object[] { datarray });
               return true;
            }
            return false;
         }

         public bool Queue(object arg1, object arg2) => Queue(new object[] { arg1, arg2 });
         public bool Queue(object arg1, object arg2, object arg3) => Queue(new object[] { arg1, arg2, arg3 });
         public bool Queue(object arg1, object arg2, object arg3, object arg4) => Queue(new object[] { arg1, arg2, arg3, arg4 });
         public bool Queue(object arg1, object arg2, object arg3, object arg4, object arg5) => Queue(new object[] { arg1, arg2, arg3, arg4, arg5 });

         void wrapper(object? datarray) {
#pragma warning disable CS8600 // Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
            object[] data = (object[])datarray;
#pragma warning restore CS8600 // Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
            switch (data.Length) {
               case 1: worker1?.Invoke(data[0]); break;
               case 2: worker2?.Invoke(data[0], data[1]); break;
               case 3: worker3?.Invoke(data[0], data[1], data[2]); break;
               case 4: worker4?.Invoke(data[0], data[1], data[2], data[3]); break;
               case 5: worker5?.Invoke(data[0], data[1], data[2], data[3], data[4]); break;
            }
            semaphoreSlim.Release();
            readychanged(_ready.Increment());
         }

      }


      #region PictureData

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

         FSofTUtils.Threading.ThreadSafeVariable<Image?> _thumbnail = new FSofTUtils.Threading.ThreadSafeVariable<Image?>(null);

         /// <summary>
         /// (kleines) Bild für die Anzeige im <see cref="ListViewItem"/>
         /// </summary>
         public Image? Thumbnail {
            get => _thumbnail.Value;
            protected set => _thumbnail.Value = value;
         }

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
         public string UserComment { get; protected set; } = string.Empty;

         /// <summary>
         /// Daten und/oder Dateiname wurden geändert
         /// </summary>
         public bool Changed => DataChanged || FilenameChanged;

         /// <summary>
         /// Daten wurden geändert
         /// </summary>
         public bool DataChanged =>
                           orgData.DateTime != DateTime ||
                           orgData.OriginalDateTime != OriginalDateTime ||
                           !equalNanDouble(orgData.Latitude, Latitude) ||
                           !equalNanDouble(orgData.Longitude, Longitude) ||
                           !equalNanDouble(orgData.Direction, Direction) ||
                           orgData.UserComment != UserComment;

         bool equalNanDouble(double x, double y) {
            if (double.IsNaN(x) == double.IsNaN(y)) {
               if (double.IsNaN(x))
                  return true;
               else
                  return x == y;
            }
            return false;
         }

         /// <summary>
         /// Dateiname wurde geändert
         /// </summary>
         public bool FilenameChanged => orgData.FileName != FileName;

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
         /// <see cref="ListView"/> des zugehöriges <see cref="ListViewItem"/> (kann NULL sein)
         /// </summary>
         public ListView? ListView => ListViewItem.ListView;

         /// <summary>
         /// zum Speichern der Originaldaten
         /// </summary>
         class OrgData {

            public string FileName = string.Empty;
            public DateTime DateTimeFile;
            public DateTime DateTime;
            public DateTime OriginalDateTime;
            public double Latitude;
            public double Longitude;
            public double Direction;
            public string UserComment = string.Empty;

         }

         /// <summary>
         /// Originaldaten
         /// </summary>
         OrgData orgData = new OrgData();

         /// <summary>
         /// für das Einlesen des <see cref="Thumbnail"/>
         /// </summary>
         SemaphoreSlim _isOnCreatingThumbnail = new SemaphoreSlim(1, 1);


         /// <summary>
         /// ermittelt alle notwendigen Daten, liest aber noch nicht das Bild (erzeugt also noch kein <see cref="Thumbnail"/>)
         /// </summary>
         /// <param name="filename"></param>
         /// <param name="readdata">sofort die Daten einlesen</param>
         public PictureData(string filename, bool readdata = false) {
            orgData.FileName = FileName = filename;
            DateTimeFile = File.GetLastWriteTime(FileName);

            ListViewItem = new ListViewItem() {
               Tag = this,
               ToolTipText = Path.GetFileName(FileName),
               //Text = text,
               ImageIndex = 0,
            };
            while (ListViewItem.SubItems.Count < SUBIDX_LAST + 1)
               ListViewItem.SubItems.Add(string.Empty);

            if (readdata)
               ReadData();
         }

         public void ReadData() {
            getActualExifData();
            noticeActualData();
            //setSubItemsText();
            setSubItemsTextTS();
         }

         /// <summary>
         /// erzeugt den <see cref="Thumbnail"/> aus den Dateidaten
         /// </summary>
         /// <param name="thumbnailsize"></param>
         public void CreateThumbnail(Size thumbnailsize) {
            _isOnCreatingThumbnail.Wait();
            if (Thumbnail == null)
               try {
                  using (Image? img = ReadImage(orgData.FileName)) {
                     if (img != null)
                        Thumbnail = createThumbnail(img, thumbnailsize.Width, thumbnailsize.Height);
                  }
               } catch (Exception ex) {
                  Debug.WriteLine(">>> CreateThumbnail Ex: " + ex);
               } finally {
                  _isOnCreatingThumbnail.Release();
               }
         }

         /// <summary>
         /// geografische Länge und Breite werden geändert
         /// </summary>
         /// <param name="longitude"></param>
         /// <param name="latitude"></param>
         public void ChangeLonLat(double longitude, double latitude) {
            Longitude = longitude;
            Latitude = latitude;
            setSubItemsText();
         }

         /// <summary>
         /// der Dateiname wir geändert (Originalpfad + <paramref name="poorfilename"/>)
         /// </summary>
         /// <param name="poorfilename"></param>
         public void ChangeFilename(string poorfilename) {
            string? dirname = Path.GetDirectoryName(orgData.FileName);
            FileName = Path.Combine(dirname != null ? dirname : string.Empty, poorfilename);
            setSubItemsText();
         }

         /// <summary>
         /// der Dateikommentar wird geändert
         /// </summary>
         /// <param name="comment"></param>
         public void ChangeUserComment(string comment) {
            UserComment = comment;
            setSubItemsText();
         }

         /// <summary>
         /// das Bilddatum wird geändert
         /// </summary>
         /// <param name="comment"></param>
         public void ChangeOriginalDateTime(DateTime dateTime) {
            if (OriginalDateTime != dateTime) {
               OriginalDateTime = dateTime;
               setSubItemsText();
            }
         }

         /// <summary>
         /// wenn Daten geändert wurden (<see cref="Changed"/>==true) wird die Datei neu gespeichert
         /// </summary>
         /// <param name="preserveDateTimeFile"></param>
         /// <returns></returns>
         public bool Save(bool preserveDateTimeFile = false) {
            if (Changed) {
               DateTime orgDateTime = File.GetLastWriteTime(orgData.FileName);
               ExifGeo exifGeo = new ExifGeo(orgData.FileName);

               bool set1 = exifGeo.SetLatLon(Latitude, Longitude, true);
               bool set2 = exifGeo.SetUserComment(UserComment);
               bool set3 = exifGeo.SetDateTimeOriginal(OriginalDateTime);

               if (set1 ||
                   set2 ||
                   set3 ||
                   orgData.FileName != FileName) {
                  if (set1 ||
                      set2 ||
                      set3) {
                     exifGeo.SaveImage(orgData.FileName);
                     getActualExifData();
                     if (preserveDateTimeFile)
                        File.SetLastWriteTime(orgData.FileName, orgDateTime);
                     else
                        DateTimeFile = File.GetLastWriteTime(orgData.FileName);
                     noticeActualData();
                  }
                  if (orgData.FileName != FileName) {
                     File.Move(orgData.FileName, FileName);
                     orgData.FileName = FileName;
                  }
               }
               return true;
            }
            return false;
         }

         /// <summary>
         /// verschiebt die Datei in das Verzeichnis <paramref name="newpath"/> und ändert bei Bedarf den Namen zu <see cref="FileName"/>
         /// </summary>
         /// <param name="newpath"></param>
         public void MoveAndRename(string newpath) {
            string newfilename = Path.Combine(newpath, Path.GetFileName(FileName));
            File.Move(orgData.FileName, newfilename);
            orgData.FileName = FileName = newfilename;
         }

         /// <summary>
         /// das Bild wird aus der Datei eingelesen
         /// </summary>
         /// <returns></returns>
         public Image? ReadImage() => ReadImage(orgData.FileName);

         /// <summary>
         /// das Bild wird aus dieser Datei eingelesen
         /// </summary>
         /// <param name="filename"></param>
         /// <returns></returns>
         public static Image? ReadImage(string filename) {
            byte[]? buffer = null;
            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) { // nur zum lesen öffnen mit read-sharing für andere
               buffer = new byte[stream.Length];
               if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                  throw new Exception(nameof(PictureManager) + "." +
                                      nameof(ReadImage) +
                                      "(): Fehler beim Lesen von '" + filename + "'.");
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
         /// akt. Daten in <see cref="orgData"/> speichern
         /// </summary>
         void noticeActualData() {
            orgData.FileName = FileName;
            orgData.DateTimeFile = DateTimeFile;
            orgData.DateTime = DateTime;
            orgData.OriginalDateTime = OriginalDateTime;
            orgData.Latitude = Latitude;
            orgData.Longitude = Longitude;
            orgData.Direction = Direction;
            orgData.UserComment = UserComment;
         }

         /// <summary>
         /// alle EXIF-Daten werden eingelesen
         /// </summary>
         void getActualExifData() {
            ExifGeo exifGeo = new ExifGeo(FileName);

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

            ListViewItem.SubItems[SUBIDX_PICTURE].Text = string.Empty;
            ListViewItem.SubItems[SUBIDX_FILENAME].Text = textprefix + Path.GetFileName(FileName) + textpostfix;
            ListViewItem.SubItems[SUBIDX_FILEDATE].Text = textprefix + DateTimeFile.ToString("G") + textpostfix;
            ListViewItem.SubItems[SUBIDX_ORIGINALDATE].Text = textprefix + OriginalDateTime.ToString("G") + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEOCOORD].Text = double.IsNaN(Longitude) || double.IsNaN(Latitude) ? textprefix : string.Format("{0:N6}° {1:N6}°", Longitude, Latitude) + textpostfix;
            ListViewItem.SubItems[SUBIDX_GEODIRECT].Text = double.IsNaN(Direction) ? "" : textprefix + string.Format("{0:N0}°", Direction) + textpostfix;
            ListViewItem.SubItems[SUBIDX_USERCOMMENT].Text = textprefix + UserComment + textpostfix;
         }

         /// <summary>
         /// threadsicheres <see cref="setSubItemsText"/>
         /// </summary>
         void setSubItemsTextTS() {
            if (ListView != null && ListView.InvokeRequired)
               ListView.Invoke(setSubItemsText);
            else
               setSubItemsText();
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

      #endregion


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
      public event EventHandler<PictureDataListEventArgs>? OnSelectPictures;

      /// <summary>
      /// Die Auswahl von (Bild-)Dateien wurde beendet.
      /// </summary>
      public event EventHandler<PictureDataListEventArgs>? OnDeselectPictures;

      /// <summary>
      /// Eine (Bild-)Datei sollte extern angezeigt werden.
      /// </summary>
      public event EventHandler<PictureDataEventArgs>? OnShowExtern;

      /// <summary>
      /// Für die (Bild-)Datei werden neue Geodaten gewünscht.
      /// </summary>
      public event EventHandler<PictureDataEventArgs>? OnNeedNewData;

      /// <summary>
      /// Der Filter wurde geändert.
      /// </summary>
      public event EventHandler<EventArgs>? OnNewFiltered;

      /// <summary>
      /// Die Sortierung wurde geändert.
      /// </summary>
      public event EventHandler<EventArgs>? OnNewSorted;

      #endregion

      #region Comparer for ListView

      private class ListViewBaseComparer : System.Collections.IComparer {

         public Size ImgSize = Size.Empty;

         public ListViewBaseComparer(Size imgsize) {
            ImgSize = imgsize;
         }

         public int Compare(object? x, object? y) {
            PictureData? p1 = (x != null && (x as ListViewItem).Tag != null) ? (x as ListViewItem).Tag as PictureData : null;
            PictureData? p2 = (y != null && (y as ListViewItem).Tag != null) ? (y as ListViewItem).Tag as PictureData : null;
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
            foreach (PictureData? pd in dataCacheAll)
               if (pd != null && pd.Changed)
                  count++;
            return count;
         }
      }

      /// <summary>
      /// Anzahl der Bilder in der Ansicht
      /// </summary>
      public int ShownPictures => dataCache4Showing.Count;

      public bool IsFocused => listView1.Focused;

      /// <summary>
      /// ALLE <see cref="PictureData"/>
      /// </summary>
      List<PictureData?> dataCacheAll = new List<PictureData?>();

      /// <summary>
      /// akt. gefilterte <see cref="PictureData"/>
      /// </summary>
      List<PictureData?> dataCache4Showing = new List<PictureData?>();

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


      static class ListViewStatus {

         static readonly Color colBackWithFocus = SystemColors.Window;
         static readonly Color colBackWithoutFocus = SystemColors.Control;
         static readonly Color colBackOnLoad = Color.FromArgb(255, 200, 200);


         static FSofTUtils.Threading.ThreadSafeBoolVariable _isOnLoad = new FSofTUtils.Threading.ThreadSafeBoolVariable();

         static public bool IsOnLoad {
            get => _isOnLoad.Value;
            set => _isOnLoad.Value = value;
         }

         static FSofTUtils.Threading.ThreadSafeBoolVariable _isFocused = new FSofTUtils.Threading.ThreadSafeBoolVariable();

         static public bool IsFocused {
            get => _isFocused.Value;
            set => _isFocused.Value = value;
         }

         static Color backColor {
            get {
               if (IsOnLoad)
                  return colBackOnLoad;
               else
                  return IsFocused ? colBackWithFocus : colBackWithoutFocus;
            }
         }

         static public void SetStatus(Control ctrl) {
            //ctrl.BackColor = Color.White;
            FSofTUtils.Threading.ThreadsafeInvoker.InvokeControlPropertyWriter(ctrl, nameof(ListView.BackColor), backColor);
         }

      }

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
         listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

         /*
            System.NotSupportedException
              HResult=0x80131515
              Nachricht = VirtualMode wird in der Tile-Ansicht nicht unterstützt.
              Quelle = System.Windows.Forms
              Stapelüberwachung:
               bei System.Windows.Forms.ListView.set_VirtualMode(Boolean value)
               bei GpxViewer.PictureEdit.PictureManager.PictureManager_Load(Object sender, EventArgs e) in C:\Users\puf\Daten\Programmierung lokal\GpxViewer\GpxViewer\PictureEdit\PictureManager.cs: Zeile886
               bei System.Windows.Forms.Control.CreateControl(Boolean ignoreVisible)
               bei System.Windows.Forms.Control.CreateControl(Boolean ignoreVisible)
               bei System.Windows.Forms.Control.CreateControl()
               bei System.Windows.Forms.Control.SetVisibleCore(Boolean value)
          */
         //listView1.VirtualListSize = 0;
         //listView1.VirtualMode = true;
         //listView1.RetrieveVirtualItem += ListView1_RetrieveVirtualItem;

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

         listView1.GotFocus += (s, e) => {
            ListViewStatus.IsFocused = true;
            ListViewStatus.SetStatus(listView1);
         };
         listView1.LostFocus += (s, e) => {
            ListViewStatus.IsFocused = false;
            ListViewStatus.SetStatus(listView1);
         };
         if (!listView1.Focused)
            listView1.Focus();
         listView1.KeyDown += (s, e) => {
            //Debug.WriteLine("::: KeyValue=" + e.KeyValue + ", KeyData=" + e.KeyData + ", KeyCode=" + e.KeyCode + ", Handled=" + e.Handled);
            if (listView1.Focused) {
               // Cursortasten + Ctrl sollen ignoriert werden!
               switch (e.KeyData) {
                  case Keys.Control | Keys.Down:
                  case Keys.Control | Keys.Up:
                  case Keys.Control | Keys.Left:
                  case Keys.Control | Keys.Right:
                     e.Handled = true;
                     break;
               }
            }
         };

         pictureFilter = PictureFilter.None;
         pictureSort = PictureSort.FilenameAscending;

         toolStripButton_SaveGpx.Enabled = false;
         toolStripButton_SaveAll.Enabled = false;
      }

      private void pictureBox1_DoubleClick(object sender, EventArgs e) => showFirstSelectedPictureExtern(false);

      //private void ListView1_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) {
      //   ListView? listView = sender as ListView;
      //   if (listView != null) {
      //      int idx = e.ItemIndex;

      //      e.Item = listView.Items[idx];


      //   }
      //}

      private void ListView1_KeyDown(object? sender, KeyEventArgs e) {
         //Debug.WriteLine("::: KeyValue=" + e.KeyValue + ", KeyData=" + e.KeyData + ", KeyCode=" + e.KeyCode + ", Handled=" + e.Handled);
         if (listView1.Focused) {
            switch (e.KeyData) {
               case Keys.Control | Keys.Down:
               case Keys.Control | Keys.Up:
               case Keys.Control | Keys.Left:
               case Keys.Control | Keys.Right:
                  e.Handled = true;
                  break;
            }
         }

      }

      #region public-functions

      /// <summary>
      /// neuen Dateipfad setzen
      /// </summary>
      /// <param name="path"></param>
      /// <param name="recursesubdirectories"></param>
      public async Task SetPicturePath(string path, bool recursesubdirectories) {
         string newpath = Path.GetFullPath(path);
         if (!string.IsNullOrEmpty(newpath) &&
             Directory.Exists(newpath)) {
            ActualPicturePath = newpath;
            listView1.Items.Clear();   // als Zeichen, dass etwas passiert
            UseWaitCursor = true;

            await fillDataCacheAsync(ActualPicturePath, recursesubdirectories);
            fillListView(listView1, pictureFilter, pictureSort);
            setStatusSaveButtons();

            UseWaitCursor = false;
            if (Parent is Form)
               Parent.Text = "Bilder: " + ActualPicturePath;
         }
      }

      /// <summary>
      /// Kommentar eines Bildes ändern
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void EditPictureComment(int idx = -1) => editPictureText(idx, true);

      /// <summary>
      /// Dateiname eines Bildes ändern
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void EditPictureFilename(int idx = -1) => editPictureText(idx, false);

      public void EditPictureDateTime(int idx = -1) {
         PictureData? pd = getPictureData(idx);
         if (pd != null) {
            Point pt = listView1.PointToScreen(pd.ListViewItem.Bounds.Location);
            if (pd.ListViewItem.ListView.View == View.Tile) {
               pt.Offset(5, 5);
            } else if (pd.ListViewItem.ListView.View == View.Details) {
               pt = listView1.PointToScreen(pd.ListViewItem.SubItems[PictureData.SUBIDX_ORIGINALDATE].Bounds.Location);
            }

            FormEditPictureDateTime form = new FormEditPictureDateTime() {
               Location = pt,
               DateTime = pd.OriginalDateTime,
               IsValid = pd.OriginalDateTime != DateTime.MinValue
            };
            DateTime olddt = form.DateTime;
            bool oldIsValid = form.IsValid;
            if (form.ShowDialog() == DialogResult.OK &&
                (olddt != (form.IsValid ? form.DateTime : DateTime.MinValue) ||
                 oldIsValid != form.IsValid)) {
               pd.ChangeOriginalDateTime(form.IsValid ? form.DateTime : DateTime.MinValue);
               if (pd.Changed) {
                  showPictureCount();
                  setStatusSaveButtons();
               }
            }
         }
      }

      /// <summary>
      /// Bild speichern
      /// </summary>
      /// <param name="all">alle ungespeicherten Items oder nur 1</param>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void SavePicture(bool all, int idx = -1) {
         Cursor orgcursor = Cursor;
         Cursor = Cursors.WaitCursor;

         bool ok = true;

         Dictionary<string, PictureData> savelst = new Dictionary<string, PictureData>();
         if (all) {
            string filename = string.Empty;
            try {
               foreach (PictureData? pd in dataCacheAll) {
                  if (pd != null) {
                     filename = pd.FileName.ToLower();
                     savelst.Add(filename, pd);
                  }
               }
            } catch {
               Cursor = orgcursor;
               MessageBox.Show("Dateinamenskonflikt bei: '" + filename + "'",
                               "FEHLER",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Stop);
               Cursor = Cursors.WaitCursor;
               ok = false;
               ok = false;
            }
         } else {
            PictureData? pd = getPictureData(idx);
            if (pd != null)
               savelst.Add(pd.FileName, pd);
         }

         if (ok) {
            // ev. Dateien umbenennen
            List<PictureData> listRename = new List<PictureData>();
            foreach (var item in savelst)
               if (item.Value.FilenameChanged)
                  listRename.Add(item.Value);
            if (listRename.Count > 0) {
               try {
                  string? orgpath = Path.GetDirectoryName(Path.GetFullPath(listRename[0].FileName));
                  if (orgpath != null) {
                     string tmppath = string.Empty;
                     Random r = new Random();
                     do {
                        tmppath = Path.Combine(orgpath, "~" + r.Next(999999999).ToString());
                     } while (File.Exists(tmppath));
                     Directory.CreateDirectory(tmppath);

                     foreach (PictureData pd in listRename)
                        pd.MoveAndRename(tmppath);
                     foreach (PictureData pd in listRename)
                        pd.MoveAndRename(orgpath);

                     Directory.Delete(tmppath);
                  }
               } catch (Exception ex) {
                  Cursor = orgcursor;
                  MessageBox.Show("Beim Umbenennen:" + Environment.NewLine + Environment.NewLine + ex.Message,
                                  "FEHLER",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Stop);
                  Cursor = Cursors.WaitCursor;
                  ok = false;
               }
            }
         }

         if (ok)
            // normales speichern
            foreach (var item in savelst) {
               if (item.Value.Changed) {
                  try {
                     item.Value.Save();
                  } catch (Exception ex) {
                     Cursor = orgcursor;
                     MessageBox.Show("Beim Speichern von '" + item.Value.FileName + "':" + Environment.NewLine + Environment.NewLine + ex.Message,
                                     "FEHLER",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Stop);
                     Cursor = Cursors.WaitCursor;
                     break;
                  }
               }
            }

         showPictureCount();
         setStatusSaveButtons();

         PictureSort ps = pictureSort;
         pictureSort = PictureSort.None;
         pictureSort = ps;

         listView1.Refresh();
         Cursor = orgcursor;
      }

      /// <summary>
      /// Bildposition auf Karte anzeigen
      /// </summary>
      /// <param name="idx">Index des Items oder das selektierte Item</param>
      public void GoToPicturePosition(int idx = -1) {
         PictureData? pd = getPictureData(idx);
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
         PictureData? pd = getPictureData(idx);
         if (pd != null)
            OnNeedNewData?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.Longitude, pd.Latitude));
      }

      public void SetPicturePosition2(int idx = -1) {
         PictureData? pd = getPictureData(idx);
         if (pd != null)
            OnNeedNewData?.Invoke(this, new PictureDataEventArgs(pd.FileName, pd.DateTime));
      }

      public void SetPositionExtern(string filename, double longitude, double latitude) {
         if (!string.IsNullOrEmpty(filename) &&
             longitude != double.MinValue &&
             latitude != double.MinValue) {
            ListView lv = listView1;
            PictureData? pd = null;
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

      public async Task OpenPath() {
         // SendKeys.Send("{TAB}{TAB}{RIGHT}");

         folderBrowserDialog1.SelectedPath = ActualPicturePath;
         if (!cancelNewLoad() &&
             folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            await SetPicturePath(folderBrowserDialog1.SelectedPath, toolStripButton_WithSubDirs.Checked);
      }

      public void SwapView() => swapView(listView1);

      public async Task Reload() {
         if (!cancelNewLoad() &&
             !string.IsNullOrEmpty(ActualPicturePath))
            await SetPicturePath(ActualPicturePath, toolStripButton_WithSubDirs.Checked);
      }

      #endregion

      void saveAsGpx() {
         List<PictureData> dataList = new List<PictureData>();
         foreach (ListViewItem item in listView1.SelectedItems) {
            PictureData? pd = item.Tag as PictureData;
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
                  string? ev2 = ev[key] as string;
                  if (ev2 != null &&
                      dataList[0].FileName.StartsWith(ev2, StringComparison.CurrentCultureIgnoreCase)) {
                     if (ev2.Length > content.Length) {
                        content = ev2;
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

      /// <summary>
      /// Text für Kommentar oder Dateiname ändern
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="comment"></param>
      void editPictureText(int idx, bool comment) {
         PictureData? pd = getPictureData(idx);
         if (pd != null) {
            Point pt = listView1.PointToScreen(pd.ListViewItem.Bounds.Location);
            if (pd.ListViewItem.ListView.View == View.Tile) {
               pt.Offset(5, 5);
            } else if (pd.ListViewItem.ListView.View == View.Details) {
               pt = listView1.PointToScreen(pd.ListViewItem.SubItems[comment ?
                                                                        PictureData.SUBIDX_USERCOMMENT :
                                                                        PictureData.SUBIDX_FILENAME].Bounds.Location);
            }

            string filenameextension = comment ? string.Empty : Path.GetExtension(pd.FileName);

            FormEditPictureFilename form = new FormEditPictureFilename() {
               Location = pt,
               EditText = comment ?
                              pd.UserComment :
                              Path.GetFileNameWithoutExtension(pd.FileName),
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
                  string newfilename = form.EditText + filenameextension;
                  if (filenameIsValid(newfilename)) {
                     pd.ChangeFilename(newfilename);
                     if (form.EditText != "") {
                        proposalFilename.Remove(newfilename);
                        proposalFilename.Insert(0, newfilename);
                     }
                  } else
                     MessageBox.Show("Der Dateiname '" + form.EditText + "' ist nicht gültig.",
                                     "Achtung",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Exclamation);
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

#if DEBUG_EXT
      DateTime start;
#endif

      async Task fillDataCacheAsync(string path, bool recursesubdirectories) =>
         await Task.Run(() => fillDataCache(path, recursesubdirectories));

      /// <summary>
      /// fill <see cref="dataCacheAll"/> with filedata from path
      /// </summary>
      /// <param name="path"></param>
      /// <param name="recursesubdirectories"></param>
      void fillDataCache(string path, bool recursesubdirectories) {
#if DEBUG_EXT
         start = DateTime.Now;
         Debug.WriteLine(">>> fillDataCache START");
#endif
         List<string> pictfiles = new List<string>();

         // alle PNG- und JPG/JPEG-Dateien einsammeln
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

         dataCacheAll.Clear();
         for (int i = 0; i < pictfiles.Count; i++)
            dataCacheAll.Add(new PictureData(pictfiles[i]));

         Task.Run(() => {
            // Hintergund in den "Einlesemodus"
            ListViewStatus.IsOnLoad = true;
            ListViewStatus.SetStatus(listView1);

            ThreadQueue workerqueue = new ThreadQueue(
               (arg1, arg2, arg3) => {
                  if (arg1 is PictureData &&
                      arg2 is int &&
                      arg3 is int) {
                     try {
                        PictureData pd = (PictureData)arg1;
                        pd.ReadData();
                        pd.CreateThumbnail(new Size((int)arg2, (int)arg3));
                     } catch (Exception ex) {
#if DEBUG_EXT
                        Debug.WriteLine(">>> LoadAllDataThreadProc: " + ex.ToString());
#endif
                     }
                  }
               },
               Environment.ProcessorCount / 2,     // trial and error (16 logische Prozessoren)
               (r) => {
#if DEBUG_EXT  
                  if (r % 100 == 0)
                     Debug.WriteLine(">>> ThreadQueue Ready=" + r + ", " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif         
                  if (r == dataCacheAll.Count) {
#if DEBUG_EXT  
                     Debug.WriteLine(">>> fillDataCache ThreadQueue Work READY=" + r + ", " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif
                     // Hintergund wieder im "Normalmodus"
                     ListViewStatus.IsOnLoad = false;
                     ListViewStatus.SetStatus(listView1);
                  }
               });
#if DEBUG_EXT
            Debug.WriteLine(">>> fillDataCache ThreadQueue Fill START: " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif
            for (int i = 0; i < dataCacheAll.Count; i++)
               workerqueue.Queue(dataCacheAll[i], ThumbnailSize.Width, ThumbnailSize.Height);
#if DEBUG_EXT
            Debug.WriteLine(">>> fillDataCache ThreadQueue Fill END: " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif
         });

#if DEBUG_EXT
         Debug.WriteLine(">>> fillDataCache READY: " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif

      }

      /// <summary>
      /// fill <see cref="listView1"/> for given <see cref="PictureFilter"/> and <see cref="PictureSort"/>
      /// </summary>
      /// <param name="lv"></param>
      /// <param name="pictureFilter"></param>
      /// <param name="pictureSorting"></param>
      void fillListView(ListView lv, PictureFilter pictureFilter, PictureSort pictureSorting) {
#if DEBUG_EXT
         Debug.WriteLine(">>> fillListView START: " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif

         switch (pictureFilter) {
            case PictureFilter.None:
               dataCache4Showing = dataCacheAll;
               break;

            case PictureFilter.WithGeoData:
               dataCache4Showing = new List<PictureData?>();
               foreach (PictureData? pd in dataCacheAll)
                  if (pd != null && pd.HasGeoData)
                     dataCache4Showing.Add(pd);
               break;

            case PictureFilter.WithoutGeoData:
               dataCache4Showing = new List<PictureData?>();
               foreach (PictureData? pd in dataCacheAll)
                  if (pd != null && !pd.HasGeoData)
                     dataCache4Showing.Add(pd);
               break;
         }
         pictureSort = pictureSorting;

         lv.SuspendLayout();
         lv.BeginUpdate();
         lv.Items.Clear();

#if DEBUG_EXT
         Debug.WriteLine(">>> fillListView A: " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif

         foreach (PictureData? pd in dataCache4Showing)
            if (pd != null)
               lv.Items.Add(pd.ListViewItem);

#if DEBUG_EXT
         Debug.WriteLine(">>> fillListView READY: " + DateTime.Now.Subtract(start).TotalSeconds + "s");
#endif

         lv.ResumeLayout();
         lv.EndUpdate();

         if (lv.Items.Count > 0)
            lv.Items[0].Selected = true;

         toolStripStatusLabel_Path.Text = ActualPicturePath;
         showPictureCount();
      }

      #region ownerdraw listview

      private void ListView1_DrawItem(object? sender, DrawListViewItemEventArgs e) {
         ListView? lv = sender as ListView;
         PictureData? pd = e.Item.Tag != null ?
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

      void drawTileBackground(Graphics g, PictureData? pd, Rectangle bounds, bool selected) {
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
      void drawTileContent(Graphics g, PictureData? pd, Rectangle bounds, Font font) {
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

      private void ListView1_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e) {
         using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.LightGray, Color.DarkGray, LinearGradientMode.Vertical))
            e.Graphics.FillRectangle(brush, e.Bounds);
         using (Pen pen = new Pen(Color.Black))
            e.Graphics.DrawRectangle(pen, e.Bounds);
         e.DrawText(TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }

      void drawDetailBackground(Graphics g, ListViewItem? lvi, Rectangle bounds) {
         //e.DrawBackground();
         if (lvi != null)
            if (lvi.Selected) {
               using (SolidBrush brush = new SolidBrush(System.Drawing.SystemColors.Highlight))
                  g.FillRectangle(brush, bounds);
               using (Pen pen = new Pen(Color.Black))
                  g.DrawRectangle(pen, bounds);
            } else {
               PictureData? pd = lvi.Tag != null ?
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

      void drawDetailColPicture(Graphics g, ListViewItem? lvi, Rectangle bounds) {
         if (lvi != null) {
            using (SolidBrush brush = new SolidBrush(lvi.BackColor))
               g.FillRectangle(brush, bounds);
            using (Pen pen = new Pen(Color.Black, 1))
               g.DrawRectangle(pen, bounds);

            PictureData? pd = lvi.Tag as PictureData;
            if (pd != null) {
               Image? img = pd.Thumbnail;
               if (img != null) {
                  g.DrawImage(img,
                              bounds.Left + 1,
                              bounds.Top + 1,
                              (bounds.Height - 1) * img.Width / img.Height,
                              bounds.Height - 1);
               }
            }
         }
      }

      void drawDetailColText(Graphics g, ListViewItem.ListViewSubItem? lvsi, Rectangle bounds) {
         if (lvsi != null)
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
      private void ListView1_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e) {
         // draw content
         if (e.ColumnIndex > 0) {
            drawDetailBackground(e.Graphics, e.Item, e.Bounds);
            drawDetailColText(e.Graphics, e.SubItem, e.Bounds);
         }
      }

      #endregion

      void showPictureCount() =>
         toolStripStatusLabel_Count.Text = (ShownPictures != 1 ? ShownPictures + " Bilder" : "1 Bild") + " (" + UnsavedPictures + " nicht gespeichert)";

      PictureDataListEventArgs.PictureData[] lastSelectedPictureData = [];

      void listView1_SelectedIndexChanged(object? sender, EventArgs e) {
         OnDeselectPictures?.Invoke(this, new PictureDataListEventArgs(lastSelectedPictureData));

         toolStripStatusLabel_Filename.Text = "";
         PictureData? pd = getFirstSelectedPictureData(sender as ListView);
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

      //async void swapViewAsync(ListView lv) {
      //   Cursor orgcursor = Cursor;
      //   Cursor = Cursors.WaitCursor;

      //   await Task.Run(() => {
      //      swapView(lv);
      //   });

      //   Cursor = orgcursor;
      //}

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

      PictureDataListEventArgs.PictureData[] getSelectedPictureDataArray(ListView? lv) {
         if (lv != null) {
            PictureData[]? pd = getSelectedPictureDatas(lv);
            if (pd != null && pd.Length > 0) {
               PictureDataListEventArgs.PictureData[] dat = new PictureDataListEventArgs.PictureData[pd.Length];
               for (int i = 0; i < pd.Length; i++)
                  dat[i] = new PictureDataListEventArgs.PictureData(pd[i].FileName, pd[i].Longitude, pd[i].Latitude);
               return dat;
            }
         }
         return Array.Empty<PictureDataListEventArgs.PictureData>();
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
      PictureData? getFirstSelectedPictureData(ListView? lv) {
         if (lv != null &&
             lv.SelectedItems.Count > 0 &&
             lv.SelectedItems[0].Tag != null)
            return lv.SelectedItems[0].Tag as PictureData;
         return null;
      }

      /// <summary>
      /// liefert die Daten aller ausgewählten Bilder oder null
      /// </summary>
      /// <param name="lv"></param>
      /// <returns></returns>
      PictureData[]? getSelectedPictureDatas(ListView lv) {
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
                  dat[count++] = (PictureData)item.Tag;
            return dat;
         }
         return null;
      }

      /// <summary>
      /// liefert die <see cref="PictureData"/> zum Item mit dem Index oder zum 1. ausgewählten Item oder null
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      PictureData? getPictureData(int idx) {
         PictureData? pd = 0 <= idx &&
                          idx < listView1.Items.Count ?
                                    listView1.Items[idx].Tag as PictureData :
                                    null;
         if (pd == null)
            pd = listView1.SelectedItems.Count == 1 ?
                     listView1.SelectedItems[0].Tag as PictureData :
                     null;
         return pd;
      }

      void showFirstSelectedPictureExtern(bool withchoose) {
         PictureData? pd = getFirstSelectedPictureData(listView1);
         if (pd != null)
            showPictureExtern(pd.FileName, withchoose);
      }

      /// <summary>
      /// Anzeige der Datei im externen Standardprogramm
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="withchoose">mit Programmauswahl</param>
      void showPictureExtern(string filename, bool withchoose) {
         //System.Diagnostics.Process.Start(pd.FileName);
         /*
System.ComponentModel.Win32Exception
HResult=0x80004005
Nachricht = An error occurred trying to start process 'C:\Users\puf\Pictures\2024_03 Vietnam, Kambodscha\20240302_194032.jpg' with working directory 'C:\Users\puf\Daten\Programmierung lokal\GpxViewer\bin\Debug\net9.0-windows7.0'. The specified executable is not a valid application for this OS platform.
Quelle = System.Diagnostics.Process
Stapelüberwachung:
bei System.Diagnostics.Process.StartWithCreateProcess(ProcessStartInfo startInfo)
bei System.Diagnostics.Process.Start(ProcessStartInfo startInfo)
bei GpxViewer.PictureEdit.PictureManager.pictureBox1_DoubleClick(Object sender, EventArgs e) in C:\Users\puf\Daten\Programmierung lokal\GpxViewer\GpxViewer\PictureEdit\PictureManager.cs: Zeile2043
bei System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
bei System.Windows.Forms.Control.WndProc(Message& m)
bei System.Windows.Forms.NativeWindow.Callback(HWND hWnd, MessageId msg, WPARAM wparam, LPARAM lparam)
          */

         if (withchoose)
            Process.Start("rundll32.exe", "shell32.dll, OpenAs_RunDLL " + filename);
         else
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
      }

      bool filenameIsValid(string fileName) {
         FileInfo? fi = null;
         try {
            fi = new FileInfo(fileName);
         } catch (ArgumentException) { } catch (PathTooLongException) { } catch (NotSupportedException) { }
         if (!ReferenceEquals(fi, null) &&
             !File.Exists(fileName))
            return true;
         return false;
      }

      #region Contextmenu ListView

      int itemidx4contextmenu = -1;

      private void contextMenuStripListView_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         e.Cancel = true;
         itemidx4contextmenu = listviewItemIdx4Point(MousePosition);
         if (0 <= itemidx4contextmenu) {
            PictureData? pd = listView1.Items[itemidx4contextmenu].Tag as PictureData;
            if (pd != null) {
               e.Cancel = false;
               ToolStripMenuItemSave.Enabled = pd.Changed;
            }
         }
      }

      public void ToolStripMenuItemSave_Click(object sender, EventArgs e) {
         SavePicture(false, itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      public void ToolStripMenuItemShow_Click(object sender, EventArgs e) {
         GoToPicturePosition(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      public void ToolStripMenuItemSet_Click(object sender, EventArgs e) {
         SetPicturePosition(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      public void ToolStripMenuItemSet2_Click(object sender, EventArgs e) {
         SetPicturePosition2(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      public void ToolStripMenuItemEditComment_Click(object sender, EventArgs e) {
         EditPictureComment(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      public void ToolStripMenuItemEditFilename_Click(object sender, EventArgs e) {
         EditPictureFilename(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      private void ToolStripMenuItemEditDateTime_Click(object sender, EventArgs e) {
         EditPictureDateTime(itemidx4contextmenu);
         itemidx4contextmenu = -1;
      }

      #endregion

      #region Contextmenu Picturebox

      private void contextMenuStripPicturebox_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
         if (getFirstSelectedPictureData(listView1) == null)
            e.Cancel = true;
      }

      private void toolStripMenuItemShowPictExtern_Click(object sender, EventArgs e) => showFirstSelectedPictureExtern(false);

      private void toolStripMenuItemShowPictExternExt_Click(object sender, EventArgs e) => showFirstSelectedPictureExtern(true);

      #endregion

      #region Toolbar

      private void toolStripButton_SwapView_Click(object? sender, EventArgs? e) => swapView(listView1);

      private void ToolStripMenuItem_ViewAll_Click(object sender, EventArgs e) => pictureFilter = PictureFilter.None;

      private void ToolStripMenuItem_ViewWithGeo_Click(object sender, EventArgs e) => pictureFilter = PictureFilter.WithGeoData;

      private void ToolStripMenuItem_ViewWithoutGeo_Click(object sender, EventArgs e) => pictureFilter = PictureFilter.WithoutGeoData;

      private void ToolStripMenuItem_FilenameAsc_Click(object sender, EventArgs e) => pictureSort = PictureSort.FilenameAscending;

      private void ToolStripMenuItem_FilenameDesc_Click(object sender, EventArgs e) => pictureSort = PictureSort.FilenameDescending;

      private void ToolStripMenuItem_FiledateAsc_Click(object sender, EventArgs e) => pictureSort = PictureSort.FiledateAscending;

      private void ToolStripMenuItem_FiledateDesc_Click(object sender, EventArgs e) => pictureSort = PictureSort.FiledateDescending;

      private void ToolStripMenuItem_GeodateAsc_Click(object sender, EventArgs e) => pictureSort = PictureSort.OriginaldateAscending;

      private void ToolStripMenuItem_GeodateDesc_Click(object sender, EventArgs e) => pictureSort = PictureSort.OriginaldateDescending;

      private void toolStripButton_OpenPath_Click(object? sender, EventArgs? e) => OpenPath();

      private void toolStripButton_WithSubDirs_Click(object sender, EventArgs e) => withSubDirs(sender as ToolStripButton);

      private void toolStripButton_SaveAll_Click(object? sender, EventArgs? e) => SavePicture(true);

      private void toolStripButton_SaveGpx_Click(object sender, EventArgs e) => saveAsGpx();

      private void toolStripButton_Reload_Click(object? sender, EventArgs? e) => Reload();

      bool internToolstripButtonSet = false;

      async Task withSubDirs(ToolStripButton? btn) {
         if (btn != null &&
             !internToolstripButtonSet) {
            if (!cancelNewLoad())
               await SetPicturePath(ActualPicturePath, btn.Checked);
            else {
               internToolstripButtonSet = true;
               btn.Checked = !btn.Checked;
               internToolstripButtonSet = false;
            }
         }
      }

      bool cancelNewLoad() {
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

      #endregion

   }
}
