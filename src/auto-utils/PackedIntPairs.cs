namespace Cell.Runtime {
  class PackedIntPairs {
    public static void Sort(long[] array, int size) {
      Array.Sort(array, size);
    }

    public static void SortFlipped(long[] array, int size) {
      Flip(array, size);
      Array.Sort(array, size);
      Flip(array, size);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static bool ContainsMajor(long[] array, int size, int value) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int midIdx = low + (high - low) / 2;
        int majorVal = Miscellanea.High(array[midIdx]);

        if (majorVal < value)
          // midIdx is below the target range
          low = midIdx + 1;
        else if (majorVal > value)
          // midIdx is above the target range
          high = midIdx - 1;
        else
          return true;
      }

      return false;
    }

    public static bool ContainsMinor(long[] array, int size, int value) {
      int low = 0;
      int high = size - 1;

      while (low <= high) {
        int midIdx = low + (high - low) / 2;
        int minorVal = Miscellanea.Low(array[midIdx]);

        if (minorVal < value)
          // midIdx is below the target range
          low = midIdx + 1;
        else if (minorVal > value)
          // midIdx is above the target range
          high = midIdx - 1;
        else
          return true;
      }

      return false;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static void Flip(long[] array, int size) {
      for (int i=0 ; i < size ; i++) {
        long entry = array[i];
        array[i] = Miscellanea.Pack(Miscellanea.High(entry), Miscellanea.Low(entry));
      }
    }
  }
}
