using Exception = System.Exception;


namespace Cell.Runtime {
  public class ForeignKeyViolationException : Exception {
    private interface ForeignKeyType {
      string OriginArgs();
      string TargetArgs();
    }

    private sealed class NoArgsForeignKeyType : ForeignKeyType {
      string originArgs, targetArgs;

      public NoArgsForeignKeyType(string originArgs, string targetArgs) {
        this.originArgs = originArgs;
        this.targetArgs = targetArgs;
      }

      public string OriginArgs() {
        return originArgs;
      }

      public string TargetArgs() {
        return targetArgs;
      }
    }

    private sealed class BinaryUnaryForeignKeyType : ForeignKeyType {
      int column;

      public BinaryUnaryForeignKeyType(int column) {
        Debug.Assert(column == 1 | column == 2);
        this.column = column;
      }

      public string OriginArgs() {
        return column == 1 ? "(a, _)" : "(_, a)";
      }

      public string TargetArgs() {
        return "(a)";
      }
    }

    private sealed class TernaryUnaryForeignKeyType : ForeignKeyType {
      int column;

      public TernaryUnaryForeignKeyType(int column) {
        Debug.Assert(column == 1 | column == 2 | column == 3);
        this.column = column;
      }

      public string OriginArgs() {
        return column == 1 ? "(a, _, _)" : column == 2 ? "(_, a, _)" : "(_, _, a)";
      }

      public string TargetArgs() {
        return "(a)";
      }
    }

    private sealed class UnaryBinaryForeignKeyType : ForeignKeyType {
      int column;

      public UnaryBinaryForeignKeyType(int column) {
        Debug.Assert(column == 1 | column == 2);
        this.column = column;
      }

      public string OriginArgs() {
        return "(a)";
      }

      public string TargetArgs() {
        return column == 1 ? "(a, _)" : "(_, a)";
      }
    }

    private sealed class UnaryTernaryForeignKeyType : ForeignKeyType {
      int column;

      public UnaryTernaryForeignKeyType(int column) {
        Debug.Assert(column == 1 | column == 2 | column == 3);
        this.column = column;
      }

      public string OriginArgs() {
        return "(a)";
      }

      public string TargetArgs() {
        return column == 1 ? "(a, _, _)" : column == 2 ? "(_, a, _)" : "(_, _, a)";
      }
    }

    private static readonly ForeignKeyType UNARY_UNARY = new NoArgsForeignKeyType("(a)", "(a)");
    private static readonly ForeignKeyType BINARY_TERNARY = new NoArgsForeignKeyType("(a, b)", "(a, b, _)");
    private static readonly ForeignKeyType TERNARY_BINARY = new NoArgsForeignKeyType("(a, b, _)", "(a, b)");
    private static readonly ForeignKeyType UNARY_SYM_BINARY = new NoArgsForeignKeyType("(a)", "(a, _)");
    private static readonly ForeignKeyType UNARY_SYM_TERNARY = new NoArgsForeignKeyType("(a)", "(a, _, _)");
    private static readonly ForeignKeyType SYM_BINARY_UNARY = new NoArgsForeignKeyType("(a, _)", "(a)");
    private static readonly ForeignKeyType SYM_TERNARY_UNARY = new NoArgsForeignKeyType("(a, _, _)", "(a)");
    private static readonly ForeignKeyType SYM_BINARY_SYM_TERNARY = new NoArgsForeignKeyType("(a, b)", "(a, b, _)");
    private static readonly ForeignKeyType SYM_TERNARY_SYM_BINARY = new NoArgsForeignKeyType("(a, b, _)", "(a, b)");


    ForeignKeyType type;
    string fromRelvar, toRelvar;
    Obj[] fromTuple, toTuple;

