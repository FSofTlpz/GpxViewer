using System.Windows.Forms;

namespace FSofTUtils.Threading {
   /// <summary>
   /// Threadsicheres Arbeiten mit Control-Eigenschaften und -Methoden
   /// </summary>
   public static class ThreadsafeInvoker {

      private delegate void MethodCaller<ControlType>(
         ControlType control,
         string methodName,
         params object[] parameters)
         where ControlType : Control;

      private delegate object PropertyValueReaderCallback<ControlType>(
          ControlType control, string propertyName)
          where ControlType : Control;

      private delegate void PropertyValueWriterCallback<ControlType, PropertyType>(
          ControlType control, string propertyName, PropertyType value)
          where ControlType : Control;


      /// <summary>
      /// Invokes a control method call.
      /// </summary>
      /// <typeparam name="ControlType">The type of the control.</typeparam>
      /// <param name="control">The control.</param>
      /// <param name="methodName">Name of the method.</param>
      /// <param name="parameters">Parameters array for the method to be called.</param>
      public static void InvokeControlMethodCall<ControlType>(
         ControlType control,
         string methodName, params object[] parameters)
         where ControlType : Control {
         if (control.InvokeRequired) {
            MethodCaller<ControlType> callerDelegate = InvokeControlMethodCall;
            control.Invoke(callerDelegate, new object[] { control, methodName, parameters });
         } else {
            System.Reflection.MethodInfo method = control.GetType().GetMethod(methodName);
            method.Invoke(control, parameters);
         }
      }

      /// <summary>
      /// Invokes reading of a control property.
      /// </summary>
      /// <typeparam name="ControlType">The type of the control.</typeparam>
      /// <param name="control">The control.</param>
      /// <param name="propertyName">Name of the property.</param>
      /// <returns></returns>
      public static object InvokeControlPropertyReader<ControlType>(
         ControlType control,
         string propertyName)
         where ControlType : Control {
         if (control.InvokeRequired) {
            PropertyValueReaderCallback<ControlType> cb = InvokeControlPropertyReader;
            return control.Invoke(cb, new object[] { control, propertyName });
         }
         System.Reflection.PropertyInfo property = control.GetType().GetProperty(propertyName);
         return property.GetValue(control, null);
      }

      /// <summary>
      /// Invokes writing to a control property.
      /// </summary>
      /// <typeparam name="ControlType">The type of the control.</typeparam>
      /// <typeparam name="PropertyType">The type of the property.</typeparam>
      /// <param name="control">The control.</param>
      /// <param name="propertyName">Name of the property.</param>
      /// <param name="value">The value.</param>
      public static void InvokeControlPropertyWriter<ControlType, PropertyType>(
          ControlType control,
          string propertyName,
          PropertyType value)
          where ControlType : Control {
         if (control.InvokeRequired) {
            PropertyValueWriterCallback<ControlType, PropertyType> cb = InvokeControlPropertyWriter;
            control.Invoke(cb, new object[] { control, propertyName, value });
         } else {
            System.Reflection.PropertyInfo property = control.GetType().GetProperty(propertyName);
            property.SetValue(control, value, null);
         }
      }

   }
}
