namespace Cell.Runtime {
  static class Ints12 {
    public static void Sort(int[] array, int size) {
      Sort(array, 0, size-1);
    }

    private static void Sort(int[] array, int first, int last) {
      if (first >= last)
        return;

      int pivot = first + (last - first) / 2;

      if (pivot != first)
        Swap(first, pivot, array);

      int low = first + 1;
      int high = last;

      for ( ; ; ) {
        // Incrementing low until it points to the first slot that does
        // not contain a value that is lower or equal to the pivot
        // Such slot may be the first element after the end of the array
        while (low <= last && !IsGreater(low, first, array))
          low++;

        // Decrementing high until it points to the first slot whose
        // value is lower or equal to the pivot. Such slot may be the
        // first one, which contains the pivot
        while (high > first && IsGreater(high, first, array))
          high--;

        Debug.Assert(low != high);
        Debug.Assert(low < high | low == high + 1);

        // Once low and high have moved past each other all elements have been partitioned
        if (low > high)
          break;

        // Swapping the first pair of out-of-order elements before resuming the scan
        Swap(low++, high--, array);
      }

      // Putting the pivot between the two partitions
      int lastLeq = low - 1;
      if (lastLeq != first)
        Swap(lastLeq, first, array);

      // Now the lower-or-equal partition starts at 'first' and ends at
      // 'lastLeq - 1' (inclusive), since lastLeq contains the pivot
      Sort(array, first, lastLeq-1);

      // The greater-then partition starts at high + 1 and
      // continues until the end of the array
      Sort(array, high+1, last);
    }


    //////////////////////////////////////////////////////////////////////////////

    private static void Swap(int idx1, int idx2, int[] array) {
      int offset1 = 2 * idx1;
      int offset2 = 2 * idx2;
      int tmp0 = array[offset1];
      int tmp1 = array[offset1 + 1];
      array[offset1]     = array[offset2];
      array[offset1 + 1] = array[offset2 + 1];
      array[offset2]     = tmp0;
      array[offset2 + 1] = tmp1;
    }

    //////////////////////////////////////////////////////////////////////////////

    public static bool Contains(int[] array, int size, int val1, int val2) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        switch (OrdCheck(mid, val1, val2, array)) {
          case -1: // mid < target range
            low = mid + 1;
            break;

          case 0: // mid in target range
            return true;

          case 1: // mid > target range
            high = mid - 1;
            break;
        }
      }

      return false;
    }

    public static bool Contains1(int[] array, int size, int val1) {
      return Contains1(array, 0, size, val1);
    }

    public static bool Contains1(int[] array, int offset, int count, int val1) {
      int low = offset;
      int high = offset + count - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        switch (RangeCheck1(mid, val1, array)) {
          case -1: // mid < target range
            low = mid + 1;
            break;

          case 0: // mid in target range
            return true;

          case 1: // mid > target range
            high = mid - 1;
            break;
        }
      }

      return false;
    }

    public static int IndexFirst1(int[] array, int size, int val1) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        switch (RangeStartCheck1(mid, val1, array)) {
          case -1: // mid < target range start
            low = mid + 1;
            break;

          case 0: // mid == target range start
            return mid;

          case 1: // mid > target range start
            high = mid - 1;
            break;
        }
      }

      return -1;
    }

    public static int Count1(int[] array, int size, int val1, int offset) {
      return RangeEndExclusive(array, size, val1, offset) - offset;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static int RangeEndExclusive(int[] array, int size, int val1, int offset) {
      int low = offset;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        switch (RangeEndCheck1(mid, val1, array, size)) {
          case -1: // mid < target range end
            low = mid + 1;
            break;

          case 0: // mid == target range end
            return mid + 1;

          case 1: // mid > target range end
            high = mid - 1;
            break;
        }
      }

      return offset;
    }

    private static int RangeStartCheck1(int idx, int val1, int[] array) {
      int ord = RangeCheck1(idx, val1, array);
      if (ord != 0 | idx == 0)
        return ord;
      ord = RangeCheck1(idx-1, val1, array);
      Debug.Assert(ord == 0 | ord == -1);
      return ord == -1 ? 0 : 1;
    }

    private static int RangeEndCheck1(int idx, int val1, int[] array, int size) {
      int ord = RangeCheck1(idx, val1, array);
      if (ord != 0 | idx == size-1)
        return ord;
      ord = RangeCheck1(idx+1, val1, array);
      Debug.Assert(ord == 0 | ord == 1);
      return ord == 1 ? 0 : -1;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static bool IsGreater(int idx1, int idx2, int[] array) {
      int offset1 = 2 * idx1;
      int offset2 = 2 * idx2;
      int elem1 = array[offset1];
      int elem2 = array[offset2];
      if (elem1 != elem2)
        return elem1 > elem2;
      elem1 = array[offset1 + 1];
      elem2 = array[offset2 + 1];
      return elem1 > elem2;
    }

    private static int OrdCheck(int idx, int val1, int val2, int[] array) {
      int offset = 2 * idx;
      int val = array[offset];
      if (val < val1)
        return -1;
      if (val > val1)
        return 1;
      val = array[offset + 1];
      if (val < val2)
        return -1;
      if (val > val2)
        return 1;
      return 0;
    }

    private static int RangeCheck1(int idx, int val1, int[] array) {
      int offset = 2 * idx;
      int val = array[offset];
      if (val < val1)
        return -1;
      if (val > val1)
        return 1;
      return 0;
    }
  }
}
