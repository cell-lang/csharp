namespace Cell.Runtime {
  public sealed class NullObj : Obj {
    public static readonly NullObj singleton = new NullObj();

    private NullObj() {
      extraData = NullObjExtraData();
    }

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      throw ErrorHandler.InternalFail();
    }

    public override TypeCode GetTypeCode() {
      throw ErrorHandler.InternalFail(this);
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.NullObj(this);
    }
  }
}
