namespace Cell.Runtime {
  public sealed class NeTernRelObj : Obj {
    Obj[] col1;
    Obj[] col2;
    Obj[] col3;
    int[] idxs231;
    int[] idxs312;
    uint hcode = Hashing.NULL_HASHCODE;


    public NeTernRelObj(Obj[] col1, Obj[] col2, Obj[] col3) {
      Debug.Assert(col1 != null && col2 != null && col3 != null);
      Debug.Assert(col1.Length == col2.Length && col1.Length == col3.Length);
      Debug.Assert(col1.Length > 0);

      int size = col1.Length;
      data = TernRelObjData((uint) size);
      extraData = NeTernRelObjExtraData();

      this.col1 = col1;
      this.col2 = col2;
      this.col3 = col3;
    }

    public override bool Contains1(Obj val) {
      return Algs.BinSearchRange(col1, 0, col1.Length, val)[1] > 0;
    }

    public override bool Contains2(Obj val) {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      return Algs.BinSearchRange(idxs231, col2, val)[1] > 0;
    }

    public override bool Contains3(Obj val) {
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);
      return Algs.BinSearchRange(idxs312, col3, val)[1] > 0;
    }

    public override bool Contains12(Obj val1, Obj val2) {
      return Algs.BinSearchRange(col1, col2, val1, val2)[1] > 0;
    }

    public override bool Contains13(Obj val1, Obj val3) {
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);
      return Algs.BinSearchRange(idxs312, col3, col1, val3, val1)[1] > 0;
    }

    public override bool Contains23(Obj val2, Obj val3) {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      return Algs.BinSearchRange(idxs231, col2, col3, val2, val3)[1] > 0;
    }

    public override bool Contains(Obj obj1, Obj obj2, Obj obj3) {
      int[] firstAndCount = Algs.BinSearchRange(col1, 0, col1.Length, obj1);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      if (count == 0)
        return false;

      firstAndCount = Algs.BinSearchRange(col2, first, count, obj2);
      first = firstAndCount[0];
      count = firstAndCount[1];
      if (count == 0)
        return false;

      int idx = Algs.BinSearch(col3, first, count, obj3);
      return idx != -1;
    }

    public override TernRelIter GetTernRelIter() {
      return new TernRelIter(col1, col2, col3);
    }

    public override TernRelIter GetTernRelIterByCol1(Obj val) {
      int[] firstAndCount = Algs.BinSearchRange(col1, 0, col1.Length, val);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new TernRelIter(col1, col2, col3, null, first, first+count-1);
    }

    public override TernRelIter GetTernRelIterByCol2(Obj val) {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      int[] firstAndCount = Algs.BinSearchRange(idxs231, col2, val);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new TernRelIter(col1, col2, col3, idxs231, first, first+count-1);
    }

    public override TernRelIter GetTernRelIterByCol3(Obj val) {
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);
      int[] firstAndCount = Algs.BinSearchRange(idxs312, col3, val);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new TernRelIter(col1, col2, col3, idxs312, first, first+count-1);
    }

    public override TernRelIter GetTernRelIterByCol12(Obj val1, Obj val2) {
      int[] firstAndCount = Algs.BinSearchRange(col1, col2, val1, val2);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new TernRelIter(col1, col2, col3, null, first, first+count-1);
    }

    public override TernRelIter GetTernRelIterByCol13(Obj val1, Obj val3) {
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);
      int[] firstAndCount = Algs.BinSearchRange(idxs312, col3, col1, val3, val1);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new TernRelIter(col1, col2, col3, idxs312, first, first+count-1);
    }

    public override TernRelIter GetTernRelIterByCol23(Obj val2, Obj val3) {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      int[] firstAndCount = Algs.BinSearchRange(idxs231, col2, col3, val2, val3);
      int first = firstAndCount[0];
      int count = firstAndCount[1];
      return new TernRelIter(col1, col2, col3, idxs231, first, first+count-1);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      Debug.Assert(GetSize() == other.GetSize());

      NeTernRelObj otherRel = (NeTernRelObj) other;
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

      col = col3;
      otherCol = otherRel.col3;
      for (int i=0 ; i < size ; i++) {
        int ord = col[i].QuickOrder(otherCol[i]);
        if (ord != 0)
          return ord;
      }

      return 0;
    }

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        ulong code = 0;
        for (int i=0 ; i < col1.Length ; i++)
          code += Hashing.Hashcode(col1[i].Hashcode(), col2[i].Hashcode(), col3[i].Hashcode());
        hcode = Hashing.Hashcode64(code);
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.NE_TERN_REL;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.NeTernRelObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    internal Obj[] Col1() {
      return col1;
    }

    internal Obj[] Col2() {
      return col2;
    }

    internal Obj[] Col3() {
      return col3;
    }
  }
}
