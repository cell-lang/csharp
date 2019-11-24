namespace Cell.Runtime {
  public sealed class BlankObj : Obj {
    public static readonly BlankObj singleton = new BlankObj();

    private BlankObj() {
      extraData = BlankObjExtraData();
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
      visitor.BlankObj(this);
    }
  }
}
