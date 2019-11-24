namespace Cell.Runtime {
  public class Array {
    public static Obj[] emptyObjArray = new Obj[0];
    public static byte[] emptyByteArray = new byte[0];
    public static int[] emptyIntArray = new int[0];
    public static long[] emptyLongArray = new long[0];
    public static double[] emptyDoubleArray = new double[0];
    public static bool[] emptyBooleanArray = new bool[0];

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static char[] Repeat(char ch, int len) {
      char[] array = new char[len];
      Fill(array, ch);
      return array;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static int Capacity(int curr, int min) {
      int capacity = 256;
      while (capacity < min)
        capacity *= 2;
      return capacity;
    }

    public static int NextCapacity(int currCapacity) {
      return System.Math.Max(32, currCapacity + currCapacity / 2);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static ulong Sum(uint[] array) {
      ulong sum = 0;
      for (int i=0 ; i < array.Length ; i++)
        sum += array[i];
      return sum;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static void Copy<T>(T[] src, T[] dest, int count) {
      for (int i=0 ; i < count ; i++)
        dest[i] = src[i];
    }

    public static void Copy(sbyte[] src, sbyte[] dest, int count) {
      for (int i=0 ; i < count ; i++)
        dest[i] = src[i];
    }

    public static void Copy(byte[] src, byte[] dest, int count) {
      for (int i=0 ; i < count ; i++)
        dest[i] = src[i];
    }

    public static void Copy(short[] src, short[] dest, int count) {
      for (int i=0 ; i < count ; i++)
        dest[i] = src[i];
    }

    public static void Copy(int[] src, int[] dest, int count) {
      for (int i=0 ; i < count ; i++)
        dest[i] = src[i];
    }

    public static void Copy(long[] src, long[] dest, int count) {
      for (int i=0 ; i < count ; i++)
        dest[i] = src[i];
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static bool[] Take(bool[] array, int count) {
      Debug.Assert(count <= array.Length);
      bool[] subarray = new bool[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static sbyte[] Take(sbyte[] array, int count) {
      Debug.Assert(count <= array.Length);
      sbyte[] subarray = new sbyte[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static byte[] Take(byte[] array, int count) {
      Debug.Assert(count <= array.Length);
      byte[] subarray = new byte[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static short[] Take(short[] array, int count) {
      Debug.Assert(count <= array.Length);
      short[] subarray = new short[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static int[] Take(int[] array, int count) {
      Debug.Assert(count <= array.Length);
      int[] subarray = new int[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static uint[] Take(uint[] array, int count) {
      Debug.Assert(count <= array.Length);
      uint[] subarray = new uint[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static long[] Take(long[] array, int count) {
      Debug.Assert(count <= array.Length);
      long[] subarray = new long[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static ulong[] Take(ulong[] array, int count) {
      Debug.Assert(count <= array.Length);
      ulong[] subarray = new ulong[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static double[] Take(double[] array, int count) {
      Debug.Assert(count <= array.Length);
      double[] subarray = new double[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static Obj[] Take(Obj[] array, int count) {
      Debug.Assert(count <= array.Length);
      Obj[] subarray = new Obj[count];
      Copy(array, subarray, count);
      return subarray;
    }

    public static Obj[] Subarray(Obj[] array, int start, int end) {
      int len = end - start;
      Obj[] subarray = new Obj[len];
      for (int i=0 ; i < len ; i++)
        subarray[i] = array[start + i];
      return subarray;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static byte[] Append(byte[] array, int count, long value) {
      // Debug.Assert(count <= array.Length);
      // Debug.Assert(value >= -128 & value <= 127);

      if (count == array.Length)
        array = Extend(array, System.Math.Max(32, 3 * count / 2));
      array[count] = (byte) value;
      return array;
    }

    public static short[] Append(short[] array, int count, long value) {
      // Debug.Assert(count <= array.Length);
      // Debug.Assert(value >= -32768 & value <= 32767);

      if (count == array.Length)
        array = Extend(array, System.Math.Max(32, 3 * count / 2));
      array[count] = (short) value;
      return array;
    }

    public static int[] Append(int[] array, int count, int value) {
      Debug.Assert(count <= array.Length);

      if (count == array.Length)
        array = Extend(array, System.Math.Max(32, 3 * count / 2));
      array[count] = value;
      return array;
    }

    public static long[] Append(long[] array, int count, long value) {
      Debug.Assert(count <= array.Length);

      if (count == array.Length)
        array = Extend(array, System.Math.Max(32, 3 * count / 2));
      array[count] = value;
      return array;
    }

    public static double[] Append(double[] array, int count, double value) {
      Debug.Assert(count <= array.Length);

      if (count == array.Length)
        array = Extend(array, System.Math.Max(32, 3 * count / 2));
      array[count] = value;
      return array;
    }

    public static Obj[] Append(Obj[] array, int count, Obj value) {
      Debug.Assert(count <= array.Length);

      if (count == array.Length)
        array = Extend(array, System.Math.Max(32, 3 * count / 2));

      array[count] = value;
      return array;
    }

    public static int[] Append2(int[] array, int count, int val1, int val2) {
      Debug.Assert(2 * count <= array.Length);

      if (array.Length < 2 * (count + 1)) {
        int newLen = System.Math.Max(64, 2 * ((3 * count) / 2));
        int[] newArray = new int[newLen];
        Copy(array, newArray, 2 * count);
        array = newArray;
      }

      array[2 * count] = val1;
      array[2 * count + 1] = val2;

      return array;
    }

    public static int[] Append3(int[] array, int count, int val1, int val2, int val3) {
      Debug.Assert(3 * count <= array.Length);

      if (array.Length < 3 * (count + 1)) {
        int newLen = System.Math.Max(96, 3 * ((3 * count) / 2));
        int[] newArray = new int[newLen];
        Copy(array, newArray, 3 * count);
        array = newArray;
      }

      int offset = 3 * count;
      array[offset] = val1;
      array[offset + 1] = val2;
      array[offset + 2] = val3;

      return array;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static Obj At(bool[] array, int size, long idx) {
      if (idx < size)
        return SymbObj.Get(array[(int) idx]);
      else
        throw ErrorHandler.SoftFail();
    }

    public static Obj At(long[] array, int size, long idx) {
      if (idx < size)
        return IntObj.Get(array[(int) idx]);
      else
        throw ErrorHandler.SoftFail();
    }

    public static Obj At(double[] array, int size, long idx) {
      if (idx < size)
        return new FloatObj(array[(int) idx]);
      else
        throw ErrorHandler.SoftFail();
    }

    public static Obj At(Obj[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static bool BoolAt(bool[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static long LongAt(byte[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static long LongAt(sbyte[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static long LongAt(short[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static long LongAt(char[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static long LongAt(int[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static long LongAt(long[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    public static double FloatAt(double[] array, int size, long idx) {
      if (idx < size)
        return array[(int) idx];
      else
        throw ErrorHandler.SoftFail();
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static byte AsByte(long value) {
      return (byte) value;
    }

    public static short AsShort(long value) {
      return (short) value;
    }

    public static int AsInt(long value) {
      return (int) value;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static void Reset(long[] array) {
      int len = array.Length;
      for (int i=0 ; i < len ; i++)
        array[i] = 0;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static void Fill(char[] array, char value) {
      int len = array.Length;
      for (int i=0 ; i < len ; i++)
        array[i] = value;
    }

    public static void Fill(int[] array, int value) {
      Fill(array, 0, array.Length, value);
    }

    public static void Fill(uint[] array, uint value) {
      Fill(array, 0, array.Length, value);
    }

    public static void Fill(long[] array, long value) {
      Fill(array, 0, array.Length, value);
    }

    public static void Fill(ulong[] array, ulong value) {
      Fill(array, 0, array.Length, value);
    }

    public static void Fill(double[] array, double value) {
      Fill(array, 0, array.Length, value);
    }

    public static void Fill(int[] array, int count, int value) {
      Fill(array, 0, count, value);
    }

    public static void Fill(int[] array, int offset, int count, int value) {
      int end = offset + count;
      for (int i=offset ; i < end ; i++)
        array[i] = value;
    }

    public static void Fill(uint[] array, int offset, int count, uint value) {
      int end = offset + count;
      for (int i=offset ; i < end ; i++)
        array[i] = value;
    }

    public static void Fill(long[] array, int offset, int count, long value) {
      int end = offset + count;
      for (int i=offset ; i < end ; i++)
        array[i] = value;
    }

    public static void Fill(ulong[] array, int offset, int count, ulong value) {
      int end = offset + count;
      for (int i=offset ; i < end ; i++)
        array[i] = value;
    }

    public static void Fill(double[] array, int offset, int count, double value) {
      int end = offset + count;
      for (int i=offset ; i < end ; i++)
        array[i] = value;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static byte[] Extend(byte[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      byte[] newArray = new byte[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static short[] Extend(short[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      short[] newArray = new short[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static ushort[] Extend(ushort[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      ushort[] newArray = new ushort[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static char[] Extend(char[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      char[] newArray = new char[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static int[] Extend(int[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      int[] newArray = new int[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static long[] Extend(long[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      long[] newArray = new long[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static ulong[] Extend(ulong[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      ulong[] newArray = new ulong[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static double[] Extend(double[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      double[] newArray = new double[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static Obj[] Extend(Obj[] array, int newSize) {
      Debug.Assert(newSize > array.Length);
      Obj[] newArray = new Obj[newSize];
      Copy(array, newArray, array.Length);
      return newArray;
    }

    public static long[] Extend(long[] array, int newSize, long defaultValue) {
      Debug.Assert(newSize > array.Length);
      long[] newArray = new long[newSize];
      Copy(array, newArray, array.Length);
      Fill(newArray, array.Length, newSize - array.Length, defaultValue);
      return newArray;
    }

    public static double[] Extend(double[] array, int newSize, double defaultValue) {
      Debug.Assert(newSize > array.Length);
      double[] newArray = new double[newSize];
      Copy(array, newArray, array.Length);
      Fill(newArray, array.Length, newSize - array.Length, defaultValue);
      return newArray;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static void Sort(int[] array) {
      System.Array.Sort(array);
    }

    public static void Sort(long[] array) {
      System.Array.Sort(array);
    }

    public static void Sort(ulong[] array) {
      System.Array.Sort(array);
    }

    public static void Sort(int[] array, int size) {
      System.Array.Sort(array, 0, size);
    }

    public static void Sort(long[] array, int size) {
      System.Array.Sort(array, 0, size);
    }

    public static void Sort(int[] array, int start, int end) {
      System.Array.Sort(array, start, end - start);
    }

    public static void Sort(long[] array, int start, int end) {
      System.Array.Sort(array, start, end - start);
    }

    public static void Sort(ulong[] array, int start, int end) {
      System.Array.Sort(array, start, end - start);
    }

    public static void Sort(Obj[] array, int start, int end) {
      System.Array.Sort(array, start, end - start, objComparer);
    }

    private class ObjComparer : System.Collections.Generic.IComparer<Obj> {
      public int Compare(Obj l, Obj r) {
        return l.QuickOrder(r);
      }
    }

    private static ObjComparer objComparer = new ObjComparer();

    // private static int Order(Obj l, Obj r) {
    //   return l.QuickOrder(r);
    // }

    public static void CanonicalSort(Obj[] array) {
      System.Array.Sort(array, Canonical.Order);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static bool SortedArrayContains(int[] array, int value) {
      return System.Array.BinarySearch(array, 0, array.Length, value) >= 0;
    }

    public static bool SortedArrayContains(int[] array, int size, int value) {
      return System.Array.BinarySearch(array, 0, size, value) >= 0;
    }

    public static bool SortedArrayContains(long[] array, int size, long value) {
      return System.Array.BinarySearch(array, 0, size, value) >= 0;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static int AnyIndexOrEncodedInsertionPointIntoSortedArray(int[] array, int value) {
      return System.Array.BinarySearch(array, value);
    }

    public static int AnyIndexOrEncodedInsertionPointIntoSortedArray(int[] array, int start, int end, int value) {
      return System.Array.BinarySearch(array, start, end - start, value);
    }

    public static int AnyIndexOrEncodedInsertionPointIntoSortedArray(uint[] array, uint value) {
      return System.Array.BinarySearch(array, value);
    }

    public static int AnyIndexOrEncodedInsertionPointIntoSortedArray(uint[] array, int start, int end, uint value) {
      return System.Array.BinarySearch(array, start, end - start, value);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static bool IsPrefix(long[] prefixArray, long[] array) {
      for (int i=0 ; i < prefixArray.Length ; i++)
        if (prefixArray[i] != array[i])
          return false;
      return true;
    }

    public static bool IsPrefix(ulong[] prefixArray, ulong[] array) {
      for (int i=0 ; i < prefixArray.Length ; i++)
        if (prefixArray[i] != array[i])
          return false;
      return true;
    }
  }
}
