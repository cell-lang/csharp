namespace Cell.Runtime {
  // unary_rel(a) -> sym_binary_rel(a | _)

  public class ForeignKeyCheckerUSB : SymBinaryTableUpdater.DeleteChecker {
    UnaryTableUpdater source;
    SymBinaryTableUpdater target;

    public ForeignKeyCheckerUSB(UnaryTableUpdater source, SymBinaryTableUpdater target) {
      Debug.Assert(source.store == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(inserts[i]))
            throw ForeignKeyViolation(inserts[i]);
      }

      target.CheckDeletes(this);
    }

    public void CheckDelete(int arg1, int arg2, SymBinaryTableUpdater target) {
      if (source.Contains(arg1) && !target.Contains(arg1))
        throw ForeignKeyViolation(arg1, arg2);
      if (source.Contains(arg2) && !target.Contains(arg2))
        throw ForeignKeyViolation(arg2, arg1);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int surr) {
      Obj arg = source.store.SurrToValue(surr);
      return ForeignKeyViolationException.UnarySymBinary(source.relvarName, target.relvarName, arg);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int surr, int otherSurr) {
      Obj arg = source.store.SurrToValue(surr);
      Obj otherArg = source.store.SurrToValue(otherSurr);
      return ForeignKeyViolationException.UnarySymBinary(source.relvarName, target.relvarName, arg, otherArg);
    }
  }
}
