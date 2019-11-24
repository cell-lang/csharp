namespace Cell.Runtime {
  public abstract class ValueStoreUpdater {
    ValueStore store;

    int deferredCount = 0;
    int[] deferredReleases = Array.emptyIntArray;

    int batchDeferredCount = 0;
    long[] batchDeferredReleases = Array.emptyLongArray;

    //////////////////////////////////////////////////////////////////////////////

    private static long Entry(int index, int count) {
      return Miscellanea.Pack(index, count);
    }

    private static int Index(long entry) {
      return Miscellanea.Low(entry);
    }

    private static int Count(long entry) {
      return Miscellanea.High(entry);
    }

    //////////////////////////////////////////////////////////////////////////////

    protected ValueStoreUpdater(ValueStore store) {
      this.store = store;
    }

    public void AddRef(int index) {
      store.AddRef(index);
    }

    public void Release(int index) {
      store.Release(index);
    }

    public void Release(int index, int count) {
      store.Release(index, count);
    }

    public void MarkForDelayedRelease(int index) {
      if (!store.TryRelease(index))
        deferredReleases = Array.Append(deferredReleases, deferredCount++, index);
    }

    public void MarkForDelayedRelease(int index, int count) {
      if (!store.TryRelease(index, count))
        batchDeferredReleases = Array.Append(batchDeferredReleases, batchDeferredCount++, Entry(index, count));
    }

    public void ApplyDelayedReleases() {
      if (deferredCount > 0) {
        for (int i=0 ; i < deferredCount ; i++)
          Release(deferredReleases[i]);
        deferredCount = 0;
        if (deferredReleases.Length > 1024)
          deferredReleases = Array.emptyIntArray;
      }

      if (batchDeferredCount > 0) {
        for (int i=0 ; i < batchDeferredCount ; i++) {
          long entry = batchDeferredReleases[i];
          Release(Index(entry), Count(entry));
        }
        batchDeferredCount = 0;
        if (batchDeferredReleases.Length > 1024)
          batchDeferredReleases = Array.emptyLongArray;
      }
    }

    public abstract Obj SurrToValue(int surr);
  }
}
