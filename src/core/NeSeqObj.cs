namespace Cell.Runtime {
  public abstract class NeSeqObj : SeqObj {
    protected uint hcode = Hashing.NULL_HASHCODE;

    //////////////////////////////////////////////////////////////////////////////

    public override bool IsNeIntSeq() {
      int len = GetSize();
      for (int i=0 ; i < len ; i++)
        if (!GetObjAt(i).IsInt())
          return false;
      return true;
    }

    public override bool IsNeFloatSeq() {
      int len = GetSize();
      for (int i=0 ; i < len ; i++)
        if (!GetObjAt(i).IsFloat())
          return false;
      return true;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool[] GetBoolArray(bool[] buffer) {
      int len = GetSize();
      if (buffer == null)
        buffer = new bool[len];
      for (int i=0 ; i < len ; i++)
        buffer[i] = GetBoolAt(i);
      return buffer;
    }

    public override long[] GetLongArray(long[] buffer) {
      int len = GetSize();
      if (buffer == null)
        buffer = new long[len];
      for (int i=0 ; i < len ; i++)
        buffer[i] = GetLongAt(i);
      return buffer;
    }

    public override double[] GetDoubleArray(double[] buffer) {
      int len = GetSize();
      if (buffer == null)
        buffer = new double[len];
      for (int i=0 ; i < len ; i++)
        buffer[i] = GetDoubleAt(i);
      return buffer;
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      int len = GetSize();
      if (buffer == null)
        buffer = new Obj[len];
      for (int i=0 ; i < len ; i++)
        buffer[i] = GetObjAt(i);
      return buffer;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override byte[] GetByteArray() {
      int len = GetSize();
      byte[] bytes = new byte[len];
      for (int i=0 ; i < len ; i++) {
        long value = GetLongAt(i);
        if (value < 0 | value > 255)
          throw ErrorHandler.InternalFail();
        bytes[i] = (byte) value;
      }
      return bytes;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override SeqIter GetSeqIter() {
      Obj[] elts = GetObjArray();
      return new SeqIter(elts, 0, elts.Length-1);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override NeSeqObj Append(Obj obj) {
      return ArrayObjs.Append(this, obj);
    }

    public override SeqObj Concat(Obj seq) {
      return seq.GetSize() != 0 ? ArrayObjs.Concat(this, (NeSeqObj) seq) : this;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override NeSeqObj UpdatedAt(long idx, Obj obj) {
      int len = GetSize();

      if (idx < 0 | idx >= len)
        ErrorHandler.SoftFail("Invalid sequence index");

      Obj[] newItems = new Obj[len];
      for (int i=0 ; i < len ; i++)
        newItems[i] = i == idx ? obj : GetObjAt(i);

      return ArrayObjs.Create(newItems);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      Debug.Assert(other is NeSeqObj && GetSize() == other.GetSize());

      int len = GetSize();
      for (int i=0 ; i < len ; i++) {
        int ord = GetObjAt(i).QuickOrder(other.GetObjAt(i));
        if (ord != 0)
          return ord;
      }
      return 0;
    }

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        ulong code = 0;
        int len = GetSize();
        for (int i=0 ; i < len ; i++)
          code = 31 * code + GetObjAt(i).Hashcode();
        hcode = Hashing.Hashcode64(code);
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.NE_SEQ;
    }

    //////////////////////////////////////////////////////////////////////////////

    public int PackedRanges(int minSize, int offset, int[] offsets, NeSeqObj[] ranges, int writeOffset) {
      if (GetSize() >= minSize) {
        offsets[writeOffset] = offset;
        ranges[writeOffset++] = this;
      }
      return writeOffset;
    }

    public int Depth() {
      return 0;
    }

    //////////////////////////////////////////////////////////////////////////////

    public abstract void Copy(int first, int count, Obj[] buffer, int destOffset);
  }
}