namespace Cell.Runtime {
  public class ObjStoreUpdater : ValueStoreUpdater {
    const int INIT_SIZE = 32;

    private Obj[] values     = new Obj[INIT_SIZE];
    private int[] hashcodes  = new int[INIT_SIZE];
    private int[] surrogates = new int[INIT_SIZE];

    private int[] hashtable  = new int[INIT_SIZE];
    private int[] buckets    = new int[INIT_SIZE];

    private int count = 0;
    private int hashRange = 0;
    private int lastSurrogate = -1;

    private ObjStore store;

    //////////////////////////////////////////////////////////////////////////////

    public ObjStoreUpdater(ObjStore store) : base(store) {
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
        store.Insert(values[i], hashcodes[i], surrogates[i]);
    }

    public void Reset() {
      if (hashRange != 0)
        Array.Fill(hashtable, hashRange, -1);

      count = 0;
      hashRange = 0;
      lastSurrogate = -1;
    }

    public int LookupOrInsertValue(Obj value) {
      int surr = ValueToSurr(value);
      if (surr != -1)
        return surr;
      return Insert(value);
    }

    // Inefficient, but used only for debugging
    public override Obj SurrToValue(int surr) {
      for (int i=0 ; i < count ; i++)
        if (surrogates[i] == surr)
          return values[i];
      return store.SurrToValue(surr);
    }

    //////////////////////////////////////////////////////////////////////////////

    private int Insert(Obj value) {
      Debug.Assert(count <= values.Length);

      lastSurrogate = store.NextFreeIdx(lastSurrogate);
      int hashcode = value.SignedHashcode();

      if (count == values.Length)
        Resize();

      values[count]     = value;
      hashcodes[count]  = hashcode;
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
            InsertIntoHashtable(i, hashcodes[i]);
        }
        InsertIntoHashtable(count, hashcode);
      }
      count++;

      return lastSurrogate;
    }

    private int ValueToSurr(Obj value) {
      int surrogate = store.ValueToSurr(value);
      if (surrogate != -1)
        return surrogate;

      if (count > 0) {
        int hashcode = value.SignedHashcode(); //## BAD BAD BAD: CALCULATING THE HASHCODE TWICE

        if (hashRange == 0) {
          for (int i=0 ; i < count ; i++)
            if (hashcodes[i] == hashcode && values[i].IsEq(value))
              return surrogates[i];
        }
        else {
          int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashRange);
          for (int i = hashtable[hashIdx] ; i != -1 ; i = buckets[i])
            if (hashcodes[i] == hashcode && values[i].IsEq(value))
              return surrogates[i];
        }
      }

      return -1;
    }

    private void Resize() {
      Debug.Assert(hashRange == values.Length);

      int currCapacity = values.Length;
      int newCapacity = 2 * currCapacity;

      Obj[] currValues     = values;
      int[] currHashcodes  = hashcodes;
      int[] currSurrogates = surrogates;

      values     = new Obj[newCapacity];
      hashcodes  = new int[newCapacity];
      hashtable  = new int[newCapacity];
      buckets    = new int[newCapacity];
      surrogates = new int[newCapacity];
      hashRange  = newCapacity;

      Array.Copy(currValues, values, currCapacity);
      Array.Copy(currHashcodes, hashcodes, currCapacity);
      Array.Copy(currSurrogates, surrogates, currCapacity);
      Array.Fill(hashtable, -1);

      for (int i=0 ; i < count ; i++)
        InsertIntoHashtable(i, hashcodes[i]);
    }

    private void InsertIntoHashtable(int index, int hashcode) {
      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashRange);
      buckets[index] = hashtable[hashIdx];
      hashtable[hashIdx] = index;
    }
  }
}
