namespace Cell.Runtime {
  public sealed class TaggedIntObj : Obj {
    public static bool Fits(long value) {
      return (value << 16) >> 16 == value;
    }

    public TaggedIntObj(ushort tag, long value) {
      Debug.Assert(Fits(value));
      data = TagIntObjData(tag, value);
      extraData = TagIntObjExtraData();
      Debug.Assert(tag == GetTagId());
      Debug.Assert(GetInnerLong() == value);
    }

    public override Obj GetInnerObj() {
      return IntObj.Get(GetInnerLong());
    }

    public override long GetInnerLong() {
      return ((long) data) >> 16;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      return Hashing.Hashcode(GetTagId(), IntObj.Hashcode(GetInnerLong()));
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.TAGGED_VALUE;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.TaggedIntObj(this);
    }
  }
}
