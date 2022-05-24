// https://github.com/praeclarum/sqlite-net
// SQLite-net 1.7.335
// SQLite-net is an open source, minimal library to allow .NET, .NET Core, and Mono applications to store data in SQLite 3 databases. 



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace GpxViewer {
   class sqlitetest {

      public class Product {
         [AutoIncrement, PrimaryKey]
         public int Id { get; set; }
         public string Name { get; set; }
         public decimal Price { get; set; }
         public uint TotalSales { get; set; }
      }

      public class Order {
         [AutoIncrement, PrimaryKey]
         public int Id { get; set; }
         public DateTime PlacedTime { get; set; }
      }

      public class OrderHistory {
         [AutoIncrement, PrimaryKey]
         public int Id { get; set; }
         public int OrderId { get; set; }
         public DateTime Time { get; set; }
         public string Comment { get; set; }
      }

      public class OrderLine {
         [AutoIncrement, PrimaryKey]
         public int Id { get; set; }
         [Indexed("IX_OrderProduct", 1)]
         public int OrderId { get; set; }
         [Indexed("IX_OrderProduct", 2)]
         public int ProductId { get; set; }
         public int Quantity { get; set; }
         public decimal UnitPrice { get; set; }
         public OrderLineStatus Status { get; set; }
      }

      public enum OrderLineStatus {
         Placed = 1,
         Shipped = 100
      }



      public class ReadmeTest {

         public class Stock {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Symbol { get; set; }
         }

         public class Valuation {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            [Indexed]
            public int StockId { get; set; }
            public DateTime Time { get; set; }
            public decimal Price { get; set; }
         }

         public static void AddStock(SQLiteConnection db, string symbol) {
            var stock = new Stock() {
               Symbol = symbol
            };

            db.Insert(stock); // Returns the number of rows added to the table
            Console.WriteLine("{0} == {1}", stock.Symbol, stock.Id);
         }

         public static IEnumerable<Valuation> QueryValuations(SQLiteConnection db, Stock stock) {
            return db.Query<Valuation>("select * from Valuation where StockId = ?", stock.Id);
         }

         public class Val {
            public decimal Money { get; set; }
            public DateTime Date { get; set; }
         }

         public static IEnumerable<Val> QueryVals(SQLiteConnection db, Stock stock) {
            return db.Query<Val>("select \"Price\" as \"Money\", \"Time\" as \"Date\" from Valuation where StockId = ?", stock.Id);
         }

         public void Synchronous() {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            File.Delete(databasePath);

            var db = new SQLiteConnection(databasePath);
            db.CreateTable<Stock>();
            db.CreateTable<Valuation>();

            AddStock(db, "A1");
            AddStock(db, "A2");
            AddStock(db, "A3");
            AddStock(db, "B1");
            AddStock(db, "B2");
            AddStock(db, "B3");

            var query = db.Table<Stock>().Where(v => v.Symbol.StartsWith("A"));

            foreach (var stock in query)
               Console.WriteLine("Stock: " + stock.Symbol);

            //Assert.AreEqual(3, query.ToList().Count);
         }

         public async Task Asynchronous() {
            await Task.Delay(1).ConfigureAwait(false);

            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            File.Delete(databasePath);

            var db = new SQLiteAsyncConnection(databasePath);
            await db.CreateTableAsync<Stock>();

            Console.WriteLine("Table created!");

            var stock = new Stock() {
               Symbol = "AAPL"
            };

            await db.InsertAsync(stock);

            Console.WriteLine("New sti ID: {0}", stock.Id);
            var query = db.Table<Stock>().Where(s => s.Symbol.StartsWith("A"));

            var result = await query.ToListAsync();

            foreach (var s in result)
               Console.WriteLine("Stock: " + s.Symbol);

            //Assert.AreEqual(1, result.Count);
            var count = await db.ExecuteScalarAsync<int>("select count(*) from Stock");
            Console.WriteLine(string.Format("Found '{0}' stock items.", count));
            //Assert.AreEqual(1, count);
         }


         public void Manual() {
            var db = new SQLiteConnection(":memory:");
            db.Execute("create table Stock(Symbol varchar(100) not null)");
            db.Execute("insert into Stock(Symbol) values (?)", "MSFT");
            var stocks = db.Query<Stock>("select * from Stock");
            //Assert.AreEqual(1, stocks.Count);
            //Assert.AreEqual("MSFT", stocks[0].Symbol);
         }

      }


      public class TestDb : SQLiteConnection {

         public TestDb(string databasepath, bool storeDateTimeAsTicks = true, object key = null, bool wal = true) :
            base(new SQLiteConnectionString(databasepath, storeDateTimeAsTicks, key: key)) {
            Trace = true;
            if (wal)
               EnableWriteAheadLogging();
         }

         public TestDb(SQLiteConnectionString connectionString, bool wal = true) :
            base(connectionString) {
            Trace = true;
            if (wal)
               EnableWriteAheadLogging();
         }

      }



      public static void Test1() {
         //var name = Guid.NewGuid() + ".sqlite";
         //string file = Path.Combine(System.Windows.Storage.ApplicationData.Current.LocalFolder.Path, name);
         //string file = Path.GetTempFileName();

         string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "MyData.db");
         File.Delete(file);


         //TestDb db = new TestDb(file);

         var db = new SQLiteConnection(file);
         db.CreateTable<Product>();
         db.CreateTable<Order>();

         Product prod;

         prod = new Product() {
            Name = "Produkt 1",
            Price = 1.2M,
            TotalSales = 0,
         };
         db.Insert(prod);
         Debug.WriteLine(prod.Id.ToString() + "/" + prod.Name);

         prod = new Product() {
            Name = "A-Produkt 2",
            Price = 2.3M,
            TotalSales = 0,
         };
         db.Insert(prod);
         Debug.WriteLine(prod.Id.ToString() + "/" + prod.Name);

         prod = new Product() {
            Name = "B-Produkt 3",
            Price = 3.4M,
            TotalSales = 0,
         };
         db.Insert(prod);
         Debug.WriteLine(prod.Id.ToString() + "/" + prod.Name);

         prod = new Product() {
            Name = "A-Produkt 4",
            Price = 4.5M,
            TotalSales = 0,
         };
         db.Insert(prod);
         Debug.WriteLine(prod.Id.ToString() + "/" + prod.Name);



         var query = db.Table<Product>().Where(v => v.Name.StartsWith("A"));

         foreach (var item in query)
            Debug.WriteLine("Product: " + item.Id.ToString() + "/" + item.Name);

      }


      /*

      - je IMG eine DB: 70060044.IMG -> LocalApplicationData  temp/gpxview/70060044.db
      - 1 Tabelle für alle DetailMaps
      - 1 Tabelle für Points
      - 1 Tabelle für Areas
      - 1 Tabelle für Lines

       */


      public static void Test2() {
      }


   }
}
