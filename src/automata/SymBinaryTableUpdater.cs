namespace Cell.Runtime {
  public class SymBinaryTableUpdater {
    static int[] emptyArray = new int[0];

    int deleteCount = 0;
    int[] deleteList = emptyArray;

    internal int insertCount = 0;
    internal int[] insertList = emptyArray;
    int[] insertList_1_2;

    bool prepared = false;

    internal string relvarName;

    internal SymBinaryTable table;
    internal ValueStoreUpdater store;


    public SymBinaryTableUpdater(string relvarName, SymBinaryTable table, ValueStoreUpdater store) {
      this.relvarName = relvarName;
      this.table = table;
      this.store = store;
    }

    public void Clear() {
      deleteList = table.RawCopy();
      deleteCount = deleteList.Length / 2;
    }

    public void Delete(int value1, int value2) {
      if (table.Contains(value1, value2)) {
        bool swap = value1 > value2;
        int minorVal = swap ? value2 : value1;
        int majorVal = swap ? value1 : value2;
        deleteList = Array.Append2(deleteList, deleteCount++, minorVal, majorVal);
      }
    }

    public void Delete(int value) {
      int[] assocs = table.Restrict(value);
      for (int i=0 ; i < assocs.Length ; i++) {
        int otherVal = assocs[i];
        bool swap = value > otherVal;
        int minorVal = swap ? otherVal : value;
        int majorVal = swap ? value : otherVal;
        deleteList = Array.Append2(deleteList, deleteCount++, minorVal, majorVal);
      }
    }

    public void Insert(int value1, int value2) {
      bool swap = value1 > value2;
      int minorVal = swap ? value2 : value1;
      int majorVal = swap ? value1 : value2;
      insertList = Array.Append2(insertList, insertCount++, minorVal, majorVal);
    }

    public void Apply() {
      for (int i=0 ; i < deleteCount ; i++) {
        int field1 = deleteList[2 * i];
        int field2 = deleteList[2 * i + 1];
        if (table.Contains(field1, field2))
          table.Delete(field1, field2);
        else
          deleteList[2 * i] = -1;
      }

      for (int i=0 ; i < insertCount ; i++) {
        int field1 = insertList[2 * i];
        int field2 = insertList[2 * i + 1];
        if (!table.Contains(field1, field2)) {
          table.Insert(field1, field2);
          store.AddRef(field1);
          store.AddRef(field2);
        }
      }
    }

    public void Finish() {
      for (int i=0 ; i < deleteCount ; i++) {
        int field1 = deleteList[2 * i];
        if (field1 != -1) {
          int field2 = deleteList[2 * i + 1];
          // Debug.Assert(table.store.SurrToObjValue(field1) != null);
          // Debug.Assert(table.store.SurrToObjValue(field2) != null);
          store.Release(field1);
          store.Release(field2);
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Reset() {
      deleteCount = 0;
      insertCount = 0;

      if (deleteList.Length > 2 * 1024)
        deleteList = emptyArray;
      if (insertList.Length > 2 * 1024)
        insertList = emptyArray;

      insertList_1_2 = null;

      prepared = false;
    }

    public void Prepare() {
      if (!prepared) {
        Ints12.Sort(deleteList, deleteCount);
        Ints12.Sort(insertList, insertCount);
        prepared = true;
      }
    }

    private void prepareInsert_1_2() {
      if (insertList_1_2 == null)
        if (insertCount > 0) {
          insertList_1_2 = Array.Take(insertList, insertCount);
          Array.Sort(insertList_1_2);
        }
        else
          insertList_1_2 = emptyArray;
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool Contains(int surr1, int surr2) {
      Prepare();

      if (surr1 > surr2) {
        int tmp = surr1;
        surr1 = surr2;
        surr2 = tmp;
      }

      if (Ints12.Contains(insertList, insertCount, surr1, surr2))
        return true;

      if (Ints12.Contains(deleteList, deleteCount, surr1, surr2))
        return false;

      return table.Contains(surr1, surr2);
    }

    public bool Contains(int surr) {
      prepareInsert_1_2();

      if (Array.SortedArrayContains(insertList_1_2, insertCount, surr))
        return true;

      if (!table.Contains(surr))
        return false;

      Prepare();

      //## BAD: THIS IS VERY INEFFICIENT IF THERE'S A LOT OF ENTRIES WHOSE FIRST ARGUMENT IS surr
      int[] surrs = table.Restrict(surr);
      for (int i=0 ; i < surrs.Length ; i++) {
        int surr1 = surr;
        int surr2 = surrs[i];
        if (surr1 > surr2) {
          surr1 = surr2;
          surr2 = surr;
        }
        if (!Ints12.Contains(deleteList, deleteCount, surr1, surr2))
          return true;
      }

      return false;
    }

    //////////////////////////////////////////////////////////////////////////////

    public interface DeleteChecker {
      void CheckDelete(int surr1, int surr2, SymBinaryTableUpdater target);
    }

    public void CheckDeletes(DeleteChecker deleteChecker) {
      // Needs to be called before iterating through deleteList, otherwise a call to
      // CheckDelete() -> Contains() -> Prepare() could reorder it while it's being iterated on
      Prepare();

      for (int i=0 ; i < deleteCount ; i++) {
        int offset = 2 * i;
        int surr1 = deleteList[offset];
        int surr2 = deleteList[offset + 1];
        deleteChecker.CheckDelete(surr1, surr2, this);
      }
    }
  }
}
