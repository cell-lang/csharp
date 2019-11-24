namespace Cell.Runtime {
  // tern_rel(_, _, c) -> unary_rel(c);

  public class ForeignKeyCheckerTU3 : UnaryTableUpdater.DeleteChecker {
    TernaryTableUpdater source;
    UnaryTableUpdater target;

    public ForeignKeyCheckerTU3(TernaryTableUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store3 == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      // Checking that every new entry satisfies the foreign key
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(inserts[3*i+2]))
            throw ForeignKeyViolation(inserts[3*i], inserts[3*i+1], inserts[3*i+2]);
      }

      // Checking that no entries were invalidated by a deletion on the target table
      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int surr3) {
      if (source.Contains3(surr3))
        throw ForeignKeyViolation(surr3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int arg1Surr, int arg2Surr, int arg3Surr) {
      Obj[] tuple = new Obj[] {
        source.store1.SurrToValue(arg1Surr),
        source.store2.SurrToValue(arg2Surr),
        source.store3.SurrToValue(arg3Surr)
      };
      return ForeignKeyViolationException.TernaryUnary(source.relvarName, 3, target.relvarName, tuple);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int delSurr) {
      TernaryTable.Iter3 it = source.table.GetIter3(delSurr);
      Obj arg1 = source.store1.SurrToValue(it.Get1());
      Obj arg2 = source.store2.SurrToValue(it.Get2());
      Obj arg3 = source.store3.SurrToValue(delSurr);
      Obj[] tuple = new Obj[] {arg1, arg2, arg3};
      return ForeignKeyViolationException.TernaryUnary(source.relvarName, 3, target.relvarName, tuple, arg3);
    }
  }
}
