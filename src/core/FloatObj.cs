using System;


namespace Cell.Runtime {
  public sealed class FloatObj : Obj {
    public FloatObj(double value) {
      data = FloatObjData(value);
      extraData = FloatObjExtraData();
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      throw ErrorHandler.InternalFail(this);
    }

    public override uint Hashcode() {
      return Hashcode(GetDouble());
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.FLOAT;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.FloatObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static int Compare(double x1, double x2) {
      return IntObj.Compare((long) FloatObjData(x1), (long) FloatObjData(x2));
    }

    public static uint Hashcode(double x) {
      return Hashing.Hashcode64(Miscellanea.DoubleBitsToULongBits(x));
    }

    ////////////////////////////////////////////////////////////////////////////

    public const double NaN = Double.NaN;

    public static bool IsNaN(double x) {
      return Double.IsNaN(x);
    }

    public static double Pow(double x, double y) {
      return Math.Pow(x, y);
    }

    public static double Sqrt(double x) {
      return Math.Sqrt(x);
    }

    public static long Bits(double x) {
      return Miscellanea.DoubleBitsToLongBits(x);
    }

    public static long Round(double x) {
      return (long) x;
    }
  }
}