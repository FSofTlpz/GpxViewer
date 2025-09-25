using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;

namespace FSofTUtils.Sys {

   /*
    Here's the core of the problem: from MSDN on QueryPerformanceCounter which is the API used by the Stopwatch class:

    On a multiprocessor computer, it should not matter which processor is called. However, you can get different results on 
    different processors due to bugs in the basic input/output system (BIOS) or the hardware abstraction layer (HAL). 
    To specify processor affinity for a thread, use the SetThreadAffinityMask function.

    ==> http://msdn.microsoft.com/en-us/library/ms644904%28VS.85%29.aspx
    
    Don't just use .NET 4.0 stopwatch and assume the problem is fixed. It isn't and they can't do anything about it unless you 
    want them to muck with your thread affinity. From the Stopwatch class documentation:

    On a multiprocessor computer, it does not matter which processor the thread runs on. However, because of bugs in the BIOS 
    or the Hardware Abstraction Layer (HAL), you can get different timing results on different processors. 
    To specify processor affinity for a thread, use the ProcessThread.ProcessorAffinity method.
     
    ==> http://stackoverflow.com/questions/1008345/system-diagnostics-stopwatch-returns-negative-numbers-in-elapsed-properties
    
    */

   public class HighResolutionWatch {

      public static bool IsHighResolution { get { return Stopwatch.IsHighResolution; } }
      /// <summary>
      /// Frequenz in Hz
      /// </summary>
      public static long Frequency { get { return Stopwatch.Frequency; } }
      /// <summary>
      /// Anzahl der Messungen
      /// </summary>
      public int Count { get { return ticks.Count; } }
      /// <summary>
      /// min. messbare Zeit in Sekunden (Zeit je Tick)
      /// </summary>
      public float TickAsSecond { get { return 1f / Frequency; } }


      protected List<long> ticks;
      protected List<string> ticksdescription;
      protected Stopwatch watch;
      protected Stopwatch sleep;


      public HighResolutionWatch() {
         ticks = new List<long>(100);
         ticksdescription = new List<string>(100);
         watch = new Stopwatch();
         sleep = new Stopwatch();

         int procno = new Random().Next(System.Environment.ProcessorCount);                     // zufällig ausgewählte CPU-Nummer (0..)
         Process.GetCurrentProcess().ProcessorAffinity = new System.IntPtr(0x1 << procno);      // Thread an diese festgelegte CPU binden
      }

      /// <summary>
      /// Start der Messung (Ticks werden auf 0 gesetzt)
      /// </summary>
      public void Start() {
         if (watch.IsRunning)
            Stop();
         ticks.Clear();
         ticksdescription.Clear();
         watch.Restart();
      }

      /// <summary>
      /// akt. Messwert speichern
      /// </summary>
      /// <returns>akt. Wert</returns>
      public long Store(string? description = "") {
         ticks.Add(watch.ElapsedTicks);
         if (description == null)
            description = "";
         ticksdescription.Add(description);
         return ticks[ticks.Count - 1];
      }

      public long Store(object? description) {
         return Store(description == null ? "" : description.ToString());
      }

      /// <summary>
      /// Stop der Messung / Messreihe
      /// </summary>
      /// <returns>akt. Wert</returns>
      public long Stop(string description = "Stop") {
         watch.Stop();
         return Store(description);
      }

      /// <summary>
      /// Anzahl der Ticks auf dem Speicherplatz; ungültige Speicherplätze liefern 0
      /// </summary>
      /// <param name="no"></param>
      /// <returns></returns>
      public long Ticks(int no) {
         return no >= 0 && no < Count ? ticks[no] : 0;
      }

      /// <summary>
      /// Beschreibung auf dem Speicherplatz
      /// </summary>
      /// <param name="no"></param>
      /// <returns></returns>
      public string Description(int no) {
         return no >= 0 && no < Count ? ticksdescription[no] : "";
      }

      /// <summary>
      /// Sekunden auf dem Speicherplatz
      /// </summary>
      /// <param name="no"></param>
      /// <returns></returns>
      public float Seconds(int no) {
         return TickAsSecond * Ticks(no);
      }

      /// <summary>
      /// Sekunden auf dem Speicherplatz für den Schritt
      /// </summary>
      /// <param name="no"></param>
      /// <returns></returns>
      public float StepSeconds(int no) {
         //if (Ticks(no) < Ticks(no - 1)) {
         //   Debug.WriteLine("MIST");
         //}
         return TickAsSecond * (Ticks(no) - Ticks(no - 1));
      }

      /// <summary>
      /// Sekunden auf dem Speicherplatz für den letzten Schritt
      /// </summary>
      /// <returns></returns>
      public float PeekStepSeconds() {
         int no = Count - 1;
         return TickAsSecond * (Ticks(no) - Ticks(no - 1));
      }

      /// <summary>
      /// Gesamtlaufzeit
      /// </summary>
      /// <returns></returns>
      public float AllSeconds() {
         return Seconds(Count - 1);
      }

      /// <summary>
      /// Sleep für den ms-Bereich (sinnvoll ab einigen ms aufwärts; nicht sehr genau)
      /// </summary>
      /// <param name="milliseconds">Millisekunden</param>
      /// <returns>Anzahl der internen Schleifendurchläufe + Laufzeit</returns>
      public float VeryShortSleep(float milliseconds) {
         long ticks = (long)(Frequency * milliseconds / 1000);
         sleep.Restart();
         int count = 0;
         while (sleep.ElapsedTicks < ticks) count++;
         sleep.Stop();
         return count + TickAsSecond * sleep.ElapsedTicks;
      }

      /// <summary>
      /// liefert die Daten in Millisekunden als Textzeile
      /// </summary>
      /// <returns></returns>
      public string AsMsText(string prefix = "") {
         StringBuilder sb = new StringBuilder(prefix);
         sb.Append("(ms):");
         for (int i = 0; i < Count; i++) {
            sb.Append(" [");
            sb.Append(Description(i).Length > 0 ? Description(i) : i.ToString());
            sb.Append("]=");
            sb.Append((StepSeconds(i) * 1000).ToString("0.0"));
         }
         sb.Append(" -> ");
         sb.Append((AllSeconds() * 1000).ToString("0.0"));
         return sb.ToString();
      }

      public override string ToString() {
         return string.Format("IsHighResolution={0}, Frequency={1}Hz, Count={2}, AllSeconds={3}", IsHighResolution, Frequency, Count, AllSeconds());
      }

   }
}
