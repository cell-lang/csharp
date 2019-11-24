namespace Cell.Runtime {
  // Valid slot states:
  //   - Value + payload: 32 bit payload - 3 zeros   - 29 bit value
  //   - Index + count:   32 bit count   - 3 bit tag - 29 bit index
  //     This type of slot can only be stored in a hashed block or passed in and out
  //   - Empty:           32 zeros - ArraySliceAllocator.EMPTY_MARKER == 0xFFFFFFFF
  //     This type of slot can only be stored in a block, but cannot be passed in or out

  class OneWayBinTable {
    private const int MIN_CAPACITY = 16;

    private const int INLINE_SLOT = OverflowTable.INLINE_SLOT;
    private const int EMPTY_MARKER = OverflowTable.EMPTY_MARKER;
    private const long EMPTY_SLOT = OverflowTable.EMPTY_SLOT;

    public long[] column = Array.emptyLongArray;
    public OverflowTable overflowTable = new OverflowTable();
    public int count = 0;

    //////////////////////////////////////////////////////////////////////////////

    private static int Low(long slot) {
      return OverflowTable.Low(slot);
    }

    private static int High(long slot) {
      return OverflowTable.High(slot);
    }

    private static int Tag(int word) {
      return OverflowTable.Tag(word);
    }

    private static bool IsEmpty(long slot) {
      return slot == EMPTY_SLOT;
    }

    private static bool IsIndex(long slot) {
      return slot != EMPTY_SLOT && Tag(Low(slot)) != OverflowTable.INLINE_SLOT;
    }

    private static int Count(long slot) {
      return OverflowTable.Count(slot);
    }

    // private static int Value(long slot) {
    //   return OverflowTable.Value(slot);
    // }

    private static long Slot(int low, int high) {
      return OverflowTable.Combine(low, high);
    }

    //////////////////////////////////////////////////////////////////////////////

    private void Set(int index, long value) {
      column[index] = value;
    }

    private void Set(int index, int low, int high) {
      Set(index, Slot(low, high));
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool Contains(int surr1, int surr2) {
      if (surr1 >= column.Length)
        return false;

      long slot = column[surr1];

      if (IsEmpty(slot))
        return false;

      if (IsIndex(slot))
        return overflowTable.Contains(slot, surr2);

      if (Low(slot) == surr2)
        return true;

      return High(slot) == surr2;
    }

    public bool ContainsKey(int surr1) {
      return surr1 < column.Length && !IsEmpty(column[surr1]);
    }

    public int[] Restrict(int surr) {
      if (surr >= column.Length)
        return Array.emptyIntArray;

      long slot = column[surr];

      if (IsEmpty(slot))
        return Array.emptyIntArray;

      if (IsIndex(slot)) {
        int count = Count(slot);
        int[] surrs = new int[count];
        overflowTable.Copy(slot, surrs);
        return surrs;
      }

      int low = Low(slot);
      int high = High(slot);
      return high == EMPTY_MARKER ? new int[] {low} : new int[] {low, high};
    }

    public int Restrict(int surr, int[] output) {
      if (surr >= column.Length)
        return 0;

      long slot = column[surr];

      if (IsEmpty(slot))
        return 0;

      if (IsIndex(slot)) {
        overflowTable.Copy(slot, output);
        return Count(slot);
      }

      output[0] = Low(slot);
      int high = High(slot);
      if (high == EMPTY_MARKER)
        return 1;
      output[1] = high;
      return 2;
    }

    public int Lookup(int surr) {
      if (surr >= column.Length)
        return -1;
      long slot = column[surr];
      if (IsEmpty(slot))
        return -1;
      if (IsIndex(slot) | High(slot) != EMPTY_MARKER)
        throw ErrorHandler.InternalFail();
      // Debug.Assert(Tag(Low(slot)) == INLINE_SLOT);
      return Low(slot);
    }

    public int Count(int surr) {
      if (surr >= column.Length)
        return 0;
      long slot = column[surr];
      if (IsEmpty(slot))
        return 0;
      if (IsIndex(slot))
        return Count(slot);
      return High(slot) == EMPTY_MARKER ? 1 : 2;
    }

    public bool Insert(int surr1, int surr2) {
      int size = column.Length;
      if (surr1 >= size)
        Resize(surr1);

      long slot = column[surr1];

      if (IsEmpty(slot)) {
        Set(surr1, surr2, EMPTY_MARKER);
        count++;
        return true;
      }

      int low = Low(slot);
      int high = High(slot);

      if (Tag(low) == INLINE_SLOT & high == EMPTY_MARKER) {
        if (surr2 == low)
          return false;
        Set(surr1, low, surr2);
        count++;
        return true;
      }

      long updatedSlot = overflowTable.Insert(slot, surr2);
      if (updatedSlot == slot)
        return false;

      Set(surr1, updatedSlot);
      count++;
      return true;
    }

    public void InsertUnique(int surr1, int surr2) {
      int size = column.Length;
      if (surr1 >= size)
        Resize(surr1);

      long slot = column[surr1];

      if (IsEmpty(slot)) {
        Set(surr1, surr2, EMPTY_MARKER);
        count++;
        return;
      }

      int low = Low(slot);
      int high = High(slot);

      if (Tag(low) == INLINE_SLOT & high == EMPTY_MARKER) {
        // Debug.Assert(surr2 != low);
        Set(surr1, low, surr2);
        count++;
        return;
      }

      long updatedSlot = overflowTable.InsertUnique(slot, surr2);
      // Debug.Assert(updatedSlot != slot);

      Set(surr1, updatedSlot);
      count++;
    }

    // Assuming there's at most one entry whose first argument is surr1
    public int Update(int surr1, int surr2) {
      if (surr1 >= column.Length)
        Resize(surr1);

      long slot = column[surr1];

      if (IsEmpty(slot)) {
        Set(surr1, surr2, EMPTY_MARKER);
        count++;
        return -1;
      }

      int low = Low(slot);
      int high = High(slot);

      if (Tag(low) == INLINE_SLOT & high == EMPTY_MARKER) {
        Set(surr1, surr2, EMPTY_MARKER);
        return low;
      }

      throw ErrorHandler.InternalFail();
    }

    public bool Delete(int surr1, int surr2) {
      if (surr1 >= column.Length)
        return false;

      long slot = column[surr1];

      if (IsEmpty(slot))
        return false;

      if (IsIndex(slot)) {
        long updatedSlot = overflowTable.Delete(slot, surr2);
        if (updatedSlot == slot)
          return false;

        Set(surr1, updatedSlot);
        count--;
        return true;
      }

      // Debug.Assert(Tag(Low(slot)) == INLINE_SLOT);

      int low = Low(slot);
      int high = High(slot);

      if (surr2 == low) {
        if (high == EMPTY_MARKER)
          Set(surr1, EMPTY_SLOT);
        else
          Set(surr1, high, EMPTY_MARKER);
        count--;
        return true;
      }

      if (surr2 == high) {
        Set(surr1, low, EMPTY_MARKER);
        count--;
        return true;
      }

      return false;
    }

    public void DeleteByKey(int surr1, int[] surrs2) {
      if (surr1 >= column.Length)
        return;

      long slot = column[surr1];

      if (IsEmpty(slot))
        return;

      Set(surr1, EMPTY_SLOT);

      if (IsIndex(slot)) {
        int slotCount = Count(slot);
        overflowTable.Copy(slot, surrs2);
        overflowTable.Delete(slot);
        count -= slotCount;
      }
      else {
        // Debug.Assert(Tag(Low(slot)) == INLINE_SLOT);
        surrs2[0] = Low(slot);
        int high = High(slot);
        if (high != EMPTY_MARKER) {
          surrs2[1] = high;
          count -= 2;
        }
        else
          count--;
      }
    }

    public bool IsMap() {
      for (int i=0 ; i < column.Length ; i++) {
        long slot = column[i];
        if (!IsEmpty(slot) & (Tag(Low(slot)) != INLINE_SLOT | High(slot) != EMPTY_MARKER))
          return false;
      }
      return true;
    }

    public int[] Copy() {
      int[] data = new int[2 * count];
      int next = 0;
      for (int i=0 ; i < column.Length ; i++) {
        long slot = column[i];
        if (!IsEmpty(slot)) {
          if (IsIndex(slot)) {
            int slotCount = Count(slot);
            for (int j=0 ; j < slotCount ; j++)
              data[next+2*j] = i;
            overflowTable.Copy(slot, data, next + 1, 2);
            next += 2 * slotCount;
          }
          else {
            data[next++] = i;
            data[next++] = Low(slot);
            int high = High(slot);
            if (high != EMPTY_MARKER) {
              data[next++] = i;
              data[next++] = high;
            }
          }
        }
      }
      // Debug.Assert(next == 2 * count);
      return data;
    }

    public int[] CopySym(int eqCount) {
      int[] data = new int[count+eqCount];

      int[] buffer = new int[32];

      int next = 0;
      for (int surr1 = 0 ; surr1 < column.Length ; surr1++) {
        long slot = column[surr1];
        if (!IsEmpty(slot)) {
          if (IsIndex(slot)) {
            int slotCount = Count(slot);
            if (slotCount > buffer.Length)
              buffer = new int[Array.Capacity(buffer.Length, slotCount)];
            int _count = Restrict(surr1, buffer);
            // Debug.Assert(_count == slotCount);
            for (int i=0 ; i < slotCount ; i++) {
              int surr2 = buffer[i];
              if (surr1 <= surr2) {
                data[next++] = surr1;
                data[next++] = surr2;
              }
            }
          }
          else {
            int low = Low(slot);
            int high = High(slot);
            if (surr1 <= low) {
              data[next++] = surr1;
              data[next++] = Low(slot);
            }
            if (high != EMPTY_MARKER & surr1 <= high) {
              data[next++] = surr1;
              data[next++] = high;
            }
          }
        }
      }
      // Debug.Assert(next == count + eqCount);
      return data;
    }

    //////////////////////////////////////////////////////////////////////////////

    public void InitReverse(OneWayBinTable source) {
      // Debug.Assert(count == 0);

      int len = source.column.Length;
      for (int i=0 ; i < len ; i++) {
        int[] surrs = source.Restrict(i);
        for (int j=0 ; j < surrs.Length ; j++)
          Insert(surrs[j], i);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private void Resize(int index) {
      int size = column.Length;
      int newSize = size == 0 ? MIN_CAPACITY : 2 * size;
      while (index >= newSize)
        newSize *= 2;
      long[] newColumn = new long[newSize];
      Array.Copy(column, newColumn, size);
      Array.Fill(newColumn, size, newSize - size, EMPTY_SLOT);
      column = newColumn;
    }

    //////////////////////////////////////////////////////////////////////////////


    // public void Check() {
    //   overflowTable.Check(column, count);
    // }

    // public void Dump() {
    //   System.out.Println("count = " + Integer.ToString(count));
    //   System.out.Print("column = [");
    //   for (int i=0 ; i < column.Length ; i++)
    //     System.out.Printf("%s%X", i > 0 ? " " : "", column[i]);
    //   System.out.Println("]");
    //   overflowTable.Dump();
    // }
  }
}
