using GarminCore;
using System.Collections.Generic;

namespace Test_GDB {
   class Program {

      static void Main(string[] args) {

         string fileName = "../../../Test1, Version 3.gdb";

         List<GDB.Object> lst = GDB.ReadGDBObjectList(fileName);

      }

   }
}
