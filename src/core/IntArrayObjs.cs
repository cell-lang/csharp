namespace Cell.Runtime {
  public class IntArrayObjs {
    internal static SignedByteArrayObj Create(sbyte[] data) {
      Debug.Assert(data.Length > 0);
      return new SignedByteArrayObj(data);
    }

    internal static UnsignedByteArrayObj Create(byte[] data) {
      Debug.Assert(data.Length > 0);
      return new UnsignedByteArrayObj(data);
    }

    internal static ShortArrayObj Create(short[] data) {
      Debug.Assert(data.Length > 0);
      return new ShortArrayObj(data);
    }

    internal static Int32ArrayObj Create(int[] data) {
      Debug.Assert(data.Length > 0);
      return new Int32ArrayObj(data);
    }

    internal static IntArrayObj Create(long[] data) {
      Debug.Assert(data.Length > 0);
      return new IntArrayObj(data);
    }

    internal static SignedByteArrayObj Create(sbyte[] data, int length) {
      Debug.Assert(length > 0);
      return new SignedByteArrayObj(Array.Take(data, length));
    }

    internal static UnsignedByteArrayObj Create(byte[] data, int length) {
      Debug.Assert(length > 0);
      return new UnsignedByteArrayObj(Array.Take(data, length));
    }

    internal static ShortArrayObj Create(short[] data, int length) {
      Debug.Assert(length > 0);
      return new ShortArrayObj(Array.Take(data, length));
    }

    internal static Int32ArrayObj Create(int[] data, int length) {
      Debug.Assert(length > 0);
      return new Int32ArrayObj(Array.Take(data, length));
    }

    internal static IntArrayObj Create(long[] data, int length) {
      Debug.Assert(length > 0);
      return new IntArrayObj(Array.Take(data, length));
    }

    internal static IntArraySliceObj CreateRightPadded(long value) {
      return PaddedIntArray.Create(value);
    }

    internal static IntArraySliceObj Append(NeIntSeqObj seq, long value) {
      return PaddedIntArray.Create(seq, value);
    }

    internal static IntArraySliceObj Concat(NeIntSeqObj left, NeIntSeqObj right) {
      return PaddedIntArray.Create(left, right);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class IntArrayObjBase : NeIntSeqObj {
    protected long[] longs;
    protected int    offset;
    Obj[]  objs;


    public override SeqObj Reverse() {
      int length = GetSize();
      int last = offset + length - 1;
      long[] revData = new long[length];
      for (int i=0 ; i < length ; i++)
        revData[i] = longs[last-i];
      return new IntArrayObj(revData);
    }

    public override long[] GetLongArray(long[] buffer) {
      if (longs == null) {
        int length = GetSize();
        longs = new long[length];
        Copy(0, length, longs, 0);
      }
      return longs;
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      if (objs == null) {
        int length = GetSize();
        objs = new Obj[length];
        Copy(0, length, objs, 0);
      }
      return objs;
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (count == 0)
        return EmptySeqObj.singleton;
      return new IntArraySliceObj(null, longs, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      if (other is IntArrayObjBase) {
        Debug.Assert(GetSize() == other.GetSize());

        IntArrayObjBase otherArray = (IntArrayObjBase) other;

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

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, long[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = longs[srcOffset+i];
    }

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = IntObj.Get(longs[srcOffset+i]);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class IntArrayObj : IntArrayObjBase {
    public IntArrayObj(long[] elts) {
      data = SeqObjData((uint) elts.Length);
      extraData = NeSeqObjExtraData();
      longs = elts;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.IntArrayObj(this);
    }

    public override long GetLongAt(long idx) {
      return longs[(int) idx];
    }

    public override Obj PackForString() {
      int len = longs.Length;

      long min = 0, max = 0;
      for (int i=0 ; i < len ; i++) {
        long elt = longs[i];
        min = elt < min ? elt : min;
        max = elt > max ? elt : max;
        if (min < -2147483648 | max > 2147483647)
          return this;
      }

      if (min >= 0 & max <= 255) {
        byte[] bytes = new byte[len];
        for (int i=0 ; i < len ; i++)
          bytes[i] = (byte) longs[i];
        return new UnsignedByteArrayObj(bytes);
      }

      if (min >= -32768 & max < 32768) {
        short[] shorts = new short[len];
        for (int i=0 ; i < len ; i++)
          shorts[i] = (short) longs[i];
        return new ShortArrayObj(shorts);
      }

      int[] ints = new int[len];
      for (int i=0 ; i < len ; i++)
        ints[i] = (int) longs[i];
      return new Int32ArrayObj(ints);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class IntArraySliceObj : IntArrayObjBase {
    PaddedIntArray source;


    public IntArraySliceObj(PaddedIntArray source, long[] elts, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      longs = elts;
      this.offset = offset;
      this.source = source;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.IntArraySliceObj(this);
    }

    public override long GetLongAt(long idx) {
      int len = GetSize();
      if (idx >= 0 & idx < len)
        return longs[offset + (int) idx];
      else
        throw ErrorHandler.InvalidIndexSoftFail(idx, len);
    }

    public override long[] GetLongArray(long[] buffer) {
      //## WHY IS buffer IGNORED HERE?
      int length = GetSize();
      long[] output = new long[length];
      Copy(0, length, output, 0);
      return output;
    }

    public override NeSeqObj Append(long value) {
      return source != null ? source.Append(offset+GetSize(), value) : base.Append(value);
    }

    public override NeIntSeqObj Concat(NeIntSeqObj seq) {
      return source != null ? source.Concat(offset+GetSize(), seq) : base.Concat(seq);
    }

    public override Obj PackForString() {
      int len = GetSize();

      long min = 0, max = 0;
      for (int i=0 ; i < len ; i++) {
        long elt = longs[offset+i];
        min = elt < min ? elt : min;
        max = elt > max ? elt : max;
        if (min < -2147483648 | max > 2147483647)
          return this;
      }

      if (min >= 0 & max <= 255) {
        byte[] bytes = new byte[len];
        for (int i=0 ; i < len ; i++)
          bytes[i] = (byte) longs[offset+i];
        return new UnsignedByteArrayObj(bytes);
      }

      if (min >= -32768 & max < 32768) {
        short[] shorts = new short[len];
        for (int i=0 ; i < len ; i++)
          shorts[i] = (short) longs[offset+i];
        return new ShortArrayObj(shorts);
      }

      int[] ints = new int[len];
      for (int i=0 ; i < len ; i++)
        ints[i] = (int) longs[offset+i];
      return new Int32ArrayObj(ints);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class PaddedIntArray {
    long[] buffer;
    int used;

    PaddedIntArray(long[] buffer, int used) {
      this.buffer = buffer;
      this.used = used;
    }

    public IntArraySliceObj Slice(int offset, int length) {
      return new IntArraySliceObj(this, buffer, offset, length);
    }

    public /*synchronized*/ IntArraySliceObj Append(int idx, long value) {
      if (idx == buffer.Length) {
        // We run out of space, expanding the array buffer
        int size = buffer.Length;
        int newSize = 2 * size;
        long[] newBuffer = new long[newSize];
        for (int i=0 ; i < size ; i++)
          newBuffer[i] = buffer[i];
        newBuffer[idx] = value;
        PaddedIntArray newArray = new PaddedIntArray(newBuffer, idx+1);
        return newArray.Slice(0, idx+1);
        //## THINK ABOUT THIS. WOULD IT WORK?
        // buffer = newBuffer;
        // used++;
        // return new IntArraySliceObj(this, buffer, 0, used);
      }
      else if (idx == used) {
        // There's space for the new element
        buffer[idx] = value;
        used++;
        return new IntArraySliceObj(this, buffer, 0, used);
      }
      else {
        // The next slot was already taken. This is supposed to happen only rarely
        Debug.Assert(idx < used & idx < buffer.Length);

        long[] newBuffer = new long[buffer.Length];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        newBuffer[idx] = value;
        PaddedIntArray newArray = new PaddedIntArray(newBuffer, idx+1);
        return newArray.Slice(0, idx+1);
      }
    }

    public /*synchronized*/ IntArraySliceObj Concat(int idx, NeIntSeqObj seq) {
      int seqLen = seq.GetSize();
      int newLen = idx + seqLen;

      if (newLen > buffer.Length) {
        // We run out of space, expanding the array buffer
        int size = MinBufferSize(newLen);
        long[] newBuffer = new long[size];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        seq.Copy(0, seqLen, newBuffer, idx);
        PaddedIntArray newArray = new PaddedIntArray(newBuffer, newLen);
        return newArray.Slice(0, newLen);
        //## THINK ABOUT THIS. WOULD IT WORK?
        // buffer = newBuffer;
        // used = newLen;
        // return new IntArraySliceObj(this, buffer, 0, used);
      }
      else if (idx == used) {
        // There's space for the new elements
        seq.Copy(0, seqLen, buffer, idx);
        used = newLen;
        return new IntArraySliceObj(this, buffer, 0, used);
      }
      else {
        // The next slot was already taken. This is supposed to happen only rarely
        Debug.Assert(idx < used & idx < buffer.Length);

        long[] newBuffer = new long[buffer.Length];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        seq.Copy(0, seqLen, newBuffer, idx);
        PaddedIntArray newArray = new PaddedIntArray(newBuffer, newLen);
        return newArray.Slice(0, newLen);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public static IntArraySliceObj Create(long value) {
      long[] buffer = new long[32];
      buffer[0] = value;
      PaddedIntArray paddedArray = new PaddedIntArray(buffer, 1);
      return paddedArray.Slice(0, 1);
    }

    public static IntArraySliceObj Create(NeIntSeqObj seq, long value) {
      int len = seq.GetSize();
      int size = MinBufferSize(len+1);
      long[] buffer = new long[size];
      seq.Copy(0, len, buffer, 0);
      buffer[len] = value;
      PaddedIntArray paddedArray = new PaddedIntArray(buffer, len+1);
      return paddedArray.Slice(0, len+1);
    }

    public static IntArraySliceObj Create(NeIntSeqObj left, NeIntSeqObj right) {
      int leftLen = left.GetSize();
      int rightLen = right.GetSize();
      int len = leftLen + rightLen;
      int size = MinBufferSize(len);
      long[] buffer = new long[size];
      left.Copy(0, leftLen, buffer, 0);
      right.Copy(0, rightLen, buffer, leftLen);
      PaddedIntArray paddedArray = new PaddedIntArray(buffer, len);
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

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class SignedByteArrayObjBase : IntArrayObjBase {
    protected sbyte[] bytes;


    public override SeqObj Reverse() {
      int length = GetSize();
      int last = offset + length - 1;
      sbyte[] revData = new sbyte[length];
      for (int i=0 ; i < length ; i++)
        revData[i] = bytes[last-i];
      return new SignedByteArrayObj(revData);
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (count == 0)
        return EmptySeqObj.singleton;
      return new SignedByteArraySliceObj(bytes, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, long[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = bytes[srcOffset+i];
    }

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = IntObj.Get(bytes[srcOffset+i]);
    }
  }


  public sealed class SignedByteArrayObj : SignedByteArrayObjBase {
    public SignedByteArrayObj(sbyte[] elts) {
      data = SeqObjData((uint) elts.Length);
      extraData = NeSeqObjExtraData();
      bytes = elts;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.SignedByteArrayObj(this);
    }

    public override long GetLongAt(long idx) {
      return bytes[(int) idx];
    }
  }


  public sealed class SignedByteArraySliceObj : SignedByteArrayObjBase {
    public SignedByteArraySliceObj(sbyte[] elts, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      bytes = elts;
      this.offset = offset;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.SignedByteArraySliceObj(this);
    }

    public override long GetLongAt(long idx) {
      int len = GetSize();
      if (idx >= 0 & idx < len)
        return bytes[offset + (int) idx];
      else
        throw ErrorHandler.InvalidIndexSoftFail(idx, len);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class UnsignedByteArrayObjBase : IntArrayObjBase {
    protected byte[] bytes;


    public override SeqObj Reverse() {
      int length = GetSize();
      int last = offset + length - 1;
      byte[] revData = new byte[length];
      for (int i=0 ; i < length ; i++)
        revData[i] = bytes[last-i];
      return new UnsignedByteArrayObj(revData);
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (count == 0)
        return EmptySeqObj.singleton;
      return new UnsignedByteArraySliceObj(bytes, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, long[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = bytes[srcOffset + i];
    }

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = IntObj.Get(bytes[srcOffset+i]);
    }
  }


  public sealed class UnsignedByteArrayObj : UnsignedByteArrayObjBase {
    public UnsignedByteArrayObj(byte[] elts) {
      data = SeqObjData((uint) elts.Length);
      extraData = NeSeqObjExtraData();
      bytes = elts;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.UnsignedByteArrayObj(this);
    }

    public override long GetLongAt(long idx) {
      return bytes[(int) idx];
    }

    public override byte[] GetByteArray() {
      return bytes;
    }
  }


  public sealed class UnsignedByteArraySliceObj : UnsignedByteArrayObjBase {
    public UnsignedByteArraySliceObj(byte[] elts, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      bytes = elts;
      this.offset = offset;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.UnsignedByteArraySliceObj(this);
    }

    public override long GetLongAt(long idx) {
      int len = GetSize();
      if (idx >= 0 & idx < len)
        return bytes[offset + (int) idx];
      else
        throw ErrorHandler.InvalidIndexSoftFail(idx, len);
    }

    public override byte[] GetByteArray() {
      int len = GetSize();
      if (offset == 0 & bytes.Length == len)
        return bytes;
      byte[] buffer = new byte[len];
      for (int i=0 ; i < len ; i++)
        buffer[i] = bytes[offset + i];
      return buffer;
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class ShortArrayObjBase : IntArrayObjBase {
    protected short[] shorts;


    public override SeqObj Reverse() {
      int length = GetSize();
      int last = offset + length - 1;
      short[] revData = new short[length];
      for (int i=0 ; i < length ; i++)
        revData[i] = shorts[last-i];
      return new ShortArrayObj(revData);
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (count == 0)
        return EmptySeqObj.singleton;
      return new ShortArraySliceObj(shorts, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, long[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = shorts[srcOffset+i];
    }

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = IntObj.Get(shorts[srcOffset+i]);
    }
  }


  public sealed class ShortArrayObj : ShortArrayObjBase {
    public ShortArrayObj(short[] elts) {
      data = SeqObjData((uint) elts.Length);
      extraData = NeSeqObjExtraData();
      shorts = elts;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.ShortArrayObj(this);
    }

    public override long GetLongAt(long idx) {
      return shorts[(int) idx];
    }
  }


  public sealed class ShortArraySliceObj : ShortArrayObjBase {
    public ShortArraySliceObj(short[] elts, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      shorts = elts;
      this.offset = offset;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.ShortArraySliceObj(this);
    }

    public override long GetLongAt(long idx) {
      int len = GetSize();
      if (idx >= 0 & idx < len)
        return shorts[offset + (int) idx];
      else
        throw ErrorHandler.InvalidIndexSoftFail(idx, len);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class Int32ArrayObjBase : IntArrayObjBase {
    protected int[] ints;


    public override SeqObj Reverse() {
      int length = GetSize();
      int last = offset + length - 1;
      int[] revData = new int[length];
      for (int i=0 ; i < length ; i++)
        revData[i] = ints[last-i];
      return new Int32ArrayObj(revData);
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (count == 0)
        return EmptySeqObj.singleton;
      return new Int32ArraySliceObj(ints, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, long[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = ints[srcOffset+i];
    }

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = IntObj.Get(ints[srcOffset+i]);
    }
  }


  public sealed class Int32ArrayObj : Int32ArrayObjBase {
    public Int32ArrayObj(int[] elts) {
      data = SeqObjData((uint) elts.Length);
      extraData = NeSeqObjExtraData();
      ints = elts;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.Int32ArrayObj(this);
    }

    public override long GetLongAt(long idx) {
      return ints[(int) idx];
    }
  }


  public sealed class Int32ArraySliceObj : Int32ArrayObjBase {
    public Int32ArraySliceObj(int[] elts, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      ints = elts;
      this.offset = offset;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.Int32ArraySliceObj(this);
    }

    public override long GetLongAt(long idx) {
      int len = GetSize();
      if (idx >= 0 & idx < len)
        return ints[offset + (int) idx];
      else
        throw ErrorHandler.InvalidIndexSoftFail(idx, len);
    }
  }
}
