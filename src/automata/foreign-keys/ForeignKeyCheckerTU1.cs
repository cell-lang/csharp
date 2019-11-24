namespace Cell.Runtime {
  // tern_rel(a, _, _) -> unary_rel(a);

  public class ForeignKeyCheckerTU1 : UnaryTableUpdater.DeleteChecker {
    TernaryTableUpdater source;
    UnaryTableUpdater target;

    public ForeignKeyCheckerTU1(TernaryTableUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store1 == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      // Checking that every new entry satisfies the foreign key
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(inserts[3*i]))
            throw ForeignKeyViolation(inserts[3*i], inserts[3*i+1], inserts[3*i+2]);
      }

      // Checking that no entries were invalidated by a deletion on the target table
      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int surr1) {
      if (source.Contains1(surr1))
        throw ForeignKeyViolation(surr1);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int arg1Surr, int arg2Surr, int arg3Surr) {
      Obj[] tuple = new Obj[] {
        source.store1.SurrToValue(arg1Surr),
        source.store2.SurrToValue(arg2Surr),
        source.store3.SurrToValue(arg3Surr)
      };
      return ForeignKeyViolationException.TernaryUnary(source.relvarName, 1, target.relvarName, tuple);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int delSurr) {
      TernaryTable.Iter1 it = source.table.GetIter1(delSurr);
      Obj arg1 = source.store1.SurrToValue(delSurr);
      Obj arg2 = source.store2.SurrToValue(it.Get1());
      Obj arg3 = source.store3.SurrToValue(it.Get2());
      Obj[] tuple = new Obj[] {arg1, arg2, arg3};
      return ForeignKeyViolationException.TernaryUnary(source.relvarName, 1, target.relvarName, tuple, arg1);
    }
  }
}
