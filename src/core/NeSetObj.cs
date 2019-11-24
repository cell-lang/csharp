namespace Cell.Runtime {
  public sealed class NeSetObj : Obj {
    Obj[] elts;
    uint[] hashcodes;
    uint hcode = Hashing.NULL_HASHCODE;


    public NeSetObj(Obj[] elts, uint[] hashcodes) {
      Debug.Assert(elts.Length > 0);

      data = SetObjData((uint) elts.Length);
      extraData = NeSetObjExtraData();
      this.elts = elts;
      this.hashcodes = hashcodes;
    }

    public override Obj Insert(Obj obj) {
      if (Contains(obj))
        return this;

      NeTreeSetObj treeSet = new NeTreeSetObj(elts, hashcodes, 0, elts.Length);
      return treeSet.Insert(obj);
    }

    public override Obj Remove(Obj obj) {
      if (!Contains(obj))
        return this;

      NeTreeSetObj treeSet = new NeTreeSetObj(elts, hashcodes, 0, elts.Length);
      return treeSet.Remove(obj);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool Contains(Obj obj) {
      uint hashcode = obj.Hashcode();
      int idx = Array.AnyIndexOrEncodedInsertionPointIntoSortedArray(hashcodes, hashcode);
      if (idx >= 0) {
        for (int i=idx ; i < elts.Length && hashcodes[i] == hashcode ; i++)
          if (elts[i].IsEq(obj))
            return true;
        for (int i=idx-1 ; i >= 0 && hashcodes[i] == hashcode ; i--)
          if (elts[i].IsEq(obj))
            return true;
      }
      return false;
    }

    public override SetIter GetSetIter() {
      return new SetIter(elts, 0, elts.Length-1);
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      return elts;
    }

    public override SeqObj InternalSort() {
      Obj[] sortedElts = Array.Take(elts, elts.Length);
      Array.CanonicalSort(sortedElts);
      return ArrayObjs.Create(sortedElts);
    }

    public override Obj RandElem() {
      return elts[0];
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      Debug.Assert(GetSize() == other.GetSize());

      if (other is NeTreeSetObj)
        return -other.InternalOrder(this);

      NeSetObj otherSet = (NeSetObj) other;
      int size = GetSize();
      Obj[] otherElts = otherSet.elts;
      for (int i=0 ; i < size ; i++) {
        int ord = elts[i].QuickOrder(otherElts[i]);
        if (ord != 0)
          return ord;
      }
      return 0;
    }

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        hcode = Hashing.Hashcode64(Array.Sum(hashcodes));
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.NE_SET;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.NeSetObj(this);
    }

    ////////////////////////////////////////////////////////////////////////////

    internal Obj[] Elts() {
      return elts;
    }
  }
}
