namespace Cell.Runtime {
  public abstract class NeIntSeqObj : NeSeqObj {
    public override bool IsNeIntSeq() {
      return true;
    }

    public override bool IsNeFloatSeq() {
      return false;
    }

    public override Obj GetObjAt(long idx) {
      return IntObj.Get(GetLongAt(idx));
    }

    public override NeSeqObj Append(Obj obj) {
      return obj.IsInt() ? Append(obj.GetLong()) : base.Append(obj);
    }

    public override NeSeqObj Append(long value) {
      return IntArrayObjs.Append(this, value);
    }

    public override SeqObj Concat(Obj seq) {
      return seq is NeIntSeqObj ? Concat((NeIntSeqObj) seq) : base.Concat(seq);
    }

    public virtual NeIntSeqObj Concat(NeIntSeqObj seq) {
      return IntArrayObjs.Concat(this, seq);
    }

    public override int InternalOrder(Obj other) {
      if (other is NeIntSeqObj) {
        Debug.Assert(GetSize() == other.GetSize());

        int len = GetSize();
        for (int i=0 ; i < len ; i++) {
          long elt = GetLongAt(i);
          long otherElt = other.GetLongAt(i);
          if (elt != otherElt)
            return elt < otherElt ? -1 : 1;
        }
        return 0;
      }
      else
        return base.InternalOrder(other);
    }

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        long code = 0;
        int len = GetSize();
        for (int i=0 ; i < len ; i++)
          code = 31 * code + IntObj.Hashcode(GetLongAt(i));
        hcode = Hashing.Hashcode64(code);
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    //////////////////////////////////////////////////////////////////////////////

    public abstract void Copy(int first, int count, long[] buffer, int destOffset);
  }
}
