using Int32 = System.Int32;


namespace Cell.Runtime {
  public sealed class IntStore : ValueStore {
    private const int INIT_SIZE = 256;
    private const int INV_IDX = 0x3FFFFFFF;

    // Bits  0 - 31: 32-bit value, or index of 64-bit value
    // Bits 32 - 61: index of next value in the bucket if used or next free index otherwise
    // Bits 62 - 63: tag: 00 used (32 bit), 01 used (64 bit), 10 free
    private long[] slots = new long[INIT_SIZE];

    // INV_IDX when there's no value in that bucket
    private int[] hashtable = new int[INIT_SIZE / 4];

    private int count = 0;
    private int firstFree = 0;

    private LargeIntStore largeInts = new LargeIntStore();

    //////////////////////////////////////////////////////////////////////////////

    private int HashIdx(long value) {
      int hashcode = (int) (value ^ (value >> 32));
      return Miscellanea.UnsignedRemaider(hashcode, hashtable.Length);
    }

    private long EmptySlot(int next) {
      Debug.Assert(next >= 0 & next <= 0x1FFFFFFF);
      return (((long) next) | (2L << 30)) << 32;
    }

    private long FilledValueSlot(int value, int next) {
      long slot = (((long) value) & 0xFFFFFFFFL) | (((long) next) << 32);
      Debug.Assert(!IsEmpty(slot));
      Debug.Assert(Value(slot) == value);
      Debug.Assert(Next(slot) == next);
      return slot;
    }

    private long FilledIdxSlot(int idx, int next) {
      Debug.Assert(idx >= 0);
      long slot = ((long) idx) | (((long) next) << 32) | (1L << 62);
      Debug.Assert(!IsEmpty(slot));
      Debug.Assert(Value(slot) == largeInts.Get(idx));
      Debug.Assert(Next(slot) == next);
      return slot;
    }

    private long ReindexedSlot(long slot, int next) {
      int tag = (int) Miscellanea.UnsignedLeftShift64(slot, 62);
      Debug.Assert(tag == 0 | tag == 1);
      return tag == 0 ? FilledValueSlot((int) slot, next) : FilledIdxSlot((int) slot, next);
    }

    private long Value(long slot) {
      Debug.Assert(!IsEmpty(slot));
      int tag = (int) Miscellanea.UnsignedLeftShift64(slot, 62);
      Debug.Assert(tag == 0 | tag == 1);
      return tag == 0 ? (int) slot : largeInts.Get((int) slot);
    }

    private int Next(long slot) {
      Debug.Assert(!IsEmpty(slot));
      return (int) (Miscellanea.UnsignedLeftShift64(slot, 32) & 0x3FFFFFFF);
    }

    private int NextFree(long slot) {
      Debug.Assert(IsEmpty(slot));
      return (int) ((slot >> 32) & 0x3FFFFFFF);
    }

    private bool IsEmpty(long slot) {
      long tag = Miscellanea.UnsignedLeftShift64(slot, 62);
      Debug.Assert(tag == 0 | tag == 1 | tag == 2);
      return tag == 2;
    }

    //////////////////////////////////////////////////////////////////////////////

