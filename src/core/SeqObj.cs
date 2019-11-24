namespace Cell.Runtime {
  public abstract class SeqObj : Obj {
    public override bool GetBoolAt(long idx) {
      return GetObjAt(idx).GetBool();
    }

    public override long GetLongAt(long idx) {
      return GetObjAt(idx).GetLong();
    }

    public override double GetDoubleAt(long idx) {
      return GetObjAt(idx).GetDouble();
    }

    public override NeSeqObj Append(bool value) {
      return Append(SymbObj.Get(value));
    }

    public override NeSeqObj Append(long value) {
      return Append(IntObj.Get(value));
    }

    public override NeSeqObj Append(double value) {
      return Append(new FloatObj(value));
    }
  }
}
