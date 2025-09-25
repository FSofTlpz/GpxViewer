#define WITH_INTERLOCKED         // damit ist ein Teil der Operationen vielleicht etwas schneller, weil das "lock" entfällt

using System.Threading;

namespace FSofTUtils.Threading {

   /* The methods of this class help protect against errors that can occur when the scheduler switches contexts while a thread is 
    * updating a variable that can be accessed by other threads, or when two threads are executing concurrently on separate processors. 
    * The members of this class do not throw exceptions. */

   /// <summary>
   /// eine threadsichere Variable
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class ThreadSafeVariable<T> {

      public readonly object VarLocker;
      protected T? v;

      public ThreadSafeVariable() {
         VarLocker = new object();
         v = default;
      }

      public ThreadSafeVariable(T v)
         : this() {
         this.v = v;
      }

      /// <summary>
      /// setzt oder liefert den Inhalt
      /// </summary>
      public virtual T? Value {
         get {
            T? result;
            lock (VarLocker) {
               result = v;
            }
            return result;
         }
         set {
            lock (VarLocker) {
               v = value;
            }
         }
      }

      public override string ToString() {
         return v?.ToString() ?? "";
      }

   }

   /// <summary>
   /// eine threadsichere int-Variable
   /// </summary>
   public class ThreadSafeIntVariable : ThreadSafeVariable<int> {

      public ThreadSafeIntVariable(int v) : base(v) { }

      public ThreadSafeIntVariable() : base() { }

      public override int Value {
         get {
            return Interlocked.Exchange(ref v, v);
         }
         set {
            Interlocked.Exchange(ref v, value);
         }
      }

      public int Add(int v) {
#if WITH_INTERLOCKED
         return Interlocked.Add(ref this.v, v);
#else
         long result;
         lock (locker) {
            result = this.v += v;
         }
         return result;
#endif
      }

      public int Sub(int v) {
         return Add(-v);
      }

      public int Mul(int v) {
         int result;
         lock (VarLocker) {
            result = this.v *= v;
         }
         return result;
      }

      public int Div(int v) {
         int result;
         lock (VarLocker) {
            result = this.v /= v;
         }
         return result;
      }

      public int Mod(int v) {
         int result;
         lock (VarLocker) {
            result = this.v %= v;
         }
         return result;
      }

      public int Increment() {
#if WITH_INTERLOCKED
         return Interlocked.Increment(ref this.v);
#else
         return Add(1);
#endif
      }

      public int Decrement() {
#if WITH_INTERLOCKED
         return Interlocked.Decrement(ref this.v);
#else
         return Add(-1);
#endif
      }

   }

   /// <summary>
   /// eine threadsichere long-Variable
   /// </summary>
   public class ThreadSafeLongVariable : ThreadSafeVariable<long> {

      public ThreadSafeLongVariable(long v) : base(v) { }

      public ThreadSafeLongVariable() : base() { }

      public override long Value {
         get {
            return Interlocked.Exchange(ref v, v);
         }
         set {
            Interlocked.Exchange(ref v, value);
         }
      }

      public long Add(long v) {
#if WITH_INTERLOCKED
         return Interlocked.Add(ref this.v, v);
#else
         long result;
         lock (locker) {
            result = this.v += v;
         }
         return result;
#endif
      }

      public long Sub(long v) {
         return Add(-v);
      }

      public long Mul(long v) {
         long result;
         lock (VarLocker) {
            result = this.v *= v;
         }
         return result;
      }

      public long Div(long v) {
         return Mul(1 / v);
      }

      public long Mod(long v) {
         long result;
         lock (VarLocker) {
            result = this.v %= v;
         }
         return result;
      }

      public long Increment() {
#if WITH_INTERLOCKED
         return Interlocked.Increment(ref this.v);
#else
         return Add(1);
#endif
      }

      public long Decrement() {
#if WITH_INTERLOCKED
         return Interlocked.Decrement(ref this.v);
#else
         return Add(-1);
#endif
      }

   }

   /// <summary>
   /// eine threadsichere double-Variable
   /// </summary>
   public class ThreadSafeDoubleVariable : ThreadSafeVariable<double> {

      public ThreadSafeDoubleVariable(double v) : base(v) { }

      public ThreadSafeDoubleVariable() : base() { }

      public override double Value {
         get {
            return Interlocked.Exchange(ref v, v);
         }
         set {
            Interlocked.Exchange(ref v, value);
         }
      }

      public double Add(double v) {
         double result;
         lock (VarLocker) {
            result = this.v += v;
         }
         return result;
      }

      public double Sub(double v) {
         return Add(-v);
      }

      public double Mul(double v) {
         double result;
         lock (VarLocker) {
            result = this.v *= v;
         }
         return result;
      }

      public double Div(double v) {
         return Mul(1 / v);
      }

      public double Mod(double v) {
         double result;
         lock (VarLocker) {
            result = this.v %= v;
         }
         return result;
      }

   }

   /// <summary>
   /// eine threadsichere decimal-Variable
   /// </summary>
   public class ThreadSafeDecimalVariable : ThreadSafeVariable<decimal> {

      public ThreadSafeDecimalVariable(decimal v) : base(v) { }

      public ThreadSafeDecimalVariable() : base() { }

      public decimal Add(decimal v) {
         decimal result;
         lock (VarLocker) {
            result = this.v += v;
         }
         return result;
      }

      public decimal Sub(decimal v) {
         return Add(-v);
      }

      public decimal Mul(decimal v) {
         decimal result;
         lock (VarLocker) {
            result = this.v *= v;
         }
         return result;
      }

      public decimal Div(decimal v) {
         return Mul(1 / v);
      }

      public decimal Mod(decimal v) {
         decimal result;
         lock (VarLocker) {
            result = this.v %= v;
         }
         return result;
      }

   }

   /// <summary>
   /// eine threadsichere bool-Variable
   /// </summary>
   public class ThreadSafeBoolVariable : ThreadSafeVariable<long> {

      public ThreadSafeBoolVariable(bool v) : base(v ? 1 : 0) { }

      public ThreadSafeBoolVariable() : base() { }

      protected bool AsBool(long value) {
         return value != 0;
      }

      protected long AsLong(bool value) {
         return value ? 1 : 0;
      }

      public bool And(bool v) {
         bool result;
         lock (VarLocker) {
            result = AsBool(this.v) && v;
         }
         return result;
      }

      public bool Or(bool v) {
         bool result;
         lock (VarLocker) {
            result = AsBool(this.v) || v;
         }
         return result;
      }

      /// <summary>
      /// negiert den vorhandenen Wert
      /// </summary>
      /// <returns></returns>
      public bool Negation() {
         bool result;
         lock (VarLocker) {
            result = !AsBool(v);
         }
         return result;
      }

      public new bool Value {
         get {
#if WITH_INTERLOCKED
            return AsBool(Interlocked.Read(ref this.v));
#else
            bool result;
            lock (locker) {
               result = AsBool(v);
            }
            return result;
#endif
         }
         set {
#if WITH_INTERLOCKED
            Interlocked.Exchange(ref this.v, AsLong(value));
#else
            lock (locker) {
               v = AsLong(value);
            }
#endif
         }
      }

      public override string ToString() {
         return AsBool(v).ToString();
      }

   }



}
