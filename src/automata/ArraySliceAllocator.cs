namespace Cell.Runtime {
  class ArraySliceAllocator {
    private const int MIN_SIZE = 32;

    public const int EMPTY_MARKER  = unchecked ((int) 0xFFFFFFFF);

    private const int END_LOWER_MARKER     = unchecked ((int) 0xFFFFFFFF);
    private const int END_UPPER_MARKER_2   = 0x3FFFFFFF;
    private const int END_UPPER_MARKER_4   = 0x5FFFFFFF;
    private const int END_UPPER_MARKER_8   = 0x7FFFFFFF;
    private const int END_UPPER_MARKER_16  = unchecked ((int) 0x9FFFFFFF);

    private const int PAYLOAD_MASK  = 0x1FFFFFFF;

    // private const int BLOCK_1    = 0;
    private const int BLOCK_2    = 1;
    private const int BLOCK_4    = 2;
    private const int BLOCK_8    = 3;
    private const int BLOCK_16   = 4;
    // private const int BLOCK_32   = 5;
    // private const int BLOCK_64   = 6;
    private const int AVAILABLE  = 7;

    //////////////////////////////////////////////////////////////////////////////

    private long[] slots;
    private int head2, head4, head8, head16;

    //////////////////////////////////////////////////////////////////////////////

    public static int Low(long slot) {
      return (int) (slot & 0xFFFFFFFFL);
    }

    public static int High(long slot) {
      return (int) Miscellanea.UnsignedLeftShift64(slot, 32);
    }

    public static long Combine(int low, int high) {
      long slot = (((long) low) & 0xFFFFFFFFL) | (((long) high) << 32);
      Debug.Assert(Low(slot) == low & High(slot) == high);
      return slot;
    }

    //////////////////////////////////////////////////////////////////////////////

    public static int Tag(int word) {
      return Miscellanea.UnsignedLeftShift32(word, 29);
    }

    protected static int Payload(int word) {
      return word & PAYLOAD_MASK;
    }

    protected static int Tag(int tag, int payload) {
      Debug.Assert(Tag(payload) == 0);
      return (tag << 29) | payload;
    }

    //////////////////////////////////////////////////////////////////////////////

    protected long[] Slots() {
      return slots;
    }

    protected long Slot(int index) {
      return slots[index];
    }

    protected void SetFullSlot(int index, long value) {
      slots[index] = value;
    }

    protected void SetSlot(int index, int low, int high) {
      slots[index] = Combine(low, high);
    }

    protected void SetSlotLow(int index, int value) {
      SetSlot(index, value, High(Slot(index)));
    }

    protected void SetSlotHigh(int index, int value) {
      SetSlot(index, Low(Slot(index)), value);
    }

    //////////////////////////////////////////////////////////////////////////////

    protected ArraySliceAllocator() {
      slots = new long[MIN_SIZE];

      SetSlot(0, END_LOWER_MARKER, Tag(BLOCK_16, 16));
      for (int i=16 ; i < MIN_SIZE - 16 ; i += 16)
        SetSlot(i, Tag(AVAILABLE, i - 16), Tag(BLOCK_16, i + 16));
      SetSlot(MIN_SIZE-16, Tag(AVAILABLE, MIN_SIZE-32), END_UPPER_MARKER_16);

      head2 = head4 = head8 = EMPTY_MARKER;
      head16 = 0;
    }

    //////////////////////////////////////////////////////////////////////////////

    protected int Alloc2Block() {
      if (head2 != EMPTY_MARKER) {
        Debug.Assert(Low(Slot(head2)) == END_LOWER_MARKER);
        Debug.Assert(High(Slot(head2)) == END_UPPER_MARKER_2 || Tag(High(Slot(head2))) == BLOCK_2);

        int blockIdx = head2;
        head2 = RemoveBlockFromChain(blockIdx, Slot(blockIdx), END_UPPER_MARKER_2, head2);
        return blockIdx;
      }
      else {
        int block4Idx = Alloc4Block();
        head2 = AddBlockToChain(block4Idx, BLOCK_2, END_UPPER_MARKER_2, head2);
        return block4Idx + 2;
      }
    }

    protected void Release2Block(int blockIdx) {
      Debug.Assert((blockIdx & 1) == 0);

      bool isFirst = (blockIdx & 3) == 0;
      int otherBlockIdx = blockIdx + (isFirst ? 2 : -2);
      long otherBlockSlot0 = Slot(otherBlockIdx);

      if (Tag(Low(otherBlockSlot0)) == AVAILABLE) {
        Debug.Assert(Tag(High(otherBlockSlot0)) == BLOCK_2);

        // The matching block is available, so we release both at once as a 4-slot block
        // But first we have to remove the matching block from the 2-slot block chain
        head2 = RemoveBlockFromChain(otherBlockIdx, otherBlockSlot0, END_UPPER_MARKER_2, head2);
        Release4Block(isFirst ? blockIdx : otherBlockIdx);
      }
      else {
        // The matching block is not available, so we
        // just add the new one to the 2-slot block chain
        head2 = AddBlockToChain(blockIdx, BLOCK_2, END_UPPER_MARKER_2, head2);
      }
    }

    protected int Alloc4Block() {
      if (head4 != EMPTY_MARKER) {
        Debug.Assert(Low(Slot(head4)) == END_LOWER_MARKER);
        Debug.Assert(High(Slot(head4)) == END_UPPER_MARKER_4 | Tag(High(Slot(head4))) == BLOCK_4);

        int blockIdx = head4;
        head4 = RemoveBlockFromChain(blockIdx, Slot(blockIdx), END_UPPER_MARKER_4, head4);
        return blockIdx;
      }
      else {
        int block8Idx = Alloc8Block();
        head4 = AddBlockToChain(block8Idx, BLOCK_4, END_UPPER_MARKER_4, head4);
        return block8Idx + 4;
      }
    }

    protected void Release4Block(int blockIdx) {
      Debug.Assert((blockIdx & 3) == 0);

      bool isFirst = (blockIdx & 7) == 0;
      int otherBlockIdx = blockIdx + (isFirst ? 4 : -4);
      long otherBlockSlot0 = Slot(otherBlockIdx);

      if (Tag(Low(otherBlockSlot0)) == AVAILABLE & Tag(High(otherBlockSlot0)) == BLOCK_4) {
        head4 = RemoveBlockFromChain(otherBlockIdx, otherBlockSlot0, END_UPPER_MARKER_4, head4);
        Release8Block(isFirst ? blockIdx : otherBlockIdx);
      }
      else
        head4 = AddBlockToChain(blockIdx, BLOCK_4, END_UPPER_MARKER_4, head4);
    }

    protected int Alloc8Block() {
      if (head8 != EMPTY_MARKER) {
        Debug.Assert(Low(Slot(head8)) == END_LOWER_MARKER);
        Debug.Assert(High(Slot(head8)) == END_UPPER_MARKER_8 | Tag(High(Slot(head8))) == BLOCK_8);

        int blockIdx = head8;
        head8 = RemoveBlockFromChain(blockIdx, Slot(blockIdx), END_UPPER_MARKER_8, head8);
        return blockIdx;
      }
      else {
        int block16Idx = Alloc16Block();
        Debug.Assert(Low(Slot(block16Idx)) == END_LOWER_MARKER);
        Debug.Assert(High(Slot(block16Idx)) == END_UPPER_MARKER_16 | Tag(High(Slot(block16Idx))) == BLOCK_16);
        head8 = AddBlockToChain(block16Idx, BLOCK_8, END_UPPER_MARKER_8, head8);
        return block16Idx + 8;
      }
    }

    protected void Release8Block(int blockIdx) {
      Debug.Assert((blockIdx & 7) == 0);

      bool isFirst = (blockIdx & 15) == 0;
      int otherBlockIdx = blockIdx + (isFirst ? 8 : -8);
      long otherBlockSlot0 = Slot(otherBlockIdx);

      if (Tag(Low(otherBlockSlot0)) == AVAILABLE & Tag(High(otherBlockSlot0)) == BLOCK_8) {
        head8 = RemoveBlockFromChain(otherBlockIdx, otherBlockSlot0, END_UPPER_MARKER_8, head8);
        Release16Block(isFirst ? blockIdx : otherBlockIdx);
      }
      else
        head8 = AddBlockToChain(blockIdx, BLOCK_8, END_UPPER_MARKER_8, head8);
    }

    protected void Release8BlockUpperHalf(int blockIdx) {
      head4 = AddBlockToChain(blockIdx+4, BLOCK_4, END_UPPER_MARKER_4, head4);
    }

    protected int Alloc16Block() {
      if (head16 == EMPTY_MARKER) {
        int len = slots.Length;
        long[] newSlots = new long[2*len];
        Array.Copy(slots, newSlots, len);
        slots = newSlots;
        for (int i=len ; i < 2 * len ; i += 16)
          SetSlot(i, Tag(AVAILABLE, i - 16), Tag(BLOCK_16, i + 16));

        Debug.Assert(High(Slot(len)) == Tag(BLOCK_16, len + 16));
        Debug.Assert(Low(Slot(2 * len - 16)) == Tag(AVAILABLE, 2 * len - 32));

        SetSlot(len, END_LOWER_MARKER, Tag(BLOCK_16, len + 16));
        SetSlot(2 * len - 16, Tag(AVAILABLE, 2 * len - 32), END_UPPER_MARKER_16);

        head16 = len;
      }

      Debug.Assert(Low(Slot(head16)) == END_LOWER_MARKER);
      Debug.Assert(High(Slot(head16)) == END_UPPER_MARKER_16 | Tag(High(Slot(head16))) == BLOCK_16);

      int blockIdx = head16;
      head16 = RemoveBlockFromChain(blockIdx, Slot(blockIdx), END_UPPER_MARKER_16, head16);
      return blockIdx;
    }

    protected void Release16Block(int blockIdx) {
      head16 = AddBlockToChain(blockIdx, BLOCK_16, END_UPPER_MARKER_16, head16);
    }

    protected void Release16BlockUpperHalf(int blockIdx) {
      head8 = AddBlockToChain(blockIdx+8, BLOCK_8, END_UPPER_MARKER_8, head8);
    }

    //////////////////////////////////////////////////////////////////////////////

    private int RemoveBlockFromChain(int blockIdx, long firstSlot, int endUpperMarker, int head) {
      int firstLow = Low(firstSlot);
      int firstHigh = High(firstSlot);

      if (firstLow != END_LOWER_MARKER) {
        // Not the first block in the chain
        Debug.Assert(head != blockIdx);
        int prevBlockIdx = Payload(firstLow);

        if (firstHigh != endUpperMarker) {
          // The block is in the middle of the chain
          // The previous and next blocks must be repointed to each other
          int nextBlockIdx = Payload(firstHigh);
          SetSlotHigh(prevBlockIdx, firstHigh);
          SetSlotLow(nextBlockIdx, firstLow);
        }
        else {
          // Last block in a chain with multiple blocks
          // The 'next' field of the previous block must be cleared
          SetSlotHigh(prevBlockIdx, endUpperMarker);
        }
      }
      else {
        // First slot in the chain, must be the one pointed to by head
        Debug.Assert(head == blockIdx);

        if (firstHigh != endUpperMarker) {
          // The head must be repointed at the next block,
          // whose 'previous' field must now be cleared
          int nextBlockIdx = Payload(firstHigh);
          head = nextBlockIdx;
          SetSlotLow(nextBlockIdx, END_LOWER_MARKER);
        }
        else {
          // No 'previous' nor 'next' slots, it must be the only one
          // Just resetting the head of the 2-slot block chain
          head = EMPTY_MARKER;
        }
      }

      return head;
    }

    private int AddBlockToChain(int blockIdx, int sizeTag, int endUpperMarker, int head) {
      if (head != EMPTY_MARKER) {
        // If the list of blocks is not empty, we link the first two blocks
        // The 'previous' field of the newly released block must be cleared
        SetSlot(blockIdx, END_LOWER_MARKER, Tag(sizeTag, head));
        SetSlotLow(head, Tag(AVAILABLE, blockIdx));
      }
      else {
        // Otherwise we just clear then 'next' field of the newly released block
        // The 'previous' field of the newly released block must be cleared
        SetSlot(blockIdx, END_LOWER_MARKER, endUpperMarker);
      }
      // The new block becomes the head one
      return blockIdx;
    }
  }
}
