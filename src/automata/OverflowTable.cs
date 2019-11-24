namespace Cell.Runtime {
  // A slot can be in any of the following states:
  //   - Single value:        32 ones                  - 3 zeros   - 29 bit value 1
  //   - Two values:          3 zeros - 29 bit value 2 - 3 zeros   - 29 bit value 1
  //   - Index + count:       32 bit count             - 3 bit tag - 29 bit index
  //     This type of slot can only be stored in a hashed block or passed in and out
  //   - Empty:               32 zeros                 - 32 ones
  //     This type of slot can only be stored in a block, but cannot be passed in or out

  class OverflowTable : ArraySliceAllocator {
    public  const int INLINE_SLOT    = 0;
    private const int SIZE_2_BLOCK   = 1;
    private const int SIZE_4_BLOCK   = 2;
    private const int SIZE_8_BLOCK   = 3;
    private const int SIZE_16_BLOCK  = 4;
    private const int HASHED_BLOCK   = 5;

    private const int SIZE_2_BLOCK_MIN_COUNT   = 3;
    private const int SIZE_4_BLOCK_MIN_COUNT   = 4;
    private const int SIZE_8_BLOCK_MIN_COUNT   = 7;
    private const int SIZE_16_BLOCK_MIN_COUNT  = 13;
    private const int HASHED_BLOCK_MIN_COUNT   = 13;

    public const long EMPTY_SLOT = 0xFFFFFFFFL;

    //////////////////////////////////////////////////////////////////////////////

    public long Insert(long handle, int value) {
      int low = Low(handle);
      int tag = Tag(low);

      if (tag == 0)
        return Insert2Block(handle, value);

      if (tag == HASHED_BLOCK)
        return InsertIntoHashedBlock(Payload(low), Count(handle), value);

      return InsertWithLinearBlock(handle, value);
    }

    public long InsertUnique(long handle, int value) {
      int low = Low(handle);
      int tag = Tag(low);

      if (tag == 0)
        return Insert2Block(handle, value);

      if (tag == HASHED_BLOCK)
        return InsertUniqueIntoHashedBlock(Payload(low), Count(handle), value);

      return InsertUniqueWithLinearBlock(handle, value);
    }

    public long Delete(long handle, int value) {
      int low = Low(handle);
      int tag = Tag(low);

      Debug.Assert(tag != INLINE_SLOT);

      if (tag == HASHED_BLOCK)
        return DeleteFromHashedBlock(Payload(low), Count(handle), value);
      else
        return DeleteFromLinearBlock(handle, value);
    }

    public void Delete(long handle) {
      int low = Low(handle);
      int tag = Tag(low);
      int blockIdx = Payload(low);

      Debug.Assert(tag != INLINE_SLOT);

      if (tag == SIZE_2_BLOCK)
        Release2Block(blockIdx);
      else if (tag == SIZE_4_BLOCK)
        Release4Block(blockIdx);
      else if (tag == SIZE_8_BLOCK)
        Release8Block(blockIdx);
      else if (tag == SIZE_16_BLOCK)
        Release16Block(blockIdx);
      else {
        Debug.Assert(tag == HASHED_BLOCK);
        for (int i=0 ; i < 16 ; i++) {
          long slot = Slot(blockIdx + i);
          if (slot != EMPTY_SLOT && Tag(Low(slot)) != INLINE_SLOT)
            Delete(slot);
        }
      }
    }

    public bool Contains(long handle, int value) {
      int tag = Tag(Low(handle));
      int blockIdx = Payload(Low(handle));

      Debug.Assert(tag != INLINE_SLOT);
      Debug.Assert(Tag(tag, blockIdx) == Low(handle));

      if (tag != HASHED_BLOCK)
        return LinearBlockContains(blockIdx, Count(handle), value);
      else
        return HashedBlockContains(blockIdx, value);
    }

    public void Copy(long handle, int[] buffer) {
      Copy(handle, buffer, 0, 1);
    }

    public void Copy(long handle, int[] buffer, int offset, int step) {
      int low = Low(handle);
      int tag = Tag(low);
      int blockIdx = Payload(low);

      Debug.Assert(tag != INLINE_SLOT);
      Debug.Assert(Tag(tag, blockIdx) == Low(handle));

      if (tag != HASHED_BLOCK) {
        int count = Count(handle);
        int end = (count + 1) / 2;
        int targetIdx = offset;

        for (int i=0 ; i < end ; i++) {
          long slot = Slot(blockIdx + i);
          int slotLow = Low(slot);
          int slotHigh = High(slot);

          Debug.Assert(slotLow != EMPTY_MARKER & Tag(slotLow) == INLINE_SLOT);

          buffer[targetIdx] = slotLow;
          targetIdx += step;

          if (slotHigh != EMPTY_MARKER) {
            Debug.Assert(Tag(slotHigh) == INLINE_SLOT);

            buffer[targetIdx] = slotHigh;
            targetIdx += step;
          }
        }
      }
      else
        CopyHashedBlock(blockIdx, buffer, offset, step, 0, 0);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static int Count(long slot) {
      Debug.Assert(Tag(Low(slot)) >= SIZE_2_BLOCK & Tag(Low(slot)) <= HASHED_BLOCK);
      // Debug.Assert(High(slot) > 2); // Not true when initializing a hashed block
      return High(slot);
    }

    ////////////////////////////////////////////////////////////////////////////

    private static int Capacity(int tag) {
      Debug.Assert(tag >= SIZE_2_BLOCK & tag <= SIZE_16_BLOCK);
      Debug.Assert(SIZE_2_BLOCK == 1 | SIZE_16_BLOCK == 4);
      return 2 << tag;
    }

    private static long LinearBlockHandle(int tag, int index, int count) {
      return Combine(Tag(tag, index), count);
    }

    private static long Size2BlockHandle(int index, int count) {
      Debug.Assert(Tag(index) == 0);
      Debug.Assert(count >= SIZE_2_BLOCK_MIN_COUNT & count <= 4);
      return Combine(Tag(SIZE_2_BLOCK, index), count);
    }

    private static long Size4BlockHandle(int index, int count) {
      Debug.Assert(Tag(index) == 0);
      Debug.Assert(count >= SIZE_4_BLOCK_MIN_COUNT & count <= 8);
      return Combine(Tag(SIZE_4_BLOCK, index), count);
    }

    private static long Size8BlockHandle(int index, int count) {
      Debug.Assert(Tag(index) == 0);
      Debug.Assert(count >= SIZE_8_BLOCK_MIN_COUNT & count <= 16);
      return Combine(Tag(SIZE_8_BLOCK, index), count);
    }

    private static long Size16BlockHandle(int index, int count) {
      Debug.Assert(Tag(index) == 0);
      Debug.Assert(count >= SIZE_16_BLOCK_MIN_COUNT & count <= 32);
      return Combine(Tag(SIZE_16_BLOCK, index), count);
    }

    private static long HashedBlockHandle(int index, int count) {
      Debug.Assert(Tag(index) == 0);
      // Debug.Assert(count >= 7); // Not true when initializing a hashed block
      long handle = Combine(Tag(HASHED_BLOCK, index), count);
      Debug.Assert(Tag(Low(handle)) == HASHED_BLOCK);
      Debug.Assert(Payload(Low(handle)) == index);
      Debug.Assert(Count(handle) == count);
      return handle;
    }

    private static int Index(int value) {
      Debug.Assert(Tag(value) == INLINE_SLOT);
      return value & 0xF;
    }

    private static int Clipped(int value) {
      return Miscellanea.UnsignedLeftShift32(value, 4);
    }

    private static int Unclipped(int value, int index) {
      Debug.Assert(Tag(value) == 0);
      Debug.Assert(Tag(value << 4) == 0);
      Debug.Assert(index >= 0 & index < 16);
      return (value << 4) | index;
    }

    private int MinCount(int tag) {
      if (tag == SIZE_2_BLOCK)
        return SIZE_2_BLOCK_MIN_COUNT;

      if (tag == SIZE_4_BLOCK)
        return SIZE_4_BLOCK_MIN_COUNT;

      if (tag == SIZE_8_BLOCK)
        return SIZE_8_BLOCK_MIN_COUNT;

      Debug.Assert(tag == SIZE_16_BLOCK | tag == HASHED_BLOCK);
      Debug.Assert(SIZE_16_BLOCK_MIN_COUNT == HASHED_BLOCK_MIN_COUNT);

      return SIZE_16_BLOCK_MIN_COUNT; // Same as HASHED_BLOCK_MIN_COUNT
    }

    private static bool IsEven(int value) {
      return (value % 2) == 0;
    }

    ////////////////////////////////////////////////////////////////////////////

    private void MarkSlotAsEmpty(int index) {
      SetSlot(index, EMPTY_MARKER, 0);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    private bool LinearBlockContains(int blockIdx, int count, int value) {
      int end = (count + 1) / 2;
      for (int i=0 ; i < end ; i++) {
        long slot = Slot(blockIdx + i);
        if (value == Low(slot) | value == High(slot))
          return true;
      }
      return false;
    }

    private bool HashedBlockContains(int blockIdx, int value) {
      int slotIdx = blockIdx + Index(value);
      long slot = Slot(slotIdx);

      if (slot == EMPTY_SLOT)
        return false;

      int low = Low(slot);
      int high = High(slot);
      int tag = Tag(low);

      if (tag == 0)
        return value == low | value == high;

      if (tag == HASHED_BLOCK)
        return HashedBlockContains(Payload(low), Clipped(value));

      return Contains(slot, Clipped(value));
    }

    ////////////////////////////////////////////////////////////////////////////

    private int CopyHashedBlock(int blockIdx, int[] dest, int offset, int step, int shift, int leastBits) {
      int subshift = shift + 4;
      int targetIdx = offset;

      for (int i=0 ; i < 16 ; i++) {
        int slotLeastBits = (i << shift) + leastBits;
        long slot = Slot(blockIdx + i);
        int low = Low(slot);

        if (low != EMPTY_MARKER) {
          int tag = Tag(low);
          if (tag == INLINE_SLOT) {
            dest[targetIdx] = (Payload(low) << shift) + leastBits;
            targetIdx += step;

            int high = High(slot);
            if (high != EMPTY_MARKER) {
              dest[targetIdx] = (Payload(high) << shift) + leastBits;
              targetIdx += step;
            }
          }
          else if (tag == HASHED_BLOCK) {
            targetIdx = CopyHashedBlock(Payload(low), dest, targetIdx, step, subshift, slotLeastBits);
          }
          else {
            int subblockIdx = Payload(low);
            int count = Count(slot);
            int end = (count + 1) / 2;

            for (int j=0 ; j < end ; j++) {
              long subslot = Slot(subblockIdx + j);
              int sublow = Low(subslot);
              int subhigh = High(subslot);

              Debug.Assert(sublow != EMPTY_MARKER & Tag(sublow) == 0);

              dest[targetIdx] = (sublow << subshift) + slotLeastBits;
              targetIdx += step;

              if (subhigh != EMPTY_MARKER) {
                dest[targetIdx] = (subhigh << subshift) + slotLeastBits;
                targetIdx += step;
              }
            }
          }
        }
      }
      return targetIdx;
    }

    ////////////////////////////////////////////////////////////////////////////

    private long Insert2Block(long handle, int value) {
      int low = Low(handle);
      int high = High(handle);

      Debug.Assert(Tag(low) == 0 & Tag(high) == 0);

      // Checking for duplicates
      if (low == value | high == value)
        return handle;

      int blockIdx = Alloc2Block();
      SetSlot(blockIdx,     low,   high);
      SetSlot(blockIdx + 1, value, EMPTY_MARKER);
      return Size2BlockHandle(blockIdx, 3);
    }

    private long InsertWithLinearBlock(long handle, int value) {
      Debug.Assert(Tag(Low(handle)) >= SIZE_2_BLOCK & Tag(Low(handle)) <= SIZE_16_BLOCK);

      int low = Low(handle);
      int tag = Tag(low);
      int blockIdx = Payload(low);
      int count = Count(handle);
      int end = (count + 1) / 2;

      // Checking for duplicates and inserting if the next free block is a high one
      for (int i=0 ; i < end ; i++) {
        long slot = Slot(blockIdx + i);
        int slotLow = Low(slot);
        int slotHigh = High(slot);
        if (value == slotLow | value == slotHigh)
          return handle;
        if (slotHigh == EMPTY_MARKER) {
          SetSlot(blockIdx + i, slotLow, value);
          return LinearBlockHandle(tag, blockIdx, count + 1);
        }
      }

      int capacity = Capacity(tag);

      // Inserting the new value if there's still room here
      // It can only be in a low slot
      if (count < capacity) {
        SetSlot(blockIdx + end, value, EMPTY_MARKER);
        return LinearBlockHandle(tag, blockIdx, count + 1);
      }

      if (tag != SIZE_16_BLOCK) {
        // Allocating the new block
        int newBlockIdx = tag == SIZE_2_BLOCK ? Alloc4Block() : (tag == SIZE_4_BLOCK ? Alloc8Block() : Alloc16Block());

        // Initializing the new block
        int idx = count / 2;
        for (int i=0 ; i < idx ; i++)
          SetFullSlot(newBlockIdx + i, Slot(blockIdx + i));
        SetSlot(newBlockIdx + idx, value, EMPTY_MARKER);
        for (int i=idx+1 ; i < count ; i++)
          MarkSlotAsEmpty(newBlockIdx + i);

        // Releasing the old block
        if (tag == SIZE_2_BLOCK)
          Release2Block(blockIdx);
        else if (tag == SIZE_4_BLOCK)
          Release4Block(blockIdx);
        else
          Release8Block(blockIdx);

        return LinearBlockHandle(tag + 1, newBlockIdx, count + 1);
      }

      // Allocating and initializing the hashed block
      int hashedBlockIdx = Alloc16Block();
      for (int i=0 ; i < 16 ; i++)
        MarkSlotAsEmpty(hashedBlockIdx + i);

      // Transferring the existing values
      for (int i=0 ; i < 16 ; i++) {
        long slot = Slot(blockIdx + i);
        long tempHandle = InsertIntoHashedBlock(hashedBlockIdx, 2 * i, Low(slot));
        Debug.Assert(Count(tempHandle) == 2 * i + 1);
        Debug.Assert(Payload(Low(tempHandle)) == hashedBlockIdx);
        tempHandle = InsertIntoHashedBlock(hashedBlockIdx, 2 * i + 1, High(slot));
        Debug.Assert(Count(tempHandle) == 2 * (i + 1));
        Debug.Assert(Payload(Low(tempHandle)) == hashedBlockIdx);
      }

      // Releasing the old block
      Release16Block(blockIdx);

      // Adding the new value
      return InsertIntoHashedBlock(hashedBlockIdx, 32, value);
    }

    private long InsertIntoHashedBlock(int blockIdx, int count, int value) {
      int slotIdx = blockIdx + Index(value);
      long slot = Slot(slotIdx);
      int low = Low(slot);

      // Checking for empty slots
      if (low == EMPTY_MARKER) {
        SetSlot(slotIdx, value, EMPTY_MARKER);
        return HashedBlockHandle(blockIdx, count + 1);
      }

      int tag = Tag(low);

      // Checking for inline slots
      if (tag == INLINE_SLOT) {
        if (value == low)
          return HashedBlockHandle(blockIdx, count);
        int high = High(slot);
        if (high == EMPTY_MARKER) {
          SetSlot(slotIdx, low, value);
          return HashedBlockHandle(blockIdx, count + 1);
        }
        Debug.Assert(Tag(high) == INLINE_SLOT);
        if (value == high)
          return HashedBlockHandle(blockIdx, count);
        long handle = Insert2Block(Combine(Clipped(low), Clipped(high)), Clipped(value));
        Debug.Assert(Count(handle) == 3);
        SetFullSlot(slotIdx, handle);
        return HashedBlockHandle(blockIdx, count + 1);
      }
      else {
        // The slot is not an inline one. Inserting the clipped value into the subblock
        long handle;
        if (tag == HASHED_BLOCK)
          handle = InsertIntoHashedBlock(Payload(low), Count(slot), Clipped(value));
        else
          handle = InsertWithLinearBlock(slot, Clipped(value));

        if (handle == slot)
          return HashedBlockHandle(blockIdx, count);

        Debug.Assert(Count(handle) == Count(slot) + 1);
        SetFullSlot(slotIdx, handle);
        return HashedBlockHandle(blockIdx, count + 1);
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    private long InsertUniqueWithLinearBlock(long handle, int value) {
      Debug.Assert(Tag(Low(handle)) >= SIZE_2_BLOCK & Tag(Low(handle)) <= SIZE_16_BLOCK);

      int low = Low(handle);
      int tag = Tag(low);
      int blockIdx = Payload(low);
      int count = Count(handle);
      int capacity = Capacity(tag);

      // Inserting the new value if there's still room here
      if (count < capacity) {
        int slotIdx = blockIdx + count / 2;
        if (IsEven(count))
          SetSlot(slotIdx, value, EMPTY_MARKER);
        else
          SetSlotHigh(slotIdx, value);
        return LinearBlockHandle(tag, blockIdx, count + 1);
      }

      if (tag != SIZE_16_BLOCK) {
        // Allocating the new block
        int newBlockIdx = tag == SIZE_2_BLOCK ? Alloc4Block() : (tag == SIZE_4_BLOCK ? Alloc8Block() : Alloc16Block());

        // Initializing the new block
        int idx = count / 2;
        for (int i=0 ; i < idx ; i++)
          SetFullSlot(newBlockIdx + i, Slot(blockIdx + i));
        SetSlot(newBlockIdx + idx, value, EMPTY_MARKER);
        for (int i=idx+1 ; i < count ; i++)
          MarkSlotAsEmpty(newBlockIdx + i);

        // Releasing the old block
        if (tag == SIZE_2_BLOCK)
          Release2Block(blockIdx);
        else if (tag == SIZE_4_BLOCK)
          Release4Block(blockIdx);
        else
          Release8Block(blockIdx);

        return LinearBlockHandle(tag + 1, newBlockIdx, count + 1);
      }

      // Allocating and initializing the hashed block
      int hashedBlockIdx = Alloc16Block();
      for (int i=0 ; i < 16 ; i++)
        MarkSlotAsEmpty(hashedBlockIdx + i);

      // Transferring the existing values
      for (int i=0 ; i < 16 ; i++) {
        long slot = Slot(blockIdx + i);
        long tempHandle = InsertUniqueIntoHashedBlock(hashedBlockIdx, 2 * i, Low(slot));
        Debug.Assert(Count(tempHandle) == 2 * i + 1);
        Debug.Assert(Payload(Low(tempHandle)) == hashedBlockIdx);
        tempHandle = InsertUniqueIntoHashedBlock(hashedBlockIdx, 2 * i + 1, High(slot));
        Debug.Assert(Count(tempHandle) == 2 * (i + 1));
        Debug.Assert(Payload(Low(tempHandle)) == hashedBlockIdx);
      }

      // Releasing the old block
      Release16Block(blockIdx);

      // Adding the new value
      return InsertUniqueIntoHashedBlock(hashedBlockIdx, 32, value);
    }

    private long InsertUniqueIntoHashedBlock(int blockIdx, int count, int value) {
      int slotIdx = blockIdx + Index(value);
      long slot = Slot(slotIdx);
      int low = Low(slot);

      // Checking for empty slots
      if (low == EMPTY_MARKER) {
        SetSlot(slotIdx, value, EMPTY_MARKER);
        return HashedBlockHandle(blockIdx, count + 1);
      }

      int tag = Tag(low);

      // Checking for inline slots
      if (tag == INLINE_SLOT) {
        Debug.Assert(value != low);
        int high = High(slot);
        if (high == EMPTY_MARKER) {
          SetSlot(slotIdx, low, value);
          return HashedBlockHandle(blockIdx, count + 1);
        }
        Debug.Assert(Tag(high) == INLINE_SLOT);
        Debug.Assert(value != high);
        long handle = Insert2Block(Combine(Clipped(low), Clipped(high)), Clipped(value));
        Debug.Assert(Count(handle) == 3);
        SetFullSlot(slotIdx, handle);
        return HashedBlockHandle(blockIdx, count + 1);
      }
      else {
        // The slot is not an inline one. Inserting the clipped value into the subblock
        long handle;
        if (tag == HASHED_BLOCK)
          handle = InsertUniqueIntoHashedBlock(Payload(low), Count(slot), Clipped(value));
        else
          handle = InsertUniqueWithLinearBlock(slot, Clipped(value));

        Debug.Assert(Count(handle) == Count(slot) + 1);
        SetFullSlot(slotIdx, handle);
        return HashedBlockHandle(blockIdx, count + 1);
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    private long DeleteFromLinearBlock(long handle, int value) {
      int tag = Tag(Low(handle));
      int blockIdx = Payload(Low(handle));
      int count = Count(handle);

      int lastSlotIdx = (count + 1) / 2 - 1;
      long lastSlot = Slot(blockIdx + lastSlotIdx);

      int lastLow = Low(lastSlot);
      int lastHigh = High(lastSlot);

      Debug.Assert(lastLow != EMPTY_MARKER && Tag(lastLow) == INLINE_SLOT);
      Debug.Assert((lastHigh != EMPTY_MARKER && Tag(lastHigh) == INLINE_SLOT) || (lastHigh == EMPTY_MARKER && !IsEven(count)));

      // Checking the last slot first
      if (value == lastLow | value == lastHigh) {
        // Removing the value
        if (value == lastLow)
          SetSlot(blockIdx + lastSlotIdx, lastHigh, EMPTY_MARKER);
        else
          SetSlot(blockIdx + lastSlotIdx, lastLow, EMPTY_MARKER);

        // Shrinking the block if need be
        if (count == MinCount(tag))
          return ShrinkLinearBlock(tag, blockIdx, count - 1);
        else
          return LinearBlockHandle(tag, blockIdx, count - 1);
      }

      // The last slot didn't contain the searched value, looking in the rest of the array
      for (int i = lastSlotIdx - 1 ; i >= 0 ; i--) {
        long slot = Slot(blockIdx + i);
        int low = Low(slot);
        int high = High(slot);

        Debug.Assert(low != EMPTY_MARKER && Tag(low) == INLINE_SLOT);
        Debug.Assert(high != EMPTY_MARKER && Tag(high) == INLINE_SLOT);

        if (value == low | value == high) {
          // Removing the last value to replace the one being deleted
          int last;
          if (IsEven(count)) {
            last = lastHigh;
            SetSlot(blockIdx + lastSlotIdx, lastLow, EMPTY_MARKER);
          }
          else {
            last = lastLow;
            MarkSlotAsEmpty(blockIdx + lastSlotIdx);
          }

          // Replacing the value to be deleted with the last one
          if (value == low)
            SetSlot(blockIdx + i, last, high);
          else
            SetSlot(blockIdx + i, low, last);

          // Shrinking the block if need be
          if (count == MinCount(tag))
            return ShrinkLinearBlock(tag, blockIdx, count - 1);
          else
            return LinearBlockHandle(tag, blockIdx, count - 1);
        }
      }

      // Value not found
      return handle;
    }

    private long ShrinkLinearBlock(int tag, int blockIdx, int count) {
      if (tag == SIZE_2_BLOCK) {
        Debug.Assert(count == 2);
        return Slot(blockIdx);
      }

      if (tag == SIZE_4_BLOCK) {
        Debug.Assert(count == 3);
        int block2Idx = Alloc2Block();
        SetFullSlot(block2Idx, Slot(blockIdx));
        SetFullSlot(block2Idx + 1, Slot(blockIdx + 1));
        Release4Block(blockIdx);
        return Size2BlockHandle(block2Idx, count);
      }

      if (tag == SIZE_8_BLOCK) {
        Debug.Assert(count == 6);
        Release8BlockUpperHalf(blockIdx);
        return Size4BlockHandle(blockIdx, count);
      }

      Debug.Assert(tag == SIZE_16_BLOCK);
      Debug.Assert(count == 12);
      Release16BlockUpperHalf(blockIdx);
      return Size8BlockHandle(blockIdx, count);
    }

    ////////////////////////////////////////////////////////////////////////////

    private long DeleteFromHashedBlock(int blockIdx, int count, int value) {
      int index = Index(value);
      int slotIdx = blockIdx + index;
      long slot = Slot(slotIdx);

      // If the slot is empty there's nothing to do
      if (slot == EMPTY_SLOT)
        return HashedBlockHandle(blockIdx, count);

      int low = Low(slot);
      int high = High(slot);

      // If the slot is not inline, we recursively call Delete(..) with a clipped value
      if (Tag(low) != INLINE_SLOT) {
        long handle = Delete(slot, Clipped(value));
        if (handle == slot)
          return HashedBlockHandle(blockIdx, count);
        int handleLow = Low(handle);
        if (Tag(handleLow) == INLINE_SLOT)
          handle = Combine(Unclipped(handleLow, index), Unclipped(High(handle), index));
        SetFullSlot(slotIdx, handle);
      }
      else if (low == value) {
        if (high == EMPTY_MARKER)
          MarkSlotAsEmpty(slotIdx);
        else
          SetSlot(slotIdx, high, EMPTY_MARKER);
      }
      else if (high == value) {
        Debug.Assert(high != EMPTY_MARKER);
        SetSlot(slotIdx, low, EMPTY_MARKER);
      }
      else {
        return HashedBlockHandle(blockIdx, count);
      }

      Debug.Assert(count >= HASHED_BLOCK_MIN_COUNT);

      // The value has actually been deleted. Shrinking the block if need be
      if (count > HASHED_BLOCK_MIN_COUNT)
        return HashedBlockHandle(blockIdx, count - 1);
      else
        return ShrinkHashedBlock(blockIdx);
    }

    private long ShrinkHashedBlock(int blockIdx) {
      Debug.Assert(HASHED_BLOCK_MIN_COUNT == 13);

      // Here we've exactly 12 elements left, therefore we need the save the first 6 slots
      long slot0  = Slot(blockIdx);
      long slot1  = Slot(blockIdx + 1);
      long slot2  = Slot(blockIdx + 2);
      long slot3  = Slot(blockIdx + 3);
      long slot4  = Slot(blockIdx + 4);
      long slot5  = Slot(blockIdx + 5);

      long state = Combine(blockIdx, EMPTY_MARKER);
      state = CopyAndReleaseBlock(slot0, state, 0);
      state = CopyAndReleaseBlock(slot1, state, 1);
      state = CopyAndReleaseBlock(slot2, state, 2);
      state = CopyAndReleaseBlock(slot3, state, 3);
      state = CopyAndReleaseBlock(slot4, state, 4);
      state = CopyAndReleaseBlock(slot5, state, 5);

      int endIdx = blockIdx + 6;
      for (int i=6 ; Low(state) < endIdx ; i++)
        state = CopyAndReleaseBlock(Slot(blockIdx + i), state, i);

      Debug.Assert(state == Combine(blockIdx + 6, EMPTY_MARKER));

      MarkSlotAsEmpty(blockIdx + 6);
      MarkSlotAsEmpty(blockIdx + 7);

      Release16BlockUpperHalf(blockIdx);
      return Size8BlockHandle(blockIdx, 12);
    }

    private long CopyAndReleaseBlock(long handle, long state, int leastBits) {
      if (handle == EMPTY_SLOT)
        return state;

      int low = Low(handle);
      int tag = Tag(low);

      int nextIdx = Low(state);
      int leftover = High(state);

      if (tag == INLINE_SLOT) {
        int high = High(handle);

        if (leftover != EMPTY_MARKER) {
          SetSlot(nextIdx++, leftover, low);
          leftover = EMPTY_MARKER;
        }
        else
          leftover = low;

        if (high != EMPTY_MARKER)
          if (leftover != EMPTY_MARKER) {
            SetSlot(nextIdx++, leftover, high);
            leftover = EMPTY_MARKER;
          }
          else
            leftover = high;
      }
      else {
        int blockIdx = Payload(low);
        int end = (Count(handle) + 1) / 2;

        for (int i=0 ; i < end ; i++) {
          long slot = Slot(blockIdx + i);

          Debug.Assert(slot != EMPTY_SLOT);

          int slotLow = Low(slot);
          int slotHigh = High(slot);

          if (leftover != EMPTY_MARKER) {
            SetSlot(nextIdx++, leftover, Unclipped(slotLow, leastBits));
            leftover = EMPTY_MARKER;
          }
          else
            leftover = Unclipped(slotLow, leastBits);

          if (slotHigh != EMPTY_MARKER) {
            if (leftover != EMPTY_MARKER) {
              SetSlot(nextIdx++, leftover, Unclipped(slotHigh, leastBits));
              leftover = EMPTY_MARKER;
            }
            else
              leftover = Unclipped(slotHigh, leastBits);
          }
        }

        if (tag == SIZE_2_BLOCK) {
          Release2Block(blockIdx);
        }
        else if (tag == SIZE_4_BLOCK) {
          Release4Block(blockIdx);
        }
        else {
          // Both 16-slot and hashed blocks contain at least 7 elements, so they cannot appear
          // here, as the parent hashed block being shrunk has only six elements left
          Debug.Assert(tag == SIZE_8_BLOCK);
          Release8Block(blockIdx);
        }
      }

      return Combine(nextIdx, leftover);
    }
  }
}