    public IntStore() : base(INIT_SIZE) {
      for (int i=0 ; i < INIT_SIZE ; i++)
        slots[i] = EmptySlot(i+1);
      for (int i=0 ; i < INIT_SIZE ; i++)
        Debug.Assert(IsEmpty(slots[i]));
      Array.Fill(hashtable, INV_IDX);
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Insert(long value, int index) {
      Debug.Assert(firstFree == index);
      Debug.Assert(index < slots.Length);
      Debug.Assert(RefCount(index) == 0);

      count++;
      firstFree = NextFree(slots[index]);

      int hashIdx = HashIdx(value);
      int head = hashtable[hashIdx];
      if (value == (int) value) {
        slots[index] = FilledValueSlot((int) value, head);
        Debug.Assert(!IsEmpty(slots[index]));
      }
      else {
        int idx64 = largeInts.Insert(value);
        slots[index] = FilledIdxSlot(idx64, head);
        Debug.Assert(!IsEmpty(slots[index]));
      }
      hashtable[hashIdx] = index;
    }

    public int InsertOrAddRef(long value) {
      int surr = ValueToSurr(value);
      if (surr != -1) {
        AddRef(surr);
        return surr;
      }
      else {
        Debug.Assert(count <= Capacity());
        if (count == Capacity())
          Resize(count + 1);
        int idx = firstFree;
        Insert(value, idx);
        AddRef(idx);
        return idx;
      }
    }

    public void Resize(int minCapacity) {
      int currCapacity = Capacity();
      int newCapacity = 2 * currCapacity;
      while (newCapacity < minCapacity)
        newCapacity = 2 * newCapacity;

      base.ResizeRefsArray(newCapacity);

      long[] currSlots = slots;

      slots     = new long[newCapacity];
      hashtable = new int[newCapacity/2];

      Array.Fill(hashtable, INV_IDX);

      for (int i=0 ; i < currCapacity ; i++) {
        long slot = currSlots[i];
        int hashIdx = HashIdx(Value(slot));

        slots[i] = ReindexedSlot(slot, hashtable[hashIdx]);
        hashtable[hashIdx] = i;
      }

      for (int i=currCapacity ; i < newCapacity ; i++)
        slots[i] = EmptySlot(i+1);
    }

    //////////////////////////////////////////////////////////////////////////////

    public int Count() {
      return count;
    }

    public int Capacity() {
      return slots.Length;
    }

    public int NextFreeIdx(int index) {
      Debug.Assert(index == -1 || index >= Capacity() || IsEmpty(slots[index]));
      if (index == -1)
        return firstFree;
      if (index >= Capacity())
        return index + 1;
      return NextFree(slots[index]);
    }

    public int ValueToSurr(long value) {
      int hashIdx = HashIdx(value);
      int idx = hashtable[hashIdx];
      int firstIdx = idx;
      while (idx != INV_IDX) {
        long slot = slots[idx];
        if (Value(slot) == value)
          return idx;
        idx = Next(slot);
      }
      return -1;
    }

    public long SurrToValue(int surr) {
      return Value(slots[surr]);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj SurrToObjValue(int surr) {
      return IntObj.Get(SurrToValue(surr));
    }

    protected override void Free(int index) {
      long slot = slots[index];
      int hashIdx = HashIdx(Value(slot));

      int idx = hashtable[hashIdx];
      Debug.Assert(idx != INV_IDX);

      if (idx == index) {
        hashtable[hashIdx] = Next(slot);
      }
      else {
        for ( ; ; ) {
          slot = slots[idx];
          int next = Next(slot);
          if (next == index) {
            slots[idx] = ReindexedSlot(slot, Next(slots[next]));
            break;
          }
          idx = next;
        }
      }

      slots[index] = EmptySlot(firstFree);
      firstFree = index;
      count--;
    }
  }

  ////////////////////////////////////////////////////////////////////////////////

  class LargeIntStore {
    private long[] slots = new long[32];
    private int firstFree = 0;

    public LargeIntStore() {
      for (int i=0 ; i < slots.Length ; i++)
        slots[i] = i + 1;
    }

    public long Get(int idx) {
      long slot = slots[idx];
      Debug.Assert(slot < 0 | slot > Int32.MaxValue);
      return slot;
    }

    public int Insert(long value) {
      Debug.Assert(value < 0 | value > Int32.MaxValue);
      int len = slots.Length;
      if (firstFree >= len) {
        slots = Array.Extend(slots, 2 * len);
        for (int i=len ; i < 2 * len ; i++)
          slots[i] = i + 1;
      }
      int idx = firstFree;
      long nextFree = slots[idx];
      Debug.Assert(nextFree >= 0 & nextFree <= slots.Length);
      slots[idx] = value;
      firstFree = (int) nextFree;
      return idx;
    }

    public void Delete(int idx) {
      Debug.Assert(slots[idx] < 0 | slots[idx] > Int32.MaxValue);
      slots[idx] = firstFree;
      firstFree = idx;
    }
  }
}
