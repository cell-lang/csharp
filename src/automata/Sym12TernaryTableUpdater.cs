namespace Cell.Runtime {
  public class Sym12TernaryTableUpdater {
    static int[] emptyArray = new int[0];

    // bool clear = false;

    int deleteCount = 0;
    int[] deleteList = emptyArray;
    int[] deleteList3;

    internal int insertCount = 0;
    internal int[] insertList = emptyArray;
    int[] insertList12;
    int[] insertList3;

    internal string relvarName;

    internal Sym12TernaryTable table;
    internal ValueStoreUpdater store12, store3;

    bool prepared = false;

    public Sym12TernaryTableUpdater(string relvarName, Sym12TernaryTable table, ValueStoreUpdater store12, ValueStoreUpdater store3) {
      this.relvarName = relvarName;
      this.table = table;
      this.store12 = store12;
      this.store3 = store3;
    }

    public void Clear() {
      deleteCount = 0;
      Sym12TernaryTable.Iter123 it = table.GetIter();
      while (!it.Done()) {
        deleteList = Array.Append3(deleteList, deleteCount++, it.Get1(), it.Get2(), it.Get3());
        it.Next();
      }
    }

    public void Insert(int value1, int value2, int value3) {
      if (value1 > value2) {
        int tmp = value1;
        value1 = value2;
        value2 = tmp;
      }
      insertList = Array.Append3(insertList, insertCount++, value1, value2, value3);
    }

    public void Delete(int value1, int value2, int value3) {
      if (value1 > value2) {
        int tmp = value1;
        value1 = value2;
        value2 = tmp;
      }
      if (table.Contains(value1, value2, value3))
        deleteList = Array.Append3(deleteList, deleteCount++, value1, value2, value3);
    }

    public void Delete12(int value1, int value2) {
      if (value1 > value2) {
        int tmp = value1;
        value1 = value2;
        value2 = tmp;
      }
      Sym12TernaryTable.Iter12 it = table.GetIter12(value1, value2);
      while (!it.Done()) {
        deleteList = Array.Append3(deleteList, deleteCount++, value1, value2, it.Get1());
        it.Next();
      }
    }

    public void Delete_13_23(int arg12, int arg3) {
      Sym12TernaryTable.Iter it = table.getIter_13_23(arg12, arg3);
      while (!it.Done()) {
        int arg1 = arg12;
        int arg2 = it.Get1();
        if (arg1 > arg2) {
          arg1 = arg2;
          arg2 = arg12;
        }
        deleteList = Array.Append3(deleteList, deleteCount++, arg1, arg2, arg3);
        it.Next();
      }
    }

    public void Delete_1_2(int arg12) {
      Sym12TernaryTable.Iter it = table.getIter_1_2(arg12);
      while (!it.Done()) {
        int arg1 = arg12;
        int arg2 = it.Get1();
        int arg3 = it.Get2();
        if (arg1 > arg2) {
          arg1 = arg2;
          arg2 = arg12;
        }
        deleteList = Array.Append3(deleteList, deleteCount++, arg1, arg2, arg3);
        it.Next();
      }
    }

    public void Delete3(int value3) {
      Sym12TernaryTable.Iter3 it = table.GetIter3(value3);
      while (!it.Done()) {
        Debug.Assert(it.Get1() <= it.Get2());
        deleteList = Array.Append3(deleteList, deleteCount++, it.Get1(), it.Get2(), value3);
        it.Next();
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public void Apply() {
      for (int i=0 ; i < deleteCount ; i++) {
        int arg1 = deleteList[3 * i];
        int arg2 = deleteList[3 * i + 1];
        int arg3 = deleteList[3 * i + 2];
        if (table.Contains(arg1, arg2, arg3))
          table.Delete(arg1, arg2, arg3);
        else
          deleteList[3 * i] = -1;
      }

      for (int i=0 ; i < insertCount ; i++) {
        int arg1 = insertList[3 * i];
        int arg2 = insertList[3 * i + 1];
        int arg3 = insertList[3 * i + 2];

        if (!table.Contains(arg1, arg2, arg3)) {
          table.Insert(arg1, arg2, arg3);
          store12.AddRef(arg1);
          store12.AddRef(arg2);
          store3.AddRef(arg3);
        }
      }
    }

    public void Finish() {
      for (int i=0 ; i < deleteCount ; i++) {
        int arg1 = deleteList[3 * i];
        if (arg1 != -1) {
          int arg2 = deleteList[3 * i + 1];
          int arg3 = deleteList[3 * i + 2];
          store12.Release(arg1);
          store12.Release(arg2);
          store3.Release(arg3);
        }
      }
    }

    public void Reset() {
      // clear = false;
      deleteCount = 0;
      insertCount = 0;

      if (deleteList.Length > 3 * 1024)
        deleteList = emptyArray;
      if (insertList.Length > 3 * 1024)
        insertList = emptyArray;

      deleteList3 = null;
      insertList12 = null;
      insertList3 = null;

      prepared = false;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void Prepare() {
      if (!prepared) {
        for (int i=0 ; i < deleteCount ; i++) {
          Debug.Assert(deleteList[3 * i] <= deleteList[3 * i + 1]);
        }
        Ints123.Sort(deleteList, deleteCount);
        Ints123.Sort(insertList, insertCount);
        prepared = true;
      }
    }

    private void PrepareDelete3() {
      if (deleteList3 == null)
        if (deleteCount > 0) {
          deleteList3 = new int[deleteCount];
          for (int i=0 ; i < deleteCount ; i++)
            deleteList3[i] = deleteList[3 * i + 2];
          Array.Sort(deleteList3);
        }
        else
          deleteList3 = emptyArray;
    }

    private void PrepareInsert12() {
      if (insertList12 == null)
        if (insertCount > 0) {
          insertList12 = new int[2 * insertCount];
          for (int i=0 ; i < insertCount ; i++) {
            insertList12[2 * i] = insertList[3 * i];
            insertList12[2 * i + 1] = insertList[3 * i + 1];
          }
          Array.Sort(insertList12);
        }
        else
          insertList12 = emptyArray;
    }

    private void PrepareInsert3() {
      if (insertList3 == null)
        if (insertCount > 0) {
          insertList3 = new int[insertCount];
          for (int i=0 ; i < insertCount ; i++)
            insertList3[i] = insertList[3 * i + 2];
          Array.Sort(insertList3);
        }
        else
          insertList3 = emptyArray;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public bool Contains12(int surr1, int surr2) {
      if (surr1 > surr2) {
        int tmp = surr1;
        surr1 = surr2;
        surr2 = tmp;
      }

      Prepare();
      if (Ints123.Contains12(insertList, insertCount, surr1, surr2))
        return true;

      if (table.Contains12(surr1, surr2)) {
        Sym12TernaryTable.Iter12 it = table.GetIter12(surr1, surr2);
        Debug.Assert(!it.Done());
        do {
          if (!Ints123.Contains(deleteList, deleteCount, surr1, surr2, it.Get1()))
            return true;
          it.Next();
        } while (!it.Done());
      }

      return false;
    }

    public bool contains_1_2(int arg12) {
      PrepareInsert12();
      if (Array.SortedArrayContains(insertList12, arg12))
        return true;

      if (table.contains_1_2(arg12)) {
        //## THIS COULD BE MADE FASTER BY CHECKING FIRST WHETHER arg12 APPEARS IN THE DELETE LIST AT ALL
        Prepare();
        Sym12TernaryTable.Iter it = table.getIter_1_2(arg12);
        Debug.Assert(!it.Done());
        do {
          int arg1 = arg12;
          int arg2 = it.Get1();
          int arg3 = it.Get2();
          if (arg1 >= arg2) {
            arg1 = arg2;
            arg2 = arg12;
          }
          if (!Ints123.Contains(deleteList, deleteCount, arg1, arg2, arg3))
            return true;
          it.Next();
        } while (!it.Done());
      }

      return false;
    }

    public bool Contains3(int surr3) {
      PrepareInsert3();

      if (Array.SortedArrayContains(insertList3, surr3))
        return true;

      if (table.Contains3(surr3)) {
        //## THIS COULD BE MADE FASTER BY CHECKING FIRST WHETHER surr3 APPEARS IN THE DELETE LIST AT ALL
        Prepare();
        Sym12TernaryTable.Iter3 it = table.GetIter3(surr3);
        Debug.Assert(!it.Done());
        do {
          if (!Ints123.Contains(deleteList, deleteCount, it.Get1(), it.Get2(), surr3))
            return true;
          it.Next();
        } while (!it.Done());
      }

      return false;
    }

    //////////////////////////////////////////////////////////////////////////////

    public int LookupAny12(int surr1, int surr2) {
      if (surr1 > surr2) {
        int tmp = surr1;
        surr1 = surr2;
        surr2 = tmp;
      }

      Prepare();

      if (Ints123.Contains12(insertList, insertCount, surr1, surr2)) {
        int idxFirst = Ints123.IndexFirst12(insertList, insertCount, surr1, surr2);
        return insertList[3 * idxFirst + 2];
      }

      if (table.Contains12(surr1, surr2)) {
        Sym12TernaryTable.Iter12 it = table.GetIter12(surr1, surr2);
        Debug.Assert(!it.Done());
        do {
          if (!Ints123.Contains(deleteList, deleteCount, surr1, surr2, it.Get1()))
            return it.Get1();
          it.Next();
        } while (!it.Done());
      }

      throw ErrorHandler.InternalFail();
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public void CheckKey_12() {
      if (insertCount != 0) {
        Prepare();

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
        PrepareDelete3();

        int prevArg1 = -1;
        int prevArg2 = -1;
        int prevArg3 = -1;

        for (int i=0 ; i < insertCount ; i++) {
          int arg1 = insertList[3 * i];
          int arg2 = insertList[3 * i + 1];
          int arg3 = insertList[3 * i + 2];

          if (arg3 == prevArg3 & (arg1 != prevArg1 | arg2 != prevArg2))
            throw Col3KeyViolationException(arg1, arg2, arg3, prevArg1, prevArg2);

          if (!Array.SortedArrayContains(deleteList, arg3) && table.Contains3(arg3))
            throw Col3KeyViolationException(arg1, arg2, arg3);

          prevArg1 = arg1;
          prevArg2 = arg2;
          prevArg3 = arg3;
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public interface DeleteChecker {
      void CheckDelete(int surr1, int surr2, int surr3, Sym12TernaryTableUpdater updater);
    }

    public void CheckDeletes(DeleteChecker deleteChecker) {
      // Needs to be called before iterating through deleteList, otherwise a call to
      // CheckDelete() -> Contains12() -> Prepare() could reorder it while it's being iterated on
      Prepare();

      for (int i=0 ; i < deleteCount ; i++) {
        int offset = 3 * i;
        int surr1 = deleteList[offset];
        int surr2 = deleteList[offset + 1];
        int surr3 = deleteList[offset + 2];
        deleteChecker.CheckDelete(surr1, surr2, surr3, this);
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
      Sym12TernaryTable.Iter3 it = table.GetIter3(arg3);
      int otherArg1 = it.Get1();
      int otherArg2 = it.Get2();
      return NewKeyViolationException(arg1, arg2, arg3, otherArg1, otherArg2, arg3, KeyViolationException.key_3, false);
    }

    private KeyViolationException NewKeyViolationException(int arg1, int arg2, int arg3, int otherArg1, int otherArg2, int otherArg3, int[] key, bool betweenNew) {
      //## BUG: STORES MAY CONTAIN ONLY PART OF THE ACTUAL VALUE (id(5) -> 5)
      Obj obj1 = store12.SurrToValue(arg1);
      Obj obj2 = store12.SurrToValue(arg2);
      Obj obj3 = store3.SurrToValue(arg3);

      Obj otherObj1 = arg1 == otherArg1 ? obj1 : store12.SurrToValue(otherArg1);
      Obj otherObj2 = arg2 == otherArg2 ? obj2 : store12.SurrToValue(otherArg2);
      Obj otherObj3 = arg3 == otherArg3 ? obj3 : store3.SurrToValue(otherArg3);

      Obj[] tuple1 = new Obj[] {obj1, obj2, obj3};
      Obj[] tuple2 = new Obj[] {otherObj1, otherObj2, otherObj3};

      return new KeyViolationException(relvarName, key, tuple1, tuple2, betweenNew);
    }
  }
}
