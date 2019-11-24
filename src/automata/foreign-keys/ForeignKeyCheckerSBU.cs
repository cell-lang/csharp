namespace Cell.Runtime {
  // bin_rel(a | b) -> unary_rel(a), unary_rel(b);

  public class ForeignKeyCheckerSBU : UnaryTableUpdater.DeleteChecker {
    SymBinaryTableUpdater source;
    UnaryTableUpdater target;

    public ForeignKeyCheckerSBU(SymBinaryTableUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      // Checking that every new entry satisfies the foreign key
      int count = source.insertCount;
      if (count == 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < 2 * count ; i++)
          if (!target.Contains(inserts[i])) {
            int surr1 = i % 2 == 0 ? inserts[i] : inserts[i-1];
            int surr2 = i % 2 == 0 ? inserts[i+1] : inserts[i];
            throw ForeignKeyViolation(surr1, surr2);
          }
      }

      // Checking that no entries were invalidated by a deletion on the target table
      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int surr) {
      if (source.Contains(surr))
        throw ForeignKeyViolation(surr);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int arg1Surr, int arg2Surr) {
      Obj[] tuple = new Obj[] {source.store.SurrToValue(arg1Surr), source.store.SurrToValue(arg2Surr)};
      return ForeignKeyViolationException.SymBinaryUnary(source.relvarName, target.relvarName, tuple);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int delSurr) {
      int otherSurr = source.table.Restrict(delSurr)[0];
      Obj arg1 = source.store.SurrToValue(delSurr);
      Obj[] tuple1 = new Obj[] {arg1, source.store.SurrToValue(otherSurr)};
      return ForeignKeyViolationException.SymBinaryUnary(source.relvarName, target.relvarName, tuple1, arg1);
    }
  }
}
