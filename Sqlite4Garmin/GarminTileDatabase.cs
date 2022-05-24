using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Sqlite4Garmin {

   // https://docs.microsoft.com/de-de/dotnet/standard/data/sqlite/?tabs=netcore-cli

   /* gelegentlich:
         System.AccessViolationException
           HResult=0x80004003
           Nachricht = Es wurde versucht, im geschützten Speicher zu lesen oder zu schreiben. Dies ist häufig ein Hinweis darauf, dass anderer Speicher beschädigt ist.
           Quelle = mscorlib
           Stapelüberwachung:
            at System.Text.UTF8Encoding.GetCharCount(Byte* bytes, Int32 count, DecoderNLS baseDecoder)
            at System.String.CreateStringFromEncoding(Byte* bytes, Int32 byteLength, Encoding encoding)
            at SQLitePCL.utf8z.utf8_to_string()
            at SQLitePCL.raw.sqlite3_prepare_v2(sqlite3 db, String sql, sqlite3_stmt& stmt, String& tail)
            at Microsoft.Data.Sqlite.SqliteCommand.<PrepareAndEnumerateStatements>d__64.MoveNext()
            at Microsoft.Data.Sqlite.SqliteCommand.<GetStatements>d__54.MoveNext()
            at Microsoft.Data.Sqlite.SqliteDataReader.NextResult()
            at Microsoft.Data.Sqlite.SqliteCommand.ExecuteReader(CommandBehavior behavior)
            at Microsoft.Data.Sqlite.SqliteCommand.ExecuteScalar()
            at Microsoft.Data.Sqlite.SqliteDataReader.GetSchemaTable()
            at System.Data.ProviderBase.SchemaMapping..ctor(DataAdapter adapter, DataSet dataset, DataTable datatable, DataReaderContainer dataReader, Boolean keyInfo, SchemaType schemaType, String sourceTableName, Boolean gettingData, DataColumn parentChapterColumn, Object parentChapterValue)
            at System.Data.Common.DataAdapter.FillMappingInternal(DataSet dataset, DataTable datatable, String srcTable, DataReaderContainer dataReader, Int32 schemaCount, DataColumn parentChapterColumn, Object parentChapterValue)
            at System.Data.Common.DataAdapter.FillMapping(DataSet dataset, DataTable datatable, String srcTable, DataReaderContainer dataReader, Int32 schemaCount, DataColumn parentChapterColumn, Object parentChapterValue)
            at System.Data.Common.DataAdapter.FillFromReader(DataSet dataset, DataTable datatable, String srcTable, DataReaderContainer dataReader, Int32 startRecord, Int32 maxRecords, DataColumn parentChapterColumn, Object parentChapterValue)
            at System.Data.Common.DataAdapter.Fill(DataTable[] dataTables, IDataReader dataReader, Int32 startRecord, Int32 maxRecords)
            at System.Data.DataTable.Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler)
      
      beim Füllen der DataTable

      z.Z. NICHT vermeidbar

    */



   /// <summary>
   /// Datenbank für ein einziges Garmin-Tile
   /// </summary>
   public class GarminTileDatabase : IDisposable {

      /*
       *    1 SQLite-DB je Garmin-Tile mit Namen des Tiles
       *    
       *    ev.:
       *       CREATE TABLE maininfo (
       *             id INTEGER PRIMARY KEY, 
       *             tilename TEXT,
       *             description TEXT,
       *             north REAL,
       *             east REAL,
       *             south REAL,
       *             west REAL
       *       )
       *             
       *       CREATE TABLE level (
       *             level INTEGER PRIMARY KEY, 
       *             bits INTEGER
       *       )
       *          
       *    CREATE TABLE subdivinfo (
       *          sd INTEGER PRIMARY KEY, 
       *          level INTEGER,
       *          north REAL,
       *          east REAL,
       *          south REAL,
       *          west REAL
       *    )
       *          
       *    CREATE TABLE points (
       *          id INTEGER PRIMARY KEY, 
       *          sd INTEGER,
       *          type INTEGER,
       *          label TEXT,
       *          lon REAL,
       *          lat REAL
       *    )
       *          
       *    CREATE TABLE lines (
       *          id INTEGER PRIMARY KEY, 
       *          sd INTEGER,
       *          type INTEGER,
       *          label TEXT,
       *          north REAL,
       *          east REAL,
       *          south REAL,
       *          west REAL,
       *          coord BLOB
       *    )
       *          
       *    CREATE TABLE areas (
       *          id INTEGER PRIMARY KEY, 
       *          sd INTEGER,
       *          type INTEGER,
       *          label TEXT,
       *          north REAL,
       *          east REAL,
       *          south REAL,
       *          west REAL,
       *          coord BLOB
       *    )
       * 
       */

      /// <summary>
      /// SQLite-Datei
      /// </summary>
      public string DBFile { get; protected set; }

      /// <summary>
      /// Ist die Datenbank geöffnet?
      /// </summary>
      bool isOpen {
         get {
            return connection != null;
         }
      }

      SqliteConnection connection = null;

      SqliteCommand command = null;


      public GarminTileDatabase(string sqlitefile) {
         DBFile = sqlitefile;
         if (!existDB())
            createDatabase();
      }

      /// <summary>
      /// liefert true, wenn die SQLite-Datei existiert und Tabelle subdivinfo enthält
      /// </summary>
      /// <returns></returns>
      bool existDB() {
         if (File.Exists(DBFile)) {
            try {
               DataTable dat = sqlSelect("SELECT * FROM subdivinfo LIMIT 1");
            } catch {
               return false;
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// öffnet die Datenbank
      /// </summary>
      void open() {
         string connstr = "Data Source=" + DBFile;
         connection = new SqliteConnection(connstr);
         connection.Open();
         command = new SqliteCommand("", connection);
      }

      /// <summary>
      /// schließt die Datenbank
      /// </summary>
      void close() {
         if (isOpen) {
            connection.Close();
            connection.Dispose();
            connection = null;
            command.Dispose();
         }
      }

      void createDatabase() {
         sqlExecute("DROP TABLE IF EXISTS subdivinfo");
         sqlExecute("CREATE TABLE subdivinfo (sd INTEGER PRIMARY KEY, level INTEGER, north REAL, east REAL, south REAL, west REAL)");
         sqlExecute("DROP TABLE IF EXISTS points");
         sqlExecute("CREATE TABLE points (id INTEGER PRIMARY KEY, sd INTEGER, type INTEGER, label TEXT, lon REAL, lat REAL)");
         sqlExecute("DROP TABLE IF EXISTS lines");
         sqlExecute("CREATE TABLE lines (id INTEGER PRIMARY KEY, sd INTEGER, type INTEGER, label TEXT, north REAL, east REAL, south REAL, west REAL, coord BLOB)");
         sqlExecute("DROP TABLE IF EXISTS areas");
         sqlExecute("CREATE TABLE areas (id INTEGER PRIMARY KEY, sd INTEGER, type INTEGER, label TEXT, north REAL, east REAL, south REAL, west REAL, coord BLOB)");
      }

      /// <summary>
      /// führt einen Nicht-Select-SQL-Befehl aus
      /// </summary>
      /// <param name="sql"></param>
      /// <param name="param">ev. benannte Parameter, die noch ersetzt werden</param>
      /// <returns>Ergebnis von SqliteCommand.ExecuteNonQuery()</returns>
      int sqlExecute(string sql, Dictionary<string, object> param = null) {
         if (!isOpen)
            open();
         //command.CommandText = sql;
         command = new SqliteCommand(sql, connection);
         if (param != null)
            foreach (var item in param)
               command.Parameters.AddWithValue(item.Key, item.Value);
         int ret = command.ExecuteNonQuery();
         command.Parameters.Clear();
         return ret;
      }

      /// <summary>
      /// führt ein Select-SQL-Befehl aus und liefert das Ergebnis als Datentabelle
      /// </summary>
      /// <param name="sql"></param>
      /// <returns></returns>
      DataTable sqlSelect(string sql) {
         if (!isOpen)
            open();

         //command.CommandText = sql;
         command = new SqliteCommand(sql, connection);
         SqliteDataReader sr = command.ExecuteReader();
         DataTable dt = new DataTable();
         dt.Load(sr);
         sr.Close();
         return dt;
      }

      /// <summary>
      /// liefert das Ergebnis des SELECT als long-Zahl
      /// </summary>
      /// <param name="sql"></param>
      /// <returns></returns>
      long sqlSelectInt(string sql) {
         if (!isOpen)
            open();
         command.CommandText = sql;
         return (long)command.ExecuteScalar();
      }

      /// <summary>
      /// liefert das Ergebnis des SELECT als Text
      /// </summary>
      /// <param name="sql"></param>
      /// <returns></returns>
      string sqlSelectString(string sql) {
         if (!isOpen)
            open();
         command.CommandText = sql;
         return command.ExecuteScalar() as string;
      }


      GarminPoint[] datatable2Point(DataTable tab) {
         GarminPoint[] lst = new GarminPoint[tab.Rows.Count];
         for (int i = 0; i < lst.Length; i++)
            lst[i] = new GarminPoint(Convert.ToInt32(tab.Rows[i]["type"]),
                                     Convert.ToSingle(tab.Rows[i]["lon"]),
                                     Convert.ToSingle(tab.Rows[i]["lat"]),
                                     tab.Rows[i]["label"].ToString());
         return lst;
      }

      GarminPolypoint[] datatable2Polypoint(DataTable tab) {
         GarminPolypoint[] lst = new GarminPolypoint[tab.Rows.Count];
         for (int i = 0; i < lst.Length; i++)
            lst[i] = new GarminPolypoint(Convert.ToInt32(tab.Rows[i]["type"]),
                                         tab.Rows[i]["label"].ToString(),
                                         Convert.ToSingle(tab.Rows[i]["north"]),
                                         Convert.ToSingle(tab.Rows[i]["east"]),
                                         Convert.ToSingle(tab.Rows[i]["south"]),
                                         Convert.ToSingle(tab.Rows[i]["west"]),
                                         tab.Rows[i]["coord"] as byte[]);
         return lst;
      }

      #region spez. INSERT-Kommandos

      SqliteCommand insertPointCommand = null;

      /// <summary>
      /// Punkt mit prepared Statement einfügen (geringfügig schneller)
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="sdidx"></param>
      void insertPoint(GarminPoint pt, int sdidx) {
         if (insertPointCommand == null) {
            insertPointCommand = new SqliteCommand("INSERT INTO points (sd, type, label, lon, lat) VALUES(:sd, :type, :label, :lon, :lat)", connection);
            insertPointCommand.Parameters.Add(":sd", SqliteType.Integer);
            insertPointCommand.Parameters.Add(":type", SqliteType.Integer);
            insertPointCommand.Parameters.Add(":label", SqliteType.Text);
            insertPointCommand.Parameters.Add(":lon", SqliteType.Real);
            insertPointCommand.Parameters.Add(":lat", SqliteType.Real);
            insertPointCommand.Prepare();
         }

         insertPointCommand.Parameters[":sd"].Value = sdidx;
         insertPointCommand.Parameters[":type"].Value = pt.FullType;
         insertPointCommand.Parameters[":label"].Value = pt.Label is null ? "" : pt.Label;
         insertPointCommand.Parameters[":lon"].Value = pt.Point.Lon;
         insertPointCommand.Parameters[":lat"].Value = pt.Point.Lat;

         insertPointCommand.ExecuteNonQuery();
      }

      SqliteCommand insertAreaCommand = null;
      SqliteCommand insertLineCommand = null;

      /// <summary>
      /// Polyline mit prepared Statement einfügen (geringfügig schneller)
      /// </summary>
      /// <param name="ppt"></param>
      /// <param name="sdidx"></param>
      /// <param name="line"></param>
      void insertPoly(GarminPolypoint ppt, int sdidx, bool line) {
         if (line && insertLineCommand == null) {
            insertLineCommand = new SqliteCommand("INSERT INTO lines (sd, type, label, north, east, south, west, coord) VALUES(:sd, :type, :label, :north, :east, :south, :west, :coord)", connection);
            insertLineCommand.Parameters.Add(":sd", SqliteType.Integer);
            insertLineCommand.Parameters.Add(":type", SqliteType.Integer);
            insertLineCommand.Parameters.Add(":label", SqliteType.Text);
            insertLineCommand.Parameters.Add(":north", SqliteType.Real);
            insertLineCommand.Parameters.Add(":east", SqliteType.Real);
            insertLineCommand.Parameters.Add(":south", SqliteType.Real);
            insertLineCommand.Parameters.Add(":west", SqliteType.Real);
            insertLineCommand.Parameters.Add(":coord", SqliteType.Blob);
            insertLineCommand.Prepare();
         } else if (!line && insertAreaCommand == null) {
            insertAreaCommand = new SqliteCommand("INSERT INTO areas (sd, type, label, north, east, south, west, coord) VALUES(:sd, :type, :label, :north, :east, :south, :west, :coord)", connection);
            insertAreaCommand.Parameters.Add(":sd", SqliteType.Integer);
            insertAreaCommand.Parameters.Add(":type", SqliteType.Integer);
            insertAreaCommand.Parameters.Add(":label", SqliteType.Text);
            insertAreaCommand.Parameters.Add(":north", SqliteType.Real);
            insertAreaCommand.Parameters.Add(":east", SqliteType.Real);
            insertAreaCommand.Parameters.Add(":south", SqliteType.Real);
            insertAreaCommand.Parameters.Add(":west", SqliteType.Real);
            insertAreaCommand.Parameters.Add(":coord", SqliteType.Blob);
            insertAreaCommand.Prepare();
         }

         SqliteCommand cmd = line ? insertLineCommand : insertAreaCommand;
         cmd.Parameters[":sd"].Value = sdidx;
         cmd.Parameters[":type"].Value = ppt.FullType;
         cmd.Parameters[":label"].Value = ppt.Label is null ? "" : ppt.Label;
         cmd.Parameters[":north"].Value = ppt.North;
         cmd.Parameters[":east"].Value = ppt.East;
         cmd.Parameters[":south"].Value = ppt.South;
         cmd.Parameters[":west"].Value = ppt.West;
         cmd.Parameters[":coord"].Value = ppt.PointsToBytes();

         cmd.ExecuteNonQuery();
      }

      #endregion

      /*
      https://www.sqlite.org/pragma.html

      PRAGMA schema.synchronous;
      PRAGMA schema.synchronous = 0 | OFF | 1 | NORMAL | 2 | FULL | 3 | EXTRA;
      Query or change the setting of the "synchronous" flag. The first (query) form will return the synchronous setting as an integer. The second form changes the synchronous setting. 
      The meanings of the various synchronous settings are as follows:
      EXTRA (3)      EXTRA synchronous is like FULL with the addition that the directory containing a rollback journal is synced after that journal is unlinked to commit a transaction 
                     in DELETE mode. EXTRA provides additional durability if the commit is followed closely by a power loss.
      FULL (2)       When synchronous is FULL (2), the SQLite database engine will use the xSync method of the VFS to ensure that all content is safely written to the disk surface prior 
                     to continuing. This ensures that an operating system crash or power failure will not corrupt the database. FULL synchronous is very safe, but it is also slower. 
                     FULL is the most commonly used synchronous setting when not in WAL mode.
      NORMAL (1)     When synchronous is NORMAL (1), the SQLite database engine will still sync at the most critical moments, but less often than in FULL mode. There is a very small 
                     (though non-zero) chance that a power failure at just the wrong time could corrupt the database in journal_mode=DELETE on an older filesystem. WAL mode is safe 
                     from corruption with synchronous=NORMAL, and probably DELETE mode is safe too on modern filesystems. WAL mode is always consistent with synchronous=NORMAL, but WAL 
                     mode does lose durability. A transaction committed in WAL mode with synchronous=NORMAL might roll back following a power loss or system crash. Transactions are durable 
                     across application crashes regardless of the synchronous setting or journal mode. The synchronous=NORMAL setting is a good choice for most applications running in WAL mode.
      OFF (0)        With synchronous OFF (0), SQLite continues without syncing as soon as it has handed data off to the operating system. If the application running SQLite crashes, the 
                     data will be safe, but the database might become corrupted if the operating system crashes or the computer loses power before that data has been written to the disk 
                     surface. On the other hand, commits can be orders of magnitude faster with synchronous OFF. 
      
      In WAL mode when synchronous is NORMAL (1), the WAL file is synchronized before each checkpoint and the database file is synchronized after each completed checkpoint and the WAL file 
      header is synchronized when a WAL file begins to be reused after a checkpoint, but no sync operations occur during most transactions. With synchronous=FULL in WAL mode, an additional 
      sync operation of the WAL file happens after each transaction commit. The extra WAL sync following each transaction help ensure that transactions are durable across a power loss. 
      Transactions are consistent with or without the extra syncs provided by synchronous=FULL. If durability is not a concern, then synchronous=NORMAL is normally all one needs in WAL mode.

      The TEMP schema always has synchronous=OFF since the content of of TEMP is ephemeral and is not expected to survive a power outage. Attempts to change the synchronous setting for TEMP 
      are silently ignored.

      
      PRAGMA schema.journal_mode;
      PRAGMA schema.journal_mode = DELETE | TRUNCATE | PERSIST | MEMORY | WAL | OFF
      This pragma queries or sets the journal mode for databases associated with the current database connection.
      The first form of this pragma queries the current journaling mode for database. When database is omitted, the "main" database is queried.
      The second form changes the journaling mode for "database" or for all attached databases if "database" is omitted. The new journal mode is returned. If the journal mode could not be 
      changed, the original journal mode is returned.
      The DELETE journaling mode    is the normal behavior. In the DELETE mode, the rollback journal is deleted at the conclusion of each transaction. Indeed, the delete operation is the 
                                    action that causes the transaction to commit. (See the document titled Atomic Commit In SQLite for additional detail.)
      The TRUNCATE journaling mode  commits transactions by truncating the rollback journal to zero-length instead of deleting it. On many systems, truncating a file is much faster than 
                                    deleting the file since the containing directory does not need to be changed.
      The PERSIST journaling mode   prevents the rollback journal from being deleted at the end of each transaction. Instead, the header of the journal is overwritten with zeros. This 
                                    will prevent other database connections from rolling the journal back. The PERSIST journaling mode is useful as an optimization on platforms where deleting 
                                    or truncating a file is much more expensive than overwriting the first block of a file with zeros. See also: PRAGMA journal_size_limit and 
                                    SQLITE_DEFAULT_JOURNAL_SIZE_LIMIT.
      The MEMORY journaling mode    stores the rollback journal in volatile RAM. This saves disk I/O but at the expense of database safety and integrity. If the application using SQLite 
                                    crashes in the middle of a transaction when the MEMORY journaling mode is set, then the database file will very likely go corrupt.
      The WAL journaling mode       uses a write-ahead log instead of a rollback journal to implement transactions. The WAL journaling mode is persistent; after being set it stays in effect 
                                    across multiple database connections and after closing and reopening the database. A database in WAL journaling mode can only be accessed by SQLite 
                                    version 3.7.0 (2010-07-21) or later.
      The OFF journaling mode       disables the rollback journal completely. No rollback journal is ever created and hence there is never a rollback journal to delete. The OFF journaling 
                                    mode disables the atomic commit and rollback capabilities of SQLite. The ROLLBACK command no longer works; it behaves in an undefined way. Applications 
                                    must avoid using the ROLLBACK command when the journal mode is OFF. If the application crashes in the middle of a transaction when the OFF journaling 
                                    mode is set, then the database file will very likely go corrupt. Without a journal, there is no way for a statement to unwind partially completed operations 
                                    following a constraint error. This might also leave the database in a corrupted state. For example, if a duplicate entry causes a CREATE UNIQUE INDEX 
                                    statement to fail half-way through, it will leave behind a partially created, and hence corrupt, index. Because OFF journaling mode allows the database 
                                    file to be corrupted using ordinary SQL, it is disabled when SQLITE_DBCONFIG_DEFENSIVE is enabled.

      Note that the journal_mode for an in-memory database is either MEMORY or OFF and can not be changed to a different value. An attempt to change the journal_mode of an in-memory 
      database to any setting other than MEMORY or OFF is ignored. Note also that the journal_mode cannot be changed while a transaction is active.
       
       */
      /// <summary>
      /// unsicherer, falls beim Schreiben etwas schief geht, aber etwas schneller: synchronous=OFF, journal_mode=MEMORY
      /// </summary>
      public void Optimize4Write() {
         sqlExecute("PRAGMA synchronous = OFF");
         sqlExecute("PRAGMA journal_mode = MEMORY");
      }

      public void StartTransaction() {
         sqlExecute("BEGIN TRANSACTION");
      }

      public void EndTransaction() {
         sqlExecute("END  TRANSACTION");
      }


      /// <summary>
      /// registriert eine neue Subdiv
      /// </summary>
      /// <param name="sdidx"></param>
      /// <param name="level"></param>
      /// <param name="north"></param>
      /// <param name="east"></param>
      /// <param name="south"></param>
      /// <param name="west"></param>
      public void RegisterSubdiv(int sdidx, int level, float north, float east, float south, float west) {
         sqlExecute(string.Format("INSERT INTO subdivinfo (sd, level, north, east, south, west) VALUES ({0},{1},{2},{3},{4},{5})",
                                  sdidx,
                                  level,
                                  north.ToString(CultureInfo.InvariantCulture),
                                  east.ToString(CultureInfo.InvariantCulture),
                                  south.ToString(CultureInfo.InvariantCulture),
                                  west.ToString(CultureInfo.InvariantCulture)));
      }

      /// <summary>
      /// liefert alle schon registrierten Subdiv-Idx
      /// </summary>
      /// <returns></returns>
      public int[] GetAllSubdivIdx() {
         DataTable tab = sqlSelect("SELECT sd FROM subdivinfo");
         int[] sdidx = new int[tab.Rows.Count];
         for (int i = 0; i < sdidx.Length; i++)
            sdidx[i] = Convert.ToInt32(tab.Rows[i][0]);
         return sdidx;
      }

      /// <summary>
      /// liefert alle schon registrierten Subdiv-Idx und zusätzlich ihren Level und die Begrenzung
      /// </summary>
      /// <param name="levels"></param>
      /// <param name="north"></param>
      /// <param name="east"></param>
      /// <param name="south"></param>
      /// <param name="west"></param>
      /// <returns></returns>
      public int[] GetAllSubdivs(out int[] levels, out float[] north, out float[] east, out float[] south, out float[] west) {
         DataTable tab = sqlSelect("SELECT sd, level, north, east, south, west FROM subdivinfo");
         int[] sdidx = new int[tab.Rows.Count];
         levels = new int[sdidx.Length];
         north = new float[sdidx.Length];
         east = new float[sdidx.Length];
         south = new float[sdidx.Length];
         west = new float[sdidx.Length];
         for (int i = 0; i < sdidx.Length; i++) {
            sdidx[i] = Convert.ToInt32(tab.Rows[i]["sd"]);
            levels[i] = Convert.ToInt32(tab.Rows[i]["level"]);
            north[i] = Convert.ToSingle(tab.Rows[i]["north"]);
            east[i] = Convert.ToSingle(tab.Rows[i]["east"]);
            south[i] = Convert.ToSingle(tab.Rows[i]["south"]);
            west[i] = Convert.ToSingle(tab.Rows[i]["west"]);
         }
         return sdidx;
      }

      public void RegisterPoint(GarminPoint pt, int sdidx) {
         insertPoint(pt, sdidx);
      }

      public GarminPoint[] GetAllPoints4Subdiv(int sdidx) {
         return datatable2Point(sqlSelect("SELECT * FROM points WHERE sd=" + sdidx.ToString()));
      }

      public GarminPoint[] GetAllPoints4Level(int level) {
         return datatable2Point(sqlSelect("SELECT * FROM points p JOIN subdivinfo si ON p.sd=si.sd WHERE si.level=" + level.ToString()));
      }

      void registerPolypoint(GarminPolypoint ppt, int sdidx, bool line) {
         insertPoly(ppt, sdidx, line);

         //sqlExecute(string.Format("INSERT INTO {0} (sd, type, label, north, east, south, west, coord) VALUES ({1},{2},:label,{3},{4},{5},{6},:coord)",
         //                         line ? "lines" : "areas",
         //                         sdidx,
         //                         ppt.FullType,
         //                         ppt.North.ToString(CultureInfo.InvariantCulture),
         //                         ppt.East.ToString(CultureInfo.InvariantCulture),
         //                         ppt.South.ToString(CultureInfo.InvariantCulture),
         //                         ppt.West.ToString(CultureInfo.InvariantCulture)),
         //           new Dictionary<string, object>() {
         //              { ":label", ppt.Label is null ? "" : ppt.Label },
         //              { ":coord", ppt.PointsToBytes() }
         //           });
      }

      public void RegisterLine(GarminPolypoint ppt, int sdidx) {
         registerPolypoint(ppt, sdidx, true);
      }

      public void RegisterArea(GarminPolypoint ppt, int sdidx) {
         registerPolypoint(ppt, sdidx, false);
      }

      GarminPolypoint[] getAllPolypoint4Subdiv(int sdidx, bool line) {
         return datatable2Polypoint(sqlSelect("SELECT * FROM " + (line ? "lines" : "areas") + " WHERE sd=" + sdidx.ToString()));
      }

      public GarminPolypoint[] GetAllLines4Subdiv(int sdidx) {
         return getAllPolypoint4Subdiv(sdidx, true);
      }

      public GarminPolypoint[] GetAllAreas4Subdiv(int sdidx) {
         return getAllPolypoint4Subdiv(sdidx, false);
      }

      GarminPolypoint[] getAllPolypoint4Level(int level, bool line) {
         return datatable2Polypoint(sqlSelect("SELECT * FROM " + (line ? "lines" : "areas") + "d JOIN subdivinfo si ON d.sd=si.sd WHERE si.level=" + level.ToString()));
      }

      public GarminPolypoint[] GetAllLines4Level(int level) {
         return getAllPolypoint4Level(level, true);
      }

      public GarminPolypoint[] GetAllAreas4Level(int level) {
         return getAllPolypoint4Level(level, false);
      }


      public override string ToString() {
         return string.Format("SQLite-DB {0}, {1}", DBFile, isOpen ? "open" : "closed");
      }

      ~GarminTileDatabase() {
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
               close();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }

}
