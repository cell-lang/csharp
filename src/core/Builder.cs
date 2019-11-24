using System.Collections.Generic;


namespace Cell.Runtime {
  public class Builder {
    public static Obj CreateSet(Obj[] objs) {
      return CreateSet(objs, objs.Length);
    }

    public static Obj CreateSet(List<Obj> objs) {
      return CreateSet(objs.ToArray(), objs.Count);
    }

    public static Obj CreateSet(Obj[] objs, long count) {
      Debug.Assert(objs.Length >= count);
      if (count != 0) {
        object[] res = Algs.SortUnique(objs, (int) count);
        return new NeSetObj((Obj[]) res[0], (uint[]) res[1]);
      }
      else
        return EmptyRelObj.singleton;
    }

    public static Obj CreateMap(List<Obj> keys, List<Obj> vals) {
      Debug.Assert(keys.Count == vals.Count);
      return CreateMap(keys.ToArray(), vals.ToArray());
    }

    public static Obj CreateMap(Obj[] keys, Obj[] vals) {
      return CreateMap(keys, vals, keys.Length);
    }

    public static Obj CreateMap(Obj[] keys, Obj[] vals, long count) {
      Obj binRel = CreateBinRel(keys, vals, count);
      if (!binRel.IsEmptyRel() && !binRel.IsNeMap())
        throw ErrorHandler.SoftFail("Error: map contains duplicate keys");
      return binRel;
    }

    public static Obj CreateRecord(ushort[] labels, Obj[] values) {
      Obj[] labelObjs = new Obj[labels.Length];
      for (int i=0 ; i < labels.Length ; i++)
        labelObjs[i] = SymbObj.Get(labels[i]);
      return CreateMap(labelObjs, values);
    }

    public static Obj CreateBinRel(List<Obj> col1, List<Obj> col2) {
      Debug.Assert(col1.Count == col2.Count);
      return CreateBinRel(col1.ToArray(), col2.ToArray(), col1.Count);
    }

    public static Obj CreateBinRel(Obj[] col1, Obj[] col2) {
      return CreateBinRel(col1, col2, col1.Length);
    }

    public static Obj CreateBinRel(Obj obj1, Obj obj2) {
      return CreateBinRel(new Obj[] {obj1}, new Obj[] {obj2}, 1);
    }

    public static Obj CreateBinRel(Obj[] col1, Obj[] col2, long count) {
      Debug.Assert(count <= col1.Length & count <= col2.Length);
      if (count != 0)
        return NeBinRelObj.Create(col1, col2, (int) count);
      else
        return EmptyRelObj.singleton;
    }

    public static Obj CreateTernRel(List<Obj> col1, List<Obj> col2, List<Obj> col3) {
      Debug.Assert(col1.Count == col2.Count && col1.Count == col3.Count);
      return CreateTernRel(col1.ToArray(), col2.ToArray(), col3.ToArray(), col1.Count);
    }

    public static Obj CreateTernRel(Obj[] col1, Obj[] col2, Obj[] col3) {
      return CreateTernRel(col1, col2, col3, col1.Length);
    }

    public static Obj CreateTernRel(Obj[] col1, Obj[] col2, Obj[] col3, long count) {
      Debug.Assert(count <= col1.Length && count <= col2.Length && count <= col3.Length);
      if (col1.Length != 0) {
        Obj[][] normCols = Algs.SortUnique(col1, col2, col3, (int) count);
        return new NeTernRelObj(normCols[0], normCols[1], normCols[2]);
      }
      else {
        return EmptyRelObj.singleton;
      }
    }

    public static Obj CreateTernRel(Obj obj1, Obj obj2, Obj obj3) {
      Obj[] col1 = new Obj[1];
      Obj[] col2 = new Obj[1];
      Obj[] col3 = new Obj[1];
      col1[0] = obj1;
      col2[0] = obj2;
      col3[0] = obj3;
      return new NeTernRelObj(col1, col2, col3);
    }

    public static Obj CreateTaggedObj(ushort tag, Obj obj) {
      if (obj.IsInt())
        return CreateTaggedIntObj(tag, obj.GetLong());

      if (tag == SymbObj.StringSymbId)
        obj = obj.PackForString();

      return new TaggedObj(tag, obj);
    }

