using System;


namespace Cell.Runtime {
  public class ErrorHandler {
    public static bool insideTransaction = false;


    public static Exception SoftFail() {
      Debug.OnSoftFailure(insideTransaction);
      throw new Exception();
    }

    public static Exception SoftFail(string msg) {
      Debug.Trace(msg);
      return SoftFail();
    }

    public static Exception SoftFail(string msg, string varName, Obj obj) {
      Debug.Trace(msg, varName, obj);
      return SoftFail();
    }

    public static Exception SoftFail(string msg, string var1Name, Obj obj1, string var2Name, Obj obj2) {
      Debug.Trace(msg, var1Name, obj1, var2Name, obj2);
      throw SoftFail();
    }

    public static Exception InvalidIndexSoftFail(long index, int len) {
      string msg = string.Format("Invalid index: {0}, sequence length = {1}", index, len);
      throw SoftFail(msg);
    }

    public static Exception InvalidRangeSoftFail(long start, long end, int len) {
      string msg = string.Format("Invalid range: [{0}, {1}), sequence length = {2}", start, end, len);
      throw SoftFail(msg);
    }

    public static Exception HardFail() {
      Debug.OnHardFailure();
      throw IO.Exit(1);
    }

    public static Exception ImplFail(string msg) {
      Debug.OnImplFailure(msg);
      throw IO.Exit(1);
    }

    public static Exception InternalFail() {
      return InternalFail(null);
    }

    public static Exception InternalFail(Obj obj) {
      Debug.OnInternalFailure(obj);
      throw IO.Exit(1);
    }
  }
}
