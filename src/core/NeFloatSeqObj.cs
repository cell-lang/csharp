namespace Cell.Runtime {
  public abstract class NeFloatSeqObj : NeSeqObj {
    public override bool IsNeIntSeq() {
      return false;
    }

    public override bool IsNeFloatSeq() {
      return true;
    }

    public override Obj GetObjAt(long idx) {
      return new FloatObj(GetDoubleAt(idx));
    }

    public override NeSeqObj Append(Obj obj) {
      return obj.IsFloat() ? Append(obj.GetDouble()) : base.Append(obj);
    }

    public override NeSeqObj Append(double value) {
      return FloatArrayObjs.Append(this, value);
    }

    public override SeqObj Concat(Obj seq) {
      return seq is NeFloatSeqObj ? Concat((NeFloatSeqObj) seq) : base.Concat(seq);
    }

    public virtual NeFloatSeqObj Concat(NeFloatSeqObj seq) {
      return FloatArrayObjs.Concat(this, seq);
    }

    //////////////////////////////////////////////////////////////////////////////

    public abstract void Copy(int first, int count, double[] buffer, int destOffset);
  }
}