    public static Obj CreateTaggedIntObj(ushort tag, long value) {
      if (TaggedIntObj.Fits(value))
        return new TaggedIntObj(tag, value);
      else
        return new TaggedObj(tag, IntObj.Get(value));
    }

    public static TaggedObj CreateString(char[] chars, int len) {
      if (len == 0)
        return new TaggedObj(SymbObj.StringSymbId, EmptySeqObj.singleton);

      int max = 0;
      for (int i=0 ; i < len ; i++) {
        char ch = chars[i];
        if (ch > max)
          max = ch;
      }

      Obj charArray;

      if (max <= 255) {
        byte[] bytes = new byte[len];
        for (int i=0 ; i < len ; i++)
          bytes[i] = (byte) chars[i];
        charArray = IntArrayObjs.Create(bytes);
      }
      else if (max <= 32767) {
        short[] shorts = new short[len];
        for (int i=0 ; i < len ; i++)
          shorts[i] = (short) chars[i];
        charArray = IntArrayObjs.Create(shorts);
      }
      else {
        int[] ints = new int[len];
        for (int i=0 ; i < len ; i++)
          ints[i] = (int) chars[i];
        charArray = IntArrayObjs.Create(ints);
      }

      return new TaggedObj(SymbObj.StringSymbId, charArray);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static Obj CreateSeq(bool[] vals) {
      if (vals.Length == 0)
        return EmptySeqObj.singleton;

      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = SymbObj.Get(vals[i]);
      return ArrayObjs.Create(objs);
    }

    public static Obj CreateSeq(sbyte[] vals) {
      if (vals.Length != 0)
        return IntArrayObjs.Create(vals);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(byte[] vals) {
      if (vals.Length != 0)
        return IntArrayObjs.Create(vals);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(short[] vals) {
      if (vals.Length != 0)
        return IntArrayObjs.Create(vals);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(int[] vals) {
      if (vals.Length != 0)
        return IntArrayObjs.Create(vals);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(long[] vals) {
      if (vals.Length != 0)
        return IntArrayObjs.Create(vals);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(double[] vals) {
      if (vals.Length != 0)
        return FloatArrayObjs.Create(vals);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(Obj[] objs) {
      int len = objs.Length;
      if (len == 0)
        return EmptySeqObj.singleton;

      if (objs[0].IsInt()) {
        for (int i=1 ; i < len ; i++)
          if (!objs[i].IsInt())
            return ArrayObjs.Create(objs);

        long[] longs = new long[len];
        for (int i=0 ; i < len ; i++)
          longs[i] = objs[i].GetLong();
        return IntArrayObjs.Create(longs);
      }

      if (objs[0].IsFloat()) {
        for (int i=1 ; i < len ; i++)
          if (!objs[i].IsFloat())
            return ArrayObjs.Create(objs);

        double[] doubles = new double[len];
        for (int i=0 ; i < len ; i++)
          doubles[i] = objs[i].GetDouble();
        return FloatArrayObjs.Create(doubles);
      }

      return ArrayObjs.Create(objs);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static Obj CreateSeq(bool[] vals, int len) {
      if (len == 0)
        return EmptySeqObj.singleton;

      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = SymbObj.Get(vals[i]);
      return ArrayObjs.Create(objs);
    }

    public static Obj CreateSeq(sbyte[] vals, int len) {
      if (len != 0)
        return IntArrayObjs.Create(vals, len);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(byte[] vals, int len) {
      if (len != 0)
        return IntArrayObjs.Create(vals, len);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(short[] vals, int len) {
      if (len != 0)
        return IntArrayObjs.Create(vals, len);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(int[] vals, int len) {
      if (len != 0)
        return IntArrayObjs.Create(vals, len);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(long[] vals, int len) {
      if (len != 0)
        return IntArrayObjs.Create(vals, len);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(double[] vals, int len) {
      if (len != 0)
        return FloatArrayObjs.Create(vals, len);
      else
        return EmptySeqObj.singleton;
    }

    public static Obj CreateSeq(Obj[] objs, int len) {
      if (len == 0)
        return EmptySeqObj.singleton;

      for (int i=0 ; i < len ; i++)
        if (!objs[i].IsInt())
          return ArrayObjs.Create(objs, len);

      long[] longs = new long[len];
      for (int i=0 ; i < len ; i++)
        longs[i] = objs[i].GetLong();
      return IntArrayObjs.Create(longs);
    }
  }
}
