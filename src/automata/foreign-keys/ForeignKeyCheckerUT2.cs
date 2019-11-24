namespace Cell.Runtime {
  // unary_rel(x) -> ternary_rel(, x, _)

  public class ForeignKeyCheckerUT2 : TernaryTableUpdater.DeleteChecker {
    UnaryTableUpdater source;
    TernaryTableUpdater target;

    public ForeignKeyCheckerUT2(UnaryTableUpdater source, TernaryTableUpdater target) {
      Debug.Assert(source.store == target.store2);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains2(inserts[i]))
            throw ToTernaryForeignKeyViolation2(inserts[i]);
      }

      target.CheckDeletes231(this);
    }

    public void MightHaveBeenDeleted(int arg1, int arg2, int arg3) {
      if (source.Contains(arg2) && !target.Contains2(arg2))
        throw ToTernaryForeingKeyViolation2(arg1, arg2, arg3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ToTernaryForeignKeyViolation2(int surr) {
      Obj arg = source.store.SurrToValue(surr);
      return ForeignKeyViolationException.UnaryTernary(source.relvarName, 2, target.relvarName, arg);
    }

    private ForeignKeyViolationException ToTernaryForeingKeyViolation2(int surr1, int surr2, int surr3) {
      Obj arg1 = target.store1.SurrToValue(surr1);
      Obj arg2 = target.store2.SurrToValue(surr2);
      Obj arg3 = target.store3.SurrToValue(surr3);
      Obj[] tuple = new Obj[] {arg1, arg2, arg3};
      return ForeignKeyViolationException.UnaryTernary(source.relvarName, 2, target.relvarName, tuple);
    }
  }
}
