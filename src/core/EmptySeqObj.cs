namespace Cell.Runtime {
  public sealed class EmptySeqObj : SeqObj {
    public static readonly EmptySeqObj singleton = new EmptySeqObj();

    private EmptySeqObj() {
      extraData = EmptySeqObjExtraData();
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool IsIntSeq() {
      return true;
    }

    public override Obj GetObjAt(long idx) {
      throw ErrorHandler.SoftFail();
    }

    public override SeqObj GetSlice(long first, long count) {
      if (first == 0 & count == 0)
        return this;
      else
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, 0);
    }

    public override bool[] GetBoolArray(bool[] buffer) {
      return Array.emptyBooleanArray;
    }

    public override long[] GetLongArray(long[] buffer) {
      return Array.emptyLongArray;
    }

    public override double[] GetDoubleArray(double[] buffer) {
      return Array.emptyDoubleArray;
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      return Array.emptyObjArray;
    }

    public override byte[] GetByteArray() {
      return Array.emptyByteArray;
    }

    public override NeSeqObj Append(Obj obj) {
      if (obj.IsInt())
        return Append(obj.GetLong());
      else if (obj.IsFloat())
        return Append(obj.GetDouble());
      else
        return ArrayObjs.CreateRightPadded(obj);
    }

    public override NeSeqObj Append(long value) {
      return IntArrayObjs.CreateRightPadded(value);
    }

    public override NeSeqObj Append(double value) {
      return FloatArrayObjs.CreateRightPadded(value);
    }

    public override SeqObj Concat(Obj seq) {
      return (SeqObj) seq;
    }

    public override SeqObj Reverse() {
      return this;
    }

    public override NeSeqObj UpdatedAt(long idx, Obj obj) {
      throw ErrorHandler.SoftFail("Invalid sequence index: " + idx.ToString());
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      return 0;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.EMPTY_SEQ;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.EmptySeqObj(this);
    }
  }
}