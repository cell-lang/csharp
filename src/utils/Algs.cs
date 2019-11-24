using System;


namespace Cell.Runtime {
  public class Algs {
    static void CheckIsOrdered(Obj[] objs) {
      for (int i=1 ; i < objs.Length ; i++) {
        int ord = objs[i-1].QuickOrder(objs[i]);
        if (ord != -1) {
          throw new Exception();
        }
      }
    }

    public static int BinSearch(Obj[] objs, Obj obj) {
      return BinSearch(objs, 0, objs.Length, obj);
    }

    public static int BinSearch(Obj[] objs, int first, int count, Obj obj) {
      int low = first;
      int high = first + count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int res = obj.QuickOrder(objs[mid]);
        if (res == -1)
          high = mid - 1; // objs[mid] > obj
        else if (res == 1)
          low = mid + 1;  // objs[mid] < obj
        else
          return mid;
      }

      return -1;
    }

    // If the element exists, return its index
    // Otherwise, return the -(I + 1) where I is the index of
    // the first element that is greater than the searched one
    public static int BinSearchEx(Obj[] objs, int first, int count, Obj obj) {
      int idx = _binSearchEx(objs, first, count, obj);
      if (idx >= 0) {
        Debug.Assert(objs[idx].IsEq(obj));
      }
      else {
        int insIdx = -idx - 1;
        Debug.Assert(insIdx >= first & insIdx <= first + count);
        Debug.Assert(insIdx == first || objs[insIdx-1].QuickOrder(obj) < 0);
        Debug.Assert(insIdx == first + count || obj.QuickOrder(objs[insIdx]) <= 0);
      }
      return idx;
    }

    public static int _binSearchEx(Obj[] objs, int first, int count, Obj obj) {
      int low = first;
      int high = first + count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int res = obj.QuickOrder(objs[mid]);
        if (res == -1)
          high = mid - 1; // objs[mid] > obj
        else if (res == 1)
          low = mid + 1;  // objs[mid] < obj
        else
          return mid;
      }

