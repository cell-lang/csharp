namespace Cell.Runtime {
  public class FloatArrayObjs {
    internal static NeSeqObj Create(double[] values) {
      Debug.Assert(values.Length > 0);
      return new FloatArrayObj(values);
    }

    internal static NeSeqObj Create(double[] values, int length) {
      Debug.Assert(length > 0);
      return new FloatArrayObj(Array.Take(values, length));
    }

    internal static NeSeqObj CreateRightPadded(double value) {
      return PaddedFloatArray.Create(value);
    }

    internal static FloatArraySliceObj Append(NeFloatSeqObj seq, double value) {
      return PaddedFloatArray.Create(seq, value);
    }

    internal static FloatArraySliceObj Concat(NeFloatSeqObj left, NeFloatSeqObj right) {
      return PaddedFloatArray.Create(left, right);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class FloatArrayObjBase : NeFloatSeqObj {
    protected int offset;
    protected double[] elts;


    public override SeqObj Reverse() {
      int length = GetSize();
      int last = offset + length - 1;
      double[] revData = new double[length];
      for (int i=0 ; i < length ; i++)
        revData[i] = elts[last-i];
      return new FloatArrayObj(revData);
    }

    public override double[] GetDoubleArray(double[] buffer) {
      int len = GetSize();
      if (len == elts.Length)
        return elts;
      if (buffer == null)
        buffer = new double[len];
      Copy(0, len, buffer, 0);
      return buffer;
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      int len = GetSize();
      if (buffer == null)
        buffer = new Obj[len];
      Copy(0, len, buffer, 0);
      return buffer;
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (len == 0)
        return EmptySeqObj.singleton;
      return new FloatArraySliceObj(null, elts, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      if (other is FloatArrayObjBase) {
        Debug.Assert(GetSize() == other.GetSize());

        FloatArrayObjBase otherArray = (FloatArrayObjBase) other;

        int len = GetSize();
        int otherOffset = otherArray.offset;
        double[] otherDoubles = otherArray.elts;
        for (int i=0 ; i < len ; i++) {
          ulong elt = FloatObjData(elts[offset + i]);
          ulong otherElt = FloatObjData(otherDoubles[otherOffset + i]);
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
        ulong code = 0;
        int len = GetSize();
        for (int i=0 ; i < len ; i++)
          code = 31 * code + FloatObj.Hashcode(elts[offset + i]);
        hcode = Hashing.Hashcode64(code);
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, double[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset + i] = elts[srcOffset + i];
    }

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset + i] = new FloatObj(elts[srcOffset + i]);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class FloatArrayObj : FloatArrayObjBase {
    public FloatArrayObj(double[] elts) {
      data = SeqObjData((uint) elts.Length);
      extraData = NeSeqObjExtraData();
      this.elts = elts;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.FloatArrayObj(this);
    }

    public override double GetDoubleAt(long idx) {
      return elts[(int) idx];
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class FloatArraySliceObj : FloatArrayObjBase {
    PaddedFloatArray source;


    public FloatArraySliceObj(PaddedFloatArray source, double[] elts, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      this.elts = elts;
      this.offset = offset;
      this.source = source;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.FloatArraySliceObj(this);
    }

    public override double GetDoubleAt(long idx) {
      if (idx >= 0 & idx < GetSize())
        return elts[offset + (int) idx];
      else
        throw ErrorHandler.SoftFail("Array index out of bounds");
    }

    public override NeSeqObj Append(double value) {
      return source != null ? source.Append(offset + GetSize(), value) : base.Append(value);
    }

    public override NeFloatSeqObj Concat(NeFloatSeqObj seq) {
      return source != null ? source.Concat(offset + GetSize(), seq) : base.Concat(seq);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class PaddedFloatArray {
    double[] buffer;
    int used;

    PaddedFloatArray(double[] buffer, int used) {
      this.buffer = buffer;
      this.used = used;
    }

    public FloatArraySliceObj Slice(int offset, int length) {
      return new FloatArraySliceObj(this, buffer, offset, length);
    }

    public /*synchronized*/ FloatArraySliceObj Append(int idx, double value) {
      if (idx == buffer.Length) {
        // We run out of space, expanding the array buffer
        int size = buffer.Length;
        int newSize = 2 * size;
        double[] newBuffer = new double[newSize];
        for (int i=0 ; i < size ; i++)
          newBuffer[i] = buffer[i];
        newBuffer[idx] = value;
        PaddedFloatArray newArray = new PaddedFloatArray(newBuffer, idx + 1);
        return newArray.Slice(0, idx + 1);
        //## THINK ABOUT THIS. WOULD IT WORK?
        // buffer = newBuffer;
        // used++;
        // return new FloatArraySliceObj(this, buffer, 0, used);
      }
      else if (idx == used) {
        // There's space for the new element
        buffer[idx] = value;
        used++;
        return new FloatArraySliceObj(this, buffer, 0, used);
      }
      else {
        // The next slot was already taken. This is supposed to happen only rarely
        Debug.Assert(idx < used & idx < buffer.Length);

        double[] newBuffer = new double[buffer.Length];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        newBuffer[idx] = value;
        PaddedFloatArray newArray = new PaddedFloatArray(newBuffer, idx + 1);
        return newArray.Slice(0, idx + 1);
      }
    }

    public /*synchronized*/ FloatArraySliceObj Concat(int idx, NeFloatSeqObj seq) {
      int seqLen = seq.GetSize();
      int newLen = idx + seqLen;

      if (newLen > buffer.Length) {
        // We run out of space, expanding the array buffer
        int size = MinBufferSize(newLen);
        double[] newBuffer = new double[size];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        seq.Copy(0, seqLen, newBuffer, idx);
        PaddedFloatArray newArray = new PaddedFloatArray(newBuffer, newLen);
        return newArray.Slice(0, newLen);
        //## THINK ABOUT THIS. WOULD IT WORK?
        // buffer = newBuffer;
        // used = newLen;
        // return new FloatArraySliceObj(this, buffer, 0, used);
      }
      else if (idx == used) {
        // There's space for the new elements
        seq.Copy(0, seqLen, buffer, idx);
        used = newLen;
        return new FloatArraySliceObj(this, buffer, 0, used);
      }
      else {
        // The next slot was already taken. This is supposed to happen only rarely
        Debug.Assert(idx < used & idx < buffer.Length);

        double[] newBuffer = new double[buffer.Length];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        seq.Copy(0, seqLen, newBuffer, idx);
        PaddedFloatArray newArray = new PaddedFloatArray(newBuffer, newLen);
        return newArray.Slice(0, newLen);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public static FloatArraySliceObj Create(double value) {
      double[] buffer = new double[32];
      buffer[0] = value;
      PaddedFloatArray paddedArray = new PaddedFloatArray(buffer, 1);
      return paddedArray.Slice(0, 1);
    }

    public static FloatArraySliceObj Create(NeFloatSeqObj seq, double value) {
      int len = seq.GetSize();
      int size = MinBufferSize(len + 1);
      double[] buffer = new double[size];
      seq.Copy(0, len, buffer, 0);
      buffer[len] = value;
      PaddedFloatArray paddedArray = new PaddedFloatArray(buffer, len + 1);
      return paddedArray.Slice(0, len + 1);
    }

    public static FloatArraySliceObj Create(NeFloatSeqObj left, NeFloatSeqObj right) {
      int leftLen = left.GetSize();
      int rightLen = right.GetSize();
      int len = leftLen + rightLen;
      int size = MinBufferSize(len);
      double[] buffer = new double[size];
      left.Copy(0, leftLen, buffer, 0);
      right.Copy(0, rightLen, buffer, leftLen);
      PaddedFloatArray paddedArray = new PaddedFloatArray(buffer, len);
      return paddedArray.Slice(0, len);
    }

    public static int MinBufferSize(int len) {
      int minSize = (5 * len) / 4;
      int size = 32;
      while (size < minSize)
        size = 2 * size;
      return size;
    }
  }
}
