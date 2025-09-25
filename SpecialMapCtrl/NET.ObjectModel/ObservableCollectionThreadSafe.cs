using System;

namespace SpecialMapCtrl.NET.ObjectModel {
   public class ObservableCollectionThreadSafe<T> : ObservableCollection<T> {

      public override event EventHandler<NotifyCollectionChangedEventArgs>? CollectionChanged;


      protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
         // Be nice - use BlockReentrancy like MSDN said
         using (BlockReentrancy()) {
            if (CollectionChanged != null) {
               var delegates = CollectionChanged.GetInvocationList();

               // Walk thru invocation list
               foreach (var handler in delegates) {
#if !GMAP4SKIA
                  var dispatcherObject = handler.Target as System.Windows.Forms.Control;

                  // If the subscriber is a DispatcherObject and different thread
                  if (dispatcherObject != null && dispatcherObject.InvokeRequired) {
                     // Invoke handler in the target dispatcher's thread
                     dispatcherObject.Invoke(handler, this, e);
                  } else // Execute handler as is 
                     CollectionChanged(this, e);
#else
                  CollectionChanged(this, e);
#endif
               }
            }
         }
      }
   }
}
