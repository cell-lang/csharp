namespace Cell.Runtime {
  public class IntStoreUpdater : ValueStoreUpdater {
    const int INIT_SIZE = 32;

    private long[] values     = new long[INIT_SIZE];
    private int[]  surrogates = new int[INIT_SIZE];

    private int[] hashtable  = new int[INIT_SIZE];
    private int[] buckets    = new int[INIT_SIZE];

    private int count = 0;
    private int hashRange = 0;
    private int lastSurrogate = -1;

    private IntStore store;

    //////////////////////////////////////////////////////////////////////////////

    public IntStoreUpdater(IntStore store) : base(store) {
      Array.Fill(hashtable, -1);
      this.store = store;
    }

    public void Apply() {
      if (count == 0)
        return;

      int storeCapacity = store.Capacity();
      int reqCapacity = store.Count() + count;

      if (storeCapacity < reqCapacity)
        store.Resize(reqCapacity);

      for (int i=0 ; i < count ; i++)
        store.Insert(values[i], surrogates[i]);
    }

    public void Reset() {
      if (hashRange != 0)
        Array.Fill(hashtable, hashRange, -1);

      count = 0;
      hashRange = 0;
      lastSurrogate = -1;
    }

    public int LookupOrInsertValue(long value) {
      int surr = ValueToSurr(value);
      if (surr != -1)
        return surr;
      return Insert(value);
    }

    // Inefficient, but used only for debugging
    public override Obj SurrToValue(int surr) {
      for (int i=0 ; i < count ; i++)
        if (surrogates[i] == surr)
          return IntObj.Get(values[i]);
      return IntObj.Get(store.SurrToValue(surr));
    }

    //////////////////////////////////////////////////////////////////////////////

    private int Hashcode(long value) {
      return (int) (value ^ (value >> 32));
    }

    private int Insert(long value) {
      Debug.Assert(count <= values.Length);

      lastSurrogate = store.NextFreeIdx(lastSurrogate);
      int hashcode = Hashcode(value);

      if (count == values.Length)
        Resize();

      values[count]     = value;
      surrogates[count] = lastSurrogate;

      if (count >= 16) {
        if (count >= hashRange) {
          if (hashRange != 0) {
            Array.Fill(hashtable, hashRange, -1);
            hashRange *= 2;
          }
          else
            hashRange = 16;

          for (int i=0 ; i < count ; i++)
            InsertIntoHashtable(i, Hashcode(values[i]));
        }
        InsertIntoHashtable(count, hashcode);
      }
      count++;

      return lastSurrogate;
    }

    private int ValueToSurr(long value) {
      int surrogate = store.ValueToSurr(value);
      if (surrogate != -1)
        return surrogate;

      if (count > 0) {
        if (hashRange == 0) {
          for (int i=0 ; i < count ; i++)
            if (values[i] == value)
              return surrogates[i];
        }
        else {
          int hashIdx = Miscellanea.UnsignedRemaider(Hashcode(value), hashRange);
          for (int i = hashtable[hashIdx] ; i != -1 ; i = buckets[i])
            if (values[i] == value)
              return surrogates[i];
        }
      }

      return -1;
    }

    private void Resize() {
      Debug.Assert(hashRange == values.Length);

      int currCapacity = values.Length;
      int newCapacity = 2 * currCapacity;

      long[] currValues     = values;
      int[]  currSurrogates = surrogates;

      values     = new long[newCapacity];
      hashtable  = new int[newCapacity];
      buckets    = new int[newCapacity];
      surrogates = new int[newCapacity];
      hashRange  = newCapacity;

      Array.Copy(currValues, values, currCapacity);
      Array.Copy(currSurrogates, surrogates, currCapacity);
      Array.Fill(hashtable, -1);

      for (int i=0 ; i < count ; i++)
        InsertIntoHashtable(i, Hashcode(values[i]));
    }

    private void InsertIntoHashtable(int index, int hashcode) {
      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashRange);
      buckets[index] = hashtable[hashIdx];
      hashtable[hashIdx] = index;
    }
  }
}
