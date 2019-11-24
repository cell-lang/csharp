namespace Cell.Runtime {
  public class UnaryTableUpdater {
    static int[] emptyArray = new int[0];

    internal bool clear = false;
    long[] bitmapCopy = null;

    int deleteCount = 0;
    int[] deleteList = emptyArray;

    internal int insertCount = 0;
    internal int[] insertList = emptyArray;

    bool prepared = false;

    internal string relvarName;
    internal UnaryTable table;
    internal ValueStoreUpdater store;


    public UnaryTableUpdater(string relvarName, UnaryTable table, ValueStoreUpdater store) {
      this.relvarName = relvarName;
      this.table = table;
      this.store = store;
    }

    public void Clear() {
      clear = true;
      deleteCount = 0;
    }

    public void Delete(long value) {
      if (!clear || table.Contains((int) value))
        deleteList = Array.Append(deleteList, deleteCount++, (int) value);
    }

    public void Insert(long value) {
      insertList = Array.Append(insertList, insertCount++, (int) value);
    }

    public void Apply() {
      if (clear) {
        int max = 0;
        for (int i=0 ; i < insertCount ; i++) {
          int surr = insertList[i];
          if (surr > max)
            max = surr;
        }
        bitmapCopy = table.Clear(max + 1);
      }
      else {
        for (int i=0 ; i < deleteCount ; i++) {
          int surr = deleteList[i];
          if (table.Contains(surr))
            table.Delete(surr);
          else
            deleteList[i] = -1;
        }
      }

      for (int i=0 ; i < insertCount ; i++) {
        int surr = insertList[i];
        if (!table.Contains(surr)) {
          table.Insert(surr);
          store.AddRef(surr);
        }
      }
    }

    public void Finish() {
      if (clear) {
        int len = bitmapCopy.Length;
        for (int i=0 ; i < len ; i++) {
          long mask = bitmapCopy[i];
          int offset = 64 * i;
          for (int j=0 ; j < 64 ; j++)
            if (Miscellanea.BitIsSet64(mask, j))
              store.Release(offset + j);
        }
      }
      else {
        for (int i=0 ; i < deleteCount ; i++) {
          int surr = deleteList[i];
          if (surr != -1)
            store.Release(surr);
        }
      }
    }

    public void Reset() {
      clear = false;
      bitmapCopy = null;

      deleteCount = 0;
      insertCount = 0;

      if (deleteList.Length > 1024)
        deleteList = emptyArray;
      if (insertList.Length > 1024)
        insertList = emptyArray;

      prepared = false;
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Prepare() {
      if (!prepared) {
        prepared = true;
        Array.Sort(deleteList, deleteCount);
        Array.Sort(insertList, insertCount);
      }
    }

    public bool Contains(int surr) {
      Prepare();

      if (Array.SortedArrayContains(insertList, insertCount, surr))
        return true;

      if (clear || Array.SortedArrayContains(deleteList, deleteCount, surr))
        return false;

      return table.Contains(surr);
    }

    //////////////////////////////////////////////////////////////////////////////

    public interface DeleteChecker {
      // arg1 is guaranteed to have been deleted and not reinserted
      void WasDeleted(int surr);
    }

    public void CheckDeletedKeys(DeleteChecker deleteChecker) {
      Prepare();

      if (clear) {
        UnaryTable.Iter it = table.GetIter();
        while (!it.Done()) {
          int surr = it.Get();
          if (Array.SortedArrayContains(insertList, insertCount, surr))
            deleteChecker.WasDeleted(surr);
          it.Next();
        }
      }
      else {
        for (int i=0 ; i < deleteCount ; i++) {
          int surr = deleteList[i];
          if (Array.SortedArrayContains(insertList, insertCount, surr))
            deleteChecker.WasDeleted(surr);
        }
      }
    }
  }
}
