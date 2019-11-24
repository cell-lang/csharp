namespace Cell.Runtime {
  class Ints123 {
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
      int offset1 = 3 * idx1;
      int offset2 = 3 * idx2;
      int tmp0 = array[offset1];
      int tmp1 = array[offset1 + 1];
      int tmp2 = array[offset1 + 2];
      array[offset1]     = array[offset2];
      array[offset1 + 1] = array[offset2 + 1];
      array[offset1 + 2] = array[offset2 + 2];
      array[offset2]     = tmp0;
      array[offset2 + 1] = tmp1;
      array[offset2 + 2] = tmp2;
    }

    //////////////////////////////////////////////////////////////////////////////

    //## THIS HASN'T BEEN TESTED YET
    public static bool Contains(int[] array, int size, int val1, int val2, int val3) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        int ord = RangeCheck123(mid, val1, val2, val3, array);
        if (ord == -1) // mid < target range
          low = mid + 1;
        else if (ord == 1) // mid > target range
          high = mid - 1;
        else
          return true;
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
        int ord = RangeCheck1(mid, val1, array);
        if (ord == -1) // mid < target range
          low = mid + 1;
        else if (ord == 1) // mid > target range
          high = mid - 1;
        else
          return true;
      }

      return false;
    }

    public static bool Contains12(int[] array, int size, int val1, int val2) {
      return Contains12(array, 0, size, val1, val2);
    }

    public static bool Contains12(int[] array, int offset, int count, int val1, int val2) {
      int low = offset;
      int high = offset + count - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        int ord = RangeCheck12(mid, val1, val2, array);
        if (ord == -1) // mid < target range
          low = mid + 1;
        else if (ord == 1) // mid > target range
          high = mid - 1;
        else
          return true;
      }

      return false;
    }

    public static int IndexFirst1(int[] array, int size, int val1) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        int ord = RangeStartCheck1(mid, val1, array);
        if (ord == -1) // mid < target range start
          low = mid + 1;
        else if (ord == 1) // mid > target range start
          high = mid - 1;
        else
          return mid;
      }

      return -1;
    }

    public static int IndexFirst12(int[] array, int size, int val1, int val2) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        int ord = RangeStartCheck12(mid, val1, val2, array);
        if (ord == -1) // mid < target range start
          low = mid + 1;
        else if (ord == 1) // mid > target range start
          high = mid - 1;
        else
          return mid;
      }

      return -1;
    }

    public static int Count1(int[] array, int size, int val1, int offset) {
      return RangeEndExclusive1(val1, offset, array, size) - offset;
    }

    public static int Count12(int[] array, int size, int val1, int val2, int offset) {
      return RangeEndExclusive12(val1, val2, offset, array, size) - offset;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static int RangeEndExclusive1(int val1, int offset, int[] array, int size) {
      int low = offset;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        int ord = RangeEndCheck1(mid, val1, array, size);
        if (ord == -1) // mid < target range end
          low = mid + 1;
        else if (ord == 1) // mid > target range end
          high = mid - 1;
        else
          return mid + 1;
      }

      return offset;
    }

    private static int RangeEndExclusive12(int val1, int val2, int offset, int[] array, int size) {
      int low = offset;
      int high = size - 1;

      while (low <= high) {
        int mid = low + (high - low) / 2;
        int ord = RangeEndCheck12(mid, val1, val2, array, size);
        if (ord == -1) // mid < target range end
          low = mid + 1;
        else if (ord == 1) // mid > target range end
          high = mid - 1;
        else
          return mid + 1;
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

    private static int RangeStartCheck12(int idx, int val1, int val2, int[] array) {
      int ord = RangeCheck12(idx, val1, val2, array);
      if (ord != 0 | idx == 0)
        return ord;
      ord = RangeCheck12(idx-1, val1, val2, array);
      Debug.Assert(ord == 0 | ord == -1);
      return ord == -1 ? 0 : 1;
    }

    private static int RangeEndCheck12(int idx, int val1, int val2, int[] array, int size) {
      int ord = RangeCheck12(idx, val1, val2, array);
      if (ord != 0 | idx == size-1)
        return ord;
      ord = RangeCheck12(idx+1, val1, val2, array);
      Debug.Assert(ord == 0 | ord == 1);
      return ord == 1 ? 0 : -1;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static bool IsGreater(int idx1, int idx2, int[] array) {
      int offset1 = 3 * idx1;
      int offset2 = 3 * idx2;
      int elem1 = array[offset1];
      int elem2 = array[offset2];
      if (elem1 != elem2)
        return elem1 > elem2;
      elem1 = array[offset1 + 1];
      elem2 = array[offset2 + 1];
      if (elem1 != elem2)
        return elem1 > elem2;
      elem1 = array[offset1 + 2];
      elem2 = array[offset2 + 2];
      return elem1 > elem2;
    }

    private static int RangeCheck1(int idx, int val1, int[] array) {
      int val = array[3 * idx];
      if (val < val1)
        return -1;
      if (val > val1)
        return 1;
      return 0;
    }

    private static int RangeCheck12(int idx, int val1, int val2, int[] array) {
      int offset = 3 * idx;
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

    private static int RangeCheck123(int idx, int val1, int val2, int val3, int[] array) {
      int offset = 3 * idx;
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
      val = array[offset + 2];
      if (val < val3)
        return -1;
      if (val > val3)
        return 1;
      return 0;
    }
  }
}
