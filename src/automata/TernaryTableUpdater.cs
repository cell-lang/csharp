namespace Cell.Runtime {
  public class TernaryTableUpdater {
    static int[] emptyArray = new int[0];

    // bool clear = false;

    int deleteCount = 0;
    int[] deleteIdxs = emptyArray;
    int[] deleteList = emptyArray;

    internal int insertCount = 0;
    internal int[] insertList = emptyArray;

    internal string relvarName;

    internal TernaryTable table;
    internal ValueStoreUpdater store1, store2, store3;

    enum Ord {ORD_NONE, ORD_123, ORD_231, ORD_312};
    Ord currOrd = Ord.ORD_NONE;

    public TernaryTableUpdater(string relvarName, TernaryTable table, ValueStoreUpdater store1, ValueStoreUpdater store2, ValueStoreUpdater store3) {
      this.relvarName = relvarName;
      this.table = table;
      this.store1 = store1;
      this.store2 = store2;
      this.store3 = store3;
    }

    public void Clear() {
      deleteCount = 0;
      TernaryTable.Iter it = table.GetIter();
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, it.Get1(), it.Get2(), it.Get3());
        it.Next();
      }
    }

    public void Insert(int value1, int value2, int value3) {
      insertList = Array.Append3(insertList, insertCount++, value1, value2, value3);
    }

    public void Delete(int value1, int value2, int value3) {
      int idx = table.ContainsAt(value1, value2, value3);
      if (idx != -1) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, idx);
        deleteList = Array.Append3(deleteList, deleteCount++, value1, value2, value3);
      }
    }

    public void Delete12(int value1, int value2) {
      TernaryTable.Iter12 it = table.GetIter12(value1, value2);
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, value1, value2, it.Get1());
        it.Next();
      }
    }

    public void Delete13(int value1, int value3) {
      TernaryTable.Iter13 it = table.GetIter13(value1, value3);
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, value1, it.Get1(), value3);
        it.Next();
      }
    }

    public void Delete23(int value2, int value3) {
      TernaryTable.Iter23 it = table.GetIter23(value2, value3);
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, it.Get1(), value2, value3);
        it.Next();
      }
    }

    public void Delete1(int value1) {
      TernaryTable.Iter1 it = table.GetIter1(value1);
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, value1, it.Get1(), it.Get2());
        it.Next();
      }
    }

    public void Delete2(int arg2) {
      TernaryTable.Iter2 it = table.GetIter2(arg2);
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, it.Get1(), arg2, it.Get2());
        it.Next();
      }
    }

    public void Delete3(int value3) {
      TernaryTable.Iter3 it = table.GetIter3(value3);
      while (!it.Done()) {
        deleteIdxs = Array.Append(deleteIdxs, deleteCount, it.Index());
        deleteList = Array.Append3(deleteList, deleteCount++, it.Get1(), it.Get2(), value3);
        it.Next();
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    // void Dump(string msg) {
    //   if (Miscellanea.debugFlag) {
    //     System.out.Println();
    //     System.out.Println(msg);
    //     for (int i=0 ; i < deleteCount ; i++) {
    //       int idx = deleteIdxs[i];
    //       int offset = 3 * i;
    //       int arg0 = deleteList[offset];
    //       int arg1 = deleteList[offset+1];
    //       int arg2 = deleteList[offset+2];
    //       bool ok = table.Contains(arg0, arg1, arg2);
    //       System.out.Printf("delete (%d, %d, %d) @ %d, %s\n", arg0, arg1, arg2, idx, ok);
    //     }
    //     System.out.Println();
    //     for (int i=0 ; i < table.flatTuples.Length / 3 ; i++) {
    //       int offset = 3 * i;
    //       int arg0 = table.flatTuples[offset];
    //       int arg1 = table.flatTuples[offset+1];
    //       int arg2 = table.flatTuples[offset+2];
    //       System.out.Printf("%2d: (%2d, %2d, %2d)\n", i, arg0, arg1, arg2);
    //     }
    //     System.out.Printf("\ncurrOrd = %s\n", currOrd);
    //   }
    // }

    public void Apply() {
      if (currOrd == Ord.ORD_NONE) {
        // deleteList has not been reordered, so it still matches deleteIdxs
        for (int i=0 ; i < deleteCount ; i++)
          if (!table.DeleteAt(deleteIdxs[i]))
            deleteList[3*i] = -1;
      }
      else if (deleteCount != 0) {
        // deleteList was reorder, so the correspondence with deleteIdxs has been lost
        // On the other hand, since deleteList is now ordered, we can eliminate the duplicates

        int DEBUG_count_1 = 0;
        int DEBUG_count_2 = 0;

        for (int i=0 ; i < deleteCount ; i++)
          if (!table.DeleteAt(deleteIdxs[i]))
            DEBUG_count_1++;

        int prevArg1 = deleteList[0];
        int prevArg2 = deleteList[1];
        int prevArg3 = deleteList[2];
        for (int i=1 ; i < deleteCount ; i++) {
          int offset = 3 * i;
          int arg1 = deleteList[offset];
          int arg2 = deleteList[offset + 1];
          int arg3 = deleteList[offset + 2];
          if (arg1 == prevArg1 & arg2 == prevArg2 & arg3 == prevArg3) {
            deleteList[offset] = -1;
            DEBUG_count_2++;
          }
          else {
            prevArg1 = arg1;
            prevArg2 = arg2;
            prevArg3 = arg3;
          }
        }

        Debug.Assert(DEBUG_count_1 == DEBUG_count_2);
      }

      for (int i=0 ; i < insertCount ; i++) {
        int arg1 = insertList[3 * i];
        int arg2 = insertList[3 * i + 1];
        int arg3 = insertList[3 * i + 2];

        if (!table.Contains(arg1, arg2, arg3)) {
          table.Insert(arg1, arg2, arg3);
          store1.AddRef(arg1);
          store2.AddRef(arg2);
          store3.AddRef(arg3);
        }
      }
    }

    public void Finish() {
      for (int i=0 ; i < deleteCount ; i++) {
        int offset = 3 * i;
        int arg1 = deleteList[offset];
        if (arg1 != -1) {
          int arg2 = deleteList[offset+1];
          int arg3 = deleteList[offset+2];
          store1.Release(arg1);
          store2.Release(arg2);
          store3.Release(arg3);
        }
      }
    }

    public void Reset() {
      // clear = false;
      deleteCount = 0;
      insertCount = 0;

      if (deleteList.Length > 3 * 1024) {
        deleteIdxs = emptyArray;
        deleteList = emptyArray;
      }

      if (insertList.Length > 3 * 1024)
        insertList = emptyArray;

      currOrd = Ord.ORD_NONE;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public void Prepare123() {
      if (deleteCount != 0 | insertCount != 0) {
        Debug.Assert(currOrd == Ord.ORD_NONE | currOrd == Ord.ORD_123);
        if (currOrd != Ord.ORD_123) {
          Ints123.Sort(deleteList, deleteCount);
          Ints123.Sort(insertList, insertCount);
          currOrd = Ord.ORD_123;
        }
      }
    }

    public void Prepare231() {
      if (deleteCount != 0 | insertCount != 0) {
        Debug.Assert(currOrd != Ord.ORD_312);
        if (currOrd != Ord.ORD_231) {
          Ints231.Sort(deleteList, deleteCount);
          Ints231.Sort(insertList, insertCount);
          currOrd = Ord.ORD_231;
        }
      }
    }

    public void Prepare312() {
      if (deleteCount != 0 | insertCount != 0) {
        if (currOrd != Ord.ORD_312) {
          Ints312.Sort(deleteList, deleteCount);
          Ints312.Sort(insertList, insertCount);
          currOrd = Ord.ORD_312;
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public bool Contains1(int surr1) {
      Prepare123();

      if (Ints123.Contains1(insertList, insertCount, surr1))
        return true;

      if (!table.Contains1(surr1))
        return false;

      int idx = Ints123.IndexFirst1(deleteList, deleteCount, surr1);
      if (idx == -1)
        return true;
      int count = Ints123.Count1(deleteList, deleteCount, surr1, idx);

      TernaryTable.Iter it = table.GetIter1(surr1);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in both 1/2/3
        // and 2/3/1 order, since the first argument is always the same
        if (!Ints231.Contains23(deleteList, idx, count, it.Get1(), it.Get2()))
          return true;
        it.Next();
      }

      return false;
    }

    public bool Contains2(int surr2) {
      Prepare231();

      if (Ints231.Contains2(insertList, insertCount, surr2))
        return true;

      if (!table.Contains2(surr2))
        return false;

      int idx = Ints231.IndexFirst2(deleteList, deleteCount, surr2);
      if (idx == -1)
        return true;
      int count = Ints231.Count2(deleteList, deleteCount, surr2, idx);

      TernaryTable.Iter it = table.GetIter2(surr2);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in both 2/3/1
        // and 3/1/2 order, since the second argument is always the same
        if (!Ints312.Contains13(deleteList, idx, count, it.Get1(), it.Get2()))
          return true;
        it.Next();
      }

      return false;
    }

    public bool Contains3(int surr3) {
      Prepare312();

      if (Ints312.Contains3(insertList, insertCount, surr3))
        return true;

      if (!table.Contains3(surr3))
        return false;

      int idx = Ints312.IndexFirst3(deleteList, deleteCount, surr3);
      if (idx == -1)
        return true;
      int count = Ints312.Count3(deleteList, deleteCount, surr3, idx);

      TernaryTable.Iter it = table.GetIter3(surr3);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in both 3/1/2
        // and 1/2/3 order, since the third argument is always the same
        if (!Ints123.Contains12(deleteList, idx, count, it.Get1(), it.Get2()))
          return true;
        it.Next();
      }

      return false;
    }

    public bool Contains12(int surr1, int surr2) {
      Prepare123();

      if (Ints123.Contains12(insertList, insertCount, surr1, surr2))
        return true;

      if (!table.Contains12(surr1, surr2))
        return false;

      int idx = Ints123.IndexFirst12(deleteList, deleteCount, surr1, surr2);
      if (idx == -1)
        return true;
      int count = Ints123.Count12(deleteList, deleteCount, surr1, surr2, idx);

      TernaryTable.Iter it = table.GetIter12(surr1, surr2);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in both 1/2/3
        // and 3/1/2 order, since the first two arguments are the same
        if (!Ints312.Contains3(deleteList, idx, count, it.Get1()))
          return true;
        it.Next();
      }

      return false;
    }

    public bool Contains13(int surr1, int surr3) {
      Prepare312();

      if (Ints312.Contains13(insertList, insertCount, surr1, surr3))
        return true;

      if (!table.Contains13(surr1, surr3))
        return false;

      int idx = Ints312.IndexFirst31(deleteList, deleteCount, surr3, surr1);
      if (idx == -1)
        return true;
      int count = Ints312.Count13(deleteList, deleteCount, surr1, surr3, idx);

      TernaryTable.Iter it = table.GetIter13(surr1, surr3);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in both 3/1/2
        // and 2/3/1 order, since the first and last argument are the same
        if (!Ints231.Contains2(deleteList, idx, count, it.Get1()))
          return true;
        it.Next();
      }

      return false;
    }

    public bool Contains23(int surr2, int surr3) {
      Prepare231();

      if (Ints231.Contains23(insertList, insertCount, surr2, surr3))
        return true;

      if (!table.Contains23(surr2, surr3))
        return false;

      int idx = Ints231.IndexFirst23(deleteList, deleteCount, surr2, surr3);
      if (idx == -1)
        return true;
      int count = Ints231.Count23(deleteList, deleteCount, surr2, surr3, idx);

      TernaryTable.Iter it = table.GetIter23(surr2, surr3);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in any order, since two arguments are the same
        if (!Ints123.Contains1(deleteList, idx, count, it.Get1()))
          return true;
        it.Next();
      }

      return false;
    }

    //////////////////////////////////////////////////////////////////////////////

    public int LookupAny12(int surr1, int surr2) {
      Prepare123();

      if (Ints123.Contains12(insertList, insertCount, surr1, surr2)) {
        int idxFirst = Ints123.IndexFirst12(insertList, insertCount, surr1, surr2);
        return insertList[3 * idxFirst + 2];
      }

      int idx = Ints123.IndexFirst12(deleteList, deleteCount, surr1, surr2);
      if (idx == -1)
        return table.GetIter12(surr1, surr2).Get1();

      int count = Ints123.Count12(deleteList, deleteCount, surr1, surr2, idx);

      TernaryTable.Iter12 it = table.GetIter12(surr1, surr2);
      while (!it.Done()) {
        // Tuples in the [idx, idx+count) range are sorted in both 1/2/3
        // and 3/1/2 order, since the first two arguments are the same
        if (!Ints312.Contains3(deleteList, idx, count, it.Get1()))
          return it.Get1();
        it.Next();
      }

      throw ErrorHandler.InternalFail();
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public void CheckKey_12() {
      if (insertCount != 0) {
        Prepare123();

        int prevArg1 = -1;
        int prevArg2 = -1;
        int prevArg3 = -1;

        for (int i=0 ; i < insertCount ; i++) {
          int arg1 = insertList[3 * i];
          int arg2 = insertList[3 * i + 1];
          int arg3 = insertList[3 * i + 2];

          if (arg1 == prevArg1 & arg2 == prevArg2 & arg3 != prevArg3)
            throw Cols12KeyViolationException(arg1, arg2, arg3, prevArg3);

          if (!Ints123.Contains12(deleteList, deleteCount, arg1, arg2) && table.Contains12(arg1, arg2))
            throw Cols12KeyViolationException(arg1, arg2, arg3);

          prevArg1 = arg1;
          prevArg2 = arg2;
          prevArg3 = arg3;
        }
      }
    }

    public void CheckKey_3() {
      if (insertCount != 0) {
        Prepare312();

        int prevArg1 = -1;
        int prevArg2 = -1;
        int prevArg3 = -1;

        for (int i=0 ; i < insertCount ; i++) {
          int arg1 = insertList[3 * i];
          int arg2 = insertList[3 * i + 1];
          int arg3 = insertList[3 * i + 2];

          if (arg3 == prevArg3 & (arg1 != prevArg1 | arg2 != prevArg2))
            throw Col3KeyViolationException(arg1, arg2, arg3, prevArg1, prevArg2);

          if (!Ints312.Contains3(deleteList, deleteCount, arg3) && table.Contains3(arg3))
            throw Col3KeyViolationException(arg1, arg2, arg3);

          prevArg1 = arg1;
          prevArg2 = arg2;
          prevArg3 = arg3;
        }
      }
    }

    public void CheckKey_23() {
      if (insertCount != 0) {
        Prepare231();

        int prevArg1 = -1;
        int prevArg2 = -1;
        int prevArg3 = -1;

        for (int i=0 ; i < insertCount ; i++) {
          int arg1 = insertList[3 * i];
          int arg2 = insertList[3 * i + 1];
          int arg3 = insertList[3 * i + 2];

          if (arg2 == prevArg2 & arg3 == prevArg3 & arg1 != prevArg1)
            throw Cols23KeyViolationException(arg1, arg2, arg3, prevArg1);

          if (!Ints231.Contains23(deleteList, deleteCount, arg2, arg3) && table.Contains23(arg2, arg3))
            throw Cols23KeyViolationException(arg1, arg2, arg3);

          prevArg1 = arg1;
          prevArg2 = arg2;
          prevArg3 = arg3;
        }
      }
    }

    public void CheckKey_13() {
      if (insertCount != 0) {
        Prepare312();

        int prevArg1 = -1;
        int prevArg2 = -1;
        int prevArg3 = -1;

        for (int i=0 ; i < insertCount ; i++) {
          int arg1 = insertList[3 * i];
          int arg2 = insertList[3 * i + 1];
          int arg3 = insertList[3 * i + 2];

          if (arg1 == prevArg1 & arg3 == prevArg3 & arg2 != prevArg2)
            throw Cols13KeyViolationException(arg1, arg2, arg3, prevArg2);

          if (!Ints312.Contains13(deleteList, deleteCount, arg1, arg3) && table.Contains13(arg1, arg3))
            throw Cols13KeyViolationException(arg1, arg2, arg3);

          prevArg1 = arg1;
          prevArg2 = arg2;
          prevArg3 = arg3;
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public interface DeleteChecker {
      void MightHaveBeenDeleted(int surr1, int surr2, int surr3);
    }

    public void CheckDeletes123(DeleteChecker deleteChecker) {
      Prepare123();
      CheckDeletes(deleteChecker);
    }

    public void CheckDeletes231(DeleteChecker deleteChecker) {
      Prepare231();
      CheckDeletes(deleteChecker);
    }

    public void CheckDeletes312(DeleteChecker deleteChecker) {
      Prepare312();
      CheckDeletes(deleteChecker);
    }

    private void CheckDeletes(DeleteChecker deleteChecker) {
      for (int i=0 ; i < deleteCount ; i++) {
        int offset = 3 * i;
        int surr1 = deleteList[offset];
        int surr2 = deleteList[offset+1];
        int surr3 = deleteList[offset+2];
        deleteChecker.MightHaveBeenDeleted(surr1, surr2, surr3);
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private KeyViolationException Cols12KeyViolationException(int arg1, int arg2, int arg3, int otherArg3) {
      return NewKeyViolationException(arg1, arg2, arg3, arg1, arg2, otherArg3, KeyViolationException.key_12, true);
    }

    private KeyViolationException Cols12KeyViolationException(int arg1, int arg2, int arg3) {
      int otherArg3 = table.Lookup12(arg1, arg2);
      return NewKeyViolationException(arg1, arg2, arg3, arg1, arg2, otherArg3, KeyViolationException.key_12, false);
    }

    private KeyViolationException Col3KeyViolationException(int arg1, int arg2, int arg3, int otherArg1, int otherArg2) {
      return NewKeyViolationException(arg1, arg2, arg3, otherArg1, otherArg2, arg3, KeyViolationException.key_3, true);
    }

    private KeyViolationException Col3KeyViolationException(int arg1, int arg2, int arg3) {
      TernaryTable.Iter3 it = table.GetIter3(arg3);
      int otherArg1 = it.Get1();
      int otherArg2 = it.Get2();
      return NewKeyViolationException(arg1, arg2, arg3, otherArg1, otherArg2, arg3, KeyViolationException.key_3, false);
    }

    private KeyViolationException Cols23KeyViolationException(int arg1, int arg2, int arg3, int otherArg1) {
      return NewKeyViolationException(arg1, arg2, arg3, otherArg1, arg2, arg3, KeyViolationException.key_23, true);
    }

    private KeyViolationException Cols23KeyViolationException(int arg1, int arg2, int arg3) {
      int otherArg1 = table.Lookup23(arg2, arg3);
      return NewKeyViolationException(arg1, arg2, arg3, otherArg1, arg2, arg3, KeyViolationException.key_23, false);
    }

    private KeyViolationException Cols13KeyViolationException(int arg1, int arg2, int arg3, int otherArg2) {
      return NewKeyViolationException(arg1, arg2, arg3, arg1, otherArg2, arg3, KeyViolationException.key_13, true);
    }

    private KeyViolationException Cols13KeyViolationException(int arg1, int arg2, int arg3) {
      int otherArg2 = table.Lookup13(arg1, arg3);
      return NewKeyViolationException(arg1, arg2, arg3, arg1, otherArg2, arg3, KeyViolationException.key_13, false);
    }

    private KeyViolationException NewKeyViolationException(int arg1, int arg2, int arg3, int otherArg1, int otherArg2, int otherArg3, int[] key, bool betweenNew) {
      //## BUG: STORES MAY CONTAIN ONLY PART OF THE ACTUAL VALUE (id(5) -> 5)
      Obj obj1 = store1.SurrToValue(arg1);
      Obj obj2 = store2.SurrToValue(arg2);
      Obj obj3 = store3.SurrToValue(arg3);

      Obj otherObj1 = arg1 == otherArg1 ? obj1 : store1.SurrToValue(otherArg1);
      Obj otherObj2 = arg2 == otherArg2 ? obj2 : store2.SurrToValue(otherArg2);
      Obj otherObj3 = arg3 == otherArg3 ? obj3 : store3.SurrToValue(otherArg3);

      Obj[] tuple1 = new Obj[] {obj1, obj2, obj3};
      Obj[] tuple2 = new Obj[] {otherObj1, otherObj2, otherObj3};

      return new KeyViolationException(relvarName, key, tuple1, tuple2, betweenNew);
    }
  }
}
