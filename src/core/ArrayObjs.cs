namespace Cell.Runtime {
  public class ArrayObjs {
    internal static NeSeqObj Create(Obj[] objs) {
      Debug.Assert(objs.Length > 0);
      return new ArrayObj(objs);
    }

    internal static NeSeqObj Create(Obj[] objs, int length) {
      Debug.Assert(length > 0);
      return new ArrayObj(Array.Take(objs, length));
    }

    internal static NeSeqObj CreateRightPadded(Obj obj) {
      return PaddedArray.Create(obj);
    }

    internal static ArraySliceObj Append(NeSeqObj seq, Obj obj) {
      return PaddedArray.Create(seq, obj);
    }

    internal static ArraySliceObj Concat(NeSeqObj left, NeSeqObj right) {
      return PaddedArray.Create(left, right);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public abstract class ArrayObjBase : NeSeqObj {
    protected Obj[] objs;
    protected int   offset;


    public override SeqObj Reverse() {
      int len = GetSize();
      int last = offset + len - 1;
      Obj[] revObjs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        revObjs[i] = objs[last-i];
      return new ArrayObj(revObjs);
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      int len = GetSize();
      if (objs.Length != len) {
        objs = Array.Subarray(objs, offset, offset+len);
        offset = 0;
      }
      return objs;
    }

    public override SeqObj GetSlice(long first, long count) {
      int len = GetSize();
      if (first < 0 | first > len | count < 0 | count > len | first + count > len)
        throw ErrorHandler.InvalidRangeSoftFail(first, first + count, len);
      if (count == 0)
        return EmptySeqObj.singleton;
      return new ArraySliceObj(null, objs, offset + (int) first, (int) count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override void Copy(int first, int count, Obj[] array, int destOffset) {
      int srcOffset = offset + first;
      for (int i=0 ; i < count ; i++)
        array[destOffset+i] = objs[srcOffset+i];
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class ArrayObj : ArrayObjBase {
    public ArrayObj(Obj[] objs) {
      int len = objs.Length;
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      this.objs = objs;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.ArrayObj(this);
    }

    public override Obj GetObjAt(long idx) {
      return objs[(int) idx];
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class ArraySliceObj : ArrayObjBase {
    PaddedArray source;

    public ArraySliceObj(PaddedArray source, Obj[] objs, int offset, int len) {
      data = SeqObjData((uint) len);
      extraData = NeSeqObjExtraData();
      this.objs = objs;
      this.offset = offset;
      this.source = source;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.ArraySliceObj(this);
    }

    public override Obj GetObjAt(long idx) {
      int len = GetSize();
      if (idx >= 0 & idx < len)
        return objs[offset + (int) idx];
      else
        throw ErrorHandler.InvalidIndexSoftFail(idx, len);
    }

    public override NeSeqObj Append(Obj obj) {
      if (source != null)
        return source.Append(offset + GetSize(), obj);
      else
        return base.Append(obj);
    }

    public override SeqObj Concat(Obj seq) {
      if (source != null && seq is NeSeqObj)
        return source.Concat(offset + GetSize(), (NeSeqObj) seq);
      else
        return base.Concat(seq);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class PaddedArray {
    Obj[] buffer;
    int used;

    PaddedArray(Obj[] buffer, int used) {
      this.buffer = buffer;
      this.used = used;
    }

    public ArraySliceObj Slice(int offset, int length) {
      return new ArraySliceObj(this, buffer, offset, length);
    }

    public /*synchronized*/ ArraySliceObj Append(int idx, Obj obj) {
      if (idx == buffer.Length) {
        // We run out of space, expanding the array buffer
        int size = buffer.Length;
        int newSize = 2 * size;
        Obj[] newBuffer = new Obj[newSize];
        for (int i=0 ; i < size ; i++)
          newBuffer[i] = buffer[i];
        newBuffer[idx] = obj;
        PaddedArray newArray = new PaddedArray(newBuffer, idx+1);
        return newArray.Slice(0, idx+1);

        //## THINK ABOUT THIS. WOULD IT WORK?
        // buffer = newBuffer;
        // used++;
        // return new ArraySliceObj(this, buffer, 0, used);
      }
      else if (idx == used) {
        // There's space for the new element
        buffer[idx] = obj;
        used++;
        return new ArraySliceObj(this, buffer, 0, used);
      }
      else {
        // The next slot was already taken. This is supposed to happen only rarely
        Debug.Assert(idx < used & idx < buffer.Length);

        Obj[] newBuffer = new Obj[buffer.Length];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        newBuffer[idx] = obj;
        PaddedArray newArray = new PaddedArray(newBuffer, idx+1);
        return newArray.Slice(0, idx+1);
      }
    }

    public /*synchronized*/ ArraySliceObj Concat(int idx, NeSeqObj seq) {
      int seqLen = seq.GetSize();
      int newLen = idx + seqLen;

      if (newLen > buffer.Length) {
        // We run out of space, expanding the array buffer
        int size = MinBufferSize(newLen);
        Obj[] newBuffer = new Obj[size];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        seq.Copy(0, seqLen, newBuffer, idx);
        PaddedArray newArray = new PaddedArray(newBuffer, newLen);
        return newArray.Slice(0, newLen);
        //## THINK ABOUT THIS. WOULD IT WORK?
        // buffer = newBuffer;
        // used = newLen;
        // return new ArraySliceObj(this, buffer, 0, used);
      }
      else if (idx == used) {
        // There's space for the new elements
        seq.Copy(0, seqLen, buffer, idx);
        used = newLen;
        return new ArraySliceObj(this, buffer, 0, used);
      }
      else {
        // The next slot was already taken. This is supposed to happen only rarely
        Debug.Assert(idx < used & idx < buffer.Length);

        Obj[] newBuffer = new Obj[buffer.Length];
        for (int i=0 ; i < idx ; i++)
          newBuffer[i] = buffer[i];
        seq.Copy(0, seqLen, newBuffer, idx);
        PaddedArray newArray = new PaddedArray(newBuffer, newLen);
        return newArray.Slice(0, newLen);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ArraySliceObj Create(Obj obj) {
      Obj[] buffer = new Obj[32];
      buffer[0] = obj;
      PaddedArray paddedArray = new PaddedArray(buffer, 1);
      return paddedArray.Slice(0, 1);
    }

    public static ArraySliceObj Create(NeSeqObj seq, Obj obj) {
      int len = seq.GetSize();
      int size = MinBufferSize(len+1);
      Obj[] buffer = new Obj[size];
      seq.Copy(0, len, buffer, 0);
      buffer[len] = obj;
      PaddedArray paddedArray = new PaddedArray(buffer, len+1);
      return paddedArray.Slice(0, len+1);
    }

    public static ArraySliceObj Create(NeSeqObj left, NeSeqObj right) {
      int leftLen = left.GetSize();
      int rightLen = right.GetSize();
      int len = leftLen + rightLen;
      int size = MinBufferSize(len);
      Obj[] buffer = new Obj[size];
      left.Copy(0, leftLen, buffer, 0);
      right.Copy(0, rightLen, buffer, leftLen);
      PaddedArray paddedArray = new PaddedArray(buffer, len);
      return paddedArray.Slice(0, len);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static int MinBufferSize(int len) {
      int minSize = (5 * len) / 4;
      int size = 32;
      while (size < minSize)
        size = 2 * size;
      return size;
    }
  }
}
