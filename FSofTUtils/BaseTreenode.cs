using System;
using System.Collections.Generic;

// https://www.tutorialspoint.com/data_structures_algorithms/tree_traversal.htm


namespace FSofTUtils {
   public class BaseTreenode {

      /// <summary>
      /// übergeordneter <see cref="BaseTreenode"/>
      /// </summary>
      public BaseTreenode? Parent {
         get;
         protected set;
      }

      /// <summary>
      /// untergeordnete Child-<see cref="BaseTreenode"/>
      /// </summary>
      public List<BaseTreenode> ChildNodes = new List<BaseTreenode>();

      /// <summary>
      /// Anzahl der <see cref="ChildNodes"/>
      /// </summary>
      public int Childs {
         get {
            return ChildNodes != null ? ChildNodes.Count : 0;
         }
      }

      public BaseTreenode? FirstChildnode {
         get {
            return Childs > 0 ? ChildNodes[0] : null;
         }
      }

      public BaseTreenode? LastChildnode {
         get {
            return Childs > 0 ? ChildNodes[Childs - 1] : null;
         }
      }


      public void AppendChild(BaseTreenode child) {
         ChildNodes.Add(child);
         child.Parent = this;
      }

      public void InsertChild(int idx, BaseTreenode child) {
         if (0 <= idx && idx < Childs) {
            ChildNodes.Insert(idx, child);
            child.Parent = this;
         } else
            AppendChild(child);
      }

      public void RemoveChildAt(int idx) {
         if (0 <= idx && idx < Childs) {
            ChildNodes[idx].Parent = null;
            ChildNodes.RemoveAt(idx);
         }
      }

      public void RemoveChild(BaseTreenode child) {
         ChildNodes.Remove(child);
         child.Parent = null;
      }


      /*
       * In-order Traversal (nur binär)
       *    rekursiv 1. Child
       *    visit node (z.B. Ausgabe)
       *    rekursiv 2. Child
       *    
       * Pre-order Traversal
       *    visit node (z.B. Ausgabe)
       *    dann rekursiv nacheinander alle childs
       *    
       * Post-order Traversal
       *    rekursiv nacheinander alle childs
       *    visit node
       */


      public enum WalkOrder {
         InOrder,
         PreOrder,
         PostOrder,
      }


      /// <summary>
      /// "wandert" durch den Tree und ruft für jeden Node Action mit diesem Node auf
      /// <para>Liefert die Funktion true, bricht der Algorithmus ab.</para>
      /// </summary>
      /// <param name="tn">Node</param>
      /// <param name="func"></param>
      /// <param name="walkOrder"></param>
      /// <returns>bei Abbruch true</returns>
      public static bool Walk(BaseTreenode? tn,
                              Func<BaseTreenode, bool> func,
                              WalkOrder walkOrder = WalkOrder.PreOrder) {
         if (tn != null) {
            switch (walkOrder) {
               case WalkOrder.PreOrder:
                  if (func(tn))
                     return true;
                  foreach (var item in tn.ChildNodes)
                     if (Walk(item, func, walkOrder))
                        return true;
                  break;

               case WalkOrder.PostOrder:
                  foreach (var item in tn.ChildNodes)
                     if (Walk(item, func, walkOrder))
                        return true;
                  if (func(tn))
                     return true;
                  break;

               case WalkOrder.InOrder:
                  if (tn.Childs > 2)
                     throw new Exception("Operation is only for binary trees!");
                  if (tn.Childs > 0)
                     if (Walk(tn.FirstChildnode, func, walkOrder))
                        return true;
                  if (func(tn))
                     return true;
                  if (tn.Childs == 2)
                     if (Walk(tn.LastChildnode, func, walkOrder))
                        return true;
                  break;
            }
         }
         return false;
      }

      /// <summary>
      /// "wandert" durch den Tree und ruft für jeden Node Action mit diesem Node und data auf
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="tn"></param>
      /// <param name="data"></param>
      /// <param name="action"></param>
      /// <param name="walkOrder"></param>
      public static void Walk<T>(BaseTreenode? tn,
                                 T data,
                                 Action<BaseTreenode, T> action,
                                 WalkOrder walkOrder = WalkOrder.PreOrder) {
         if (tn != null) {
            switch (walkOrder) {
               case WalkOrder.PreOrder:
                  action(tn, data);
                  foreach (var item in tn.ChildNodes)
                     Walk(item, data, action, walkOrder);
                  break;

               case WalkOrder.PostOrder:
                  foreach (var item in tn.ChildNodes)
                     Walk(item, data, action, walkOrder);
                  action(tn, data);
                  break;

               case WalkOrder.InOrder:
                  if (tn.Childs > 2)
                     throw new Exception("Operation is only for binary trees!");
                  if (tn.Childs > 0)
                     Walk(tn.FirstChildnode, data, action, walkOrder);
                  action(tn, data);
                  if (tn.Childs == 2)
                     Walk(tn.LastChildnode, data, action, walkOrder);
                  break;
            }
         }
      }

      /// <summary>
      /// "wandert" durch den Tree und ruft für jeden Node Action mit diesem Node auf
      /// <para>Liefert die Funktion true, bricht der Algorithmus ab.</para>
      /// </summary>
      /// <param name="tn">Node</param>
      /// <param name="data"></param>
      /// <param name="func"></param>
      /// <param name="walkOrder"></param>
      /// <returns>bei Abbruch true</returns>
      public static bool Walk<T>(BaseTreenode? tn, 
                                 T data, Func<BaseTreenode, T, bool> func, 
                                 WalkOrder walkOrder = WalkOrder.PreOrder) {
         if (tn != null) {
            switch (walkOrder) {
               case WalkOrder.PreOrder:
                  if (func(tn, data))
                     return true;
                  foreach (var item in tn.ChildNodes)
                     if (Walk(item, data, func, walkOrder))
                        return true;
                  break;

               case WalkOrder.PostOrder:
                  foreach (var item in tn.ChildNodes)
                     if (Walk(item, data, func, walkOrder))
                        return true;
                  if (func(tn, data))
                     return true;
                  break;

               case WalkOrder.InOrder:
                  if (tn.Childs > 2)
                     throw new Exception("Operation is only for binary trees!");
                  if (tn.Childs > 0)
                     if (Walk(tn.FirstChildnode, data, func, walkOrder))
                        return true;
                  if (func(tn, data))
                     return true;
                  if (tn.Childs == 2)
                     if (Walk(tn.LastChildnode, data, func, walkOrder))
                        return true;
                  break;
            }
         }
         return false;
      }

      public override string ToString() {
         return "Childs=" + Childs;
      }

   }
}
