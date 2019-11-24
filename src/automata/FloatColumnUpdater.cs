namespace Cell.Runtime {
  public sealed class FloatColumnUpdater {
    bool clear = false;
    int deleteCount = 0;
    int[] deleteIdxs = Array.emptyIntArray;

    internal int insertCount = 0;
    internal int[] insertIdxs = Array.emptyIntArray;
    internal double[] insertValues = Array.emptyDoubleArray;

    internal int updateCount = 0;
    internal int[] updateIdxs = Array.emptyIntArray;
    internal double[] updateValues = Array.emptyDoubleArray;

    // int minIdx = Integer.MAX_VALUE; //## IMPLEMENT THIS
    int maxIdx = -1;
    bool dirty = false;
    long[] bitmap = Array.emptyLongArray;

    internal string relvarName;
    internal FloatColumn column;
    internal ValueStoreUpdater store;

    //////////////////////////////////////////////////////////////////////////////

    public FloatColumnUpdater(string relvarName, FloatColumn column, ValueStoreUpdater store) {
      this.relvarName = relvarName;
      this.column = column;
      this.store = store;
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Clear() {
      clear = true;
      deleteCount = 0;
    }

    public void Delete1(int index) {
      Delete(index);
    }

    public void Delete(int index) {
      if (!clear) {
        if (deleteCount < deleteIdxs.Length)
          deleteIdxs[deleteCount++] = index;
        else
          deleteIdxs = Array.Append(deleteIdxs, deleteCount++, index);
        if (index > maxIdx)
          maxIdx = index;
      }
    }

    public void Insert(int index, double value) {
      if (insertCount < insertIdxs.Length) {
        insertIdxs[insertCount] = index;
        insertValues[insertCount++] = value;
      }
      else {
        insertIdxs = Array.Append(insertIdxs, insertCount, index);
        insertValues = Array.Append(insertValues, insertCount++, value);
      }
      if (index > maxIdx)
        maxIdx = index;
    }

    public void Update(int index, double value) {
      if (updateCount < updateIdxs.Length) {
        updateIdxs[updateCount] = index;
        updateValues[updateCount++] = value;
      }
      else {
        updateIdxs = Array.Append(updateIdxs, updateCount, index);
        updateValues = Array.Append(updateValues, updateCount++, value);
      }
      if (index > maxIdx)
        maxIdx = index;
    }

    public void Apply() {
      if (clear) {
        column.Clear();
      }
      else {
        for (int i=0 ; i < deleteCount ; i++) {
          int index = deleteIdxs[i];
          column.Delete(index);
        }
      }

      for (int i=0 ; i < updateCount ; i++) {
        int index = updateIdxs[i];
        double value = updateValues[i];
        column.Update(index, value);
      }

      for (int i=0 ; i < insertCount ; i++) {
        int index = insertIdxs[i];
        double value = insertValues[i];
        column.Insert(index, value);
      }
    }

    public void Finish() {

    }

    //////////////////////////////////////////////////////////////////////////////

    public void Reset() {
      maxIdx = -1;

      if (dirty) {
        dirty = false;

        int count = deleteCount + insertCount + updateCount;

        if (!clear && 3 * count < bitmap.Length) {
          for (int i=0 ; i < deleteCount ; i++)
            bitmap[deleteIdxs[i] / 32] = 0;

          for (int i=0 ; i < updateCount ; i++)
            bitmap[updateIdxs[i] / 32] = 0;

          for (int i=0 ; i < insertCount ; i++)
            bitmap[insertIdxs[i] / 32] = 0;
        }
        else
          Array.Fill(bitmap, 0);
      }

      clear = false;

      deleteCount = 0;
      insertCount = 0;
      updateCount = 0;

      if (deleteIdxs.Length > 2048)
        deleteIdxs = Array.emptyIntArray;

      if (insertIdxs.Length > 2048) {
        insertIdxs = Array.emptyIntArray;
        insertValues = Array.emptyDoubleArray;
      }

      if (updateIdxs.Length > 2048) {
        updateIdxs = Array.emptyIntArray;
        updateValues = Array.emptyDoubleArray;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    internal bool Contains1(int surr1) {
      if (surr1 > maxIdx)
        return !clear && column.Contains1(surr1);

      // This call is only needed to build the delete/update/insert bitmap
      if (!dirty)
        BuildBitmapAndCheckKey();

      int slotIdx = surr1 / 32;
      int bitsShift = 2 * (surr1 % 32);
      long slot = bitmap[slotIdx];
      long status = slot >> bitsShift;

      if ((status & 2) != 0)
        return true;  // Inserted/updated
      else if ((status & 1) != 0)
        return false; // Deleted and not reinserted
      else
        return column.Contains1(surr1); // Untouched
    }

    //////////////////////////////////////////////////////////////////////////////

    internal double Lookup(int surr1) {
      if (surr1 <= maxIdx & (insertCount != 0 | updateCount != 0)) {
        Debug.Assert(dirty);

        int slotIdx = surr1 / 32;
        int bitsShift = 2 * (surr1 % 32);
        long slot = bitmap[slotIdx];
        long status = slot >> bitsShift;

        if ((status & 2) != 0) {
          for (int i=0 ; i < insertCount ; i++)
            if (insertIdxs[i] == surr1)
              return insertValues[i];

          for (int i=0 ; i < updateCount ; i++)
            if (updateIdxs[i] == surr1)
              return updateValues[i];

          ErrorHandler.InternalFail();
        }
      }

      return column.Lookup(surr1);
    }

    //////////////////////////////////////////////////////////////////////////////

    public void CheckKey_1() {
      if (insertCount != 0 | updateCount != 0) {
        Debug.Assert(maxIdx != -1);
        Debug.Assert(!dirty);

        BuildBitmapAndCheckKey();
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private void BuildBitmapAndCheckKey() {
      dirty = true;

      if (maxIdx / 32 >= bitmap.Length)
        bitmap = Array.Extend(bitmap, Array.Capacity(bitmap.Length, maxIdx / 32 + 1));

      // 00 - untouched
      // 01 - deleted
      // 10 - inserted
      // 11 - updated or inserted and deleted

      if (clear) {
        Array.Fill(bitmap, 0x5555555555555555L); //## BAD: THIS CAN BE MADE MORE EFFICIENT
      }
      else {
        for (int i=0 ; i < deleteCount ; i++) {
          int idx = deleteIdxs[i];
          int slotIdx = idx / 32;
          int bitsShift = 2 * (idx % 32);
          bitmap[slotIdx] |= 1L << bitsShift;
        }
      }

      for (int i=0 ; i < updateCount ; i++) {
        int idx = updateIdxs[i];
        int slotIdx = idx / 32;
        int bitsShift = 2 * (idx % 32);
        long slot = bitmap[slotIdx];
        if (((slot >> bitsShift) & 2) != 0)
          //## HERE I WOULD ACTUALLY NEED TO CHECK THAT THE NEW VALUE IS DIFFERENT FROM THE OLD ONE
          throw Col1KeyViolation(idx, updateValues[i], true);
        bitmap[slotIdx] = slot | (3L << bitsShift);
      }

      for (int i=0 ; i < insertCount ; i++) {
        int idx = insertIdxs[i];
        int slotIdx = idx / 32;
        int bitsShift = 2 * (idx % 32);
        long slot = bitmap[slotIdx];
        int bits = (int) ((slot >> bitsShift) & 3);
        if (bits >= 2)
          //## HERE I WOULD ACTUALLY NEED TO CHECK THAT THE NEW VALUE IS DIFFERENT FROM THE OLD ONE
          throw Col1KeyViolation(idx, insertValues[i], true);
        if ((bits == 0 && column.Contains1(idx)))
          //## HERE I WOULD ACTUALLY NEED TO CHECK THAT THE NEW VALUE IS DIFFERENT FROM THE OLD ONE
          throw Col1KeyViolation(idx, insertValues[i], false);
        bitmap[slotIdx] = slot | (2L << bitsShift);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private KeyViolationException Col1KeyViolation(int idx, double value, bool betweenNew) {
      if (betweenNew) {
        for (int i=0 ; i < updateCount ; i++)
          if (updateIdxs[i] == idx)
            return Col1KeyViolation(idx, value, updateValues[i], betweenNew);

        for (int i=0 ; i < insertCount ; i++)
          if (insertIdxs[i] == idx)
            return Col1KeyViolation(idx, value, insertValues[i], betweenNew);

        throw ErrorHandler.InternalFail();
      }
      else
        return Col1KeyViolation(idx, value, column.Lookup(idx), betweenNew);
    }

    private KeyViolationException Col1KeyViolation(int idx, double value, double otherValue, bool betweenNew) {
      //## BUG: Stores may contain only part of the value (id(5) -> 5)
      Obj key = store.SurrToValue(idx);
      Obj[] tuple1 = new Obj[] {key, new FloatObj(value)};
      Obj[] tuple2 = new Obj[] {key, new FloatObj(otherValue)};
      return new KeyViolationException(relvarName, KeyViolationException.key_1, tuple1, tuple2, betweenNew);
    }
  }
}
