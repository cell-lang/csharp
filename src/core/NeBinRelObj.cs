namespace Cell.Runtime {
  public class NeBinRelObj : Obj {
    protected Obj[] col1;
    protected Obj[] col2;
    protected uint[] hashcodes1;
    protected bool isMap;
    protected uint hcode = Hashing.NULL_HASHCODE;
    int[] revIdxs;


    private NeBinRelObj(Obj[] col1, Obj[] col2, uint[] hashcodes1, bool isMap) {
      Debug.Assert(col1 != null && col2 != null);
      Debug.Assert(col1.Length > 0);
      Debug.Assert(col1.Length == col2.Length);

      data = BinRelObjData((uint) col1.Length);
      extraData = NeBinRelObjExtraData();

      this.col1 = col1;
      this.col2 = col2;
      this.hashcodes1 = hashcodes1;
      this.isMap = isMap;
    }

    protected NeBinRelObj() {

    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj SetKeyValue(Obj key, Obj value) {
      if (!isMap)
        throw ErrorHandler.InternalFail(this);

      NeTreeMapObj tree = new NeTreeMapObj(col1, col2, hashcodes1, 0, col1.Length);
      return tree.SetKeyValue(key, value);
    }

    public override Obj DropKey(Obj key) {
      if (!isMap)
        throw ErrorHandler.InternalFail(this);

      if (!Contains1(key))
        return this;

      NeTreeMapObj tree = new NeTreeMapObj(col1, col2, hashcodes1, 0, col1.Length);
      return tree.DropKey(key);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool IsNeMap() {
      return isMap;
    }

    public override bool IsNeRecord() {
      if (!isMap)
        return false;
      int len = col1.Length;
      for (int i=0 ; i < len ; i++)
        if (!col1[i].IsSymb())
          return false;
      return true;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool Contains1(Obj key) {
      Debug.Assert(isMap);
      return KeyRangeStart(col1, hashcodes1, key) >= 0;
    }

    public override bool Contains2(Obj obj) {
      if (revIdxs == null)
        revIdxs = Algs.SortedIndexes(col2, col1);
      return Algs.BinSearchRange(revIdxs, col2, obj)[1] > 0;
    }

    public override bool Contains(Obj obj1, Obj obj2) {
      if (isMap) {
        int idx = KeyRangeStart(col1, hashcodes1, obj1);
        return idx >= 0 && col2[idx].IsEq(obj2);
      }
      else {
        int idx = KeyRangeStart(col1, hashcodes1, obj1);
        if (idx >= 0) {
          int endIdx = KeyRangeEnd(idx, col1, hashcodes1, obj1);
          //## BAD BAD BAD: LINEAR SEARCH, INEFFICIENT
          for (int i=idx ; i < endIdx ; i++)
            if (col2[i].IsEq(obj2))
              return true;
        }
        return false;
      }
    }

    public override bool HasField(ushort fieldId) {
      int len = col1.Length;
      for (int i=0 ; i < len ; i++)
        if (col1[i].IsSymb(fieldId))
          return true;
      return false;
    }

    public override BinRelIter GetBinRelIter() {
      return new BinRelIter(col1, col2);
    }

    public override BinRelIter GetBinRelIterByCol1(Obj obj) {
      int startIdx = KeyRangeStart(col1, hashcodes1, obj);
      if (startIdx < 0)
        return BinRelIter.emptyIter;
      int endIdx = KeyRangeEnd(startIdx, col1, hashcodes1, obj);
      return new BinRelIter(col1, col2, startIdx, endIdx-1);
    }


    public override BinRelIter GetBinRelIterByCol2(Obj obj) {
      if (revIdxs == null)
        revIdxs = Algs.SortedIndexes(col2, col1);
      int[] firstAndCount = Algs.BinSearchRange(revIdxs, col2, obj);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new BinRelIter(col1, col2, revIdxs, first, first+count-1);
    }

    public override Obj Lookup(Obj key) {
      int idx = KeyRangeStart(col1, hashcodes1, key);
      if (idx < 0)
        throw ErrorHandler.SoftFail("Key not found:", "collection", this, "key", key);
      if (!isMap && idx < col1.Length - 1 && col1[idx+1].IsEq(key))
          throw ErrorHandler.SoftFail("Duplicate key:", "collection", this, "key", key);
      return col2[idx];
    }

    public override Obj LookupField(ushort fieldId) {
      int len = col1.Length;
      for (int i=0 ; i < len ; i++)
        if (col1[i].IsSymb(fieldId))
          return col2[i];
      // We should never get here. The typechecker should prevent it.
      throw ErrorHandler.InternalFail(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      Debug.Assert(GetSize() == other.GetSize());

      if (other is RecordObj)
        return -other.InternalOrder(this);

      if (other is NeTreeMapObj)
        return -other.InternalOrder(this);

      NeBinRelObj otherRel = (NeBinRelObj) other;
      int size = GetSize();

      Obj[] col = col1;
      Obj[] otherCol = otherRel.col1;
      for (int i=0 ; i < size ; i++) {
        int ord = col[i].QuickOrder(otherCol[i]);
        if (ord != 0)
          return ord;
      }

      col = col2;
      otherCol = otherRel.col2;
      for (int i=0 ; i < size ; i++) {
        int ord = col[i].QuickOrder(otherCol[i]);
        if (ord != 0)
          return ord;
      }

      return 0;
    }

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        long hcode = 0;
        for (int i=0 ; i < col1.Length ; i++)
          hcode += Hashing.Hashcode(hashcodes1[i], col2[i].Hashcode());
        hcode = Hashing.Hashcode64(hcode);
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.NE_BIN_REL;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.NeBinRelObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    internal virtual Obj[] Col1() {
      return col1;
    }

    internal Obj[] Col2() {
      return col2;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static int KeyRangeStart(Obj[] objs, uint[] hashcodes, Obj key) {
      uint hashcode = key.Hashcode();
      int idx = Array.AnyIndexOrEncodedInsertionPointIntoSortedArray(hashcodes, hashcode);
      if (idx < 0)
        return idx;

      while (idx > 0 && hashcodes[idx-1] == hashcode)
        idx--;

      do {
        int ord = key.QuickOrder(objs[idx]);
        if (ord > 0) // objs[idx] < key, checking the next slot
          idx++;
        else if (ord < 0) // key < objs[idx], search failed
          return -1;
        else
          return idx;
      } while (idx < hashcodes.Length && hashcodes[idx] == hashcode);

      return -1;
    }

    private static int KeyRangeEnd(int rangeStart, Obj[] objs, uint[] hashcodes, Obj key) {
      int idx = rangeStart + 1;
      uint hashcode = hashcodes[rangeStart];
      while (idx < objs.Length && hashcodes[idx] == hashcode && objs[idx].IsEq(key))
        idx++;
      return idx;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void SelfCheck(Obj[] _col1, Obj[] _col2, int _count) {
      int len = col1.Length;
      Check(len <= _count);
      Check(col2.Length == len);
      Check(hashcodes1.Length == len);

      for (int i=0 ; i < len ; i++)
        Check(hashcodes1[i] == col1[i].Hashcode());

      bool _isMap = true;
      for (int i=1 ; i < len ; i++) {
        Check(hashcodes1[i-1] <= hashcodes1[i]);

        if (hashcodes1[i] == hashcodes1[i-1]) {
          int ord1 = col1[i-1].QuickOrder(col1[i]);
          Check(ord1 <= 0);
          if (ord1 == 0) {
            _isMap = false;
            Check(col2[i-1].QuickOrder(col2[i]) < 0);
          }
        }
      }

      Check(isMap == _isMap);

      for (int i=0 ; i < len ; i++) {
        Obj key = col1[i];
        int start = KeyRangeStart(col1, hashcodes1, key);
        Check(start >= 0);
        int end = KeyRangeEnd(start, col1, hashcodes1, key);
        Check(!isMap || start + 1 == end);
        Check(i >= start);
        Check(i < end);
      }

      if (isMap) {
        for (int i=0 ; i < len ; i++) {
          Obj key = col1[i];
          Obj value = col2[i];
          Check(Lookup(key).IsEq(value));
        }

        for (int i=0 ; i < _count ; i++)
          Check(Lookup(_col1[i]).IsEq(_col2[i]));
      }

      for (int i=0 ; i < _count ; i++) {
        Check(Contains(_col1[i], _col2[i]));
      }
    }

    // private static void Dump(NeBinRelObj rel, Obj[] _col1, Obj[] _col2, int _count) {
    //   System.out.Println(rel.ToString());
    //   System.out.Println();
    //   for (int i=0 ; i < _count ; i++)
    //     System.out.Printf("%s -> %s\n", _col1[i].ToString(), _col2[i].ToString());
    // }

    private static void Check(bool cond) {
      Debug.Assert(cond);
    }

    /////////////////////////////////////////////////////////////////////////////

    public static NeBinRelObj Create(Obj[] col1, Obj[] col2, int count) {
      Debug.Assert(count > 0);

      IdxSorter sorter1 = null;
      IdxSorter sorter2 = null;

      ulong[] keysIdxs = IndexesSortedByHashcode(col1, count);

      bool isMap = true;

      int writeIdx = 0;
      int hashStartIdx = 0;
      do {
        uint hashcode = MostSignificant(keysIdxs[hashStartIdx]);
        int hashEndIdx = hashStartIdx + 1;
        while (hashEndIdx < count && MostSignificant(keysIdxs[hashEndIdx]) == hashcode)
          hashEndIdx++;

        if (hashEndIdx - hashStartIdx > 1) {
          if (sorter1 == null)
            sorter1 = new IdxSorter(col1);
          sorter1.Sort(keysIdxs, hashStartIdx, hashEndIdx);

          int keyStartIdx = hashStartIdx;
          do {
            Obj key = col1[LeastSignificant(keysIdxs[keyStartIdx])];
            int keyEndIdx = keyStartIdx + 1;
            while (keyEndIdx < hashEndIdx && key.IsEq(col1[LeastSignificant(keysIdxs[keyEndIdx])]))
              keyEndIdx++;

            int uniqueKeyEndIdx = keyEndIdx;
            if (keyEndIdx - keyStartIdx > 1) {
              for (int i=keyStartIdx ; i < keyEndIdx ; i++) {
                uint idx = LeastSignificant(keysIdxs[i]);
                keysIdxs[i] = (((ulong) col2[idx].Hashcode()) << 32) | idx;
              }
              Array.Sort(keysIdxs, keyStartIdx, keyEndIdx);

              if (sorter2 == null)
                sorter2 = new IdxSorter(col2);
              uniqueKeyEndIdx = SortHashcodeRangeUnique(keysIdxs, keyStartIdx, keyEndIdx, col2, sorter2);

              if (uniqueKeyEndIdx != keyStartIdx + 1)
                isMap = false;

              for (int i=keyStartIdx ; i < uniqueKeyEndIdx ; i++)
                keysIdxs[i] = (((ulong) hashcode) << 32) | LeastSignificant(keysIdxs[i]);
            }

            if (keyStartIdx != writeIdx)
              for (int i=keyStartIdx ; i < uniqueKeyEndIdx ; i++)
                keysIdxs[writeIdx++] = keysIdxs[i];
            else
              writeIdx += uniqueKeyEndIdx - keyStartIdx;

            keyStartIdx = keyEndIdx;
          } while (keyStartIdx < hashEndIdx);
        }
        else {
          if (hashStartIdx != writeIdx)
            keysIdxs[writeIdx] = keysIdxs[hashStartIdx];
          writeIdx++;
        }

        hashStartIdx = hashEndIdx;
      } while (hashStartIdx < count);

      uint[] hashcodes = new uint[writeIdx];
      Obj[] sortedCol1 = new Obj[writeIdx];
      Obj[] sortedCol2 = new Obj[writeIdx];
      for (int i=0 ; i < writeIdx ; i++) {
        ulong keyIdx = keysIdxs[i];
        hashcodes[i] = MostSignificant(keyIdx);
        uint idx = LeastSignificant(keyIdx);
        sortedCol1[i] = col1[idx];
        sortedCol2[i] = col2[idx];
      }

      NeBinRelObj relObj = new NeBinRelObj(sortedCol1, sortedCol2, hashcodes, isMap);
      // relObj.SelfCheck(col1, col2, count);
      return relObj;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static int SortHashcodeRangeUnique(ulong[] keysIdxs, int start, int end, Obj[] objs, IdxSorter sorter) {
      int writeIdx = start;
      int hashStartIdx = start;
      do {
        uint hashcode = MostSignificant(keysIdxs[hashStartIdx]);
        int hashEndIdx = hashStartIdx + 1;
        while (hashEndIdx < end && MostSignificant(keysIdxs[hashStartIdx]) == hashcode)
          hashEndIdx++;

        if (hashEndIdx - hashStartIdx > 1) {
          sorter.Sort(keysIdxs, hashStartIdx, hashEndIdx);

          int idx = hashStartIdx;
          do {
            if (idx != writeIdx)
              keysIdxs[writeIdx] = keysIdxs[idx];
            writeIdx++;

            Obj obj = objs[LeastSignificant(keysIdxs[idx++])];
            while (idx < hashEndIdx && obj.IsEq(objs[LeastSignificant(keysIdxs[idx])]))
              idx++;
          } while (idx < hashEndIdx);
        }
        else {
          if (hashStartIdx != writeIdx)
            keysIdxs[writeIdx] = keysIdxs[hashStartIdx];
          writeIdx++;
        }

        hashStartIdx = hashEndIdx;
      } while (hashStartIdx < end);

      return writeIdx;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static ulong[] IndexesSortedByHashcode(Obj[] objs, int count) {
      ulong[] keysIdxs = new ulong[count];
      for (uint i=0 ; i < count ; i++)
        keysIdxs[i] = (((ulong) objs[i].Hashcode()) << 32) | i;
      Array.Sort(keysIdxs);
      return keysIdxs;
    }

    private sealed class IdxSorter : AbstractULongSorter {
      Obj[] objs;

      public IdxSorter(Obj[] objs) {
        this.objs = objs;
      }

      protected override bool IsGreater(ulong value1, ulong value2) {
        int idx1 = (int) (value1 & 0xFFFFFFFF);
        int idx2 = (int) (value2 & 0xFFFFFFFF);
        return objs[idx1].QuickOrder(objs[idx2]) > 0;
      }
    }

    private static uint MostSignificant(ulong value) {
      return (uint) (value >> 32);
    }

    private static uint LeastSignificant(ulong value) {
      return (uint) (value & 0xFFFFFFFF);
    }
  }
}