namespace Cell.Runtime {
  public class IntObj : Obj {
    IntObj(long value) {
      data = IntObjData(value);
      extraData = IntObjExtraData();
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      return Hashcode(GetLong());
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.INTEGER;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.IntObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    static IntObj[] smallIntObjs = new IntObj[384];

    static IntObj() {
      for (int i=0 ; i < 384 ; i++)
        smallIntObjs[i] = new IntObj(i - 128);
    }

    public static IntObj Get(long value) {
      if (value >= -128 & value < 256)
        return smallIntObjs[128 + (int) value];
      else
        return new IntObj(value);
    }

    public static int Compare(long x1, long x2) {
      return x1 == x2 ? 0 : (x1 < x2 ? -1 : 1);
    }

    public static uint Hashcode(long x) {
      return Hashing.Hashcode64(x);
    }
  }
}