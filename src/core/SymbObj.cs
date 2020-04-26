using System;
using System.Collections.Generic;


namespace Cell.Runtime {
  public sealed class SymbObj : Obj {
    internal string stringRepr;
    private uint hashcode;


    private SymbObj(ushort id) {
      data = SymbObjData(id);
      extraData = SymbObjExtraData();
      Debug.Assert(GetSymbId() == id);
      stringRepr = IdxToStr(id);

      // Calculating the hash code
      ulong hcode = 0;
      int len = stringRepr.Length;
      for (int i=0 ; i < len ; i++)
        hcode = 31 * hcode + stringRepr[i];
      hashcode = Hashing.Hashcode64(hcode);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      return hashcode;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.SYMBOL;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.SymbObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static SymbObj Get(ushort id) {
      return symbObjs[id];
    }

    //## THIS COULD BE OPTIMIZED
    public static SymbObj Get(bool b) {
      return Get(b ? TrueSymbId : FalseSymbId);
    }

    //////////////////////////////////////////////////////////////////////////////

    private static string[] defaultSymbols = {
      "false",
      "true",
      "void",
      "string",
      "date",
      "time",
      "nothing",
      "just",
      "success",
      "failure"
    };

    public static ushort FalseSymbId   = 0;
    public static ushort TrueSymbId    = 1;
    public static ushort VoidSymbId    = 2;
    public static ushort StringSymbId  = 3;
    public static ushort DateSymbId    = 4;
    public static ushort TimeSymbId    = 5;
    public static ushort NothingSymbId = 6;
    public static ushort JustSymbId    = 7;
    public static ushort SuccessSymbId = 8;
    public static ushort FailureSymbId = 9;

    public static ushort InvalidSymbId = 0xFFFF;

    private static string[] embeddedSymbols;

    private static List<string> symbTable = new List<string>();
    private static Dictionary<string, ushort> symbMap = new Dictionary<string, ushort>();
    private static List<SymbObj> symbObjs = new List<SymbObj>();

    static SymbObj() {
      for (ushort i=0 ; i < defaultSymbols.Length ; i++) {
        string str = defaultSymbols[i];
        symbTable.Add(str);
        symbMap[str] = i;
        symbObjs.Add(new SymbObj(i));
      }

      embeddedSymbols = Cell.Generated.Static.embeddedSymbols;

      for (int i=0 ; i < embeddedSymbols.Length ; i++) {
        int idx = StrToIdx(embeddedSymbols[i]);
        Debug.Assert(idx == i);
      }
    }

    public static ushort BytesToIdx(byte[] bytes, int len) {
      return StrToIdx(Miscellanea.AsciiString(bytes, len));
    }

    public static ushort BytesToIdx(byte[] bytes) {
      return BytesToIdx(bytes, bytes.Length);
    }

    public static ushort StrToIdx(string str) {
      if (symbMap.ContainsKey(str))
        return symbMap[str];

      int count = symbTable.Count;
      if (count < 65535) {
        ushort idx = (ushort) count;
        symbTable.Add(str);
        symbMap[str] = idx;
        symbObjs.Add(new SymbObj(idx));
        return idx;
      }

      throw ErrorHandler.ImplFail("Exceeded the maximum number of symbols (65535)");
    }

    public static string IdxToStr(int idx) {
      return symbTable[idx];
    }

    public static int CompSymbs(int id1, int id2) {
      if (id1 == id2)
        return 0;
      int len = embeddedSymbols.Length;
      if (id1 < len | id2 < len)
        return id1 < id2 ? 1 : -1;
      string str1 = symbTable[id1];
      string str2 = symbTable[id2];
      return Miscellanea.Order(str1, str2);
    }

    public static int CompBools(bool b1, bool b2) {
      return b1 == b2 ? 0 : (b1 ? -1 : 1);
    }
  }
}
