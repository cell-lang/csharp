namespace Cell.Runtime {
  public sealed class ObjStore : ValueStore {
    const int INIT_SIZE = 256;
                                                           // VALUE     NO VALUE
    private Obj[] values             = new Obj[INIT_SIZE]; //           null
    private int[] hashcodeOrNextFree = new int[INIT_SIZE]; // hashcode  index of the next free slot (can be out of bound)

    private int[] hashtable = new int[INIT_SIZE/2]; // -1 when there's no value in that bucket
    private int[] buckets   = new int[INIT_SIZE]; // junk when there's no value

    private int count = 0;
    private int firstFree = 0;

    //////////////////////////////////////////////////////////////////////////////

    public ObjStore() : base(INIT_SIZE) {
      Array.Fill(hashtable, -1);
      for (int i=0 ; i < INIT_SIZE ; i++)
        hashcodeOrNextFree[i] = i + 1;
    }

    //////////////////////////////////////////////////////////////////////////////

    public void PrintInfo(string name) {
      // int usedBuckets = 0;
      // for (int i=0 ; i < values.Length ; i++)
      //   if (values[i] != null)
      //     if (buckets[i] != -1)
      //       usedBuckets++;
      // System.out.Printf(
      //   "%-14s: %6d / %7d (%f) - %f\n",
      //   name + ":",
      //   usedBuckets,
      //   count,
      //   (double) usedBuckets / (double) count,
      //   (double) count / (double) values.Length
      // );
    }

    public void Insert(Obj value, int hashcode, int index) {
      Debug.Assert(firstFree == index);
      // Debug.Assert(nextFreeIdx[index] != -1);
      Debug.Assert(index < values.Length);
      Debug.Assert(values[index] == null);
      Debug.Assert(hashcode == value.SignedHashcode());

      count++;
      firstFree = hashcodeOrNextFree[index];
      values[index] = value;
      hashcodeOrNextFree[index] = hashcode;

      InsertIntoHashtable(index, hashcode);
    }

    public int InsertOrAddRef(Obj value) {
      int surr = ValueToSurr(value);
      if (surr != -1) {
        AddRef(surr);
        return surr;
      }
      else {
        int capacity = Capacity();
        Debug.Assert(count <= capacity);
        if (count == capacity)
          Resize(count+1);
        int idx = firstFree;
        Insert(value, value.SignedHashcode(), firstFree);
        AddRef(idx);
        return idx;
      }
    }

    public void Resize(int minCapacity) {
      int currCapacity = values.Length;
      int newCapacity = 2 * currCapacity;
      while (newCapacity < minCapacity)
        newCapacity = 2 * newCapacity;

      base.ResizeRefsArray(newCapacity);

      Obj[]  currValues             = values;
      int[]  currHashcodeOrNextFree = hashcodeOrNextFree;

      values             = new Obj[newCapacity];
      hashcodeOrNextFree = new int[newCapacity];
      hashtable          = new int[newCapacity/2];
      buckets            = new int[newCapacity];

      Array.Copy(currValues, values, currCapacity);
      Array.Copy(currHashcodeOrNextFree, hashcodeOrNextFree, currCapacity);
      Array.Fill(hashtable, -1);

      for (int i=0 ; i < currCapacity ; i++)
        if (values[i] != null)
          InsertIntoHashtable(i, hashcodeOrNextFree[i]);

      for (int i=currCapacity ; i < newCapacity ; i++)
        hashcodeOrNextFree[i] = i + 1;
    }

    //////////////////////////////////////////////////////////////////////////////

    public int Count() {
      return count;
    }

    public int Capacity() {
      return values.Length;
    }

    public int NextFreeIdx(int index) {
      Debug.Assert(index == -1 || index >= values.Length || values[index] == null);
      if (index == -1)
        return firstFree;
      if (index >= values.Length)
        return index + 1;
      return hashcodeOrNextFree[index];
    }

    public int ValueToSurr(Obj value) {
      if (count == 0)
        return -1;
      int hashcode = value.SignedHashcode();
      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashtable.Length);
      int idx = hashtable[hashIdx];
      while (idx != -1) {
        Debug.Assert(values[idx] != null);
        if (hashcodeOrNextFree[idx] == hashcode && value.IsEq(values[idx]))
          return idx;
        idx = buckets[idx];
      }
      return -1;
    }

    public Obj SurrToValue(int index) {
      return values[index];
    }

    //////////////////////////////////////////////////////////////////////////////

    protected override void Free(int index) {
      Debug.Assert(values[index] != null);

      RemoveFromHashtable(index);
      values[index] = null;
      hashcodeOrNextFree[index] = firstFree;
      count--;
      firstFree = index;
    }

    //## THIS IS REDUNDANT
    public override Obj SurrToObjValue(int index) {
      return values[index];
    }

    //////////////////////////////////////////////////////////////////////////////

    private void InsertIntoHashtable(int index, int hashcode) {
      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashtable.Length);
      buckets[index] = hashtable[hashIdx];
      hashtable[hashIdx] = index;
    }

    private void RemoveFromHashtable(int index) {
      int hashcode = hashcodeOrNextFree[index];
      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashtable.Length);
      int idx = hashtable[hashIdx];
      Debug.Assert(idx != -1);

      if (idx == index) {
        hashtable[hashIdx] = buckets[index];
        // buckets[index] = -1; // NOT STRICTLY NECESSARY...
        return;
      }

      int prevIdx = idx;
      idx = buckets[idx];
      while (idx != index) {
        prevIdx = idx;
        idx = buckets[idx];
        Debug.Assert(idx != -1);
      }

      buckets[prevIdx] = buckets[index];
      // buckets[index] = -1; // NOT STRICTLY NECESSARY
    }
  }
}
