namespace Cell.Runtime {
  // unary_rel(c) -> tern_rel(_ | _, c);

  public class ForeignKeyCheckerUST3 : Sym12TernaryTableUpdater.DeleteChecker {
    UnaryTableUpdater source;
    Sym12TernaryTableUpdater target;

    public ForeignKeyCheckerUST3(UnaryTableUpdater source, Sym12TernaryTableUpdater target) {
      Debug.Assert(source.store == target.store3);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains3(inserts[i]))
            throw ForeignKeyViolation(inserts[i]);
      }

      target.CheckDeletes(this);
    }

    public void CheckDelete(int arg1, int arg2, int arg3, Sym12TernaryTableUpdater updater) {
      if (source.Contains(arg3) && !target.Contains3(arg3))
        throw ForeignKeyViolation(arg1, arg2, arg3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int surr) {
      Obj arg = source.store.SurrToValue(surr);
      return ForeignKeyViolationException.UnaryTernary(source.relvarName, 3, target.relvarName, arg);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int surr1, int surr2, int surr3) {
      Obj arg1 = target.store12.SurrToValue(surr1);
      Obj arg2 = target.store12.SurrToValue(surr2);
      Obj arg3 = source.store.SurrToValue(surr3);
      Obj[] tuple = new Obj[] {arg1, arg2, arg3};
      return ForeignKeyViolationException.UnaryTernary(source.relvarName, 3, target.relvarName, tuple);
    }
  }
}