    private ForeignKeyViolationException(ForeignKeyType type, string fromRelvar, string toRelvar, Obj[] fromTuple, Obj[] toTuple) {
      this.type = type;
      this.fromRelvar = fromRelvar;
      this.toRelvar = toRelvar;
      this.fromTuple = fromTuple;
      this.toTuple = toTuple;
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException UnaryUnary(string fromRelvar, string toRelvar, Obj value) {
      Obj[] tuple = new Obj[] {value};
      return new ForeignKeyViolationException(UNARY_UNARY, fromRelvar, toRelvar, tuple, tuple);
    }

    public static ForeignKeyViolationException BinaryUnary(string fromRelvar, int column, string toRelvar, Obj[] fromTuple) {
      return BinaryUnary(fromRelvar, column, toRelvar, fromTuple, null);
    }

    public static ForeignKeyViolationException BinaryUnary(string fromRelvar, int column, string toRelvar, Obj[] fromTuple, Obj toArg) {
      ForeignKeyType type = new BinaryUnaryForeignKeyType(column);
      Obj[] toTuple = toArg != null ? new Obj[] {toArg} : null;
      return new ForeignKeyViolationException(type, fromRelvar, toRelvar, fromTuple, toTuple);
    }

    public static ForeignKeyViolationException SymBinaryUnary(string fromRelvar, string toRelvar, Obj[] fromTuple) {
      return SymBinaryUnary(fromRelvar, toRelvar, fromTuple, null);
    }

    public static ForeignKeyViolationException SymBinaryUnary(string fromRelvar, string toRelvar, Obj[] fromTuple, Obj toArg) {
      Obj[] toTuple = toArg != null ? new Obj[] {toArg} : null;
      return new ForeignKeyViolationException(SYM_BINARY_UNARY, fromRelvar, toRelvar, fromTuple, toTuple);
    }

    public static ForeignKeyViolationException SymBinarySymTernary(string fromRelvar, string toRelvar, Obj arg1, Obj arg2) {
      Obj[] fromTuple = new Obj[] {arg1, arg2};
      return new ForeignKeyViolationException(SYM_BINARY_SYM_TERNARY, fromRelvar, toRelvar, fromTuple, null);
    }

    public static ForeignKeyViolationException SymBinarySymTernary(string fromRelvar, string toRelvar, Obj arg1, Obj arg2, Obj arg3) {
      Obj[] fromTuple = new Obj[] {arg1, arg2};
      Obj[] toTuple = new Obj[] {arg1, arg2, arg3};
      return new ForeignKeyViolationException(SYM_BINARY_SYM_TERNARY, fromRelvar, toRelvar, fromTuple, toTuple);
    }

    public static ForeignKeyViolationException TernaryUnary(string fromRelvar, int column, string toRelvar, Obj[] fromTuple) {
      return TernaryUnary(fromRelvar, column, toRelvar, fromTuple, null);
    }

    public static ForeignKeyViolationException TernaryUnary(string fromRelvar, int column, string toRelvar, Obj[] fromTuple, Obj toArg) {
      ForeignKeyType type = new TernaryUnaryForeignKeyType(column);
      Obj[] toTuple = toArg != null ? new Obj[] {toArg} : null;
      return new ForeignKeyViolationException(type, fromRelvar, toRelvar, fromTuple, toTuple);
    }

    public static ForeignKeyViolationException SymTernary12Unary(string fromRelvar, string toRelvar, Obj[] fromTuple) {
      return new ForeignKeyViolationException(SYM_TERNARY_UNARY, fromRelvar, toRelvar, fromTuple, null);
    }

    public static ForeignKeyViolationException SymTernary12Unary(string fromRelvar, string toRelvar, Obj[] fromTuple, Obj toArg) {
      return new ForeignKeyViolationException(SYM_TERNARY_UNARY, fromRelvar, toRelvar, fromTuple, new Obj[] {toArg});
    }

    public static ForeignKeyViolationException SymTernary3Unary(string fromRelvar, string toRelvar, Obj[] fromTuple) {
      return TernaryUnary(fromRelvar, 3, toRelvar, fromTuple);
    }

    public static ForeignKeyViolationException SymTernary3Unary(string fromRelvar, string toRelvar, Obj[] fromTuple, Obj toArg) {
      return TernaryUnary(fromRelvar, 3, toRelvar, fromTuple, toArg);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException UnaryBinary(string fromRelvar, int column, string toRelvar, Obj fromArg) {
      ForeignKeyType type = new UnaryBinaryForeignKeyType(column);
      return new ForeignKeyViolationException(type, fromRelvar, toRelvar, new Obj[] {fromArg}, null);
    }

    public static ForeignKeyViolationException UnaryBinary(string fromRelvar, int column, string toRelvar, Obj[] toTuple) {
      ForeignKeyType type = new UnaryBinaryForeignKeyType(column);
      return new ForeignKeyViolationException(type, fromRelvar, toRelvar, new Obj[] {toTuple[column-1]}, toTuple);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException UnaryTernary(string fromRelvar, int column, string toRelvar, Obj fromArg) {
      ForeignKeyType type = new UnaryTernaryForeignKeyType(column);
      return new ForeignKeyViolationException(type, fromRelvar, toRelvar, new Obj[] {fromArg}, null);
    }

    public static ForeignKeyViolationException UnaryTernary(string fromRelvar, int column, string toRelvar, Obj[] toTuple) {
      ForeignKeyType type = new UnaryTernaryForeignKeyType(column);
      return new ForeignKeyViolationException(type, fromRelvar, toRelvar, new Obj[] {toTuple[column-1]}, toTuple);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException UnarySymBinary(string fromRelvar, string toRelvar, Obj arg) {
      return new ForeignKeyViolationException(UNARY_SYM_BINARY, fromRelvar, toRelvar, new Obj[] {arg}, null);
    }

    public static ForeignKeyViolationException UnarySymBinary(string fromRelvar, string toRelvar, Obj arg, Obj otherArg) {
      return new ForeignKeyViolationException(UNARY_SYM_BINARY, fromRelvar, toRelvar, new Obj[] {arg}, new Obj[] {arg, otherArg});
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException UnarySym12Ternary(string fromRelvar, string toRelvar, Obj arg) {
      return new ForeignKeyViolationException(UNARY_SYM_TERNARY, fromRelvar, toRelvar, new Obj[] {arg}, null);
    }

    public static ForeignKeyViolationException UnarySym12Ternary(string fromRelvar, string toRelvar, Obj delArg12, Obj otherArg12, Obj arg3) {
      return new ForeignKeyViolationException(UNARY_SYM_TERNARY, fromRelvar, toRelvar, new Obj[] {delArg12}, new Obj[] {delArg12, otherArg12, arg3});
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException SymTernarySymBinary(string fromRelvar, string toRelvar, Obj arg1, Obj arg2, Obj arg3) {
      return new ForeignKeyViolationException(SYM_TERNARY_SYM_BINARY, fromRelvar, toRelvar, new Obj[] {arg1, arg2}, new Obj[] {arg1, arg2, arg3});
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException BinaryTernary(string fromRelvar, string toRelvar, Obj arg1, Obj arg2) {
      return new ForeignKeyViolationException(BINARY_TERNARY, fromRelvar, toRelvar, new Obj[] {arg1, arg2}, null);
    }

    public static ForeignKeyViolationException BinaryTernary(string fromRelvar, string toRelvar, Obj arg1, Obj arg2, Obj arg3) {
      return new ForeignKeyViolationException(BINARY_TERNARY, fromRelvar, toRelvar, new Obj[] {arg1, arg2}, new Obj[] {arg1, arg2, arg3});
    }

    //////////////////////////////////////////////////////////////////////////////

    public static ForeignKeyViolationException TernaryBinary(string fromRelvar, string toRelvar, Obj[] fromTuple) {
      return new ForeignKeyViolationException(BINARY_TERNARY, fromRelvar, toRelvar, fromTuple, null);
    }

    public static ForeignKeyViolationException TernaryBinary(string fromRelvar, string toRelvar, Obj[] fromTuple, Obj[] toTuple) {
      return new ForeignKeyViolationException(BINARY_TERNARY, fromRelvar, toRelvar, fromTuple, toTuple);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override string ToString() {
      DataWriter writer = IO.StringDataWriter();
      writer.Write("Foreign key violation: " + fromRelvar + type.OriginArgs() + " -> " + toRelvar + type.TargetArgs() + "\n");
      if (toTuple == null) {
        // The violation was caused by an insertion
        writer.Write("The failure was caused by the attempted insertion of:\n  " + fromRelvar + "(");
        for (int i=0 ; i < fromTuple.Length ; i++) {
          if (i > 0)
            writer.Write(", ");
          ObjPrinter.Print(fromTuple[i], writer);
        }
        writer.Write(")\n");
      }
      else {
        // The violation was caused by a deletion in the target table
        writer.Write("The failure was caused by the attempted deletion of:\n  " + toRelvar + "(");
        for (int i=0 ; i < toTuple.Length ; i++) {
          if (i > 0)
            writer.Write(", ");
          ObjPrinter.Print(toTuple[i], writer);
        }
        writer.Write(")\n");
        writer.Write("which was prevented by the presence of:\n  " + fromRelvar + "(");
        for (int i=0 ; i < fromTuple.Length ; i++) {
          if (i > 0)
            writer.Write(", ");
          ObjPrinter.Print(fromTuple[i], writer);
        }
        writer.Write(")\n");
      }
      return writer.Output();
    }
  }
}
