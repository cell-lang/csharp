namespace Cell.Runtime {
  public class TernaryTable {
    public const int Empty = -1;

    const int MinSize = 32;

    int[] flatTuples = new int[3 * MinSize];
    public int count = 0;
    int firstFree = 0;

    public Index index123, index12, index13, index23, index1, index2, index3;

    public SurrObjMapper mapper1, mapper2, mapper3;

    //////////////////////////////////////////////////////////////////////////////

    int Field1OrNext(int idx) {
      return flatTuples[3 * idx];
    }

    int Field2OrEmptyMarker(int idx) {
      return flatTuples[3 * idx + 1];
    }

    int Field3(int idx) {
      return flatTuples[3 * idx + 2];
    }

    void SetEntry(int idx, int field1, int field2, int field3) {
      int offset = 3 * idx;
      flatTuples[offset]   = field1;
      flatTuples[offset+1] = field2;
      flatTuples[offset+2] = field3;

      Debug.Assert(Field1OrNext(idx) == field1);
      Debug.Assert(Field2OrEmptyMarker(idx) == field2);
      Debug.Assert(Field3(idx) == field3);
    }

    int Capacity() {
      return flatTuples.Length / 3;
    }

    void Resize() {
      int len = flatTuples.Length;
      Debug.Assert(3 * count == len);
      int[] newFlatTuples = new int[2 * len];
      Array.Copy(flatTuples, newFlatTuples, len);
      flatTuples = newFlatTuples;
      int size = len / 3;
      for (int i=size ; i < 2 * size ; i++)
        SetEntry(i, i+1, Empty, 0);
    }

    //////////////////////////////////////////////////////////////////////////////

    public TernaryTable(SurrObjMapper mapper1, SurrObjMapper mapper2, SurrObjMapper mapper3) {
      for (int i=0 ; i < MinSize ; i++)
        SetEntry(i, i+1, Empty, 0);

      index123 = new Index(MinSize);
      index12  = new Index(MinSize);

      this.mapper1 = mapper1;
      this.mapper2 = mapper2;
      this.mapper3 = mapper3;
    }

    public int Size() {
      return count;
    }

    public void Insert(int field1, int field2, int field3) {
      if (Contains(field1, field2, field3))
        return;

      // Increasing the size of the table if need be
      if (firstFree >= Capacity()) {
        Resize();
        index123 = null;
        index12 = null;
        index13 = null;
        index23 = null;
        index1 = null;
        index2 = null;
        index3 = null;
        BuildIndex123();
        BuildIndex12();
      }

      // Inserting the new tuple
      int index = firstFree;
      firstFree = Field1OrNext(firstFree);
      SetEntry(index, field1, field2, field3);
      count++;

      // Updating the indexes
      index123.Insert(index, Hashing.Hashcode(field1, field2, field3));
      index12.Insert(index, Hashing.Hashcode(field1, field2));
      if (index13 != null)
        index13.Insert(index, Hashing.Hashcode(field1, field3));
      if (index23 != null)
        index23.Insert(index, Hashing.Hashcode(field2, field3));
      if (index1 != null)
        index1.Insert(index, Hashing.Hashcode(field1));
      if (index2 != null)
        index2.Insert(index, Hashing.Hashcode(field2));
      if (index3 != null)
        index3.Insert(index, Hashing.Hashcode(field3));
    }

    public void Clear() {
      count = 0;
      firstFree = 0;

      int size = Capacity();
      for (int i=0 ; i < size ; i++)
        SetEntry(i, i+1, Empty, 0);

      index123.Clear();
      index12.Clear();
      index13.Clear();
      index23.Clear();
      index1.Clear();
      index2.Clear();
      index3.Clear();
    }

    public void Delete(int field1, int field2, int field3) {
      int hashcode = Hashing.Hashcode(field1, field2, field3);
      for (int idx = index123.Head(hashcode) ; idx != Empty ; idx = index123.Next(idx)) {
        if (Field1OrNext(idx) == field1 & Field2OrEmptyMarker(idx) == field2 & Field3(idx) == field3) {
          DeleteAt(idx, hashcode);
          return;
        }
      }
    }

    public int ContainsAt(int field1, int field2, int field3) {
      int hashcode = Hashing.Hashcode(field1, field2, field3);
      for (int idx = index123.Head(hashcode) ; idx != Empty ; idx = index123.Next(idx)) {
        if (Field1OrNext(idx) == field1 & Field2OrEmptyMarker(idx) == field2 & Field3(idx) == field3)
          return idx;
      }
      return -1;
    }

    public bool Contains(int field1, int field2, int field3) {
      int hashcode = Hashing.Hashcode(field1, field2, field3);
      for (int idx = index123.Head(hashcode) ; idx != Empty ; idx = index123.Next(idx)) {
        if (Field1OrNext(idx) == field1 & Field2OrEmptyMarker(idx) == field2 & Field3(idx) == field3)
          return true;
      }
      return false;
    }

    public bool Contains12(int field1, int field2) {
      int hashcode = Hashing.Hashcode(field1, field2);
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx)) {
        if (Field1OrNext(idx) == field1 & Field2OrEmptyMarker(idx) == field2)
          return true;
      }
      return false;
    }

    public bool Contains13(int field1, int field3) {
      if (index13 == null)
        BuildIndex13();
      int hashcode = Hashing.Hashcode(field1, field3);
      for (int idx = index13.Head(hashcode) ; idx != Empty ; idx = index13.Next(idx)) {
        if (Field1OrNext(idx) == field1 & Field3(idx) == field3)
          return true;
      }
      return false;
    }

    public bool Contains23(int field2, int field3) {
      if (index23 == null)
        BuildIndex23();
      int hashcode = Hashing.Hashcode(field2, field3);
      for (int idx = index23.Head(hashcode) ; idx != Empty ; idx = index23.Next(idx)) {
        if (Field2OrEmptyMarker(idx) == field2 & Field3(idx) == field3)
          return true;
      }
      return false;
    }

    public bool Contains1(int field1) {
      if (index1 == null)
        BuildIndex1();
      int hashcode = Hashing.Hashcode(field1);
      for (int idx = index1.Head(hashcode) ; idx != Empty ; idx = index1.Next(idx)) {
        if (Field1OrNext(idx) == field1)
          return true;
      }
      return false;
    }

    public bool Contains2(int field2) {
      if (index2 == null)
        BuildIndex2();
      int hashcode = Hashing.Hashcode(field2);
      for (int idx = index2.Head(hashcode) ; idx != Empty ; idx = index2.Next(idx)) {
        if (Field2OrEmptyMarker(idx) == field2)
          return true;
      }
      return false;
    }

    public bool Contains3(int field3) {
      if (index3 == null)
        BuildIndex3();
      int hashcode = Hashing.Hashcode(field3);
      for (int idx = index3.Head(hashcode) ; idx != Empty ; idx = index3.Next(idx)) {
        if (Field3(idx) == field3)
          return true;
      }
      return false;
    }


    public int Lookup12(int arg1, int arg2) {
      int hashcode = Hashing.Hashcode(arg1, arg2);
      int value = -1;
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx)) {
        if (Field1OrNext(idx) == arg1 && Field2OrEmptyMarker(idx) == arg2)
          if (value == -1)
            value = Field3(idx);
          else
            throw ErrorHandler.SoftFail();
      }
      return value;
    }

    public int Lookup13(int arg1, int arg3) {
      if (index13 == null)
        BuildIndex13();
      int hashcode = Hashing.Hashcode(arg1, arg3);
      int value = -1;
      for (int idx = index13.Head(hashcode) ; idx != Empty ; idx = index13.Next(idx))
        if (Field1OrNext(idx) == arg1 && Field3(idx) == arg3)
          if (value == -1)
            value = Field2OrEmptyMarker(idx);
          else
            throw ErrorHandler.SoftFail();
      return value;
    }

    public int Lookup23(int arg2, int arg3) {
      if (index23 == null)
        BuildIndex23();
      int hashcode = Hashing.Hashcode(arg2, arg3);
      int value = -1;
      for (int idx = index23.Head(hashcode) ; idx != Empty ; idx = index23.Next(idx))
        if (Field2OrEmptyMarker(idx) == arg2 && Field3(idx) == arg3)
          if (value == -1)
            value = Field1OrNext(idx);
          else
            throw ErrorHandler.SoftFail();
      return value;
    }

    public int Count12(int arg1, int arg2) {
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1, arg2);
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx))
        if (Field1OrNext(idx) == arg1 & Field2OrEmptyMarker(idx) == arg2)
          count++;
      return count;
    }

    public int Count13(int arg1, int arg3) {
      if (index13 == null)
        BuildIndex13();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1, arg3);
      for (int idx = index13.Head(hashcode) ; idx != Empty ; idx = index13.Next(idx))
        if (Field1OrNext(idx) == arg1 & Field3(idx) == arg3)
          count++;
      return count;
    }

    public int Count23(int arg2, int arg3) {
      if (index23 == null)
        BuildIndex23();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg2, arg3);
      for (int idx = index23.Head(hashcode) ; idx != Empty ; idx = index23.Next(idx))
        if (Field2OrEmptyMarker(idx) == arg2 & Field3(idx) == arg3)
          count++;
      return count;
    }

    public int Count1(int arg1) {
      if (index1 == null)
        BuildIndex1();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1);
      for (int idx = index1.Head(hashcode) ; idx != Empty ; idx = index1.Next(idx))
        if (Field1OrNext(idx) == arg1)
          count++;
      return count;
    }

    public int Count2(int arg2) {
      if (index2 == null)
        BuildIndex2();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg2);
      for (int idx = index2.Head(hashcode) ; idx != Empty ; idx = index2.Next(idx))
        if (Field2OrEmptyMarker(idx) == arg2)
          count++;
      return count;
    }

    public int Count3(int arg3) {
      if (index3 == null)
        BuildIndex3();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg3);
      for (int idx = index3.Head(hashcode) ; idx != Empty ; idx = index3.Next(idx))
        if (Field3(idx) == arg3)
          count++;
      return count;
    }

    public bool Count12Eq(int arg1, int arg2, int expCount) {
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1, arg2);
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx))
        if (Field1OrNext(idx) == arg1 & Field2OrEmptyMarker(idx) == arg2) {
          count++;
          if (count > expCount)
            return false;
        }
      return count == expCount;
    }

    public bool Count1Eq(int arg1, int expCount) {
      if (index1 == null)
        BuildIndex1();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1);
      for (int idx = index1.Head(hashcode) ; idx != Empty ; idx = index1.Next(idx))
        if (Field1OrNext(idx) == arg1) {
          count++;
          if (count > expCount)
            return false;
        }
      return count == expCount;
    }

    public bool Count2Eq(int arg2, int expCount) {
      if (index2 == null)
        BuildIndex2();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg2);
      for (int idx = index2.Head(hashcode) ; idx != Empty ; idx = index2.Next(idx))
        if (Field2OrEmptyMarker(idx) == arg2) {
          count++;
          if (count > expCount)
            return false;
        }
      return count == expCount;
    }

    public bool Count3Eq(int arg3, int expCount) {
      if (index3 == null)
        BuildIndex3();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg3);
      for (int idx = index3.Head(hashcode) ; idx != Empty ; idx = index3.Next(idx))
        if (Field3(idx) == arg3) {
          count++;
          if (count > expCount)
            return false;
        }
      return count == expCount;
    }

    public Iter123 GetIter() {
      return new Iter123(this);
    }

    public Iter12 GetIter12(int field1, int field2) {
      int hashcode = Hashing.Hashcode(field1, field2);
      return new Iter12(this, field1, field2, index12.Head(hashcode));
    }

    public Iter13 GetIter13(int field1, int field3) {
      if (index13 == null)
        BuildIndex13();
      int hashcode = Hashing.Hashcode(field1, field3);
      return new Iter13(this, field1, field3, index13.Head(hashcode));
    }

    public Iter23 GetIter23(int field2, int field3) {
      if (index23 == null)
        BuildIndex23();
      int hashcode = Hashing.Hashcode(field2, field3);
      return new Iter23(this, field2, field3, index23.Head(hashcode));
    }

    public Iter1 GetIter1(int field1) {
      if (index1 == null)
        BuildIndex1();
      int hashcode = Hashing.Hashcode(field1);
      return new Iter1(this, field1, index1.Head(hashcode));
    }

    public Iter2 GetIter2(int field2) {
      if (index2 == null)
        BuildIndex2();
      int hashcode = Hashing.Hashcode(field2);
      return new Iter2(this, field2, index2.Head(hashcode));
    }

    public Iter3 GetIter3(int field3) {
      if (index3 == null)
        BuildIndex3();
      int hashcode = Hashing.Hashcode(field3);
      return new Iter3(this, field3, index3.Head(hashcode));
    }

    public Obj Copy(int idx1, int idx2, int idx3) {
      return Copy(new TernaryTable[] {this}, idx1, idx2, idx3);
    }

    ////////////////////////////////////////////////////////////////////////////

    public bool Col3IsKey() {
      if (index3 == null)
        BuildIndex3();

      int[] hashtable = index3.hashtable;
      int[] bucket = new int[32];

      for (int i=0 ; i < hashtable.Length ; i++) {
        int count = 0;
        int idx = hashtable[i];
        while (idx != Empty) {
          bucket = Array.Append(bucket, count++, flatTuples[3 * idx + 2]);
          idx = index3.Next(idx);
        }

        if (count > 1) {
          if (count > 2)
            Array.Sort(bucket, count);
          int last = bucket[0];
          for (int j=1 ; j < count ; j++) {
            int val = bucket[j];
            if (val == last)
              return false;
            last = val;
          }
        }
      }

      return true;
    }

    public bool Cols12AreKey() {
      return ColsAreKey(index12, 0, 1);
    }

    public bool Cols13AreKey() {
      if (index13 == null)
        BuildIndex13();
      return ColsAreKey(index13, 0, 2);
    }

    public bool Cols23AreKey() {
      if (index23 == null)
        BuildIndex23();
      return ColsAreKey(index23, 1, 2);
    }

    ////////////////////////////////////////////////////////////////////////////

    bool ColsAreKey(Index index, int col1, int col2) {
      int[] hashtable = index.hashtable;
      long[] bucket = new long[32];

      for (int i=0 ; i < hashtable.Length ; i++) {
        int count = 0;

        int idx = hashtable[i];
        while (idx != Empty) {
          int offset = 3 * idx;
          long arg1 = flatTuples[offset + col1];
          long arg2 = flatTuples[offset + col2];
          long packedArgs = arg1 | (arg2 << 32);
          Debug.Assert(arg1 == (packedArgs & 0xFFFFFFFFL));
          Debug.Assert(arg2 == Miscellanea.UnsignedLeftShift64(packedArgs, 32));
          bucket = Array.Append(bucket, count++, packedArgs);
          idx = index.Next(idx);
        }

        if (count > 1) {
          if (count > 2)
            Array.Sort(bucket, count);
          long last = bucket[0];
          for (int j=1 ; j < count ; j++) {
            long val = bucket[j];
            if (val == last)
              return false;
            last = val;
          }
        }
      }

      return true;
    }

    public bool DeleteAt(int index) {
      int field2 = Field2OrEmptyMarker(index);
      if (field2 == Empty)
        return false;

      int field1 = Field1OrNext(index);
      int field3 = Field3(index);

      // Removing the tuple
      SetEntry(index, firstFree, Empty, 0);
      firstFree = index;
      count--;

      // Updating the indexes
      index123.Delete(index, Hashing.Hashcode(field1, field2, field3));
      index12.Delete(index, Hashing.Hashcode(field1, field2));
      if (index13 != null)
        index13.Delete(index, Hashing.Hashcode(field1, field3));
      if (index23 != null)
        index23.Delete(index, Hashing.Hashcode(field2, field3));
      if (index1 != null)
        index1.Delete(index, Hashing.Hashcode(field1));
      if (index2 != null)
        index2.Delete(index, Hashing.Hashcode(field2));
      if (index3 != null)
        index3.Delete(index, Hashing.Hashcode(field3));

      return true;
    }

    //////////////////////////////////////////////////////////////////////////////

    Index GetIndex123() {
      if (index123 == null)
        BuildIndex123();
      return index123;
    }

    Index GetIndex12() {
      if (index12 == null)
        BuildIndex12();
      return index12;
    }

    Index GetIndex13() {
      if (index13 == null)
        BuildIndex13();
      return index13;
    }

    Index GetIndex23() {
      if (index23 == null)
        BuildIndex23();
      return index23;
    }

    Index GetIndex1() {
      if (index1 == null)
        BuildIndex1();
      return index1;
    }

    Index GetIndex2() {
      if (index2 == null)
        BuildIndex2();
      return index2;
    }

    Index GetIndex3() {
      if (index3 == null)
        BuildIndex3();
      return index3;
    }

    //////////////////////////////////////////////////////////////////////////////

    void DeleteAt(int index, int hashcode) {
      int field1 = Field1OrNext(index);
      int field2 = Field2OrEmptyMarker(index);
      int field3 = Field3(index);
      Debug.Assert(field2 != Empty);

      // Removing the tuple
      SetEntry(index, firstFree, Empty, 0);
      firstFree = index;
      count--;

      // Updating the indexes
      index123.Delete(index, hashcode);
      index12.Delete(index, Hashing.Hashcode(field1, field2));
      if (index13 != null)
        index13.Delete(index, Hashing.Hashcode(field1, field3));
      if (index23 != null)
        index23.Delete(index, Hashing.Hashcode(field2, field3));
      if (index1 != null)
        index1.Delete(index, Hashing.Hashcode(field1));
      if (index2 != null)
        index2.Delete(index, Hashing.Hashcode(field2));
      if (index3 != null)
        index3.Delete(index, Hashing.Hashcode(field3));
    }

    void BuildIndex123() {
      int size = Capacity();
      index123 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index123.Insert(i, Hashing.Hashcode(Field1OrNext(i), field2, Field3(i)));
      }
    }

    void BuildIndex12() {
      int size = Capacity();
      index12 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index12.Insert(i, Hashing.Hashcode(Field1OrNext(i), field2));
      }
    }

    void BuildIndex13() {
      int size = Capacity();
      index13 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index13.Insert(i, Hashing.Hashcode(Field1OrNext(i), Field3(i)));
      }
    }

    void BuildIndex23() {
      int size = Capacity();
      index23 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index23.Insert(i, Hashing.Hashcode(field2, Field3(i)));
      }
    }

    void BuildIndex1() {
      int size = Capacity();
      index1 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index1.Insert(i, Hashing.Hashcode(Field1OrNext(i)));
      }
    }

    void BuildIndex2() {
      int size = Capacity();
      index2 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index2.Insert(i, Hashing.Hashcode(field2));
      }
    }

    void BuildIndex3() {
      int size = Capacity();
      index3 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int field2 = Field2OrEmptyMarker(i);
        if (field2 != Empty)
          index3.Insert(i, Hashing.Hashcode(Field3(i)));
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static Obj Copy(TernaryTable[] tables, int idx1, int idx2, int idx3) {
      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].count;

      if (count == 0)
        return EmptyRelObj.singleton;

      Obj[] objs1 = new Obj[count];
      Obj[] objs2 = new Obj[count];
      Obj[] objs3 = new Obj[count];

      int next = 0;
      for (int iT=0 ; iT < tables.Length ; iT++) {
        TernaryTable table = tables[iT];
        SurrObjMapper mapper1 = table.mapper1;
        SurrObjMapper mapper2 = table.mapper2;
        SurrObjMapper mapper3 = table.mapper3;
        int size = table.Capacity();
        for (int iS=0 ; iS < size ; iS++) {
          int field2 = table.Field2OrEmptyMarker(iS);
          if (field2 != Empty) {
            objs1[next] = mapper1(table.Field1OrNext(iS));
            objs2[next] = mapper2(field2);
            objs3[next] = mapper3(table.Field3(iS));
            next++;
          }
        }
      }
      Debug.Assert(next == count);

      Obj[][] cols = new Obj[3][];
      cols[idx1] = objs1;
      cols[idx2] = objs2;
      cols[idx3] = objs3;

      return Builder.CreateTernRel(cols[0], cols[1], cols[2], count);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public abstract class Iter {
      protected TernaryTable table;
      protected int index;

      protected Iter(TernaryTable table) {
        this.table = table;
      }

      public bool Done() {
        return index == Empty;
      }

      public int Index() {
        return index;
      }

      public virtual int Get1() {
        throw ErrorHandler.InternalFail();
      }

      public virtual int Get2() {
        throw ErrorHandler.InternalFail();
      }

      public virtual int Get3() {
        throw ErrorHandler.InternalFail();
      }

      public abstract void Next();
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter123 : Iter {
      public Iter123(TernaryTable table) : base(table) {
        if (table.count > 0) {
          index = 0;
          while (table.Field2OrEmptyMarker(index) == Empty)
            index++;
        }
        else
          index = Empty;
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field1OrNext(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Field2OrEmptyMarker(index);
      }

      public override int Get3() {
        Debug.Assert(index != Empty);
        return table.Field3(index);
      }

      public override void Next() {
        Debug.Assert(index != Empty);
        int size = table.Capacity();
        do {
          index++;
          if (index == size) {
            index = Empty;
            return;
          }
        } while (table.Field2OrEmptyMarker(index) == Empty);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter12 : Iter {
      int arg1;
      int arg2;

      public Iter12(TernaryTable table, int arg1, int arg2, int index) : base(table) {
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.index = index;
        if (index != Empty && !IsMatch())
          Next();
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field3(index);
      }

      public override void Next() {
        Debug.Assert(index != Empty);
        do {
          index = table.index12.Next(index);
        } while (index != Empty && !IsMatch());
      }


      private bool IsMatch() {
        return table.Field1OrNext(index) == arg1 && table.Field2OrEmptyMarker(index) == arg2;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter13 : Iter {
      int arg1;
      int arg3;

      public Iter13(TernaryTable table, int arg1, int arg3, int index) : base(table) {
        this.arg1 = arg1;
        this.arg3 = arg3;
        this.index = index;
        if (index != Empty && !IsMatch())
          Next();
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field2OrEmptyMarker(index);
      }

      public override void Next() {
        Debug.Assert(index != Empty);
        do {
          index = table.index13.Next(index);
        } while (index != Empty && !IsMatch());
      }

      private bool IsMatch() {
        return table.Field1OrNext(index) == arg1 && table.Field3(index) == arg3;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter23 : Iter {
      int arg2;
      int arg3;

      public Iter23(TernaryTable table, int arg2, int arg3, int index) : base(table) {
        this.arg2 = arg2;
        this.arg3 = arg3;
        this.index = index;
        if (index != Empty && !IsMatch())
          Next();
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field1OrNext(index);
      }

      public override void Next() {
        Debug.Assert(index != Empty);
        do {
          index = table.index23.Next(index);
        } while (index != Empty && !IsMatch());
      }

      private bool IsMatch() {
        return table.Field2OrEmptyMarker(index) == arg2 && table.Field3(index) == arg3;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter1 : Iter {
      int arg1;

      public Iter1(TernaryTable table, int arg1, int index) : base(table) {
        this.arg1 = arg1;
        this.index = index;
        if (index != Empty && !IsMatch())
          Next();
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field2OrEmptyMarker(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Field3(index);
      }

      public override void Next() {
        do {
          index = table.index1.Next(index);
        } while (index != Empty && table.Field1OrNext(index) != arg1);
      }

      private bool IsMatch() {
        return table.Field1OrNext(index) == arg1;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter2 : Iter {
      int arg2;

      public Iter2(TernaryTable table, int arg2, int index) : base(table) {
        this.arg2 = arg2;
        this.index = index;
        if (index != Empty && !IsMatch())
          Next();
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field1OrNext(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Field3(index);
      }

      public override void Next() {
        do {
          index = table.index2.Next(index);
        } while (index != Empty && table.Field2OrEmptyMarker(index) != arg2);
      }

      private bool IsMatch() {
        return table.Field2OrEmptyMarker(index) == arg2;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter3 : Iter {
      int arg3;

      public Iter3(TernaryTable table, int arg3, int index) : base(table) {
        this.arg3 = arg3;
        this.index = index;
        if (index != Empty && !IsMatch())
          Next();
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Field1OrNext(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Field2OrEmptyMarker(index);
      }

      public override void Next() {
        do {
          index = table.index3.Next(index);
        } while (index != Empty && table.Field3(index) != arg3);
      }

      private bool IsMatch() {
        return table.Field3(index) == arg3;
      }
    }
  }
}
