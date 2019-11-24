namespace Cell.Runtime {
  // unary_rel(a) -> tern_rel(a | _, _)

  public class ForeignKeyCheckerUST12 : Sym12TernaryTableUpdater.DeleteChecker {
    UnaryTableUpdater source;
    Sym12TernaryTableUpdater target;

    public ForeignKeyCheckerUST12(UnaryTableUpdater source, Sym12TernaryTableUpdater target) {
      Debug.Assert(source.store == target.store12);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.contains_1_2(inserts[i]))
            throw ForeignKeyViolation(inserts[i]);
      }
      target.CheckDeletes(this);
    }

    public void CheckDelete(int arg1, int arg2, int arg3, Sym12TernaryTableUpdater updater) {
      if (source.Contains(arg1) && !target.contains_1_2(arg1))
        throw ForeignKeyViolation(arg1, arg2, arg3);
      if (source.Contains(arg2) && !target.contains_1_2(arg2))
        throw ForeignKeyViolation(arg2, arg1, arg3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int surr) {
      Obj arg = source.store.SurrToValue(surr);
      return ForeignKeyViolationException.UnarySym12Ternary(source.relvarName, target.relvarName, arg);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int delSurr12, int otherSurr12, int surr3) {
      Obj delArg12 = source.store.SurrToValue(delSurr12);
      Obj otherArg12 = source.store.SurrToValue(otherSurr12);
      Obj arg3 = target.store3.SurrToValue(surr3);
      return ForeignKeyViolationException.UnarySym12Ternary(source.relvarName, target.relvarName, delArg12, otherArg12, arg3);
    }
  }
}