      return -low - 1;
    }

    public static int[] BinSearchRange(Obj[] objs, int offset, int length, Obj obj) {
      int first;

      int low = offset;
      int high = offset + length - 1;
      int lowerBound = low;
      int upperBound = high;


      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int ord = obj.QuickOrder(objs[mid]);
        if (ord == -1) {
          upperBound = high = mid - 1; // objs[mid] > obj
        }
        else if (ord == 1) {
          lowerBound = low = mid + 1; // objs[mid] < obj
        }
        else {
          if (mid == offset || !objs[mid-1].IsEq(obj)) {
            first = mid;
            low = lowerBound;
            high = upperBound;

            while (low <= high) {
              mid = (int) (((long) low + (long) high) / 2);
              ord = obj.QuickOrder(objs[mid]);
              if (ord == -1) {
                high = mid - 1; // objs[mid] > obj
              }
              else if (ord == 1) {
                low = mid + 1; // objs[mid] < obj
              }
              else {
                if (mid == upperBound || !objs[mid+1].IsEq(obj))
                  return new int[] {first, mid - first + 1};
                else
                  low = mid + 1;
              }
            }

            // We're not supposed to ever get here.
            throw ErrorHandler.InternalFail();
          }
          else
            high = mid - 1;
        }
      }

      return new int[] {0, 0};
    }


    public static int[] BinSearchRange(int[] idxs, Obj[] objs, Obj obj) {
      Debug.Assert(idxs.Length == objs.Length);

      int offset = 0;
      int length = idxs.Length;

      int low = offset;
      int high = offset + length - 1;
      int lowerBound = low;
      int upperBound = high;


      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int ord = obj.QuickOrder(objs[idxs[mid]]);
        if (ord == -1) {
          upperBound = high = mid - 1; // objs[idxs[mid]] > obj
        }
        else if (ord == 1) {
          lowerBound = low = mid + 1; // objs[idxs[mid]] < obj
        }
        else {
          if (mid == offset || !objs[idxs[mid-1]].IsEq(obj)) {
            int first = mid;
            low = lowerBound;
            high = upperBound;

            while (low <= high) {
              mid = (int) (((long) low + (long) high) / 2);
              ord = obj.QuickOrder(objs[idxs[mid]]);
              if (ord == -1) {
                high = mid - 1; // objs[idxs[mid]] > obj
              }
              else if (ord == 1) {
                low = mid + 1; // objs[idxs[mid]] < obj
              }
              else {
                if (mid == upperBound || !objs[idxs[mid+1]].IsEq(obj))
                  return new int[] {first, mid - first + 1};
                else
                  low = mid + 1;
              }
            }

            // We're not supposed to ever get here.
            throw ErrorHandler.InternalFail();
          }
          else
            high = mid - 1;
        }
      }

      return new int[] {0, 0};
    }


    public static int[] BinSearchRange(Obj[] major, Obj[] minor, Obj majorVal, Obj minorVal) {
      int offset = 0;
      int length = major.Length;

      int low = offset;
      int high = offset + length - 1;
      int lowerBound = low;
      int upperBound = high;


      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int ord = majorVal.QuickOrder(major[mid]);
        if (ord == 0)
          ord = minorVal.QuickOrder(minor[mid]);
        if (ord == -1) {
          // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
          upperBound = high = mid - 1;
        }
        else if (ord == 1) {
          // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
          lowerBound = low = mid + 1;
        }
        else {
          if (mid == offset || (!major[mid-1].IsEq(majorVal) || !minor[mid-1].IsEq(minorVal))) {
            int first = mid;
            low = lowerBound;
            high = upperBound;

            while (low <= high) {
              mid = (int) (((long) low + (long) high) / 2);

              ord = majorVal.QuickOrder(major[mid]);
              if (ord == 0)
                ord = minorVal.QuickOrder(minor[mid]);

              if (ord == -1) {
                // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
                high = mid - 1;
              }
              else if (ord == 1) {
                // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
                low = mid + 1;
              }
              else {
                if (mid == upperBound || (!major[mid+1].IsEq(majorVal) || !minor[mid+1].IsEq(minorVal)))
                  return new int[] {first, mid - first + 1};
                else
                  low = mid + 1;
              }
            }

            // We're not supposed to ever get here.
            throw ErrorHandler.InternalFail();
          }
          else
            high = mid - 1;
        }
      }

      return new int[] {0, 0};
    }


    public static int[] BinSearchRange(int[] idxs, Obj[] major, Obj[] minor, Obj majorVal, Obj minorVal) {
      int offset = 0;
      int length = major.Length;

      int low = offset;
      int high = offset + length - 1;
      int lowerBound = low;
      int upperBound = high;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int midIdx = idxs[mid];

        int ord = majorVal.QuickOrder(major[midIdx]);
        if (ord == 0)
          ord = minorVal.QuickOrder(minor[midIdx]);

        if (ord == -1) {
          // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
          upperBound = high = mid - 1;
        }
        else if (ord == 1) {
          // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
          lowerBound = low = mid + 1;
        }
        else {
          bool isFirst = mid == offset;
          if (!isFirst) {
            int prevIdx = idxs[mid-1];
            isFirst = !major[prevIdx].IsEq(majorVal) || !minor[prevIdx].IsEq(minorVal);
          }

          if (isFirst) {
            int first = mid;
            low = lowerBound;
            high = upperBound;

            while (low <= high) {
              mid = (int) (((long) low + (long) high) / 2);
              midIdx = idxs[mid];

              ord = majorVal.QuickOrder(major[midIdx]);
              if (ord == 0)
                ord = minorVal.QuickOrder(minor[midIdx]);

              if (ord == -1) {
                // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
                high = mid - 1;
              }
              else if (ord == 1) {
                // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
                low = mid + 1;
              }
              else {
                bool isLast = mid == upperBound;
                if (!isLast) {
                  int nextIdx = idxs[mid+1];
                  isLast = !major[nextIdx].IsEq(majorVal) || !minor[nextIdx].IsEq(minorVal);
                }
                if (isLast) {
                  return new int[] {first, mid - first + 1};
                }
                else
                  low = mid + 1;
              }
            }

            // We're not supposed to ever get here.
            throw ErrorHandler.InternalFail();
          }
          else
            high = mid - 1;
        }
      }

      return new int[] {0, 0};
    }

    // public static Obj[] SortUnique(Obj[] objs, int count) {
    //   Debug.Assert(count > 0);

    //   int extraData0 = objs[0].extraData;
    //   for (int i=1 ; i < count ; i++)
    //     if (objs[i].extraData != extraData0)
    //       return _sortUnique(objs, count);


    //   int[] keysIdxs = new int[3*count];
    //   for (int i=0 ; i < count ; i++) {
    //     long data = objs[i].data;
    //     int idx = 3 * i;
    //     keysIdxs[idx]   = (int) (data >>> 32);
    //     keysIdxs[idx+1] = (int) data;
    //     keysIdxs[idx+2] = i;
    //   }
    //   Ints123.Sort(keysIdxs, count);

    //   Obj[] objs2 = new Obj[count];
    //   int groupKey1 = keysIdxs[0];
    //   int groupKey2 = keysIdxs[1];
    //   int groupStartIdx = 0;
    //   int nextIdx = 0;
    //   for (int i=0 ; i < count ; i++) {
    //     int i3 = 3 * i;
    //     int key1 = keysIdxs[i3];
    //     int key2 = keysIdxs[i3+1];
    //     int idx  = keysIdxs[i3+2];

    //     if (key1 != groupKey1 | key2 != groupKey2) {
    //       if (nextIdx - groupStartIdx > 1)
    //         nextIdx = SortUnique(objs2, groupStartIdx, nextIdx);
    //       groupKey1 = key1;
    //       groupKey2 = key2;
    //       groupStartIdx = nextIdx;
    //     }

    //     objs2[nextIdx++] = objs[idx];
    //   }
    //   if (nextIdx - groupStartIdx > 1)
    //     nextIdx = SortUnique(objs2, groupStartIdx, nextIdx);

    //   if (nextIdx == count)
    //     return objs2;
    //   else
    //     return Arrays.CopyOf(objs2, nextIdx);
    // }

    public static object[] SortUnique(Obj[] objs, int count) {
      Debug.Assert(count > 0);

      ulong[] keysIdxs = IndexesSortedByHashcode(objs, count);

      Obj[] sortedObjs = new Obj[count];
      uint[] hashcodes = new uint[count];
      uint groupKey = (uint) (keysIdxs[0] >> 32);
      int groupStartIdx = 0;
      int nextIdx = 0;
      for (int i=0 ; i < count ; i++) {
        ulong keyIdx = keysIdxs[i];
        uint key = (uint) (keyIdx >> 32);
        uint idx = (uint) keyIdx;

        if (key != groupKey) {
          if (nextIdx - groupStartIdx > 1)
            nextIdx = SortUnique(sortedObjs, groupStartIdx, nextIdx);
          for (int j=groupStartIdx ; j < nextIdx ; j++)
            hashcodes[j] = groupKey;
          groupKey = key;
          groupStartIdx = nextIdx;
        }

        sortedObjs[nextIdx++] = objs[idx];
      }
      if (nextIdx - groupStartIdx > 1)
        nextIdx = SortUnique(sortedObjs, groupStartIdx, nextIdx);
      for (int j=groupStartIdx ; j < nextIdx ; j++)
        hashcodes[j] = groupKey;

      if (nextIdx == count)
        return new object[] {sortedObjs, hashcodes};
      else
        return new object[] {Array.Take(sortedObjs, nextIdx), Array.Take(hashcodes, nextIdx)};
    }

    private static ulong[] IndexesSortedByHashcode(Obj[] objs, int count) {
      ulong[] keysIdxs = new ulong[count];
      for (uint i=0 ; i < count ; i++) {
        keysIdxs[i] = (((ulong) objs[i].Hashcode()) << 32) | i;
      }
      Array.Sort(keysIdxs);
      return keysIdxs;
    }

    private static int SortUnique(Obj[] objs, int first, int end) {
      Array.Sort(objs, first, end);
      int prev = first;
      for (int i=first+1 ; i < end ; i++)
        if (!objs[prev].IsEq(objs[i]))
          if (i != ++prev)
            objs[prev] = objs[i];
      return prev + 1;
    }

    // public static Obj[] SortUnique(Obj[] objs, int count) {
    //   Debug.Assert(count > 0);
    //   Arrays.Sort(objs, 0, count);
    //   int prev = 0;
    //   for (int i=1 ; i < count ; i++)
    //     if (!objs[prev].IsEq(objs[i]))
    //       if (i != ++prev)
    //         objs[prev] = objs[i];
    //   int len = prev + 1;
    //   return Arrays.CopyOf(objs, len);
    // }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    // public static Obj[][] SortUnique(Obj[] col1, Obj[] col2, int count) {
    //   Debug.Assert(count > 0);

    //   int[] idxs = new int[count];
    //   for (int i=0 ; i < count ; i++)
    //     idxs[i] = i;

    //   SortIdxs(idxs, 0, count-1, col1, col2);

    //   int prev = 0;
    //   for (int i=1 ; i < count ; i++) {
    //     int j = idxs[i];
    //     int k = idxs[i-1];
    //     if (!col1[j].IsEq(col1[k]) || !col2[j].IsEq(col2[k]))
    //       if (i != ++prev)
    //         idxs[prev] = idxs[i];
    //   }

    //   int size = prev + 1;
    //   Obj[] normCol1 = new Obj[size];
    //   Obj[] normCol2 = new Obj[size];

    //   for (int i=0 ; i < size ; i++) {
    //     int j = idxs[i];
    //     normCol1[i] = col1[j];
    //     normCol2[i] = col2[j];
    //   }

    //   return new Obj[][] {normCol1, normCol2};
    // }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static Obj[][] SortUnique(Obj[] col1, Obj[] col2, Obj[] col3, int count) {
      Debug.Assert(count > 0);

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      SortIdxs(idxs, 0, count-1, col1, col2, col3);

      int prev = 0;
      for (int i=1 ; i < count ; i++) {
        int j = idxs[i];
        int k = idxs[i-1];
        if (!col1[j].IsEq(col1[k]) || !col2[j].IsEq(col2[k]) || !col3[j].IsEq(col3[k]))
          if (i != ++prev)
            idxs[prev] = idxs[i];
      }

      int size = prev + 1;
      Obj[] normCol1 = new Obj[size];
      Obj[] normCol2 = new Obj[size];
      Obj[] normCol3 = new Obj[size];

      for (int i=0 ; i < size ; i++) {
        int j = idxs[i];
        normCol1[i] = col1[j];
        normCol2[i] = col2[j];
        normCol3[i] = col3[j];
      }

      return new Obj[][] {normCol1, normCol2, normCol3};
    }

    public static bool SortedArrayHasDuplicates(Obj[] objs) {
      for (int i=1 ; i < objs.Length ; i++)
        if (objs[i].IsEq(objs[i-1]))
          return true;
      return false;
    }

    public static int[] SortedIndexes(Obj[] major, Obj[] minor) {
      Debug.Assert(major.Length == minor.Length);

      int count = major.Length;

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      SortIdxs(idxs, 0, count-1, major, minor);

      return idxs;
    }

    public static int[] SortedIndexes(Obj[] col1, Obj[] col2, Obj[] col3) {
      Debug.Assert(col1.Length == col2.Length && col1.Length == col3.Length);

      int count = col1.Length;

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      SortIdxs(idxs, 0, count-1, col1, col2, col3);

      return idxs;
    }

    //////////////////////////////////////////////////////////////////////////////

    static void SortIdxs(int[] indexes, int first, int last, Obj[] major, Obj[] minor) {
      if (first >= last)
        return;

      int pivotIdx = first + (last - first) / 2;
      int pivot = indexes[pivotIdx];
      Obj pivotMajor = major[pivot];
      Obj pivotMinor = minor[pivot];

      if (pivotIdx > first)
        indexes[pivotIdx] = indexes[first];
        // indexes[first] = pivot; // Not necessary

      int low = first + 1;
      int high = last;

      while (low <= high) {
        while (low <= last) {
          int idx = indexes[low];
          int ord = major[idx].QuickOrder(pivotMajor);
          if (ord == 0)
            ord = minor[idx].QuickOrder(pivotMinor);

          if (ord > 0) // Including all elements that are lower or equal than the pivot
            break;
          else
            low++;
        }

        // <low> is now the lowest index that does not contain a value that is
        // lower or equal than the pivot. It may be outside the bounds of the array

        while (high >= first) {
          int idx = indexes[high];
          int ord = major[idx].QuickOrder(pivotMajor);
          if (ord == 0)
            ord = minor[idx].QuickOrder(pivotMinor);

          if (ord <= 0) // Including only elements that are greater than the pivot
            break;
          else
            high--;
        }

        // <high> is not the highest index that does not contain an element that
        // is greater than the pivot. It may be outside the bounds of the array

        Debug.Assert(low != high);

        if (low < high) {
          int tmp = indexes[low];
          indexes[low] = indexes[high];
          indexes[high] = tmp;
          low++;
          high--;
        }
      }

      if (low - 1 > first)
        indexes[first] = indexes[low - 1];
      indexes[low - 1] = pivot;

      if (low - 2 > first) {
        SortIdxs(indexes, first, low-2, major, minor);
      }

      if (high < last)
        SortIdxs(indexes, high+1, last, major, minor);
    }

    static void SortIdxs(int[] indexes, int first, int last, Obj[] ord1, Obj[] ord2, Obj[] ord3) {
      if (first >= last)
        return;

      int pivotIdx = first + (last - first) / 2;
      int pivot = indexes[pivotIdx];
      Obj pivotOrd1 = ord1[pivot];
      Obj pivotOrd2 = ord2[pivot];
      Obj pivotOrd3 = ord3[pivot];

      if (pivotIdx > first)
        indexes[pivotIdx] = indexes[first];
        // indexes[first] = pivot; // Not necessary

      int low = first + 1;
      int high = last;

      while (low <= high) {
        while (low <= last) {
          int idx = indexes[low];
          int ord = ord1[idx].QuickOrder(pivotOrd1);
          if (ord == 0)
            ord = ord2[idx].QuickOrder(pivotOrd2);
          if (ord == 0)
            ord = ord3[idx].QuickOrder(pivotOrd3);

          if (ord > 0) // Including all elements that are lower or equal than the pivot
            break;
          else
            low++;
        }

        // <low> is now the lowest index that does not contain a value that is
        // lower or equal than the pivot. It may be outside the bounds of the array

        while (high >= first) {
          int idx = indexes[high];
          int ord = ord1[idx].QuickOrder(pivotOrd1);
          if (ord == 0)
            ord = ord2[idx].QuickOrder(pivotOrd2);
          if (ord == 0)
            ord = ord3[idx].QuickOrder(pivotOrd3);

          if (ord <= 0) // Including only elements that are greater than the pivot
            break;
          else
            high--;
        }

        // <high> is not the highest index that does not contain an element that
        // is greater than the pivot. It may be outside the bounds of the array

        Debug.Assert(low != high);

        if (low < high) {
          int tmp = indexes[low];
          indexes[low] = indexes[high];
          indexes[high] = tmp;
          low++;
          high--;
        }
      }

      if (low - 1 > first)
        indexes[first] = indexes[low - 1];
      indexes[low - 1] = pivot;

      if (low - 2 > first) {
        SortIdxs(indexes, first, low-2, ord1, ord2, ord3);
      }

      if (high < last)
        SortIdxs(indexes, high+1, last, ord1, ord2, ord3);
    }
  }
}
