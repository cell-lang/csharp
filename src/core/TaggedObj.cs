namespace Cell.Runtime {
  public sealed class TaggedObj : Obj {
    Obj obj;


    public TaggedObj(ushort tag, Obj obj) {
      data = TagObjData(tag);
      extraData = RefTagObjExtraData();
      this.obj = obj;
    }

    public override bool HasField(ushort fieldId) {
      return obj.HasField(fieldId);
    }

    public override Obj GetInnerObj() {
      return obj;
    }

    public override long GetInnerLong() {
      return obj.GetLong();
    }

    public override Obj LookupField(ushort fieldId) {
      return obj.LookupField(fieldId);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      Debug.Assert(GetTagId() == other.GetTagId());
      return obj.QuickOrder(((TaggedObj) other).obj);
    }

    public override uint Hashcode() {
      return Hashing.Hashcode(SymbObj.Get(GetTagId()).Hashcode(), obj.Hashcode());
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.TAGGED_VALUE;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.TaggedObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override string GetString() {
      Debug.Assert(GetTagId() == SymbObj.StringSymbId);
      return Miscellanea.UnicodeString(obj.GetLongArray());
    }
  }
}
