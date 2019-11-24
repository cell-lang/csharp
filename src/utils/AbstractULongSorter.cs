namespace Cell.Runtime {
  public abstract class AbstractULongSorter {
    public void Sort(ulong[] array, int size) {
      _sort(array, 0, size-1);
    }

    public void Sort(ulong[] array, int start, int end) {
      _sort(array, start, end-1);
    }

    //////////////////////////////////////////////////////////////////////////////

    private void _sort(ulong[] array, int first, int last) {
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
        while (low <= last && !IsGreater(array[low], array[first]))
          low++;

        // Decrementing high until it points to the first slot whose
        // value is lower or equal to the pivot. Such slot may be the
        // first one, which contains the pivot
        while (high > first && IsGreater(array[high], array[first]))
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
      _sort(array, first, lastLeq-1);

      // The greater-then partition starts at high + 1 and
      // continues until the end of the array
      _sort(array, high+1, last);
    }


    //////////////////////////////////////////////////////////////////////////////

    private void Swap(int idx1, int idx2, ulong[] array) {
      ulong tmp = array[idx1];
      array[idx1] = array[idx2];
      array[idx2] = tmp;
    }

    //////////////////////////////////////////////////////////////////////////////

    protected abstract bool IsGreater(ulong value1, ulong value2);
  }
}
