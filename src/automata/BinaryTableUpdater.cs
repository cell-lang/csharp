namespace Cell.Runtime {
  public class BinaryTableUpdater {
    private bool hasDeletes = false;
    private bool clear = false;

    private int deleteCount = 0;
    private long[] deleteList = Array.emptyLongArray;
    private bool deleteListIsSorted = false; // Sorted exclusively in left-to-right order

    private int delete1Count = 0;
    private int[] delete1List = Array.emptyIntArray;
    private bool delete1ListIsSorted = false;

    private int delete2Count = 0;
    private int[] delete2List = Array.emptyIntArray;
    private bool delete2ListIsSorted = false;

    private int[] buffer = Array.emptyIntArray;

    private int insertCount = 0;
    private long[] insertList = Array.emptyLongArray;
    private Ord insertListOrd = Ord.ORD_NONE;


    private enum Ord {ORD_NONE, ORD_12, ORD_21};


    internal string relvarName;
    internal BinaryTable table;
    internal ValueStoreUpdater store1, store2;

    //////////////////////////////////////////////////////////////////////////////

    private static long Tuple(int arg1, int arg2) {
      return Miscellanea.Pack(arg2, arg1);
    }

    static internal int Arg1(long tuple) {
      return Miscellanea.High(tuple);
    }

    static internal int Arg2(long tuple) {
      return Miscellanea.Low(tuple);
    }

    //////////////////////////////////////////////////////////////////////////////

    public BinaryTableUpdater(string relvarName, BinaryTable table, ValueStoreUpdater store1, ValueStoreUpdater store2) {
      this.relvarName = relvarName;
      this.table = table;
      this.store1 = store1;
      this.store2 = store2;
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Clear() {
      hasDeletes = true;
      clear = true;
    }

    public void Delete(int arg1, int arg2) {
      hasDeletes = true;
      deleteList = Array.Append(deleteList, deleteCount++, Tuple(arg1, arg2));
    }

    public void Delete1(int arg1) {
      if (table.Contains1(arg1)) {
        hasDeletes = true;
        delete1List = Array.Append(delete1List, delete1Count++, arg1);
      }
    }

    public void Delete2(int arg2) {
      if (table.Contains2(arg2)) {
        hasDeletes = true;
        delete2List = Array.Append(delete2List, delete2Count++, arg2);
      }
    }

    public void Insert(int arg1, int arg2) {
      insertList = Array.Append(insertList, insertCount++, Tuple(arg1, arg2));
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Apply() {
      if (hasDeletes) {
        if (clear) {
          //## IF THE RIGHT-TO-LEFT SEARCH DATA STRUCTURES HAVE BEEN BUILD
          //## THERE'S A MUCH FASTER WAY TO IMPLEMENT THIS
          int left = table.Size();
          for (int arg1=0 ; left > 0 ; arg1++) {
            int count = table.Count1(arg1);
            if (count > 0) {
              store1.MarkForDelayedRelease(arg1, count);
              if (buffer.Length < count)
                buffer = new int[Array.Capacity(buffer.Length, count)];
              table.Delete1(arg1, buffer);
              for (int i2=0 ; i2 < count ; i2++)
                store2.MarkForDelayedRelease(buffer[i2]);
              left -= count;
            }
          }
        }
        else {
          if (delete1Count != 0) {
            for (int i1=0 ; i1 < delete1Count ; i1++) {
              int arg1 = delete1List[i1];
              int count = table.Count1(arg1);
              if (count > 0) {
                store1.MarkForDelayedRelease(arg1, count);
                if (buffer.Length < count)
                  buffer = new int[Array.Capacity(buffer.Length, count)];
                table.Delete1(arg1, buffer);
                for (int i2=0 ; i2 < count ; i2++)
                  store2.MarkForDelayedRelease(buffer[i2]);
              }
            }
          }

          if (delete2Count != 0) {
            for (int i2=0 ; i2 < delete2Count ; i2++) {
              int arg2 = delete2List[i2];
              int count = table.Count2(arg2);
              if (count > 0) {
                store2.MarkForDelayedRelease(arg2, count);
                if (buffer.Length < count)
                  buffer = new int[Array.Capacity(buffer.Length, count)];
                table.Delete2(arg2, buffer);
                for (int i1=0 ; i1 < count ; i1++)
                  store1.MarkForDelayedRelease(buffer[i1]);
              }
            }
          }

          for (int i=0 ; i < deleteCount ; i++) {
            long entry = deleteList[i];
            int arg1 = Arg1(entry);
            int arg2 = Arg2(entry);
            if (table.Delete(arg1, arg2)) {
              store1.MarkForDelayedRelease(arg1);
              store2.MarkForDelayedRelease(arg2);
            }
          }
        }
      }

      for (int i=0 ; i < insertCount ; i++) {
        long entry = insertList[i];
        int arg1 = Arg1(entry);
        int arg2 = Arg2(entry);
        if (table.Insert(arg1, arg2)) {
          store1.AddRef(arg1);
          store2.AddRef(arg2);
        }
      }
    }

    public void Finish() {

    }

    //////////////////////////////////////////////////////////////////////////////

    public void Reset() {
      if (hasDeletes) {
        hasDeletes = false;

        if (clear)
          clear = false;

        if (deleteCount > 0) {
          deleteCount = 0;
          deleteListIsSorted = false;
          if (deleteList.Length > 1024)
            deleteList = Array.emptyLongArray;
        }

        if (delete1Count > 0) {
          delete1Count = 0;
          delete1ListIsSorted = false;
          if (delete1List.Length > 1024)
            delete1List = Array.emptyIntArray;
        }

        if (delete2Count > 0) {
          delete2Count = 0;
          delete1ListIsSorted = false;
          if (delete2List.Length > 1024)
            delete2List = Array.emptyIntArray;
        }

        if (buffer.Length > 1024)
          buffer = Array.emptyIntArray;
      }

      if (insertCount > 0) {
        insertCount = 0;
        insertListOrd = Ord.ORD_NONE;
        if (insertList.Length > 1024)
          insertList = Array.emptyLongArray;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool Contains(int arg1, int arg2) {
      if (WasInserted(arg1, arg2))
        return true;

      if (clear || WereDeleted1(arg1) || WereDeleted2(arg2) || WasDeleted(arg1, arg2))
        return false;

      return table.Contains(arg1, arg2);
    }

    public bool Contains1(int arg1) {
      return WasInserted1(arg1) || ContainsUndeleted1(arg1);
    }

    public bool Contains2(int arg2) {
      return WasInserted2(arg2) || ContainsUndeleted2(arg2);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////// These methods are reserved for foreign key checkers /////////////

    internal bool HasInsertions() {
      return insertCount > 0;
    }

    internal long[] Insertions(long[] buffer, int[] counter) {
      if (insertCount > buffer.Length)
        buffer = new long[insertCount];
      counter[0] = insertCount;
      Array.Copy(insertList, buffer, insertCount);
      return buffer;
    }

    internal bool WasCleared() {
      return clear;
    }

    internal bool HasPartialDeletes() {
      return deleteCount > 0 || delete1Count > 0 || delete2Count > 0;
    }

    internal long[] Deletes(long[] buffer, int[] counter) {
      int count = deleteCount;
      for (int i=0 ; i < delete1Count ; i++)
        count += table.Count1(delete1List[i]);
      for (int i=0 ; i < delete2Count ; i++)
        count += table.Count2(delete2List[i]);
      counter[0] = count;

      if (count > buffer.Length)
        buffer = new long[count];
      Array.Copy(deleteList, buffer, deleteCount);
      int idx = deleteCount;

      for (int i1=0 ; i1 < delete1Count ; i1++) {
        int arg1 = delete1List[i1];
        int[] arg2s = table.Restrict1(arg1);
        for (int i2=0 ; i2 < arg2s.Length ; i2++)
          buffer[idx++] = Tuple(arg1, arg2s[i2]);
      }

      for (int i2=0 ; i2 < delete2Count ; i2++) {
        int arg2 = delete2List[i2];
        int[] arg1s = table.Restrict2(arg2);
        for (int i1=0 ; i1 < arg1s.Length ; i1++)
          buffer[idx++] = Tuple(arg1s[i1], arg2);
      }

      Debug.Assert(idx == count);
      return buffer;
    }

    internal int[] Deletes1(int[] buffer, int[] counter) {
      int count = deleteCount + delete1Count;
      for (int i=0 ; i < delete2Count ; i++)
        count += table.Count2(delete2List[i]);
      counter[0] = count;
      if (count > buffer.Length)
        buffer = new int[count];
      int idx = 0;
      for (int i=0 ; i < deleteCount ; i++)
        buffer[idx++] = Arg1(deleteList[i]);
      for (int i=0 ; i < delete1Count ; i++)
        buffer[idx++] = delete1List[i];
      for (int i2=0 ; i2 < delete2Count ; i2++) {
        int[] arg1s = table.Restrict2(delete2List[i2]);
        for (int i1=0 ; i1 < arg1s.Length ; i1++)
          buffer[idx++] = arg1s[i1];
      }
      Debug.Assert(idx == count);
      return buffer;
    }

    internal int[] Deletes2(int[] buffer, int[] counter) {
      int count = deleteCount + delete2Count;
      for (int i=0 ; i < delete1Count ; i++)
        count += table.Count1(delete1List[i]);
      counter[0] = count;
      if (count > buffer.Length)
        buffer = new int[count];
      int idx = 0;
      for (int i=0 ; i < deleteCount ; i++)
        buffer[idx++] = Arg2(deleteList[i]);
      for (int i1=0 ; i1 < delete1Count ; i1++) {
        int[] arg2s = table.Restrict1(delete1List[i1]);
        for (int i2=0 ; i2 < arg2s.Length ; i2++)
          buffer[idx++] = arg2s[i2];
      }
      Debug.Assert(idx == count);
      return buffer;
    }

    internal int AnyDeletedArg1(int arg2) {
      //## NOT ESPECIALLY EFFICIENTS, BUT IT'S CALLED ONLY WHEN A FOREIGN KEY HAS BEEN VIOLATED
      int[] arg1s = table.Restrict2(arg2);
      for (int i=0 ; i < arg1s.Length ; i++)
        if (!Contains(arg1s[i], arg2))
          return arg1s[i];
      throw ErrorHandler.InternalFail();
    }

    internal int AnyDeletedArg2(int arg1) {
      //## NOT ESPECIALLY EFFICIENTS, BUT IT'S CALLED ONLY WHEN A FOREIGN KEY HAS BEEN VIOLATED
      int[] arg2s = table.Restrict1(arg1);
      for (int i=0 ; i < arg2s.Length ; i++)
        if (!Contains(arg1, arg2s[i]))
          return arg2s[i];
      throw ErrorHandler.InternalFail();
    }

    //////////////////////////////////////////////////////////////////////////////

    private bool ContainsUndeleted1(int arg1) {
      if (clear || WereDeleted1(arg1))
        return false;

      if (!table.Contains1(arg1))
        return false;

      //## BAD: THIS IS VERY INEFFICIENT IF THERE'S A LOT OF ENTRIES WHOSE FIRST ARGUMENT IS arg1
      int[] args2 = table.Restrict1(arg1);
      for (int i=0 ; i < args2.Length ; i++) {
        int arg2 = args2[i];
        if (WereDeleted2(arg2) || WasDeleted(arg1, arg2))
          return false;
      }

      return true;
    }

    private bool ContainsUndeleted2(int arg2) {
      if (clear || WereDeleted2(arg2))
        return false;

      if (!table.Contains2(arg2))
        return false;

      //## BAD: THIS IS VERY INEFFICIENT IF THERE'S A LOT OF ENTRIES WHOSE SECOND ARGUMENT IS arg2
      int[] args1 = table.Restrict2(arg2);
      for (int i=0 ; i < args1.Length ; i++) {
        int arg1 = args1[i];
        if (WereDeleted1(arg1) || WasDeleted(arg1, arg2))
          return false;
      }

      return true;
    }

    //////////////////////////////////////////////////////////////////////////////

    private bool WasInserted(int arg1, int arg2) {
      if (insertCount <= 16) {
        for (int i=0 ; i < insertCount ; i++) {
          long entry = insertList[i];
          if (arg1 == Arg1(entry) & arg2 == Arg2(entry))
            return true;
        }
        return false;
      }
      else {
        SortInsertList();
        return Array.SortedArrayContains(insertList, insertCount, Tuple(arg1, arg2));
      }
    }

    private bool WasInserted1(int arg1) {
      if (insertCount <= 16) {
        for (int i=0 ; i < insertCount ; i++) {
          long entry = insertList[i];
          if (arg1 == Arg1(entry))
            return true;
        }
        return false;
      }
      else {
        SortInsertList();
        return PackedIntPairs.ContainsMajor(insertList, insertCount, arg1);
      }
    }

    private bool WasInserted2(int arg2) {
      if (insertCount <= 16) {
        for (int i=0 ; i < insertCount ; i++) {
          long entry = insertList[i];
          if (arg2 == Arg2(entry))
            return true;
        }
        return false;
      }
      else {
        SortInsertListFlipped();
        return PackedIntPairs.ContainsMinor(insertList, insertCount, arg2);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private bool WasDeleted(int arg1, int arg2) {
      if (deleteCount <= 16) {
        for (int i=0 ; i < deleteCount ; i++) {
          long entry = deleteList[i];
          if (Arg1(entry) == arg1 & Arg2(entry) == arg2)
            return true;
        }
        return false;
      }
      else {
        SortDeleteList();
        return Array.SortedArrayContains(deleteList, deleteCount, Tuple(arg1, arg2));
      }
    }

    private bool WereDeleted1(int arg1) {
      if (delete1Count <= 16) {
        for (int i=0 ; i < delete1Count ; i++)
          if (delete1List[i] == arg1)
            return true;
        return false;
      }
      else {
        SortDelete1List();
        return Array.SortedArrayContains(delete1List, delete1Count, arg1);
      }
    }

    private bool WereDeleted2(int arg2) {
      if (delete2Count <= 16) {
        for (int i=0 ; i < delete2Count ; i++)
          if (delete2List[i] == arg2)
            return true;
        return false;
      }
      else {
        SortDelete2List();
        return Array.SortedArrayContains(delete2List, delete2Count, arg2);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private void SortInsertList() {
      if (insertListOrd != Ord.ORD_12) {
        Debug.Assert(insertListOrd == Ord.ORD_NONE);
        PackedIntPairs.Sort(insertList, insertCount);
        insertListOrd = Ord.ORD_12;
      }
    }

    private void SortInsertListFlipped() {
      if (insertListOrd != Ord.ORD_21) {
        PackedIntPairs.SortFlipped(insertList, insertCount);
        insertListOrd = Ord.ORD_21;
      }
    }

    private void SortDeleteList() {
      if (!deleteListIsSorted) {
        Array.Sort(deleteList, deleteCount);
        deleteListIsSorted = true;
      }
    }

    private void SortDelete1List() {
      if (!delete1ListIsSorted) {
        Array.Sort(delete1List, delete1Count);
        delete1ListIsSorted = true;
      }
    }

    private void SortDelete2List() {
      if (!delete2ListIsSorted) {
        Array.Sort(delete2List, delete2Count);
        delete2ListIsSorted = true;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public void CheckKey_1() {
      if (insertCount != 0) {
        SortInsertList();

        long prev = -1;
        for (int i=0 ; i < insertCount ; i++) {
          long entry = insertList[i];

          if (entry != prev) {
            int arg1 = Arg1(entry);

            if (arg1 == Arg1(prev))
              throw Col1KeyViolation(arg1, Arg2(entry), Arg2(prev));

            if (ContainsUndeleted1(arg1))
              throw Col1KeyViolation(arg1, Arg2(entry));
          }

          prev = entry;
        }
      }
    }

    public void CheckKey_2() {
      if (insertCount != 0) {
        SortInsertListFlipped();

        long prev = -1;
        for (int i=0 ; i < insertCount ; i++) {
          long entry = insertList[i];

          if (entry != prev) {
            int arg2 = Arg2(entry);

            if (arg2 == Arg2(prev))
              throw Col2KeyViolation(Arg1(entry), arg2, Arg1(prev));

            if (ContainsUndeleted2(arg2))
              throw Col2KeyViolation(Arg1(entry), arg2);
          }

          prev = entry;
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private KeyViolationException Col1KeyViolation(int surr1, int surr2, int otherArg2Surr) {
      return Col1KeyViolation(surr1, surr2, otherArg2Surr, true);
    }

    private KeyViolationException Col1KeyViolation(int surr1, int surr2) {
      return Col1KeyViolation(surr1, surr2, table.Restrict1(surr1)[0], false);
    }

    private KeyViolationException Col1KeyViolation(int surr1, int surr2, int otherArg2Surr, bool betweenNew) {
      //## BUG: STORES MAY CONTAIN ONLY PART OF THE ACTUAL VALUE (id(5) -> 5)
      Obj obj1 = store1.SurrToValue(surr1);
      Obj[] tuple1 = new Obj[] {obj1, store2.SurrToValue(surr2)};
      Obj[] tuple2 = new Obj[] {obj1, store2.SurrToValue(otherArg2Surr)};
      return new KeyViolationException(relvarName, KeyViolationException.key_1, tuple1, tuple2, betweenNew);
    }

    //////////////////////////////////////////////////////////////////////////////

    private KeyViolationException Col2KeyViolation(int surr1, int surr2, int otherArg1Surr) {
      return Col2KeyViolation(surr1, surr2, otherArg1Surr, true);
    }

    private KeyViolationException Col2KeyViolation(int surr1, int surr2) {
      return Col2KeyViolation(surr1, surr2, table.Restrict2(surr2)[0], false);
    }

    private KeyViolationException Col2KeyViolation(int surr1, int surr2, int otherArg1Surr, bool betweenNew) {
      //## BUG: STORES MAY CONTAIN ONLY PART OF THE ACTUAL VALUE (id(5) -> 5)
      Obj obj2 = store2.SurrToValue(surr2);
      Obj[] tuple1 = new Obj[] {store1.SurrToValue(surr1), obj2};
      Obj[] tuple2 = new Obj[] {store1.SurrToValue(otherArg1Surr), obj2};
      return new KeyViolationException(relvarName, KeyViolationException.key_2, tuple1, tuple2, betweenNew);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    // public void Dump(bool flipped) {
    //   System.out.Print("deleteList =");
    //   for (int i=0 ; i < deleteCount ; i++)
    //     if (flipped)
    //       System.out.Printf(" (%d, %d)", deleteList[2 * i + 1], deleteList[2 * i]);
    //     else
    //       System.out.Printf(" (%d, %d)", deleteList[2 * i], deleteList[2 * i + 1]);
    //   System.out.Println();

    //   System.out.Print("insertList =");
    //   for (int i=0 ; i < insertCount ; i++)
    //     if (flipped)
    //       System.out.Printf(" (%d, %d)", insertList[2 * i + 1], insertList[2 * i]);
    //     else
    //       System.out.Printf(" (%d, %d)", insertList[2 * i], insertList[2 * i + 1]);
    //   System.out.Println("\n");

    //   System.out.Print("deleteList =");
    //   for (int i=0 ; i < deleteCount ; i++) {
    //     int arg1 = deleteList[2 * i];
    //     int arg2 = deleteList[2 * i + 1];
    //     Obj obj1 = store1.SurrToValue(arg1);
    //     Obj obj2 = store2.SurrToValue(arg2);
    //     if (flipped) {
    //       Obj tmp = obj1;
    //       obj1 = obj2;
    //       obj2 = tmp;
    //     }
    //     System.out.Printf(" (%s, %s)", obj1.ToString(), obj2.ToString());
    //   }
    //   System.out.Println("");

    //   System.out.Print("insertList =");
    //   for (int i=0 ; i < insertCount ; i++) {
    //     int arg1 = insertList[2 * i];
    //     int arg2 = insertList[2 * i + 1];
    //     Obj obj1 = store1.SurrToValue(arg1);
    //     Obj obj2 = store2.SurrToValue(arg2);
    //     if (flipped) {
    //       Obj tmp = obj1;
    //       obj1 = obj2;
    //       obj2 = tmp;
    //     }
    //     System.out.Printf(" (%s, %s)",
    //       obj1 != null ? obj1.ToString() : "null",
    //       obj2 != null ? obj2.ToString() : "null"
    //     );
    //   }

    //   System.out.Printf("\n\n%s\n\n", table.Copy(flipped).ToString());

    //   // store1.Dump();
    //   // store2.Dump();
    // }
  }
}
