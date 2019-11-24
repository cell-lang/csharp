namespace Cell.Runtime {
  public abstract class ValueStore {
    private byte[] references;
    private IntCtrs extraRefs;

    //////////////////////////////////////////////////////////////////////////////

    public ValueStore(int capacity) {
      references = new byte[capacity];
      extraRefs  = new IntCtrs();
    }

    //////////////////////////////////////////////////////////////////////////////

    public abstract Obj SurrToObjValue(int surr);
    protected abstract void Free(int index);

    //////////////////////////////////////////////////////////////////////////////

    public void AddRef(int index) {
      int refs = references[index] + 1;
      if (refs == 256) {
        extraRefs.Increment(index);
        refs -= 64;
      }
      references[index] = (byte) refs;
    }

    public void Release(int index) {
      int refs = references[index] - 1;
      Debug.Assert(refs >= 0);
      if (refs == 127) {
        if (extraRefs.TryDecrement(index))
          refs += 64;
      }
      else if (refs == 0) {
        Free(index);
      }
      references[index] = (byte) refs;
    }

    public void Release(int index, int amount) {
      int refs = references[index];
      Debug.Assert(refs > 0);

      if (refs < 128) {
        refs -= amount;
      }
      else {
        refs -= amount;
        while (refs < 128 && extraRefs.TryDecrement(index))
          refs += 64;
      }

      Debug.Assert(refs >= 0 & refs <= 255);

      references[index] = (byte) refs;

      if (refs == 0)
        Free(index);
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool TryRelease(int index) {
      return TryRelease(index, 1);
    }

    public bool TryRelease(int index, int amount) {
      int refs = references[index];
      Debug.Assert(refs >= 0);

      if (refs < 128) {
        if (amount >= refs)
          return false;
        refs -= amount;
        references[index] = (byte) refs;
        return true;
      }
      else if (refs + 64 * extraRefs.Get(index) > amount) {
        Release(index, amount);
        return true;
      }
      else
        return false;
    }

    //////////////////////////////////////////////////////////////////////////////

    protected int RefCount(int index) {
      return references[index] + 64 * extraRefs.Get(index);
    }

    protected void ResizeRefsArray(int newCapacity) {
      byte[] currReferences = references;
      references = new byte[newCapacity];
      Array.Copy(currReferences, references, currReferences.Length);
    }
  }
}
