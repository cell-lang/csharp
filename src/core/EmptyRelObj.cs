namespace Cell.Runtime {
  public sealed class EmptyRelObj : Obj {
    public static readonly EmptyRelObj singleton = new EmptyRelObj();

    private EmptyRelObj() {
      extraData = EmptyRelObjExtraData();
    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj Insert(Obj obj) {
      return new NeTreeSetObj(obj);
    }

    public override Obj Remove(Obj obj) {
      return this;
    }

    public override Obj SetKeyValue(Obj key, Obj value) {
      return new NeTreeMapObj(key, value);
    }

    public override Obj DropKey(Obj key) {
      return this;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool Contains(Obj obj) {
      return false;
    }

    public override bool Contains(Obj val1, Obj val2) {
      return false;
    }

    public override bool Contains(Obj val1, Obj val2, Obj val3) {
      return false;
    }

    public override bool Contains1(Obj val) {
      return false;
    }

    public override bool Contains2(Obj val) {
      return false;
    }

    public override bool Contains3(Obj val) {
      return false;
    }

    public override bool Contains12(Obj val1, Obj val2) {
      return false;
    }

    public override bool Contains13(Obj val1, Obj val3) {
      return false;
    }

    public override bool Contains23(Obj val2, Obj val3) {
      return false;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool HasField(ushort fieldId) {
      return false;
    }

    public override SetIter GetSetIter() {
      return iter1;
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      return Array.emptyObjArray;
    }

    public override BinRelIter GetBinRelIter() {
      return iter2;
    }

    public override BinRelIter GetBinRelIterByCol1(Obj obj) {
      return iter2;
    }

    public override BinRelIter GetBinRelIterByCol2(Obj obj) {
      return iter2;
    }

    public override TernRelIter GetTernRelIter() {
      return iter3;
    }

    public override TernRelIter GetTernRelIterByCol1(Obj val) {
      return iter3;
    }

    public override TernRelIter GetTernRelIterByCol2(Obj val) {
      return iter3;
    }

    public override TernRelIter GetTernRelIterByCol3(Obj val) {
      return iter3;
    }

    public override TernRelIter GetTernRelIterByCol12(Obj val1, Obj val2) {
      return iter3;
    }

    public override TernRelIter GetTernRelIterByCol13(Obj val1, Obj val3) {
      return iter3;
    }

    public override TernRelIter GetTernRelIterByCol23(Obj val2, Obj val3) {
      return iter3;
    }

    public override SeqObj InternalSort() {
      return EmptySeqObj.singleton;
    }

    public override Obj Lookup(Obj key) {
      throw ErrorHandler.SoftFail("Key not found:", "collection", this, "key", key);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      return 0;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.EMPTY_REL;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.EmptyRelObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    private static readonly SetIter     iter1 = new SetIter(new Obj[0], 0, -1);
    private static readonly BinRelIter  iter2 = new BinRelIter(new Obj[0], new Obj[0]);
    private static readonly TernRelIter iter3 = new TernRelIter(new Obj[0], new Obj[0], new Obj[0]);
  }
}