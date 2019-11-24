using Exception       = System.Exception;
using DateTime        = System.DateTime;
using TimeSpan        = System.TimeSpan;
using DateTimeKind    = System.DateTimeKind;
using StringReader    = System.IO.StringReader;


namespace Cell.Runtime {
  public class Conversions {
    public static Obj ConvertText(string text) {
      TokenStream tokens = new Tokenizer(new CharStream(new StringReader(text)));
      Parser parser = new Cell.Generated.Static.Parser(tokens);
      Obj obj = parser.ParseObj();
      parser.CheckEof();
      return obj;
    }

    public static string ExportAsText(Obj obj) {
      return obj.ToString();
    }

    public static Obj StringToObj(string str) {
      //## THIS ONE IS REAL BAD TOO. IT SHOULD USE THE MINIMUM SIZE ARRAY POSSIBLE!
      int[] cps = Miscellanea.CodePoints(str);
      return Builder.CreateTaggedObj(SymbObj.StringSymbId, Builder.CreateSeq(cps));
    }

    ////////////////////////////////////////////////////////////////////////////

    private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Obj DateToObj(DateTime date) {
      TimeSpan delta = date - epoch;
      return Builder.CreateTaggedIntObj(SymbObj.DateSymbId, delta.Days);
    }

    public static DateTime ObjToDate(Obj date) {
      long epochDay = date.GetInnerLong();
      return epoch.AddDays(epochDay);
    }

    public static Obj DateTimeToObj(DateTime time) {
      TimeSpan delta = time - epoch;
      long ticks = delta.Ticks;
      long epochSecond = ticks / 10000000;
      long nanosecs = 100 * (ticks % 10000000);

      if (epochSecond >= -9223372036L) {
        if (epochSecond < 9223372036L | (epochSecond == 9223372036L & nanosecs <= 854775807)) {
          long epochNanoSecs = 1000000000 * epochSecond + nanosecs;
          return Builder.CreateTaggedIntObj(SymbObj.TimeSymbId, epochNanoSecs);
        }
      }
      else if (epochSecond == -9223372037L & nanosecs >= 145224192) {
        long epochNanoSecs = -9223372036000000000L - (1000000000 - nanosecs);
        return Builder.CreateTaggedIntObj(SymbObj.TimeSymbId, epochNanoSecs);
      }

      throw new Exception("DateTime is outside the supported range: " + time.ToString());
    }

    public static DateTime ObjToDateTime(Obj time) {
      long epochNanoSecs = time.GetInnerLong();
      return epoch.AddTicks(epochNanoSecs / 100);
    }

    ////////////////////////////////////////////////////////////////////////////

    public static bool[] ToBoolArray(Obj obj) {
      if (obj.IsSeq()) {
        bool[] array = obj.GetBoolArray();
        return Array.Take(array, array.Length);
      }

      Obj[] elts = obj.GetObjArray();
      int len = elts.Length;
      bool[] bools = new bool[len];
      for (int i=0 ; i < len ; i++)
        bools[i] = elts[i].GetBool();
      return bools;
    }

    public static long[] ToLongArray(Obj obj) {
      if (obj.IsSeq()) {
        long[] array = obj.GetLongArray();
        return Array.Take(array, array.Length);
      }

      Obj[] elts = obj.GetObjArray();
      int len = elts.Length;
      long[] longs = new long[len];
      for (int i=0 ; i < len ; i++)
        longs[i] = elts[i].GetLong();
      return longs;
    }

    public static double[] ToDoubleArray(Obj obj) {
      if (obj.IsSeq()) {
        double[] array = obj.GetDoubleArray();
        return Array.Take(array, array.Length);
      }

      Obj[] elts = obj.GetObjArray();
      int len = elts.Length;
      double[] doubles = new double[len];
      for (int i=0 ; i < len ; i++)
        doubles[i] = elts[i].GetDouble();
      return doubles;
    }

    public static string[] ToSymbolArray(Obj obj) {
      Obj[] elts = obj.GetObjArray();
      int len = elts.Length;
      string[] symbs = new string[len];
      for (int i=0 ; i < len ; i++)
        symbs[i] = elts[i].ToString();
      return symbs;
    }

    public static string[] ToStringArray(Obj obj) {
      Obj[] elts = obj.GetObjArray();
      int len = elts.Length;
      string[] strs = new string[len];
      for (int i=0 ; i < len ; i++)
        strs[i] = elts[i].GetString();
      return strs;
    }

    public static string[] ToTextArray(Obj obj) {
      Obj[] elts = obj.GetObjArray();
      int len = elts.Length;
      string[] strs = new string[len];
      for (int i=0 ; i < len ; i++)
        strs[i] = ExportAsText(elts[i]);
      return strs;
    }
  }
}
