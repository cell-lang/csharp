namespace Cell.Runtime {
  public class Canonical {
    public static int Order(Obj obj1, Obj obj2) {
      Obj.TypeCode code1 = obj1.GetTypeCode();
      Obj.TypeCode code2 = obj2.GetTypeCode();

      if (code1 != code2)
        return ((int) code1) < ((int) code2) ? -1 : 1;

      switch (code1) {
        case Obj.TypeCode.SYMBOL:
          return SymbolOrder((SymbObj) obj1, (SymbObj) obj2);

        case Obj.TypeCode.INTEGER:
          return IntegerOrder(obj1, obj2);

        case Obj.TypeCode.FLOAT:
          return FloatOrder(obj1, obj2);

        case Obj.TypeCode.EMPTY_SEQ:
          return 0;

        case Obj.TypeCode.EMPTY_REL:
          return 0;

        case Obj.TypeCode.NE_SEQ:
          return SeqOrder(obj1, obj2);

        case Obj.TypeCode.NE_SET:
          return SetOrder((NeSetObj) obj1, (NeSetObj) obj2);

        case Obj.TypeCode.NE_BIN_REL:
          return BinRelOrder((NeBinRelObj) obj1, (NeBinRelObj) obj2);

        case Obj.TypeCode.NE_TERN_REL:
          return TernRelOrder((NeTernRelObj) obj1, (NeTernRelObj) obj2);

        case Obj.TypeCode.TAGGED_VALUE:
          return TaggedValueOrder(obj1, obj2);
      }

      throw ErrorHandler.InternalFail();
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    static int Order(Obj[] objs1, Obj[] objs2) {
      int len = objs1.Length;
      for (int i=0 ; i < len ; i++) {
        int ord = Order(objs1[i], objs2[i]);
        if (ord != 0)
          return ord;
      }
      return 0;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    static int SymbolOrder(SymbObj obj1, SymbObj obj2) {
      string str1 = obj1.stringRepr;
      string str2 = obj2.stringRepr;
      int len1 = str1.Length;
      int len2 = str2.Length;

      if (len1 != len2)
        return len1 < len2 ? -1 : 1;

      for (int i=0 ; i < len1 ; i++) {
        char ch1 = str1[i];
        char ch2 = str2[i];
        if (ch1 != ch2)
          return ch1 < ch2 ? -1 : 1;
      }

      return 0;
    }

    static int IntegerOrder(Obj obj1, Obj obj2) {
      long value1 = obj1.GetLong();
      long value2 = obj2.GetLong();
      return value1 != value2 ? (value1 < value2 ? -1 : 1) : 0;
    }

    static int FloatOrder(Obj obj1, Obj obj2) {
      double value1 = obj1.GetDouble();
      double value2 = obj2.GetDouble();
      return value1 != value2 ? (value1 < value2 ? -1 : 1) : 0;
    }

    static int SeqOrder(Obj obj1, Obj obj2) {
      int len1 = obj1.GetSize();
      int len2 = obj2.GetSize();

      if (len1 != len2)
        return len1 < len2 ? -1 : 1;

      for (int i=0 ; i < len1 ; i++) {
        int ord = Order(obj1.GetObjAt(i), obj2.GetObjAt(i));
        if (ord != 0)
          return ord;
      }

      return 0;
    }

    static int SetOrder(NeSetObj obj1, NeSetObj obj2) {
      int size1 = obj1.GetSize();
      int size2 = obj2.GetSize();

      if (size1 != size2)
        return size1 < size2 ? -1 : 1;
      else
        return Order(obj1.Elts(), obj2.Elts());
    }

    static int BinRelOrder(NeBinRelObj obj1, NeBinRelObj obj2) {
      int size1 = obj1.GetSize();
      int size2 = obj2.GetSize();

      if (size1 != size2)
        return size1 < size2 ? -1 : 1;

      int ord = Order(obj1.Col1(), obj2.Col1());
      if (ord == 0)
        ord = Order(obj1.Col2(), obj2.Col2());
      return ord;
    }

    static int TernRelOrder(NeTernRelObj obj1, NeTernRelObj obj2) {
      int size1 = obj1.GetSize();
      int size2 = obj2.GetSize();

      if (size1 != size2)
        return size1 < size2 ? -1 : 1;

      int ord = Order(obj1.Col1(), obj2.Col1());
      if (ord != 0)
        return ord;

      ord = Order(obj1.Col2(), obj2.Col2());
      if (ord != 0)
        return ord;

      return Order(obj1.Col3(), obj2.Col3());
    }

    static int TaggedValueOrder(Obj obj1, Obj obj2) {
      int ord = Order(obj1.GetTag(), obj2.GetTag());
      if (ord != 0)
        return ord;
      return Order(obj1.GetInnerObj(), obj2.GetInnerObj());
    }
  }
}