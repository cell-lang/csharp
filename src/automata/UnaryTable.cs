namespace Cell.Runtime {
  public class UnaryTable {
    public class Iter {
      int index;
      UnaryTable table;

      public Iter(int index, UnaryTable table) {
        this.table = table;
        if (table.count == 0)
          this.index = 64 * table.bitmap.Length;
        else {
          this.index = index;
          if (!table.Contains(0))
            Next();
        }
      }

      public int Get() {
        return index;
      }

      public bool Done() {
        return index >= 64 * table.bitmap.Length;
      }

      public void Next() {
        int size = 64 * table.bitmap.Length;
        do {
          index++;
        } while (index < size && !table.Contains(index));
      }
    }


    const int InitSize = 4;

    long[] bitmap = new long[InitSize];
    int count = 0;

    public SurrObjMapper mapper;

    public UnaryTable(SurrObjMapper mapper) {
      this.mapper = mapper;
    }

    public int Size() {
      return count;
    }

    public bool Contains(int surr) {
      int widx = surr / 64;
      return widx < bitmap.Length && Miscellanea.BitIsSet64(bitmap[widx], surr % 64);
    }

    public Iter GetIter() {
      return new Iter(0, this);
    }

    int LiveCount() {
      int liveCount = 0;
      for (int i=0 ; i < bitmap.Length ; i++) {
        long mask = bitmap[i];
        for (int j=0 ; j < 64 ; j++)
          if (Miscellanea.BitIsSet64(mask, j))
            liveCount++;
      }
      return liveCount;
    }

    public void Insert(int surr) {
      int widx = surr / 64;
      int bidx = (int) (surr % 64);

      int len = bitmap.Length;
      if (widx >= len) {
        int newLen = 2 * len;
        while (widx >= newLen)
          newLen *= 2;
        long[] newBitmap = new long[newLen];
        Array.Copy(bitmap, newBitmap, len);
        bitmap = newBitmap;
      }

      long mask = bitmap[widx];
      if (!Miscellanea.BitIsSet64(mask, bidx)) {
        bitmap[widx] = mask | (1L << bidx);
        count++;
      }
      // Debug.Assert(count == LiveCount());
    }

    public void Delete(int surr) {
      Debug.Assert(surr < 64 * bitmap.Length);

      int widx = surr / 64;
      if (widx < bitmap.Length) {
        long mask = bitmap[widx];
        int bidx = (int) surr % 64;
        if (Miscellanea.BitIsSet64(mask, bidx)) {
          bitmap[widx] = mask & ~(1L << bidx);
          count--;
        }
      }
      // Debug.Assert(count == LiveCount());
    }

    public long[] Clear(int minCapacity) {
      count = 0;
      int size = InitSize;
      while (64 * size < minCapacity)
        size *= 2;
      long[] bitmapCopy = bitmap;
      bitmap = new long[size];
      return bitmapCopy;
    }

    public Obj Copy() {
      return Copy(new UnaryTable[] {this});
    }

  //    public static string IntToBinaryString(int number) {
  //      string binStr = "";
  //      while (number != 0) {
  //        binStr = (number & 1) + binStr;
  //        number = number >>> 1;
  //      }
  //      if (binStr == "")
  //        binStr = "0";
  //      return binStr;
  //    }
  //
  //    public static string IntToBinaryString(long number) {
  //      string binStr = "";
  //      while (number > 0) {
  //        binStr = (number & 1) + binStr;
  //        number = number >>> 1;
  //      }
  //      if (binStr == "")
  //        binStr = "0";
  //      return binStr;
  //    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static Obj Copy(UnaryTable[] tables) {
      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].count;
      if (count == 0)
        return EmptyRelObj.singleton;
      Obj[] objs = new Obj[count];
      int next = 0;
      for (int i=0 ; i < tables.Length ; i++) {
        UnaryTable table = tables[i];
        SurrObjMapper mapper = table.mapper;
        long[] bitmap = table.bitmap;
        for (int j=0 ; j < bitmap.Length ; j++) {
          long mask = bitmap[j];
          for (int k=0 ; k < 64 ; k++)
            if (Miscellanea.BitIsSet64(mask, k))
              objs[next++] = mapper(k + 64 * j);
        }
      }
      Debug.Assert(next == count);
      return Builder.CreateSet(objs, objs.Length);
    }
  }
}
