using System;


namespace Cell.Runtime {
  public class Procs {
    public static Obj FileRead_P(Obj fname, object env) {
      try {
        byte[] content = IO.ReadAllBytes(fname.GetString());
        Obj bytesObj = Builder.CreateSeq(content);
        return Builder.CreateTaggedObj(SymbObj.JustSymbId, bytesObj);
      }
      catch (Exception) {
        return SymbObj.Get(SymbObj.NothingSymbId);
      }
    }

    public static Obj FileWrite_P(Obj fname, Obj data, object env) {
      try {
        IO.WriteAllBytes(fname.GetString(), data.GetByteArray());
        return SymbObj.Get(SymbObj.TrueSymbId);
      }
      catch (Exception) {
        return SymbObj.Get(SymbObj.FalseSymbId);
      }
    }

    public static Obj FileAppend_P(Obj fname, Obj data, object env) {
      try {
        IO.AppendAllBytes(fname.GetString(), data.GetByteArray());
        return SymbObj.Get(SymbObj.TrueSymbId);
      }
      catch (Exception) {
        return SymbObj.Get(SymbObj.FalseSymbId);
      }
    }

    public static void Print_P(Obj str, object env) {
      IO.StdOutWrite(str.GetString());
    }

    public static Obj GetChar_P(object env) {
      int ch = IO.StdInRead(-1, -1);
      if (ch != -1)
        return Builder.CreateTaggedObj(SymbObj.JustSymbId, IntObj.Get(ch));
      else
        return SymbObj.Get(SymbObj.NothingSymbId);
    }

    public static Obj Now_P(object env) {
      long msecs = IO.UnixTimeMs();
      return Builder.CreateTaggedIntObj(SymbObj.TimeSymbId, 1000000 * msecs);
    }

    private static long startTick = -1;

    public static Obj Ticks_P(object env) {
      long tick = IO.UnixTimeMs();
      if (startTick == -1)
        startTick = tick;
      return IntObj.Get(tick - startTick);
    }

    public static void Exit_P(Obj code, object env) {
      IO.Exit((int) code.GetLong());
    }
  }
}
