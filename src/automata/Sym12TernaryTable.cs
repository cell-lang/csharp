namespace Cell.Runtime {
  public class Sym12TernaryTable {
    public const int Empty = -1;

    const int MinSize = 32;

    int[] flatTuples = new int[3 * MinSize];
    public int count = 0;
    int firstFree = 0;

    public Index index123, index12, index13, index23, index1, index2, index3;

    public SurrObjMapper mapper12, mapper3;

    //////////////////////////////////////////////////////////////////////////////

    int Arg1OrNext(int idx) {
      return flatTuples[3 * idx];
    }

    int Arg2OrEmptyMarker(int idx) {
      return flatTuples[3 * idx + 1];
    }

    int Arg3(int idx) {
      return flatTuples[3 * idx + 2];
    }

    void SetEntry(int idx, int arg1, int arg2, int arg3) {
      int offset = 3 * idx;
      flatTuples[offset]   = arg1;
      flatTuples[offset+1] = arg2;
      flatTuples[offset+2] = arg3;

      Debug.Assert(Arg1OrNext(idx) == arg1);
      Debug.Assert(Arg2OrEmptyMarker(idx) == arg2);
      Debug.Assert(Arg3(idx) == arg3);
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

    public Sym12TernaryTable(SurrObjMapper mapper12, SurrObjMapper mapper3) {
      for (int i=0 ; i < MinSize ; i++)
        SetEntry(i, i+1, Empty, 0);

      index123 = new Index(MinSize);
      index12  = new Index(MinSize);

      this.mapper12 = mapper12;
      this.mapper3 = mapper3;
    }

    public int Size() {
      return count;
    }

    public void Insert(int arg1, int arg2, int arg3) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }

      if (Contains(arg1, arg2, arg3))
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
      firstFree = Arg1OrNext(firstFree);
      SetEntry(index, arg1, arg2, arg3);
      count++;

      // Updating the indexes
      index123.Insert(index, Hashing.Hashcode(arg1, arg2, arg3));
      index12.Insert(index, Hashing.Hashcode(arg1, arg2));
      if (index13 != null)
        index13.Insert(index, Hashing.Hashcode(arg1, arg3));
      if (index23 != null)
        index23.Insert(index, Hashing.Hashcode(arg2, arg3));
      if (index1 != null)
        index1.Insert(index, Hashing.Hashcode(arg1));
      if (index2 != null)
        index2.Insert(index, Hashing.Hashcode(arg2));
      if (index3 != null)
        index3.Insert(index, Hashing.Hashcode(arg3));
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

    public void Delete(int arg1, int arg2, int arg3) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      int hashcode = Hashing.Hashcode(arg1, arg2, arg3);
      for (int idx = index123.Head(hashcode) ; idx != Empty ; idx = index123.Next(idx))
        if (Arg1OrNext(idx) == arg1 & Arg2OrEmptyMarker(idx) == arg2 & Arg3(idx) == arg3) {
          DeleteAt(idx, hashcode);
          return;
        }
    }

    public bool Contains(int arg1, int arg2, int arg3) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      int hashcode = Hashing.Hashcode(arg1, arg2, arg3);
      for (int idx = index123.Head(hashcode) ; idx != Empty ; idx = index123.Next(idx))
        if (Arg1OrNext(idx) == arg1 & Arg2OrEmptyMarker(idx) == arg2 & Arg3(idx) == arg3)
          return true;
      return false;
    }

    public bool Contains12(int arg1, int arg2) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      int hashcode = Hashing.Hashcode(arg1, arg2);
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx))
        if (Arg1OrNext(idx) == arg1 & Arg2OrEmptyMarker(idx) == arg2)
          return true;
      return false;
    }

    public bool contains_13_23(int arg12, int arg3) {
      int hashcode = Hashing.Hashcode(arg12, arg3);
      if (index13 == null)
        BuildIndex13();
      for (int idx = index13.Head(hashcode) ; idx != Empty ; idx = index13.Next(idx))
        if ((Arg1OrNext(idx) == arg12 || Arg2OrEmptyMarker(idx) == arg12) & Arg3(idx) == arg3)
          return true;
      if (index23 == null)
        BuildIndex23();
      for (int idx = index23.Head(hashcode) ; idx != Empty ; idx = index23.Next(idx))
        if ((Arg1OrNext(idx) == arg12 || Arg2OrEmptyMarker(idx) == arg12) & Arg3(idx) == arg3)
          return true;
      return false;
    }

    public bool contains_1_2(int arg12) {
      int hashcode = Hashing.Hashcode(arg12);
      if (index1 == null)
        BuildIndex1();
      for (int idx = index1.Head(hashcode) ; idx != Empty ; idx = index1.Next(idx))
        if (Arg1OrNext(idx) == arg12 | Arg2OrEmptyMarker(idx) == arg12)
          return true;
      if (index2 == null)
        BuildIndex2();
      for (int idx = index2.Head(hashcode) ; idx != Empty ; idx = index2.Next(idx))
        if (Arg1OrNext(idx) == arg12 | Arg2OrEmptyMarker(idx) == arg12)
          return true;
      return false;
    }

    public bool Contains3(int arg3) {
      if (index3 == null)
        BuildIndex3();
      int hashcode = Hashing.Hashcode(arg3);
      for (int idx = index3.Head(hashcode) ; idx != Empty ; idx = index3.Next(idx))
        if (Arg3(idx) == arg3)
          return true;
      return false;
    }

    public int Count12(int arg1, int arg2) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1, arg2);
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx))
        if (Arg1OrNext(idx) == arg1 & Arg2OrEmptyMarker(idx) == arg2)
          count++;
      return count;
    }

    public int Lookup12(int arg1, int arg2) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      int hashcode = Hashing.Hashcode(arg1, arg2);
      int value = -1;
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx))
        if (Arg1OrNext(idx) == arg1 & Arg2OrEmptyMarker(idx) == arg2)
          if (value == -1)
            value = Arg3(idx);
          else
            throw ErrorHandler.SoftFail();
      return value;
    }

    public int lookup_13_23(int arg12, int arg3) {
      int hashcode = Hashing.Hashcode(arg12, arg3);
      int value = -1;

      if (index13 == null)
        BuildIndex13();

      for (int idx = index13.Head(hashcode) ; idx != Empty ; idx = index13.Next(idx))
        if (Arg3(idx) == arg3) {
          if (Arg1OrNext(idx) == arg12) {
            if (value == -1)
              value = Arg2OrEmptyMarker(idx);
            else
              throw ErrorHandler.SoftFail();
          }
          else if (Arg2OrEmptyMarker(idx) == arg12) {
            if (value == -1)
              value = Arg1OrNext(idx);
            else
              throw ErrorHandler.SoftFail();
          }
        }

      if (index23 == null)
        BuildIndex23();

      for (int idx = index23.Head(hashcode) ; idx != Empty ; idx = index23.Next(idx))
        if (Arg3(idx) == arg3) {
          if (Arg1OrNext(idx) == arg12) {
            int arg2 = Arg2OrEmptyMarker(idx);
            if (value == -1)
              value = arg2;
            else if (value != arg2)
              throw ErrorHandler.SoftFail();
          }
          else if (Arg2OrEmptyMarker(idx) == arg12) {
            int arg1 = Arg1OrNext(idx);
            if (value == -1)
              value = arg1;
            else if (value != arg1)
              throw ErrorHandler.SoftFail();
          }
        }

      return value;
    }

    public int count_13_23(int arg12, int arg3) {
      int hashcode = Hashing.Hashcode(arg12, arg3);
      int count = 0;
      if (index13 == null)
        BuildIndex13();
      for (int idx = index13.Head(hashcode) ; idx != Empty ; idx = index13.Next(idx))
        if (Arg1OrNext(idx) == arg12 && Arg3(idx) == arg3)
          count++;
      if (index23 == null)
        BuildIndex23();
      for (int idx = index23.Head(hashcode) ; idx != Empty ; idx = index23.Next(idx))
        if (Arg1OrNext(idx) != arg12 && Arg2OrEmptyMarker(idx) == arg12 && Arg3(idx) == arg3)
          count++;
      return count;
    }

    public int count_1_2(int arg12) {
      int hashcode = Hashing.Hashcode(arg12);
      int count = 0;
      if (index1 == null)
        BuildIndex1();
      for (int idx = index1.Head(hashcode) ; idx != Empty ; idx = index1.Next(idx))
        if (Arg1OrNext(idx) == arg12)
          count++;
      if (index2 == null)
        BuildIndex2();
      for (int idx = index2.Head(hashcode) ; idx != Empty ; idx = index2.Next(idx))
        if (Arg1OrNext(idx) != arg12 && Arg2OrEmptyMarker(idx) == arg12)
          count++;
      return count;
    }

    public int Count3(int arg3) {
      if (index3 == null)
        BuildIndex3();
      int count = 0;
      int hashcode = Hashing.Hashcode(arg3);
      for (int idx = index3.Head(hashcode) ; idx != Empty ; idx = index3.Next(idx))
        if (Arg3(idx) == arg3)
          count++;
      return count;
    }

    public bool Count12Eq(int arg1, int arg2, int expCount) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      int count = 0;
      int hashcode = Hashing.Hashcode(arg1, arg2);
      for (int idx = index12.Head(hashcode) ; idx != Empty ; idx = index12.Next(idx))
        if (Arg1OrNext(idx) == arg1 & Arg2OrEmptyMarker(idx) == arg2) {
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
        if (Arg3(idx) == arg3) {
          count++;
          if (count > expCount)
            return false;
        }
      return count == expCount;
    }

    public Iter123 GetIter() {
      return new Iter123(this);
    }

    public Iter12 GetIter12(int arg1, int arg2) {
      if (arg1 > arg2) {
        int tmp = arg1;
        arg1 = arg2;
        arg2 = tmp;
      }
      Debug.Assert(arg1 <= arg2);
      int hashcode = Hashing.Hashcode(arg1, arg2);
      return new Iter12(this, arg1, arg2, index12.Head(hashcode));
    }

    public Iter getIter_13_23(int arg12, int arg3) {
      int hashcode = Hashing.Hashcode(arg12, arg3);
      if (index13 == null)
        BuildIndex13();
      Iter iter1 = new Iter13(this, arg12, arg3, index13.Head(hashcode));
      if (index23 == null)
        BuildIndex23();
      Iter iter2 = new Iter23(this, arg12, arg3, index23.Head(hashcode));
      if (iter1.Done())
        return iter2;
      if (iter2.Done())
        return iter1;
      return new IterPair(this, iter1, iter2);
    }

    public Iter getIter_1_2(int arg12) {
      int hashcode = Hashing.Hashcode(arg12);
      if (index1 == null)
        BuildIndex1();
      Iter iter1 = new Iter1(this, arg12, index1.Head(hashcode));
      if (index2 == null)
        BuildIndex2();
      Iter iter2 = new Iter2(this, arg12, index2.Head(hashcode));
      if (iter1.Done())
        return iter2;
      if (iter2.Done())
        return iter1;
      return new IterPair(this, iter1, iter2);
    }

    public Iter3 GetIter3(int arg3) {
      if (index3 == null)
        BuildIndex3();
      int hashcode = Hashing.Hashcode(arg3);
      return new Iter3(this, arg3, index3.Head(hashcode));
    }

    public Obj Copy() {
      return Copy(new Sym12TernaryTable[] {this});
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
          bucket = Array.Append(bucket, count++, flatTuples[3 * i]);
          idx = index3.Next(idx);
        }

        if (count > 1) {
          if (count > 2)
            Array.Sort(bucket, count);
          int last = bucket[0];
          for (int j=1 ; j < count ; i++) {
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
          for (int j=1 ; j < count ; i++) {
            long val = bucket[j];
            if (val == last)
              return false;
            last = val;
          }
        }
      }

      return true;
    }

    void DeleteAt(int index, int hashcode) {
      int arg1 = Arg1OrNext(index);
      int arg2 = Arg2OrEmptyMarker(index);
      int arg3 = Arg3(index);
      Debug.Assert(arg2 != Empty);

      // Removing the tuple
      SetEntry(index, firstFree, Empty, 0);
      firstFree = index;
      count--;

      // Updating the indexes
      index123.Delete(index, hashcode);
      index12.Delete(index, Hashing.Hashcode(arg1, arg2));
      if (index13 != null)
        index13.Delete(index, Hashing.Hashcode(arg1, arg3));
      if (index23 != null)
        index23.Delete(index, Hashing.Hashcode(arg2, arg3));
      if (index1 != null)
        index1.Delete(index, Hashing.Hashcode(arg1));
      if (index2 != null)
        index2.Delete(index, Hashing.Hashcode(arg2));
      if (index3 != null)
        index3.Delete(index, Hashing.Hashcode(arg3));
    }

    void BuildIndex123() {
      int size = Capacity();
      index123 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int arg2 = Arg2OrEmptyMarker(i);
        if (arg2 != Empty)
          index123.Insert(i, Hashing.Hashcode(Arg1OrNext(i), arg2, Arg3(i)));
      }
    }

    void BuildIndex12() {
      int size = Capacity();
      index12 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int arg2 = Arg2OrEmptyMarker(i);
        if (arg2 != Empty)
          index12.Insert(i, Hashing.Hashcode(Arg1OrNext(i), arg2));
      }
    }

    void BuildIndex13() {
      int size = Capacity();
      index13 = new Index(size);
      for (int i=0 ; i < size ; i++)
        if (Arg2OrEmptyMarker(i) != Empty)
          index13.Insert(i, Hashing.Hashcode(Arg1OrNext(i), Arg3(i)));
    }

    void BuildIndex23() {
      int size = Capacity();
      index23 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int arg2 = Arg2OrEmptyMarker(i);
        if (arg2 != Empty)
          index23.Insert(i, Hashing.Hashcode(arg2, Arg3(i)));
      }
    }

    void BuildIndex1() {
      int size = Capacity();
      index1 = new Index(size);
      for (int i=0 ; i < size ; i++)
        if (Arg2OrEmptyMarker(i) != Empty)
          index1.Insert(i, Hashing.Hashcode(Arg1OrNext(i)));
    }

    void BuildIndex2() {
      int size = Capacity();
      index2 = new Index(size);
      for (int i=0 ; i < size ; i++) {
        int arg2 = Arg2OrEmptyMarker(i);
        if (arg2 != Empty)
          index2.Insert(i, Hashing.Hashcode(arg2));
      }
    }

    void BuildIndex3() {
      int size = Capacity();
      index3 = new Index(size);
      for (int i=0 ; i < size ; i++)
        if (Arg2OrEmptyMarker(i) != Empty)
          index3.Insert(i, Hashing.Hashcode(Arg3(i)));
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static Obj Copy(Sym12TernaryTable[] tables) {
      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].count;

      if (count == 0)
        return EmptyRelObj.singleton;

      Obj[] col1 = new Obj[count];
      Obj[] col2 = new Obj[count];
      Obj[] col3 = new Obj[count];

      int next = 0;
      for (int iT=0 ; iT < tables.Length ; iT++) {
        Sym12TernaryTable table = tables[iT];
        SurrObjMapper mapper12 = table.mapper12;
        SurrObjMapper mapper3 = table.mapper3;
        int size = table.Capacity();
        for (int iS=0 ; iS < size ; iS++) {
          int arg2 = table.Arg2OrEmptyMarker(iS);
          if (arg2 != Empty) {
            col1[next] = mapper12(table.Arg1OrNext(iS));
            col2[next] = mapper12(arg2);
            col3[next] = mapper3(table.Arg3(iS));
            next++;
          }
        }
      }
      Debug.Assert(next == count);

      return Builder.CreateTernRel(col1, col2, col3, count);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public abstract class Iter {
      protected Sym12TernaryTable table;

      protected Iter(Sym12TernaryTable table) {
        this.table = table;
      }

      public abstract bool Done();
      public abstract void Next();

      public virtual int Get1() {
        throw ErrorHandler.InternalFail();
      }

      public virtual int Get2() {
        throw ErrorHandler.InternalFail();
      }

      public virtual int Get3() {
        throw ErrorHandler.InternalFail();
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class IterPair : Iter {
      Iter iter1;
      Iter iter2;

      public IterPair(Sym12TernaryTable table, Iter iter1, Iter iter2) : base(table) {
        Debug.Assert(!iter1.Done() & !iter2.Done());
        this.iter1 = iter1;
        this.iter2 = iter2;
      }

      public override bool Done() {
        return iter1 == null;
      }

      public override void Next() {
        iter1.Next();
        if (iter1.Done()) {
          iter1 = iter2;
          iter2 = null;
        }
      }

      public override int Get1() {
        return iter1.Get1();
      }

      public override int Get2() {
        return iter1.Get2();
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public abstract class IdxIter : Iter {
      protected int index;

      protected IdxIter(Sym12TernaryTable table, int index) : base(table) {
        this.index = index;
      }

      public override bool Done() {
        return index == Empty;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter123 : IdxIter {
      int end;

      public Iter123(Sym12TernaryTable table) : base(table, 0) {
        end = table.Capacity();
        while (table.Arg2OrEmptyMarker(index) == Empty) {
          index++;
          if (index == end) {
            index = Empty;
            break;
          }
        }
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg1OrNext(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Arg2OrEmptyMarker(index);
      }

      public override int Get3() {
        Debug.Assert(index != Empty);
        return table.Arg3(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index++;
          if (index == end) {
            index = Empty;
            return;
          }
        } while (table.Arg2OrEmptyMarker(index) == Empty);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter12 : IdxIter {
      int arg1, arg2;

      public Iter12(Sym12TernaryTable table, int arg1, int arg2, int index0) : base(table, index0) {
        Debug.Assert(arg1 <= arg2);
        this.arg1 = arg1;
        this.arg2 = arg2;
        while (index != Empty && !IsMatch())
          index = table.index12.Next(index);
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg3(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index = table.index12.Next(index);
        }
        while (index != Empty && !IsMatch());
      }

      bool IsMatch() {
        return table.Arg1OrNext(index) == arg1 && table.Arg2OrEmptyMarker(index) == arg2;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private sealed class Iter13 : IdxIter {
      int arg1, arg3;

      public Iter13(Sym12TernaryTable table, int arg1, int arg3, int index0) : base(table, index0) {
        this.arg1 = arg1;
        this.arg3 = arg3;
        while (index != Empty && !IsMatch())
          index = table.index13.Next(index);
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg2OrEmptyMarker(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index = table.index13.Next(index);
        } while (index != Empty && !IsMatch());
      }

      bool IsMatch() {
        return table.Arg1OrNext(index) == arg1 && table.Arg3(index) == arg3;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private sealed class Iter23 : IdxIter {
      int arg2, arg3;

      public Iter23(Sym12TernaryTable table, int arg2, int arg3, int index0) : base(table, index0) {
        this.arg2 = arg2;
        this.arg3 = arg3;
        while (index != Empty && !IsMatch())
          index = table.index23.Next(index);
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg1OrNext(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index = table.index23.Next(index);
        } while (index != Empty && !IsMatch());
      }

      bool IsMatch() {
        // Since it's always used together with Iter13, in order to avoid duplicates we skip entries
        // of the form (arg2, arg2, arg3) that would be found by a search through the other index
        return table.Arg1OrNext(index) != arg2 &&
               table.Arg2OrEmptyMarker(index) == arg2 &&
               table.Arg3(index) == arg3;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private sealed class Iter1 : IdxIter {
      int arg1;

      public Iter1(Sym12TernaryTable table, int arg1, int index0) : base(table, index0) {
        this.arg1 = arg1;
        while (index != Empty && !IsMatch())
          index = table.index1.Next(index);
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg2OrEmptyMarker(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Arg3(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index = table.index1.Next(index);
        } while (index != Empty && !IsMatch());
      }

      bool IsMatch() {
        return table.Arg1OrNext(index) == arg1;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private sealed class Iter2 : IdxIter {
      int arg2;

      public Iter2(Sym12TernaryTable table, int arg2, int index0) : base(table, index0) {
        this.arg2 = arg2;
        while (index != Empty && !IsMatch())
          index = table.index2.Next(index);
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg1OrNext(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Arg3(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index = table.index2.Next(index);
        } while (index != Empty && !IsMatch());
      }

      bool IsMatch() {
        // Since it's always used together with Iter1, in order to avoid duplicates we skip entries
        // of the form (arg2, arg2, *) that would be found by a search through the other index
        return table.Arg1OrNext(index) != arg2 && table.Arg2OrEmptyMarker(index) == arg2;
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public sealed class Iter3 : IdxIter {
      int arg3;

      public Iter3(Sym12TernaryTable table, int arg3, int index0) : base(table, index0) {
        this.arg3 = arg3;
        while (index != Empty && table.Arg3(index) != arg3)
          index = table.index3.Next(index);
      }

      public override int Get1() {
        Debug.Assert(index != Empty);
        return table.Arg1OrNext(index);
      }

      public override int Get2() {
        Debug.Assert(index != Empty);
        return table.Arg2OrEmptyMarker(index);
      }

      public override void Next() {
        Debug.Assert(!Done());
        do {
          index = table.index3.Next(index);
        } while (index != Empty && table.Arg3(index) != arg3);
      }
    }
  }
}
